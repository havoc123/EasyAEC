using System.Globalization;
using NAudio.CoreAudioApi;

namespace EasyAEC_GUI;

public partial class Form1 : Form
{
    private bool _engineRunning;
    private AudioEngine? _audioEngine;
    private string _configFilePath = Path.Combine(AppContext.BaseDirectory, "config.json");

    private sealed class WasapiDeviceItem
    {
        public required string Id { get; init; }
        public required string DisplayName { get; init; }

        public override string ToString() => DisplayName;
    }

    public Form1()
    {
        InitializeComponent();
        LogWriter.Attach(this, chkDebugMode, txtDebugLog);
        FormClosing += Form1_FormClosing;
        Load += Form1_Load;
        InitializeUiDefaults();
    }

    private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
    {
        FormClosing -= Form1_FormClosing;
        SaveConfig();
        if (!_engineRunning && _audioEngine is null)
            return;
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

    private void Form1_Load(object? sender, EventArgs e)
    {
        Load -= Form1_Load;
        RefreshAudioDevices(log: false);
        LoadConfig();
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
        if (InvokeRequired)
        {
            BeginInvoke(() => AudioEngine_OnLevelsUpdated(sender, e));
            return;
        }

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

    /// <summary>
    /// 保存当前配置到 config.json 文件。
    /// </summary>
    private void SaveConfig()
    {
        try
        {
            var config = new AppConfig
            {
                InDevice = GetSelectedDeviceDisplayName(cboListenInput),
                RefDevice = GetSelectedDeviceDisplayName(cboListenOutput),
                OutDevice = GetSelectedDeviceDisplayName(cboAudioOutput),
                SuppressionLevel = cboSuppressionStrength.SelectedItem?.ToString() ?? "标准",
                DelayMs = int.TryParse(txtDelayCompensationMs.Text.Trim(), NumberStyles.Integer, 
                    CultureInfo.InvariantCulture, out var delay) ? delay : 60,
                IsDebug = chkDebugMode.Checked
            };

            config.SaveToFile(_configFilePath);
            LogWriter.LogInfo("Config", $"配置已保存到 {_configFilePath}");
        }
        catch (Exception ex)
        {
            LogWriter.LogError("Config", $"保存配置失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 从 config.json 文件加载配置。
    /// 在 Form1_Load 中调用（位于设备列表刷新之后）。
    /// </summary>
    private void LoadConfig()
    {
        try
        {
            var config = AppConfig.LoadFromFile(_configFilePath);
            if (config == null)
            {
                LogWriter.LogInfo("Config", "未找到配置文件，使用默认设置。");
                return;
            }

            // 加载设备（验证设备是否存在）
            LoadDeviceIfExists(cboListenInput, config.InDevice);
            LoadDeviceIfExists(cboListenOutput, config.RefDevice);
            LoadDeviceIfExists(cboAudioOutput, config.OutDevice);

            // 加载压制强度
            if (!string.IsNullOrWhiteSpace(config.SuppressionLevel))
            {
                var index = cboSuppressionStrength.Items.IndexOf(config.SuppressionLevel);
                if (index >= 0)
                    cboSuppressionStrength.SelectedIndex = index;
            }

            // 加载延迟补偿
            txtDelayCompensationMs.Text = config.DelayMs.ToString(CultureInfo.InvariantCulture);

            // 加载 Debug 状态
            chkDebugMode.Checked = config.IsDebug;

            LogWriter.LogInfo("Config", "配置已从文件加载。");
        }
        catch (Exception ex)
        {
            LogWriter.LogError("Config", $"加载配置失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 尝试在下拉框中选中指定名称的设备。
    /// 如果设备不存在，则不进行选中。
    /// </summary>
    private static void LoadDeviceIfExists(ComboBox comboBox, string deviceDisplayName)
    {
        if (string.IsNullOrWhiteSpace(deviceDisplayName) || deviceDisplayName == "（未选择）")
            return;

        // 遍历下拉框项，寻找匹配的设备显示名称
        for (int i = 0; i < comboBox.Items.Count; i++)
        {
            if (comboBox.Items[i] is WasapiDeviceItem item && item.DisplayName == deviceDisplayName)
            {
                comboBox.SelectedIndex = i;
                return;
            }
        }

        // 若未找到，则保持默认选择（第一项）
        LogWriter.LogWarning("Config", $"未找到设备 '{deviceDisplayName}'，将使用当前列表中的第一个设备。");
    }
}
