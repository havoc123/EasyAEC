using System.Globalization;
using System.Text;

namespace EasyAEC_GUI;

public partial class Form1 : Form
{
    private bool _engineRunning;

    public Form1()
    {
        InitializeComponent();
        Load += Form1_Load;
        InitializeUiDefaults();
    }

    private void Form1_Load(object? sender, EventArgs e)
    {
        Load -= Form1_Load;
        PopulatePlaceholderDevices(log: false);
    }

    /// <summary>
    /// 填充压制强度、占位设备项，并写一条启动日志（尚未连接真实 WASAPI）。
    /// </summary>
    private void InitializeUiDefaults()
    {
        cboSuppressionStrength.Items.Clear();
        cboSuppressionStrength.Items.AddRange(new object[] { "柔和", "标准", "强力" });
        cboSuppressionStrength.SelectedIndex = 1;

        AppendLog("EasyAEC UI 已加载。当前为界面原型：设备列表为占位数据，电平表待接入音频引擎后驱动。");
        SetBriefStatus("就绪");
    }

    /// <summary>
    /// 向底部多行日志区追加一行带时间戳的文本，并滚动到底部。
    /// </summary>
    public void AppendLog(string message)
    {
        if (IsDisposed || !IsHandleCreated)
            return;

        var line = $"[{DateTime.Now:HH:mm:ss.fff}] {message}{Environment.NewLine}";
        if (InvokeRequired)
        {
            BeginInvoke(() => AppendLog(message));
            return;
        }

        txtDebugLog.AppendText(line);
        txtDebugLog.SelectionStart = txtDebugLog.TextLength;
        txtDebugLog.ScrollToCaret();
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
        PopulatePlaceholderDevices(log: true);
    }

    /// <summary>
    /// 占位设备列表；接入 NAudio 后改为枚举 WASAPI 设备。
    /// </summary>
    private void PopulatePlaceholderDevices(bool log)
    {
        const string placeholder = "（占位）尚未枚举设备 — 将使用 NAudio";

        static void Fill(ComboBox box, string role, string ph)
        {
            box.BeginUpdate();
            try
            {
                box.Items.Clear();
                box.Items.Add($"{role} — 默认设备 {ph}");
                box.Items.Add($"{role} — 虚拟线路 1 {ph}");
                box.Items.Add($"{role} — 虚拟线路 2 {ph}");
                box.SelectedIndex = 0;
            }
            finally
            {
                box.EndUpdate();
            }
        }

        Fill(cboListenInput, "监听输入", placeholder);
        Fill(cboListenOutput, "监听输出", placeholder);
        Fill(cboAudioOutput, "音频输出", placeholder);

        if (log)
        {
            AppendLog("已刷新设备列表（占位数据）。接入 NAudio 后将显示真实设备名称与 ID。");
            SetBriefStatus("设备列表已更新（占位）");
        }
    }

    private void BtnStartRun_Click(object? sender, EventArgs e)
    {
        if (_engineRunning)
        {
            AppendLog("引擎已在运行中。");
            return;
        }

        if (!TryParseDelayMs(txtDelayCompensationMs.Text, out var delayMs, out var parseError))
        {
            AppendLog($"延迟补偿无效：{parseError}");
            SetBriefStatus("延迟补偿格式错误");
            MessageBox.Show(this, parseError, "延迟补偿", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var sb = new StringBuilder();
        sb.Append("开始运行 — ");
        sb.Append($"监听输入: {cboListenInput.Text}; ");
        sb.Append($"监听输出: {cboListenOutput.Text}; ");
        sb.Append($"音频输出: {cboAudioOutput.Text}; ");
        sb.Append($"压制强度: {cboSuppressionStrength.Text}; ");
        sb.Append($"延迟补偿: {delayMs.ToString(CultureInfo.InvariantCulture)} ms; ");
        sb.Append($"Debug: {(chkDebugMode.Checked ? "开" : "关")}");
        AppendLog(sb.ToString());

        _engineRunning = true;
        btnStartRun.Enabled = false;
        btnStopRun.Enabled = true;
        SetBriefStatus("运行中（UI 模拟，未接音频）");

        // 电平表占位：清零；接入引擎后在此处更新 Value
        pbrMeterInput.Value = 0;
        pbrMeterReference.Value = 0;
        pbrMeterOutput.Value = 0;
    }

    private void BtnStopRun_Click(object? sender, EventArgs e)
    {
        if (!_engineRunning)
        {
            AppendLog("当前未在运行。");
            return;
        }

        AppendLog("已停止运行。");
        _engineRunning = false;
        btnStartRun.Enabled = true;
        btnStopRun.Enabled = false;
        SetBriefStatus("已停止");

        pbrMeterInput.Value = 0;
        pbrMeterReference.Value = 0;
        pbrMeterOutput.Value = 0;
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
