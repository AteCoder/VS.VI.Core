Imports isr.VI.ExceptionExtensions
''' <summary> Defines a Status Subsystem for a TSP System. </summary>
''' <license> (c) 2013 Integrated Scientific Resources, Inc.<para>
''' Licensed under The MIT License. </para><para>
''' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
''' BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
''' NON-INFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
''' DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
''' OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
''' </para> </license>
''' <history date="10/7/2013" by="David" revision=""> Created. </history>
Public MustInherit Class StatusSubsystemBase
    Inherits VI.Scpi.StatusSubsystemBase

#Region " CONSTRUCTION + CLEANUP "

    ''' <summary> Initializes a new instance of the <see cref="StatusSubsystemBase" /> class. </summary>
    ''' <param name="session"> A reference to a <see cref="Session">message based TSP session</see>. </param>
    Protected Sub New(ByVal session As VI.Pith.SessionBase)
        MyBase.New(session, TspSyntax.EventLog.NoErrorCompoundMessage)

        Me._VersionInfo = New VersionInfo

        ' check for query and other errors reported by the standard event register
        Me.StandardDeviceErrorAvailableBits = VI.Pith.StandardEvents.CommandError Or VI.Pith.StandardEvents.DeviceDependentError Or
                                              VI.Pith.StandardEvents.ExecutionError Or VI.Pith.StandardEvents.QueryError
    End Sub

    ''' <summary>
    ''' Releases the unmanaged resources used by the <see cref="T:System.Windows.Forms.Control" />
    ''' and its child controls and optionally releases the managed resources.
    ''' </summary>
    ''' <param name="disposing"> true to release both managed and unmanaged resources; false to
    '''                          release only unmanaged resources. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If Not Me.IsDisposed AndAlso disposing Then
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

#End Region

#Region " I PRESETTABLE "

    ''' <summary> Clears the active state. Issues selective device clear. </summary>
    Public Overrides Sub ClearActiveState()
        ' A delay is required before issuing a device clear.
        ' First a 1ms delay was added. Without the delay, the instrument had error -286 TSP Runtime Error and User Abort error 
        ' if the clear command was issued after turning off the status _G.status.request_enable=0
        ' Thereafter, the instrument has a resource not found error when trying to connect after failing to connect 
        ' because instruments were off. Stopping here in debug mode seems to have alleviated this issue.  So,
        ' the delay was increased to 10 ms.
        Threading.Thread.Sleep(10)
        MyBase.ClearActiveState()
        Me.QueryOperationCompleted()
    End Sub

    ''' <summary> Sets the subsystem to its initial post reset state. </summary>
    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Public Overrides Sub InitKnownState()

        Me.SerialNumber = New Long?
        Me.SerialNumberReading = ""

        MyBase.InitKnownState()

        Try
            ' flush the input buffer in case the instrument has some leftovers.
            Me.Session.DiscardUnreadData()
            If Not String.IsNullOrWhiteSpace(Me.Session.DiscardedData) Then
                Me.Talker.Publish(TraceEventType.Information, My.MyLibrary.TraceEventId,
                                   "Data discarded after turning prompts and errors off;. Data: {0}.", Me.Session.DiscardedData)
            End If
        Catch ex As VI.Pith.NativeException
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId,
                               "Exception ignored clearing read buffer;. {0}", ex.ToFullBlownString)
        End Try

        Try
            ' flush write may cause the instrument to send off a new data.
            Me.Session.DiscardUnreadData()
            If Not String.IsNullOrWhiteSpace(Me.Session.DiscardedData) Then
                Me.Talker.Publish(TraceEventType.Information, My.MyLibrary.TraceEventId,
                                   "Unread data discarded after discarding unset data;. Data: {0}.", Me.Session.DiscardedData)
            End If
        Catch ex As VI.Pith.NativeException
            Me.Talker.Publish(TraceEventType.Error, My.MyLibrary.TraceEventId,
                               "Exception ignored clearing read buffer;. {0}", ex.ToFullBlownString)
        End Try

        ' enable service request on all events
        ' this is part of INIT already. Me.EnableServiceRequest(StandardEvents.All, VI.Pith.ServiceRequests.All)
        Me.OperationCompleted = Me.QueryOperationCompleted

    End Sub

#End Region

#Region " COMMAND SYNTAX "

#Region " EXECUTION STATE "

    ''' <summary> Gets the preset command. </summary>
    ''' <value> The preset command. </value>
    ''' <remarks> No preset command for the TSP system. </remarks>
    Protected Overrides ReadOnly Property PresetCommand As String = ""

    ''' <summary> Gets the clear execution state command. </summary>
    ''' <value> The clear execution state command. </value>
    Protected Overrides ReadOnly Property ClearExecutionStateCommand As String = TspSyntax.Status.ClearExecutionStateCommand

    ''' <summary> Gets the reset known state command. </summary>
    ''' <value> The reset known state command. </value>
    ''' <remarks> Uses reset() to reset all devices on the TSP link. </remarks>
    Protected Overrides ReadOnly Property ResetKnownStateCommand As String = LuaSyntax.ResetKnownStateCommand

#End Region

#Region " LANGUAGE "

    ''' <summary> Gets or sets the Language query command. </summary>
    ''' <value> The Language query command. </value>
    Protected Overrides ReadOnly Property LanguageQueryCommand As String = VI.Pith.Ieee488.Syntax.LanguageQueryCommand

    ''' <summary> Gets or sets the Language command format. </summary>
    ''' <value> The Language command format. </value>
    Protected Overrides ReadOnly Property LanguageCommandFormat As String = VI.Pith.Ieee488.Syntax.LanguageCommandFormat

#End Region

#Region " SERVICE REQUEST "

    ''' <summary> Gets the bits that would be set for detecting if an error is available. </summary>
    ''' <value> The error available bits. </value>
    Public Overrides ReadOnly Property ErrorAvailableBits As VI.Pith.ServiceRequests = VI.Pith.ServiceRequests.ErrorAvailable

    ''' <summary> Gets the bits that would be set for detecting if an Measurement is available. </summary>
    ''' <value> The Measurement available bits. </value>
    Public Overrides ReadOnly Property MeasurementAvailableBits As VI.Pith.ServiceRequests = VI.Pith.ServiceRequests.MeasurementEvent

    ''' <summary> Gets the bits that would be set for detecting if an Message is available. </summary>
    ''' <value> The Message available bits. </value>
    Public Overrides ReadOnly Property MessageAvailableBits As VI.Pith.ServiceRequests = VI.Pith.ServiceRequests.MessageAvailable

    ''' <summary> Gets the bits that would be set for detecting if a Standard Event is available. </summary>
    ''' <value> The Standard Event available bits. </value>
    Public Overrides ReadOnly Property StandardEventAvailableBits As VI.Pith.ServiceRequests = VI.Pith.ServiceRequests.StandardEvent

    ''' <summary> Gets or sets the operation completed query command. </summary>
    ''' <value> The operation completed query command. </value>
    Protected Overrides ReadOnly Property OperationCompletedQueryCommand As String = LuaSyntax.OperationCompletedQueryCommand
    ' Protected Overrides ReadOnly Property OperationCompletedQueryCommand As String = Vi.Pith.Ieee488.Syntax.OperationCompletedQueryCommand

    ''' <summary> Gets the standard service enable command format. </summary>
    ''' <value> The standard service enable command format. </value>
    Protected Overrides ReadOnly Property StandardServiceEnableCommandFormat As String = TspSyntax.Status.StandardServiceEnableCommandFormat

    ''' <summary> Gets or sets the standard service enable and complete command format. </summary>
    ''' <value> The standard service enable command and complete format. </value>
    Protected Overrides ReadOnly Property StandardServiceEnableCompleteCommandFormat As String = TspSyntax.Status.StandardServiceEnableCompleteCommandFormat

    ''' <summary> Gets the service request enable command format. </summary>
    ''' <value> The service request enable command format. </value>
    Protected Overrides ReadOnly Property ServiceRequestEnableCommandFormat As String = TspSyntax.Status.ServiceRequestEnableCommandFormat

#End Region

#Region " STANDARD EVENT "

    ''' <summary> Gets the standard event status query command. </summary>
    ''' <value> The standard event status query command. </value>
    Protected Overrides ReadOnly Property StandardEventStatusQueryCommand As String = TspSyntax.Status.StandardEventPrintCommand

    ''' <summary> Gets the standard event enable query command. </summary>
    ''' <value> The standard event enable query command. </value>
    Protected Overrides ReadOnly Property StandardEventEnableQueryCommand As String = TspSyntax.Status.StandardEventEnablePrintCommand

#End Region

#Region " MEASUREMENT EVENTS "

    ''' <summary> Gets the measurement status query command. </summary>
    ''' <value> The measurement status query command. </value>
    Protected Overrides ReadOnly Property MeasurementStatusQueryCommand As String = "" ' Not available with TSP2

    ''' <summary> Gets the measurement event condition query command. </summary>
    ''' <value> The measurement event condition query command. </value>
    Protected Overrides ReadOnly Property MeasurementEventConditionQueryCommand As String = "" ' Not available with TSP2

#End Region

#Region " OPERATION REGISTER EVENTS "

    ''' <summary> Gets the operation event enable Query command </summary>
    ''' <value> The operation event enable Query command. </value>
    Protected Overrides ReadOnly Property OperationEventEnableQueryCommand As String = TspSyntax.Status.OperationEventEnablePrintCommand

    ''' <summary> Gets the operation event enable command format. </summary>
    ''' <value> The operation event enable command format. </value>
    Protected Overrides ReadOnly Property OperationEventEnableCommandFormat As String = TspSyntax.Status.OperationEventEnableCommandFormat

    ''' <summary> Gets the operation event status query command. </summary>
    ''' <value> The operation event status query command. </value>
    Protected Overrides ReadOnly Property OperationEventStatusQueryCommand As String = TspSyntax.Status.OperationEventPrintCommand

    ''' <summary> Programs the Operation register event enable bit mask. </summary>
    ''' <param name="value"> The bitmask. </param>
    ''' <returns> The mask to use for enabling the events; nothing if unknown. </returns>
    Public Overrides Function WriteOperationEventEnableBitmask(ByVal value As Integer) As Integer?
        If (value And OperationEventBits.UserRegister) <> 0 Then
            ' if enabling the user register, enable all events on the user register. 
            value = &H4FFF
        End If
        Return Me.WriteOperationEventEnableBitmask(value)
    End Function

#End Region

#Region " QUESTIONABLE REGISTER "

    ''' <summary> Gets the questionable status query command. </summary>
    ''' <value> The questionable status query command. </value>
    Protected Overrides ReadOnly Property QuestionableStatusQueryCommand As String = VI.Pith.Scpi.Syntax.QuestionableEventQueryCommand

#End Region

#Region " LINE FREQUENCY "

    ''' <summary> Gets line frequency query command. </summary>
    ''' <value> The line frequency query command. </value>
    Protected Overrides ReadOnly Property LineFrequencyQueryCommand As String = TspSyntax.LocalNode.LineFrequencyPrintCommand

#End Region

#End Region

#Region " IDENTITY "

    ''' <summary> Gets the identity query command. </summary>
    ''' <value> The identity query command. </value>
    Protected Overrides ReadOnly Property IdentityQueryCommand As String = TspSyntax.LocalNode.IdentityQueryCommand

    ''' <summary> Gets the serial number query command. </summary>
    ''' <value> The serial number query command. </value>
    Protected Overrides ReadOnly Property SerialNumberQueryCommand As String = TspSyntax.LocalNode.SerialNumberPrintNumberCommand

    ''' <summary> Parse version information. </summary>
    ''' <param name="value"> The value. </param>
    Protected Overrides Sub ParseVersionInfo(value As String)
        MyBase.ParseVersionInfo(value)
        Me._VersionInfo = New VersionInfo
        Me.VersionInfo.Parse(value)
    End Sub

    ''' <summary> Gets or sets the information describing the version. </summary>
    ''' <value> Information describing the version. </value>
    Public Overloads ReadOnly Property VersionInfo As VersionInfo

#End Region

#Region " DEVICE ERRORS "

    ''' <summary> Gets or sets the clear error queue command. </summary>
    ''' <value> The clear error queue command. </value>
    Protected Overrides ReadOnly Property ClearErrorQueueCommand As String = TspSyntax.EventLog.ClearEventLogCommand

    ''' <summary> Gets the last error query command. </summary>
    ''' <value> The last error query command. </value>
    Protected Overrides ReadOnly Property DeviceErrorQueryCommand As String = "" 'VI.Pith.Scpi.Syntax.LastSystemErrorQueryCommand

    ''' <summary> Gets or sets the 'Next Error' query command. </summary>
    ''' <value> The error queue query command. </value>
    Protected Overrides ReadOnly Property DequeueErrorQueryCommand As String = "" 'VI.Pith.Scpi.Syntax.LastSystemErrorQueryCommand

    ''' <summary> Gets the 'Next Error' query command. </summary>
    ''' <value> The error queue query command. </value>
    Protected Overrides ReadOnly Property NextDeviceErrorQueryCommand As String = TspSyntax.EventLog.NextErrorFormattedPrintCommand

    ''' <summary> Enqueue device error. </summary>
    ''' <param name="compoundErrorMessage"> Message describing the compound error. </param>
    ''' <returns> <c>true</c> if it succeeds; otherwise <c>false</c> </returns>
    Protected Overrides Function EnqueueDeviceError(ByVal compoundErrorMessage As String) As VI.DeviceError
        Dim de As DeviceError = New DeviceError
        de.Parse(compoundErrorMessage)
        If de.IsError Then Me.DeviceErrorQueue.Enqueue(de)
        Return de
    End Function

#End Region

#Region " COLLECT GARBAGE "

    ''' <summary> Gets or sets the collect garbage wait complete command. </summary>
    ''' <value> The collect garbage wait complete command. </value>
    Protected Overrides ReadOnly Property CollectGarbageWaitCompleteCommand As String = LuaSyntax.CollectGarbageWaitCompleteCommand

#End Region

End Class

''' <summary>Enumerates the status bits for the operations register.</summary>
<Flags()> Public Enum OperationEventBits
    <ComponentModel.Description("Empty")> None = 0

    ''' <summary>Calibrating.</summary>
    <ComponentModel.Description("Calibrating")> Calibrating = &H1

    ''' <summary>Measuring.</summary>
    <ComponentModel.Description("Measuring")> Measuring = &H10

    ''' <summary>Prompts enabled.</summary>
    <ComponentModel.Description("Prompts Enabled")> Prompts = &H800

    ''' <summary>User Register.</summary>
    <ComponentModel.Description("User Register")> UserRegister = &H1000

    ''' <summary>User Register.</summary>
    <ComponentModel.Description("Instrument summary")> InstrumentSummary = &H2000

    ''' <summary>Program running.</summary>
    <ComponentModel.Description("Program Running")> ProgramRunning = &H4000

    ''' <summary>Unknown value. Sets bit 16 (zero based and beyond the register size).</summary>
    <ComponentModel.Description("Unknown")> Unknown = &H10000

End Enum

