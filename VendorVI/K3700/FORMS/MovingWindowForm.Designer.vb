﻿<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class MovingWindowForm

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer
    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(MovingWindowForm))
        Me._ToolTip = New System.Windows.Forms.ToolTip(Me.components)
        Me._Tabs = New System.Windows.Forms.TabControl()
        Me._InstrumentTabPage = New System.Windows.Forms.TabPage()
        Me._InstrumentLayout = New System.Windows.Forms.TableLayoutPanel()
        Me._InstrumentPanel = New isr.VI.Tsp.K3700.K3700Panel()
        Me._MessagesTabPage = New System.Windows.Forms.TabPage()
        Me._TraceMessagesBox = New isr.Core.Pith.TraceMessagesBox()
        Me._StatusStrip = New System.Windows.Forms.StatusStrip()
        Me._StatusLabel = New System.Windows.Forms.ToolStripStatusLabel()
        Me._MovingWindowMeter = New isr.VI.Tsp.K3700.MovingWindowMeter()
        Me._Tabs.SuspendLayout()
        Me._InstrumentTabPage.SuspendLayout()
        Me._InstrumentLayout.SuspendLayout()
        Me._MessagesTabPage.SuspendLayout()
        Me._StatusStrip.SuspendLayout()
        Me.SuspendLayout()
        '
        '_ToolTip
        '
        Me._ToolTip.IsBalloon = True
        '
        '_Tabs
        '
        Me._Tabs.Controls.Add(Me._InstrumentTabPage)
        Me._Tabs.Controls.Add(Me._MessagesTabPage)
        Me._Tabs.Dock = System.Windows.Forms.DockStyle.Fill
        Me._Tabs.ItemSize = New System.Drawing.Size(42, 22)
        Me._Tabs.Location = New System.Drawing.Point(0, 0)
        Me._Tabs.Name = "_Tabs"
        Me._Tabs.SelectedIndex = 0
        Me._Tabs.Size = New System.Drawing.Size(796, 484)
        Me._Tabs.TabIndex = 1
        '
        '_InstrumentTabPage
        '
        Me._InstrumentTabPage.Controls.Add(Me._InstrumentLayout)
        Me._InstrumentTabPage.Location = New System.Drawing.Point(4, 26)
        Me._InstrumentTabPage.Name = "_InstrumentTabPage"
        Me._InstrumentTabPage.Size = New System.Drawing.Size(788, 454)
        Me._InstrumentTabPage.TabIndex = 5
        Me._InstrumentTabPage.Text = "Instrument"
        Me._InstrumentTabPage.ToolTipText = "Keithley 200x Multimeter"
        Me._InstrumentTabPage.UseVisualStyleBackColor = True
        '
        '_InstrumentLayout
        '
        Me._InstrumentLayout.ColumnCount = 4
        Me._InstrumentLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 6.0!))
        Me._InstrumentLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me._InstrumentLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me._InstrumentLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 6.0!))
        Me._InstrumentLayout.Controls.Add(Me._InstrumentPanel, 2, 1)
        Me._InstrumentLayout.Controls.Add(Me._MovingWindowMeter, 1, 1)
        Me._InstrumentLayout.Dock = System.Windows.Forms.DockStyle.Fill
        Me._InstrumentLayout.Location = New System.Drawing.Point(0, 0)
        Me._InstrumentLayout.Name = "_InstrumentLayout"
        Me._InstrumentLayout.RowCount = 3
        Me._InstrumentLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 6.0!))
        Me._InstrumentLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me._InstrumentLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 6.0!))
        Me._InstrumentLayout.Size = New System.Drawing.Size(788, 454)
        Me._InstrumentLayout.TabIndex = 1
        '
        '_InstrumentPanel
        '
        Me._InstrumentPanel.BackColor = System.Drawing.Color.Transparent
        Me._InstrumentPanel.ClosedResourceTitleFormat = "{0}:Closed"
        Me._InstrumentPanel.Dock = System.Windows.Forms.DockStyle.Fill
        Me._InstrumentPanel.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me._InstrumentPanel.Location = New System.Drawing.Point(397, 9)
        Me._InstrumentPanel.Name = "_InstrumentPanel"
        Me._InstrumentPanel.OpenResourceTitleFormat = "{0}:{1}"
        Me._InstrumentPanel.ResourceTitle = "K3700"
        Me._InstrumentPanel.Size = New System.Drawing.Size(382, 436)
        Me._InstrumentPanel.TabIndex = 0
        '
        '_MessagesTabPage
        '
        Me._MessagesTabPage.Controls.Add(Me._TraceMessagesBox)
        Me._MessagesTabPage.Location = New System.Drawing.Point(4, 26)
        Me._MessagesTabPage.Name = "_MessagesTabPage"
        Me._MessagesTabPage.Size = New System.Drawing.Size(391, 454)
        Me._MessagesTabPage.TabIndex = 2
        Me._MessagesTabPage.Text = "Log"
        Me._MessagesTabPage.UseVisualStyleBackColor = True
        '
        '_TraceMessagesBox
        '
        Me._TraceMessagesBox.AcceptsReturn = True
        Me._TraceMessagesBox.AlertLevel = System.Diagnostics.TraceEventType.Warning
        Me._TraceMessagesBox.BackColor = System.Drawing.SystemColors.Info
        Me._TraceMessagesBox.CausesValidation = False
        Me._TraceMessagesBox.Dock = System.Windows.Forms.DockStyle.Fill
        Me._TraceMessagesBox.Font = New System.Drawing.Font("Consolas", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me._TraceMessagesBox.ForeColor = System.Drawing.SystemColors.WindowText
        Me._TraceMessagesBox.Location = New System.Drawing.Point(0, 0)
        Me._TraceMessagesBox.MaxLength = 0
        Me._TraceMessagesBox.Multiline = True
        Me._TraceMessagesBox.Name = "_TraceMessagesBox"
        Me._TraceMessagesBox.PresetCount = 100
        Me._TraceMessagesBox.ReadOnly = True
        Me._TraceMessagesBox.ResetCount = 200
        Me._TraceMessagesBox.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me._TraceMessagesBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me._TraceMessagesBox.Size = New System.Drawing.Size(391, 454)
        Me._TraceMessagesBox.TabIndex = 15
        Me._TraceMessagesBox.TraceLevel = System.Diagnostics.TraceEventType.Verbose
        '
        '_StatusStrip
        '
        Me._StatusStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me._StatusLabel})
        Me._StatusStrip.Location = New System.Drawing.Point(0, 484)
        Me._StatusStrip.Name = "_StatusStrip"
        Me._StatusStrip.Size = New System.Drawing.Size(796, 22)
        Me._StatusStrip.TabIndex = 2
        Me._StatusStrip.Text = "Status Strip"
        '
        '_StatusLabel
        '
        Me._StatusLabel.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me._StatusLabel.Name = "_StatusLabel"
        Me._StatusLabel.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never
        Me._StatusLabel.Size = New System.Drawing.Size(781, 17)
        Me._StatusLabel.Spring = True
        Me._StatusLabel.Text = "Ready"
        '
        '_MovingWindowMeter
        '
        Me._MovingWindowMeter.Dock = System.Windows.Forms.DockStyle.Fill
        Me._MovingWindowMeter.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me._MovingWindowMeter.Location = New System.Drawing.Point(9, 9)
        Me._MovingWindowMeter.Name = "_MovingWindowMeter"
        Me._MovingWindowMeter.Size = New System.Drawing.Size(382, 436)
        Me._MovingWindowMeter.TabIndex = 1
        '
        'MovingWindowForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 17.0!)
        Me.BackColor = System.Drawing.SystemColors.Control
        Me.ClientSize = New System.Drawing.Size(796, 506)
        Me.Controls.Add(Me._Tabs)
        Me.Controls.Add(Me._StatusStrip)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Location = New System.Drawing.Point(297, 150)
        Me.MaximizeBox = False
        Me.Name = "MovingWindowForm"
        Me.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.StartPosition = System.Windows.Forms.FormStartPosition.Manual
        Me.Text = "Virtual Instrument Service Request Tester"
        Me._Tabs.ResumeLayout(False)
        Me._InstrumentTabPage.ResumeLayout(False)
        Me._InstrumentLayout.ResumeLayout(False)
        Me._MessagesTabPage.ResumeLayout(False)
        Me._MessagesTabPage.PerformLayout()
        Me._StatusStrip.ResumeLayout(False)
        Me._StatusStrip.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents _TraceMessagesBox As isr.Core.Pith.TraceMessagesBox
    Private WithEvents _MessagesTabPage As System.Windows.Forms.TabPage
    Private WithEvents _Tabs As System.Windows.Forms.TabControl
    Private WithEvents _ToolTip As System.Windows.Forms.ToolTip
    Private WithEvents _StatusStrip As System.Windows.Forms.StatusStrip
    Private WithEvents _StatusLabel As System.Windows.Forms.ToolStripStatusLabel
    Private WithEvents _InstrumentTabPage As System.Windows.Forms.TabPage
    Private WithEvents _InstrumentLayout As System.Windows.Forms.TableLayoutPanel
    Private WithEvents _InstrumentPanel As K3700Panel
    Friend WithEvents _MovingWindowMeter As MovingWindowMeter
End Class

