Imports System.ComponentModel
Imports System.Windows.Forms
Imports isr.Core.Pith
Imports isr.Core.Pith.ErrorProviderExtensions
Imports isr.Core.Pith.EnumExtensions
Imports isr.VI.ExceptionExtensions
''' <summary> Provides a user interface for the Keithley 7000 Device. </summary>
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
<CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")>
<System.ComponentModel.DisplayName("K7000 Panel"),
      System.ComponentModel.Description("Keithley 7000 Device Panel"),
      System.Drawing.ToolboxBitmap(GetType(K7000Panel))>
Public Class K7000Panel
    Inherits VI.Instrument.ResourcePanelBase

#Region " CONSTRUCTION + CLEANUP "

    ''' <summary> Default constructor. </summary>
    <CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")>
    Public Sub New()
        Me.New(New Device)
        Me.IsDeviceOwner = True
    End Sub

    ''' <summary> Constructor. </summary>
    ''' <param name="device"> The device. </param>
    Public Sub New(ByVal device As Device)
        MyBase.New(device)
        Me.InitializingComponents = True
        Me.InitializeComponent()
        Me.InitializingComponents = False
        Me._AssignDevice(device)
        ' note that the caption is not set if this is run inside the On Load function.
        With Me.TraceMessagesBox
            ' set defaults for the messages box.
            .ResetCount = 500
            .PresetCount = 250
            .ContainerPanel = Me._MessagesTabPage
        End With
        With Me._ServiceRequestFlagsComboBox
            .DataSource = Nothing
            .Items.Clear()
            .DataSource = [Enum].GetNames(GetType(isr.VI.Pith.ServiceRequests))
        End With
        With Me._ServiceRequestEnableBitmaskNumeric.NumericUpDownControl
            .Hexadecimal = True
            .Maximum = 255
            .Minimum = 0
            .Value = 0
        End With
        Me.EnableTraceLevelControls()
    End Sub

    ''' <summary>
    ''' Releases the unmanaged resources used by the isr.VI.Instrument.ResourcePanelBase and
    ''' optionally releases the managed resources.
    ''' </summary>
    ''' <param name="disposing"> true to release both managed and unmanaged resources; false to
    '''                          release only unmanaged resources. </param>
    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If Not Me.IsDisposed AndAlso disposing Then
                Try
                    If Me._ChannelListBuilder IsNot Nothing Then Me._ChannelListBuilder.Dispose() : Me._ChannelListBuilder = Nothing
                Catch ex As Exception
                    Debug.Assert(Not Debugger.IsAttached, "Exception occurred disposing the channel builder", $"Exception {ex.ToFullBlownString}")
                End Try
                Try
                    Me.Device?.RemovePrivateListener(Me.TraceMessagesBox)
                    If Me.Device IsNot Nothing Then Me.DeviceClosing(Me, New System.ComponentModel.CancelEventArgs)
                Catch ex As Exception
                    Debug.Assert(Not Debugger.IsAttached, $"{ex.Message} occurred closing the device", $"Exception {ex.ToFullBlownString}")
                End Try
                ' the device gets closed and disposed (if panel is device owner) in the base class
                If Me.components IsNot Nothing Then Me.components.Dispose() : Me.components = Nothing
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

#End Region

#Region " FORM EVENTS "

    ''' <summary> Handles the <see cref="E:System.Windows.Forms.UserControl.Load" /> event. </summary>
    ''' <param name="e"> An <see cref="T:System.EventArgs" /> that contains the event data. </param>
    Protected Overrides Sub OnLoad(e As EventArgs)
        Try
        Finally
            MyBase.OnLoad(e)
        End Try
    End Sub

#End Region

#Region " DEVICE "

    ''' <summary> Assigns a device. </summary>
    ''' <param name="value"> True to show or False to hide the control. </param>
    Private Sub _AssignDevice(ByVal value As Device)
        Me._Device = value
        Me._Device.CaptureSyncContext(Threading.SynchronizationContext.Current)
        Me._Device.AddPrivateListener(Me.TraceMessagesBox)
        Me.OnDeviceOpenChanged(value)
    End Sub

    ''' <summary> Assigns a device. </summary>
    ''' <param name="value"> True to show or False to hide the control. </param>
    Public Overloads Sub AssignDevice(ByVal value As Device)
        Me.IsDeviceOwner = False
        MyBase.AssignDevice(value)
        Me._AssignDevice(value)
    End Sub

    ''' <summary> Releases the device. </summary>
    Protected Overrides Sub ReleaseDevice()
        If Me.IsDeviceOwner Then
            MyBase.ReleaseDevice()
        Else
            Me._Device = Nothing
        End If
    End Sub

    ''' <summary> Gets the is device assigned. </summary>
    ''' <value> The is device assigned. </value>
    Public Overrides ReadOnly Property IsDeviceAssigned As Boolean
        Get
            Return MyBase.IsDeviceAssigned AndAlso Me.Device IsNot Nothing AndAlso Not Me.Device.IsDisposed
        End Get
    End Property

    ''' <summary> Gets a reference to the Device. </summary>
    ''' <value> The device. </value>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(False)>
    Public Overloads ReadOnly Property Device() As Device

#End Region

#Region " DEVICE EVENT HANDLERS "

    ''' <summary> Executes the device open changed action. </summary>
    ''' <param name="device"> The device. </param>
    Protected Overrides Sub OnDeviceOpenChanged(ByVal device As VI.DeviceBase)
        MyBase.OnDeviceOpenChanged(device)
        If Me.IsDeviceOpen Then
            Me._SimpleReadWriteControl.Connect(device?.Session)
        Else
            Me._SimpleReadWriteControl.Disconnect()
        End If
        For Each t As Windows.Forms.TabPage In Me._Tabs.TabPages
            If t IsNot Me._MessagesTabPage Then Me.RecursivelyEnable(t.Controls, Me.IsDeviceOpen)
        Next
        If Me.IsDeviceOpen Then
            Me._ClearInterfaceMenuItem.Visible = Me.Device.StatusSubsystemBase.SupportsClearInterface
            VI.Pith.SessionBase.ListNotificationLevels(Me._SessionNotificationLevelComboBox.ComboBox)
            AddHandler Me._SessionNotificationLevelComboBox.ComboBox.SelectedIndexChanged, AddressOf Me._SessionNotificationLevelComboBox_SelectedIndexChanged
            VI.Pith.SessionBase.SelectItem(Me._SessionNotificationLevelComboBox, NotifySyncLevel.None)
        End If
    End Sub


    ''' <summary> Handles the device property changed event. </summary>
    ''' <param name="device">    The device. </param>
    ''' <param name="propertyName"> Name of the property. </param>
    Protected Overrides Sub HandlePropertyChange(ByVal device As VI.DeviceBase, ByVal propertyName As String)
        If device Is Nothing OrElse String.IsNullOrWhiteSpace(propertyName) Then Return
        MyBase.HandlePropertyChange(device, propertyName)
        Select Case propertyName
            Case NameOf(isr.VI.DeviceBase.SessionServiceRequestEventEnabled)
                Me._SessionServiceRequestHandlerEnabledMenuItem.Checked = device.SessionServiceRequestEventEnabled
            Case NameOf(isr.VI.DeviceBase.DeviceServiceRequestHandlerAdded)
                Me._DeviceServiceRequestHandlerEnabledMenuItem.Checked = device.DeviceServiceRequestHandlerAdded
            Case NameOf(isr.VI.DeviceBase.MessageNotificationLevel)
                VI.Pith.SessionBase.SelectItem(Me._SessionNotificationLevelComboBox, device.MessageNotificationLevel)
            Case NameOf(isr.VI.DeviceBase.ServiceRequestEnableBitmask)
                Dim value As VI.Pith.ServiceRequests = device.ServiceRequestEnableBitmask
                Me._ServiceRequestEnableBitmaskNumeric.Value = value
                Me._ServiceRequestEnableBitmaskNumeric.ToolTipText = $"SRE:0b{Convert.ToString(value, 2),8}".Replace(" ", "0")
        End Select
    End Sub

    ''' <summary> Event handler. Called when device opened. </summary>
    ''' <param name="sender"> <see cref="System.Object"/> instance of this
    ''' <see cref="System.Windows.Forms.Control"/> </param>
    ''' <param name="e">      Event information. </param>
    Protected Overrides Sub DeviceOpened(ByVal sender As Object, ByVal e As System.EventArgs)
        AddHandler Me.Device.TriggerSubsystem.PropertyChanged, AddressOf Me.TriggerSubsystemPropertyChanged
        AddHandler Me.Device.RouteSubsystem.PropertyChanged, AddressOf Me.RouteSubsystemPropertyChanged
        AddHandler Me.Device.StatusSubsystem.PropertyChanged, AddressOf Me.StatusSubsystemPropertyChanged
        AddHandler Me.Device.SystemSubsystem.PropertyChanged, AddressOf Me.SystemSubsystemPropertyChanged
        MyBase.DeviceOpened(sender, e)
    End Sub

    Protected Overrides Sub DeviceInitialized(ByVal sender As Object, ByVal e As System.EventArgs)
        ' must be done after the device base opens where the subsystem gets initialized.
        With Me._ArmLayer1SourceComboBox.ComboBox
            .DataSource = Nothing
            .Items.Clear()
            .DataSource = GetType(Scpi.ArmSources).ValueNamePairs(Me.Device.ArmLayer1Subsystem.SupportedArmSources)
            .DisplayMember = "Value"
            .ValueMember = "Key"
        End With
    End Sub


    ''' <summary> Executes the title changed action. </summary>
    ''' <param name="value"> True to show or False to hide the control. </param>
    Protected Overrides Sub OnTitleChanged(ByVal value As String)
        Me._TitleLabel.Text = value
        Me._TitleLabel.Visible = Not String.IsNullOrWhiteSpace(value)
        MyBase.OnTitleChanged(value)
    End Sub

    ''' <summary> Event handler. Called when device is closing. </summary>
    ''' <param name="sender"> <see cref="System.Object"/> instance of this
    ''' <see cref="System.Windows.Forms.Control"/> </param>
    ''' <param name="e">      Event information. </param>
    Protected Overrides Sub DeviceClosing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs)
        MyBase.DeviceClosing(sender, e)
        If e?.Cancel Then Return
        If Me.IsDeviceOpen Then
            RemoveHandler Me.Device.TriggerSubsystem.PropertyChanged, AddressOf Me.TriggerSubsystemPropertyChanged
            RemoveHandler Me.Device.RouteSubsystem.PropertyChanged, AddressOf Me.RouteSubsystemPropertyChanged
            RemoveHandler Me.Device.StatusSubsystem.PropertyChanged, AddressOf Me.StatusSubsystemPropertyChanged
            RemoveHandler Me.Device.SystemSubsystem.PropertyChanged, AddressOf Me.SystemSubsystemPropertyChanged
        End If
    End Sub

#End Region

#Region " SUBSYSTEMS "

#Region " ROUTE "

    ''' <summary> Handle the Route subsystem property changed event. </summary>
    ''' <param name="subsystem">    The subsystem. </param>
    ''' <param name="propertyName"> Name of the property. </param>
    Private Overloads Sub HandlePropertyChange(ByVal subsystem As RouteSubsystem, ByVal propertyName As String)
        If subsystem Is Nothing OrElse String.IsNullOrWhiteSpace(propertyName) Then Return
        Select Case propertyName
            Case NameOf(VI.K7000.RouteSubsystem.ScanList)
                If Not String.IsNullOrWhiteSpace(subsystem.ScanList) Then
                    Me.Talker.Publish(TraceEventType.Information, My.MyLibrary.TraceEventId,
                                       "{0} scan list changed to {1}.", Me.ResourceName, subsystem.ScanList)
                End If
        End Select
    End Sub

    ''' <summary> Route subsystem property changed. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Property Changed event information. </param>
    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub RouteSubsystemPropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)
        If Me.InitializingComponents OrElse sender Is Nothing OrElse e Is Nothing Then Return
        Dim activity As String = $"handling {NameOf(RouteSubsystem)}.{e.PropertyName} change"
        Try
            If Me.InvokeRequired Then
                Me.Invoke(New Action(Of Object, System.ComponentModel.PropertyChangedEventArgs)(AddressOf Me.RouteSubsystemPropertyChanged), New Object() {sender, e})
            Else
                Me.HandlePropertyChange(TryCast(sender, RouteSubsystem), e.PropertyName)
            End If
        Catch ex As Exception
            If Me.Talker Is Nothing Then
                My.MyLibrary.LogUnpublishedException(activity, ex)
            Else
                Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"Exception {activity};. {ex.ToFullBlownString}")
            End If
        End Try
    End Sub

#End Region

#Region " STATUS "

    ''' <summary> Reports the last error. </summary>
    Private Sub OnLastError(ByVal lastError As DeviceError)
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
    Protected Overrides Sub HandlePropertyChange(ByVal subsystem As VI.StatusSubsystemBase, ByVal propertyName As String)
        If subsystem Is Nothing OrElse String.IsNullOrWhiteSpace(propertyName) Then Return
        MyBase.HandlePropertyChange(subsystem, propertyName)
        Select Case propertyName
            Case NameOf(StatusSubsystemBase.DeviceErrorsReport)
                OnLastError(subsystem.LastDeviceError)
            Case NameOf(StatusSubsystemBase.LastDeviceError)
                OnLastError(subsystem.LastDeviceError)
            Case NameOf(StatusSubsystemBase.ErrorAvailable)
            Case NameOf(StatusSubsystemBase.ServiceRequestStatus)
                'Me._StatusRegisterLabel.Text = $"0x{subsystem.ServiceRequestStatus:X2}"
            Case NameOf(StatusSubsystemBase.StandardEventStatus)
                ' Me._StandardRegisterLabel.Text = $"0x{subsystem.StandardEventStatus:X2}". 
        End Select
    End Sub

    ''' <summary> Status subsystem property changed. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Property Changed event information. </param>
    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub StatusSubsystemPropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)
        If Me.InitializingComponents OrElse sender Is Nothing OrElse e Is Nothing Then Return
        Dim activity As String = $"handling {NameOf(StatusSubsystem)}.{e.PropertyName} change"
        Try
            If Me.InvokeRequired Then
                Me.Invoke(New Action(Of Object, System.ComponentModel.PropertyChangedEventArgs)(AddressOf Me.StatusSubsystemPropertyChanged), New Object() {sender, e})
            Else
                Me.HandlePropertyChange(TryCast(sender, StatusSubsystem), e.PropertyName)
            End If
        Catch ex As Exception
            If Me.Talker Is Nothing Then
                My.MyLibrary.LogUnpublishedException(activity, ex)
            Else
                Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"Exception {activity};. {ex.ToFullBlownString}")
            End If
        End Try
    End Sub

#End Region

#Region " SYSTEM "

    ''' <summary> Handle the System subsystem property changed event. </summary>
    ''' <param name="subsystem">    The subsystem. </param>
    ''' <param name="propertyName"> Name of the property. </param>
    <CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")>
    Private Overloads Sub HandlePropertyChange(ByVal subsystem As SystemSubsystem, ByVal propertyName As String)
        If subsystem Is Nothing OrElse String.IsNullOrWhiteSpace(propertyName) Then Return
        Select Case propertyName
        End Select
    End Sub

    ''' <summary> System subsystem property changed. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Property Changed event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub SystemSubsystemPropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)
        If Me.InitializingComponents OrElse sender Is Nothing OrElse e Is Nothing Then Return
        Dim activity As String = $"handling {NameOf(SystemSubsystem)}.{e.PropertyName} change"
        Try
            If Me.InvokeRequired Then
                Me.Invoke(New Action(Of Object, System.ComponentModel.PropertyChangedEventArgs)(AddressOf Me.SystemSubsystemPropertyChanged), New Object() {sender, e})
            Else
                Me.HandlePropertyChange(TryCast(sender, SystemSubsystem), e.PropertyName)
            End If
        Catch ex As Exception
            If Me.Talker Is Nothing Then
                My.MyLibrary.LogUnpublishedException(activity, ex)
            Else
                Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"Exception {activity};. {ex.ToFullBlownString}")
            End If
        End Try
    End Sub

#End Region

#Region " TRIGGER "

    ''' <summary> Handle the Trigger subsystem property changed event. </summary>
    ''' <param name="subsystem">    The subsystem. </param>
    ''' <param name="propertyName"> Name of the property. </param>
    Private Overloads Sub HandlePropertyChange(ByVal subsystem As TriggerSubsystem, ByVal propertyName As String)
        If subsystem Is Nothing OrElse String.IsNullOrWhiteSpace(propertyName) Then Return
        Select Case propertyName
            Case NameOf(VI.K7000.TriggerSubsystem.ContinuousEnabled)
                If subsystem.ContinuousEnabled.HasValue Then
                    Me._ContinuousTriggerEnabledCheckBox.Checked = subsystem.ContinuousEnabled.Value
                End If
        End Select
    End Sub

    ''' <summary> Trigger subsystem property changed. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Property Changed event information. </param>
    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub TriggerSubsystemPropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)
        If Me.InitializingComponents OrElse sender Is Nothing OrElse e Is Nothing Then Return
        Dim activity As String = $"handling {NameOf(TriggerSubsystem)}.{e.PropertyName} change"
        Try
            If Me.InvokeRequired Then
                Me.Invoke(New Action(Of Object, System.ComponentModel.PropertyChangedEventArgs)(AddressOf Me.TriggerSubsystemPropertyChanged), New Object() {sender, e})
            Else
                Me.HandlePropertyChange(TryCast(sender, TriggerSubsystem), e.PropertyName)
            End If
        Catch ex As Exception
            If Me.Talker Is Nothing Then
                My.MyLibrary.LogUnpublishedException(activity, ex)
            Else
                Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"Exception {activity};. {ex.ToFullBlownString}")
            End If
        End Try
    End Sub

#End Region

#End Region

#Region " DISPLAY  "

    ''' <summary>Enables or disables service requests.</summary>
    ''' <param name="turnOn">True to turn on or false to turn off the service request.</param>
    ''' <param name="serviceRequestMask">Specifies the 
    '''   <see cref="isr.VI.Pith.ServiceRequests">service request flags</see></param>
    Private Sub ToggleEndServiceRequest(ByVal turnOn As Boolean, ByVal serviceRequestMask As VI.Pith.ServiceRequests)
        If Me.Visible Then
            Me._ServiceRequestMaskTextBox.Text = serviceRequestMask.ToString()
            Me._EnableEndOfSettlingRequestToggle.Enabled = False
            Me._EnableEndOfSettlingRequestToggle.Checked = turnOn
            Me._EnableEndOfSettlingRequestToggle.Enabled = True
            Me._EnableEndOfSettlingRequestToggle.Visible = True
            Me._EnableEndOfSettlingRequestToggle.Invalidate()
        End If
    End Sub

    ''' <summary>Enables or disables service requests.</summary>
    ''' <param name="turnOn">True to turn on or false to turn off the service request.</param>
    ''' <param name="serviceRequestMask">Specifies the 
    '''   <see cref="isr.VI.Pith.ServiceRequests">service request flags</see></param>
    Private Sub ToggleServiceRequest(ByVal turnOn As Boolean, ByVal serviceRequestMask As VI.Pith.ServiceRequests)
        If Me.Visible Then
            Me._ServiceRequestMaskTextBox.Text = serviceRequestMask.ToString()
            Me._EnableServiceRequestToggle.Enabled = False
            Me._EnableServiceRequestToggle.Checked = turnOn
            Me._EnableServiceRequestToggle.Enabled = True
            Me._EnableServiceRequestToggle.Visible = True
            Me._EnableServiceRequestToggle.Invalidate()
        End If
    End Sub

    ''' <summary>Updates the display.</summary>
    Private Sub RefreshDisplay()

        If Me.Visible Then

            ' check if we are asking for a new scan list
            If Me._ScanListComboBox IsNot Nothing AndAlso Me._ScanListComboBox.Text.Length > 0 AndAlso
                    Me._ScanListComboBox.FindString(Me._ScanListComboBox.Text) < 0 Then
                ' if we have a new string, add it to the scan list
                Me._ScanListComboBox.Items.Add(Me._ScanListComboBox.Text)
            End If

            ' update the current scan list
            Me._ScanTextBox.Text = Me.Device.RouteSubsystem.ScanList()

        End If

    End Sub

#End Region

#Region " CHANNEL "

    ''' <summary>Gets or sets the channel list.</summary>
    Private _ChannelListBuilder As VI.ChannelListBuilder

    ''' <summary>Adds new items to the combo box.</summary>
    Private Sub UpdateChannelListComboBox()

        If Me.Visible Then
            ' check if we are asking for a new channel list
            If Me._ChannelListComboBox IsNot Nothing AndAlso Me._ChannelListComboBox.Text.Length > 0 AndAlso
                    Me._ChannelListComboBox.FindString(Me._ChannelListComboBox.Text) < 0 Then
                ' if we have a new string, add it to the channel list
                Me._ChannelListComboBox.Items.Add(Me._ChannelListComboBox.Text)
            End If
        End If

    End Sub

#End Region

#Region " CONTROL EVENT HANDLERS: SLOT "

    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _ReadSlotConfigurationButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles _ReadSlotConfigurationButton.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()
            Dim slotNumber As Int32 = Int32.Parse(Me._SlotNumberComboBox.Text, Globalization.CultureInfo.CurrentCulture)
            Me._CardTypeTextBox.Text = Me.Device.RouteSubsystem.QuerySlotCardType(slotNumber)
            Me._SettlingTimeTextBoxTextBox.Text = Me.Device.RouteSubsystem.QuerySlotCardSettlingTime(slotNumber).TotalSeconds.ToString(Globalization.CultureInfo.CurrentCulture)
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.Message)
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"Exception occurred;. {ex.ToFullBlownString}")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _UpdateSlotConfigurationButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles _UpdateSlotConfigurationButton.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()
            Dim slotNumber As Int32 = Int32.Parse(Me._SlotNumberComboBox.Text, Globalization.CultureInfo.CurrentCulture)
            Me._CardTypeTextBox.Text = Me.Device.RouteSubsystem.ApplySlotCardType(slotNumber, Me._CardTypeTextBox.Text)
            Me._SettlingTimeTextBoxTextBox.Text = Me.Device.RouteSubsystem.ApplySlotCardSettlingTime(slotNumber, TimeSpan.FromTicks(CLng(TimeSpan.TicksPerSecond * CDbl(Me._SettlingTimeTextBoxTextBox.Text)))).TotalSeconds.ToString(Globalization.CultureInfo.CurrentCulture)
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.Message)
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"Exception occurred;. {ex.ToFullBlownString}")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

#End Region

#Region " CONTROL EVENT HANDLERS: SCAN "

    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _StepButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles _StepButton.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()

            ' clear registers
            Me.Device.ClearExecutionState()

            ' close the scan list
            Me.Device.TriggerSubsystem.Initiate()
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.Message)
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"Exception occurred;. {ex.ToFullBlownString}")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _UpdateScanListButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles _UpdateScanListButton.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()
            Me.Device.RouteSubsystem.ApplyScanList(Me._ScanListComboBox.Text)
            Me.RefreshDisplay()
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.Message)
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"Exception occurred;. {ex.ToFullBlownString}")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

#End Region

#Region " CONTROL EVENT HANDLERS: CHANNEL "

    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _AddChannelToList_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles _AddChannelToList.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()
            If Me._ChannelListBuilder Is Nothing Then
                Me._ChannelListBuilder = New isr.VI.ChannelListBuilder
            End If
            Me._ChannelListBuilder.AddChannel(Int32.Parse(Me._SlotNumberTextBox.Text, Globalization.CultureInfo.CurrentCulture),
                Int32.Parse(Me._RelayNumberTextBox.Text, Globalization.CultureInfo.CurrentCulture))
            Me._ChannelListComboBox.Text = Me._ChannelListBuilder.ChannelList
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.Message)
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"Exception occurred;. {ex.ToFullBlownString}")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _AddMemoryLocationButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles _AddMemoryLocationButton.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()
            If Me._ChannelListBuilder Is Nothing Then
                Me._ChannelListBuilder = New isr.VI.ChannelListBuilder
            End If
            Me._ChannelListBuilder.AddChannel(Int32.Parse(Me._MemoryLocationChannelItemTextBox.Text, Globalization.CultureInfo.CurrentCulture))
            Me._ChannelListComboBox.Text = Me._ChannelListBuilder.ChannelList
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.Message)
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"Exception occurred;. {ex.ToFullBlownString}")
        Finally
            Me.ReadServiceRequestStatus()
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _ChannelCloseButton_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles _ChannelCloseButton.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()
            UpdateChannelListComboBox()
            Me.Device.ClearExecutionState()
            Me.Device.StatusSubsystem.EnableWaitComplete()
            Me.Device.RouteSubsystem.ApplyClosedChannels(Me._ChannelListComboBox.Text, TimeSpan.FromSeconds(1))
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.Message)
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"Exception occurred;. {ex.ToFullBlownString}")
        Finally
            Me.ReadServiceRequestStatus()
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _ChannelOpenButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles _ChannelOpenButton.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()
            UpdateChannelListComboBox()
            Me.Device.ClearExecutionState()
            Me.Device.StatusSubsystem.EnableWaitComplete()
            Me.Device.RouteSubsystem.ApplyOpenChannels(Me._ChannelListComboBox.Text, TimeSpan.FromSeconds(1))
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.Message)
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"Exception occurred;. {ex.ToFullBlownString}")
        Finally
            Me.ReadServiceRequestStatus()
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _ClearChannelListButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles _ClearChannelListButton.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()
            Me._ChannelListBuilder = New isr.VI.ChannelListBuilder
            Me._ChannelListComboBox.Text = Me._ChannelListBuilder.ChannelList
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.Message)
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"Exception occurred;. {ex.ToFullBlownString}")
        Finally
            Me.ReadServiceRequestStatus()
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _MemoryLocationTextBox_Validating(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles _MemoryLocationTextBox.Validating
        Try
            If Integer.Parse(Me._MemoryLocationTextBox.Text, Globalization.CultureInfo.CurrentCulture) < 1 OrElse
                Integer.Parse(Me._MemoryLocationTextBox.Text, Globalization.CultureInfo.CurrentCulture) > 100 Then
                e.Cancel = True
            End If
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.Message)
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"Exception occurred;. {ex.ToFullBlownString}")
        End Try
    End Sub

    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub MemoryLocationChannelItemTextBox_Validating(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles _MemoryLocationChannelItemTextBox.Validating
        Try
            Dim dataValue As Int32
            If Not String.IsNullOrWhiteSpace(Me._MemoryLocationChannelItemTextBox.Text.Trim) AndAlso
                Integer.TryParse(Me._MemoryLocationChannelItemTextBox.Text, System.Globalization.NumberStyles.Integer,
                                 Globalization.CultureInfo.CurrentCulture, dataValue) Then
                e.Cancel = (dataValue < 1 Or dataValue > 100)
            Else
                e.Cancel = True
            End If
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.Message)
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"Exception occurred;. {ex.ToFullBlownString}")
        End Try
    End Sub

    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _OpenAllChannelsButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles _OpenAllChannelsButton.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()
            Me.Device.ClearExecutionState()
            Me.Device.StatusSubsystem.EnableWaitComplete()
            Me.Device.RouteSubsystem.ApplyOpenAll(TimeSpan.FromSeconds(1))
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.Message)
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"Exception occurred;. {ex.ToFullBlownString}")
        Finally
            Me.ReadServiceRequestStatus()
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub RelayNumberTextBox_Validating(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles _RelayNumberTextBox.Validating
        Try
            Dim dataValue As Integer
            If Not String.IsNullOrWhiteSpace(Me._RelayNumberTextBox.Text.Trim) AndAlso
                Integer.TryParse(Me._RelayNumberTextBox.Text, System.Globalization.NumberStyles.Integer,
                                 Globalization.CultureInfo.CurrentCulture, dataValue) Then
                e.Cancel = (dataValue < 1 Or dataValue > 10)
            Else
                e.Cancel = True
            End If
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.Message)
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"Exception occurred;. {ex.ToFullBlownString}")
        End Try
    End Sub

    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _SaveToMemoryButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles _SaveToMemoryButton.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()
            Me.Device.ClearExecutionState()
            Me.Device.StatusSubsystem.EnableWaitComplete()
            Me.Device.RouteSubsystem.SaveChannelPattern(Int32.Parse(Me._MemoryLocationTextBox.Text, Globalization.CultureInfo.CurrentCulture),
                                                        TimeSpan.FromSeconds(1))
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.Message)
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"Exception occurred;. {ex.ToFullBlownString}")
        Finally
            Me.ReadServiceRequestStatus()
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _SlotNumberTextBox_Validating(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles _SlotNumberTextBox.Validating
        Try
            Dim dataValue As Integer
            If Not String.IsNullOrWhiteSpace(Me._SlotNumberTextBox.Text.Trim) AndAlso
                Integer.TryParse(Me._SlotNumberTextBox.Text, System.Globalization.NumberStyles.Integer,
                                 Globalization.CultureInfo.CurrentCulture, dataValue) Then
                e.Cancel = (dataValue < 1 Or dataValue > 10)
            Else
                e.Cancel = True
            End If
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.Message)
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"Exception occurred;. {ex.ToFullBlownString}")
        End Try
    End Sub

#End Region

#Region " CONTROL EVENT HANDLERS: SRQ "

    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _ServiceRequestMaskAddButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles _ServiceRequestMaskAddButton.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()
            Dim selectedFlag As VI.Pith.ServiceRequests = CType(Me._ServiceRequestFlagsComboBox.SelectedItem, isr.VI.Pith.ServiceRequests)
            Dim serviceByte As Byte = Byte.Parse(Me._ServiceRequestMaskTextBox.Text, Globalization.CultureInfo.CurrentCulture)
            serviceByte = Convert.ToByte(serviceByte Or selectedFlag)
            Me._ServiceRequestMaskTextBox.Text = serviceByte.ToString(Globalization.CultureInfo.CurrentCulture)
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.Message)
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"Exception occurred;. {ex.ToFullBlownString}")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _ServiceRequestMaskRemoveButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles _ServiceRequestMaskRemoveButton.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()
            Dim selectedFlag As VI.Pith.ServiceRequests = CType(Me._ServiceRequestFlagsComboBox.SelectedItem, isr.VI.Pith.ServiceRequests)
            Dim serviceByte As Byte = Byte.Parse(Me._ServiceRequestMaskTextBox.Text, Globalization.CultureInfo.CurrentCulture)
            serviceByte = Convert.ToByte(serviceByte And (Not selectedFlag))
            Me._ServiceRequestMaskTextBox.Text = serviceByte.ToString(Globalization.CultureInfo.CurrentCulture)
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.Message)
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"Exception occurred;. {ex.ToFullBlownString}")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _EnableEndOfSettlingRequestCheckBox_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles _EnableEndOfSettlingRequestToggle.CheckedChanged
        If Me.InitializingComponents OrElse sender Is Nothing OrElse e Is Nothing Then Return
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()
            If Me.Visible AndAlso Me._EnableEndOfSettlingRequestToggle.Enabled Then
                Me.Device.ToggleEndOfScanService(Me._EnableEndOfSettlingRequestToggle.Checked,
                                                 CType(Me._ServiceRequestMaskTextBox.Text, isr.VI.Pith.ServiceRequests))
            End If
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.Message)
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"Exception occurred;. {ex.ToFullBlownString}")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub ServiceRequestMaskTextBox_Validating(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles _ServiceRequestMaskTextBox.Validating
        Try
            Dim data As String = Me._ServiceRequestMaskTextBox.Text.Trim
            Dim dataValue As Integer
            If Not String.IsNullOrWhiteSpace(data) AndAlso
                Integer.TryParse(data, System.Globalization.NumberStyles.Integer,
                                 Globalization.CultureInfo.CurrentCulture, dataValue) Then
                If dataValue < 0 Or dataValue > 255 Then
                    Me._ServiceRequestMaskTextBox.Text = "0"
                    e.Cancel = True
                    Return
                End If
            Else
                Me._ServiceRequestMaskTextBox.Text = "0"
                e.Cancel = True
            End If
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.Message)
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"Exception occurred;. {ex.ToFullBlownString}")
        End Try
    End Sub

#End Region

#Region " CONTROL EVENT HANDLERS: RESET "

    ''' <summary> Clears interface. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _ClearInterfaceMenuItem_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles _ClearInterfaceMenuItem.Click
        Dim activity As String = "clearing interface"
        Dim menuItem As ToolStripMenuItem = TryCast(sender, ToolStripMenuItem)
        Try
            If menuItem IsNot Nothing Then
                Me.Cursor = Cursors.WaitCursor
                Me.ErrorProvider.Clear()
                Me.Talker.Publish(TraceEventType.Information, My.MyLibrary.TraceEventId, $"{Me.Title} {activity};. {Me.Device.ResourceNameCaption}")
                Me.StartElapsedStopwatch()
                Me.Device.ClearInterface()
                Me._TimingLabel.Text = Me.ReadElapsedTime.ToString("s\.ffff")
            End If
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.Message)
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"{Me.Title} exception {activity};. {ex.ToFullBlownString}")
        Finally
            Me.ReadServiceRequestStatus()
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    ''' <summary> Clears device (SDC). </summary>
    ''' <param name="sender"> <see cref="System.Object"/> instance of this
    ''' <see cref="System.Windows.Forms.Control"/> </param>
    ''' <param name="e">      Event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _ClearDeviceMenuItem_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles _ClearDeviceMenuItem.Click
        Dim activity As String = "clearing device active state (SDC)"
        Dim menuItem As ToolStripMenuItem = TryCast(sender, ToolStripMenuItem)
        Try
            If menuItem IsNot Nothing Then
                Me.Cursor = Cursors.WaitCursor
                Me.ErrorProvider.Clear()
                Me.Talker.Publish(TraceEventType.Information, My.MyLibrary.TraceEventId, $"{Me.Title} {activity};. {Me.Device.ResourceNameCaption}")
                Me.StartElapsedStopwatch()
                Me.Device.ClearActiveState()
                Me._TimingLabel.Text = Me.ReadElapsedTime.ToString("s\.ffff")
            End If
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.Message)
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"{Me.Title} exception {activity};. {ex.ToFullBlownString}")
        Finally
            Me.ReadServiceRequestStatus()
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    ''' <summary> Clears (CLS) the execution state menu item click. </summary>
    ''' <param name="sender"> <see cref="System.Object"/>
    '''                       instance of this
    '''                       <see cref="System.Windows.Forms.Control"/> </param>
    ''' <param name="e">      Event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _ClearExecutionStateMenuItem_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles _ClearExecutionStateMenuItem.Click
        Dim activity As String = "clearing the execution state"
        Dim menuItem As ToolStripMenuItem = TryCast(sender, ToolStripMenuItem)
        Try
            If menuItem IsNot Nothing Then
                Me.Cursor = Cursors.WaitCursor
                Me.ErrorProvider.Clear()
                Me.Talker.Publish(TraceEventType.Information, My.MyLibrary.TraceEventId, $"{Me.Title} {activity};. {Me.Device.ResourceNameCaption}")
                Me.StartElapsedStopwatch()
                Me.Device.ClearExecutionState()
                Me._TimingLabel.Text = Me.ReadElapsedTime.ToString("s\.ffff")
            End If
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.Message)
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"{Me.Title} exception {activity};. {ex.ToFullBlownString}")
        Finally
            Me.ReadServiceRequestStatus()
            Me.Cursor = Cursors.Default
        End Try
    End Sub


    ''' <summary> Resets (RST) the known state menu item click. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _ResetKnownStateMenuItem_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles _ResetKnownStateMenuItem.Click
        Dim activity As String = "resetting known state"
        Dim menuItem As ToolStripMenuItem = TryCast(sender, ToolStripMenuItem)
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()
            If menuItem IsNot Nothing Then
                Me.Talker.Publish(TraceEventType.Information, My.MyLibrary.TraceEventId, $"{Me.Title} {activity};. {Me.Device.ResourceNameCaption}")
                Me.StartElapsedStopwatch()
                Me.Device.ResetKnownState()
                Me._TimingLabel.Text = Me.ReadElapsedTime.ToString("s\.ffff")
            End If
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.Message)
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"{Me.Title} exception {activity};. {ex.ToFullBlownString}")
        Finally
            Me.ReadServiceRequestStatus()
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    ''' <summary> Initializes to known state menu item click. </summary>
    ''' <param name="sender"> <see cref="System.Object"/> instance of this
    '''                       <see cref="System.Windows.Forms.Control"/> </param>
    ''' <param name="e">      Event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _InitKnownStateMenuItem_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles _InitKnownStateMenuItem.Click
        Dim activity As String = "resetting known state"
        Dim menuItem As ToolStripMenuItem = TryCast(sender, ToolStripMenuItem)
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()
            If menuItem IsNot Nothing Then
                'Me.Talker.Publish(TraceEventType.Information, My.MyLibrary.TraceEventId, $"{Me.Title} {activity};. {Me.Device.ResourceNameCaption}")
                'Me.Device.ResetKnownState()
                activity = "initializing known state"
                Me.Talker.Publish(TraceEventType.Information, My.MyLibrary.TraceEventId, $"{Me.Title} {activity};. {Me.Device.ResourceNameCaption}")
                Me.StartElapsedStopwatch()
                Me.Device.InitKnownState()
                Me._TimingLabel.Text = Me.ReadElapsedTime.ToString("s\.ffff")
            End If
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.Message)
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"{Me.Title} exception {activity};. {ex.ToFullBlownString}")
        Finally
            Me.ReadServiceRequestStatus()
            Me.Cursor = Cursors.Default
        End Try
    End Sub

#Region " TRACE LEVEL "

    ''' <summary> Enables the trace level controls. </summary>
    Private Sub EnableTraceLevelControls()

        TalkerControlBase.ListTraceEventLevels(Me._LogTraceLevelComboBox.ComboBox)
        AddHandler Me._LogTraceLevelComboBox.ComboBox.SelectedValueChanged, AddressOf Me._LogTraceLevelComboBox_SelectedValueChanged

        TalkerControlBase.ListTraceEventLevels(Me._DisplayTraceLevelComboBox.ComboBox)
        AddHandler Me._DisplayTraceLevelComboBox.ComboBox.SelectedValueChanged, AddressOf Me._DisplayTraceLevelComboBox_SelectedValueChanged

        TalkerControlBase.SelectItem(Me._LogTraceLevelComboBox, My.Settings.TraceLogLevel)
        TalkerControlBase.SelectItem(Me._DisplayTraceLevelComboBox, My.Settings.TraceShowLevel)

    End Sub

    ''' <summary>
    ''' Event handler. Called by _LogTraceLevelComboBox for selected value changed events.
    ''' </summary>
    ''' <param name="sender"> <see cref="System.Object"/> instance of this
    '''                       <see cref="System.Windows.Forms.Control"/> </param>
    ''' <param name="e">      Event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _LogTraceLevelComboBox_SelectedValueChanged(sender As Object, e As EventArgs)
        Dim activity As String = "selecting log trace level on this instrument only"
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()
            Me.Device.ApplyTalkerTraceLevel(ListenerType.Logger,
                                            TalkerControlBase.SelectedValue(Me._LogTraceLevelComboBox, My.Settings.TraceLogLevel))
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.Message)
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"{Me.Title} exception {activity};. {ex.ToFullBlownString}")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    ''' <summary>
    ''' Event handler. Called by _DisplayTraceLevelComboBox for selected value changed events.
    ''' </summary>
    ''' <param name="sender"> <see cref="System.Object"/> instance of this
    '''                       <see cref="System.Windows.Forms.Control"/> </param>
    ''' <param name="e">      Event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _DisplayTraceLevelComboBox_SelectedValueChanged(sender As Object, e As EventArgs)
        Dim activity As String = "selecting Display trace level on this instrument only"
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()
            Me.Device.ApplyTalkerTraceLevel(ListenerType.Display,
                                            TalkerControlBase.SelectedValue(Me._DisplayTraceLevelComboBox, My.Settings.TraceShowLevel))
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.Message)
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"{Me.Title} exception {activity};. {ex.ToFullBlownString}")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

#End Region

#End Region

#Region " CONTROL EVENT HANDLERS: SESSION "

    ''' <summary> Handles Reads Status Byte Menu Item click event. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _ReadStatusByteMenuItem_Click(ByVal sender As Object, e As System.EventArgs) Handles _ReadStatusByteMenuItem.Click
        If Me.InitializingComponents OrElse sender Is Nothing OrElse e Is Nothing Then Return
        Dim activity As String = "reading status byte"
        Dim menuItem As ToolStripMenuItem = TryCast(sender, ToolStripMenuItem)
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()
            If menuItem IsNot Nothing Then
                Me.StartElapsedStopwatch()
                Me.ReadServiceRequestStatus()
                Me._TimingLabel.Text = Me.ReadElapsedTime.ToString("s\.ffff")
            End If
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.ToString)
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"{Me.Title} exception {activity};. {ex.ToFullBlownString}")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    ''' <summary> Toggles session message tracing. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _SessionNotificationLevelComboBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles _SessionNotificationLevelComboBox.SelectedIndexChanged
        If Me.InitializingComponents OrElse sender Is Nothing OrElse e Is Nothing Then Return
        Dim activity As String = "selecting session notification level"
        Dim combo As Core.Controls.ToolStripComboBox = TryCast(sender, Core.Controls.ToolStripComboBox)
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()
            If combo IsNot Nothing Then
                Me.Talker.Publish(TraceEventType.Information, My.MyLibrary.TraceEventId, $"{Me.Title} {activity};. {Me.Device.ResourceNameCaption}")
                Me.Device.MessageNotificationLevel = VI.Pith.SessionBase.SelectedValue(combo, NotifySyncLevel.None)
            End If
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.Message)
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"{Me.Title} exception {activity};. {ex.ToFullBlownString}")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    ''' <summary> Toggles the session service request handler . </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _SessionServiceRequestHandlerEnabledMenuItem_CheckStateChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles _SessionServiceRequestHandlerEnabledMenuItem.CheckStateChanged
        If Me.InitializingComponents OrElse sender Is Nothing OrElse e Is Nothing Then Return
        Dim activity As String = "Toggle session service request handling"
        Dim menuItem As ToolStripMenuItem = TryCast(sender, ToolStripMenuItem)
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()
            If menuItem IsNot Nothing AndAlso menuItem.Checked <> Me.Device.Session.ServiceRequestEventEnabled Then
                Me.Talker.Publish(TraceEventType.Information, My.MyLibrary.TraceEventId, $"{Me.Title} {activity};. {Me.Device.ResourceNameCaption}")
                If menuItem IsNot Nothing AndAlso menuItem.Checked Then
                    Me.Device.Session.EnableServiceRequest()
                    If Me._ServiceRequestEnableBitmaskNumeric.Value = 0 Then
                        Me.Device.StatusSubsystem.EnableServiceRequest(VI.Pith.ServiceRequests.All)
                    Else
                        Me.Device.StatusSubsystem.EnableServiceRequest(CType(Me._ServiceRequestEnableBitmaskNumeric.Value, VI.Pith.ServiceRequests))
                    End If
                Else
                    Me.Device.Session.DisableServiceRequest()
                    Me.Device.StatusSubsystem.EnableServiceRequest(VI.Pith.ServiceRequests.None)
                End If
                Me.Device.StatusSubsystem.ReadEventRegisters()
            End If
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.Message)
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"{Me.Title} exception {activity};. {ex.ToFullBlownString}")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    ''' <summary> Toggles the Device service request handler . </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _DeviceServiceRequestHandlerEnabledMenuItem_CheckStateChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles _DeviceServiceRequestHandlerEnabledMenuItem.CheckStateChanged
        If Me.InitializingComponents OrElse sender Is Nothing OrElse e Is Nothing Then Return
        Dim activity As String = "Toggle device service request handling"
        Dim menuItem As ToolStripMenuItem = TryCast(sender, ToolStripMenuItem)
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()
            If menuItem IsNot Nothing AndAlso menuItem.Checked <> Me.Device.DeviceServiceRequestHandlerAdded Then
                Me.Talker.Publish(TraceEventType.Information, My.MyLibrary.TraceEventId, $"{Me.Title} {activity};. {Me.Device.ResourceNameCaption}")
                If menuItem IsNot Nothing AndAlso menuItem.Checked Then
                    Me.AddServiceRequestEventHandler()
                Else
                    Me.RemoveServiceRequestEventHandler()
                End If
                Me.Device.StatusSubsystem.ReadEventRegisters()
            End If
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.Message)
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"{Me.Title} exception {activity};. {ex.ToFullBlownString}")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

#End Region

#Region " CONTROL EVENT HANDLERS: TRIGGER "

    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _AbortButton_Click(sender As Object, e As EventArgs) Handles _AbortButton.Click

        If Me.InitializingComponents OrElse sender Is Nothing OrElse e Is Nothing Then Return
        Dim activity As String = "aborting trigger plan"
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()
            If Me.IsDeviceOpen Then
                Me.Talker.Publish(TraceEventType.Information, My.MyLibrary.TraceEventId, $"{Me.Title} {activity};. {Me.Device.ResourceNameCaption}")
                Me.Device.TriggerSubsystem.Abort()
            End If
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.Message)
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"{Me.Title} exception {activity};. {ex.ToFullBlownString}")
        Finally
            Me.ReadServiceRequestStatus()
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _ApplyTriggerPlanButton_Click(sender As Object, e As EventArgs) Handles _ApplyTriggerPlanButton.Click
        If Me.InitializingComponents OrElse sender Is Nothing OrElse e Is Nothing Then Return
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()
            If Not Me.DesignMode AndAlso sender IsNot Nothing Then
                Me.Device.RouteSubsystem.ApplyOpenAll(TimeSpan.FromSeconds(1))
                Me.Device.TriggerSubsystem.ApplyTriggerCount(CInt(Me._ChannelLayerCountNumeric.Value))
                Me.Device.TriggerSubsystem.ApplyTriggerSource(Scpi.TriggerSources.External)
            End If
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.Message)
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"{ex.Message} occurred setting the trigger plan;. {ex.ToFullBlownString}")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _InitiateButton_Click(sender As Object, e As EventArgs) Handles _InitiateButton.Click
        If Me.InitializingComponents OrElse sender Is Nothing OrElse e Is Nothing Then Return
        Dim activity As String = "Initiating trigger plan"
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()
            Me.Device.ClearExecutionState()
            Me.Talker.Publish(TraceEventType.Information, My.MyLibrary.TraceEventId, $"{Me.Title} {activity};. {Me.Device.ResourceNameCaption}")
            Me.Device.TriggerSubsystem.Initiate()
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.Message)
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"{Me.Title} exception {activity};. {ex.ToFullBlownString}")
        Finally
            Me.ReadServiceRequestStatus()
            Me.Cursor = Cursors.Default
        End Try

    End Sub

    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _ConfigureExternalScan_Click(sender As Object, e As EventArgs) Handles _ConfigureExternalScan.Click
        If Me.InitializingComponents OrElse sender Is Nothing OrElse e Is Nothing Then Return
        Dim activity As String = "Configuring external scan trigger plan"
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.ErrorProvider.Clear()
            Me.Device.ClearExecutionState()
            ' enable service requests
            'Me.EnableServiceRequestEventHandler()
            'Me.Device.StatusSubsystem.EnableServiceRequest(VI.Pith.ServiceRequests.All)

            Me.Talker.Publish(TraceEventType.Information, My.MyLibrary.TraceEventId, $"{Me.Title} {activity};. {Me.Device.ResourceNameCaption}")
            Me.Device.TriggerSubsystem.ApplyContinuousEnabled(False)
            Me.Device.TriggerSubsystem.Abort()
            Me.Device.StatusSubsystem.EnableWaitComplete()
            Me.Device.RouteSubsystem.WriteOpenAll(TimeSpan.FromSeconds(1))
            Me.ReadServiceRequestStatus()

            Me.Device.RouteSubsystem.ApplyOpenAll(TimeSpan.FromSeconds(1))
            Me.Device.RouteSubsystem.QueryClosedChannels()
            Me.Device.RouteSubsystem.ApplyScanList(Me._ChannelListComboBox.Text) ' "(@1!1:1!10)"
            Me.Device.ArmLayer1Subsystem.ApplyArmSource(Scpi.ArmSources.Immediate)
            Me.Device.ArmLayer1Subsystem.ApplyArmCount(1)
            Me.Device.ArmLayer2Subsystem.ApplyArmSource(Scpi.ArmSources.Immediate)
            Me.Device.ArmLayer2Subsystem.ApplyArmCount(1)
            Me.Device.ArmLayer2Subsystem.ApplyDelay(TimeSpan.Zero)
            Me.Device.TriggerSubsystem.ApplyTriggerSource(Scpi.TriggerSources.External)
            Me.Device.TriggerSubsystem.ApplyTriggerCount(9999) ' in place of infinite
            Me.Device.TriggerSubsystem.ApplyDelay(TimeSpan.Zero)
            Me.Device.TriggerSubsystem.ApplyDirection(Scpi.Direction.Source)
            Me.StatusLabel.Text = "Ready: Initiate 7001 and then meter"
        Catch ex As Exception
            Me.ErrorProvider.Annunciate(sender, ex.Message)
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, $"{Me.Title} exception {activity};. {ex.ToFullBlownString}")
        Finally
            Me.ReadServiceRequestStatus()
            Me.Cursor = Cursors.Default
        End Try

    End Sub

#End Region

#Region " READ AND WRITE "

    ''' <summary> Executes the property changed action. </summary>
    ''' <param name="sender">       Source of the event. </param>
    ''' <param name="propertyName"> Name of the property. </param>
    Private Overloads Sub OnPropertyChanged(ByVal sender As Instrument.SimpleReadWriteControl, ByVal propertyName As String)
        If sender IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(propertyName) Then
            Select Case propertyName
                Case NameOf(Instrument.SimpleReadWriteControl.StatusMessage)
                    Me.StatusLabel.Text = sender.StatusMessage
                Case NameOf(Instrument.SimpleReadWriteControl.ServiceRequestValue)
                    Me.StatusRegisterLabel.Text = $"0x{sender.ServiceRequestValue:X2}"
            End Select
        End If
    End Sub

    ''' <summary> Event handler. Called by <see crefname="_SimpleReadWriteControl"/> for property changed
    ''' events. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Property Changed event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _SimpleReadWriteControl_PropertyChanged(ByVal sender As Object, ByVal e As PropertyChangedEventArgs) Handles _SimpleReadWriteControl.PropertyChanged
        Try
            If Me.InvokeRequired Then
                Me.Invoke(New Action(Of Object, PropertyChangedEventArgs)(AddressOf Me._SimpleReadWriteControl_PropertyChanged), New Object() {sender, e})
            Else
                Me.OnPropertyChanged(TryCast(sender, Instrument.SimpleReadWriteControl), e?.PropertyName)
            End If
        Catch ex As Exception
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId,
                               "Exception handling {0} property change;. {1}",
                               e?.PropertyName, ex.ToFullBlownString)
        End Try
    End Sub

#End Region

#Region " TALKER "

    ''' <summary> Identifies talkers. </summary>
    Protected Overrides Sub IdentifyTalkers()
        MyBase.IdentifyTalkers()
        My.MyLibrary.Identify(Talker)
    End Sub

    ''' <summary> Assigns talker. </summary>
    ''' <param name="talker"> The talker. </param>
    Public Overrides Sub AssignTalker(talker As ITraceMessageTalker)
        Me._SimpleReadWriteControl.AssignTalker(talker)
        MyBase.AssignTalker(talker)
    End Sub

    ''' <summary> Applies the trace level to all listeners to the specified type. </summary>
    ''' <param name="listenerType"> Type of the listener. </param>
    ''' <param name="value">        The value. </param>
    Public Overrides Sub ApplyListenerTraceLevel(ByVal listenerType As ListenerType, ByVal value As TraceEventType)
        Me._SimpleReadWriteControl.ApplyListenerTraceLevel(listenerType, value)
        ' this should apply only to the listeners associated with this form
        ' MyBase.ApplyListenerTraceLevel(listenerType, value)
    End Sub

    Public Overrides Sub ApplyListenerTraceLevels(ByVal talker As ITraceMessageTalker)
        Me._SimpleReadWriteControl.ApplyListenerTraceLevels(talker)
        ' this should apply only to the listeners associated with this form
        ' MyBase.ApplyListenerTraceLevels(talker)
    End Sub

    Public Overrides Sub ApplyTalkerTraceLevels(ByVal talker As ITraceMessageTalker)
        Me._SimpleReadWriteControl.ApplyTalkerTraceLevels(talker)
        MyBase.ApplyTalkerTraceLevels(talker)
    End Sub

#End Region

End Class

