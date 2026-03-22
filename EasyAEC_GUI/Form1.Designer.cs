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
        lblListenOutput = new Label();
        lblAudioOutput = new Label();
        cboListenInput = new ComboBox();
        cboListenOutput = new ComboBox();
        cboAudioOutput = new ComboBox();
        btnRefreshDevices = new Button();
        panelMeterIn = new Panel();
        pbrMeterInput = new ProgressBar();
        lblMeterInTitle = new Label();
        panelMeterRef = new Panel();
        pbrMeterReference = new ProgressBar();
        lblMeterRefTitle = new Label();
        panelMeterOut = new Panel();
        pbrMeterOutput = new ProgressBar();
        lblMeterOutTitle = new Label();
        grpCoreControl = new GroupBox();
        tableCore = new TableLayoutPanel();
        lblSuppression = new Label();
        cboSuppressionStrength = new ComboBox();
        lblDelayMs = new Label();
        chkDebugMode = new CheckBox();
        flowButtons = new FlowLayoutPanel();
        btnStartRun = new Button();
        btnStopRun = new Button();
        txtDelayCompensationMs = new TextBox();
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
        // grpDeviceRouting
        // 
        grpDeviceRouting.Controls.Add(tableDevices);
        grpDeviceRouting.Dock = DockStyle.Fill;
        grpDeviceRouting.Font = new Font("Microsoft YaHei UI", 9F);
        grpDeviceRouting.Location = new Point(0, 0);
        grpDeviceRouting.Margin = new Padding(0, 0, 0, 20);
        grpDeviceRouting.Name = "grpDeviceRouting";
        grpDeviceRouting.Padding = new Padding(28, 20, 28, 24);
        grpDeviceRouting.Size = new Size(1960, 400);
        grpDeviceRouting.TabIndex = 0;
        grpDeviceRouting.TabStop = false;
        grpDeviceRouting.Text = "设备路由";
        // 
        // tableDevices
        // 
        tableDevices.AutoSize = true;
        tableDevices.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        tableDevices.ColumnCount = 4;
        tableDevices.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33333F));
        tableDevices.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33333F));
        tableDevices.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33334F));
        tableDevices.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 288F));
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
        tableDevices.Location = new Point(28, 51);
        tableDevices.Margin = new Padding(6);
        tableDevices.Name = "tableDevices";
        tableDevices.RowCount = 3;
        tableDevices.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
        tableDevices.RowStyles.Add(new RowStyle(SizeType.Absolute, 76F));
        tableDevices.RowStyles.Add(new RowStyle(SizeType.Absolute, 108F));
        tableDevices.Size = new Size(1904, 240);
        tableDevices.TabIndex = 0;
        // 
        // lblListenInput
        // 
        lblListenInput.AutoSize = true;
        lblListenInput.Dock = DockStyle.Fill;
        lblListenInput.Location = new Point(6, 0);
        lblListenInput.Margin = new Padding(6, 0, 12, 8);
        lblListenInput.Name = "lblListenInput";
        lblListenInput.Size = new Size(520, 48);
        lblListenInput.TabIndex = 0;
        lblListenInput.Text = "监听输入";
        lblListenInput.TextAlign = ContentAlignment.BottomLeft;
        // 
        // lblListenOutput
        // 
        lblListenOutput.AutoSize = true;
        lblListenOutput.Dock = DockStyle.Fill;
        lblListenOutput.Location = new Point(544, 0);
        lblListenOutput.Margin = new Padding(6, 0, 12, 8);
        lblListenOutput.Name = "lblListenOutput";
        lblListenOutput.Size = new Size(520, 48);
        lblListenOutput.TabIndex = 2;
        lblListenOutput.Text = "监听输出";
        lblListenOutput.TextAlign = ContentAlignment.BottomLeft;
        // 
        // lblAudioOutput
        // 
        lblAudioOutput.AutoSize = true;
        lblAudioOutput.Dock = DockStyle.Fill;
        lblAudioOutput.Location = new Point(1082, 0);
        lblAudioOutput.Margin = new Padding(6, 0, 12, 8);
        lblAudioOutput.Name = "lblAudioOutput";
        lblAudioOutput.Size = new Size(520, 48);
        lblAudioOutput.TabIndex = 4;
        lblAudioOutput.Text = "音频输出";
        lblAudioOutput.TextAlign = ContentAlignment.BottomLeft;
        // 
        // cboListenInput
        // 
        cboListenInput.Dock = DockStyle.Fill;
        cboListenInput.DropDownStyle = ComboBoxStyle.DropDownList;
        cboListenInput.FormattingEnabled = true;
        cboListenInput.Location = new Point(6, 62);
        cboListenInput.Margin = new Padding(6, 6, 12, 6);
        cboListenInput.Name = "cboListenInput";
        cboListenInput.Size = new Size(520, 39);
        cboListenInput.TabIndex = 1;
        // 
        // cboListenOutput
        // 
        cboListenOutput.Dock = DockStyle.Fill;
        cboListenOutput.DropDownStyle = ComboBoxStyle.DropDownList;
        cboListenOutput.FormattingEnabled = true;
        cboListenOutput.Location = new Point(544, 62);
        cboListenOutput.Margin = new Padding(6, 6, 12, 6);
        cboListenOutput.Name = "cboListenOutput";
        cboListenOutput.Size = new Size(520, 39);
        cboListenOutput.TabIndex = 3;
        // 
        // cboAudioOutput
        // 
        cboAudioOutput.Dock = DockStyle.Fill;
        cboAudioOutput.DropDownStyle = ComboBoxStyle.DropDownList;
        cboAudioOutput.FormattingEnabled = true;
        cboAudioOutput.Location = new Point(1082, 62);
        cboAudioOutput.Margin = new Padding(6, 6, 12, 6);
        cboAudioOutput.Name = "cboAudioOutput";
        cboAudioOutput.Size = new Size(520, 39);
        cboAudioOutput.TabIndex = 5;
        // 
        // btnRefreshDevices
        // 
        btnRefreshDevices.Anchor = AnchorStyles.Right;
        btnRefreshDevices.AutoSize = true;
        btnRefreshDevices.Location = new Point(1628, 62);
        btnRefreshDevices.Margin = new Padding(12, 6, 4, 6);
        btnRefreshDevices.Name = "btnRefreshDevices";
        btnRefreshDevices.Padding = new Padding(16, 4, 16, 4);
        btnRefreshDevices.Size = new Size(272, 64);
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
        panelMeterIn.Location = new Point(6, 138);
        panelMeterIn.Margin = new Padding(6, 6, 12, 0);
        panelMeterIn.Name = "panelMeterIn";
        panelMeterIn.Size = new Size(520, 102);
        panelMeterIn.TabIndex = 7;
        // 
        // pbrMeterInput
        // 
        pbrMeterInput.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        pbrMeterInput.Location = new Point(0, 44);
        pbrMeterInput.Margin = new Padding(6);
        pbrMeterInput.Name = "pbrMeterInput";
        pbrMeterInput.Size = new Size(520, 36);
        pbrMeterInput.Style = ProgressBarStyle.Continuous;
        pbrMeterInput.TabIndex = 1;
        // 
        // lblMeterInTitle
        // 
        lblMeterInTitle.AutoSize = true;
        lblMeterInTitle.Location = new Point(0, 0);
        lblMeterInTitle.Margin = new Padding(6, 0, 6, 0);
        lblMeterInTitle.Name = "lblMeterInTitle";
        lblMeterInTitle.Size = new Size(158, 31);
        lblMeterInTitle.TabIndex = 0;
        lblMeterInTitle.Text = "监听输入电平";
        // 
        // panelMeterRef
        // 
        panelMeterRef.Controls.Add(pbrMeterReference);
        panelMeterRef.Controls.Add(lblMeterRefTitle);
        panelMeterRef.Dock = DockStyle.Fill;
        panelMeterRef.Location = new Point(544, 138);
        panelMeterRef.Margin = new Padding(6, 6, 12, 0);
        panelMeterRef.Name = "panelMeterRef";
        panelMeterRef.Size = new Size(520, 102);
        panelMeterRef.TabIndex = 8;
        // 
        // pbrMeterReference
        // 
        pbrMeterReference.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        pbrMeterReference.Location = new Point(0, 44);
        pbrMeterReference.Margin = new Padding(6);
        pbrMeterReference.Name = "pbrMeterReference";
        pbrMeterReference.Size = new Size(520, 36);
        pbrMeterReference.Style = ProgressBarStyle.Continuous;
        pbrMeterReference.TabIndex = 1;
        // 
        // lblMeterRefTitle
        // 
        lblMeterRefTitle.AutoSize = true;
        lblMeterRefTitle.Location = new Point(0, 0);
        lblMeterRefTitle.Margin = new Padding(6, 0, 6, 0);
        lblMeterRefTitle.Name = "lblMeterRefTitle";
        lblMeterRefTitle.Size = new Size(222, 31);
        lblMeterRefTitle.TabIndex = 0;
        lblMeterRefTitle.Text = "监听输出(参考)电平";
        // 
        // panelMeterOut
        // 
        panelMeterOut.Controls.Add(pbrMeterOutput);
        panelMeterOut.Controls.Add(lblMeterOutTitle);
        panelMeterOut.Dock = DockStyle.Fill;
        panelMeterOut.Location = new Point(1082, 138);
        panelMeterOut.Margin = new Padding(6, 6, 12, 0);
        panelMeterOut.Name = "panelMeterOut";
        panelMeterOut.Size = new Size(520, 102);
        panelMeterOut.TabIndex = 9;
        // 
        // pbrMeterOutput
        // 
        pbrMeterOutput.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        pbrMeterOutput.Location = new Point(0, 44);
        pbrMeterOutput.Margin = new Padding(6);
        pbrMeterOutput.Name = "pbrMeterOutput";
        pbrMeterOutput.Size = new Size(520, 36);
        pbrMeterOutput.Style = ProgressBarStyle.Continuous;
        pbrMeterOutput.TabIndex = 1;
        // 
        // lblMeterOutTitle
        // 
        lblMeterOutTitle.AutoSize = true;
        lblMeterOutTitle.Location = new Point(0, 0);
        lblMeterOutTitle.Margin = new Padding(6, 0, 6, 0);
        lblMeterOutTitle.Name = "lblMeterOutTitle";
        lblMeterOutTitle.Size = new Size(158, 31);
        lblMeterOutTitle.TabIndex = 0;
        lblMeterOutTitle.Text = "音频输出电平";
        // 
        // grpCoreControl
        // 
        grpCoreControl.Controls.Add(tableCore);
        grpCoreControl.Dock = DockStyle.Fill;
        grpCoreControl.Font = new Font("Microsoft YaHei UI", 9F);
        grpCoreControl.Location = new Point(0, 420);
        grpCoreControl.Margin = new Padding(0, 0, 0, 20);
        grpCoreControl.Name = "grpCoreControl";
        grpCoreControl.Padding = new Padding(28, 20, 28, 24);
        grpCoreControl.Size = new Size(1960, 172);
        grpCoreControl.TabIndex = 1;
        grpCoreControl.TabStop = false;
        grpCoreControl.Text = "核心控制";
        // 
        // tableCore
        // 
        tableCore.AutoSize = true;
        tableCore.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        tableCore.ColumnCount = 8;
        tableCore.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 176F));
        tableCore.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240F));
        tableCore.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 236F));
        tableCore.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 136F));
        tableCore.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
        tableCore.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tableCore.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 377F));
        tableCore.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 236F));
        tableCore.Controls.Add(lblSuppression, 0, 0);
        tableCore.Controls.Add(cboSuppressionStrength, 1, 0);
        tableCore.Controls.Add(lblDelayMs, 2, 0);
        tableCore.Controls.Add(chkDebugMode, 4, 0);
        tableCore.Controls.Add(flowButtons, 6, 0);
        tableCore.Controls.Add(txtDelayCompensationMs, 3, 0);
        tableCore.Dock = DockStyle.Top;
        tableCore.Location = new Point(28, 51);
        tableCore.Margin = new Padding(6);
        tableCore.Name = "tableCore";
        tableCore.RowCount = 1;
        tableCore.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));
        tableCore.Size = new Size(1904, 80);
        tableCore.TabIndex = 0;
        // 
        // lblSuppression
        // 
        lblSuppression.AutoSize = true;
        lblSuppression.Dock = DockStyle.Fill;
        lblSuppression.Location = new Point(6, 0);
        lblSuppression.Margin = new Padding(6, 0, 16, 0);
        lblSuppression.Name = "lblSuppression";
        lblSuppression.Size = new Size(154, 80);
        lblSuppression.TabIndex = 0;
        lblSuppression.Text = "压制强度";
        lblSuppression.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // cboSuppressionStrength
        // 
        cboSuppressionStrength.Dock = DockStyle.Fill;
        cboSuppressionStrength.DropDownStyle = ComboBoxStyle.DropDownList;
        cboSuppressionStrength.FormattingEnabled = true;
        cboSuppressionStrength.Location = new Point(182, 16);
        cboSuppressionStrength.Margin = new Padding(6, 16, 24, 6);
        cboSuppressionStrength.Name = "cboSuppressionStrength";
        cboSuppressionStrength.Size = new Size(210, 39);
        cboSuppressionStrength.TabIndex = 1;
        // 
        // lblDelayMs
        // 
        lblDelayMs.AutoSize = true;
        lblDelayMs.Dock = DockStyle.Fill;
        lblDelayMs.Location = new Point(422, 0);
        lblDelayMs.Margin = new Padding(6, 0, 12, 0);
        lblDelayMs.Name = "lblDelayMs";
        lblDelayMs.Size = new Size(218, 80);
        lblDelayMs.TabIndex = 2;
        lblDelayMs.Text = "延迟补偿(ms)";
        lblDelayMs.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // chkDebugMode
        // 
        chkDebugMode.AutoSize = true;
        tableCore.SetColumnSpan(chkDebugMode, 2);
        chkDebugMode.Dock = DockStyle.Left;
        chkDebugMode.Location = new Point(794, 18);
        chkDebugMode.Margin = new Padding(6, 18, 24, 6);
        chkDebugMode.Name = "chkDebugMode";
        chkDebugMode.Size = new Size(178, 56);
        chkDebugMode.TabIndex = 4;
        chkDebugMode.Text = "Debug 模式";
        chkDebugMode.UseVisualStyleBackColor = true;
        // 
        // flowButtons
        // 
        flowButtons.AutoSize = true;
        tableCore.SetColumnSpan(flowButtons, 2);
        flowButtons.Controls.Add(btnStartRun);
        flowButtons.Controls.Add(btnStopRun);
        flowButtons.Dock = DockStyle.Fill;
        flowButtons.Location = new Point(1291, 0);
        flowButtons.Margin = new Padding(0);
        flowButtons.Name = "flowButtons";
        flowButtons.Padding = new Padding(0, 8, 0, 0);
        flowButtons.Size = new Size(613, 80);
        flowButtons.TabIndex = 5;
        flowButtons.WrapContents = false;
        // 
        // btnStartRun
        // 
        btnStartRun.AutoSize = true;
        btnStartRun.Location = new Point(0, 8);
        btnStartRun.Margin = new Padding(0, 0, 20, 0);
        btnStartRun.Name = "btnStartRun";
        btnStartRun.Padding = new Padding(28, 8, 28, 8);
        btnStartRun.Size = new Size(296, 66);
        btnStartRun.TabIndex = 0;
        btnStartRun.Text = "开始运行";
        btnStartRun.UseVisualStyleBackColor = true;
        btnStartRun.Click += BtnStartRun_Click;
        // 
        // btnStopRun
        // 
        btnStopRun.AutoSize = true;
        btnStopRun.Enabled = false;
        btnStopRun.Location = new Point(316, 8);
        btnStopRun.Margin = new Padding(0);
        btnStopRun.Name = "btnStopRun";
        btnStopRun.Padding = new Padding(28, 8, 28, 8);
        btnStopRun.Size = new Size(296, 66);
        btnStopRun.TabIndex = 1;
        btnStopRun.Text = "停止运行";
        btnStopRun.UseVisualStyleBackColor = true;
        btnStopRun.Click += BtnStopRun_Click;
        // 
        // txtDelayCompensationMs
        // 
        txtDelayCompensationMs.Anchor = AnchorStyles.Left;
        txtDelayCompensationMs.Location = new Point(658, 27);
        txtDelayCompensationMs.Margin = new Padding(6, 18, 16, 6);
        txtDelayCompensationMs.MaxLength = 6;
        txtDelayCompensationMs.Name = "txtDelayCompensationMs";
        txtDelayCompensationMs.Size = new Size(110, 38);
        txtDelayCompensationMs.TabIndex = 3;
        txtDelayCompensationMs.Text = "0";
        txtDelayCompensationMs.TextAlign = HorizontalAlignment.Right;
        // 
        // pnlLogHost
        // 
        pnlLogHost.Controls.Add(txtDebugLog);
        pnlLogHost.Dock = DockStyle.Fill;
        pnlLogHost.Location = new Point(0, 612);
        pnlLogHost.Margin = new Padding(0);
        pnlLogHost.Name = "pnlLogHost";
        pnlLogHost.Size = new Size(1960, 515);
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
        txtDebugLog.Size = new Size(1956, 511);
        txtDebugLog.TabIndex = 0;
        txtDebugLog.WordWrap = false;
        // 
        // statusStripMain
        // 
        statusStripMain.ImageScalingSize = new Size(20, 20);
        statusStripMain.Items.AddRange(new ToolStripItem[] { tsslStatus });
        statusStripMain.Location = new Point(24, 1159);
        statusStripMain.Name = "statusStripMain";
        statusStripMain.Padding = new Padding(2, 0, 24, 0);
        statusStripMain.Size = new Size(1960, 41);
        statusStripMain.SizingGrip = false;
        statusStripMain.TabIndex = 1;
        statusStripMain.Text = "statusStrip1";
        // 
        // tsslStatus
        // 
        tsslStatus.Name = "tsslStatus";
        tsslStatus.Size = new Size(62, 31);
        tsslStatus.Text = "就绪";
        // 
        // mainLayout
        // 
        mainLayout.ColumnCount = 1;
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        mainLayout.Controls.Add(grpDeviceRouting, 0, 0);
        mainLayout.Controls.Add(grpCoreControl, 0, 1);
        mainLayout.Controls.Add(pnlLogHost, 0, 2);
        mainLayout.Dock = DockStyle.Fill;
        mainLayout.Location = new Point(24, 24);
        mainLayout.Margin = new Padding(0);
        mainLayout.Name = "mainLayout";
        mainLayout.Padding = new Padding(0, 0, 0, 8);
        mainLayout.RowCount = 3;
        mainLayout.RowStyles.Add(new RowStyle());
        mainLayout.RowStyles.Add(new RowStyle());
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        mainLayout.Size = new Size(1960, 1135);
        mainLayout.TabIndex = 0;
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(192F, 192F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(2000, 1200);
        Controls.Add(mainLayout);
        Controls.Add(statusStripMain);
        Font = new Font("Microsoft YaHei UI", 9F);
        Margin = new Padding(6);
        MinimumSize = new Size(1774, 969);
        Name = "Form1";
        Padding = new Padding(24, 24, 16, 0);
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
