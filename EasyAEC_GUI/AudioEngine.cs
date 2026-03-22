using System.Collections;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace EasyAEC_GUI;

/// <summary>
/// WASAPI 采集（麦克风 + 回路参考）与播放；统一为 48kHz / Mono / IEEE Float；按 480 采样（10ms）调用 AEC DLL。
/// </summary>
public sealed class AudioEngine : IDisposable
{
    public const int TargetSampleRate = 48000;
    public const int FrameSamples = 480; // 10ms @ 48kHz mono float

    private readonly object _sync = new();
    private readonly Queue<float> _micQueue = new();
    private readonly Queue<float> _refHold = new();
    private readonly Queue<float> _refDelayed = new();

    private readonly float[] _near = new float[FrameSamples];
    private readonly float[] _far = new float[FrameSamples];
    private readonly float[] _outClean = new float[FrameSamples];
    private readonly byte[] _outBytes = new byte[FrameSamples * sizeof(float)];
    private readonly float[] _mixScratch;

    private int _delaySamples;
    private WasapiCapture? _micCapture;
    private WasapiLoopbackCapture? _loopCapture;
    private WasapiOut? _wasapiOut;
    private SynchronizedFloatPlayback? _playback;
    private MMDevice? _captureDevice;
    private MMDevice? _loopDevice;
    private MMDevice? _playbackDevice;
    private WaveFormat? _micFormat;
    private WaveFormat? _loopFormat;

    private bool _running;
    private bool _disposed;
    private DateTime _lastLevelsUtc = DateTime.MinValue;

    /// <summary>近端 / 参考 / 输出 峰值 0–100（节流后从音频线程触发，由 UI 侧 Invoke）。</summary>
    public event EventHandler<AudioLevelsEventArgs>? LevelsUpdated;

    public AudioEngine()
    {
        _mixScratch = new float[FrameSamples * 32];
    }

    public bool IsRunning
    {
        get
        {
            lock (_sync) return _running;
        }
    }

    /// <summary>
    /// 启动音频图。调用前请确保已成功 <see cref="AecWrapper.AEC_Init"/>。
    /// </summary>
    public void Start(string captureDeviceId, string loopbackRenderDeviceId, string playbackDeviceId, int delayCompensationMs)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(captureDeviceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(loopbackRenderDeviceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(playbackDeviceId);

        lock (_sync)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(AudioEngine));
            if (_running) throw new InvalidOperationException("AudioEngine 已在运行。");

            _delaySamples = Math.Clamp(delayCompensationMs, 0, 500) * (TargetSampleRate / 1000);

            using var enumerator = new MMDeviceEnumerator();
            _captureDevice = enumerator.GetDevice(captureDeviceId);
            _loopDevice = enumerator.GetDevice(loopbackRenderDeviceId);
            _playbackDevice = enumerator.GetDevice(playbackDeviceId);

            _micQueue.Clear();
            _refHold.Clear();
            _refDelayed.Clear();

            _micCapture = new WasapiCapture(_captureDevice);
            _micFormat = _micCapture.WaveFormat;
            _micCapture.DataAvailable += OnMicDataAvailable;
            _micCapture.RecordingStopped += OnRecordingStopped;

            _loopCapture = new WasapiLoopbackCapture(_loopDevice);
            _loopFormat = _loopCapture.WaveFormat;
            _loopCapture.DataAvailable += OnLoopDataAvailable;
            _loopCapture.RecordingStopped += OnRecordingStopped;

            var playFormat = WaveFormat.CreateIeeeFloatWaveFormat(TargetSampleRate, 1);
            var buffer = new BufferedWaveProvider(playFormat)
            {
                BufferLength = FrameSamples * sizeof(float) * 400,
                DiscardOnBufferOverflow = true
            };
            _playback = new SynchronizedFloatPlayback(buffer, _sync);
            _wasapiOut = new WasapiOut(_playbackDevice, AudioClientShareMode.Shared, true, 100);
            _wasapiOut.Init(_playback);
            _wasapiOut.Play();

            _loopCapture.StartRecording();
            _micCapture.StartRecording();

            _running = true;
        }
    }

    public void Stop()
    {
        lock (_sync)
        {
            if (!_running)
                return;

            _running = false;

            try
            {
                _micCapture?.StopRecording();
            }
            catch
            {
                /* ignore */
            }

            try
            {
                _loopCapture?.StopRecording();
            }
            catch
            {
                /* ignore */
            }

            try
            {
                _wasapiOut?.Stop();
            }
            catch
            {
                /* ignore */
            }
        }
    }

    public void Dispose()
    {
        lock (_sync)
        {
            if (_disposed)
                return;
            _disposed = true;
            _running = false;

            SafeDispose(_micCapture);
            _micCapture = null;
            SafeDispose(_loopCapture);
            _loopCapture = null;
            SafeDispose(_wasapiOut);
            _wasapiOut = null;
            _playback = null;

            _captureDevice?.Dispose();
            _captureDevice = null;
            _loopDevice?.Dispose();
            _loopDevice = null;
            _playbackDevice?.Dispose();
            _playbackDevice = null;
        }

        GC.SuppressFinalize(this);
    }

    private void OnRecordingStopped(object? sender, StoppedEventArgs e)
    {
        if (e.Exception != null)
            LogWriter.LogError("AudioEngine", $"采集停止异常：{e.Exception.Message}");
    }

    private void OnMicDataAvailable(object? sender, WaveInEventArgs e)
    {
        if (e.BytesRecorded <= 0 || _micFormat is null)
            return;

        lock (_sync)
        {
            if (!_running)
                return;
            AppendConvertedSamples(e.Buffer, e.BytesRecorded, _micFormat, _micQueue);
            TrimIfNeeded(_micQueue);
            PumpFramesLocked();
        }
    }

    private void OnLoopDataAvailable(object? sender, WaveInEventArgs e)
    {
        if (e.BytesRecorded <= 0 || _loopFormat is null)
            return;

        lock (_sync)
        {
            if (!_running)
                return;
            var chunk = ConvertToFloatMono48k(e.Buffer, e.BytesRecorded, _loopFormat);
            foreach (var s in chunk)
            {
                _refHold.Enqueue(s);
                while (_refHold.Count > _delaySamples)
                    _refDelayed.Enqueue(_refHold.Dequeue());
            }

            TrimIfNeeded(_refDelayed);
            PumpFramesLocked();
        }
    }

    private void PumpFramesLocked()
    {
        while (_micQueue.Count >= FrameSamples && _refDelayed.Count >= FrameSamples)
        {
            for (var i = 0; i < FrameSamples; i++)
                _near[i] = _micQueue.Dequeue();
            for (var i = 0; i < FrameSamples; i++)
                _far[i] = _refDelayed.Dequeue();

            AecWrapper.AEC_ProcessFrame(_near, _far, _outClean, FrameSamples);

            var nPeak = PeakTo100(_near, FrameSamples);
            var fPeak = PeakTo100(_far, FrameSamples);
            var oPeak = PeakTo100(_outClean, FrameSamples);
            MaybeRaiseLevels(nPeak, fPeak, oPeak);

            Buffer.BlockCopy(_outClean, 0, _outBytes, 0, _outBytes.Length);
            _playback?.Write(_outBytes, _outBytes.Length);
        }
    }

    private void MaybeRaiseLevels(int near, int far, int output)
    {
        var now = DateTime.UtcNow;
        if ((now - _lastLevelsUtc).TotalMilliseconds < 33)
            return;
        _lastLevelsUtc = now;
        LevelsUpdated?.Invoke(this, new AudioLevelsEventArgs(near, far, output));
    }

    private void AppendConvertedSamples(byte[] buffer, int byteCount, WaveFormat sourceFormat, Queue<float> target)
    {
        foreach (var s in ConvertToFloatMono48k(buffer, byteCount, sourceFormat))
            target.Enqueue(s);
    }

    private List<float> ConvertToFloatMono48k(byte[] buffer, int byteCount, WaveFormat sourceFormat)
    {
        using var ms = new MemoryStream(buffer, 0, byteCount, writable: false, publiclyVisible: true);
        using var raw = new RawSourceWaveStream(ms, sourceFormat);
        var toSample = new WaveToSampleProvider(raw);
        ISampleProvider sp = toSample;
        if (sp.WaveFormat.Channels == 2)
        {
            sp = new StereoToMonoSampleProvider(toSample)
            {
                LeftVolume = 0.5f,
                RightVolume = 0.5f
            };
        }
        else if (sp.WaveFormat.Channels > 2)
        {
            sp = new DownmixToMonoSampleProvider(toSample, _mixScratch);
        }

        if (sp.WaveFormat.SampleRate != TargetSampleRate)
            sp = new WdlResamplingSampleProvider(sp, TargetSampleRate);

        var list = new List<float>(byteCount / Math.Max(1, sourceFormat.BlockAlign));
        var readBuf = new float[sp.WaveFormat.SampleRate / 5];
        int read;
        while ((read = sp.Read(readBuf, 0, readBuf.Length)) > 0)
        {
            for (var i = 0; i < read; i++)
                list.Add(readBuf[i]);
        }

        return list;
    }

    private static void TrimIfNeeded(Queue<float> q)
    {
        const int maxSamples = FrameSamples * 200;
        while (q.Count > maxSamples)
            q.Dequeue();
    }

    private static int PeakTo100(float[] data, int length)
    {
        var m = 0f;
        for (var i = 0; i < length; i++)
        {
            var a = Math.Abs(data[i]);
            if (a > m) m = a;
        }

        return (int)Math.Clamp(Math.Round(m * 100.0, MidpointRounding.AwayFromZero), 0, 100);
    }

    private static void SafeDispose(object? d)
    {
        if (d is IDisposable id)
        {
            try
            {
                id.Dispose();
            }
            catch
            {
                /* ignore */
            }
        }
    }

    /// <summary>
    /// 将 <see cref="BufferedWaveProvider"/> 的 Read / AddSamples 置于同一把锁下，供采集线程写入、WasapiOut 线程读取。
    /// </summary>
    private sealed class SynchronizedFloatPlayback : IWaveProvider
    {
        private readonly BufferedWaveProvider _inner;
        private readonly object _lockObj;

        public SynchronizedFloatPlayback(BufferedWaveProvider inner, object lockObj)
        {
            _inner = inner;
            _lockObj = lockObj;
        }

        public WaveFormat WaveFormat => _inner.WaveFormat;

        public int Read(byte[] buffer, int offset, int count)
        {
            lock (_lockObj) return _inner.Read(buffer, offset, count);
        }

        public void Write(byte[] data, int count)
        {
            lock (_lockObj) _inner.AddSamples(data, 0, count);
        }
    }

    /// <summary>多声道降为单声道（取各声道平均）。</summary>
    private sealed class DownmixToMonoSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private readonly float[] _scratch;
        private readonly WaveFormat _wf;

        public DownmixToMonoSampleProvider(ISampleProvider source, float[] scratch)
        {
            _source = source;
            _scratch = scratch;
            _wf = WaveFormat.CreateIeeeFloatWaveFormat(source.WaveFormat.SampleRate, 1);
        }

        public WaveFormat WaveFormat => _wf;

        public int Read(float[] buffer, int offset, int count)
        {
            var ch = _source.WaveFormat.Channels;
            var need = count * ch;
            if (need > _scratch.Length)
                throw new InvalidOperationException("混音临时缓冲不足。");

            var got = _source.Read(_scratch, 0, need);
            var frames = got / ch;
            for (var i = 0; i < frames; i++)
            {
                double sum = 0;
                for (var ci = 0; ci < ch; ci++)
                    sum += _scratch[i * ch + ci];
                buffer[offset + i] = (float)(sum / ch);
            }

            return frames;
        }
    }
}

public sealed class AudioLevelsEventArgs : EventArgs
{
    public AudioLevelsEventArgs(int nearPeak, int farPeak, int outputPeak)
    {
        NearPeak = nearPeak;
        FarPeak = farPeak;
        OutputPeak = outputPeak;
    }

    /// <summary>监听输入（麦克风）峰值 0–100。</summary>
    public int NearPeak { get; }

    /// <summary>监听输出回路（参考）峰值 0–100。</summary>
    public int FarPeak { get; }

    /// <summary>经 DLL 处理后的输出峰值 0–100。</summary>
    public int OutputPeak { get; }
}
