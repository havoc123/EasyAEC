namespace EasyAEC_GUI;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        grpDeviceRouting = new GroupBox();
        tableDevices = new TableLayoutPanel();
        lblListenInput = new Label();
        cboListenInput = new ComboBox();
        lblListenOutput = new Label();
        cboListenOutput = new ComboBox();
        lblAudioOutput = new Label();
        cboAudioOutput = new ComboBox();
        btnRefreshDevices = new Button();
        panelMeterIn = new Panel();
        lblMeterInTitle = new Label();
        pbrMeterInput = new ProgressBar();
        panelMeterRef = new Panel();
        lblMeterRefTitle = new Label();
        pbrMeterReference = new ProgressBar();
        panelMeterOut = new Panel();
        lblMeterOutTitle = new Label();
        pbrMeterOutput = new ProgressBar();
        grpCoreControl = new GroupBox();
        tableCore = new TableLayoutPanel();
        lblSuppression = new Label();
        cboSuppressionStrength = new ComboBox();
        lblDelayMs = new Label();
        txtDelayCompensationMs = new TextBox();
        chkDebugMode = new CheckBox();
        flowButtons = new FlowLayoutPanel();
        btnStartRun = new Button();
        btnStopRun = new Button();
        pnlLogHost = new Panel();
        txtDebugLog = new TextBox();
        statusStripMain = new StatusStrip();
        tsslStatus = new ToolStripStatusLabel();
        mainLayout = new TableLayoutPanel();
        grpDeviceRouting.SuspendLayout();
        tableDevices.SuspendLayout();
        panelMeterIn.SuspendLayout();
        panelMeterRef.SuspendLayout();
        panelMeterOut.SuspendLayout();
        grpCoreControl.SuspendLayout();
        tableCore.SuspendLayout();
        flowButtons.SuspendLayout();
        pnlLogHost.SuspendLayout();
        statusStripMain.SuspendLayout();
        mainLayout.SuspendLayout();
        SuspendLayout();
        //
        // mainLayout
        //
        mainLayout.ColumnCount = 1;
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        mainLayout.Controls.Add(grpDeviceRouting, 0, 0);
        mainLayout.Controls.Add(grpCoreControl, 0, 1);
        mainLayout.Controls.Add(pnlLogHost, 0, 2);
        mainLayout.Dock = DockStyle.Fill;
        mainLayout.Location = new Point(0, 0);
        mainLayout.Margin = new Padding(0);
        mainLayout.Name = "mainLayout";
        mainLayout.Padding = new Padding(0, 0, 0, 4);
        mainLayout.RowCount = 3;
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        mainLayout.Size = new Size(976, 568);
        mainLayout.TabIndex = 0;
        //
        // grpDeviceRouting
        //
        grpDeviceRouting.Controls.Add(tableDevices);
        grpDeviceRouting.Dock = DockStyle.Fill;
        grpDeviceRouting.Font = new Font("Microsoft YaHei UI", 9F);
        grpDeviceRouting.Location = new Point(0, 0);
        grpDeviceRouting.Margin = new Padding(0, 0, 0, 10);
        grpDeviceRouting.Name = "grpDeviceRouting";
        grpDeviceRouting.Padding = new Padding(14, 10, 14, 12);
        grpDeviceRouting.Size = new Size(976, 200);
        grpDeviceRouting.TabIndex = 0;
        grpDeviceRouting.TabStop = false;
        grpDeviceRouting.Text = "设备路由";
        //
        // tableDevices — 三列等宽 + 右侧刷新；电平表与下拉同列对齐
        //
        tableDevices.AutoSize = true;
        tableDevices.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        tableDevices.ColumnCount = 4;
        tableDevices.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33333F));
        tableDevices.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33333F));
        tableDevices.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33334F));
        tableDevices.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 142F));
        tableDevices.Controls.Add(lblListenInput, 0, 0);
        tableDevices.Controls.Add(lblListenOutput, 1, 0);
        tableDevices.Controls.Add(lblAudioOutput, 2, 0);
        tableDevices.Controls.Add(cboListenInput, 0, 1);
        tableDevices.Controls.Add(cboListenOutput, 1, 1);
        tableDevices.Controls.Add(cboAudioOutput, 2, 1);
        tableDevices.Controls.Add(btnRefreshDevices, 3, 1);
        tableDevices.Controls.Add(panelMeterIn, 0, 2);
        tableDevices.Controls.Add(panelMeterRef, 1, 2);
        tableDevices.Controls.Add(panelMeterOut, 2, 2);
        tableDevices.Dock = DockStyle.Top;
        tableDevices.Location = new Point(14, 30);
        tableDevices.Name = "tableDevices";
        tableDevices.RowCount = 3;
        tableDevices.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        tableDevices.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        tableDevices.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
        tableDevices.Size = new Size(948, 120);
        tableDevices.TabIndex = 0;
        //
        // lblListenInput
        //
        lblListenInput.AutoSize = true;
        lblListenInput.Dock = DockStyle.Fill;
        lblListenInput.Location = new Point(3, 0);
        lblListenInput.Margin = new Padding(3, 0, 6, 4);
        lblListenInput.Name = "lblListenInput";
        lblListenInput.Size = new Size(261, 28);
        lblListenInput.TabIndex = 0;
        lblListenInput.Text = "监听输入";
        lblListenInput.TextAlign = ContentAlignment.BottomLeft;
        //
        // cboListenInput
        //
        cboListenInput.Dock = DockStyle.Fill;
        cboListenInput.DropDownStyle = ComboBoxStyle.DropDownList;
        cboListenInput.FormattingEnabled = true;
        cboListenInput.Location = new Point(3, 31);
        cboListenInput.Margin = new Padding(3, 3, 6, 3);
        cboListenInput.Name = "cboListenInput";
        cboListenInput.Size = new Size(261, 25);
        cboListenInput.TabIndex = 1;
        //
        // lblListenOutput
        //
        lblListenOutput.AutoSize = true;
        lblListenOutput.Dock = DockStyle.Fill;
        lblListenOutput.Location = new Point(273, 0);
        lblListenOutput.Margin = new Padding(3, 0, 6, 4);
        lblListenOutput.Name = "lblListenOutput";
        lblListenOutput.Size = new Size(261, 28);
        lblListenOutput.TabIndex = 2;
        lblListenOutput.Text = "监听输出";
        lblListenOutput.TextAlign = ContentAlignment.BottomLeft;
        //
        // cboListenOutput
        //
        cboListenOutput.Dock = DockStyle.Fill;
        cboListenOutput.DropDownStyle = ComboBoxStyle.DropDownList;
        cboListenOutput.FormattingEnabled = true;
        cboListenOutput.Location = new Point(273, 31);
        cboListenOutput.Margin = new Padding(3, 3, 6, 3);
        cboListenOutput.Name = "cboListenOutput";
        cboListenOutput.Size = new Size(261, 25);
        cboListenOutput.TabIndex = 3;
        //
        // lblAudioOutput
        //
        lblAudioOutput.AutoSize = true;
        lblAudioOutput.Dock = DockStyle.Fill;
        lblAudioOutput.Location = new Point(543, 0);
        lblAudioOutput.Margin = new Padding(3, 0, 6, 4);
        lblAudioOutput.Name = "lblAudioOutput";
        lblAudioOutput.Size = new Size(261, 28);
        lblAudioOutput.TabIndex = 4;
        lblAudioOutput.Text = "音频输出";
        lblAudioOutput.TextAlign = ContentAlignment.BottomLeft;
        //
        // cboAudioOutput
        //
        cboAudioOutput.Dock = DockStyle.Fill;
        cboAudioOutput.DropDownStyle = ComboBoxStyle.DropDownList;
        cboAudioOutput.FormattingEnabled = true;
        cboAudioOutput.Location = new Point(543, 31);
        cboAudioOutput.Margin = new Padding(3, 3, 6, 3);
        cboAudioOutput.Name = "cboAudioOutput";
        cboAudioOutput.Size = new Size(261, 25);
        cboAudioOutput.TabIndex = 5;
        //
        // btnRefreshDevices
        //
        btnRefreshDevices.Anchor = AnchorStyles.Right;
        btnRefreshDevices.AutoSize = true;
        btnRefreshDevices.Location = new Point(3, 33);
        btnRefreshDevices.Margin = new Padding(6, 3, 2, 3);
        btnRefreshDevices.Name = "btnRefreshDevices";
        btnRefreshDevices.Padding = new Padding(8, 2, 8, 2);
        btnRefreshDevices.Size = new Size(131, 28);
        btnRefreshDevices.TabIndex = 6;
        btnRefreshDevices.Text = "刷新设备列表";
        btnRefreshDevices.UseVisualStyleBackColor = true;
        btnRefreshDevices.Click += BtnRefreshDevices_Click;
        //
        // panelMeterIn
        //
        panelMeterIn.Controls.Add(pbrMeterInput);
        panelMeterIn.Controls.Add(lblMeterInTitle);
        panelMeterIn.Dock = DockStyle.Fill;
        panelMeterIn.Location = new Point(3, 69);
        panelMeterIn.Margin = new Padding(3, 3, 6, 0);
        panelMeterIn.Name = "panelMeterIn";
        panelMeterIn.Size = new Size(261, 51);
        panelMeterIn.TabIndex = 7;
        //
        // lblMeterInTitle
        //
        lblMeterInTitle.AutoSize = true;
        lblMeterInTitle.Location = new Point(0, 0);
        lblMeterInTitle.Name = "lblMeterInTitle";
        lblMeterInTitle.Size = new Size(104, 17);
        lblMeterInTitle.TabIndex = 0;
        lblMeterInTitle.Text = "监听输入电平";
        //
        // pbrMeterInput
        //
        pbrMeterInput.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        pbrMeterInput.Location = new Point(0, 22);
        pbrMeterInput.Maximum = 100;
        pbrMeterInput.Name = "pbrMeterInput";
        pbrMeterInput.Size = new Size(261, 18);
        pbrMeterInput.Style = ProgressBarStyle.Continuous;
        pbrMeterInput.TabIndex = 1;
        //
        // panelMeterRef
        //
        panelMeterRef.Controls.Add(pbrMeterReference);
        panelMeterRef.Controls.Add(lblMeterRefTitle);
        panelMeterRef.Dock = DockStyle.Fill;
        panelMeterRef.Location = new Point(273, 69);
        panelMeterRef.Margin = new Padding(3, 3, 6, 0);
        panelMeterRef.Name = "panelMeterRef";
        panelMeterRef.Size = new Size(261, 51);
        panelMeterRef.TabIndex = 8;
        //
        // lblMeterRefTitle
        //
        lblMeterRefTitle.AutoSize = true;
        lblMeterRefTitle.Location = new Point(0, 0);
        lblMeterRefTitle.Name = "lblMeterRefTitle";
        lblMeterRefTitle.Size = new Size(148, 17);
        lblMeterRefTitle.TabIndex = 0;
        lblMeterRefTitle.Text = "监听输出(参考)电平";
        //
        // pbrMeterReference
        //
        pbrMeterReference.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        pbrMeterReference.Location = new Point(0, 22);
        pbrMeterReference.Maximum = 100;
        pbrMeterReference.Name = "pbrMeterReference";
        pbrMeterReference.Size = new Size(261, 18);
        pbrMeterReference.Style = ProgressBarStyle.Continuous;
        pbrMeterReference.TabIndex = 1;
        //
        // panelMeterOut
        //
        panelMeterOut.Controls.Add(pbrMeterOutput);
        panelMeterOut.Controls.Add(lblMeterOutTitle);
        panelMeterOut.Dock = DockStyle.Fill;
        panelMeterOut.Location = new Point(543, 69);
        panelMeterOut.Margin = new Padding(3, 3, 6, 0);
        panelMeterOut.Name = "panelMeterOut";
        panelMeterOut.Size = new Size(261, 51);
        panelMeterOut.TabIndex = 9;
        //
        // lblMeterOutTitle
        //
        lblMeterOutTitle.AutoSize = true;
        lblMeterOutTitle.Location = new Point(0, 0);
        lblMeterOutTitle.Name = "lblMeterOutTitle";
        lblMeterOutTitle.Size = new Size(92, 17);
        lblMeterOutTitle.TabIndex = 0;
        lblMeterOutTitle.Text = "音频输出电平";
        //
        // pbrMeterOutput
        //
        pbrMeterOutput.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        pbrMeterOutput.Location = new Point(0, 22);
        pbrMeterOutput.Maximum = 100;
        pbrMeterOutput.Name = "pbrMeterOutput";
        pbrMeterOutput.Size = new Size(261, 18);
        pbrMeterOutput.Style = ProgressBarStyle.Continuous;
        pbrMeterOutput.TabIndex = 1;
        //
        // grpCoreControl
        //
        grpCoreControl.Controls.Add(tableCore);
        grpCoreControl.Dock = DockStyle.Fill;
        grpCoreControl.Font = new Font("Microsoft YaHei UI", 9F);
        grpCoreControl.Location = new Point(0, 210);
        grpCoreControl.Margin = new Padding(0, 0, 0, 10);
        grpCoreControl.Name = "grpCoreControl";
        grpCoreControl.Padding = new Padding(14, 10, 14, 12);
        grpCoreControl.Size = new Size(976, 86);
        grpCoreControl.TabIndex = 1;
        grpCoreControl.TabStop = false;
        grpCoreControl.Text = "核心控制";
        //
        // tableCore
        //
        tableCore.AutoSize = true;
        tableCore.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        tableCore.ColumnCount = 8;
        tableCore.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88F));
        tableCore.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
        tableCore.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 118F));
        tableCore.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 68F));
        tableCore.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
        tableCore.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tableCore.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 118F));
        tableCore.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 118F));
        tableCore.Controls.Add(lblSuppression, 0, 0);
        tableCore.Controls.Add(cboSuppressionStrength, 1, 0);
        tableCore.Controls.Add(lblDelayMs, 2, 0);
        tableCore.Controls.Add(txtDelayCompensationMs, 3, 0);
        tableCore.Controls.Add(chkDebugMode, 4, 0);
        tableCore.Controls.Add(flowButtons, 6, 0);
        tableCore.Dock = DockStyle.Top;
        tableCore.Location = new Point(14, 30);
        tableCore.Name = "tableCore";
        tableCore.RowCount = 1;
        tableCore.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        tableCore.Size = new Size(948, 40);
        tableCore.TabIndex = 0;
        tableCore.SetColumnSpan(chkDebugMode, 2);
        //
        // lblSuppression
        //
        lblSuppression.AutoSize = true;
        lblSuppression.Dock = DockStyle.Fill;
        lblSuppression.Location = new Point(3, 0);
        lblSuppression.Margin = new Padding(3, 0, 8, 0);
        lblSuppression.Name = "lblSuppression";
        lblSuppression.Size = new Size(77, 40);
        lblSuppression.TabIndex = 0;
        lblSuppression.Text = "压制强度";
        lblSuppression.TextAlign = ContentAlignment.MiddleLeft;
        //
        // cboSuppressionStrength
        //
        cboSuppressionStrength.Dock = DockStyle.Fill;
        cboSuppressionStrength.DropDownStyle = ComboBoxStyle.DropDownList;
        cboSuppressionStrength.FormattingEnabled = true;
        cboSuppressionStrength.Location = new Point(91, 8);
        cboSuppressionStrength.Margin = new Padding(3, 8, 12, 3);
        cboSuppressionStrength.Name = "cboSuppressionStrength";
        cboSuppressionStrength.Size = new Size(105, 25);
        cboSuppressionStrength.TabIndex = 1;
        //
        // lblDelayMs
        //
        lblDelayMs.AutoSize = true;
        lblDelayMs.Dock = DockStyle.Fill;
        lblDelayMs.Location = new Point(211, 0);
        lblDelayMs.Margin = new Padding(3, 0, 6, 0);
        lblDelayMs.Name = "lblDelayMs";
        lblDelayMs.Size = new Size(109, 40);
        lblDelayMs.TabIndex = 2;
        lblDelayMs.Text = "延迟补偿(ms)";
        lblDelayMs.TextAlign = ContentAlignment.MiddleLeft;
        //
        // txtDelayCompensationMs — 约 60px 宽
        //
        txtDelayCompensationMs.Anchor = AnchorStyles.Left;
        txtDelayCompensationMs.Location = new Point(329, 9);
        txtDelayCompensationMs.Margin = new Padding(3, 9, 8, 3);
        txtDelayCompensationMs.MaxLength = 6;
        txtDelayCompensationMs.Name = "txtDelayCompensationMs";
        txtDelayCompensationMs.Size = new Size(60, 23);
        txtDelayCompensationMs.TabIndex = 3;
        txtDelayCompensationMs.Text = "0";
        txtDelayCompensationMs.TextAlign = HorizontalAlignment.Right;
        //
        // chkDebugMode
        //
        chkDebugMode.AutoSize = true;
        chkDebugMode.Dock = DockStyle.Left;
        chkDebugMode.Location = new Point(400, 9);
        chkDebugMode.Margin = new Padding(3, 9, 12, 3);
        chkDebugMode.Name = "chkDebugMode";
        chkDebugMode.Size = new Size(105, 22);
        chkDebugMode.TabIndex = 4;
        chkDebugMode.Text = "Debug 模式";
        chkDebugMode.UseVisualStyleBackColor = true;
        //
        // flowButtons
        //
        flowButtons.AutoSize = true;
        flowButtons.Controls.Add(btnStartRun);
        flowButtons.Controls.Add(btnStopRun);
        flowButtons.Dock = DockStyle.Fill;
        flowButtons.FlowDirection = FlowDirection.LeftToRight;
        flowButtons.Location = new Point(712, 3);
        flowButtons.Margin = new Padding(0);
        flowButtons.Name = "flowButtons";
        flowButtons.Padding = new Padding(0, 4, 0, 0);
        flowButtons.Size = new Size(224, 34);
        flowButtons.TabIndex = 5;
        flowButtons.WrapContents = false;
        tableCore.SetColumnSpan(flowButtons, 2);
        //
        // btnStartRun
        //
        btnStartRun.AutoSize = true;
        btnStartRun.Location = new Point(3, 4);
        btnStartRun.Margin = new Padding(0, 0, 10, 0);
        btnStartRun.Name = "btnStartRun";
        btnStartRun.Padding = new Padding(14, 4, 14, 4);
        btnStartRun.Size = new Size(94, 30);
        btnStartRun.TabIndex = 0;
        btnStartRun.Text = "开始运行";
        btnStartRun.UseVisualStyleBackColor = true;
        btnStartRun.Click += BtnStartRun_Click;
        //
        // btnStopRun
        //
        btnStopRun.AutoSize = true;
        btnStopRun.Enabled = false;
        btnStopRun.Location = new Point(107, 4);
        btnStopRun.Margin = new Padding(0, 0, 0, 0);
        btnStopRun.Name = "btnStopRun";
        btnStopRun.Padding = new Padding(14, 4, 14, 4);
        btnStopRun.Size = new Size(94, 30);
        btnStopRun.TabIndex = 1;
        btnStopRun.Text = "停止运行";
        btnStopRun.UseVisualStyleBackColor = true;
        btnStopRun.Click += BtnStopRun_Click;
        //
        // pnlLogHost — 承载日志框，配合 Anchor 四向拉伸
        //
        pnlLogHost.Controls.Add(txtDebugLog);
        pnlLogHost.Dock = DockStyle.Fill;
        pnlLogHost.Location = new Point(0, 306);
        pnlLogHost.Margin = new Padding(0);
        pnlLogHost.Name = "pnlLogHost";
        pnlLogHost.Size = new Size(976, 258);
        pnlLogHost.TabIndex = 2;
        //
        // txtDebugLog
        //
        txtDebugLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        txtDebugLog.BackColor = SystemColors.Window;
        txtDebugLog.Font = new Font("Consolas", 9F);
        txtDebugLog.Location = new Point(0, 0);
        txtDebugLog.Margin = new Padding(0);
        txtDebugLog.Multiline = true;
        txtDebugLog.Name = "txtDebugLog";
        txtDebugLog.ReadOnly = true;
        txtDebugLog.ScrollBars = ScrollBars.Both;
        txtDebugLog.Size = new Size(976, 258);
        txtDebugLog.TabIndex = 0;
        txtDebugLog.WordWrap = false;
        //
        // statusStripMain
        //
        statusStripMain.ImageScalingSize = new Size(20, 20);
        statusStripMain.Items.AddRange(new ToolStripItem[] { tsslStatus });
        statusStripMain.Location = new Point(0, 578);
        statusStripMain.Name = "statusStripMain";
        statusStripMain.Padding = new Padding(1, 0, 12, 0);
        statusStripMain.Size = new Size(1000, 22);
        statusStripMain.SizingGrip = false;
        statusStripMain.TabIndex = 1;
        statusStripMain.Text = "statusStrip1";
        //
        // tsslStatus
        //
        tsslStatus.Name = "tsslStatus";
        tsslStatus.Size = new Size(32, 17);
        tsslStatus.Text = "就绪";
        //
        // Form1
        //
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(1000, 600);
        Controls.Add(mainLayout);
        Controls.Add(statusStripMain);
        Font = new Font("Microsoft YaHei UI", 9F);
        MinimumSize = new Size(900, 520);
        Name = "Form1";
        Padding = new Padding(12, 12, 8, 0);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "EasyAEC — 零延迟声学回声消除";
        grpDeviceRouting.ResumeLayout(false);
        grpDeviceRouting.PerformLayout();
        tableDevices.ResumeLayout(false);
        tableDevices.PerformLayout();
        panelMeterIn.ResumeLayout(false);
        panelMeterIn.PerformLayout();
        panelMeterRef.ResumeLayout(false);
        panelMeterRef.PerformLayout();
        panelMeterOut.ResumeLayout(false);
        panelMeterOut.PerformLayout();
        grpCoreControl.ResumeLayout(false);
        grpCoreControl.PerformLayout();
        tableCore.ResumeLayout(false);
        tableCore.PerformLayout();
        flowButtons.ResumeLayout(false);
        flowButtons.PerformLayout();
        pnlLogHost.ResumeLayout(false);
        pnlLogHost.PerformLayout();
        statusStripMain.ResumeLayout(false);
        statusStripMain.PerformLayout();
        mainLayout.ResumeLayout(false);
        mainLayout.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    private TableLayoutPanel mainLayout;
    private GroupBox grpDeviceRouting;
    private TableLayoutPanel tableDevices;
    private Label lblListenInput;
    private ComboBox cboListenInput;
    private Label lblListenOutput;
    private ComboBox cboListenOutput;
    private Label lblAudioOutput;
    private ComboBox cboAudioOutput;
    private Button btnRefreshDevices;
    private Panel panelMeterIn;
    private Label lblMeterInTitle;
    private ProgressBar pbrMeterInput;
    private Panel panelMeterRef;
    private Label lblMeterRefTitle;
    private ProgressBar pbrMeterReference;
    private Panel panelMeterOut;
    private Label lblMeterOutTitle;
    private ProgressBar pbrMeterOutput;
    private GroupBox grpCoreControl;
    private TableLayoutPanel tableCore;
    private Label lblSuppression;
    private ComboBox cboSuppressionStrength;
    private Label lblDelayMs;
    private TextBox txtDelayCompensationMs;
    private CheckBox chkDebugMode;
    private FlowLayoutPanel flowButtons;
    private Button btnStartRun;
    private Button btnStopRun;
    private Panel pnlLogHost;
    private TextBox txtDebugLog;
    private StatusStrip statusStripMain;
    private ToolStripStatusLabel tsslStatus;

    #endregion
}
