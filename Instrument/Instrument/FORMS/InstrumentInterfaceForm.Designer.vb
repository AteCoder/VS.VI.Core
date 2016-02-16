﻿<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class InstrumentInterfaceForm

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer
    Private WithEvents _CommandsComboBoxLabel As System.Windows.Forms.Label
    Private WithEvents _TspTipLabel As System.Windows.Forms.Label
    Private WithEvents _SendButton As System.Windows.Forms.Button
    Private WithEvents _ReceiveButton As System.Windows.Forms.Button
    Private WithEvents _ReadStatusRegisterButton As System.Windows.Forms.Button
    Private WithEvents _CommandsComboBox As System.Windows.Forms.ComboBox
    Private WithEvents _SendComboCommandButton As System.Windows.Forms.Button
    Private WithEvents _SendReceiveTabPage As System.Windows.Forms.TabPage
    Private WithEvents _TraceMessagesBox As isr.Core.Pith.TraceMessagesBox
    Private WithEvents _messagesTabPage As System.Windows.Forms.TabPage
    Private WithEvents _Tabs As System.Windows.Forms.TabControl
    Private WithEvents _StatusLabel As System.Windows.Forms.ToolStripStatusLabel
    Private WithEvents _statusBar As System.Windows.Forms.StatusStrip
    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(InstrumentInterfaceForm))
        Me._ToolTip = New System.Windows.Forms.ToolTip(Me.components)
        Me._SendButton = New System.Windows.Forms.Button()
        Me._ReceiveButton = New System.Windows.Forms.Button()
        Me._ReadStatusRegisterButton = New System.Windows.Forms.Button()
        Me._SendComboCommandButton = New System.Windows.Forms.Button()
        Me._PollRadioButton = New System.Windows.Forms.RadioButton()
        Me._ServiceRequestReceiveOptionRadioButton = New System.Windows.Forms.RadioButton()
        Me._DisconnectCommandsTextBox = New System.Windows.Forms.TextBox()
        Me._Tabs = New System.Windows.Forms.TabControl()
        Me._connectTabPage = New System.Windows.Forms.TabPage()
        Me._ConnectTabLayout = New System.Windows.Forms.TableLayoutPanel()
        Me._connectGroupBox = New System.Windows.Forms.GroupBox()
        Me._InterfacePanel = New isr.VI.Instrument.InterfacePanel()
        Me._settingsGroupBox = New System.Windows.Forms.GroupBox()
        Me._SendDisconnectCommandsCheckBox = New System.Windows.Forms.CheckBox()
        Me._SendReceiveTabPage = New System.Windows.Forms.TabPage()
        Me._sendReceiveLayout = New System.Windows.Forms.TableLayoutPanel()
        Me._sendReceiveControlPanel = New System.Windows.Forms.Panel()
        Me._ReceiveOptionsGroupBox = New System.Windows.Forms.GroupBox()
        Me._PollIntervalUnitsLabel = New System.Windows.Forms.Label()
        Me._PollIntervalNumericUpDown = New System.Windows.Forms.NumericUpDown()
        Me._ReadManualRadioButton = New System.Windows.Forms.RadioButton()
        Me._CommandsComboBox = New System.Windows.Forms.ComboBox()
        Me._CommandsComboBoxLabel = New System.Windows.Forms.Label()
        Me._TspTipLabel = New System.Windows.Forms.Label()
        Me._sendReceiveSplitContainer = New System.Windows.Forms.SplitContainer()
        Me._SendGroupBox = New System.Windows.Forms.GroupBox()
        Me._InputTextBox = New System.Windows.Forms.TextBox()
        Me._ReceiveGroupBox = New System.Windows.Forms.GroupBox()
        Me._OutputTextBox = New System.Windows.Forms.TextBox()
        Me._messagesTabPage = New System.Windows.Forms.TabPage()
        Me._TraceMessagesBox = New isr.Core.Pith.TraceMessagesBox()
        Me._statusBar = New System.Windows.Forms.StatusStrip()
        Me._StatusLabel = New System.Windows.Forms.ToolStripStatusLabel()
        Me._StatusRegisterLabel = New System.Windows.Forms.ToolStripStatusLabel()
        Me._SreCommandLabel = New System.Windows.Forms.Label()
        Me._SrqBitsNumeric = New System.Windows.Forms.NumericUpDown()
        Me._SrqBitsLabel = New System.Windows.Forms.Label()
        Me._SreCommandComboBox = New System.Windows.Forms.ComboBox()
        Me._MessageAvailableBitsLabel = New System.Windows.Forms.Label()
        Me._MessageAvailableBitsNumeric = New System.Windows.Forms.NumericUpDown()
        Me._Tabs.SuspendLayout()
        Me._connectTabPage.SuspendLayout()
        Me._ConnectTabLayout.SuspendLayout()
        Me._connectGroupBox.SuspendLayout()
        Me._settingsGroupBox.SuspendLayout()
        Me._SendReceiveTabPage.SuspendLayout()
        Me._sendReceiveLayout.SuspendLayout()
        Me._sendReceiveControlPanel.SuspendLayout()
        Me._ReceiveOptionsGroupBox.SuspendLayout()
        CType(Me._PollIntervalNumericUpDown, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me._sendReceiveSplitContainer, System.ComponentModel.ISupportInitialize).BeginInit()
        Me._sendReceiveSplitContainer.Panel1.SuspendLayout()
        Me._sendReceiveSplitContainer.Panel2.SuspendLayout()
        Me._sendReceiveSplitContainer.SuspendLayout()
        Me._SendGroupBox.SuspendLayout()
        Me._ReceiveGroupBox.SuspendLayout()
        Me._messagesTabPage.SuspendLayout()
        Me._statusBar.SuspendLayout()
        CType(Me._SrqBitsNumeric, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me._MessageAvailableBitsNumeric, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        '_ToolTip
        '
        Me._ToolTip.IsBalloon = True
        '
        '_SendButton
        '
        Me._SendButton.BackColor = System.Drawing.SystemColors.Control
        Me._SendButton.Cursor = System.Windows.Forms.Cursors.Default
        Me._SendButton.Enabled = False
        Me._SendButton.Font = New System.Drawing.Font(Me.Font, System.Drawing.FontStyle.Bold)
        Me._SendButton.ForeColor = System.Drawing.SystemColors.ControlText
        Me._SendButton.Location = New System.Drawing.Point(13, 12)
        Me._SendButton.Name = "_SendButton"
        Me._SendButton.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me._SendButton.Size = New System.Drawing.Size(103, 35)
        Me._SendButton.TabIndex = 0
        Me._SendButton.Text = "&SEND"
        Me._ToolTip.SetToolTip(Me._SendButton, "Sends")
        Me._SendButton.UseVisualStyleBackColor = True
        '
        '_ReceiveButton
        '
        Me._ReceiveButton.BackColor = System.Drawing.SystemColors.Control
        Me._ReceiveButton.Cursor = System.Windows.Forms.Cursors.Default
        Me._ReceiveButton.Enabled = False
        Me._ReceiveButton.Font = New System.Drawing.Font(Me.Font, System.Drawing.FontStyle.Bold)
        Me._ReceiveButton.ForeColor = System.Drawing.SystemColors.ControlText
        Me._ReceiveButton.Location = New System.Drawing.Point(122, 12)
        Me._ReceiveButton.Name = "_ReceiveButton"
        Me._ReceiveButton.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me._ReceiveButton.Size = New System.Drawing.Size(80, 35)
        Me._ReceiveButton.TabIndex = 1
        Me._ReceiveButton.Text = "&RECEIVE"
        Me._ToolTip.SetToolTip(Me._ReceiveButton, "Receives data.  Enabled after reading SRQ.")
        Me._ReceiveButton.UseVisualStyleBackColor = True
        '
        '_ReadStatusRegisterButton
        '
        Me._ReadStatusRegisterButton.BackColor = System.Drawing.SystemColors.Control
        Me._ReadStatusRegisterButton.Cursor = System.Windows.Forms.Cursors.Default
        Me._ReadStatusRegisterButton.Enabled = False
        Me._ReadStatusRegisterButton.Font = New System.Drawing.Font(Me.Font, System.Drawing.FontStyle.Bold)
        Me._ReadStatusRegisterButton.ForeColor = System.Drawing.SystemColors.ControlText
        Me._ReadStatusRegisterButton.Location = New System.Drawing.Point(213, 12)
        Me._ReadStatusRegisterButton.Name = "_ReadStatusRegisterButton"
        Me._ReadStatusRegisterButton.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me._ReadStatusRegisterButton.Size = New System.Drawing.Size(190, 35)
        Me._ReadStatusRegisterButton.TabIndex = 2
        Me._ReadStatusRegisterButton.Text = "READ SR&Q (enable receive)"
        Me._ToolTip.SetToolTip(Me._ReadStatusRegisterButton, "Reads SRQ and enables receive")
        Me._ReadStatusRegisterButton.UseVisualStyleBackColor = True
        '
        '_SendComboCommandButton
        '
        Me._SendComboCommandButton.BackColor = System.Drawing.SystemColors.Control
        Me._SendComboCommandButton.Cursor = System.Windows.Forms.Cursors.Default
        Me._SendComboCommandButton.Enabled = False
        Me._SendComboCommandButton.Font = New System.Drawing.Font(Me.Font, System.Drawing.FontStyle.Bold)
        Me._SendComboCommandButton.ForeColor = System.Drawing.SystemColors.ControlText
        Me._SendComboCommandButton.Location = New System.Drawing.Point(287, 71)
        Me._SendComboCommandButton.Name = "_SendComboCommandButton"
        Me._SendComboCommandButton.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me._SendComboCommandButton.Size = New System.Drawing.Size(132, 35)
        Me._SendComboCommandButton.TabIndex = 5
        Me._SendComboCommandButton.Text = "&SEND COMMAND"
        Me._ToolTip.SetToolTip(Me._SendComboCommandButton, "Sends a single command")
        Me._SendComboCommandButton.UseVisualStyleBackColor = True
        '
        '_PollRadioButton
        '
        Me._PollRadioButton.AutoSize = True
        Me._PollRadioButton.Font = New System.Drawing.Font(Me.Font, System.Drawing.FontStyle.Bold)
        Me._PollRadioButton.Location = New System.Drawing.Point(14, 50)
        Me._PollRadioButton.Name = "_PollRadioButton"
        Me._PollRadioButton.Size = New System.Drawing.Size(58, 21)
        Me._PollRadioButton.TabIndex = 1
        Me._PollRadioButton.Text = "POLL"
        Me._ToolTip.SetToolTip(Me._PollRadioButton, "Uses a timer to poll the service request register and receive a buffer whenever t" &
        "he message available bit is set.")
        Me._PollRadioButton.UseVisualStyleBackColor = True
        '
        '_ServiceRequestReceiveOptionRadioButton
        '
        Me._ServiceRequestReceiveOptionRadioButton.AutoSize = True
        Me._ServiceRequestReceiveOptionRadioButton.Font = New System.Drawing.Font(Me.Font, System.Drawing.FontStyle.Bold)
        Me._ServiceRequestReceiveOptionRadioButton.Location = New System.Drawing.Point(14, 75)
        Me._ServiceRequestReceiveOptionRadioButton.Name = "_ServiceRequestReceiveOptionRadioButton"
        Me._ServiceRequestReceiveOptionRadioButton.Size = New System.Drawing.Size(67, 21)
        Me._ServiceRequestReceiveOptionRadioButton.TabIndex = 3
        Me._ServiceRequestReceiveOptionRadioButton.Text = "EVENT"
        Me._ToolTip.SetToolTip(Me._ServiceRequestReceiveOptionRadioButton, "Use the service request event on measurement available to receive the buffer.")
        Me._ServiceRequestReceiveOptionRadioButton.UseVisualStyleBackColor = True
        '
        '_DisconnectCommandsTextBox
        '
        Me._DisconnectCommandsTextBox.AcceptsReturn = True
        Me._DisconnectCommandsTextBox.BackColor = System.Drawing.SystemColors.Window
        Me._DisconnectCommandsTextBox.Cursor = System.Windows.Forms.Cursors.IBeam
        Me._DisconnectCommandsTextBox.Font = New System.Drawing.Font(Me.Font, System.Drawing.FontStyle.Bold)
        Me._DisconnectCommandsTextBox.ForeColor = System.Drawing.SystemColors.WindowText
        Me._DisconnectCommandsTextBox.Location = New System.Drawing.Point(21, 57)
        Me._DisconnectCommandsTextBox.MaxLength = 0
        Me._DisconnectCommandsTextBox.Multiline = True
        Me._DisconnectCommandsTextBox.Name = "_DisconnectCommandsTextBox"
        Me._DisconnectCommandsTextBox.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me._DisconnectCommandsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me._DisconnectCommandsTextBox.Size = New System.Drawing.Size(294, 277)
        Me._DisconnectCommandsTextBox.TabIndex = 28
        Me._DisconnectCommandsTextBox.Text = "localnode.prompts = 0"
        Me._ToolTip.SetToolTip(Me._DisconnectCommandsTextBox, "Lists the commands to send to the instrument when closing the session.")
        '
        '_Tabs
        '
        Me._Tabs.Controls.Add(Me._connectTabPage)
        Me._Tabs.Controls.Add(Me._SendReceiveTabPage)
        Me._Tabs.Controls.Add(Me._messagesTabPage)
        Me._Tabs.Dock = System.Windows.Forms.DockStyle.Fill
        Me._Tabs.ItemSize = New System.Drawing.Size(42, 22)
        Me._Tabs.Location = New System.Drawing.Point(0, 0)
        Me._Tabs.Name = "_Tabs"
        Me._Tabs.SelectedIndex = 0
        Me._Tabs.Size = New System.Drawing.Size(875, 538)
        Me._Tabs.TabIndex = 1

        '
        '_connectTabPage
        '
        Me._connectTabPage.Controls.Add(Me._ConnectTabLayout)
        Me._connectTabPage.Location = New System.Drawing.Point(4, 22)
        Me._connectTabPage.Name = "_connectTabPage"
        Me._connectTabPage.Padding = New System.Windows.Forms.Padding(3)
        Me._connectTabPage.Size = New System.Drawing.Size(867, 512)
        Me._connectTabPage.TabIndex = 3
        Me._connectTabPage.Text = "CONNECT"
        Me._connectTabPage.UseVisualStyleBackColor = True
        '
        '_ConnectTabLayout
        '
        Me._ConnectTabLayout.ColumnCount = 4
        Me._ConnectTabLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me._ConnectTabLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        Me._ConnectTabLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        Me._ConnectTabLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me._ConnectTabLayout.Controls.Add(Me._connectGroupBox, 1, 1)
        Me._ConnectTabLayout.Controls.Add(Me._settingsGroupBox, 2, 1)
        Me._ConnectTabLayout.Dock = System.Windows.Forms.DockStyle.Fill
        Me._ConnectTabLayout.Location = New System.Drawing.Point(3, 3)
        Me._ConnectTabLayout.Name = "_ConnectTabLayout"
        Me._ConnectTabLayout.RowCount = 3
        Me._ConnectTabLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me._ConnectTabLayout.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me._ConnectTabLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me._ConnectTabLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24.0!))
        Me._ConnectTabLayout.Size = New System.Drawing.Size(861, 506)
        Me._ConnectTabLayout.TabIndex = 26
        '
        '_connectGroupBox
        '
        Me._connectGroupBox.Controls.Add(Me._InterfacePanel)
        Me._connectGroupBox.Location = New System.Drawing.Point(56, 74)
        Me._connectGroupBox.Name = "_connectGroupBox"
        Me._connectGroupBox.Size = New System.Drawing.Size(408, 357)
        Me._connectGroupBox.TabIndex = 27
        Me._connectGroupBox.TabStop = False
        Me._connectGroupBox.Text = "CONNECT:"
        '
        '_InterfacePanel
        '
        Me._InterfacePanel.BackColor = System.Drawing.Color.Transparent
        Me._InterfacePanel.Enabled = False
        Me._InterfacePanel.Font = New System.Drawing.Font(Me.Font, System.Drawing.FontStyle.Bold)
        Me._InterfacePanel.Location = New System.Drawing.Point(13, 23)
        Me._InterfacePanel.Name = "_InterfacePanel"
        Me._InterfacePanel.Size = New System.Drawing.Size(368, 311)
        Me._InterfacePanel.TabIndex = 0
        '
        '_settingsGroupBox
        '
        Me._settingsGroupBox.Controls.Add(Me._DisconnectCommandsTextBox)
        Me._settingsGroupBox.Controls.Add(Me._SendDisconnectCommandsCheckBox)
        Me._settingsGroupBox.Location = New System.Drawing.Point(470, 74)
        Me._settingsGroupBox.Name = "_settingsGroupBox"
        Me._settingsGroupBox.Size = New System.Drawing.Size(335, 355)
        Me._settingsGroupBox.TabIndex = 29
        Me._settingsGroupBox.TabStop = False
        Me._settingsGroupBox.Text = "SETTINGS: "
        '
        '_SendDisconnectCommandsCheckBox
        '
        Me._SendDisconnectCommandsCheckBox.AutoSize = True
        Me._SendDisconnectCommandsCheckBox.BackColor = System.Drawing.Color.Transparent
        Me._SendDisconnectCommandsCheckBox.Cursor = System.Windows.Forms.Cursors.Default
        Me._SendDisconnectCommandsCheckBox.Font = New System.Drawing.Font(Me.Font, System.Drawing.FontStyle.Bold)
        Me._SendDisconnectCommandsCheckBox.ForeColor = System.Drawing.SystemColors.ControlText
        Me._SendDisconnectCommandsCheckBox.Location = New System.Drawing.Point(21, 28)
        Me._SendDisconnectCommandsCheckBox.Name = "_SendDisconnectCommandsCheckBox"
        Me._SendDisconnectCommandsCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me._SendDisconnectCommandsCheckBox.Size = New System.Drawing.Size(228, 21)
        Me._SendDisconnectCommandsCheckBox.TabIndex = 27
        Me._SendDisconnectCommandsCheckBox.Text = "Send Commands On Disconnect:"
        Me._SendDisconnectCommandsCheckBox.UseVisualStyleBackColor = False
        '
        '_SendReceiveTabPage
        '
        Me._SendReceiveTabPage.Controls.Add(Me._sendReceiveLayout)
        Me._SendReceiveTabPage.Location = New System.Drawing.Point(4, 22)
        Me._SendReceiveTabPage.Name = "_SendReceiveTabPage"
        Me._SendReceiveTabPage.Size = New System.Drawing.Size(867, 512)
        Me._SendReceiveTabPage.TabIndex = 0
        Me._SendReceiveTabPage.Text = "SEND / RECEIVE"
        Me._SendReceiveTabPage.UseVisualStyleBackColor = True
        '
        '_sendReceiveLayout
        '
        Me._sendReceiveLayout.ColumnCount = 3
        Me._sendReceiveLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 12.0!))
        Me._sendReceiveLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me._sendReceiveLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 12.0!))
        Me._sendReceiveLayout.Controls.Add(Me._sendReceiveControlPanel, 1, 2)
        Me._sendReceiveLayout.Controls.Add(Me._sendReceiveSplitContainer, 1, 1)
        Me._sendReceiveLayout.Dock = System.Windows.Forms.DockStyle.Fill
        Me._sendReceiveLayout.Location = New System.Drawing.Point(0, 0)
        Me._sendReceiveLayout.Name = "_sendReceiveLayout"
        Me._sendReceiveLayout.RowCount = 4
        Me._sendReceiveLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 5.0!))
        Me._sendReceiveLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me._sendReceiveLayout.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me._sendReceiveLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 5.0!))
        Me._sendReceiveLayout.Size = New System.Drawing.Size(867, 512)
        Me._sendReceiveLayout.TabIndex = 0
        '
        '_sendReceiveControlPanel
        '
        Me._sendReceiveControlPanel.Controls.Add(Me._SreCommandComboBox)
        Me._sendReceiveControlPanel.Controls.Add(Me._MessageAvailableBitsNumeric)
        Me._sendReceiveControlPanel.Controls.Add(Me._MessageAvailableBitsLabel)
        Me._sendReceiveControlPanel.Controls.Add(Me._SrqBitsNumeric)
        Me._sendReceiveControlPanel.Controls.Add(Me._SrqBitsLabel)
        Me._sendReceiveControlPanel.Controls.Add(Me._SendButton)
        Me._sendReceiveControlPanel.Controls.Add(Me._SendComboCommandButton)
        Me._sendReceiveControlPanel.Controls.Add(Me._ReceiveOptionsGroupBox)
        Me._sendReceiveControlPanel.Controls.Add(Me._SreCommandLabel)
        Me._sendReceiveControlPanel.Controls.Add(Me._CommandsComboBox)
        Me._sendReceiveControlPanel.Controls.Add(Me._CommandsComboBoxLabel)
        Me._sendReceiveControlPanel.Controls.Add(Me._ReadStatusRegisterButton)
        Me._sendReceiveControlPanel.Controls.Add(Me._TspTipLabel)
        Me._sendReceiveControlPanel.Controls.Add(Me._ReceiveButton)
        Me._sendReceiveControlPanel.Dock = System.Windows.Forms.DockStyle.Top
        Me._sendReceiveControlPanel.Enabled = False
        Me._sendReceiveControlPanel.Location = New System.Drawing.Point(15, 378)
        Me._sendReceiveControlPanel.Name = "_sendReceiveControlPanel"
        Me._sendReceiveControlPanel.Size = New System.Drawing.Size(837, 126)
        Me._sendReceiveControlPanel.TabIndex = 0
        '
        '_ReceiveOptionsGroupBox
        '
        Me._ReceiveOptionsGroupBox.Controls.Add(Me._PollIntervalUnitsLabel)
        Me._ReceiveOptionsGroupBox.Controls.Add(Me._PollIntervalNumericUpDown)
        Me._ReceiveOptionsGroupBox.Controls.Add(Me._ServiceRequestReceiveOptionRadioButton)
        Me._ReceiveOptionsGroupBox.Controls.Add(Me._PollRadioButton)
        Me._ReceiveOptionsGroupBox.Controls.Add(Me._ReadManualRadioButton)
        Me._ReceiveOptionsGroupBox.Location = New System.Drawing.Point(668, 11)
        Me._ReceiveOptionsGroupBox.Name = "_ReceiveOptionsGroupBox"
        Me._ReceiveOptionsGroupBox.Size = New System.Drawing.Size(166, 105)
        Me._ReceiveOptionsGroupBox.TabIndex = 12
        Me._ReceiveOptionsGroupBox.TabStop = False
        Me._ReceiveOptionsGroupBox.Text = "RECEIVE OPTIONS:"
        '
        '_PollIntervalUnitsLabel
        '
        Me._PollIntervalUnitsLabel.AutoSize = True
        Me._PollIntervalUnitsLabel.Location = New System.Drawing.Point(140, 53)
        Me._PollIntervalUnitsLabel.Name = "_PollIntervalUnitsLabel"
        Me._PollIntervalUnitsLabel.Size = New System.Drawing.Size(25, 17)
        Me._PollIntervalUnitsLabel.TabIndex = 4
        Me._PollIntervalUnitsLabel.Text = "ms"
        '
        '_PollIntervalNumericUpDown
        '
        Me._PollIntervalNumericUpDown.Font = New System.Drawing.Font(Me.Font, System.Drawing.FontStyle.Bold)
        Me._PollIntervalNumericUpDown.Location = New System.Drawing.Point(87, 49)
        Me._PollIntervalNumericUpDown.Maximum = New Decimal(New Integer() {999, 0, 0, 0})
        Me._PollIntervalNumericUpDown.Minimum = New Decimal(New Integer() {10, 0, 0, 0})
        Me._PollIntervalNumericUpDown.Name = "_PollIntervalNumericUpDown"
        Me._PollIntervalNumericUpDown.Size = New System.Drawing.Size(50, 25)
        Me._PollIntervalNumericUpDown.TabIndex = 2
        Me._PollIntervalNumericUpDown.Value = New Decimal(New Integer() {500, 0, 0, 0})
        '
        '_ReadManualRadioButton
        '
        Me._ReadManualRadioButton.AutoSize = True
        Me._ReadManualRadioButton.Checked = True
        Me._ReadManualRadioButton.Font = New System.Drawing.Font(Me.Font, System.Drawing.FontStyle.Bold)
        Me._ReadManualRadioButton.Location = New System.Drawing.Point(14, 24)
        Me._ReadManualRadioButton.Name = "_ReadManualRadioButton"
        Me._ReadManualRadioButton.Size = New System.Drawing.Size(82, 21)
        Me._ReadManualRadioButton.TabIndex = 0
        Me._ReadManualRadioButton.TabStop = True
        Me._ReadManualRadioButton.Text = "MANUAL"
        Me._ReadManualRadioButton.UseVisualStyleBackColor = True
        '
        '_CommandsComboBox
        '
        Me._CommandsComboBox.BackColor = System.Drawing.SystemColors.Window
        Me._CommandsComboBox.Cursor = System.Windows.Forms.Cursors.Default
        Me._CommandsComboBox.Font = New System.Drawing.Font(Me.Font, System.Drawing.FontStyle.Bold)
        Me._CommandsComboBox.ForeColor = System.Drawing.SystemColors.WindowText
        Me._CommandsComboBox.Items.AddRange(New Object() {"print( Me._VERSION )", "localnode.prompts = 1", "localnode.errorqueue.clear()", "localnode.reset()", "*RST", "*CLS", "*IDN?", "M63x", "U0x", "U1x"})
        Me._CommandsComboBox.Location = New System.Drawing.Point(15, 76)
        Me._CommandsComboBox.Name = "_CommandsComboBox"
        Me._CommandsComboBox.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me._CommandsComboBox.Size = New System.Drawing.Size(266, 25)
        Me._CommandsComboBox.TabIndex = 4
        Me._CommandsComboBox.Text = "Combo1"
        '
        '_CommandsComboBoxLabel
        '
        Me._CommandsComboBoxLabel.BackColor = System.Drawing.Color.Transparent
        Me._CommandsComboBoxLabel.Cursor = System.Windows.Forms.Cursors.Default
        Me._CommandsComboBoxLabel.ForeColor = System.Drawing.SystemColors.ControlText
        Me._CommandsComboBoxLabel.Location = New System.Drawing.Point(15, 59)
        Me._CommandsComboBoxLabel.Name = "_CommandsComboBoxLabel"
        Me._CommandsComboBoxLabel.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me._CommandsComboBoxLabel.Size = New System.Drawing.Size(160, 16)
        Me._CommandsComboBoxLabel.TabIndex = 3
        Me._CommandsComboBoxLabel.Text = "SINGLE COMMANDS: "
        '
        '_TspTipLabel
        '
        Me._TspTipLabel.AutoSize = True
        Me._TspTipLabel.BackColor = System.Drawing.Color.Transparent
        Me._TspTipLabel.Cursor = System.Windows.Forms.Cursors.Default
        Me._TspTipLabel.ForeColor = System.Drawing.SystemColors.ControlText
        Me._TspTipLabel.Location = New System.Drawing.Point(18, 104)
        Me._TspTipLabel.Name = "_TspTipLabel"
        Me._TspTipLabel.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me._TspTipLabel.Size = New System.Drawing.Size(375, 17)
        Me._TspTipLabel.TabIndex = 21
        Me._TspTipLabel.Text = "Receiving the prompt after a print() or *IDN causes error 420.  "
        '
        '_sendReceiveSplitContainer
        '
        Me._sendReceiveSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill
        Me._sendReceiveSplitContainer.Location = New System.Drawing.Point(15, 8)
        Me._sendReceiveSplitContainer.Name = "_sendReceiveSplitContainer"
        '
        '_sendReceiveSplitContainer.Panel1
        '
        Me._sendReceiveSplitContainer.Panel1.Controls.Add(Me._SendGroupBox)
        '
        '_sendReceiveSplitContainer.Panel2
        '
        Me._sendReceiveSplitContainer.Panel2.Controls.Add(Me._ReceiveGroupBox)
        Me._sendReceiveSplitContainer.Size = New System.Drawing.Size(837, 364)
        Me._sendReceiveSplitContainer.SplitterDistance = 278
        Me._sendReceiveSplitContainer.SplitterWidth = 5
        Me._sendReceiveSplitContainer.TabIndex = 26
        '
        '_SendGroupBox
        '
        Me._SendGroupBox.Controls.Add(Me._InputTextBox)
        Me._SendGroupBox.Dock = System.Windows.Forms.DockStyle.Fill
        Me._SendGroupBox.Location = New System.Drawing.Point(0, 0)
        Me._SendGroupBox.Name = "_SendGroupBox"
        Me._SendGroupBox.Size = New System.Drawing.Size(278, 364)
        Me._SendGroupBox.TabIndex = 0
        Me._SendGroupBox.TabStop = False
        Me._SendGroupBox.Text = "SENT TO INSTRUMENT"
        '
        '_InputTextBox
        '
        Me._InputTextBox.AcceptsReturn = True
        Me._InputTextBox.BackColor = System.Drawing.SystemColors.Window
        Me._InputTextBox.Cursor = System.Windows.Forms.Cursors.IBeam
        Me._InputTextBox.Dock = System.Windows.Forms.DockStyle.Fill
        Me._InputTextBox.Font = New System.Drawing.Font(Me.Font, System.Drawing.FontStyle.Bold)
        Me._InputTextBox.ForeColor = System.Drawing.SystemColors.WindowText
        Me._InputTextBox.Location = New System.Drawing.Point(3, 21)
        Me._InputTextBox.MaxLength = 0
        Me._InputTextBox.Multiline = True
        Me._InputTextBox.Name = "_InputTextBox"
        Me._InputTextBox.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me._InputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me._InputTextBox.Size = New System.Drawing.Size(272, 340)
        Me._InputTextBox.TabIndex = 0
        '
        '_ReceiveGroupBox
        '
        Me._ReceiveGroupBox.Controls.Add(Me._OutputTextBox)
        Me._ReceiveGroupBox.Dock = System.Windows.Forms.DockStyle.Fill
        Me._ReceiveGroupBox.Location = New System.Drawing.Point(0, 0)
        Me._ReceiveGroupBox.Name = "_ReceiveGroupBox"
        Me._ReceiveGroupBox.Size = New System.Drawing.Size(554, 364)
        Me._ReceiveGroupBox.TabIndex = 0
        Me._ReceiveGroupBox.TabStop = False
        Me._ReceiveGroupBox.Text = "RECEIVED FROM INSTRUMENT: "
        '
        '_OutputTextBox
        '
        Me._OutputTextBox.AcceptsReturn = True
        Me._OutputTextBox.BackColor = System.Drawing.SystemColors.Window
        Me._OutputTextBox.Cursor = System.Windows.Forms.Cursors.IBeam
        Me._OutputTextBox.Dock = System.Windows.Forms.DockStyle.Fill
        Me._OutputTextBox.Font = New System.Drawing.Font(Me.Font, System.Drawing.FontStyle.Bold)
        Me._OutputTextBox.ForeColor = System.Drawing.SystemColors.WindowText
        Me._OutputTextBox.Location = New System.Drawing.Point(3, 21)
        Me._OutputTextBox.MaxLength = 0
        Me._OutputTextBox.Multiline = True
        Me._OutputTextBox.Name = "_OutputTextBox"
        Me._OutputTextBox.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me._OutputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me._OutputTextBox.Size = New System.Drawing.Size(548, 340)
        Me._OutputTextBox.TabIndex = 0
        '
        '_messagesTabPage
        '
        Me._messagesTabPage.Controls.Add(Me._TraceMessagesBox)
        Me._messagesTabPage.Location = New System.Drawing.Point(4, 22)
        Me._messagesTabPage.Name = "_messagesTabPage"
        Me._messagesTabPage.Size = New System.Drawing.Size(867, 512)
        Me._messagesTabPage.TabIndex = 2
        Me._MessagesTabPage.Text = "Log"
        Me._messagesTabPage.UseVisualStyleBackColor = True
        '
        '_MessagesTextBox
        '
        Me._TraceMessagesBox.AcceptsReturn = True
        Me._TraceMessagesBox.BackColor = System.Drawing.SystemColors.Window
        Me._TraceMessagesBox.Cursor = System.Windows.Forms.Cursors.IBeam
        Me._TraceMessagesBox.Dock = System.Windows.Forms.DockStyle.Fill
        Me._TraceMessagesBox.ForeColor = System.Drawing.SystemColors.WindowText
        Me._TraceMessagesBox.Location = New System.Drawing.Point(0, 0)
        Me._TraceMessagesBox.MaxLength = 0
        Me._TraceMessagesBox.Multiline = True
        Me._TraceMessagesBox.Name = "_MessagesTextBox"
        Me._TraceMessagesBox.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me._TraceMessagesBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me._TraceMessagesBox.Size = New System.Drawing.Size(867, 512)
        Me._TraceMessagesBox.TabIndex = 15
        '
        '_statusBar
        '
        Me._statusBar.Font = New System.Drawing.Font(Me.Font, System.Drawing.FontStyle.Bold)
        Me._statusBar.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me._StatusLabel, Me._StatusRegisterLabel})
        Me._statusBar.Location = New System.Drawing.Point(0, 538)
        Me._statusBar.Name = "_statusBar"
        Me._statusBar.Padding = New System.Windows.Forms.Padding(1, 0, 16, 0)
        Me._statusBar.ShowItemToolTips = True
        Me._statusBar.Size = New System.Drawing.Size(875, 25)
        Me._statusBar.TabIndex = 0
        Me._statusBar.Text = "Ready"
        '
        '_statusStatusBarPanel
        '
        Me._StatusLabel.AutoSize = False
        Me._StatusLabel.BorderSides = CType((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) _
            Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) _
            Or System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom), System.Windows.Forms.ToolStripStatusLabelBorderSides)
        Me._StatusLabel.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter
        Me._StatusLabel.Margin = New System.Windows.Forms.Padding(0)
        Me._StatusLabel.Name = "_statusStatusBarPanel"
        Me._StatusLabel.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never
        Me._StatusLabel.Size = New System.Drawing.Size(822, 25)
        Me._StatusLabel.Spring = True
        Me._StatusLabel.Text = "Ready"
        Me._StatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me._StatusLabel.ToolTipText = "Status"
        '
        '_statusRegisterLabel
        '
        Me._StatusRegisterLabel.AutoToolTip = True
        Me._StatusRegisterLabel.Name = "_statusRegisterLabel"
        Me._StatusRegisterLabel.Size = New System.Drawing.Size(36, 20)
        Me._StatusRegisterLabel.Text = "0x00"
        Me._StatusRegisterLabel.ToolTipText = "Status Register value"
        '
        '_SreCommandLabel
        '
        Me._SreCommandLabel.AutoSize = True
        Me._SreCommandLabel.Location = New System.Drawing.Point(439, 17)
        Me._SreCommandLabel.Name = "_SreCommandLabel"
        Me._SreCommandLabel.Size = New System.Drawing.Size(97, 17)
        Me._SreCommandLabel.TabIndex = 6
        Me._SreCommandLabel.Text = "SRE Command:"
        Me._SreCommandLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me._ToolTip.SetToolTip(Me._SreCommandLabel, "Service Request Enable command")
        '
        '_SrqBitsNumeric
        '
        Me._SrqBitsNumeric.Font = New System.Drawing.Font(Me.Font, System.Drawing.FontStyle.Bold)
        Me._SrqBitsNumeric.Hexadecimal = True
        Me._SrqBitsNumeric.Location = New System.Drawing.Point(538, 44)
        Me._SrqBitsNumeric.Maximum = New Decimal(New Integer() {128, 0, 0, 0})
        Me._SrqBitsNumeric.Name = "_SrqBitsNumeric"
        Me._SrqBitsNumeric.Size = New System.Drawing.Size(43, 25)
        Me._SrqBitsNumeric.TabIndex = 9
        Me._ToolTip.SetToolTip(Me._SrqBitsNumeric, "Service Requested Bits")
        Me._SrqBitsNumeric.Value = New Decimal(New Integer() {64, 0, 0, 0})
        '
        '_SrqBitsLabel
        '
        Me._SrqBitsLabel.AutoSize = True
        Me._SrqBitsLabel.Location = New System.Drawing.Point(479, 48)
        Me._SrqBitsLabel.Name = "_SrqBitsLabel"
        Me._SrqBitsLabel.Size = New System.Drawing.Size(57, 17)
        Me._SrqBitsLabel.TabIndex = 8
        Me._SrqBitsLabel.Text = "SRQ Bits"
        Me._SrqBitsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        '_SreCommandComboBox
        '
        Me._SreCommandComboBox.Font = New System.Drawing.Font(Me.Font, System.Drawing.FontStyle.Bold)
        Me._SreCommandComboBox.FormattingEnabled = True
        Me._SreCommandComboBox.Items.AddRange(New Object() {"*SRE 16", "M63x"})
        Me._SreCommandComboBox.Location = New System.Drawing.Point(538, 13)
        Me._SreCommandComboBox.Name = "_SreCommandComboBox"
        Me._SreCommandComboBox.Size = New System.Drawing.Size(121, 25)
        Me._SreCommandComboBox.TabIndex = 7
        Me._SreCommandComboBox.Text = "*SRE 16"
        Me._ToolTip.SetToolTip(Me._SreCommandComboBox, "Service request Enable command")
        '
        '_MessageAvailableBitsLabel
        '
        Me._MessageAvailableBitsLabel.AutoSize = True
        Me._MessageAvailableBitsLabel.Location = New System.Drawing.Point(476, 79)
        Me._MessageAvailableBitsLabel.Name = "_MessageAvailableBitsLabel"
        Me._MessageAvailableBitsLabel.Size = New System.Drawing.Size(60, 17)
        Me._MessageAvailableBitsLabel.TabIndex = 10
        Me._MessageAvailableBitsLabel.Text = "MAV Bits"
        Me._MessageAvailableBitsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        '_MessageAvailableBitsNumeric
        '
        Me._MessageAvailableBitsNumeric.Font = New System.Drawing.Font(Me.Font, System.Drawing.FontStyle.Bold)
        Me._MessageAvailableBitsNumeric.Hexadecimal = True
        Me._MessageAvailableBitsNumeric.Location = New System.Drawing.Point(538, 75)
        Me._MessageAvailableBitsNumeric.Maximum = New Decimal(New Integer() {128, 0, 0, 0})
        Me._MessageAvailableBitsNumeric.Name = "_MessageAvailableBitsNumeric"
        Me._MessageAvailableBitsNumeric.Size = New System.Drawing.Size(43, 25)
        Me._MessageAvailableBitsNumeric.TabIndex = 11
        Me._ToolTip.SetToolTip(Me._MessageAvailableBitsNumeric, "Message Available Bits")
        Me._MessageAvailableBitsNumeric.Value = New Decimal(New Integer() {16, 0, 0, 0})
        '
        'MessageBasedVisaPanel
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 17.0!)
        Me.BackColor = System.Drawing.SystemColors.Control
        Me.ClientSize = New System.Drawing.Size(875, 563)
        Me.Controls.Add(Me._Tabs)
        Me.Controls.Add(Me._statusBar)
        Me.Cursor = System.Windows.Forms.Cursors.Default
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Location = New System.Drawing.Point(297, 150)
        Me.MaximizeBox = False
        Me.Name = "MessageBasedVisaPanel"
        Me.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.StartPosition = System.Windows.Forms.FormStartPosition.Manual
        Me.Text = "Mini VISA Tester"
        Me._Tabs.ResumeLayout(False)
        Me._connectTabPage.ResumeLayout(False)
        Me._ConnectTabLayout.ResumeLayout(False)
        Me._connectGroupBox.ResumeLayout(False)
        Me._settingsGroupBox.ResumeLayout(False)
        Me._settingsGroupBox.PerformLayout()
        Me._SendReceiveTabPage.ResumeLayout(False)
        Me._sendReceiveLayout.ResumeLayout(False)
        Me._sendReceiveControlPanel.ResumeLayout(False)
        Me._sendReceiveControlPanel.PerformLayout()
        Me._ReceiveOptionsGroupBox.ResumeLayout(False)
        Me._ReceiveOptionsGroupBox.PerformLayout()
        CType(Me._PollIntervalNumericUpDown, System.ComponentModel.ISupportInitialize).EndInit()
        Me._sendReceiveSplitContainer.Panel1.ResumeLayout(False)
        Me._sendReceiveSplitContainer.Panel2.ResumeLayout(False)
        CType(Me._sendReceiveSplitContainer, System.ComponentModel.ISupportInitialize).EndInit()
        Me._sendReceiveSplitContainer.ResumeLayout(False)
        Me._SendGroupBox.ResumeLayout(False)
        Me._SendGroupBox.PerformLayout()
        Me._ReceiveGroupBox.ResumeLayout(False)
        Me._ReceiveGroupBox.PerformLayout()
        Me._messagesTabPage.ResumeLayout(False)
        Me._messagesTabPage.PerformLayout()
        Me._statusBar.ResumeLayout(False)
        Me._statusBar.PerformLayout()
        CType(Me._SrqBitsNumeric, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me._MessageAvailableBitsNumeric, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents _ReceiveOptionsGroupBox As System.Windows.Forms.GroupBox
    Private WithEvents _connectTabPage As System.Windows.Forms.TabPage
    Private WithEvents _ConnectTabLayout As System.Windows.Forms.TableLayoutPanel
    Private WithEvents _connectGroupBox As System.Windows.Forms.GroupBox
    Private WithEvents _ReadManualRadioButton As System.Windows.Forms.RadioButton
    Private WithEvents _ServiceRequestReceiveOptionRadioButton As System.Windows.Forms.RadioButton
    Private WithEvents _PollRadioButton As System.Windows.Forms.RadioButton
    Private WithEvents _StatusRegisterLabel As System.Windows.Forms.ToolStripStatusLabel
    Private WithEvents _sendReceiveSplitContainer As System.Windows.Forms.SplitContainer
    Private WithEvents _SendGroupBox As System.Windows.Forms.GroupBox
    Private WithEvents _InputTextBox As System.Windows.Forms.TextBox
    Private WithEvents _ReceiveGroupBox As System.Windows.Forms.GroupBox
    Private WithEvents _sendReceiveLayout As System.Windows.Forms.TableLayoutPanel
    Private WithEvents _sendReceiveControlPanel As System.Windows.Forms.Panel
    Private WithEvents _OutputTextBox As System.Windows.Forms.TextBox
    Private WithEvents _PollIntervalUnitsLabel As System.Windows.Forms.Label
    Private WithEvents _PollIntervalNumericUpDown As System.Windows.Forms.NumericUpDown
    Private WithEvents _InterfacePanel As Instrument.InterfacePanel
    Private WithEvents _settingsGroupBox As System.Windows.Forms.GroupBox
    Private WithEvents _DisconnectCommandsTextBox As System.Windows.Forms.TextBox
    Private WithEvents _SendDisconnectCommandsCheckBox As System.Windows.Forms.CheckBox
    Private WithEvents _ToolTip As System.Windows.Forms.ToolTip
    Private WithEvents _SreCommandComboBox As System.Windows.Forms.ComboBox
    Private WithEvents _MessageAvailableBitsNumeric As System.Windows.Forms.NumericUpDown
    Private WithEvents _MessageAvailableBitsLabel As System.Windows.Forms.Label
    Private WithEvents _SrqBitsNumeric As System.Windows.Forms.NumericUpDown
    Private WithEvents _SrqBitsLabel As System.Windows.Forms.Label
    Private WithEvents _SreCommandLabel As System.Windows.Forms.Label

End Class

