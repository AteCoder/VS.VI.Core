﻿Imports System.Threading
Imports System.ComponentModel
Imports isr.Core.Pith
Imports isr.Core.Pith.EventHandlerExtensions
Imports isr.Core.Pith.EscapeSequencesExtensions
Imports isr.Core.Pith.StopwatchExtensions
''' <summary> Defines the contract that must be implemented by Devices. </summary>
''' <license> (c) 2012 Integrated Scientific Resources, Inc.<para>
''' Licensed under The MIT License. </para><para>
''' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
''' BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
''' NON-INFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
''' DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
''' OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.         
''' </para> </license>
''' <history date="9/26/2012" by="David" revision="1.0.4652"> Created. </history>
Public MustInherit Class DeviceBase
    Inherits PropertyPublisherBase
    Implements IPresettablePropertyPublisher, ITalker

#Region " CONSTRUCTORS  and  DESTRUCTORS "

    ''' <summary> Initializes a new instance of the <see cref="DeviceBase" /> class. </summary>
    Protected Sub New()
        Me.New(isr.VI.SessionFactory.Get.Factory.CreateSession())
        Me._IsSessionOwner = True
    End Sub

    ''' <summary> Initializes a new instance of the <see cref="DeviceBase" /> class. </summary>
    ''' <param name="visaSession"> A reference to a <see cref="VI.SessionBase">message based
    ''' session</see>. </param>
    Protected Sub New(ByVal visaSession As VI.SessionBase)
        MyBase.New()
        If visaSession Is Nothing Then
            Me._Session = isr.VI.SessionFactory.Get.Factory.CreateSession()
            Me._IsSessionOwner = True
        Else
            Me._Session = visaSession
        End If
        Me._Subsystems = New SubsystemCollection
        Me._ResourcesFilter = VI.ResourceNamesManager.BuildInstrumentFilter
        Me._InitializeTimeout = TimeSpan.FromMilliseconds(30000)
        Me._UsingSyncServiceRequestHandler = True
        Me._Talker = New TraceMessageTalker
        Me._DeviceClearRefractoryPeriod = TimeSpan.FromMilliseconds(1000)
        Me._ResetRefractoryPeriod = TimeSpan.FromMilliseconds(1000)
        Me._InitRefractoryPeriod = TimeSpan.FromMilliseconds(100)
        Me._ClearRefractoryPeriod = TimeSpan.FromMilliseconds(100)
        Me._ResourceTitle = DeviceBase.DefaultResourceTitle
        Me._ResourceName = DeviceBase.ResourceNameClosed
        Me._CapturedSyncContext = SynchronizationContext.Current
    End Sub

#Region " I Disposable Support "

    ''' <summary>
    ''' Releases the unmanaged resources used by the <see cref="T:System.Windows.Forms.Control" />
    ''' and its child controls and optionally releases the managed resources.
    ''' </summary>
    ''' <param name="disposing"> true to release both managed and unmanaged resources; false to
    '''                          release only unmanaged resources. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If Not Me.IsDisposed AndAlso disposing Then
                Me.Talker?.Listeners.Clear()
                Me._Talker = Nothing
                Me.SessionMessagesTraceEnabled = False
                Me.Session?.DisableServiceRequest()
                Me.RemoveServiceRequestEventHandler()
                Me.RemoveEventHandler(Me.ServiceRequestedEvent)
                Me.RemoveOpeningEventHandler(Me.OpeningEvent)
                Me.RemoveOpenedEventHandler(Me.OpenedEvent)
                Me.RemoveClosingEventHandler(Me.ClosingEvent)
                Me.RemoveClosedEventHandler(Me.ClosedEvent)
                Me.RemoveInitializingEventHandler(Me.InitializingEvent)
                Me.RemoveInitializedEventHandler(Me.InitializedEvent)
                If Me._IsSessionOwner Then
                    Try
                        Me.CloseSession()
                    Catch ex As Exception
                        Debug.Assert(Not Debugger.IsAttached, ex.ToString)
                    End Try
                    Try
                        If Me._Session IsNot Nothing Then
                            Me.Session.Dispose()
                        End If
                        ' release the session
                        ' Trying to null the session raises an ObjectDisposedException 
                        ' if session service request handler was not released. 
                        Me._Session = Nothing
                    Catch ex As ObjectDisposedException
                        Debug.Assert(Not Debugger.IsAttached, ex.ToString)
                    End Try
                End If
            End If
        Catch ex As Exception
            Debug.Assert(Not Debugger.IsAttached, ex.ToString)
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

#End Region

#End Region

#Region " I PRESETTABLE "

    Private _InitializeTimeout As TimeSpan
    ''' <summary> Gets or sets the time out for doing a reset and clear on the instrument. </summary>
    ''' <value> The connect timeout. </value>
    Public Property InitializeTimeout() As TimeSpan
        Get
            Return Me._InitializeTimeout
        End Get
        Set(ByVal value As TimeSpan)
            If Not value.Equals(Me.InitializeTimeout) Then
                Me._InitializeTimeout = value
                Me.AsyncNotifyPropertyChanged(NameOf(Me.InitializeTimeout))
            End If
        End Set
    End Property

    ''' <summary> Clears the Device. Issues <see cref="StatusSubsystemBase.ClearActiveState">Selective device clear</see>. </summary>
    Public Overridable Sub ClearActiveState()
        Me.Talker.Publish(TraceEventType.Verbose, My.MyLibrary.TraceEventId, $"{Me.ResourceName} Clearing active state")
    End Sub

    ''' <summary> Clears the queues and resets all registers to zero. Sets the subsystem properties to
    ''' the following CLS default values:<para>
    ''' </para> </summary>
    ''' <remarks> *CLS. </remarks>
    Public Overridable Sub ClearExecutionState() Implements IPresettable.ClearExecutionState
        Me.Talker.Publish(TraceEventType.Verbose, My.MyLibrary.TraceEventId, $"{Me.ResourceName} Clearing execution state")
        Me.Subsystems.ClearExecutionState()
    End Sub

    ''' <summary> Initializes the Device. Used after reset to set a desired initial state. </summary>
    ''' <remarks> Use this to customize the reset. </remarks>
    Public Overridable Sub InitKnownState() Implements IPresettable.InitKnownState
        Me.Talker.Publish(TraceEventType.Verbose, My.MyLibrary.TraceEventId, $"{Me.ResourceName} Initializing known state")
        Me.Subsystems.InitKnownState()
        Me.IsInitialized = True
    End Sub

    ''' <summary> Resets the Device to its known state. </summary>
    ''' <remarks> *RST. </remarks>
    Public Overridable Sub PresetKnownState() Implements IPresettable.PresetKnownState
        Me.Talker.Publish(TraceEventType.Verbose, My.MyLibrary.TraceEventId, $"{Me.ResourceName} Presetting known state")
        Me.Subsystems.PresetKnownState()
    End Sub

    ''' <summary> Resets the Device to its known state. </summary>
    ''' <remarks> *RST. </remarks>
    Public Overridable Sub ResetKnownState() Implements IPresettable.ResetKnownState
        Me.Talker.Publish(TraceEventType.Verbose, My.MyLibrary.TraceEventId, $"{Me.ResourceName} Resetting known state")
        Me.Subsystems.ResetKnownState()
    End Sub

    Private _DeviceClearRefractoryPeriod As TimeSpan
    ''' <summary> Gets the device clear refractory period. </summary>
    ''' <value> The device clear refractory period. </value>
    Public Property DeviceClearRefractoryPeriod As TimeSpan
        Get
            Return Me._DeviceClearRefractoryPeriod
        End Get
        Set(value As TimeSpan)
            If Not value.Equals(Me.DeviceClearRefractoryPeriod) Then
                Me._DeviceClearRefractoryPeriod = value
                Me.AsyncNotifyPropertyChanged()
            End If
        End Set
    End Property

    Private _ResetRefractoryPeriod As TimeSpan
    ''' <summary> Gets the reset refractory period. </summary>
    ''' <value> The reset refractory period. </value>
    Public Property ResetRefractoryPeriod As TimeSpan
        Get
            Return Me._ResetRefractoryPeriod
        End Get
        Set(value As TimeSpan)
            If Not value.Equals(Me.ResetRefractoryPeriod) Then
                Me._ResetRefractoryPeriod = value
                Me.AsyncNotifyPropertyChanged()
            End If
        End Set
    End Property

    Private _InitRefractoryPeriod As TimeSpan
    ''' <summary> Gets the initialize refractory period. </summary>
    ''' <value> The initialize refractory period. </value>
    Public Property InitRefractoryPeriod As TimeSpan
        Get
            Return Me._InitRefractoryPeriod
        End Get
        Set(value As TimeSpan)
            If Not value.Equals(Me.DeviceClearRefractoryPeriod) Then
                Me._InitRefractoryPeriod = value
                Me.AsyncNotifyPropertyChanged()
            End If
        End Set
    End Property

    Private _ClearRefractoryPeriod As TimeSpan
    ''' <summary> Gets the clear refractory period. </summary>
    ''' <value> The clear refractory period. </value>
    Public Property ClearRefractoryPeriod As TimeSpan
        Get
            Return Me._ClearRefractoryPeriod
        End Get
        Set(value As TimeSpan)
            If Not value.Equals(Me.ClearRefractoryPeriod) Then
                Me._ClearRefractoryPeriod = value
                Me.AsyncNotifyPropertyChanged()
            End If
        End Set
    End Property

    ''' <summary>
    ''' Resets, clears and initializes the device. Starts with issuing a selective-device-clear, reset (RST),
    ''' Clear Status (CLS, and clear error queue) and initialize.
    ''' </summary>
    ''' <remarks> David, 1/8/2016. </remarks>
    ''' <param name="deviceClearRefractoryPeriod"> The device clear refractory period. </param>
    ''' <param name="resetRefractoryPeriod">       The reset refractory period. </param>
    ''' <param name="initRefractoryPeriod">        The initialize refractory period. </param>
    ''' <param name="clearRefractoryPeriod">       The clear refractory period. </param>
    Public Sub ResetClearInit(ByVal deviceClearRefractoryPeriod As TimeSpan,
                              ByVal resetRefractoryPeriod As TimeSpan,
                              ByVal initRefractoryPeriod As TimeSpan,
                              ByVal clearRefractoryPeriod As TimeSpan)

        ' issues selective device clear.
        Me.ClearActiveState()
        If Me.Session.IsSessionOpen Then Stopwatch.StartNew.Wait(deviceClearRefractoryPeriod)

        ' reset device
        Me.ResetKnownState()
        If Me.Session.IsSessionOpen Then Stopwatch.StartNew.Wait(resetRefractoryPeriod)

        ' Clear the device Status and set more defaults
        Me.ClearExecutionState()
        If Me.Session.IsSessionOpen Then Stopwatch.StartNew.Wait(clearRefractoryPeriod)

        ' initialize the device must be done after clear (5892)
        Me.InitKnownState()
        If Me.Session.IsSessionOpen Then Stopwatch.StartNew.Wait(initRefractoryPeriod)


    End Sub

    ''' <summary>
    ''' Resets, clears and initializes the device. Starts with issuing a selective-device-clear, reset (RST),
    ''' Clear Status (CLS, and clear error queue) and initialize.
    ''' </summary>
    Public Overridable Sub ResetClearInit()
        Me.ResetClearInit(Me.DeviceClearRefractoryPeriod, Me.ResetRefractoryPeriod,
                          Me.InitRefractoryPeriod, Me.ClearRefractoryPeriod)
    End Sub

    ''' <summary>
    ''' Resets, clears and initializes the device. Starts with issuing a selective-device-clear, reset (RST),
    ''' Clear Status (CLS, and clear error queue) and initialize.
    ''' </summary>
    ''' <param name="timeout"> The timeout to use. This allows using a longer timeout than the
    ''' minimal timeout set for the session. Typically, a source meter requires a 5000 milliseconds
    ''' timeout. </param>
    Public Overridable Sub ResetClearInit(ByVal timeout As TimeSpan)
        Try
            Me.Session.StoreTimeout(timeout)
            Me.ResetClearInit()
        Catch
            Throw
        Finally
            Me.Session.RestoreTimeout()
        End Try
    End Sub

#End Region

#Region " SESSION "

    ''' <summary> true if this object is session owner. </summary>
    Private _IsSessionOwner As Boolean

    ''' <summary> Executes the property changed action. </summary>
    ''' <remarks> David, 1/21/2016. </remarks>
    ''' <param name="sender">       Source of the event. </param>
    ''' <param name="propertyName"> Name of the property. </param>
    Private Sub OnPropertyChanged(ByVal sender As SessionBase, ByVal propertyName As String)
        If sender Is Nothing OrElse String.IsNullOrWhiteSpace(propertyName) Then Return
        Select Case propertyName
            Case NameOf(sender.IsDeviceOpen)
                If sender.IsDeviceOpen Then
                    AddHandler Me.Session.PropertyChanged, AddressOf Me.SessionPropertyChanged
                Else
                    RemoveHandler Me.Session.PropertyChanged, AddressOf Me.SessionPropertyChanged
                End If
            Case NameOf(sender.ResourceTitle)
                Me.ResourceTitle = sender.ResourceTitle
            Case NameOf(sender.ResourceName)
                Me.ResourceName = sender.ResourceName
        End Select
    End Sub

    ''' <summary> Session property changed. </summary>
    ''' <remarks> David, 1/21/2016. </remarks>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Property changed event information. </param>
    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub _Session_PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Handles _Session.PropertyChanged
        Try
            Me.OnPropertyChanged(TryCast(sender, SessionBase), e?.PropertyName)
        Catch ex As Exception
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId,
                               "Exception handling property '{0}' changed event;. Details: {1}", e.PropertyName, ex)
        End Try
    End Sub

    ''' <summary> The session. </summary>
    Private WithEvents _Session As SessionBase

    ''' <summary> Gets the session. </summary>
    ''' <value> The session. </value>
    Public ReadOnly Property Session As SessionBase
        Get
            Return Me._Session
        End Get
    End Property

    ''' <summary> Gets or sets the default resource title. </summary>
    Public Const DefaultResourceTitle As String = "Device"

    Private _ResourceTitle As String
    ''' <summary> Gets the resource title. </summary>
    ''' <value> The resource title. </value>
    Public Property ResourceTitle As String
        Get
            Return Me._ResourceTitle
        End Get
        Set(value As String)
            If Not String.Equals(Me.ResourceTitle, value) Then
                Me._ResourceTitle = value
                Me.AsyncNotifyPropertyChanged()
            End If
            If Me.Session IsNot Nothing Then Me.Session.ResourceTitle = value
        End Set
    End Property

    ''' <summary> Gets or sets the resource name closed. </summary>
    Public Const ResourceNameClosed As String = "<closed>"

    Private _ResourceName As String
    ''' <summary> Gets the name of the resource. </summary>
    ''' <value> The name of the resource or &lt;closed&gt; if not open. </value>
    Public Property ResourceName As String
        Get
            Return Me._ResourceName
        End Get
        Set(ByVal value As String)
            If Not String.Equals(Me.ResourceName, value) Then
                Me._ResourceName = value
                Me.AsyncNotifyPropertyChanged()
            End If
        End Set
    End Property

#End Region

#Region " PUBLISHER "

    ''' <summary> Resumes the property events. </summary>
    Public Overrides Sub ResumePublishing()
        MyBase.ResumePublishing()
        Me.Subsystems.ResumePublishing()
    End Sub

    ''' <summary> Suppresses the property events. </summary>
    Public Overrides Sub SuspendPublishing()
        MyBase.SuspendPublishing()
        Me.Subsystems.SuspendPublishing()
    End Sub

#End Region

#Region " RESOURCE NAME PATTERN "

    ''' <summary> Gets or sets the resources search pattern. </summary>
    ''' <value> The resources search pattern. </value>
    Public Property ResourcesFilter As String

#End Region

#Region " OPEN / CLOSE "

    Private _IsInitialized As Boolean

    ''' <summary> Gets or sets the Initialized sentinel of the device.
    '''           The device is ready after it is initialized. </summary>
    ''' <value> <c>True</c> if hardware device is Initialized; <c>False</c> otherwise. </value>
    Public Overridable Property IsInitialized As Boolean
        Get
            Return Me._IsInitialized
        End Get
        Set(ByVal value As Boolean)
            If Not Me.IsInitialized.Equals(value) Then
                Me._IsInitialized = value
                Me.AsyncNotifyPropertyChanged(NameOf(Me.IsInitialized))
                Windows.Forms.Application.DoEvents()
                Me.Talker?.Publish(TraceEventType.Information, My.MyLibrary.TraceEventId,
                                   $"{Me.ResourceTitle} {IIf(Me.IsInitialized, "initialized", "not ready")};. ")
                Windows.Forms.Application.DoEvents()
            End If
        End Set
    End Property

    ''' <summary> Gets or sets the Enabled sentinel of the device.
    '''           A device is enabled when hardware can be used. </summary>
    ''' <value> <c>True</c> if hardware device is enabled; <c>False</c> otherwise. </value>
    Public Overridable Property Enabled As Boolean
        Get
            Return Me.Session.Enabled
        End Get
        Set(ByVal value As Boolean)
            If Me.Enabled <> value Then
                Me.Session.Enabled = value
                Me.AsyncNotifyPropertyChanged(NameOf(Me.Enabled))
                Windows.Forms.Application.DoEvents()
                Me.Talker?.Publish(TraceEventType.Information, My.MyLibrary.TraceEventId,
                                   $"{Me.ResourceTitle} {IIf(Me.Enabled, "enabled", "disabled")};. ")
                Windows.Forms.Application.DoEvents()
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets a value indicating whether the device is open. See also
    ''' <see cref="SessionBase.IsDeviceOpen"/>.
    ''' </summary>
    ''' <value> <c>True</c> if the device has an open session; otherwise, <c>False</c>. </value>
    Public ReadOnly Property IsDeviceOpen As Boolean
        Get
            Return Me.Session IsNot Nothing AndAlso Me.Session.IsDeviceOpen
        End Get
    End Property

    ''' <summary> Allows the derived device to take actions before opening. </summary>
    ''' <param name="e"> Event information to send to registered event handlers. </param>
    ''' <remarks> This override should occur as the first call of the overriding method. </remarks>
    Protected Overridable Sub OnOpening(ByVal e As ComponentModel.CancelEventArgs)
        If e Is Nothing Then Throw New ArgumentNullException(NameOf(e))
        If Me.Subsystems?.Any Then
            e.Cancel = True
            Dim msg As String = Me.Talker?.Publish(TraceEventType.Warning, My.MyLibrary.TraceEventId,
                                                   $"Aborting attempt to add subsystems on top of existing subsystems for resource '{ResourceName}';. ")
            Debug.Assert(Not Debugger.IsAttached, msg)
        Else
            Me.IsInitialized = False
            Me.SuspendPublishing()
            Me.SyncNotifyOpening(e)
        End If
    End Sub

    ''' <summary> Allows the derived device to take actions after opening. </summary>
    ''' <remarks>
    ''' This override should occur as the last call of the overriding method. Use this notification
    ''' to add subsystems to this device.
    ''' </remarks>
    Protected Overridable Sub OnOpened()
        Me.AsyncNotifyPropertyChanged(NameOf(Me.IsDeviceOpen))
        ' 2016/01/18: this was done before adding listeners, which was useful when using the device
        ' as a class in a 'meter'. As a result, the actions taken when handling the Opened event, 
        ' such as Reset and Initialize do not get reported. 
        ' The solution was to add the device Initialize event to process publishing and initialization of device
        ' and subsystems.
        ' Use this notification to add subsystems to this device.
        Me.SyncNotifyOpened(System.EventArgs.Empty)
    End Sub

    ''' <summary> Executes the initializing actions. </summary>
    ''' <remarks> David, 1/20/2016. </remarks>
    Protected Overridable Sub OnInitializing(ByVal e As ComponentModel.CancelEventArgs)
        If e Is Nothing Then Throw New ArgumentNullException(NameOf(e))
        Me.SyncNotifyInitializing(e)

        ' add listens and enable publishing while initializing the device.
        Me.AddSubsystemListeners()

        ' 2/18/16: replaces calls to reset and then init known states
        Me.ResetClearInit()

    End Sub

    Protected Overridable Sub OnInitialized()
        ' publish 
        Me.ResumePublishing()
        Me.Publish()
        Me.SyncNotifyInitialized(System.EventArgs.Empty)
    End Sub

    ''' <summary> Opens the session. </summary>
    ''' <remarks> Register the device trace notifier before opening the session. </remarks>
    ''' <exception cref="OperationFailedException"> Thrown when operation failed to execute. </exception>
    ''' <param name="resourceName"> Name of the resource. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Public Overridable Sub OpenSession(ByVal resourceName As String, ByVal resourceTitle As String)
        Dim success As Boolean = False
        Try
            If Me.Enabled Then Me.Talker?.Publish(TraceEventType.Verbose, My.MyLibrary.TraceEventId, $"Opening session to {resourceName};. ")
            Me.Session.ResourceTitle = resourceTitle
            Me.Session.OpenSession(resourceName, resourceTitle)
            If Me.Session.IsSessionOpen Then
                Me.Talker?.Publish(TraceEventType.Verbose, My.MyLibrary.TraceEventId, $"Session open to {resourceName};. ")
            ElseIf Me.Session.Enabled Then
                Throw New OperationFailedException($"Unable to open session to {resourceName};. ")
            ElseIf Not Me.IsDeviceOpen Then
                Throw New OperationFailedException($"Unable to emulate {resourceName};. ")
            End If
            If Me.Session.IsSessionOpen OrElse (Me.IsDeviceOpen AndAlso Not Me.Session.Enabled) Then

                Dim e As New ComponentModel.CancelEventArgs
                Me.OnOpening(e)

                If e.Cancel Then
                    Throw New OperationCanceledException($"Opening {resourceTitle}:{resourceName} canceled;. ")
                End If

                Me.OnOpened()

                If Me.IsDeviceOpen Then

                    Me.OnInitializing(e)
                    If e.Cancel Then Throw New OperationCanceledException($"{resourceTitle}:{resourceName} initialization canceled;. ")

                    Me.OnInitialized()

                Else
                    Throw New OperationFailedException($"Opening {resourceTitle}:{resourceName} device failed;. ")
                End If

            Else
                Throw New OperationFailedException($"Opening {resourceTitle}:{resourceName} session failed;. ")
            End If
            success = True
        Catch
            Throw
        Finally
            If Not success Then
                Me.Talker.Publish(TraceEventType.Warning, My.MyLibrary.TraceEventId, $"{resourceName} failed connecting. Disconnecting.")
                Me.TryCloseSession()
            End If
        End Try
    End Sub

    ''' <summary> Try open session. </summary>
    ''' <remarks> David, 10/13/2016. </remarks>
    ''' <param name="resourceName">  Name of the resource. </param>
    ''' <param name="resourceTitle"> The resource title. </param>
    ''' <param name="e">             Cancel details event information. </param>
    ''' <returns> <c>True</c> if success; <c>False</c> otherwise. </returns>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Public Overridable Function TryOpenSession(ByVal resourceName As String, ByVal resourceTitle As String, ByVal e As CancelDetailsEventArgs) As Boolean
        If e Is Nothing Then Throw New ArgumentNullException(NameOf(e))
        Try
            Me.OpenSession(resourceName, resourceTitle)
        Catch ex As OperationFailedException
            e.RegisterCancellation($"Exception opening {resourceTitle}:{resourceName};. Details: {ex.ToString}")
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, e.Details)
        End Try
        Return Not e.Cancel AndAlso Me.IsDeviceOpen
    End Function

    ''' <summary> Allows the derived device to take actions before closing. </summary>
    ''' <param name="e"> Event information to send to registered event handlers. </param>
    ''' <remarks> This override should occur as the first call of the overriding method. </remarks>
    Protected Overridable Sub OnClosing(ByVal e As ComponentModel.CancelEventArgs)
        If e Is Nothing Then Throw New ArgumentNullException(NameOf(e))
        If Not e.Cancel Then Me.SyncNotifyClosing(e)
        If Not e.Cancel Then
            Me.IsInitialized = False
            Me.SuspendPublishing()
        End If
    End Sub

    ''' <summary> Allows the derived device to take actions after closing. </summary>
    ''' <remarks> This override should occur as the last call of the overriding method. </remarks>
    Protected Overridable Sub OnClosed()
        Me.ResumePublishing()
        Me.AsyncNotifyPropertyChanged(NameOf(Me.IsDeviceOpen))
        Me.SyncNotifyClosed(System.EventArgs.Empty)
    End Sub

    ''' <summary> Closes the session. </summary>
    ''' <exception cref="OperationFailedException"> Thrown when operation failed to execute. </exception>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Public Overridable Sub CloseSession()
        Try
            ' check if already closed.
            If Me.IsDisposed OrElse Not Me.IsDeviceOpen Then  Return
            Dim e As New ComponentModel.CancelEventArgs
            Me.OnClosing(e)
            If e.Cancel Then Return
            Me.RemoveServiceRequestEventHandler()
            Me.Session.DisableServiceRequest()
            Me.Session.CloseSession()
            Me.OnClosed()
        Catch ex As NativeException
            Throw New OperationFailedException("Native Exception occurred closing the session.", ex)
        Catch ex As Exception
            Throw New OperationFailedException("Exception occurred closing the session.", ex)
        End Try
    End Sub

    ''' <summary> Try close session. </summary>
    ''' <returns> <c>True</c> if session closed; otherwise <c>False</c>. </returns>
    Public Overridable Function TryCloseSession() As Boolean
        Try
            Me.CloseSession()
        Catch ex As OperationFailedException
            Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId, "Exception occurred closing session;. Details: {0}", ex)
            Return False
        End Try
        Return Not Me.IsDeviceOpen
    End Function

#End Region

#Region " SESSION: PROPERTY CHANGES AND MESSAGES EVENTS  "

    ''' <summary> Gets or sets the session messages trace enabled. </summary>
    ''' <value> The session messages trace enabled. </value>
    Public Property SessionMessagesTraceEnabled As Boolean
        Get
            Return Me.Session.SessionMessagesTraceEnabled
        End Get
        Set(value As Boolean)
            If Not value.Equals(Me.SessionMessagesTraceEnabled) Then
                Me.Session.SessionMessagesTraceEnabled = value
                Me.SafePostPropertyChanged()
            End If
        End Set
    End Property

    ''' <summary> Session property changed. </summary>
    ''' <remarks> David, 3/7/2016. </remarks>
    ''' <param name="sender">       Source of the event. </param>
    ''' <param name="propertyName"> Name of the property. </param>
    Private Sub SessionPropertyChanged(ByVal sender As SessionBase, ByVal propertyName As String)
        If sender IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(propertyName) Then
            Select Case propertyName
                Case NameOf(sender.ServiceRequestEventEnabled)
                    Me.SafePostPropertyChanged(NameOf(Me.SessionServiceRequestHandlerAdded))
                Case NameOf(sender.ServiceRequestEnableBitmask)
                    Me.SafePostPropertyChanged(NameOf(Me.ServiceRequestEnableBitmask))
                Case NameOf(sender.LastMessageReceived)
                    Dim value As String = sender.LastMessageReceived
                    If Not String.IsNullOrWhiteSpace(value) Then
                        Me.Talker?.Publish(TraceEventType.Information, My.MyLibrary.TraceEventId,
                                               "{0} sent: '{1}'.", Me.ResourceName, value.InsertCommonEscapeSequences)
                    End If
                Case NameOf(sender.LastMessageSent)
                    Dim value As String = sender.LastMessageSent
                    If Not String.IsNullOrWhiteSpace(value) Then
                        Me.Talker?.Publish(TraceEventType.Information, My.MyLibrary.TraceEventId,
                                               "{0} received: '{1}'.", Me.ResourceName, value)
                    End If
            End Select
        End If
    End Sub

    ''' <summary> Session property changed. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Property changed event information. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub SessionPropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs)
        Try
            If sender IsNot Nothing AndAlso e IsNot Nothing Then
                Me.SessionPropertyChanged(CType(sender, SessionBase), e.PropertyName)
            End If
        Catch ex As Exception
            If e Is Nothing Then
                Debug.Assert(Not Debugger.IsAttached, "Exception handling property", "Exception handling '{0}' property change. Details: {1}.",
                         "Null event arguments", ex.Message)
            ElseIf String.IsNullOrEmpty(e.PropertyName) Then
                Debug.Assert(Not Debugger.IsAttached, "Exception handling property", "Exception handling '{0}' property change. Details: {1}.",
                         "Empty", ex.Message)
            Else
                Debug.Assert(Not Debugger.IsAttached, "Exception handling property", "Exception handling '{0}' property change. Details: {1}.",
                         e.PropertyName, ex.Message)
            End If
        End Try
    End Sub

#End Region

#Region " SESSION: SERVICE REQUEST MANAGEMENT "

    Private _UsingSyncServiceRequestHandler As Boolean

    ''' <summary> Gets or sets the sentinel indicating how service request is handled: Synchronously or Asynchronously. </summary>
    ''' <value>  <c>True</c> if service request is handled Synchronously; <c>False</c> if Asynchronously. </value>
    Public Property UsingSyncServiceRequestHandler As Boolean
        Get
            Return Me._UsingSyncServiceRequestHandler
        End Get
        Set(value As Boolean)
            If value <> Me.UsingSyncServiceRequestHandler Then
                Me._UsingSyncServiceRequestHandler = value
                Me.AsyncNotifyPropertyChanged(NameOf(Me.UsingSyncServiceRequestHandler))
            End If
        End Set
    End Property

    ''' <summary> Gets the service request enable bitmask. </summary>
    ''' <value> The service request enable bitmask. </value>
    Public ReadOnly Property ServiceRequestEnableBitmask As ServiceRequests
        Get
            Return Me.Session.ServiceRequestEnableBitmask
        End Get
    End Property


    ''' <summary> Gets the session service request handler added. </summary>
    ''' <value> The session service request handler added. </value>
    Public ReadOnly Property SessionServiceRequestHandlerAdded As Boolean
        Get
            Return Me.IsDeviceOpen AndAlso Me.Session.ServiceRequestEventEnabled
        End Get
    End Property

    Dim _DeviceServiceRequestHandlerAdded As Boolean
    ''' <summary> Gets the device service request handler Added. </summary>
    ''' <value> The device service request handler registered. </value>
    Public Property DeviceServiceRequestHandlerAdded As Boolean
        Get
            Return Me._DeviceServiceRequestHandlerAdded
        End Get
        Protected Set(value As Boolean)
            Me._DeviceServiceRequestHandlerAdded = value
            Me.SafePostPropertyChanged()
        End Set
    End Property

    ''' <summary> Registers the device service request handler. </summary>
    ''' <remarks>
    ''' The Session service request handler must be registered and enabled before registering the
    ''' device event handler.
    ''' </remarks>
    Public Sub AddServiceRequestEventHandler()
        If Not Me.DeviceServiceRequestHandlerAdded Then
            AddHandler Me.Session.ServiceRequested, AddressOf Me.OnSessionBaseServiceRequested
            Me.DeviceServiceRequestHandlerAdded = True
        End If
    End Sub

    ''' <summary> Removes the device service request event handler. </summary>
    Public Sub RemoveServiceRequestEventHandler()
        If Me.DeviceServiceRequestHandlerAdded Then
            RemoveHandler Me.Session.ServiceRequested, AddressOf Me.OnSessionBaseServiceRequested
            Me.DeviceServiceRequestHandlerAdded = False
        End If
    End Sub

#End Region

#Region " SUBSYSTEMS "

    ''' <summary> Adds a subsystem. </summary>
    ''' <param name="value"> The value. </param>
    Public Sub AddSubsystem(ByVal value As SubsystemBase)
        Me.Subsystems.Add(value)
    End Sub

    Private _Subsystems As SubsystemCollection
    ''' <summary> Enumerates the Presettable subsystems. </summary>
    Protected ReadOnly Property Subsystems As SubsystemCollection
        Get
            Return Me._Subsystems
        End Get
    End Property

    ''' <summary> Handles the Status subsystem property changed event. </summary>
    ''' <param name="subsystem">    The subsystem. </param>
    ''' <param name="propertyName"> Name of the property. </param>
    Protected Overridable Sub OnPropertyChanged(ByVal subsystem As StatusSubsystemBase, ByVal propertyName As String)
        If subsystem Is Nothing OrElse String.IsNullOrWhiteSpace(propertyName) Then Return
        Select Case propertyName
            Case NameOf(subsystem.Identity)
                If Not String.IsNullOrWhiteSpace(subsystem.Identity) Then
                    Me.Talker?.Publish(TraceEventType.Information, My.MyLibrary.TraceEventId, "{0} identified;. as {1}", Me.ResourceName, subsystem.Identity)
                    If Not String.IsNullOrWhiteSpace(subsystem.VersionInfo?.Model) Then Me.ResourceTitle = subsystem.VersionInfo.Model
                End If
            Case NameOf(subsystem.DeviceErrors)
                If Not String.IsNullOrWhiteSpace(subsystem.DeviceErrors) Then
                    Me.Talker?.Publish(TraceEventType.Warning, My.MyLibrary.TraceEventId, "{0} errors;. {1}", Me.ResourceName, subsystem.DeviceErrors)
                End If
            Case NameOf(subsystem.LastDeviceError)
                If subsystem.LastDeviceError?.IsError Then
                    Me.Talker?.Publish(TraceEventType.Warning, My.MyLibrary.TraceEventId, "{0} last error;. {1}", Me.ResourceName,
                                       subsystem.LastDeviceError.CompoundErrorMessage)
                End If
        End Select
    End Sub

#End Region

#Region " EVENTS: SERVICE REQUESTED "

    Private _ServiceRequestFailureMessage As String

    ''' <summary> Gets or sets a message describing the service request failure. </summary>
    ''' <value> A message describing the service request failure. </value>
    Public Property ServiceRequestFailureMessage As String
        Get
            Return Me._ServiceRequestFailureMessage
        End Get
        Set(value As String)
            If String.IsNullOrEmpty(value) Then value = ""
            Me._ServiceRequestFailureMessage = value
            If Not String.IsNullOrWhiteSpace(Me.ServiceRequestFailureMessage) Then
                Me.Talker?.Publish(TraceEventType.Warning, My.MyLibrary.TraceEventId, Me.ServiceRequestFailureMessage)
            End If
        End Set
    End Property

    ''' <summary> Reads the event registers after receiving a service request. </summary>
    Protected MustOverride Sub ProcessServiceRequest()

    ''' <summary> Reads the event registers after receiving a service request. </summary>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Protected Function TryProcessServiceRequest() As Boolean
        Dim result As Boolean = True
        Try
            Me.ProcessServiceRequest()
        Catch ex As Exception
            Me.ServiceRequestFailureMessage = Me.Talker?.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId,
                                                                 $"Exception occurred processing service request;. Details: {ex}")
            result = False
        End Try
        Return result
    End Function

    ''' <summary> Occurs when service is requested. </summary>
    Public Event ServiceRequested As EventHandler(Of EventArgs)

    ''' <summary> Removes event handler. </summary>
    ''' <remarks> David, 12/17/2015. </remarks>
    ''' <param name="value"> The handler. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub RemoveEventHandler(ByVal value As EventHandler(Of EventArgs))
        For Each d As [Delegate] In value.SafeInvocationList
            Try
                RemoveHandler Me.ServiceRequested, CType(d, EventHandler(Of EventArgs))
            Catch ex As Exception
                Debug.Assert(Not Debugger.IsAttached, ex.ToString)
            End Try
        Next
    End Sub

#Region " INVOKE "

    ''' <summary> Synchronously Invokes the <see cref="ServiceRequested">Service Requested Event</see>. Must
    ''' be called with the <see cref="SynchronizationContext">sync context</see> </summary>
    ''' <param name="e"> The <see cref="EventArgs" /> instance containing the event data. </param>
    Private Sub InvokeServiceRequested(ByVal e As EventArgs)
        Dim evt As EventHandler(Of EventArgs) = Me.ServiceRequestedEvent
        evt?.Invoke(Me, e)
    End Sub

    ''' <summary> Synchronously Invokes the <see cref="ServiceRequested">Service Requested Event</see>. Must
    ''' be called with the <see cref="SynchronizationContext">sync context</see> </summary>
    ''' <param name="obj"> The object. </param>
    Private Sub InvokeServiceRequested(ByVal obj As Object)
        Me.InvokeServiceRequested(CType(obj, EventArgs))
    End Sub

#End Region

#Region " EVENT HANDLERS "

    ''' <summary> Gets or sets a context for the captured synchronization. </summary>
    ''' <value> The captured synchronization context. </value>
    Public Property CapturedSyncContext As Threading.SynchronizationContext

    ''' <summary> Synchronously handles the Service Request event of the <see cref="Session">session</see> control. </summary>
    ''' <param name="sender"> The source of the event. </param>
    ''' <param name="e">      The <see cref="EventArgs" /> instance
    ''' containing the event data. </param>
    Private Sub OnSessionBaseServiceRequested(ByVal sender As Object, ByVal e As EventArgs)
        If SynchronizationContext.Current Is Nothing Then
            ' captured sync context may not be required if not raising events to the sync context.
            Threading.SynchronizationContext.SetSynchronizationContext(Me.CapturedSyncContext)
        End If
        Me.TryProcessServiceRequest()
        If Me.UsingSyncServiceRequestHandler Then
            If Me.MultipleSyncContextsExpected OrElse SynchronizationContext.Current Is Nothing Then
                ' Even though the current sync context is nothing, one of the targets might 
                ' still require invocation. Therefore, save invoke is implemented.
                Me.ServiceRequestedEvent.SafeInvoke(Me, e)
            Else
                SynchronizationContext.Current.Send(New SendOrPostCallback(AddressOf InvokeServiceRequested), e)
            End If
        Else
            If Me.MultipleSyncContextsExpected OrElse SynchronizationContext.Current Is Nothing Then
                ' Even though the current sync context is nothing, one of the targets might 
                ' still require invocation. Therefore, save invoke is implemented.
                Me.ServiceRequestedEvent.SafeBeginInvoke(Me, e)
            Else
                SynchronizationContext.Current.Post(New SendOrPostCallback(AddressOf InvokeServiceRequested), e)
            End If
        End If
    End Sub

#End Region

#End Region

#Region " DEVICE OPEN, INITIALIZE AND CLOSE EVENTS "

#Region " OPENING "

    ''' <summary> Occurs when Opening. </summary>
    Public Event Opening As EventHandler(Of ComponentModel.CancelEventArgs)

    ''' <summary> Removes event handler. </summary>
    ''' <remarks> David, 12/17/2015. </remarks>
    ''' <param name="value"> The handler. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub RemoveOpeningEventHandler(ByVal value As EventHandler(Of ComponentModel.CancelEventArgs))
        For Each d As [Delegate] In value.SafeInvocationList
            Try
                RemoveHandler Me.Opening, CType(d, EventHandler(Of ComponentModel.CancelEventArgs))
            Catch ex As Exception
                Debug.Assert(Not Debugger.IsAttached, ex.ToString)
            End Try
        Next
    End Sub


    ''' <summary> Safely and synchronously <see cref="SynchronizationContext.Send">sends</see> or
    ''' invokes the <see cref="Opening">Opening Event</see>. </summary>
    ''' <param name="e"> The <see cref="System.EventArgs" /> instance containing the event data. </param>
    Private Sub SyncNotifyOpening(ByVal e As ComponentModel.CancelEventArgs)
        If Me.MultipleSyncContextsExpected OrElse SynchronizationContext.Current Is Nothing Then
            ' Even though the current sync context is nothing, one of the targets might 
            ' still require invocation. Therefore, save invoke is implemented.
            Me.OpeningEvent.SafeInvoke(Me, e)
        Else
            SynchronizationContext.Current.Send(New SendOrPostCallback(AddressOf Me.InvokeOpening), e)
        End If
    End Sub

#Region " INVOKE "

    ''' <summary> Synchronously Invokes the <see cref="Opening">Opening Event</see>. Must be
    ''' called with the <see cref="SynchronizationContext">sync context</see> </summary>
    ''' <param name="e"> The <see cref="System.EventArgs" /> instance containing
    ''' the event data. </param>
    Private Sub InvokeOpening(ByVal e As ComponentModel.CancelEventArgs)
        Me.OpeningEvent.UnsafeInvoke(Me, e)
    End Sub

    ''' <summary> Synchronously Invokes the <see cref="Opening">Opening Event</see>. Must be
    ''' called with the <see cref="SynchronizationContext">sync context</see> </summary>
    ''' <param name="obj"> The object. </param>
    Private Sub InvokeOpening(ByVal obj As Object)
        Me.InvokeOpening(CType(obj, ComponentModel.CancelEventArgs))
    End Sub

#End Region

#End Region

#Region " OPENED "

    ''' <summary> Occurs when Opened. </summary>
    Public Event Opened As EventHandler(Of System.EventArgs)

    ''' <summary> Removes event handler. </summary>
    ''' <remarks> David, 12/17/2015. </remarks>
    ''' <param name="value"> The handler. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub RemoveOpenedEventHandler(ByVal value As EventHandler(Of EventArgs))
        For Each d As [Delegate] In value.SafeInvocationList
            Try
                RemoveHandler Me.Opened, CType(d, EventHandler(Of EventArgs))
            Catch ex As Exception
                Debug.Assert(Not Debugger.IsAttached, ex.ToString)
            End Try
        Next
    End Sub

    ''' <summary> Safely and synchronously <see cref="SynchronizationContext.Send">sends</see> or
    ''' invokes the <see cref="Opened">Opened Event</see>. </summary>
    ''' <param name="e"> The <see cref="System.EventArgs" /> instance containing the event data. </param>
    Protected Overridable Sub SyncNotifyOpened(ByVal e As System.EventArgs)
        If Me.MultipleSyncContextsExpected OrElse SynchronizationContext.Current Is Nothing Then
            ' Even though the current sync context is nothing, one of the targets might 
            ' still require invocation. Therefore, save invoke is implemented.
            Me.OpenedEvent.SafeInvoke(Me)
        Else
            SynchronizationContext.Current.Send(New SendOrPostCallback(AddressOf Me.InvokeOpened), e)
        End If
    End Sub

#Region " INVOKE "

    ''' <summary> Synchronously Invokes the <see cref="Opened">Opened Event</see>. Must be
    ''' called with the <see cref="SynchronizationContext">sync context</see> </summary>
    ''' <param name="e"> The <see cref="System.EventArgs" /> instance containing
    ''' the event data. </param>
    Private Sub InvokeOpened(ByVal e As System.EventArgs)
        Me.OpenedEvent.UnsafeInvoke(Me, e)
    End Sub

    ''' <summary> Synchronously Invokes the <see cref="Opened">Opened Event</see>. Must be
    ''' called with the <see cref="SynchronizationContext">sync context</see> </summary>
    ''' <param name="obj"> The object. </param>
    Private Sub InvokeOpened(ByVal obj As Object)
        Me.InvokeOpened(CType(obj, System.EventArgs))
    End Sub

#End Region

#End Region

#Region " CLOSING "

    ''' <summary> Occurs when Closing. </summary>
    Public Event Closing As EventHandler(Of System.ComponentModel.CancelEventArgs)

    ''' <summary> Removes event handler. </summary>
    ''' <remarks> David, 12/17/2015. </remarks>
    ''' <param name="value"> The handler. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub RemoveClosingEventHandler(ByVal value As EventHandler(Of System.ComponentModel.CancelEventArgs))
        For Each d As [Delegate] In value.SafeInvocationList
            Try
                RemoveHandler Me.Closing, CType(d, EventHandler(Of System.ComponentModel.CancelEventArgs))
            Catch ex As Exception
                Debug.Assert(Not Debugger.IsAttached, ex.ToString)
            End Try
        Next
    End Sub

    ''' <summary> Safely and synchronously <see cref="SynchronizationContext.Send">sends</see> or
    ''' invokes the <see cref="Closing">Closing Event</see>. </summary>
    ''' <param name="e"> The <see cref="System.EventArgs" /> instance containing the event data. </param>
    Private Sub SyncNotifyClosing(ByVal e As System.ComponentModel.CancelEventArgs)
        If Me.MultipleSyncContextsExpected OrElse SynchronizationContext.Current Is Nothing Then
            ' Even though the current sync context is nothing, one of the targets might 
            ' still require invocation. Therefore, save invoke is implemented.
            Me.ClosingEvent.SafeInvoke(Me, e)
        Else
            SynchronizationContext.Current.Send(New SendOrPostCallback(AddressOf Me.InvokeClosing), e)
        End If
    End Sub

#Region " INVOKE "

    ''' <summary> Synchronously Invokes the <see cref="Closing">Closing Event</see>. Must be
    ''' called with the <see cref="SynchronizationContext">sync context</see> </summary>
    ''' <param name="e"> The <see cref="System.EventArgs" /> instance containing
    ''' the event data. </param>
    Private Sub InvokeClosing(ByVal e As System.ComponentModel.CancelEventArgs)
        Me.ClosingEvent.UnsafeInvoke(Me, e)
    End Sub

    ''' <summary> Synchronously Invokes the <see cref="Closing">Closing Event</see>. Must be
    ''' called with the <see cref="SynchronizationContext">sync context</see> </summary>
    ''' <param name="obj"> The object. </param>
    Private Sub InvokeClosing(ByVal obj As Object)
        Me.InvokeClosing(CType(obj, System.ComponentModel.CancelEventArgs))
    End Sub

#End Region

#End Region

#Region " CLOSED "

    ''' <summary> Occurs when Closed. </summary>
    Public Event Closed As EventHandler(Of System.EventArgs)

    ''' <summary> Removes event handler. </summary>
    ''' <remarks> David, 12/17/2015. </remarks>
    ''' <param name="value"> The handler. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub RemoveClosedEventHandler(ByVal value As EventHandler(Of EventArgs))
        For Each d As [Delegate] In value.SafeInvocationList
            Try
                RemoveHandler Me.Closed, CType(d, EventHandler(Of EventArgs))
            Catch ex As Exception
                Debug.Assert(Not Debugger.IsAttached, ex.ToString)
            End Try
        Next
    End Sub

    ''' <summary> Safely and synchronously <see cref="SynchronizationContext.Send">sends</see> or
    ''' invokes the <see cref="Closed">Closed Event</see>. </summary>
    ''' <param name="e"> The <see cref="System.EventArgs" /> instance containing the event data. </param>
    Private Sub SyncNotifyClosed(ByVal e As System.EventArgs)
        If Me.MultipleSyncContextsExpected OrElse SynchronizationContext.Current Is Nothing Then
            ' Even though the current sync context is nothing, one of the targets might 
            ' still require invocation. Therefore, save invoke is implemented.
            Me.ClosedEvent.SafeInvoke(Me, e)
        Else
            SynchronizationContext.Current.Send(New SendOrPostCallback(AddressOf Me.InvokeClosed), e)
        End If
    End Sub

#Region " INVOKE "

    ''' <summary> Synchronously Invokes the <see cref="Closed">Closed Event</see>. Must be
    ''' called with the <see cref="SynchronizationContext">sync context</see> </summary>
    ''' <param name="e"> The <see cref="System.EventArgs" /> instance containing
    ''' the event data. </param>
    Private Sub InvokeClosed(ByVal e As System.EventArgs)
        Me.ClosedEvent.UnsafeInvoke(Me, e)
    End Sub

    ''' <summary> Synchronously Invokes the <see cref="Closed">Closed Event</see>. Must be
    ''' called with the <see cref="SynchronizationContext">sync context</see> </summary>
    ''' <param name="obj"> The object. </param>
    Private Sub InvokeClosed(ByVal obj As Object)
        Me.InvokeClosed(CType(obj, System.EventArgs))
    End Sub

#End Region

#End Region

#Region " INITIALIZING "

    ''' <summary> Occurs when Initializing. </summary>
    Public Event Initializing As EventHandler(Of System.ComponentModel.CancelEventArgs)

    ''' <summary> Removes event handler. </summary>
    ''' <remarks> David, 12/17/2015. </remarks>
    ''' <param name="value"> The handler. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub RemoveInitializingEventHandler(ByVal value As EventHandler(Of System.ComponentModel.CancelEventArgs))
        For Each d As [Delegate] In value.SafeInvocationList
            Try
                RemoveHandler Me.Initializing, CType(d, EventHandler(Of System.ComponentModel.CancelEventArgs))
            Catch ex As Exception
                Debug.Assert(Not Debugger.IsAttached, ex.ToString)
            End Try
        Next
    End Sub

    ''' <summary> Safely and synchronously <see cref="SynchronizationContext.Send">sends</see> or
    ''' invokes the <see cref="Initializing">Initializing Event</see>. </summary>
    ''' <param name="e"> The <see cref="System.EventArgs" /> instance containing the event data. </param>
    Private Sub SyncNotifyInitializing(ByVal e As System.ComponentModel.CancelEventArgs)
        If Me.MultipleSyncContextsExpected OrElse SynchronizationContext.Current Is Nothing Then
            ' Even though the current sync context is nothing, one of the targets might 
            ' still require invocation. Therefore, save invoke is implemented.
            Me.InitializingEvent.SafeInvoke(Me, e)
        Else
            SynchronizationContext.Current.Send(New SendOrPostCallback(AddressOf Me.InvokeInitializing), e)
        End If
    End Sub

#Region " INVOKE "

    ''' <summary> Synchronously Invokes the <see cref="Initializing">Initializing Event</see>. Must be
    ''' called with the <see cref="SynchronizationContext">sync context</see> </summary>
    ''' <param name="e"> The <see cref="System.EventArgs" /> instance containing
    ''' the event data. </param>
    Private Sub InvokeInitializing(ByVal e As System.ComponentModel.CancelEventArgs)
        Me.InitializingEvent.UnsafeInvoke(Me, e)
    End Sub

    ''' <summary> Synchronously Invokes the <see cref="Initializing">Initializing Event</see>. Must be
    ''' called with the <see cref="SynchronizationContext">sync context</see> </summary>
    ''' <param name="obj"> The object. </param>
    Private Sub InvokeInitializing(ByVal obj As Object)
        Me.InvokeInitializing(CType(obj, System.ComponentModel.CancelEventArgs))
    End Sub

#End Region

#End Region

#Region " INITIALIZED "

    ''' <summary> Occurs when Initialized. </summary>
    Public Event Initialized As EventHandler(Of System.EventArgs)

    ''' <summary> Removes event handler. </summary>
    ''' <remarks> David, 12/17/2015. </remarks>
    ''' <param name="value"> The handler. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub RemoveInitializedEventHandler(ByVal value As EventHandler(Of EventArgs))
        For Each d As [Delegate] In value.SafeInvocationList
            Try
                RemoveHandler Me.Initialized, CType(d, EventHandler(Of EventArgs))
            Catch ex As Exception
                Debug.Assert(Not Debugger.IsAttached, ex.ToString)
            End Try
        Next
    End Sub

    ''' <summary> Safely and synchronously <see cref="SynchronizationContext.Send">sends</see> or
    ''' invokes the <see cref="Initialized">Initialized Event</see>. </summary>
    ''' <param name="e"> The <see cref="System.EventArgs" /> instance containing the event data. </param>
    Private Sub SyncNotifyInitialized(ByVal e As System.EventArgs)
        If Me.MultipleSyncContextsExpected OrElse SynchronizationContext.Current Is Nothing Then
            ' Even though the current sync context is nothing, one of the targets might 
            ' still require invocation. Therefore, save invoke is implemented.
            Me.InitializedEvent.SafeInvoke(Me)
        Else
            SynchronizationContext.Current.Send(New SendOrPostCallback(AddressOf Me.InvokeInitialized), e)
        End If
    End Sub

#Region " INVOKE "

    ''' <summary> Synchronously Invokes the <see cref="Initialized">Initialized Event</see>. Must be
    ''' called with the <see cref="SynchronizationContext">sync context</see> </summary>
    ''' <param name="e"> The <see cref="System.EventArgs" /> instance containing
    ''' the event data. </param>
    Private Sub InvokeInitialized(ByVal e As System.EventArgs)
        Me.InitializedEvent.UnsafeInvoke(Me, e)
    End Sub

    ''' <summary> Synchronously Invokes the <see cref="Initialized">Initialized Event</see>. Must be
    ''' called with the <see cref="SynchronizationContext">sync context</see> </summary>
    ''' <param name="obj"> The object. </param>
    Private Sub InvokeInitialized(ByVal obj As Object)
        Me.InvokeInitialized(CType(obj, System.EventArgs))
    End Sub

#End Region

#End Region

#End Region

#Region " TALKER "

    ''' <summary> Gets the trace message talker. </summary>
    ''' <value> The trace message talker. </value>
    Public ReadOnly Property Talker As ITraceMessageTalker

    ''' <summary> Adds the listeners such as the top level trace messages box and log. </summary>
    ''' <remarks> David, 12/29/2015. </remarks>
    ''' <param name="listeners"> The listeners. </param>
    Public Overridable Sub AddListeners(ByVal listeners As IEnumerable(Of ITraceMessageListener)) Implements ITalker.AddListeners
        Me.Talker.Listeners.Add(listeners)
    End Sub

    ''' <summary> Clears the listeners. </summary>
    ''' <remarks> David, 12/30/2015. </remarks>
    Public Sub ClearListeners() Implements ITalker.ClearListeners
        Me.Talker.Listeners?.Clear()
        Me.Subsystems.ClearListeners()
    End Sub

    ''' <summary> Adds the listeners such as the top level trace messages box and log. </summary>
    ''' <remarks> David, 12/14/2016. </remarks>
    ''' <param name="talker"> The talker. </param>
    Public Sub AddListeners(talker As ITraceMessageTalker) Implements ITalker.AddListeners
        Me.Talker.AddListeners(talker)
    End Sub

    ''' <summary> Updates the trace log level described by traceLevel. </summary>
    ''' <remarks> David, 12/14/2016. </remarks>
    ''' <param name="traceLevel"> The trace level. </param>
    Public Overridable Sub UpdateTraceLogLevel(ByVal traceLevel As TraceEventType) Implements ITalker.UpdateTraceLogLevel
        Me.Talker.UpdateTraceLogLevel(traceLevel)
        Me.Subsystems.UpdateTraceLogLevel(traceLevel)
    End Sub

    ''' <summary> Updates the trace show level described by traceLevel. </summary>
    ''' <remarks> David, 12/14/2016. </remarks>
    ''' <param name="traceLevel"> The trace level. </param>
    Public Overridable Sub UpdateTraceShowLevel(ByVal traceLevel As TraceEventType) Implements ITalker.UpdateTraceShowLevel
        Me.Talker.UpdateTraceShowLevel(traceLevel)
        Me.Subsystems.UpdateTraceShowLevel(traceLevel)
    End Sub

    ''' <summary> Adds subsystem listeners. </summary>
    ''' <remarks> David, 12/30/2015. </remarks>
    Public Overridable Sub AddSubsystemListeners()
        Me.Subsystems.AddListeners(Me.Talker.Listeners)
    End Sub

    ''' <summary> Clears the subsystem listeners. </summary>
    ''' <remarks> David, 12/30/2015. </remarks>
    Public Overridable Sub ClearSubsystemListeners()
        Me.Subsystems.ClearListeners()
    End Sub

#End Region

End Class

#Region " UNUSED "
' using event handler extensions.
#If False Then
#Region " SAFE EVENTS "

    ''' <summary> Synchronously notifies (Invokes a <see cref="ISynchronizeInvoke">sync enabled
    ''' entity</see>) or (Dynamically Invokes) an
    ''' <see cref="EventHandler(Of EventArgs)">Event</see>. </summary>
    ''' <param name="handler"> The event handler. </param>
    Private Overloads Sub SafeInvoke(ByVal handler As EventHandler(Of EventArgs), ByVal e As EventArgs)
        For Each d As [Delegate] In handler.SafeInvocationList
            If d.Target Is Nothing Then
                d.DynamicInvoke(New Object() {Me, e})
            Else
                Dim target As ISynchronizeInvoke = TryCast(d . Target, ISynchronizeInvoke)
                If target Is Nothing Then
                    d.DynamicInvoke(New Object() {Me, e})
                Else
                    target.Invoke(d, New Object() {Me, e})
                End If
            End If
        Next
    End Sub

    ''' <summary> Asynchronously (Begins Invoke a <see cref="ISynchronizeInvoke">sync enabled
    ''' entity</see>) or synchronously (Dynamically Invokes) notifies an 
    ''' <see cref="EventHandler(Of EventArgs)">Event</see>. </summary>
    ''' <param name="handler"> The event handler. </param>
    Private Overloads Sub SafeBeginInvoke(ByVal handler As EventHandler(Of EventArgs), ByVal e As EventArgs)
        For Each d As [Delegate] In handler.SafeInvocationList
            If d.Target Is Nothing Then
                d.DynamicInvoke(New Object() {Me, e})
            Else
                Dim target As ISynchronizeInvoke = TryCast(d . Target, ISynchronizeInvoke)
                If target Is Nothing Then
                    d.DynamicInvoke(New Object() {Me, e})
                Else
                    target.BeginInvoke(d, New Object() {Me, e})
                End If
            End If
        Next
    End Sub

#End Region

#End If
#End Region

#Region " OLD OPEN SESSION THAT PREVENTED EXCEPTIONS "
#If False Then
    ''' <summary> Opens the session. </summary>
    ''' <remarks> Register the device trace notifier before opening the session. </remarks>
    ''' <exception cref="OperationFailedException"> Thrown when operation failed to execute. </exception>
    ''' <param name="resourceName"> Name of the resource. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Public Overridable Sub OpenSession(ByVal resourceName As String, ByVal resourceTitle As String)
        Try
            If Me.Enabled Then Me.Talker?.Publish(TraceEventType.Verbose, My.MyLibrary.TraceEventId, $"Opening session to {resourceName};. ")
            Me.Session.ResourceTitle = resourceTitle
            Me.Session.OpenSession(resourceName, resourceTitle)
            If Me.Session.IsSessionOpen Then
                Me.Talker?.Publish(TraceEventType.Verbose, My.MyLibrary.TraceEventId, $"Session open to {resourceName};. ")
            ElseIf Me.Session.Enabled Then
                ' if session did not open and the session is enabled, then we have an error, which could be stressed.
                Me.Talker?.Publish(TraceEventType.Warning, My.MyLibrary.TraceEventId, $"Unable to open session to {resourceName};. ")
            ElseIf Not Me.IsDeviceOpen Then
                Me.Talker?.Publish(TraceEventType.Warning, My.MyLibrary.TraceEventId, $"Unable to emulate {resourceName};. ")
            End If
            If Me.Session.IsSessionOpen OrElse (Me.IsDeviceOpen AndAlso Not Me.Session.Enabled) Then
                Dim e As New ComponentModel.CancelEventArgs
                Me.OnOpening(e)
                If e.Cancel Then
                    Me.Talker?.Publish(TraceEventType.Information, My.MyLibrary.TraceEventId, $"Opening {resourceTitle}:{resourceName} canceled;. ")
                    Me.TryCloseSession()
                Else
                    Me.OnOpened()
                    Try
                        If Me.IsDeviceOpen Then
                            Me.OnInitializing(e)
                        Else
                            Me.Talker?.Publish(TraceEventType.Information, My.MyLibrary.TraceEventId, $"Opening {resourceTitle}:{resourceName} failed;. ")
                        End If
                    Catch ex As Exception
                        Me.Talker.Publish(TraceEventType.Warning, My.MyLibrary.TraceEventId, $"{resourceName} initializing failed;. Details: {0}", ex)
                    End Try
                    Try
                        If Not e.Cancel AndAlso Me.IsDeviceOpen Then Me.OnInitialized()
                    Catch ex As Exception
                        Me.Talker.Publish(TraceEventType.Warning, My.MyLibrary.TraceEventId, $"{resourceName} initialize actions failed;. Details: {0}", ex)
                    End Try
                End If
            Else
                ' if failed to open, close
                Me.CloseSession()
            End If
        Catch ex As OperationFailedException
            Throw
        Catch ex As NativeException
            Me.Talker.Publish(TraceEventType.Warning, My.MyLibrary.TraceEventId, $"{resourceName} failed connecting. Disconnecting.")
            Me.TryCloseSession()
            Throw New OperationFailedException($"VISA exception occurred opening session to '{resourceName}'.", ex)
        Catch ex As Exception
            Me.Talker.Publish(TraceEventType.Warning, My.MyLibrary.TraceEventId, $"{resourceName} failed connecting. Disconnecting.")
            Me.TryCloseSession()
            Throw New OperationFailedException($"Exception occurred opening session to '{resourceName}'.", ex)
        End Try
    End Sub
#End If
#End Region