using System.Globalization;
using System.Text;

namespace EasyAEC_GUI;

/// <summary>
/// 结构化调试日志；在「Debug 模式」开启时同步写入底部日志框与 Debug.log。
/// </summary>
public static class LogWriter
{
    private static readonly object FileLock = new();
    private static Form? _uiForm;
    private static Func<bool>? _isDebugMode;
    private static TextBox? _logTextBox;
    private static string _logFilePath = Path.Combine(AppContext.BaseDirectory, "Debug.log");

    /// <summary>在窗体构造中、InitializeComponent 之后调用，绑定 UI 与 Debug 开关。</summary>
    public static void Attach(Form mainForm, CheckBox debugModeCheckBox, TextBox debugLogTextBox)
    {
        _uiForm = mainForm;
        _isDebugMode = () => debugModeCheckBox.Checked;
        _logTextBox = debugLogTextBox;
        _logFilePath = Path.Combine(AppContext.BaseDirectory, "Debug.log");
    }

    public static void LogInfo(string module, string message) => Write("INFO", module, message);

    public static void LogWarning(string module, string message) => Write("WARN", module, message);

    public static void LogError(string module, string message) => Write("ERROR", module, message);

    public static void LogDebug(string module, string message) => Write("DEBUG", module, message);

    private static void Write(string level, string module, string detail)
    {
        var ts = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
        var line = $"[{ts}] [{level}] [{module}] {detail}";
        var fullLine = line + Environment.NewLine;

        if (_isDebugMode?.Invoke() != true)
            return;

        try
        {
            lock (FileLock)
            {
                File.AppendAllText(_logFilePath, fullLine, Encoding.UTF8);
            }
        }
        catch
        {
            // 避免日志本身拖垮主流程
        }

        AppendToUi(fullLine);
    }

    private static void AppendToUi(string fullLine)
    {
        if (_uiForm is null || _logTextBox is null)
            return;

        void Apply()
        {
            var tb = _logTextBox;
            if (tb is null || tb.IsDisposed)
                return;
            tb.AppendText(fullLine);
            tb.SelectionStart = tb.TextLength;
            tb.ScrollToCaret();
        }

        var form = _uiForm;
        if (form is null)
            return;
        if (form.InvokeRequired)
            form.BeginInvoke(Apply);
        else
            Apply();
    }
}
