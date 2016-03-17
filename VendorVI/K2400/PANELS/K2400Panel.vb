Imports System.ComponentModel
Imports System.Windows.Forms
Imports isr.Core.Controls.ComboBoxExtensions
Imports isr.Core.Controls.CheckBoxExtensions
Imports isr.Core.Controls.NumericUpDownExtensions
Imports isr.Core.Controls.SafeSetterExtensions
Imports isr.Core.Controls.ToolStripExtensions
Imports isr.Core.Pith
Imports isr.Core.Pith.EnumExtensions
Imports isr.Core.Pith.EscapeSequencesExtensions
Imports isr.Core.Pith.ErrorProviderExtensions
Imports isr.VI.SourceMeasure
''' <summary> Provides a user interface for the Keithley 2400 Device. </summary>
''' <license> (c) 2005 Integrated Scientific Resources, Inc.<para>
''' Licensed under The MIT License. </para><para>
''' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
''' BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
''' NON-INFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
''' DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
''' OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
''' </para> </license>
''' <history date="01/15/2008" by="David" revision="2.0.2936.x"> Create based on the 24xx
''' system classes. </history>
<System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")>
<System.ComponentModel.DisplayName("K2700 Panel"),
      System.ComponentModel.Description("Keithley 2700 Device Panel"),
      System.Drawing.ToolboxBitmap(GetType(K2400Panel))>
Public Class K2400Panel
    Inherits VI.Instrument.ResourcePanelBase

#Region " CONSTRUCTORS  and  DESTRUCTORS "

    Private _InitializingComponents As Boolean
    ''' <summary> Default constructor. </summary>
    <CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")>
    Public Sub New()
        Me.New(New Device)
        Me.IsDeviceOwner = True
    End Sub

    ''' <summary> Specialized constructor for use only by derived class. </summary>
    ''' <remarks> David, 1/21/2016. </remarks>
    ''' <param name="device"> The device. </param>
    Protected Sub New(ByVal device As Device)
        MyBase.New(device)
        Me._InitializingComponents = True
        Me.InitializeComponent()
        Me._InitializingComponents = False
        Me._AssignDevice(device)
    End Sub

    ''' <summary>
    ''' Releases the unmanaged resources used by the isr.VI.Instrument.ResourcePanelBase and
    ''' optionally releases the managed resources.
    ''' </summary>
    ''' <remarks> David, 12/22/2015. </remarks>
    ''' <param name="disposing"> true to release both managed and unmanaged resources; false to
    '''                          release only unmanaged resources. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If Not Me.IsDisposed AndAlso disposing Then
                Try
                    Me._BinningInfo = Nothing
                    Me._ActiveInsulationResistance = Nothing
                    If Me._isInsulationTestOwner AndAlso Me._InsulationTest IsNot Nothing Then
                        Me._InsulationTest.Dispose()
                        Me._InsulationTest = Nothing
                    End If
                    If Me.Device IsNot Nothing Then Me.DeviceClosing(Me, New System.ComponentModel.CancelEventArgs)
                Catch ex As Exception
                    Debug.Assert(Not Debugger.IsAttached, "Exception occurred closing the device", "Exception details: {0}", ex)
                End Try
                ' the device gets disposed in the base class!
                If Me.components IsNot Nothing Then Me.components.Dispose() : Me.components = Nothing
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

#End Region

#Region " FORM EVENTS "

    ''' <summary> Handles the <see cref="E:System.Windows.Forms.UserControl.Load" /> event. </summary>
    ''' <remarks> David, 1/4/2016. </remarks>
    ''' <param name="e"> An <see cref="T:System.EventArgs" /> that contains the event data. </param>
    Protected Overrides Sub OnLoad(e As EventArgs)
        Try
            Me.TraceMessagesBox.ContainerPanel = Me._MessagesTabPage
        Finally
            MyBase.OnLoad(e)
        End Try
    End Sub

#End Region

#Region " DEVICE "

    ''' <summary> Assigns a device. </summary>
    ''' <remarks> David, 1/21/2016. </remarks>
    ''' <param name="value"> True to show or False to hide the control. </param>
    Private Sub _AssignDevice(ByVal value As Device)
        Me._Device = value
        Me.AddListeners()
        Me.OnDeviceOpenChanged(value)
    End Sub

    ''' <summary> Assigns a device. </summary>
    ''' <remarks> David, 1/21/2016. </remarks>
    ''' <param name="value"> True to show or False to hide the control. </param>
    Public Overloads Sub AssignDevice(ByVal value As Device)
        Me.IsDeviceOwner = False
        MyBase.AssignDevice(value)
        Me._AssignDevice(value)
    End Sub

    ''' <summary> Releases the device. </summary>
    ''' <remarks> David, 1/21/2016. </remarks>
    Protected Overrides Sub ReleaseDevice()
        MyBase.ReleaseDevice()
    End Sub

    ''' <summary> Gets a reference to the Keithley 2400 Device. </summary>
    ''' <value> The device. </value>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(False)>
    Public Overloads ReadOnly Property Device() As Device

#End Region

#Region " DEVICE EVENT HANDLERS "

    ''' <summary> Gets the session. </summary>
    ''' <value> The session. </value>
    Private ReadOnly Property Session As VI.SessionBase
        Get
            Return Me.Device.Session
        End Get
    End Property

    ''' <summary> Executes the device open changed action. </summary>
    Protected Overrides Sub OnDeviceOpenChanged(ByVal device As DeviceBase)
        Dim isOpen As Boolean = CType(device?.IsDeviceOpen, Boolean?).GetValueOrDefault(False)
        If isOpen Then
            Me._SimpleReadWriteControl.Connect(device?.Session)
            ' Me._SimpleReadWriteControl.ReadEnabled = True
        Else
            Me._SimpleReadWriteControl.Disconnect()
        End If
        ' enable the tabs even if the device failed to open.
        ' Me._Tabs.Enabled = True
        For Each t As Windows.Forms.TabPage In Me._Tabs.TabPages
            If t IsNot Me._MessagesTabPage Then
                For Each c As Windows.Forms.Control In t.Controls : Me.RecursivelyEnable(c, isOpen) : Next
            End If
        Next
    End Sub

    ''' <summary> Handle the device property changed event. </summary>
    ''' <param name="device">    The device. </param>
    ''' <param name="propertyName"> Name of the property. </param>
    Protected Overrides Sub OnDevicePropertyChanged(ByVal device As DeviceBase, ByVal propertyName As String)
        If device Is Nothing OrElse String.IsNullOrWhiteSpace(propertyName) Then Return
        MyBase.OnDevicePropertyChanged(device, propertyName)
        Select Case propertyName
            Case NameOf(device.IsServiceRequestEventEnabled)
                Me._ServiceRequestsHandlerEnabledMenuItem.SafeCheckedSetter(device.IsServiceRequestEventEnabled)
            Case NameOf(device.SessionMessagesTraceEnabled)
                Me._SessionTraceEnabledMenuItem.SafeCheckedSetter(device.SessionMessagesTraceEnabled)
        End Select
    End Sub

#If False Then

#End If
#If False Then
    ''' <summary> Device property changed. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Property changed event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Protected Overrides Sub DevicePropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)
        Try
            If sender IsNot Nothing AndAlso e IsNot Nothing Then
                Me.OnDevicePropertyChanged(TryCast(sender, Device), e.PropertyName)
            End If
        Catch ex As Exception
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId,
                               "Exception handling property '{0}' changed event;. Details: {1}", e.PropertyName, ex)
        Finally
            MyBase.DevicePropertyChanged(sender, e)
        End Try
    End Sub
#End If

    ''' <summary> Event handler. Called when device opened. </summary>
    ''' <param name="sender"> <see cref="System.Object"/> instance of this
    ''' <see cref="System.Windows.Forms.Control"/> </param>
    ''' <param name="e">      Event information. </param>
    Protected Overrides Sub DeviceOpened(ByVal sender As Object, ByVal e As System.EventArgs)
        AddHandler Me.Device.ArmLayerSubsystem.PropertyChanged, AddressOf Me.ArmSubsystemPropertyChanged
        AddHandler Me.Device.ContactCheckLimit.PropertyChanged, AddressOf Me.ContactCheckLimitPropertyChanged
        AddHandler Me.Device.ComplianceLimit.PropertyChanged, AddressOf Me.ComplianceLimitPropertyChanged
        AddHandler Me.Device.CompositeLimit.PropertyChanged, AddressOf Me.CompositeLimitPropertyChanged
        AddHandler Me.Device.UpperLowerLimit.PropertyChanged, AddressOf Me.UpperLowerLimitPropertyChanged
        AddHandler Me.Device.Calculate2Subsystem.PropertyChanged, AddressOf Me.Calculate2SubsystemPropertyChanged
        AddHandler Me.Device.DigitalOutput.PropertyChanged, AddressOf Me.DigitalOutputPropertyChanged
        AddHandler Me.Device.FormatSubsystem.PropertyChanged, AddressOf Me.FormatSubsystemPropertyChanged
        AddHandler Me.Device.MeasureSubsystem.PropertyChanged, AddressOf Me.MeasureSubsystemPropertyChanged
        AddHandler Me.Device.RouteSubsystem.PropertyChanged, AddressOf Me.RouteSubsystemPropertyChanged
        AddHandler Me.Device.SenseSubsystem.PropertyChanged, AddressOf Me.SenseSubsystemPropertyChanged
        AddHandler Me.Device.SenseCurrentSubsystem.PropertyChanged, AddressOf Me.SenseCurrentSubsystemPropertyChanged
        AddHandler Me.Device.SenseResistanceSubsystem.PropertyChanged, AddressOf Me.SenseResistanceSubsystemPropertyChanged
        AddHandler Me.Device.SenseVoltageSubsystem.PropertyChanged, AddressOf Me.SenseVoltageSubsystemPropertyChanged
        AddHandler Me.Device.SourceSubsystem.PropertyChanged, AddressOf Me.SourceSubsystemPropertyChanged
        AddHandler Me.Device.SourceCurrentSubsystem.PropertyChanged, AddressOf Me.SourceCurrentSubsystemPropertyChanged
        AddHandler Me.Device.SourceVoltageSubsystem.PropertyChanged, AddressOf Me.SourceVoltageSubsystemPropertyChanged
        AddHandler Me.Device.TriggerSubsystem.PropertyChanged, AddressOf Me.TriggerSubsystemPropertyChanged
        AddHandler Me.Device.StatusSubsystem.PropertyChanged, AddressOf Me.StatusSubsystemPropertyChanged
        AddHandler Me.Device.SystemSubsystem.PropertyChanged, AddressOf Me.SystemSubsystemPropertyChanged
        Me._MeterTimer = New System.Windows.Forms.Timer()
        Me._MeterTimer.Enabled = False
        MyBase.DeviceOpened(sender, e)
    End Sub

    ''' <summary> Executes the title changed action. </summary>
    ''' <remarks> David, 1/14/2016. </remarks>
    ''' <param name="value"> True to show or False to hide the control. </param>
    Protected Overrides Sub OnTitleChanged(ByVal value As String)
        Me._TitleLabel.Text = value
        Me._TitleLabel.Visible = Not String.IsNullOrWhiteSpace(value)
        MyBase.OnTitleChanged(Title)
    End Sub

    ''' <summary> Event handler. Called when device is closing. </summary>
    ''' <param name="sender"> <see cref="System.Object"/> instance of this
    ''' <see cref="System.Windows.Forms.Control"/> </param>
    ''' <param name="e">      Event information. </param>
    Protected Overrides Sub DeviceClosing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs)
        MyBase.DeviceClosing(sender, e)
        If e?.Cancel Then Return
        If Me._MeterTimer IsNot Nothing Then
            Me._MeterTimer.Enabled = False
            Me._MeterTimer.Dispose()
            Me._MeterTimer = Nothing
        End If
        If Me.IsDeviceOpen Then
            RemoveHandler Me.Device.ArmLayerSubsystem.PropertyChanged, AddressOf Me.ArmSubsystemPropertyChanged
            RemoveHandler Me.Device.ContactCheckLimit.PropertyChanged, AddressOf Me.ContactCheckLimitPropertyChanged
            RemoveHandler Me.Device.ComplianceLimit.PropertyChanged, AddressOf Me.ComplianceLimitPropertyChanged
            RemoveHandler Me.Device.CompositeLimit.PropertyChanged, AddressOf Me.CompositeLimitPropertyChanged
            RemoveHandler Me.Device.UpperLowerLimit.PropertyChanged, AddressOf Me.UpperLowerLimitPropertyChanged
            RemoveHandler Me.Device.Calculate2Subsystem.PropertyChanged, AddressOf Me.Calculate2SubsystemPropertyChanged
            RemoveHandler Me.Device.DigitalOutput.PropertyChanged, AddressOf Me.DigitalOutputPropertyChanged
            RemoveHandler Me.Device.FormatSubsystem.PropertyChanged, AddressOf Me.FormatSubsystemPropertyChanged
            RemoveHandler Me.Device.MeasureSubsystem.PropertyChanged, AddressOf Me.MeasureSubsystemPropertyChanged
            RemoveHandler Me.Device.SenseSubsystem.PropertyChanged, AddressOf Me.SenseSubsystemPropertyChanged
            RemoveHandler Me.Device.RouteSubsystem.PropertyChanged, AddressOf Me.RouteSubsystemPropertyChanged
            RemoveHandler Me.Device.SenseCurrentSubsystem.PropertyChanged, AddressOf Me.SenseCurrentSubsystemPropertyChanged
            RemoveHandler Me.Device.SenseResistanceSubsystem.PropertyChanged, AddressOf Me.SenseResistanceSubsystemPropertyChanged
            RemoveHandler Me.Device.SenseVoltageSubsystem.PropertyChanged, AddressOf Me.SenseVoltageSubsystemPropertyChanged
            RemoveHandler Me.Device.SourceSubsystem.PropertyChanged, AddressOf Me.SourceSubsystemPropertyChanged
            RemoveHandler Me.Device.SourceCurrentSubsystem.PropertyChanged, AddressOf Me.SourceCurrentSubsystemPropertyChanged
            RemoveHandler Me.Device.SourceVoltageSubsystem.PropertyChanged, AddressOf Me.SourceVoltageSubsystemPropertyChanged
            RemoveHandler Me.Device.TriggerSubsystem.PropertyChanged, AddressOf Me.TriggerSubsystemPropertyChanged
            RemoveHandler Me.Device.StatusSubsystem.PropertyChanged, AddressOf Me.StatusSubsystemPropertyChanged
            RemoveHandler Me.Device.SystemSubsystem.PropertyChanged, AddressOf Me.SystemSubsystemPropertyChanged
        End If
    End Sub

    ''' <summary> Event handler. Called by _HandleServiceRequestsCheckBox for check state changed
    ''' events. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _ServiceRequestsHandlerEnabledMenuItem_CheckStateChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles _ServiceRequestsHandlerEnabledMenuItem.CheckStateChanged
        If Me._InitializingComponents Then Return
        Dim checkBox As CheckBox = TryCast(sender, CheckBox)
        Try
            Me.Cursor = Cursors.WaitCursor
            If checkBox IsNot Nothing AndAlso Not checkBox.Checked = Me.Device.IsServiceRequestEventEnabled Then
                If checkBox IsNot Nothing AndAlso checkBox.Checked Then
                    Me.EnableServiceRequestEventHandler()
                    Me.Device.StatusSubsystem.EnableServiceRequest(ServiceRequests.All)
                Else
                    Me.Device.StatusSubsystem.EnableServiceRequest(ServiceRequests.None)
                    Me.DisableServiceRequestEventHandler()
                End If
                Me.Device.StatusSubsystem.ReadRegisters()
            End If
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, "Failed toggling service request")
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, "Exception occurred toggling service request;. Details: {0}", ex)
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

#End Region

#Region " SUBSYSTEMS "

#Region " CALCULATE 2 "

    ''' <summary> Handle the Calculate2 subsystem property changed event. </summary>
    ''' <param name="subsystem">    The subsystem. </param>
    ''' <param name="propertyName"> Name of the property. </param>
    <CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")>
    Private Sub OnSubsystemPropertyChanged(ByVal subsystem As Calculate2Subsystem, ByVal propertyName As String)
        If subsystem Is Nothing OrElse String.IsNullOrWhiteSpace(propertyName) Then Return
        Select Case propertyName
        End Select
    End Sub

    ''' <summary> Calculate2 subsystem property changed. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Property changed event inCalculate2ion. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub Calculate2SubsystemPropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)
        Try
            If sender IsNot Nothing AndAlso e IsNot Nothing Then
                Me.OnSubsystemPropertyChanged(TryCast(sender, Calculate2Subsystem), e.PropertyName)
            End If
        Catch ex As Exception
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId,
                               "Exception handling Calculate2 Subsystem property changed Event;. Failed property {0}. Details: {1}",
                               e.PropertyName, ex)
        End Try
    End Sub


#End Region

#Region " CONTACT CHECK LIMIT "

    ''' <summary> Handle the contact check property changed event. </summary>
    ''' <param name="subsystem">    The subsystem. </param>
    ''' <param name="propertyName"> Name of the property. </param>
    Private Sub OnSubsystemPropertyChanged(ByVal subsystem As ContactCheckLimit, ByVal propertyName As String)
        If subsystem Is Nothing OrElse String.IsNullOrWhiteSpace(propertyName) Then Return
        Select Case propertyName
            Case NameOf(subsystem.FailureBits)
                If subsystem.FailureBits.HasValue Then Me._ContactCheckBitPatternNumeric.SafeValueSetter(subsystem.FailureBits.Value)
        End Select
    End Sub

    ''' <summary> Contact Check Limit subsystem property changed. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Property changed event inCalculate2ion. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub ContactCheckLimitPropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)
        Try
            If sender IsNot Nothing AndAlso e IsNot Nothing Then
                Me.OnSubsystemPropertyChanged(TryCast(sender, ContactCheckLimit), e.PropertyName)
            End If
        Catch ex As Exception
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId,
                               "Exception handling Calculate2 Subsystem property changed Event;. Failed property {0}. Details: {1}",
                               e.PropertyName, ex)
        End Try
    End Sub


#End Region

#Region " COMPOSITE LIMIT "

    ''' <summary> Handle the contact check property changed event. </summary>
    ''' <param name="subsystem">    The subsystem. </param>
    ''' <param name="propertyName"> Name of the property. </param>
    <CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")>
    Private Sub OnSubsystemPropertyChanged(ByVal subsystem As CompositeLimit, ByVal propertyName As String)
        If subsystem Is Nothing OrElse String.IsNullOrWhiteSpace(propertyName) Then Return
        Select Case propertyName
            Case NameOf(subsystem.FailureBits)
        End Select
    End Sub

    ''' <summary> Contact Check Limit subsystem property changed. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Property changed event inCalculate2ion. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub CompositeLimitPropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)
        Try
            If sender IsNot Nothing AndAlso e IsNot Nothing Then
                Me.OnSubsystemPropertyChanged(TryCast(sender, CompositeLimit), e.PropertyName)
            End If
        Catch ex As Exception
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId,
                               "Exception handling Calculate2 Subsystem property changed Event;. Failed property {0}. Details: {1}",
                               e.PropertyName, ex)
        End Try
    End Sub

#End Region

#Region " COMPLIANCE LIMIT "

    ''' <summary> Handle the contact check property changed event. </summary>
    ''' <param name="subsystem">    The subsystem. </param>
    ''' <param name="propertyName"> Name of the property. </param>
    <CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")>
    Private Sub OnSubsystemPropertyChanged(ByVal subsystem As ComplianceLimit, ByVal propertyName As String)
        If subsystem Is Nothing OrElse String.IsNullOrWhiteSpace(propertyName) Then Return
        Select Case propertyName
            Case NameOf(subsystem.FailureBits)
        End Select
    End Sub

    ''' <summary> Contact Check Limit subsystem property changed. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Property changed event inCalculate2ion. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub ComplianceLimitPropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)
        Try
            If sender IsNot Nothing AndAlso e IsNot Nothing Then
                Me.OnSubsystemPropertyChanged(TryCast(sender, ComplianceLimit), e.PropertyName)
            End If
        Catch ex As Exception
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId,
                               "Exception handling Calculate2 Subsystem property changed Event;. Failed property {0}. Details: {1}",
                               e.PropertyName, ex)
        End Try
    End Sub

#End Region

#Region " UPPER LOWER LIMIT "

    ''' <summary> Handle the upper lower limit subsystem property changed event. </summary>
    ''' <param name="subsystem">    The subsystem. </param>
    ''' <param name="propertyName"> Name of the property. </param>
    <CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")>
    Private Sub OnSubsystemPropertyChanged(ByVal subsystem As UpperLowerLimit, ByVal propertyName As String)
        If subsystem Is Nothing OrElse String.IsNullOrWhiteSpace(propertyName) Then Return
        Select Case propertyName
        End Select
    End Sub

    ''' <summary> Upper lower limit  subsystem property changed. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Property changed event inCalculate2ion. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub UpperLowerLimitPropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)
        Try
            If sender IsNot Nothing AndAlso e IsNot Nothing Then
                Me.OnSubsystemPropertyChanged(TryCast(sender, UpperLowerLimit), e.PropertyName)
            End If
        Catch ex As Exception
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId,
                               "Exception handling Calculate2 Subsystem property changed Event;. Failed property {0}. Details: {1}",
                               e.PropertyName, ex)
        End Try
    End Sub

#End Region

#Region " DIGITAL OUTPUT "

    ''' <summary> Handle the contact check property changed event. </summary>
    ''' <param name="subsystem">    The subsystem. </param>
    ''' <param name="propertyName"> Name of the property. </param>
    <CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")>
    Private Sub OnSubsystemPropertyChanged(ByVal subsystem As DigitalOutput, ByVal propertyName As String)
        If subsystem Is Nothing OrElse String.IsNullOrWhiteSpace(propertyName) Then Return
        Select Case propertyName
            Case NameOf(subsystem.BitSize)
            Case NameOf(subsystem.Delay)
        End Select
    End Sub

    ''' <summary> Contact Check Limit subsystem property changed. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Property changed event inCalculate2ion. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub DigitalOutputPropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)
        Try
            If sender IsNot Nothing AndAlso e IsNot Nothing Then
                Me.OnSubsystemPropertyChanged(TryCast(sender, DigitalOutput), e.PropertyName)
            End If
        Catch ex As Exception
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId,
                               "Exception handling Calculate2 Subsystem property changed Event;. Failed property {0}. Details: {1}",
                               e.PropertyName, ex)
        End Try
    End Sub

#End Region

#Region " FORMAT AND READING "

    ''' <summary> Handle the format subsystem property changed event. </summary>
    ''' <param name="subsystem">    The subsystem. </param>
    ''' <param name="propertyName"> Name of the property. </param>
    Private Sub OnSubsystemPropertyChanged(ByVal subsystem As FormatSubsystem, ByVal propertyName As String)
        If subsystem Is Nothing OrElse String.IsNullOrWhiteSpace(propertyName) Then Return
        Select Case propertyName
            Case NameOf(subsystem.Elements)
                If Me.Device IsNot Nothing AndAlso subsystem.Elements <> ReadingElements.None Then
                    Dim selectedIndex As Integer = Me._ReadingComboBox.SelectedIndex
                    With Me._ReadingComboBox
                        .DataSource = Nothing
                        .Items.Clear()
                        .DataSource = GetType(VI.ReadingElements).ValueDescriptionPairs(subsystem.Elements And Not ReadingElements.Units)
                        .DisplayMember = "Value"
                        .ValueMember = "Key"
                        If .Items.Count > 0 Then
                            .SelectedIndex = Math.Max(selectedIndex, 0)
                        End If
                    End With
                End If
        End Select
    End Sub

    ''' <summary> Format subsystem property changed. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Property changed event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub FormatSubsystemPropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)
        Try
            If sender IsNot Nothing AndAlso e IsNot Nothing Then
                Me.OnSubsystemPropertyChanged(TryCast(sender, FormatSubsystem), e.PropertyName)
            End If
        Catch ex As Exception
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId,
                               "Exception handling Format Subsystem property changed Event;. Failed property {0}. Details: {1}",
                               e.PropertyName, ex)
        End Try
    End Sub

    ''' <summary> Selects a new reading to display. </summary>
    ''' <param name="value"> The <see cref="TraceMessage">message</see> to display and
    ''' log. </param>
    ''' <returns> The VI.ReadingElements. </returns>
    Friend Function SelectReading(ByVal value As VI.ReadingElements) As VI.ReadingElements
        If Me.IsDeviceOpen AndAlso
                (value <> VI.ReadingElements.None) AndAlso (value <> Me.SelectedReading) Then
            Me._ReadingComboBox.SafeSelectItem(value.ValueDescriptionPair)
        End If
        Return Me.SelectedReading
    End Function

    ''' <summary> Gets the selected reading. </summary>
    ''' <value> The selected reading. </value>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(False)>
    Private ReadOnly Property SelectedReading() As VI.ReadingElements
        Get
            Return CType(CType(Me._ReadingComboBox.SelectedItem, System.Collections.Generic.KeyValuePair(Of [Enum], String)).Key, VI.ReadingElements)
        End Get
    End Property

#End Region

#Region " MEASURE "

    ''' <summary> Executes the measurement available action. </summary>
    ''' <param name="readings"> The readings. </param>
    Private Sub onMeasurementAvailable(ByVal readings As Readings)
        Const clear As String = "    "
        If readings Is Nothing OrElse readings.IsEmpty Then
            Me._ReadingToolStripStatusLabel.Text = "-.------- :)"
            Me._ComplianceToolStripStatusLabel.Text = clear
            Me._TbdToolStripStatusLabel.Text = clear
        Else
            Me._ReadingToolStripStatusLabel.SafeTextSetter(readings.ToString(Me.SelectedReading))
            Dim reading As MeasuredAmount = readings.SelectMeasuredAmount(Me.SelectedReading)
            If reading Is Nothing OrElse reading.Amount Is Nothing Then
                Me._ComplianceToolStripStatusLabel.Text = clear
                Me._TbdToolStripStatusLabel.Text = clear
            Else
                Dim failureCode As String = reading.MetaStatus.ToShortDescription("")
                Dim details As String = reading.MetaStatus.ToLongDescription("")
                Me._ComplianceToolStripStatusLabel.Text = (failureCode & clear).Substring(0, clear.Length)
                Me._TbdToolStripStatusLabel.Text = clear
                If String.IsNullOrEmpty(details) Then
                    Me.Talker?.Publish(TraceEventType.Information, My.MyLibrary.TraceEventId, "Instruments parsed reading elements.")
                Else
                    Me.Talker?.Publish(TraceEventType.Information, My.MyLibrary.TraceEventId, details)
                End If
            End If
        End If
    End Sub

    ''' <summary> Handles the Measure subsystem property changed event. </summary>
    ''' <param name="subsystem">    The subsystem. </param>
    ''' <param name="propertyName"> Name of the property. </param>
    Private Sub OnSubsystemPropertyChanged(ByVal subsystem As MeasureSubsystem, ByVal propertyName As String)
        If subsystem Is Nothing OrElse String.IsNullOrWhiteSpace(propertyName) Then Return
        Select Case propertyName
            Case NameOf(subsystem.LastReading)
                Me._LastReadingTextBox.SafeTextSetter(subsystem.LastReading)
                Me.Talker?.Publish(TraceEventType.Information, My.MyLibrary.TraceEventId,
                                   "Measure message: {0}.", subsystem.LastReading.InsertCommonEscapeSequences)
            Case NameOf(subsystem.MeasurementAvailable)
                Me.onMeasurementAvailable(subsystem.Readings)
        End Select
    End Sub

    ''' <summary> Measure subsystem property changed. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Property changed event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub MeasureSubsystemPropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)
        Try
            If sender IsNot Nothing AndAlso e IsNot Nothing Then
                Me.OnSubsystemPropertyChanged(TryCast(sender, MeasureSubsystem), e.PropertyName)
            End If
        Catch ex As Exception
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId,
                               "Exception handling Measure Subsystem property changed Event;. Failed property {0}. Details: {1}",
                               e.PropertyName, ex)
        End Try
    End Sub

#End Region

#Region " ROUTE "

    ''' <summary> Handle the Route subsystem property changed event. </summary>
    ''' <param name="subsystem">    The subsystem. </param>
    ''' <param name="propertyName"> Name of the property. </param>
    <CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")>
    Private Sub OnSubsystemPropertyChanged(ByVal subsystem As RouteSubsystem, ByVal propertyName As String)
        If subsystem Is Nothing OrElse String.IsNullOrWhiteSpace(propertyName) Then Return
        Select Case propertyName
            Case NameOf(subsystem.TerminalMode)
                Dim value As Boolean? = New Boolean?
                If subsystem.TerminalMode.HasValue Then
                    value = subsystem.TerminalMode.Value = RouteTerminalMode.Front
                End If
                Me._OutputTerminalMenuItem.SafeSilentCheckStateSetter(value.ToCheckState)
        End Select
    End Sub

    ''' <summary> Route subsystem property changed. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Property changed event inRouteion. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub RouteSubsystemPropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)
        Try
            If sender IsNot Nothing AndAlso e IsNot Nothing Then
                Me.OnSubsystemPropertyChanged(TryCast(sender, RouteSubsystem), e.PropertyName)
            End If
        Catch ex As Exception
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId,
                               "Exception handling Route Subsystem property changed Event;. Failed property {0}. Details: {1}",
                               e.PropertyName, ex)
        End Try
    End Sub

    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _OutputTerminalMenuItem_CheckStateChanged(sender As Object, e As EventArgs) Handles _OutputTerminalMenuItem.CheckStateChanged
        If Me._InitializingComponents Then Return
        Me.ErrorProvider.Clear()
        Dim menuItem As ToolStripMenuItem = TryCast(sender, ToolStripMenuItem)
        Try
            If Not Me.Device.RouteSubsystem.TerminalMode.HasValue OrElse
                (Me.Device.RouteSubsystem.TerminalMode.Value = RouteTerminalMode.Front) <> menuItem.Checked Then
                If menuItem.Checked Then
                    Me.Device.RouteSubsystem.ApplyTerminalMode(RouteTerminalMode.Front)
                Else
                    Me.Device.RouteSubsystem.ApplyTerminalMode(RouteTerminalMode.Rear)
                End If
            End If
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, "Exception occurred toggling output terminal")
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, "Exception occurred toggling output terminal;. Details: {0}", ex)
        Finally
            If menuItem IsNot Nothing Then
                If menuItem.CheckState = Windows.Forms.CheckState.Indeterminate Then
                    menuItem.Text = "R/F?"
                Else
                    menuItem.Text = CStr(IIf(menuItem.Checked, "Front", "Rear"))
                End If
            End If
        End Try
    End Sub

#End Region

#Region " SENSE "

    ''' <summary> Handles the supported function modes changed action. </summary>
    ''' <param name="subsystem"> The subsystem. </param>
    Private Sub onSupportedFunctionModesChanged(ByVal subsystem As SenseSubsystem)
        If subsystem IsNot Nothing AndAlso subsystem.SupportedFunctionModes <> VI.Scpi.SenseFunctionModes.None Then
            With Me._SenseFunctionComboBox
                .DataSource = Nothing
                .Items.Clear()
                .DataSource = GetType(VI.Scpi.SenseFunctionModes).ValueDescriptionPairs(subsystem.SupportedFunctionModes)
                .DisplayMember = "Value"
                .ValueMember = "Key"
                If .Items.Count > 0 Then
                    .SelectedItem = VI.Scpi.SenseFunctionModes.Voltage.ValueDescriptionPair()
                End If
            End With
            With Me._EnabledSenseFunctionsListBox
                .DataSource = Nothing
                .Items.Clear()
                .DataSource = GetType(VI.Scpi.SenseFunctionModes).ValueDescriptionPairs(subsystem.SupportedFunctionModes)
                .DisplayMember = "Value"
                .ValueMember = "Key"
            End With
        End If
    End Sub

    ''' <summary> Gets or sets the check function modes. </summary>
    ''' <exception cref="ArgumentNullException">    Thrown when one or more required arguments are
    '''                                             null. </exception>
    ''' <exception cref="OperationFailedException"> Thrown when operation failed to execute. </exception>
    ''' <value> The check function modes. </value>
    Public Property CheckedFunctionModes As Scpi.SenseFunctionModes
        Get
            Dim checkedModes As Scpi.SenseFunctionModes = Scpi.SenseFunctionModes.None
            With Me._EnabledSenseFunctionsListBox
                For Each item As Object In .CheckedItems
                    Dim mode As Scpi.SenseFunctionModes = CType(CType(item, System.Collections.Generic.KeyValuePair(Of [Enum], String)).Key, VI.Scpi.SenseFunctionModes)
                    If checkedModes = Scpi.SenseFunctionModes.None Then
                        checkedModes = mode
                    Else
                        checkedModes = checkedModes Or mode
                    End If
                Next
            End With
            Return checkedModes
        End Get
        Set(value As Scpi.SenseFunctionModes)
            With Me._EnabledSenseFunctionsListBox
                For Each item As Object In .Items
                    Dim mode As Scpi.SenseFunctionModes = CType(CType(item, System.Collections.Generic.KeyValuePair(Of [Enum], String)).Key, VI.Scpi.SenseFunctionModes)
                    If (value And mode) = 0 Then
                        .SetItemCheckState(.Items.IndexOf(item), CheckState.Unchecked)
                    Else
                        .SetItemCheckState(.Items.IndexOf(item), CheckState.Checked)
                    End If
                Next
            End With
        End Set
    End Property

    ''' <summary> Executes the function modes changed action. </summary>
    ''' <remarks> David, 3/16/2016. </remarks>
    ''' <param name="subsystem"> The subsystem. </param>
    Private Sub OnFunctionModesChanged(ByVal subsystem As SenseSubsystem)
        If subsystem IsNot Nothing AndAlso subsystem.FunctionModes.HasValue Then
            Me.CheckedFunctionModes = subsystem.FunctionModes.Value
        End If
    End Sub

    ''' <summary> Handle the Sense subsystem property changed event. </summary>
    ''' <param name="subsystem">    The subsystem. </param>
    ''' <param name="propertyName"> Name of the property. </param>
    Private Sub OnSubsystemPropertyChanged(ByVal subsystem As SenseSubsystem, ByVal propertyName As String)
        If subsystem Is Nothing OrElse String.IsNullOrWhiteSpace(propertyName) Then Return
        ' Me._senseRangeTextBox.SafeTextSetter(Me.Device.SenseRange(VI.ResourceAccessLevels.Cache).ToString(Globalization.CultureInfo.CurrentCulture))
        ' Me._integrationPeriodTextBox.SafeTextSetter(Me.Device.SenseIntegrationPeriodCaption)
        Select Case propertyName
            Case NameOf(subsystem.ConcurrentSenseEnabled)
                If subsystem.ConcurrentSenseEnabled.HasValue Then Me._ConcurrentSenseCheckBox.SafeCheckedSetter(subsystem.ConcurrentSenseEnabled.Value)
                ' if concurrent sense changes, may no longer have concurrent mode.
                subsystem.QueryFunctionModes()
            Case NameOf(subsystem.PowerLineCycles)
                If subsystem.PowerLineCycles.HasValue Then Me._NplcNumeric.SafeValueSetter(CDec(subsystem.PowerLineCycles.Value))
            Case NameOf(subsystem.MeasurementAvailable)
                Me.onMeasurementAvailable(subsystem.Readings)
            Case NameOf(subsystem.SupportedFunctionModes)
                Me.onSupportedFunctionModesChanged(subsystem)
            Case NameOf(subsystem.FunctionModes)
                Me.onFunctionModesChanged(subsystem)
        End Select
    End Sub

    ''' <summary> Sense subsystem property changed. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Property changed event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub SenseSubsystemPropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)
        Try
            If sender IsNot Nothing AndAlso e IsNot Nothing Then
                Me.OnSubsystemPropertyChanged(TryCast(sender, SenseSubsystem), e.PropertyName)
            End If
        Catch ex As Exception
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId,
                               "Exception handling Sense Subsystem property changed Event;. Failed property {0}. Details: {1}",
                               e.PropertyName, ex)
        End Try
    End Sub

    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _ApplySenseFunctionButton_Click(sender As Object, e As EventArgs) Handles _ApplySenseFunctionButton.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()
            Me.Device.OutputSubsystem.WriteOutputOnState(False)
            Me.Device.SenseSubsystem.ApplyFunctionModes(Me.selectedFunctionMode)
            Me.Device.SenseSubsystem.ApplyConcurrentSenseEnabled(Me._ConcurrentSenseCheckBox.Checked)
            Me.Device.SystemSubsystem.ApplyFourWireSenseEnabled(Me._FourWireSenseCheckBox.Checked)
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, "Failed applying source function")
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, "Exception occurred applying source function;. Details: {0}", ex)
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

#Region " CONTROL EVENT HANDLERS: SENSE "

    ''' <summary>
    ''' Gets or sets the selected function mode.
    ''' </summary>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(False)>
    Private ReadOnly Property selectedFunctionMode() As VI.Scpi.SenseFunctionModes
        Get
            Return CType(CType(Me._SenseFunctionComboBox.SelectedItem, System.Collections.Generic.KeyValuePair(
                  Of [Enum], String)).Key, VI.Scpi.SenseFunctionModes)
        End Get
    End Property

    ''' <summary> Handles the function modes changed action. </summary>
    ''' <param name="value"> The <see cref="TraceMessage">message</see> to display and
    ''' log. </param>
    Private Sub OnSelectedFunctionChanged(ByVal value As VI.Scpi.SenseFunctionModes)
        Me._SenseFunctionComboBox.SelectedItem = value
        Dim unit As Arebis.TypedUnits.Unit = Arebis.StandardUnits.ElectricUnits.Volt
        Select Case value
            Case VI.Scpi.SenseFunctionModes.Current
                unit = Arebis.StandardUnits.ElectricUnits.Ampere
                With Me._SenseRangeNumeric
                    .Minimum = 0
                    .Maximum = 10D
                    .DecimalPlaces = 6
                    .Increment = 0.001D
                End With
                With Device.SenseCurrentSubsystem
                    .QueryPowerLineCycles()
                    .QueryAutoRangeEnabled()
                    .QueryProtectionLevel()
                    .QueryRange()
                End With
            Case VI.Scpi.SenseFunctionModes.Voltage
                unit = Arebis.StandardUnits.ElectricUnits.Volt
                With Me._SenseRangeNumeric
                    .Minimum = 0
                    .Maximum = 1100D
                    .DecimalPlaces = 3
                    .Increment = 1D
                End With
                With Device.SenseVoltageSubsystem
                    .QueryPowerLineCycles()
                    .QueryAutoRangeEnabled()
                    .QueryProtectionLevel()
                    .QueryRange()
                End With
            Case VI.Scpi.SenseFunctionModes.Resistance
                unit = Arebis.StandardUnits.ElectricUnits.Ohm
                With Me._SenseRangeNumeric
                    .Minimum = 0
                    .Maximum = 1000000000D
                    .DecimalPlaces = 0
                    .Increment = 1000D
                End With
                With Device.SenseResistanceSubsystem
                    .QueryPowerLineCycles()
                    .QueryAutoRangeEnabled()
                    .QueryRange()
                End With
        End Select
        Me._SenseRangeNumericLabel.Text = String.Format(Globalization.CultureInfo.CurrentCulture, "Range [{0}]:", unit.Symbol)
        Me._SenseRangeNumericLabel.Left = Me._SenseRangeNumeric.Left - Me._SenseRangeNumericLabel.Width
    End Sub

    ''' <summary> Event handler. Called by _SenseFunctionComboBox for selected index changed
    ''' events. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId:="degC")>
    Private Sub _SenseFunctionComboBox_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles _SenseFunctionComboBox.SelectedIndexChanged
        If Me._InitializingComponents Then Return
        Me.OnSelectedFunctionChanged(Me.selectedFunctionMode)
    End Sub

    ''' <summary>
    ''' Applies the selected measurements settings.
    ''' </summary>
    Private Sub applySenseSettings(ByVal value As VI.Scpi.SenseFunctionModes)

        Me.Device.ClearExecutionState()

        ' make sure output is off.
        Me.Device.OutputSubsystem.WriteOutputOnState(False)

        If value = VI.Scpi.SenseFunctionModes.CurrentDC Then

            With Me.Device.SenseCurrentSubsystem
                .ApplyPowerLineCycles(Me._NplcNumeric.Value)
                .ApplyAutoRangeEnabled(Me._SenseAutoRangeToggle.Checked)
                If Not Me._SenseAutoRangeToggle.Checked Then .ApplyRange(Me._SenseRangeNumeric.Value)
            End With

        ElseIf value = VI.Scpi.SenseFunctionModes.Resistance Then

            With Me.Device.SenseResistanceSubsystem
                .ApplyPowerLineCycles(Me._NplcNumeric.Value)
                .ApplyAutoRangeEnabled(Me._SenseAutoRangeToggle.Checked)
                If Not Me._SenseAutoRangeToggle.Checked Then .ApplyRange(Me._SenseRangeNumeric.Value)
            End With

        ElseIf value = VI.Scpi.SenseFunctionModes.VoltageDC Then

            With Me.Device.SenseVoltageSubsystem
                .ApplyPowerLineCycles(Me._NplcNumeric.Value)
                .ApplyAutoRangeEnabled(Me._SenseAutoRangeToggle.Checked)
                If Not Me._SenseAutoRangeToggle.Checked Then .ApplyRange(Me._SenseRangeNumeric.Value)
            End With

        End If

    End Sub

    ''' <summary> Event handler. Called by ApplySenseSettingsButton for click events. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _ApplySenseSettingsButton_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles _ApplySenseSettingsButton.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()
            Me.applySenseSettings(Me.selectedFunctionMode)
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.ToString)
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, "Exception occurred applying sense settings;. Details: {0}", ex)
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

#End Region

#End Region

#Region " SENSE VOLTAGE "

    ''' <summary> Handle the Sense subsystem property changed event. </summary>
    ''' <param name="subsystem">    The subsystem. </param>
    ''' <param name="propertyName"> Name of the property. </param>
    Private Sub OnSubsystemPropertyChanged(ByVal subsystem As SenseVoltageSubsystem, ByVal propertyName As String)
        If subsystem Is Nothing OrElse String.IsNullOrWhiteSpace(propertyName) Then Return

        ' apply only if using voltage sense.
        If Me.Device.SenseSubsystem.FunctionMode.GetValueOrDefault(Scpi.SenseFunctionModes.None) = Scpi.SenseFunctionModes.Voltage Then
            Select Case propertyName
                Case NameOf(subsystem.AutoRangeEnabled)
                    If subsystem.AutoRangeEnabled.HasValue Then Me._SenseAutoRangeToggle.SafeCheckedSetter(subsystem.AutoRangeEnabled.Value)
                Case NameOf(subsystem.PowerLineCycles)
                    If subsystem.PowerLineCycles.HasValue Then Me._NplcNumeric.SafeValueSetter(subsystem.PowerLineCycles.Value)
                Case NameOf(subsystem.Range)
                    If subsystem.Range.HasValue Then Me._SenseRangeNumeric.SafeValueSetter(subsystem.Range.Value)
            End Select
        End If

        ' protection applies only if using a current source.
        If Me.Device.SourceSubsystem.FunctionMode.GetValueOrDefault(SourceFunctionModes.None) = SourceFunctionModes.Current Then
            Select Case propertyName
                Case NameOf(subsystem.ProtectionLevel)
                    If subsystem.ProtectionLevel.HasValue Then Me._SourceLimitNumeric.SafeValueSetter(subsystem.ProtectionLevel.Value)
            End Select
        End If

    End Sub

    ''' <summary> Sense voltage subsystem property changed. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Property changed event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub SenseVoltageSubsystemPropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)
        Try
            If sender IsNot Nothing AndAlso e IsNot Nothing Then
                Me.OnSubsystemPropertyChanged(TryCast(sender, SenseVoltageSubsystem), e.PropertyName)
            End If
        Catch ex As Exception
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId,
                               "Exception handling Sense Voltage Subsystem property changed Event;. Failed property {0}. Details: {1}",
                               e.PropertyName, ex)
        End Try
    End Sub

#End Region

#Region " SENSE CURRENT "

    ''' <summary> Handle the Sense subsystem property changed event. </summary>
    ''' <param name="subsystem">    The subsystem. </param>
    ''' <param name="propertyName"> Name of the property. </param>
    Private Sub OnSubsystemPropertyChanged(ByVal subsystem As SenseCurrentSubsystem, ByVal propertyName As String)
        If subsystem Is Nothing OrElse String.IsNullOrWhiteSpace(propertyName) Then Return

        ' apply only if using current sense.
        If Me.Device.SenseSubsystem.FunctionMode.GetValueOrDefault(Scpi.SenseFunctionModes.None) = Scpi.SenseFunctionModes.Current Then
            Select Case propertyName
                Case NameOf(subsystem.AutoRangeEnabled)
                    If subsystem.AutoRangeEnabled.HasValue Then Me._SenseAutoRangeToggle.SafeCheckedSetter(subsystem.AutoRangeEnabled.Value)
                Case NameOf(subsystem.PowerLineCycles)
                    If subsystem.PowerLineCycles.HasValue Then Me._NplcNumeric.SafeValueSetter(subsystem.PowerLineCycles.Value)
                Case NameOf(subsystem.Range)
                    If subsystem.Range.HasValue Then Me._SenseRangeNumeric.SafeValueSetter(subsystem.Range.Value)
            End Select
        End If

        ' protection applies only if using a voltage source.
        If Me.Device.SourceSubsystem.FunctionMode.GetValueOrDefault(SourceFunctionModes.None) = SourceFunctionModes.Voltage Then
            Select Case propertyName
                Case NameOf(subsystem.ProtectionLevel)
                    If subsystem.ProtectionLevel.HasValue Then Me._SourceLimitNumeric.SafeValueSetter(subsystem.ProtectionLevel.Value)
            End Select
        End If

    End Sub

    ''' <summary> Sense Current subsystem property changed. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Property changed event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub SenseCurrentSubsystemPropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)
        Try
            If sender IsNot Nothing AndAlso e IsNot Nothing Then
                Me.OnSubsystemPropertyChanged(TryCast(sender, SenseCurrentSubsystem), e.PropertyName)
            End If
        Catch ex As Exception
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId,
                               "Exception handling Sense Current Subsystem property changed Event;. Failed property {0}. Details: {1}",
                               e.PropertyName, ex)
        End Try
    End Sub

#End Region

#Region " SENSE RESISTANCE "

    ''' <summary> Handle the Sense subsystem property changed event. </summary>
    ''' <param name="subsystem">    The subsystem. </param>
    ''' <param name="propertyName"> Name of the property. </param>
    Private Sub OnSubsystemPropertyChanged(ByVal subsystem As SenseResistanceSubsystem, ByVal propertyName As String)
        If subsystem Is Nothing OrElse String.IsNullOrWhiteSpace(propertyName) Then Return

        ' apply only if using current sense.
        If Me.Device.SenseSubsystem.FunctionMode.GetValueOrDefault(Scpi.SenseFunctionModes.None) = Scpi.SenseFunctionModes.Resistance Then
            Select Case propertyName
                Case NameOf(subsystem.AutoRangeEnabled)
                    If subsystem.AutoRangeEnabled.HasValue Then
                        Me._SenseAutoRangeToggle.SafeCheckedSetter(subsystem.AutoRangeEnabled.Value)
                    End If
                Case NameOf(subsystem.PowerLineCycles)
                    If subsystem.PowerLineCycles.HasValue Then
                        Me._NplcNumeric.SafeValueSetter(subsystem.PowerLineCycles.Value)
                    End If
                Case NameOf(subsystem.Range)
                    If subsystem.Range.HasValue Then
                        Me._SenseRangeNumeric.SafeValueSetter(subsystem.Range.Value)
                    End If
            End Select
        End If

    End Sub

    ''' <summary> Sense Four Wire Resistance subsystem property changed. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Property changed event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub SenseResistanceSubsystemPropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)
        Try
            If sender IsNot Nothing AndAlso e IsNot Nothing Then
                Me.OnSubsystemPropertyChanged(TryCast(sender, SenseResistanceSubsystem), e.PropertyName)
            End If
        Catch ex As Exception
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId,
                               "Exception handling Sense Resistance Subsystem property changed Event;. Failed property {0}. Details: {1}",
                               e.PropertyName, ex)
        End Try
    End Sub

#End Region

#Region " SOURCE "

    ''' <summary> Handles the supported function modes changed action. </summary>
    ''' <param name="subsystem"> The subsystem. </param>
    Private Sub onSupportedFunctionModesChanged(ByVal subsystem As SourceSubsystem)
        If subsystem IsNot Nothing AndAlso subsystem.SupportedFunctionModes <> VI.SourceFunctionModes.None Then
            With Me._SourceFunctionComboBox
                .DataSource = Nothing
                .Items.Clear()
                .DataSource = GetType(VI.SourceFunctionModes).ValueDescriptionPairs(subsystem.SupportedFunctionModes)
                .DisplayMember = "Value"
                .ValueMember = "Key"
                If .Items.Count > 0 Then
                    .SelectedItem = VI.SourceFunctionModes.Voltage.ValueDescriptionPair()
                End If
            End With
        End If
    End Sub

    ''' <summary> Gets or sets the selected source function mode. </summary>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(False)>
    Private ReadOnly Property SelectedSourceFunctionMode() As VI.SourceFunctionModes
        Get
            Return CType(CType(Me._SourceFunctionComboBox.SelectedItem, System.Collections.Generic.KeyValuePair(Of [Enum], String)).Key, VI.SourceFunctionModes)
        End Get
    End Property

    Private Sub OnSourceFunctionChanged(ByVal subsystem As SourceSubsystem)
        If subsystem Is Nothing Then Return
        Dim unit As Arebis.TypedUnits.Unit = Arebis.StandardUnits.ElectricUnits.Volt
        Dim range As RangeR = New RangeR(-1, 1)
        Dim decimalPlaces As Integer = 3
        Dim increment As Decimal = 0
        Dim limitUnit As Arebis.TypedUnits.Unit = Arebis.StandardUnits.ElectricUnits.Ampere
        Dim limitRange As RangeR = New RangeR(-1, 1)
        Dim limitDecimalPlaces As Integer = 3
        Dim LimitIncrement As Decimal = 0
        Select Case subsystem.FunctionMode.Value
            Case SourceFunctionModes.Current
                unit = Arebis.StandardUnits.ElectricUnits.Ampere
                range = Me.Device.SourceCurrentSubsystem.LevelRange
                decimalPlaces = 6
                increment = 0.0001D
                limitUnit = Arebis.StandardUnits.ElectricUnits.Volt
                limitRange = Me.Device.SourceVoltageSubsystem.LevelRange
                limitDecimalPlaces = 3
                LimitIncrement = 1
            Case SourceFunctionModes.Voltage
                range = Me.Device.SourceVoltageSubsystem.LevelRange
                decimalPlaces = 3
                limitRange = Me.Device.SourceCurrentSubsystem.LevelRange
                limitDecimalPlaces = 6
                increment = 1D
                LimitIncrement = 0.0001D
        End Select
        With Me._SourceLevelNumeric
            .Maximum = CDec(range.Max)
            .Minimum = CDec(range.Min)
            .DecimalPlaces = decimalPlaces
            .Increment = increment
        End With
        With Me._SourceRangeNumeric
            .Maximum = CDec(range.Max)
            .Minimum = CDec(range.Min)
            .DecimalPlaces = decimalPlaces
            .Increment = increment
        End With
        With Me._SourceLimitNumeric
            .Maximum = CDec(limitRange.Max)
            .Minimum = CDec(limitRange.Min)
            .DecimalPlaces = limitDecimalPlaces
            .Increment = LimitIncrement
        End With
        Me._SourceLevelNumericLabel.Text = $"Level [{unit.Symbol}]"
        Me._SourceLevelNumericLabel.Left = Me._SourceLevelNumeric.Left - Me._SourceLevelNumericLabel.Width
        Me._SourceRangeNumericLabel.Text = $"Range [{unit.Symbol}]"
        Me._SourceRangeNumericLabel.Left = Me._SourceRangeNumeric.Left - Me._SourceRangeNumericLabel.Width
        Me._SourceLimitNumericLabel.Text = $"Limit [{limitUnit.Symbol}]"
        Me._SourceLimitNumericLabel.Left = Me._SourceLimitNumeric.Left - Me._SourceLimitNumericLabel.Width

        Select Case subsystem.FunctionMode.Value
            Case SourceFunctionModes.Current
                With Me.Device.SourceCurrentSubsystem
                    .QueryLevel()
                    .QueryRange()
                End With
            Case SourceFunctionModes.Voltage
                With Me.Device.SourceVoltageSubsystem
                    .QueryLevel()
                    .QueryRange()
                End With
        End Select
    End Sub

    ''' <summary> Handle the Source subsystem property changed event. </summary>
    ''' <param name="subsystem">    The subsystem. </param>
    ''' <param name="propertyName"> Name of the property. </param>
    Private Sub OnSubsystemPropertyChanged(ByVal subsystem As SourceSubsystem, ByVal propertyName As String)
        If subsystem Is Nothing OrElse String.IsNullOrWhiteSpace(propertyName) Then Return
        ' Me._SourceRangeTextBox.SafeTextSetter(Me.Device.SourceRange(VI.ResourceAccessLevels.Cache).ToString(Globalization.CultureInfo.CurrentCulture))
        ' Me._integrationPeriodTextBox.SafeTextSetter(Me.Device.SourceIntegrationPeriodCaption)
        Select Case propertyName
            Case NameOf(subsystem.AutoClearEnabled)
                If subsystem.AutoClearEnabled.HasValue Then
                    Me._SourceAutoClearEnabledMenuItem.SafeCheckedSetter(subsystem.AutoClearEnabled.Value)
                End If
            Case NameOf(subsystem.Delay)
                If subsystem.Delay.HasValue Then
                    Me._SourceDelayNumeric.SafeValueSetter(subsystem.Delay.Value.Ticks / TimeSpan.TicksPerMillisecond)
                End If
            Case NameOf(subsystem.FunctionMode)
                If subsystem.FunctionMode.HasValue Then
                    Me._SourceFunctionComboBox.SelectedItem = subsystem.FunctionMode.Value.ValueDescriptionPair
                    Me.OnSourceFunctionChanged(subsystem)
                End If
            Case NameOf(subsystem.SupportedFunctionModes)
                Me.onSupportedFunctionModesChanged(subsystem)
        End Select
    End Sub

    ''' <summary> Source subsystem property changed. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Property changed event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub SourceSubsystemPropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)
        Try
            If sender IsNot Nothing AndAlso e IsNot Nothing Then
                Me.OnSubsystemPropertyChanged(TryCast(sender, SourceSubsystem), e.PropertyName)
            End If
        Catch ex As Exception
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId,
                               "Exception handling Source Subsystem property changed Event;. Failed property {0}. Details: {1}",
                               e.PropertyName, ex)
        End Try
    End Sub

    ''' <summary> Source automatic clear enabled menu item check state changed. </summary>
    ''' <remarks> David, 3/7/2016. </remarks>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Event information. </param>
    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _SourceAutoClearEnabledMenuItem_CheckStateChanged(sender As Object, e As EventArgs) Handles _SourceAutoClearEnabledMenuItem.CheckStateChanged
        If Me._InitializingComponents Then Return
        Me.ErrorProvider.Clear()
        Dim menuItem As ToolStripMenuItem = TryCast(sender, ToolStripMenuItem)
        Try
            If Not Me.Device.SourceSubsystem.AutoClearEnabled.HasValue OrElse
                Me.Device.SourceSubsystem.AutoClearEnabled.Value <> menuItem.Checked Then
                Me.Device.SourceSubsystem.ApplyAutoClearEnabled(menuItem.Checked)
            End If
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, "Exception occurred setting output auto on mode")
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, "Exception occurred setting output auto on mode;. Details: {0}", ex)
        End Try
    End Sub

    ''' <summary> Handle the Source Voltage subsystem property changed event. </summary>
    ''' <param name="subsystem">    The subsystem. </param>
    ''' <param name="propertyName"> Name of the property. </param>
    Private Sub OnSubsystemPropertyChanged(ByVal subsystem As SourceVoltageSubsystem, ByVal propertyName As String)
        If subsystem Is Nothing OrElse String.IsNullOrWhiteSpace(propertyName) Then Return
        If Me.Device.SourceSubsystem.FunctionMode.GetValueOrDefault(SourceFunctionModes.None) <> SourceFunctionModes.Voltage Then Return
        Select Case propertyName
            Case NameOf(subsystem.Level)
                If subsystem.Level.HasValue Then Me._SourceLevelNumeric.SafeValueSetter(subsystem.Level.Value)
            Case NameOf(subsystem.Range)
                If subsystem.Range.HasValue Then Me._SourceRangeNumeric.SafeValueSetter(subsystem.Range.Value)
        End Select
    End Sub

    ''' <summary> Source Voltage subsystem property changed. </summary>
    ''' <param name="sender"> Source Voltage of the event. </param>
    ''' <param name="e">      Property changed event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub SourceVoltageSubsystemPropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)
        Try
            If sender IsNot Nothing AndAlso e IsNot Nothing Then
                Me.OnSubsystemPropertyChanged(TryCast(sender, SourceVoltageSubsystem), e.PropertyName)
            End If
        Catch ex As Exception
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId,
                               "Exception handling Source Voltage Subsystem property changed Event;. Failed property {0}. Details: {1}",
                               e.PropertyName, ex)
        End Try
    End Sub

    ''' <summary> Handle the Source Current subsystem property changed event. </summary>
    ''' <param name="subsystem">    The subsystem. </param>
    ''' <param name="propertyName"> Name of the property. </param>
    Private Sub OnSubsystemPropertyChanged(ByVal subsystem As SourceCurrentSubsystem, ByVal propertyName As String)
        If subsystem Is Nothing OrElse String.IsNullOrWhiteSpace(propertyName) Then Return
        If Me.Device.SourceSubsystem.FunctionMode.GetValueOrDefault(SourceFunctionModes.None) <> SourceFunctionModes.Current Then Return
        Select Case propertyName
            Case NameOf(subsystem.Level)
                If subsystem.Level.HasValue Then Me._SourceLevelNumeric.SafeValueSetter(subsystem.Level.Value)
            Case NameOf(subsystem.Range)
                If subsystem.Range.HasValue Then Me._SourceRangeNumeric.SafeValueSetter(subsystem.Range.Value)
        End Select
    End Sub

    ''' <summary> Source Current subsystem property changed. </summary>
    ''' <param name="sender"> Source Current of the event. </param>
    ''' <param name="e">      Property changed event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub SourceCurrentSubsystemPropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)
        Try
            If sender IsNot Nothing AndAlso e IsNot Nothing Then
                Me.OnSubsystemPropertyChanged(TryCast(sender, SourceCurrentSubsystem), e.PropertyName)
            End If
        Catch ex As Exception
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId,
                               "Exception handling Source Current Subsystem property changed Event;. Failed property {0}. Details: {1}",
                               e.PropertyName, ex)
        End Try
    End Sub

    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _ApplySourceFunctionButton_Click(sender As Object, e As EventArgs) Handles _ApplySourceFunctionButton.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()
            Me.Device.ClearExecutionState()

            ' make sure output is off.
            Me.Device.OutputSubsystem.WriteOutputOnState(False)

            Me.Device.SourceSubsystem.ApplyFunctionMode(Me.SelectedSourceFunctionMode)
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, "Failed applying source function")
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, "Exception occurred applying source function;. Details: {0}", ex)
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _applySourceSettingButton_Click(sender As Object, e As EventArgs) Handles _ApplySourceSettingButton.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()
            Me.Device.ClearExecutionState()

            ' make sure output is off.
            Me.Device.OutputSubsystem.WriteOutputOnState(False)

            ' get the delay time
            If Me._TriggerDelayNumeric.Value >= 0 Then
                Me.Device.TriggerSubsystem.ApplyDelay(TimeSpan.FromTicks(CLng(TimeSpan.TicksPerSecond * Me._TriggerDelayNumeric.Value)))
            Else
                Me.Device.TriggerSubsystem.ApplyAutoDelayEnabled(True)
            End If

            Me.Device.SourceSubsystem.ApplyDelay(TimeSpan.FromMilliseconds(Me._SourceDelayNumeric.Value))
            Select Case Me.Device.SourceSubsystem.FunctionMode.Value
                Case SourceFunctionModes.Current
                    With Me.Device.SourceCurrentSubsystem
                        .ApplyRange(1.1 * Me._SourceLevelNumeric.Value)
                        .ApplyLevel(Me._SourceLevelNumeric.Value)
                    End With
                    With Me.Device.SenseVoltageSubsystem
                        .ApplyProtectionLevel(Me._SourceLimitNumeric.Value)
                    End With
                Case SourceFunctionModes.Voltage
                    With Me.Device.SourceVoltageSubsystem
                        .ApplyRange(1.1 * Me._SourceLevelNumeric.Value)
                        .ApplyLevel(Me._SourceLevelNumeric.Value)
                    End With
                    With Me.Device.SenseCurrentSubsystem
                        .ApplyProtectionLevel(Me._SourceLimitNumeric.Value)
                    End With
            End Select
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, "Failed applying source settings")
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, "Exception occurred applying source settings;. Details: {0}", ex)
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

#End Region

#Region " STATUS "

    ''' <summary> Reports the last error. </summary>
    Private Sub onLastError(ByVal lastError As DeviceError)
        If lastError?.IsError Then
            Me._LastErrorTextBox.ForeColor = Drawing.Color.OrangeRed
        Else
            Me._LastErrorTextBox.ForeColor = Drawing.Color.Aquamarine
        End If
        Me._LastErrorTextBox.Text = lastError.CompoundErrorMessage
    End Sub

    ''' <summary> Handle the Status subsystem property changed event. </summary>
    ''' <param name="subsystem">    The subsystem. </param>
    ''' <param name="propertyName"> Name of the property. </param>
    Protected Overrides Sub OnPropertyChanged(ByVal subsystem As StatusSubsystemBase, ByVal propertyName As String)
        If subsystem Is Nothing OrElse String.IsNullOrWhiteSpace(propertyName) Then Return
        MyBase.OnPropertyChanged(subsystem, propertyName)
        Select Case propertyName
            Case NameOf(subsystem.DeviceErrors)
                onLastError(subsystem.LastDeviceError)
            Case NameOf(subsystem.LastDeviceError)
                onLastError(subsystem.LastDeviceError)
            Case NameOf(subsystem.ErrorAvailable)
                If Not subsystem.ReadingDeviceErrors Then
                    ' if no errors, this clears the error queue.
                    subsystem.QueryDeviceErrors()
                End If
        End Select
    End Sub

    ''' <summary> Status subsystem property changed. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Property changed event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub StatusSubsystemPropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)
        Try
            Me.OnPropertyChanged(TryCast(sender, StatusSubsystem), e?.PropertyName)
        Catch ex As Exception
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId,
                               "Exception handling property '{0}' changed event;. Details: {1}", e.PropertyName, ex)
        End Try
    End Sub

    ''' <summary> Reads a service request status. </summary>
    ''' <remarks> David, 12/26/2015. </remarks>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Public Sub ReadServiceRequestStatus()
        Try
            Me.Device.StatusSubsystem.ReadServiceRequestStatus()
        Catch ex As Exception
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, "Exception reading service request;. Details: {0}", ex)
        End Try
    End Sub

#End Region

#Region " SYSTEM "

    ''' <summary> Contact check enabled menu item check state changed. </summary>
    ''' <remarks> David, 3/7/2016. </remarks>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Event information. </param>
    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _ContactCheckEnabledMenuItem_CheckStateChanged(sender As Object, e As EventArgs) Handles _ContactCheckEnabledMenuItem.CheckStateChanged
        If Me._InitializingComponents Then Return
        Me.ErrorProvider.Clear()
        Dim menuItem As ToolStripMenuItem = TryCast(sender, ToolStripMenuItem)
        Try
            If Not Me.Device.SystemSubsystem.ContactCheckEnabled.HasValue OrElse Me.Device.SystemSubsystem.ContactCheckEnabled.Value <> menuItem.Checked Then
                Me._ContactCheckEnabledMenuItem.SafeSilentCheckedSetter(Me.Device.SystemSubsystem.ContactCheckEnabled.Value)
            End If
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, "Exception occurred enabling contact check")
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, "Exception occurred enabling contact check;. Details: {0}", ex)
        End Try
    End Sub

    ''' <summary> Handle the System subsystem property changed event. </summary>
    ''' <param name="subsystem">    The subsystem. </param>
    ''' <param name="propertyName"> Name of the property. </param>
    Private Sub OnSubsystemPropertyChanged(ByVal subsystem As SystemSubsystem, ByVal propertyName As String)
        If subsystem Is Nothing OrElse String.IsNullOrWhiteSpace(propertyName) Then Return
        Select Case propertyName
            Case NameOf(subsystem.ContactCheckEnabled)
                If subsystem.ContactCheckEnabled.HasValue Then Me._ContactCheckEnabledMenuItem.SafeSilentCheckedSetter(subsystem.ContactCheckEnabled.Value)
            Case NameOf(subsystem.FrontSwitched)
                Me._OutputTerminalMenuItem.SafeSilentCheckStateSetter((Not subsystem.FrontSwitched).ToCheckState)
            Case NameOf(subsystem.FourWireSenseEnabled)
                If subsystem.FourWireSenseEnabled.HasValue Then Me._FourWireSenseCheckBox.SafeCheckedSetter(subsystem.FourWireSenseEnabled.Value)
            Case NameOf(subsystem.SupportsContactCheck)
                Me._ContactCheckToggle.SafeEnabledSetter(subsystem.SupportsContactCheck.GetValueOrDefault(False))
                Me._ContactCheckBitPatternNumeric.SafeEnabledSetter(subsystem.SupportsContactCheck.GetValueOrDefault(False))
        End Select
        Application.DoEvents()
    End Sub

    ''' <summary> System subsystem property changed. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Property changed event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub SystemSubsystemPropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)
        Try
            Me.OnSubsystemPropertyChanged(TryCast(sender, SystemSubsystem), e?.PropertyName)
        Catch ex As Exception
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId,
                               "Exception handling property '{0}' changed event;. Details: {1}", e.PropertyName, ex)
        End Try
    End Sub

#End Region

#Region " TRIGGER "

    ''' <summary> The device access locker. </summary>
    Private _DeviceAccessLocker As New Object

    Private WithEvents _MeterTimer As System.Windows.Forms.Timer

    ''' <summary>
    ''' Aborts the measurement cycle.
    ''' </summary>
    Private Sub _abortMeasurement()
        Me._MeterTimer.Enabled = False
        Me.Device.TriggerSubsystem.Abort()
    End Sub

    ''' <summary>
    ''' Aborts the measurement cycle.
    ''' </summary>
    Private Sub abortMeasurement()
        SyncLock _DeviceAccessLocker
            Me._abortMeasurement()
        End SyncLock
    End Sub

    ''' <summary>
    ''' Asserts a trigger to emulate triggering for timing measurements.
    ''' </summary>
    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _AssertTriggerToolStripButton_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles _AssertTriggerToolStripButton.Click
        Me.ErrorProvider.Clear()
        Try
            Dim mode As VI.ArmSources = Me.Device.ArmLayerSubsystem.ArmSource.GetValueOrDefault(ArmSources.None)
            If (mode = ArmSources.Manual) AndAlso Me._MeterTimer.Enabled Then
                Me.assertTrigger()
            ElseIf mode <> ArmSources.Manual Then
                Me.ErrorProvider.Annunciate(sender, $"Manual trigger ignored in '{mode.Description}' mode")
            ElseIf Not Me._MeterTimer.Enabled Then
                Me.ErrorProvider.Annunciate(sender, "Manual trigger ignored -- triggering is not active")
            End If
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, "Exception occurred aborting")
            My.Application.Log.WriteException(ex, TraceEventType.Error, "Exception occurred aborting.")
        End Try
    End Sub

    ''' <summary>
    ''' Outputs a trigger to make a measurement.
    ''' </summary>
    Private Sub assertTrigger()
        SyncLock _DeviceAccessLocker
            Me.Device.Session.AssertTrigger()
        End SyncLock
    End Sub

    Private _abortRequested As Boolean
    ''' <summary>
    ''' Turns on or aborts waiting for trigger.
    ''' </summary>
    <CodeAnalysis.SuppressMessage("Microsoft.Mobility", "CA1601:DoNotUseTimersThatPreventPowerStateChanges")>
    Private Sub toggleAwaitTrigger(ByVal enabled As Boolean, ByVal checked As Boolean)
        Me.ErrorProvider.SetIconPadding(Me._TriggerToolStrip, -10)
        Me.ErrorProvider.SetError(Me._TriggerToolStrip, "")
        If enabled Then
            Me._abortRequested = Not checked
            If checked Then
                SyncLock _DeviceAccessLocker
                    Me.Talker.Publish(TraceEventType.Verbose, My.MyLibrary.TraceEventId, "Preparing instrument for waiting for trigger;. ")
                    Me.TriggerAction = "INITIATING"
                    ' clear execution state before enabling events
                    Me.Device.ClearExecutionState()

                    ' set the service request
                    Me.Device.StatusSubsystem.ApplyMeasurementEventEnableBitmask(MeasurementEvents.All)
                    Me.Device.StatusSubsystem.EnableServiceRequest(StandardEvents.All And Not StandardEvents.RequestControl,
                                                                   ServiceRequests.StandardEvent Or ServiceRequests.OperationEvent)
                    Me.Device.ClearExecutionState()
                    Me.Device.TriggerSubsystem.Initiate()
                    Me.TriggerAction = "WAITING FOR TRIGGERED MEASUREMENT"
                    Me.Talker.Publish(TraceEventType.Verbose, My.MyLibrary.TraceEventId, "Monitoring instrument for measurements;. ")
                    Me._MeterTimer.Interval = 100
                    Me._MeterTimer.Enabled = True
                End SyncLock
            Else
                Me.TriggerAction = "ABORT REQUESTED"
                Me._abortMeasurement()
            End If
        End If
        If Me._AwaitTriggerToolStripButton.Checked Then
            Me._AwaitTriggerToolStripButton.Text = "ABORT TRIGGERING"
        Else
            Me._AwaitTriggerToolStripButton.Text = "WAIT FOR A TRIGGER"
        End If
    End Sub

    ''' <summary>
    ''' Turns on or aborts waiting for trigger.
    ''' </summary>
    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _AwaitTriggerToolStripButton_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles _AwaitTriggerToolStripButton.CheckedChanged
        If Me._InitializingComponents Then Return
        Me.ErrorProvider.Clear()
        Try
            Me.toggleAwaitTrigger(_AwaitTriggerToolStripButton.Enabled, _AwaitTriggerToolStripButton.Checked)
        Catch ex As Exception
            Me._abortMeasurement()
            Me.ErrorProvider.Annunciate(sender, "Exception occurred initiating trigger")
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, "Exception occurred initiating trigger;. Details: {0}", ex)
        End Try
    End Sub

    ''' <summary> Gets or sets the trigger action. </summary>
    ''' <value> The trigger action. </value>
    Private Property TriggerAction As String
        Get
            Return Me._TriggerActionToolStripLabel.Text
        End Get
        Set(value As String)
            If Not String.Equals(Me.TriggerAction, value) Then
                Me._TriggerActionToolStripLabel.Text = value
            End If
        End Set
    End Property

    ''' <summary> Gets the next status bar. </summary>
    ''' <value> The next status bar. </value>
    Private ReadOnly Property NextStatusBar As String
        Get
            Static bar As String = "|"
            If bar = "|" Then
                bar = "/"
            ElseIf bar = "/" Then
                bar = "-"
            ElseIf bar = "-" Then
                bar = "\"
            ElseIf bar = "\" Then
                bar = "|"
            End If
            Return bar
        End Get
    End Property

    ''' <summary>
    ''' Monitors measurements. Once found, reads and displays and restarts the cycle.
    ''' </summary>
    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _MeterTimer_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles _MeterTimer.Tick
        SyncLock _DeviceAccessLocker
            Try
                Me._MeterTimer.Enabled = False
                If Me._abortRequested Then
                    Me._abortMeasurement()
                    Return
                End If
                Me.Device.StatusSubsystem.ReadServiceRequestStatus()
                If Me.Device.StatusSubsystem.MeasurementAvailable Then
                    Me.TriggerAction = "READING..."

                    ' update display modalities if changed.
                    Me.Device.MeasureSubsystem.Read()
                    Me.TriggerAction = "DATA AVAILABLE..."

                    Me._TriggerActionToolStripLabel.Text = "PREPARING..."

                    ' clear execution state before enabling events
                    Me.Device.ClearExecutionState()

                    ' set the service request
                    Me.Device.StatusSubsystem.ApplyMeasurementEventEnableBitmask(MeasurementEvents.All)
                    Me.Device.StatusSubsystem.EnableServiceRequest(StandardEvents.All And Not StandardEvents.RequestControl,
                                                                   ServiceRequests.StandardEvent Or ServiceRequests.OperationEvent)
                    Me.Device.ClearExecutionState()
                    Me.Device.TriggerSubsystem.Initiate()
                    Me.TriggerAction = "WAITING FOR TRIGGRED MEASUREMENT..."
                    Me.Talker.Publish(TraceEventType.Verbose, My.MyLibrary.TraceEventId, "Monitoring instrument for measurements;. ")
                Else
                    Me.TriggerAction = "WAITING FOR TRIGGERED MEASUREMENT"
                    Me._WaitHourglassLabel.Text = Me.NextStatusBar
                End If
                Me._MeterTimer.Enabled = Not Me._abortRequested
            Catch ex As Exception
                Me.ErrorProvider.SetError(Me._TriggerToolStrip, "Exception occurred monitoring instrument for data")
                Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, "Exception occurred monitoring instrument for data;. Details: {0}", ex)
                Me._AwaitTriggerToolStripButton.Checked = False
            End Try
        End SyncLock
    End Sub

    ''' <summary> Handle the Trigger subsystem property changed event. </summary>
    ''' <param name="subsystem">    The subsystem. </param>
    ''' <param name="propertyName"> Name of the property. </param>
    Private Sub OnSubsystemPropertyChanged(ByVal subsystem As TriggerSubsystem, ByVal propertyName As String)
        If subsystem Is Nothing OrElse String.IsNullOrWhiteSpace(propertyName) Then Return
        Select Case propertyName
            Case NameOf(subsystem.Delay)
                If subsystem.Delay.HasValue Then Me._TriggerDelayNumeric.SafeValueSetter(subsystem.Delay.Value.TotalMilliseconds)
        End Select
    End Sub

    ''' <summary> Trigger subsystem property changed. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Property changed event inTriggerion. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub TriggerSubsystemPropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)
        Try
            If sender IsNot Nothing AndAlso e IsNot Nothing Then
                Me.OnSubsystemPropertyChanged(TryCast(sender, TriggerSubsystem), e.PropertyName)
            End If
        Catch ex As Exception
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId,
                               "Exception handling Trigger Subsystem property changed Event;. Failed property {0}. Details: {1}",
                               e.PropertyName, ex)
        End Try
    End Sub

    ''' <summary> Handle the Arm subsystem property changed event. </summary>
    ''' <param name="subsystem">    The subsystem. </param>
    ''' <param name="propertyName"> Name of the property. </param>
    Private Sub OnSubsystemPropertyChanged(ByVal subsystem As ArmLayerSubsystem, ByVal propertyName As String)
        If subsystem Is Nothing OrElse String.IsNullOrWhiteSpace(propertyName) Then Return
        Select Case propertyName
            Case NameOf(subsystem.InputLineNumber)
            Case NameOf(subsystem.ArmSource)
                Me._ArmSourceComboBox.SelectedItem = subsystem.ArmSource.Value.ValueDescriptionPair
            Case NameOf(subsystem.SupportedArmSources)
                If Me.Device IsNot Nothing AndAlso subsystem.SupportedArmSources <> VI.ArmSources.None Then
                    Dim selectedIndex As Integer = Me._ReadingComboBox.SelectedIndex
                    With Me._ArmSourceComboBox
                        .DataSource = Nothing
                        .Items.Clear()
                        .DataSource = GetType(VI.ArmSources).ValueDescriptionPairs(subsystem.SupportedArmSources)
                        .DisplayMember = "Value"
                        .ValueMember = "Key"
                        If .Items.Count > 0 Then
                            .SelectedIndex = Math.Max(selectedIndex, 0)
                        End If
                    End With
                End If
        End Select
    End Sub

    ''' <summary> Arm subsystem property changed. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Property changed event inArmion. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub ArmSubsystemPropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)
        Try
            If sender IsNot Nothing AndAlso e IsNot Nothing Then
                Me.OnSubsystemPropertyChanged(TryCast(sender, ArmLayerSubsystem), e.PropertyName)
            End If
        Catch ex As Exception
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId,
                               "Exception handling Arm Subsystem property changed Event;. Failed property {0}. Details: {1}",
                               e.PropertyName, ex)
        End Try
    End Sub

    ''' <summary> Gets or sets the selected arm source. </summary>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(False)>
    Private ReadOnly Property SelectedArmSource() As VI.ArmSources
        Get
            Return CType(CType(Me._ArmSourceComboBox.SelectedItem, System.Collections.Generic.KeyValuePair(Of [Enum], String)).Key, VI.ArmSources)
        End Get
    End Property

#End Region

#End Region

#Region " CONTROL EVENT HANDLERS: HIPOT "

    Private _isInsulationTestOwner As Boolean
    Private _InsulationTest As InsulationTest
    ''' <summary> Tests assign insulation. </summary>
    ''' <remarks> David, 3/10/2016. </remarks>
    <CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")>
    Public Sub AssignInsulationTest()
        Me._InsulationTest = New InsulationTest
        Me._isInsulationTestOwner = False
        Me.AssignInsulationTest(New InsulationTest)
        Me._isInsulationTestOwner = True
    End Sub

    ''' <summary> Tests assign insulation. </summary>
    ''' <remarks> David, 3/10/2016. </remarks>
    ''' <param name="value"> True to show or False to hide the control. </param>
    Public Sub AssignInsulationTest(ByVal value As InsulationTest)
        If Me._isInsulationTestOwner AndAlso Me._InsulationTest IsNot Nothing Then
            Me._InsulationTest.Dispose()
            Me._InsulationTest = Nothing
        End If
        Me._isInsulationTestOwner = False
        If value Is Nothing Then
            Me._BinningInfo = Nothing
            Me._ActiveInsulationResistance = Nothing
        Else
            Me._BinningInfo = value.Binning
            Me._ActiveInsulationResistance = value.Insulation
            value.Publish()
        End If
    End Sub

    Private WithEvents _BinningInfo As BinningInfo

    ''' <summary> Gets information describing the binning. </summary>
    ''' <value> Information describing the binning. </value>
    Public ReadOnly Property BinningInfo As BinningInfo
        Get
            Return Me._BinningInfo
        End Get
    End Property

    Private Overloads Sub OnPropertyChanged(ByVal sender As BinningInfo, ByVal propertyName As String)
        If sender Is Nothing OrElse String.IsNullOrWhiteSpace(propertyName) Then Return
        Select Case propertyName
            Case NameOf(sender.ArmSource)
                Me._ArmSourceComboBox.SafeSelectItem(sender.ArmSource.ValueDescriptionPair)
            Case NameOf(sender.PassBits)
                Me._PassBitPatternNumeric.SafeValueSetter(sender.PassBits)
            Case NameOf(sender.LowerLimit)
                Me._ResistanceLowLimitNumeric.SafeValueSetter(0.00001 * sender.LowerLimit)
            Case NameOf(sender.LowerLimitFailureBits)
                Me._FailBitPatternNumeric.SafeValueSetter(sender.LowerLimitFailureBits)
            Case NameOf(sender.StrobePulseWidth)
                Me._EotStrobeDurationNumeric.SafeValueSetter(sender.StrobePulseWidth.Ticks / TimeSpan.TicksPerMillisecond)
            Case NameOf(sender.FailureBits)
                Me._FailBitPatternNumeric.Value = sender.FailureBits
            Case NameOf(sender.UpperLimitFailureBits)
                Me._FailBitPatternNumeric.Value = sender.UpperLimitFailureBits
            Case NameOf(sender.LowerLimitFailureBits)
                Me._FailBitPatternNumeric.Value = sender.LowerLimitFailureBits
            Case NameOf(sender.LowerLimit)
                Me._ResistanceLowLimitNumeric.Value = CDec(0.000001 * sender.LowerLimit)
            Case NameOf(sender.PassBits)
                Me._PassBitPatternNumeric.Value = sender.PassBits
        End Select
    End Sub

    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _BinningInfo_PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Handles _BinningInfo.PropertyChanged
        Try
            If sender IsNot Nothing AndAlso e IsNot Nothing Then
                Me.OnPropertyChanged(TryCast(sender, BinningInfo), e.PropertyName)
            End If
        Catch ex As Exception
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId,
                               "Exception handling the binning info property changed Event;. Failed property {0}. Details: {1}",
                               e.PropertyName, ex)
        End Try
    End Sub

    Private WithEvents _ActiveInsulationResistance As InsulationResistance

    ''' <summary> Gets or sets the active limit. </summary>
    ''' <value> The active limit. </value>
    Public ReadOnly Property ActiveInsulationResistance As InsulationResistance
        Get
            Return Me._ActiveInsulationResistance
        End Get
    End Property

    Private Overloads Sub OnPropertyChanged(ByVal sender As InsulationResistance, ByVal propertyName As String)
        If sender Is Nothing OrElse String.IsNullOrWhiteSpace(propertyName) Then Return
        Select Case propertyName
            Case NameOf(sender.ContactCheckEnabled)
                Me._ContactCheckToggle.SafeCheckedSetter(sender.ContactCheckEnabled)
            Case NameOf(sender.CurrentLimit)
                Me._CurrentLimitNumeric.SafeValueSetter(0.00001 * sender.CurrentLimit)
            Case NameOf(sender.DwellTime)
            Case NameOf(sender.PowerLineCycles)
                Me._ApertureNumeric.SafeValueSetter(sender.PowerLineCycles)
            Case NameOf(sender.ResistanceLowLimit)
                Me._ResistanceLowLimitNumeric.SafeValueSetter(0.00001 * sender.ResistanceLowLimit)
            Case NameOf(sender.ResistanceRange)
                Me._ResistanceRangeNumeric.SafeValueSetter(0.000001 * sender.ResistanceRange)
            Case NameOf(sender.VoltageLevel)
                Me._VoltageLevelNumeric.SafeValueSetter(sender.VoltageLevel)
        End Select
    End Sub

    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _ActiveInsulationResistance_PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Handles _ActiveInsulationResistance.PropertyChanged
        Try
            If sender IsNot Nothing AndAlso e IsNot Nothing Then
                Me.OnPropertyChanged(TryCast(sender, InsulationResistance), e.PropertyName)
            End If
        Catch ex As Exception
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId,
                               "Exception handling insulation resistance property changed Event;. Failed property {0}. Details: {1}",
                               e.PropertyName, ex)
        End Try
    End Sub

    ''' <summary> Tests configure hipot start. </summary>
    ''' <remarks> David, 3/10/2016. </remarks>
    ''' <exception cref="ArgumentNullException">    Thrown when one or more required arguments are
    '''                                             null. </exception>
    ''' <exception cref="OperationFailedException"> Thrown when operation failed to execute. </exception>
    ''' <param name="binning">    The binning info. </param>
    ''' <param name="insulation"> The insulation info. </param>
    '''
    ''' ### <exception cref="ArgumentException"> Thrown when one or more arguments have unsupported or
    '''                                          illegal values. </exception>
    Private Sub ConfigureHipotStartTest(ByVal binning As BinningInfo,
                                        ByVal insulation As InsulationResistance)

        If binning Is Nothing Then Throw New ArgumentNullException(NameOf(binning))
        If insulation Is Nothing Then Throw New ArgumentNullException(NameOf(insulation))

        Me.ErrorProvider.Clear()
        Me.Device.ClearExecutionState()
        Me.Device.OutputSubsystem.WriteOutputOnState(False)

        ' ======
        ' CALC2

        ' default: Me.Session.Write(":CALC2:CLIM:BCON IMM")
        ' Me.Session.Write(":CALC2:CLIM:CLE")
        Me.Device.CompositeLimit.ClearLimits()

        ' default: Me.Session.Write(":CALC2:CLIM:CLE:AUTO ON")
        Me.Device.CompositeLimit.ApplyAutoClearEnabled(True)

        ' default: Me.Session.Write(":CALC2:CLIM:MODE GRAD")
        Me.Device.CompositeLimit.ApplyLimitMode(LimitMode.Grading)

        ' Make limit comparisons on resistance reading
        ' Me.Session.Write(":CALC2:FEED RES")
        Me.Device.Calculate2Subsystem.ApplyFeedSource(Scpi.FeedSource.Resistance)

        ' Compliance Limit: send BAD code if measurement is in compliance
        ' default: Me.Session.Write(":CALC2:LIM1:COMP:FAIL IN")
        Me.Device.ComplianceLimit.ApplyIncomplianceCondition(True)

        ' Set fail bin
        ' Me.Session.Write(String.Format(Globalization.CultureInfo.InvariantCulture, ":CALC2:LIM1:COMP:SOUR2 {0}", Me._FailBitPatternNumeric.Value))
        Me.Device.ComplianceLimit.ApplyFailureBits(binning.FailureBits)
        binning.FailureBits = Me.Device.ComplianceLimit.FailureBits.GetValueOrDefault(0)

        ' Me.Session.Write(":CALC2:LIM1:STAT 1")
        Me.Device.ComplianceLimit.ApplyEnabled(binning.Enabled)
        binning.Enabled = Me.Device.ComplianceLimit.Enabled.GetValueOrDefault(False)

        ' Lower Limit
        With Me.Device.UpperLowerLimit
            ' Me.Session.Write(":CALC2:LIM2:UPP 1E+19")
            .ApplyUpperLimit(binning.UpperLimit)
            binning.UpperLimit = .UpperLimit.GetValueOrDefault(0)
            ' ":CALC2:LIM2:UPP:SOUR2 {0}", Me._FailBitPatternNumeric.Value
            .ApplyUpperLimitFailureBits(binning.UpperLimitFailureBits)
            binning.UpperLimitFailureBits = .UpperLimitFailureBits.GetValueOrDefault(0)
            ' ":CALC2:LIM2:LOW {0}", Me._ResistanceLowLimitNumeric.Value
            .ApplyLowerLimit(binning.LowerLimit)
            binning.LowerLimit = .LowerLimit.GetValueOrDefault(0)
            ' ":CALC2:LIM2:LOW:SOUR2 {0}", Me._FailBitPatternNumeric.Value))
            .ApplyLowerLimitFailureBits(binning.LowerLimitFailureBits)
            binning.LowerLimitFailureBits = .LowerLimitFailureBits.GetValueOrDefault(0)
            ' Me.Session.Write(":CALC2:LIM2:STAT 1")
            .ApplyEnabled(binning.Enabled)
            binning.Enabled = .Enabled.GetValueOrDefault(False)
        End With

        ' Send GOOD code if measurement is in range
        With Device.CompositeLimit
            ' ":CALC2:CLIM:PASS:SOUR2 {0}", Me._PassBitPatternNumeric.Value
            .ApplyPassBits(binning.PassBits)
            binning.PassBits = .PassBits.GetValueOrDefault(0)
            ' Me.Session.Write(":CALC2:CLIM:BCON IMM")
            .ApplyBinningControl(BinningControl.Immediate)
        End With

        ' enable contact check
        If insulation.ContactCheckEnabled Then
            With Device.ContactCheckLimit
                ' Me.Session.Write(":CALC2:LIM4:STAT ON")
                .ApplyEnabled(insulation.ContactCheckEnabled)
                insulation.ContactCheckEnabled = .Enabled.GetValueOrDefault(False)
                If Me._ContactCheckBitPatternNumeric.Value > 0 Then
                    .ApplyFailureBits(CInt(Me._ContactCheckBitPatternNumeric.Value))
                End If
            End With
            ' this is done when setting the insulation test: 
            ' Me.Session.Write(":SYSTem:CCH ON")
        Else
            With Device.ContactCheckLimit
                ' Me.Session.Write(":CALC2:LIM4:STAT OFF")
                .ApplyEnabled(insulation.ContactCheckEnabled)
                insulation.ContactCheckEnabled = .Enabled.GetValueOrDefault(True)
            End With
            ' this is done when setting the insulation test: 
            ' Me.Session.Write(":SYSTem:CCH OFF")
        End If

        ' ======
        ' EOT
        ' Set byte size to 3 enabling EOT mode.
        ' Me.Session.Write(":SOUR2:BSIZ 3")
        Me.Device.DigitalOutput.ApplyBitSize(3)

        ' Set the output logic to high
        ' Me.Session.Write(":SOUR2:TTL #b000")
        Me.Device.DigitalOutput.ApplyLevel(0)

        ' Set Digital I/O Mode to EOT
        ' Me.Session.Write(":SOUR2:TTL4:MODE EOT")
        Me.Device.DigitalOutput.ApplyOutputMode(OutputMode.EndTest)

        ' Set EOT polarity to HI
        ' Me.Session.Write(":SOUR2:TTL4:BST HI")
        Me.Device.DigitalOutput.ApplyOutputSignalPolarity(OutputSignalPolarity.High)

        ' Set the output level to set automatically to the
        ' :TTL level after the pass or fail output bit
        ' pattern or a limit test is sent to the handler.
        ' this can be turned off and the clear command issued on each start.
        ' Me.Session.Write(":SOUR2:CLE")
        Me.Device.DigitalOutput.ClearOutput()
        ' Me.Session.Write(":SOUR2:CLE:AUTO ON")
        Me.Device.DigitalOutput.ApplyAutoClearEnabled(True)

        ' Set the duration of the EOT strobe
        ' Me.Session.Write(":SOUR2:CLE:AUTO:DEL " & CStr(Me._EotStrobeDurationNumeric.Value))
        Me.Device.DigitalOutput.ApplyDelay(TimeSpan.FromMilliseconds(Me._EotStrobeDurationNumeric.Value))

        ' =================
        ' ARM MODEL:

        ' arm to immediate mode.
        ' default is IMM:  Me.Session.Write(":ARM:SOUR IMM")

        ' Set ARM counter to 1
        ' default is 1: Me.Session.Write(":ARM:COUN 1")

        ' Define Trigger layer to interface with the
        ' Trigger Master board:

        ' clear any pending triggers.
        ' Me.Session.Write(":TRIG:CLE")
        Me.Device.TriggerSubsystem.ClearTriggers()

        ' Set TRIGGER input line to Trigger Link line number
        ' default. to be set when starting: Me.Session.Write(":TRIG:ILIN 1")

        ' Set TRIGGER output line to Trigger Link line number
        ' default: Me.Session.Write(":TRIG:OLIN 2")

        ' Set Input trigger to Acceptor
        ' Me.Session.Write(":TRIG:DIR ACC")
        'Me.Device.TriggerSubsystem.ApplyDirection(Direction.Acceptor)

        ' Me.Session.Write(":TRIG:DIR SOUR")
        'Me.Device.TriggerSubsystem.ApplyDirection(Direction.Source)
        Me.Device.TriggerSubsystem.ApplyDirection(binning.TriggerDirection)
        If binning.TriggerDirection <> Me.Device.TriggerSubsystem.Direction.GetValueOrDefault(VI.Direction.None) Then
            Throw New OperationFailedException($"Failed setting trigger direction to {binning.TriggerDirection};. Value set to {Me.Device.TriggerSubsystem.Direction}")
        End If

        ' start with immediate trigger allowing non-triggered measurements.
        ' Me.Session.Write(":TRIG:SOUR IMM")
        ' see below: Me.Device.TriggerSubsystem.ApplyTriggerSource(TriggerSource.Immediate)

        ' no outputs triggers
        ' default is none: Me.Session.Write(":TRIG:OUTP NONE")

        ' A single trigger.
        ' default is 1: Me.Session.Write(":TRIG:COUN 1")

        ' default: Me.Session.Write(":TRIG:INP NONE")

        ' set trigger source to immediate
        Me.Device.TriggerSubsystem.ApplyTriggerSource(binning.TriggerSource)
        If binning.TriggerSource <> Me.Device.TriggerSubsystem.TriggerSource.GetValueOrDefault(TriggerSources.None) Then
            Throw New OperationFailedException($"Failed setting trigger source to {binning.TriggerSource};. Value set to {Me.Device.TriggerSubsystem.TriggerSource}")
        End If

        Me.Device.ArmLayerSubsystem.ApplyDirection(binning.ArmDirection)
        If binning.ArmDirection <> Me.Device.ArmLayerSubsystem.Direction.GetValueOrDefault(VI.Direction.None) Then
            Throw New OperationFailedException($"Failed setting Arm direction to {binning.ArmDirection};. Value set to {Me.Device.ArmLayerSubsystem.Direction}")
        End If

        Me.Device.ArmLayerSubsystem.ApplyArmSource(binning.ArmSource)
        binning.ArmSource = Me.Device.ArmLayerSubsystem.ArmSource.GetValueOrDefault(ArmSources.None)

        Me.Device.ArmLayerSubsystem.ApplyArmCount(binning.ArmCount)
        binning.ArmCount = Me.Device.ArmLayerSubsystem.ArmCount.GetValueOrDefault(0)

    End Sub

    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _ApplySotSettingsButton_Click(sender As Object, e As EventArgs) Handles _ApplySotSettingsButton.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()
            Me.ApplyHipotBinningInfo()
            Me.ApplyHipotSettings()
            Me.ConfigureHipotStartTest(Me.BinningInfo, Me.ActiveInsulationResistance)
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, "Failed applying trigger settings")
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, "Exception occurred applying trigger settings;. Details: {0}", ex)
        Finally
            Me.Cursor = Cursors.Default
        End Try

    End Sub

    Private Sub ApplyHipotBinningInfo()
        With Me.BinningInfo
            .TriggerDirection = VI.Direction.Source
            .TriggerSource = TriggerSources.Immediate
            .ArmSource = Me.SelectedArmSource
            If .ArmSource = VI.ArmSources.Manual Then
                .ArmCount = 1
            Else
                .ArmCount = 0
            End If
            .FailureBits = CInt(Me._FailBitPatternNumeric.Value)
            .UpperLimit = isr.VI.Scpi.Syntax.Infinity
            .UpperLimitFailureBits = CInt(Me._FailBitPatternNumeric.Value)
            .LowerLimit = 1000000.0 * Me._ResistanceLowLimitNumeric.Value
            .LowerLimitFailureBits = CInt(Me._FailBitPatternNumeric.Value)
            .PassBits = CInt(Me._PassBitPatternNumeric.Value)
            .Enabled = True
        End With
    End Sub

    ''' <summary>
    ''' Configures the high potential measurement.
    ''' </summary>
    Public Function ConfigureHipot(ByVal hipotSettings As InsulationResistance) As Boolean
        If hipotSettings Is Nothing Then Throw New ArgumentNullException(NameOf(hipotSettings))
        ' make sure the 2400 output is off
        ' Me.Session.Write(":OUTP OFF")
        Me.Device.OutputSubsystem.ApplyOutputOnState(False)

#If False Then
    Me.Session.Write("*RST")
    Me.Session.Write(":SENS:FUNC ""RES""")
    Me.Session.Write(":SENS:RES:MODE MAN")
    Me.Session.Write(":SYST:RSEN ON")
    Me.Session.Write(":SOUR:FUNC VOLT")
    Me.Session.Write(":SOUR:VOLT:RANG 5")
    Me.Session.Write(":SOUR:VOLT 5")
    Me.Session.Write(":SENS:CURR:RANG 10e-6")
    Me.Session.Write(":OUTPut ON")
    Dim value As String = Me.Session.Query(":READ?")
    Me.Session.Write(":OUTPut OFF")
#End If

        ' Use rear terminals
        ' Me.Session.Write(":ROUT:TERM REAR")
        Me.Device.RouteSubsystem.ApplyTerminalMode(VI.RouteTerminalMode.Rear)

        ' Me.Session.Write(":SENS:FUNC ""RES""")
        Me.Device.SenseSubsystem.ApplyFunctionMode(Scpi.SenseFunctionModes.Resistance)

        ' set to manual mode to require manually setting the measurement as voltage source
        ' Me.Session.Write(":SENS:RES:MODE MAN")
        Me.Device.SenseResistanceSubsystem.ApplyConfigurationMode(ConfigurationMode.Manual)

        ' the source must be set first.
        ' Me.Session.Write(":SOUR:CLE:AUTO ON")
        Me.Device.SourceSubsystem.ApplyAutoClearEnabled(True)

        'Me.Session.Write(":SOUR:DEL " & CStr(Me._dwellTimeNumeric.Value))
        'Me._dwellTimeNumeric.Value = CDec(Me.Session.Query(":SOUR:DEL?"))
        Me.Device.SourceSubsystem.ApplyDelay(hipotSettings.DwellTime)
        hipotSettings.DwellTime = Me.Device.SourceSubsystem.Delay.GetValueOrDefault(TimeSpan.Zero)

        ' Me.Session.Write(":SOUR:FUNC VOLT")
        Me.Device.SourceSubsystem.ApplyFunctionMode(SourceFunctionModes.Voltage)

        'Me.Session.Write(":SOUR:VOLT:RANG " & SelectVoltageRange(Me._VoltageLevelNumeric.Value))
        Me.Device.SourceVoltageSubsystem.ApplyRange(hipotSettings.VoltageLevel)

        ' Me.Session.Write(":SOUR:VOLT:LEV " & CStr(Me._VoltageLevelNumeric.Value))
        ' Me._VoltageLevelNumeric.Value = CDec(Me.Session.Query(":SOUR:VOLT:LEV?"))
        Me.Device.SourceVoltageSubsystem.ApplyLevel(hipotSettings.VoltageLevel)
        hipotSettings.VoltageLevel = Me.Device.SourceVoltageSubsystem.Level.GetValueOrDefault(0)

        ' Me.Session.Write(":SENS:CURR:RANG " & CStr(Me.CurrentRange))
        Me.Device.SenseCurrentSubsystem.ApplyRange(Me.ActiveInsulationResistance.CurrentRange)

        ' Me.Session.Write(":SENS:CURR:PROT " & CStr(Me._CurrentLimitNumeric.Value))
        Me.Device.SenseCurrentSubsystem.ApplyProtectionLevel(hipotSettings.CurrentLimit)
        hipotSettings.CurrentLimit = Me.Device.SenseCurrentSubsystem.ProtectionLevel.GetValueOrDefault(0)

        'Me.Session.Write(":SENS:VOLT:NPLC " & CStr(Me._ApertureNumeric.Value))
        Me.Device.SenseVoltageSubsystem.ApplyPowerLineCycles(hipotSettings.PowerLineCycles)
        hipotSettings.PowerLineCycles = Me.Device.SenseVoltageSubsystem.PowerLineCycles.GetValueOrDefault(0)

        ' Me.Session.Write(":SENS:CURR:NPLC " & CStr(Me._ApertureNumeric.Value))
        Me.Device.SenseCurrentSubsystem.ApplyPowerLineCycles(hipotSettings.PowerLineCycles)
        hipotSettings.PowerLineCycles = Me.Device.SenseCurrentSubsystem.PowerLineCycles.GetValueOrDefault(0)

        ' Enable four wire connection
        ' Me.Session.Write(":SYST:RSEN ON")
        Me.Device.SystemSubsystem.ApplyFourWireSenseEnabled(True)

        ' force immediate update of auto zero
        ' Me.Session.Write(":SYST:AZER ONCE")
        Me.Device.SystemSubsystem.ApplyAutoZeroEnabled(True)

        Me.Device.SystemSubsystem.ApplyContactCheckEnabled(hipotSettings.ContactCheckEnabled)
        hipotSettings.ContactCheckEnabled = Me.Device.SystemSubsystem.ContactCheckEnabled.GetValueOrDefault(False)
        Me.Device.ContactCheckLimit.ApplyEnabled(hipotSettings.ContactCheckEnabled)
        hipotSettings.ContactCheckEnabled = Me.Device.ContactCheckLimit.Enabled.GetValueOrDefault(False)

        ' Me.Session.Write(":FORM:ELEM VOLT,CURR,RES,STATUS")
        Me.Device.FormatSubsystem.ApplyElements(ReadingElements.Voltage Or ReadingElements.Current Or
                                                ReadingElements.Resistance Or ReadingElements.Status)

        Return True

    End Function

    Private Sub ApplyHipotSettings()
        With Me.ActiveInsulationResistance
            .DwellTime = TimeSpan.FromSeconds(Me._dwellTimeNumeric.Value)
            .VoltageLevel = Me._VoltageLevelNumeric.Value
            .CurrentLimit = 0.000001 * Me._CurrentLimitNumeric.Value
            .PowerLineCycles = Me._ApertureNumeric.Value
            .ContactCheckEnabled = Me._ContactCheckToggle.Checked
            .ResistanceLowLimit = 1000000.0 * Me._ResistanceLowLimitNumeric.Value
            .ResistanceRange = 1000000.0 * Me._ResistanceRangeNumeric.Value
        End With
    End Sub

    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _ApplyHipotSettingsButton_Click(sender As Object, e As EventArgs) Handles _ApplyHipotSettingsButton.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()
            Me.ConfigureHipot(Me.ActiveInsulationResistance)
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, "Failed applying high potential settings")
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, "Exception occurred applying high potential settings;. Details: {0}", ex)
        Finally
            Me.Cursor = Cursors.Default
        End Try

    End Sub

#End Region

#Region " CONTROL EVENT HANDLERS: RESET "

    ''' <summary> Event handler. Called by _SessionTraceEnableCheckBox for checked changed events. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _SessionTraceEnabledMenuItem_CheckedChanged(ByVal sender As Object, e As System.EventArgs) Handles _SessionTraceEnabledMenuItem.CheckedChanged
        If Me._InitializingComponents Then Return
        Dim checkBox As Windows.Forms.CheckBox = CType(sender, Windows.Forms.CheckBox)
        Try
            Me.Cursor = Cursors.WaitCursor
            If checkBox.Enabled Then
                Me.Device.SessionMessagesTraceEnabled = checkBox.Checked
            End If
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.ToString)
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId,
                               "Exception occurred initiating a measurement;. Details: {0}", ex)
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    ''' <summary> Event handler. Called by interfaceClearButton for click events. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _ClearInterfaceMenuItem_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles _ClearInterfaceMenuItem.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()
            Me.Talker?.Publish(TraceEventType.Information, My.MyLibrary.TraceEventId,
                               "{0} clearing interface;. {1}", Me.ResourceTitle, Me.ResourceName)
            Me.Device.SystemSubsystem.ClearInterface()
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.ToString)
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, "Exception occurred clearing interface;. Details: {0}", ex)
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    ''' <summary> Event handler. Called by _SelectiveDeviceClearButton for click events. </summary>
    ''' <param name="sender"> <see cref="System.Object"/> instance of this
    ''' <see cref="System.Windows.Forms.Control"/> </param>
    ''' <param name="e">      Event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _ClearDeviceMenuItem_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles _ClearDeviceMenuItem.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()
            Me.Talker?.Publish(TraceEventType.Information, My.MyLibrary.TraceEventId,
                               "{0} clearing selective device;. {1}", Me.ResourceTitle, Me.ResourceName)
            Me.Device.SystemSubsystem.ClearDevice()
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.ToString)
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, "Exception occurred sending SDC;. Details: {0}", ex)
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    ''' <summary> Issue RST. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _ResetKnownStateMenuItem_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles _ResetKnownStateMenuItem.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()
            If Me.IsDeviceOpen Then
                Me.Talker?.Publish(TraceEventType.Information, My.MyLibrary.TraceEventId,
                                   "{0} resetting known state;. {1}", Me.ResourceTitle, Me.ResourceName)
                Me.Device.ResetKnownState()
            End If
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.ToString)
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, "Exception occurred resetting known state;. Details: {0}", ex)
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    ''' <summary> Event handler. Called by _InitializeKnownStateButton for click events. </summary>
    ''' <param name="sender"> <see cref="System.Object"/> instance of this
    ''' <see cref="System.Windows.Forms.Control"/> </param>
    ''' <param name="e">      Event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _InitializeKnowStateMenuItem_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles _InitializeKnowStateMenuItem.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()
            If Me.IsDeviceOpen Then
                Me.Talker?.Publish(TraceEventType.Information, My.MyLibrary.TraceEventId,
                                   "{0} resetting known state;. {1}", Me.ResourceTitle, Me.ResourceName)
                Me.Device.ResetKnownState()
                Me.Talker?.Publish(TraceEventType.Information, My.MyLibrary.TraceEventId,
                                   "{0} initializing known state;. {1}", Me.ResourceTitle, Me.ResourceName)
                Me.Device.InitKnownState()
            End If
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.ToString)
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, "Exception occurred initializing known state;. Details: {0}", ex)
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

#End Region

#Region " CONTROL EVENT HANDLERS: READING "

    ''' <summary> Event handler. Called by InitButton for click events. Initiates a reading for
    ''' retrieval by way of the service request event. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub InitButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles _InitiateButton.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()

            ' clear execution state before enabling events
            Me.Device.ClearExecutionState()

            ' set the service request
            Me.Device.StatusSubsystem.ApplyMeasurementEventEnableBitmask(MeasurementEvents.All)
            Me.Device.StatusSubsystem.EnableServiceRequest(VI.ServiceRequests.All And Not VI.ServiceRequests.MessageAvailable)

            ' trigger the initiation of the measurement letting the service request do the rest.
            Me.Device.ClearExecutionState()
            Me.Device.TriggerSubsystem.Initiate()

        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.ToString)
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, "Exception occurred initiating a measurement;. Details: {0}", ex)
        Finally
            Me.Cursor = Cursors.Default
        End Try

    End Sub

    ''' <summary> Event handler. Called by _ReadingComboBox for selected index changed events. Selects
    ''' a new reading to display. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _ReadingComboBox_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles _ReadingComboBox.SelectedIndexChanged
        Try
            Me.Cursor = Cursors.WaitCursor
            If Me._ReadingComboBox.Enabled AndAlso Me._ReadingComboBox.SelectedIndex >= 0 AndAlso
                    Not String.IsNullOrWhiteSpace(Me._ReadingComboBox.Text) Then
                Me.onMeasurementAvailable(Me.Device.MeasureSubsystem.Readings)
            End If
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.ToString)
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, "Exception occurred initiating a measurement;. Details: {0}", ex)
        Finally
            Me.Cursor = Cursors.Default
        End Try

    End Sub

    ''' <summary> Event handler. Called by _ReadButton for click events. Query the Device for a
    ''' reading. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _ReadButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles _ReadButton.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()

            ' update display modalities if changed.
            Me.Device.MeasureSubsystem.Read()

        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.ToString)
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, "Exception occurred initiating a measurement;. Details: {0}", ex)
        Finally
            Me.Cursor = Cursors.Default
        End Try

    End Sub

#End Region

#Region " READ AND WRITE "

    ''' <summary> Executes the simple read write control property changed action. </summary>
    ''' <remarks> David, 12/29/2015. </remarks>
    ''' <param name="sender">       Source of the event. </param>
    ''' <param name="propertyName"> Name of the property. </param>
    Private Sub OnSimpleReadWriteControlPropertyChanged(sender As Instrument.SimpleReadWriteControl, ByVal propertyName As String)
        If sender IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(propertyName) Then
            Select Case propertyName
                Case NameOf(sender.ReceivedMessage)
                Case NameOf(sender.SentMessage)
                Case NameOf(sender.StatusMessage)
                    Me.StatusLabel.Text = sender.StatusMessage
                Case NameOf(sender.ServiceRequestValue)
                    Me.StatusRegisterLabel.Text = $"0x{sender.ServiceRequestValue:X2}"
                Case NameOf(sender.ElapsedTime)
            End Select
        End If
    End Sub

    ''' <summary> Simple read write control property changed. </summary>
    ''' <remarks> David, 12/29/2015. </remarks>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Property changed event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _SimpleReadWriteControl_PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Handles _SimpleReadWriteControl.PropertyChanged
        Try
            Me.OnSimpleReadWriteControlPropertyChanged(TryCast(sender, Instrument.SimpleReadWriteControl), e?.PropertyName)
        Catch ex As Exception
            Me.StatusLabel.Text = "Exception occurred handling change"
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId,
                               "Exception handling Simple Read and Write property changed Event;. Failed property {0}. Details: {1}",
                               e?.PropertyName, ex)
        End Try
    End Sub

#End Region

#Region " TALKER "

    ''' <summary> Adds listeners such as current level trace message box and log. </summary>
    ''' <remarks> David, 12/30/2015. </remarks>
    Protected Overrides Sub AddListeners()
        MyBase.AddListeners()
        Me._SimpleReadWriteControl.AddListeners(Me.Talker.Listeners)
    End Sub

    ''' <summary> Adds listeners such as top level trace message box and log. </summary>
    ''' <remarks> David, 12/30/2015. </remarks>
    ''' <param name="listeners"> The listeners. </param>
    Public Overrides Sub AddListeners(ByVal listeners As IEnumerable(Of ITraceMessageListener))
        MyBase.AddListeners(listeners)
        Me._SimpleReadWriteControl.AddListeners(listeners)
    End Sub

    ''' <summary> Adds the log listener. </summary>
    ''' <remarks> David, 1/21/2016. </remarks>
    ''' <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
    ''' <param name="log"> The log. </param>
    Public Overrides Sub AddListeners(ByVal log As MyLog)
        If log Is Nothing Then Throw New ArgumentNullException(NameOf(log))
        MyBase.AddListeners(log)
        My.MyLibrary.Identify(Me.Talker)
    End Sub


#End Region

End Class

#Region " UNUSED "
#If False Then
        AddHandler Me.Device.SenseFourWireResistanceSubsystem.PropertyChanged, AddressOf Me.SenseFourWireResistanceSubsystemPropertyChanged
        RemoveHandler Me.Device.SenseFourWireResistanceSubsystem.PropertyChanged, AddressOf Me.SenseFourWireResistanceSubsystemPropertyChanged
#End If
#End Region
