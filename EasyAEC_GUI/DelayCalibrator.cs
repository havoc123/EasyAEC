using System.Numerics;
using MathNet.Numerics.IntegralTransforms;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace EasyAEC_GUI;

/// <summary>
/// 使用 Chirp + GCC-PHAT 自动测算系统物理回声路径延迟。
/// </summary>
public static class DelayCalibrator
{
    private static int? _lastRecommendedDelayMs;

    public sealed class CalibrationResult
    {
        public required int DelayMs { get; init; }
        public required int MedianDelayMs { get; init; }
        public required bool IsClamped { get; init; }
        public required IReadOnlyList<int> SamplesMs { get; init; }
        public required IReadOnlyList<int> InlierSamplesMs { get; init; }
        public required IReadOnlyList<int> HighConfidenceSamplesMs { get; init; }
        public required double AverageConfidenceRatio { get; init; }
        public required bool IsReliable { get; init; }
        public required int ClusterStartMs { get; init; }
        public required int ClusterEndMs { get; init; }
        public required int ClusterCount { get; init; }
        public required int RawRecommendedMs { get; init; }
    }

    private const int SampleRate = 48000;
    // 为“快速自动标定”缩短测试时长：120ms 激励 + 240ms 采集窗。
    private const int ChirpDurationMs = 120;
    private const int RecordDurationMs = 240;
    private const int SearchMinDelayMs = 20;
    private const int SearchMaxDelayMsDefault = 120;
    private const float Epsilon = 1e-8f;

    /// <summary>
    /// 自动测算延迟并返回毫秒值（非负）。
    /// </summary>
    public static async Task<int> RunCalibrationAsync(string playbackDeviceId, string captureDeviceId, CancellationToken cancellationToken = default)
    {
        var result = await RunCalibrationDetailedAsync(playbackDeviceId, captureDeviceId, 5, 300, null, cancellationToken).ConfigureAwait(false);
        return result.DelayMs;
    }

    /// <summary>
    /// 多次测量并取中位数，最后做范围限制。
    /// </summary>
    public static async Task<CalibrationResult> RunCalibrationDetailedAsync(
        string playbackDeviceId,
        string captureDeviceId,
        int repeatCount,
        int maxDelayMs,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playbackDeviceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(captureDeviceId);

        repeatCount = Math.Clamp(repeatCount, 3, 15);
        maxDelayMs = Math.Clamp(maxDelayMs, 60, 160);

        progress?.Report($"正在测量 1/{repeatCount}...");

        var samples = new List<int>(repeatCount);
        var confidentSamples = new List<int>(repeatCount);
        var confidenceRatios = new List<double>(repeatCount);

        for (var i = 0; i < repeatCount; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            progress?.Report($"正在测量 {i + 1}/{repeatCount}...");

            var reference = GenerateLinearChirp();
            var capture = await PlayAndCaptureAsync(playbackDeviceId, captureDeviceId, reference, cancellationToken).ConfigureAwait(false);

            var effectiveMaxDelayMs = Math.Min(maxDelayMs, SearchMaxDelayMsDefault);
            var minSearchSamples = Math.Max(0, SearchMinDelayMs * SampleRate / 1000);
            var maxSearchSamples = Math.Max(minSearchSamples + 1, effectiveMaxDelayMs * SampleRate / 1000);
            var (delaySamples, confidenceRatio) = EstimateDelayByMatchedFilter(reference, capture, minSearchSamples, maxSearchSamples);
            var delayMs = (int)Math.Round(delaySamples * 1000.0 / SampleRate, MidpointRounding.AwayFromZero);
            delayMs = Math.Max(0, delayMs);

            samples.Add(delayMs);
            confidenceRatios.Add(confidenceRatio);

            // 主峰/次峰比越高，说明该次估计越可信。
            if (confidenceRatio >= 1.20)
                confidentSamples.Add(delayMs);

            // 稍作间隔，避免设备缓冲残留影响下一次测量。
            if (i < repeatCount - 1)
                await Task.Delay(80, cancellationToken).ConfigureAwait(false);
        }

        progress?.Report("正在汇总测量结果...");

        var confidenceMean = confidenceRatios.Count > 0 ? confidenceRatios.Average() : 0d;

        var preSource = confidentSamples.Count >= Math.Max(3, repeatCount / 2)
            ? confidentSamples
            : samples;

        var inliers = FilterInliersByMad(preSource, 20);
        var source = inliers.Count >= 3 ? inliers : preSource;

        var binnedSource = source.Count >= 3 ? source : samples;
        var (clusterStartMs, clusterEndMs, clusterCount, clusteredSamples) = FindDominantClusterByBin(binnedSource, 10);

        var rawRecommendedMs = Median(clusteredSamples);
        var clamped = Math.Clamp(rawRecommendedMs, 0, maxDelayMs);

        // 轻量平滑，减少相邻两次测量跳变。
        var smoothed = _lastRecommendedDelayMs.HasValue
            ? (int)Math.Round(_lastRecommendedDelayMs.Value * 0.7 + clamped * 0.3, MidpointRounding.AwayFromZero)
            : clamped;
        smoothed = Math.Clamp(smoothed, 0, maxDelayMs);
        _lastRecommendedDelayMs = smoothed;

        var reliable = clusterCount >= Math.Max(3, repeatCount / 2) && confidenceMean >= 1.12;

        return new CalibrationResult
        {
            DelayMs = smoothed,
            MedianDelayMs = rawRecommendedMs,
            IsClamped = smoothed != rawRecommendedMs,
            SamplesMs = samples.AsReadOnly(),
            InlierSamplesMs = inliers.AsReadOnly(),
            HighConfidenceSamplesMs = confidentSamples.AsReadOnly(),
            AverageConfidenceRatio = confidenceMean,
            IsReliable = reliable,
            ClusterStartMs = clusterStartMs,
            ClusterEndMs = clusterEndMs,
            ClusterCount = clusterCount,
            RawRecommendedMs = rawRecommendedMs
        };
    }

    /// <summary>
    /// 生成 300ms 的对数扫频 Chirp：20Hz -> 20000Hz，并施加 Hann 窗。
    /// 对数扫频在真实声学链路（含设备频响起伏）中通常比线性扫频更稳。
    /// </summary>
    private static float[] GenerateLinearChirp()
    {
        var sampleCount = SampleRate * ChirpDurationMs / 1000;
        var signal = new float[sampleCount];

        const double startHz = 20.0;
        const double endHz = 20000.0;
        const double gain = 0.9;

        var lnRatio = Math.Log(endHz / startHz);
        var durationSec = ChirpDurationMs / 1000.0;
        var k = durationSec / lnRatio;

        for (var i = 0; i < sampleCount; i++)
        {
            var t = i / (double)SampleRate;
            var phase = 2.0 * Math.PI * startHz * k * (Math.Exp(t / k) - 1.0);

            // Hann 窗抑制首尾突变，降低旁瓣，提升主峰可分辨性。
            var w = 0.5 * (1.0 - Math.Cos(2.0 * Math.PI * i / Math.Max(1, sampleCount - 1)));
            signal[i] = (float)(gain * w * Math.Sin(phase));
        }

        return signal;
    }

    /// <summary>
    /// 播放参考 Chirp，并并行录制 500ms 麦克风数据。
    /// </summary>
    private static async Task<float[]> PlayAndCaptureAsync(string playbackDeviceId, string captureDeviceId, float[] reference, CancellationToken cancellationToken)
    {
        using var enumerator = new MMDeviceEnumerator();
        using var playbackDevice = enumerator.GetDevice(playbackDeviceId);
        using var captureDevice = enumerator.GetDevice(captureDeviceId);

        using var captureRawStream = new MemoryStream(SampleRate * RecordDurationMs * 4);
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var capture = new WasapiCapture(captureDevice);
        capture.DataAvailable += (_, e) =>
        {
            if (e.BytesRecorded <= 0)
                return;

            // 先按原始字节流连续拼接，避免“分块重采样”引入的时间轴抖动。
            captureRawStream.Write(e.Buffer, 0, e.BytesRecorded);
        };
        capture.RecordingStopped += (_, e) =>
        {
            if (e.Exception is not null)
                tcs.TrySetException(e.Exception);
            else
                tcs.TrySetResult(true);
        };

        var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(SampleRate, 1);
        var provider = new BufferedWaveProvider(waveFormat)
        {
            BufferLength = Math.Max(reference.Length * sizeof(float) * 8, SampleRate * sizeof(float)),
            DiscardOnBufferOverflow = true
        };

        var refBytes = new byte[reference.Length * sizeof(float)];
        Buffer.BlockCopy(reference, 0, refBytes, 0, refBytes.Length);
        provider.AddSamples(refBytes, 0, refBytes.Length);

        using var wasapiOut = new WasapiOut(playbackDevice, AudioClientShareMode.Shared, true, 30);
        wasapiOut.Init(provider);

        capture.StartRecording();
        wasapiOut.Play();

        try
        {
            await Task.Delay(RecordDurationMs, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            try
            {
                wasapiOut.Stop();
            }
            catch
            {
                // 忽略停止时的设备状态异常
            }

            try
            {
                capture.StopRecording();
            }
            catch
            {
                // 忽略停止时的设备状态异常
            }
        }

        await tcs.Task.ConfigureAwait(false);

        // 停录后再统一做一次格式转换（含下混与重采样），保证时间轴连续。
        var captureSamples = ConvertToFloatMono48k(captureRawStream.ToArray(), (int)captureRawStream.Length, capture.WaveFormat);

        // 统一长度：至少覆盖参考长度，后续 GCC-PHAT 会继续补零到 2 的幂。
        var targetLength = Math.Max(reference.Length, captureSamples.Count);
        if (captureSamples.Count < targetLength)
            captureSamples.AddRange(Enumerable.Repeat(0f, targetLength - captureSamples.Count));

        return captureSamples.ToArray();
    }

    /// <summary>
    /// 匹配滤波估计延迟（样本点）。
    /// 实现方式：FFT(reference/capture) -> Y(f)*conj(X(f)) -> IFFT 得到互相关。
    /// 与 GCC-PHAT 相比，匹配滤波保留幅度信息，在当前运行链路下通常更稳定。
    /// </summary>
    private static (int DelaySamples, double ConfidenceRatio) EstimateDelayByMatchedFilter(float[] reference, float[] capture, int minSearchSamples, int maxSearchSamples)
    {
        var n = NextPowerOfTwo(reference.Length + capture.Length);
        var x = new Complex[n];
        var y = new Complex[n];

        for (var i = 0; i < reference.Length; i++)
            x[i] = new Complex(reference[i], 0d);
        for (var i = 0; i < capture.Length; i++)
            y[i] = new Complex(capture[i], 0d);

        // 1) 前向 FFT。
        Fourier.Forward(x, FourierOptions.Matlab);
        Fourier.Forward(y, FourierOptions.Matlab);

        // 2) 互功率谱（不做 PHAT，相当于匹配滤波）。
        var spectrum = new Complex[n];
        for (var i = 0; i < n; i++)
            spectrum[i] = y[i] * Complex.Conjugate(x[i]);

        // 3) 逆变换回时域，得到互相关序列。
        Fourier.Inverse(spectrum, FourierOptions.Matlab);

        // 4) 仅在合理正延迟窗口搜索主峰。
        var start = Math.Clamp(minSearchSamples, 0, n / 2 - 1);
        var endExclusive = Math.Clamp(maxSearchSamples, start + 1, n / 2);
        var bestIndex = start;
        var bestValue = 0d;

        // 先找主峰。
        for (var i = start; i < endExclusive; i++)
        {
            var value = Math.Abs(spectrum[i].Real);
            if (value > bestValue)
            {
                bestValue = value;
                bestIndex = i;
            }
        }

        // 再找“非同一主峰簇”的次峰，避免主峰邻域导致主次峰比恒接近 1。
        var guardSamples = Math.Max(12, SampleRate / 1000 * 3); // 至少 3ms 保护带
        var secondBest = 0d;
        for (var i = start; i < endExclusive; i++)
        {
            if (Math.Abs(i - bestIndex) <= guardSamples)
                continue;

            var value = Math.Abs(spectrum[i].Real);
            if (value > secondBest)
                secondBest = value;
        }

        if (secondBest < Epsilon)
            secondBest = Epsilon;

        var confidence = bestValue / secondBest;
        return (bestIndex, confidence);
    }

    /// <summary>
    /// 使用 MAD（中位数绝对偏差）剔除离群测量点，提升稳定性。
    /// </summary>
    private static List<int> FilterInliersByMad(IReadOnlyList<int> values, int thresholdMs)
    {
        if (values.Count <= 2)
            return values.ToList();

        var ordered = values.OrderBy(v => v).ToArray();
        var median = ordered[ordered.Length / 2];

        var deviations = values.Select(v => Math.Abs(v - median)).OrderBy(v => v).ToArray();
        var mad = deviations[deviations.Length / 2];

        var dynamicThreshold = Math.Max(thresholdMs, mad * 3);
        return values.Where(v => Math.Abs(v - median) <= dynamicThreshold).ToList();
    }

    private static (int StartMs, int EndMs, int Count, List<int> Samples) FindDominantClusterByBin(IReadOnlyList<int> values, int binSizeMs)
    {
        if (values.Count == 0)
            return (0, binSizeMs, 0, new List<int> { 0 });

        var bins = values
            .GroupBy(v => Math.Max(0, v / binSizeMs))
            .Select(g => new { Bin = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.Bin)
            .ToList();

        var bestBin = bins[0].Bin;
        var start = bestBin * binSizeMs;
        var end = start + binSizeMs;

        var clustered = values.Where(v => v >= start && v < end).ToList();
        if (clustered.Count == 0)
            clustered = values.ToList();

        return (start, end, clustered.Count, clustered);
    }

    private static int Median(IReadOnlyList<int> values)
    {
        if (values.Count == 0)
            return 0;

        var ordered = values.OrderBy(v => v).ToArray();
        return ordered[ordered.Length / 2];
    }

    private static int NextPowerOfTwo(int value)
    {
        if (value <= 1)
            return 1;

        var n = 1;
        while (n < value)
            n <<= 1;
        return n;
    }

    private static List<float> ConvertToFloatMono48k(byte[] buffer, int byteCount, WaveFormat sourceFormat)
    {
        using var ms = new MemoryStream(buffer, 0, byteCount, writable: false, publiclyVisible: true);
        using var raw = new RawSourceWaveStream(ms, sourceFormat);
        var toSample = new WaveToSampleProvider(raw);

        ISampleProvider sampleProvider = toSample;
        if (sampleProvider.WaveFormat.Channels == 2)
        {
            sampleProvider = new StereoToMonoSampleProvider(toSample)
            {
                LeftVolume = 0.5f,
                RightVolume = 0.5f
            };
        }
        else if (sampleProvider.WaveFormat.Channels > 2)
        {
            sampleProvider = new MultiplexingSampleProvider(new[] { sampleProvider }, 1);
        }

        if (sampleProvider.WaveFormat.SampleRate != SampleRate)
            sampleProvider = new WdlResamplingSampleProvider(sampleProvider, SampleRate);

        var list = new List<float>(byteCount / Math.Max(1, sourceFormat.BlockAlign));
        var readBuf = new float[SampleRate / 10];
        int read;
        while ((read = sampleProvider.Read(readBuf, 0, readBuf.Length)) > 0)
        {
            for (var i = 0; i < read; i++)
                list.Add(readBuf[i]);
        }

        return list;
    }
}
