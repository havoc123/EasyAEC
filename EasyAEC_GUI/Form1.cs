using System.Globalization;
using System.Text.Json;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace EasyAEC_GUI;

public partial class Form1 : Form
{
    private readonly string _configPath = Path.Combine(AppContext.BaseDirectory, "config.json");

    private bool _engineRunning;
    private AudioEngine? _audioEngine;
    private readonly List<int> _recentCalibratedDelays = new();

    private readonly NotifyIcon _trayIcon;
    private readonly ContextMenuStrip _trayMenu;
    private bool _allowExitFromTray;
    private bool _trayHintShown;

    private sealed class WasapiDeviceItem
    {
        public required string Id { get; init; }
        public required string DisplayName { get; init; }

        public override string ToString() => DisplayName;
    }

    private sealed class AppConfig
    {
        public string? ListenInputDeviceId { get; set; }
        public string? ListenOutputDeviceId { get; set; }
        public string? AudioOutputDeviceId { get; set; }
        public int SuppressionStrengthIndex { get; set; } = 1;
        public int DelayCompensationMs { get; set; } = 0;
        public bool DebugMode { get; set; }
    }

    public Form1()
    {
        InitializeComponent();
        LogWriter.Attach(this, chkDebugMode, txtDebugLog);

        _trayMenu = new ContextMenuStrip();
        _trayMenu.Items.Add("打开 EasyAEC", null, (_, _) => RestoreFromTray());
        _trayMenu.Items.Add("退出", null, (_, _) => ExitFromTray());

        _trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "EasyAEC",
            Visible = false,
            ContextMenuStrip = _trayMenu
        };
        _trayIcon.DoubleClick += (_, _) => RestoreFromTray();

        FormClosing += Form1_FormClosing;
        Resize += Form1_Resize;
        Load += Form1_Load;
        InitializeUiDefaults();
    }

    private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (!_allowExitFromTray)
        {
            e.Cancel = true;
            HideToTray();
            return;
        }

        FormClosing -= Form1_FormClosing;
        Resize -= Form1_Resize;

        SaveConfig();

        if (_engineRunning || _audioEngine is not null)
        {
            StopAudioEngineInternal();
            try
            {
                AecWrapper.AEC_Destroy();
            }
            catch (DllNotFoundException)
            {
            }
            catch (BadImageFormatException)
            {
            }

            _engineRunning = false;
        }

        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        _trayMenu.Dispose();
    }

    private void Form1_Load(object? sender, EventArgs e)
    {
        Load -= Form1_Load;
        RefreshAudioDevices(log: false);
        LoadConfig();
    }

    private void Form1_Resize(object? sender, EventArgs e)
    {
        if (WindowState == FormWindowState.Minimized)
            HideToTray();
    }

    private void HideToTray()
    {
        Hide();
        ShowInTaskbar = false;
        _trayIcon.Visible = true;

        if (!_trayHintShown)
        {
            _trayHintShown = true;
            _trayIcon.BalloonTipTitle = "EasyAEC";
            _trayIcon.BalloonTipText = "程序已最小化到托盘，双击托盘图标可恢复。";
            _trayIcon.ShowBalloonTip(1800);
        }
    }

    private void RestoreFromTray()
    {
        Show();
        ShowInTaskbar = true;
        WindowState = FormWindowState.Normal;
        Activate();
        _trayIcon.Visible = false;
    }

    private void ExitFromTray()
    {
        _allowExitFromTray = true;
        _trayIcon.Visible = false;
        Close();
    }

    /// <summary>
    /// 填充压制强度等默认值。
    /// </summary>
    private void InitializeUiDefaults()
    {
        cboSuppressionStrength.Items.Clear();
        cboSuppressionStrength.Items.AddRange(new object[] { "柔和", "标准", "强力" });
        cboSuppressionStrength.SelectedIndex = 1;

        LogWriter.LogInfo("UI", "EasyAEC UI 已加载。窗体加载完成后将枚举 WASAPI 设备；开始运行后由 AudioEngine 驱动电平表。");
        SetBriefStatus("就绪");
    }

    private void SetBriefStatus(string text)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => SetBriefStatus(text));
            return;
        }

        tsslStatus.Text = text;
    }

    private void BtnRefreshDevices_Click(object? sender, EventArgs e)
    {
        RefreshAudioDevices(log: true);
    }

    /// <summary>
    /// 使用 NAudio MMDeviceEnumerator 枚举当前系统中活动的 WASAPI 设备。
    /// </summary>
    private void RefreshAudioDevices(bool log)
    {
        try
        {
            var captureItems = EnumerateDevices(DataFlow.Capture);
            var renderItems = EnumerateDevices(DataFlow.Render);

            FillComboWithDevices(cboListenInput, captureItems);
            FillComboWithDevices(cboListenOutput, renderItems);
            FillComboWithDevices(cboAudioOutput, renderItems);

            if (log)
            {
                LogWriter.LogInfo("Audio",
                    $"已刷新设备列表：Capture {captureItems.Count} 个，Render {renderItems.Count} 个。");
                SetBriefStatus("设备列表已更新");
            }
        }
        catch (Exception ex)
        {
            LogWriter.LogError("Audio", $"枚举音频设备失败：{ex.Message}");
            SetBriefStatus("设备枚举失败");
        }
    }

    private static List<WasapiDeviceItem> EnumerateDevices(DataFlow dataFlow)
    {
        var list = new List<WasapiDeviceItem>();
        using var enumerator = new MMDeviceEnumerator();
        foreach (var dev in enumerator.EnumerateAudioEndPoints(dataFlow, DeviceState.Active))
        {
            using (dev)
            {
                list.Add(new WasapiDeviceItem { Id = dev.ID, DisplayName = dev.FriendlyName });
            }
        }

        return list;
    }

    private static void FillComboWithDevices(ComboBox box, IReadOnlyList<WasapiDeviceItem> items)
    {
        box.BeginUpdate();
        try
        {
            box.Items.Clear();
            foreach (var it in items)
                box.Items.Add(it);

            if (box.Items.Count > 0)
                box.SelectedIndex = 0;
        }
        finally
        {
            box.EndUpdate();
        }
    }

    private static string GetSelectedDeviceDisplayName(ComboBox box)
    {
        if (box.SelectedItem is WasapiDeviceItem w)
            return w.DisplayName;
        return string.IsNullOrWhiteSpace(box.Text) ? "（未选择）" : box.Text.Trim();
    }

    private static string? GetSelectedDeviceId(ComboBox box) =>
        box.SelectedItem is WasapiDeviceItem w ? w.Id : null;

    private void BtnStartRun_Click(object? sender, EventArgs e)
    {
        if (_engineRunning)
        {
            LogWriter.LogWarning("Runtime", "引擎已在运行中。");
            return;
        }

        if (!TryParseDelayMs(txtDelayCompensationMs.Text, out var delayMs, out var parseError))
        {
            LogWriter.LogWarning("Runtime", $"延迟补偿无效：{parseError}");
            SetBriefStatus("延迟补偿格式错误");
            MessageBox.Show(this, parseError, "延迟补偿", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        SaveConfig();

        var suppressionLevel = Math.Clamp(cboSuppressionStrength.SelectedIndex, 0, 2);
        const int sampleRate = 48000;
        try
        {
            if (AecWrapper.AEC_Init(sampleRate, suppressionLevel) == 0)
            {
                LogWriter.LogError("AEC_Core", "AEC_Init 返回失败。");
                SetBriefStatus("AEC 初始化失败");
                MessageBox.Show(this, "AEC_Init 返回失败。", "AEC_Core", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            LogWriter.LogInfo("AEC_Core", "DLL 初始化成功！");
        }
        catch (DllNotFoundException ex)
        {
            LogWriter.LogError("AEC_Core", $"找不到 AEC_Core.dll：{ex.Message}");
            SetBriefStatus("缺少 AEC_Core.dll");
            MessageBox.Show(this,
                "未找到 AEC_Core.dll。请先编译 C++ 项目 AEC_Core（x64），并确认 DLL 与 exe 在同一输出目录。",
                "AEC_Core",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }
        catch (BadImageFormatException ex)
        {
            LogWriter.LogError("AEC_Core", $"DLL 与进程架构不匹配（需 64 位）：{ex.Message}");
            SetBriefStatus("DLL 架构错误");
            MessageBox.Show(this,
                "AEC_Core.dll 与当前进程位数不一致。请将 C# 与 C++ 均设为 x64 后重新生成。",
                "AEC_Core",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        LogWriter.LogInfo("Runtime",
            $"监听输入: {GetSelectedDeviceDisplayName(cboListenInput)}; " +
            $"监听输出: {GetSelectedDeviceDisplayName(cboListenOutput)}; " +
            $"音频输出: {GetSelectedDeviceDisplayName(cboAudioOutput)}; " +
            $"压制强度: {cboSuppressionStrength.Text}; " +
            $"延迟补偿: {delayMs.ToString(CultureInfo.InvariantCulture)} ms");

        var idMic = GetSelectedDeviceId(cboListenInput);
        var idLoop = GetSelectedDeviceId(cboListenOutput);
        var idPlay = GetSelectedDeviceId(cboAudioOutput);
        if (string.IsNullOrWhiteSpace(idMic) || string.IsNullOrWhiteSpace(idLoop) || string.IsNullOrWhiteSpace(idPlay))
        {
            LogWriter.LogWarning("Runtime", "请先刷新并选择有效的音频设备。");
            MessageBox.Show(this, "请先刷新设备列表，并为三个下拉框各选一个设备。", "设备", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        AudioEngine? engine = null;
        try
        {
            engine = new AudioEngine();
            engine.LevelsUpdated += AudioEngine_OnLevelsUpdated;
            engine.Start(idMic, idLoop, idPlay, delayMs);
            _audioEngine = engine;
            engine = null;
        }
        catch (Exception ex)
        {
            if (engine is not null)
            {
                engine.LevelsUpdated -= AudioEngine_OnLevelsUpdated;
                try
                {
                    engine.Dispose();
                }
                catch
                {
                    /* ignore */
                }
            }

            LogWriter.LogError("AudioEngine", $"启动失败：{ex.Message}");
            MessageBox.Show(this, $"音频引擎启动失败：{ex.Message}", "AudioEngine", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        _engineRunning = true;
        btnStartRun.Enabled = false;
        btnStopRun.Enabled = true;
        SetBriefStatus("运行中（WASAPI 实时）");

        pbrMeterInput.Value = 0;
        pbrMeterReference.Value = 0;
        pbrMeterOutput.Value = 0;
    }

    private void AudioEngine_OnLevelsUpdated(object? sender, AudioLevelsEventArgs e)
    {
        if (IsDisposed || !IsHandleCreated)
            return;

        // 引擎停止后忽略所有延迟到达的电平回调，避免电平表“继续跳动”。
        if (!_engineRunning || sender != _audioEngine)
            return;

        if (InvokeRequired)
        {
            BeginInvoke(() => AudioEngine_OnLevelsUpdated(sender, e));
            return;
        }

        if (!_engineRunning || sender != _audioEngine)
            return;

        pbrMeterInput.Value = e.NearPeak;
        pbrMeterReference.Value = e.FarPeak;
        pbrMeterOutput.Value = e.OutputPeak;
    }

    private void BtnStopRun_Click(object? sender, EventArgs e)
    {
        if (!_engineRunning)
        {
            LogWriter.LogInfo("Runtime", "当前未在运行。");
            return;
        }

        LogWriter.LogInfo("Runtime", "已停止运行。");
        StopAudioEngineInternal();

        try
        {
            AecWrapper.AEC_Destroy();
        }
        catch (DllNotFoundException)
        {
            // DLL 缺失时忽略清理
        }
        catch (BadImageFormatException)
        {
        }

        _engineRunning = false;
        btnStartRun.Enabled = true;
        btnStopRun.Enabled = false;
        SetBriefStatus("已停止");

        pbrMeterInput.Value = 0;
        pbrMeterReference.Value = 0;
        pbrMeterOutput.Value = 0;
    }

    private async void BtnAutoCalibrate_Click(object? sender, EventArgs e)
    {
        btnAutoCalibrate.Enabled = false;
        var oldStartEnabled = btnStartRun.Enabled;
        var oldStopEnabled = btnStopRun.Enabled;
        btnStartRun.Enabled = false;
        btnStopRun.Enabled = false;

        try
        {
            var idMic = GetSelectedDeviceId(cboListenInput);
            var idLoop = GetSelectedDeviceId(cboListenOutput);
            var idPlay = GetSelectedDeviceId(cboAudioOutput);
            if (string.IsNullOrWhiteSpace(idMic) || string.IsNullOrWhiteSpace(idLoop) || string.IsNullOrWhiteSpace(idPlay))
            {
                MessageBox.Show(this, "请先刷新设备列表，并为三个下拉框各选一个设备。", "设备", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_engineRunning)
            {
                LogWriter.LogInfo("Calibrator", "检测到引擎正在运行，已自动停止以进行标定。");
                StopAudioEngineInternal();
                try
                {
                    AecWrapper.AEC_Destroy();
                }
                catch (DllNotFoundException)
                {
                }
                catch (BadImageFormatException)
                {
                }

                _engineRunning = false;
            }

            var suppressionLevel = Math.Clamp(cboSuppressionStrength.SelectedIndex, 0, 2);
            const int sampleRate = 48000;
            if (AecWrapper.AEC_Init(sampleRate, suppressionLevel) == 0)
                throw new InvalidOperationException("AEC_Init 返回失败。请确认 AEC_Core.dll 已正确加载。");

            var engine = new AudioEngine();
            engine.LevelsUpdated += AudioEngine_OnLevelsUpdated;
            engine.Start(idMic, idLoop, idPlay, 60);
            _audioEngine = engine;
            _engineRunning = true;

            using var testTone = StartCalibrationReferenceTone();
            await Task.Delay(180); // 预热输出/回采缓冲

            // 运行态快速标定：粗扫(5点) + 细扫(3点)
            // 每个候选点测两次取均值，降低瞬时抖动。
            const int repeatsPerCandidate = 2;
            const int coarseWindowMs = 260;

            var currentDelayMs = 60;

            // 锁定窗口：以上次值为中心，仅扫 ±9ms（步长 3ms），提升稳定性与速度。
            var coarseCandidates = Enumerable.Range(-3, 7)
                .Select(i => Math.Clamp(currentDelayMs + i * 3, 20, 140))
                .Distinct()
                .OrderBy(v => v)
                .ToArray();

            var coarseScores = new List<(int DelayMs, double OutRms, double FarRms, double Ratio, int ConvMs)>();
            for (var i = 0; i < coarseCandidates.Length; i++)
            {
                var d = coarseCandidates[i];
                SetBriefStatus($"运行态标定(粗扫) {i + 1}/{coarseCandidates.Length}：{d} ms");

                var outAccum = 0d;
                var farAccum = 0d;
                var ratioAccum = 0d;
                var convAccum = 0d;
                for (var r = 0; r < repeatsPerCandidate; r++)
                {
                    _audioEngine.SetDelayCompensationMs(d);
                    _audioEngine.ResetCalibrationEnergy();
                    await Task.Delay(coarseWindowMs);
                    var score = _audioEngine.GetCalibrationScore();
                    outAccum += score.OutRms;
                    farAccum += score.FarRms;
                    ratioAccum += score.ResidualRatio;
                    convAccum += score.ConvergenceMs == int.MaxValue ? coarseWindowMs + 50 : score.ConvergenceMs;
                }

                coarseScores.Add((
                    d,
                    outAccum / repeatsPerCandidate,
                    farAccum / repeatsPerCandidate,
                    ratioAccum / repeatsPerCandidate,
                    (int)Math.Round(convAccum / repeatsPerCandidate, MidpointRounding.AwayFromZero)));
            }

            var coarseBest = coarseScores
                .OrderBy(x => x.ConvMs)
                .ThenBy(x => x.Ratio)
                .ThenBy(x => x.OutRms)
                .First();
            var center = coarseBest.DelayMs;
            var fineCandidates = new[] { Math.Max(20, center - 6), center, Math.Min(140, center + 6) }
                .Distinct()
                .OrderBy(v => v)
                .ToArray();

            const int fineWindowMs = 220;
            var fineScores = new List<(int DelayMs, double OutRms, double FarRms, double Ratio, int ConvMs)>();
            for (var i = 0; i < fineCandidates.Length; i++)
            {
                var d = fineCandidates[i];
                SetBriefStatus($"运行态标定(细扫) {i + 1}/{fineCandidates.Length}：{d} ms");

                var outAccum = 0d;
                var farAccum = 0d;
                var ratioAccum = 0d;
                var convAccum = 0d;
                for (var r = 0; r < repeatsPerCandidate; r++)
                {
                    _audioEngine.SetDelayCompensationMs(d);
                    _audioEngine.ResetCalibrationEnergy();
                    await Task.Delay(fineWindowMs);
                    var score = _audioEngine.GetCalibrationScore();
                    outAccum += score.OutRms;
                    farAccum += score.FarRms;
                    ratioAccum += score.ResidualRatio;
                    convAccum += score.ConvergenceMs == int.MaxValue ? fineWindowMs + 50 : score.ConvergenceMs;
                }

                fineScores.Add((
                    d,
                    outAccum / repeatsPerCandidate,
                    farAccum / repeatsPerCandidate,
                    ratioAccum / repeatsPerCandidate,
                    (int)Math.Round(convAccum / repeatsPerCandidate, MidpointRounding.AwayFromZero)));
            }

            // 评分策略（收敛速度优先）：
            // 1) 先比达到阈值 residualRatio<=0.35 的时间(ConvergenceMs)，越小越好；
            // 2) 若收敛时间接近，再比残差比(Ratio)；
            // 3) 参考能量过低时回退到 OutRms。
            const double farFloor = 0.00005;
            var useRatioMode = fineScores.Any(x => x.FarRms >= farFloor);

            var ranked = useRatioMode
                ? fineScores.OrderBy(x => x.ConvMs).ThenBy(x => x.Ratio).ThenBy(x => x.OutRms).ToList()
                : fineScores.OrderBy(x => x.ConvMs).ThenBy(x => x.OutRms).ToList();

            var best = ranked[0];
            var second = ranked.Count > 1
                ? ranked[1]
                : (best.DelayMs, best.OutRms, best.FarRms, best.Ratio, best.ConvMs);

            // 门限按“收敛时间优先 + 次级评分”判定。
            double relativeImprove;
            double absoluteImprove;
            bool hasClearWinner;
            var convImproveMs = second.ConvMs - best.ConvMs;

            if (useRatioMode)
            {
                const double minRelativeImprove = 0.04;   // 4%
                const double minAbsoluteImprove = 0.000012;
                relativeImprove = second.Ratio > 1e-12 ? (second.Ratio - best.Ratio) / second.Ratio : 0d;
                absoluteImprove = second.Ratio - best.Ratio;
                hasClearWinner = convImproveMs >= 10 || relativeImprove >= minRelativeImprove || absoluteImprove >= minAbsoluteImprove;
            }
            else
            {
                const double minRelativeImprove = 0.03;   // 3%
                const double minAbsoluteImprove = 0.00001;
                relativeImprove = second.OutRms > 1e-12 ? (second.OutRms - best.OutRms) / second.OutRms : 0d;
                absoluteImprove = second.OutRms - best.OutRms;
                hasClearWinner = convImproveMs >= 10 || relativeImprove >= minRelativeImprove || absoluteImprove >= minAbsoluteImprove;
            }

            // 滞回门限：新值必须显著优于当前值才更新，避免小波动来回跳。
            var currentScore = fineScores.FirstOrDefault(x => x.DelayMs == currentDelayMs);
            var currentScoreValue = useRatioMode
                ? (currentScore == default ? double.MaxValue : currentScore.Ratio)
                : (currentScore == default ? double.MaxValue : currentScore.OutRms);
            var bestScoreValue = useRatioMode ? best.Ratio : best.OutRms;
            var hysteresisPass = currentScore == default || bestScoreValue <= currentScoreValue * 0.92; // 至少好 8%

            // 如果当前还是默认 0ms，且首次标定尚未有“明显赢家”，
            // 仍应用本轮最优值，避免一直卡在 0ms 无法起步。
            var appliedDelayMs = (hasClearWinner && hysteresisPass) || currentDelayMs == 0 ? best.DelayMs : currentDelayMs;
            _audioEngine.SetDelayCompensationMs(appliedDelayMs);

            // 滑动中位数：记录最近 3 次应用值，再写中位数，进一步防抖。
            _recentCalibratedDelays.Add(appliedDelayMs);
            while (_recentCalibratedDelays.Count > 3)
                _recentCalibratedDelays.RemoveAt(0);
            var smoothedDelayMs = _recentCalibratedDelays.OrderBy(v => v).ElementAt(_recentCalibratedDelays.Count / 2);
            _audioEngine.SetDelayCompensationMs(smoothedDelayMs);
            txtDelayCompensationMs.Text = smoothedDelayMs.ToString(CultureInfo.InvariantCulture);

            var appliedByBootstrap = !hasClearWinner && currentDelayMs == 0;
            SetBriefStatus(hasClearWinner
                ? $"运行态标定完成：{smoothedDelayMs} ms"
                : appliedByBootstrap
                    ? $"运行态标定完成：首次自举应用 {smoothedDelayMs} ms"
                    : $"运行态标定完成：候选点差异过小，保持 {smoothedDelayMs} ms");

            var coarseText = string.Join(", ", coarseScores.Select(x => $"{x.DelayMs}ms=conv{x.ConvMs}ms,ratio{x.Ratio:F5},out{x.OutRms:F5}/far{x.FarRms:F5}"));
            var fineText = string.Join(", ", fineScores.Select(x => $"{x.DelayMs}ms=conv{x.ConvMs}ms,ratio{x.Ratio:F5}(out{x.OutRms:F5}/far{x.FarRms:F5})"));
            var scoreMode = useRatioMode ? "ConvergenceMs + Out/Far" : "ConvergenceMs + OutRms(参考能量过低回退)";
            LogWriter.LogInfo("Calibrator",
                $"运行态快速标定完成：推荐={best.DelayMs} ms, 应用={appliedDelayMs} ms, 平滑后={smoothedDelayMs} ms, mode={scoreMode}, clearWinner={hasClearWinner}, hysteresisPass={hysteresisPass}, convImprove={convImproveMs}ms, relImprove={relativeImprove:P1}, absImprove={absoluteImprove:F6}, 粗扫=[{coarseText}], 细扫=[{fineText}], 评分=收敛时间优先，次级看残差");
        }
        catch (Exception ex)
        {
            SetBriefStatus("运行态标定失败");
            LogWriter.LogError("Calibrator", $"运行态快速标定失败：{ex.Message}");
            MessageBox.Show(this, $"运行态标定失败：{ex.Message}", "自动测算延迟", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            try
            {
                StopAudioEngineInternal();
                AecWrapper.AEC_Destroy();
            }
            catch (DllNotFoundException)
            {
            }
            catch (BadImageFormatException)
            {
            }

            _engineRunning = false;
            btnAutoCalibrate.Enabled = true;
            btnStartRun.Enabled = true;
            btnStopRun.Enabled = false;

            pbrMeterInput.Value = 0;
            pbrMeterReference.Value = 0;
            pbrMeterOutput.Value = 0;
        }
    }

    private IDisposable StartCalibrationReferenceTone()
    {
        var loopDeviceId = GetSelectedDeviceId(cboListenOutput);
        if (string.IsNullOrWhiteSpace(loopDeviceId))
            throw new InvalidOperationException("未选择监听输出设备，无法播放标定测试音。");

        var enumerator = new MMDeviceEnumerator();
        var device = enumerator.GetDevice(loopDeviceId);

        // 标定期间向“监听输出”注入稳定测试音，确保 Far 参考能量充足。
        var tone = new SignalGenerator(AudioEngine.TargetSampleRate, 1)
        {
            Type = SignalGeneratorType.Sin,
            Frequency = 997,
            Gain = 0.18
        };

        var wasapiOut = new WasapiOut(device, AudioClientShareMode.Shared, true, 30);
        wasapiOut.Init(tone);
        wasapiOut.Play();

        return new CalibrationToneHandle(enumerator, device, wasapiOut);
    }

    private sealed class CalibrationToneHandle : IDisposable
    {
        private MMDeviceEnumerator? _enumerator;
        private MMDevice? _device;
        private WasapiOut? _output;

        public CalibrationToneHandle(MMDeviceEnumerator enumerator, MMDevice device, WasapiOut output)
        {
            _enumerator = enumerator;
            _device = device;
            _output = output;
        }

        public void Dispose()
        {
            try
            {
                _output?.Stop();
            }
            catch
            {
                // ignore
            }

            try
            {
                _output?.Dispose();
            }
            catch
            {
                // ignore
            }

            try
            {
                _device?.Dispose();
            }
            catch
            {
                // ignore
            }

            try
            {
                _enumerator?.Dispose();
            }
            catch
            {
                // ignore
            }

            _output = null;
            _device = null;
            _enumerator = null;
        }
    }

    private void StopAudioEngineInternal()
    {
        var eng = _audioEngine;
        _audioEngine = null;
        if (eng is null)
            return;
        eng.LevelsUpdated -= AudioEngine_OnLevelsUpdated;
        try
        {
            eng.Stop();
        }
        catch
        {
            /* ignore */
        }

        try
        {
            eng.Dispose();
        }
        catch
        {
            /* ignore */
        }
    }

    private void LoadConfig()
    {
        try
        {
            if (!File.Exists(_configPath))
                return;

            var json = File.ReadAllText(_configPath);
            var config = JsonSerializer.Deserialize<AppConfig>(json);
            if (config is null)
                return;

            SelectComboByDeviceId(cboListenInput, config.ListenInputDeviceId);
            SelectComboByDeviceId(cboListenOutput, config.ListenOutputDeviceId);
            SelectComboByDeviceId(cboAudioOutput, config.AudioOutputDeviceId);

            cboSuppressionStrength.SelectedIndex = Math.Clamp(config.SuppressionStrengthIndex, 0, Math.Max(0, cboSuppressionStrength.Items.Count - 1));
            txtDelayCompensationMs.Text = config.DelayCompensationMs.ToString(CultureInfo.InvariantCulture);
            chkDebugMode.Checked = config.DebugMode;

            LogWriter.LogInfo("Config", $"已加载配置：{_configPath}");
        }
        catch (Exception ex)
        {
            LogWriter.LogWarning("Config", $"加载配置失败：{ex.Message}");
        }
    }

    private void SaveConfig()
    {
        try
        {
            var delayMs = 0;
            _ = int.TryParse(txtDelayCompensationMs.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out delayMs);

            var config = new AppConfig
            {
                ListenInputDeviceId = GetSelectedDeviceId(cboListenInput),
                ListenOutputDeviceId = GetSelectedDeviceId(cboListenOutput),
                AudioOutputDeviceId = GetSelectedDeviceId(cboAudioOutput),
                SuppressionStrengthIndex = Math.Clamp(cboSuppressionStrength.SelectedIndex, 0, 2),
                DelayCompensationMs = delayMs,
                DebugMode = chkDebugMode.Checked
            };

            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configPath, json);
        }
        catch (Exception ex)
        {
            LogWriter.LogWarning("Config", $"保存配置失败：{ex.Message}");
        }
    }

    private static void SelectComboByDeviceId(ComboBox box, string? deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            return;

        for (var i = 0; i < box.Items.Count; i++)
        {
            if (box.Items[i] is WasapiDeviceItem item && string.Equals(item.Id, deviceId, StringComparison.OrdinalIgnoreCase))
            {
                box.SelectedIndex = i;
                return;
            }
        }
    }

    private static bool TryParseDelayMs(string text, out int delayMs, out string error)
    {
        delayMs = 0;
        error = string.Empty;

        var t = text.Trim();
        if (t.Length == 0)
        {
            error = "请输入整数毫秒值（可为负数）。";
            return false;
        }

        if (!int.TryParse(t, NumberStyles.Integer, CultureInfo.InvariantCulture, out delayMs))
        {
            error = "延迟补偿必须是整数（可带负号），单位 ms。";
            return false;
        }

        return true;
    }
}
