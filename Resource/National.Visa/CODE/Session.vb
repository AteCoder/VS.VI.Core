﻿''' <summary> A National Instrument message based session. </summary>
''' <remarks> David, 11/20/2015. </remarks>
''' <license>
''' (c) 2015 Integrated Scientific Resources, Inc. All rights reserved.<para>
''' Licensed under The MIT License.</para><para>
''' THE SOFTWARE IS PROVIDED 'AS IS', WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
''' BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
''' NON-INFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
''' DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
''' OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.</para>
''' </license>
''' <history date="11/20/2015" by="David" revision=""> Created. </history>
Public Class Session
    Inherits SessionBase

#Region " CONSTRUCTORS  and  DESTRUCTORS "

    ''' <summary> Initializes a new instance of the MessageBasedSession object from the specified
    ''' resource name. </summary>
    Public Sub New()
        MyBase.New()
    End Sub

#Region " Disposable Support"

    ''' <summary>
    ''' Releases the unmanaged resources used by the <see cref="T:System.Windows.Forms.Control" />
    ''' and its child controls and optionally releases the managed resources.
    ''' </summary>
    ''' <param name="disposing"> true to release both managed and unmanaged resources; false to
    '''                          release only unmanaged resources. </param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If Not Me.IsDisposed AndAlso disposing Then
                Try
                    Me.DiscardSession()
                    Me._LastNativeError = Nothing
                Catch ex As Exception
                    Debug.Assert(Not Debugger.IsAttached, "Failed discarding enabled events.",
                                         "Failed discarding enabled events. Details: {0}", ex)
                End Try
            End If
        Finally
            Me.IsDisposed = True
            MyBase.Dispose(disposing)
        End Try
    End Sub

#End Region

#End Region

#Region " SESSION "

    ''' <summary> Gets the sentinel indicating weather this is a dummy session. </summary>
    ''' <exception cref="NativeException"> Thrown when a Native error condition occurs. </exception>
    ''' <value> The dummy sentinel. </value>
    Public Overrides ReadOnly Property IsDummy As Boolean = False

    Private _VisaSession As NationalInstruments.Visa.MessageBasedSession
    ''' <summary> The visa session. </summary>
    ''' <remarks> Must be defined without events; Otherwise, setting the timeout causes a memory exception. </remarks>
    Private Property VisaSession As NationalInstruments.Visa.MessageBasedSession
        Get
            Return Me._VisaSession
        End Get
        Set(value As NationalInstruments.Visa.MessageBasedSession)
            Me._VisaSession = value
            Me._TcpIpSession = TryCast(Me.VisaSession, NationalInstruments.Visa.TcpipSession)
        End Set
    End Property

    ''' <summary> Gets the type of the hardware interface. </summary>
    ''' <value> The type of the hardware interface. </value>
    <CLSCompliant(False)>
    Public ReadOnly Property HardwareInterfaceType As Ivi.Visa.HardwareInterfaceType
        Get
            If Me.VisaSession Is Nothing Then
                Return Ivi.Visa.HardwareInterfaceType.Custom
            Else
                Return Me.VisaSession.HardwareInterfaceType
            End If
        End Get
    End Property

    ''' <summary> Gets the TCP IP session. </summary>
    ''' <value> The TCP IP session. </value>
    <CLSCompliant(False)>
    Public ReadOnly Property TcpIpSession As NationalInstruments.Visa.TcpipSession

    ''' <summary>
    ''' Gets the session open sentinel. When open, the session is capable of addressing the hardware.
    ''' See also <see cref="P:isr.VI.SessionBase.IsDeviceOpen" />.
    ''' </summary>
    ''' <value> The is session open. </value>
    Public Overrides ReadOnly Property IsSessionOpen As Boolean
        Get
            Return Me.VisaSession IsNot Nothing AndAlso Not Me.VisaSession.IsDisposed
        End Get
    End Property

    ''' <summary>
    ''' Initializes a new instance of the <see cref="NationalInstruments.Visa.Session" /> class.
    ''' </summary>
    ''' <remarks>
    ''' This method does not lock the resource. Rev 4.1 and 5.0 of VISA did not support this call and
    ''' could not verify the resource.
    ''' </remarks>
    ''' <exception cref="NativeException"> Thrown when a Native error condition occurs. </exception>
    ''' <param name="resourceName"> Name of the resource. </param>
    ''' <param name="timeout">      The open timeout. </param>
    Protected Overrides Sub CreateSession(ByVal resourceName As String, ByVal timeout As TimeSpan)
        Try
            Me._LastNativeError = NativeError.Success
            Dim result As Ivi.Visa.ResourceOpenStatus = Ivi.Visa.ResourceOpenStatus.Success
            If Me.Enabled Then
                Using resMgr As New NationalInstruments.Visa.ResourceManager
                    Me.VisaSession = CType(resMgr.Open(resourceName, Ivi.Visa.AccessModes.None, CInt(timeout.TotalMilliseconds), result),
                                            NationalInstruments.Visa.MessageBasedSession)
                End Using
            End If
            If result <> Ivi.Visa.ResourceOpenStatus.Success OrElse
               Not Me.KeepAlive OrElse Not Me.IsAlive Then Throw New OperationFailedException($"Resource not found;. {resourceName}")
        Catch ex As Ivi.Visa.NativeVisaException
            Me.DisposeSession()
            Me._LastNativeError = New NativeError(ex.ErrorCode, resourceName, "@opening", "opening session")
            Throw New NativeException(Me._LastNativeError, ex)
        Catch
            Me.DisposeSession()
            Throw
        End Try
    End Sub

    ''' <summary> Dispose session. </summary>
    ''' <remarks> David, 3/14/2016. </remarks>
    Private Sub DisposeSession()
        If Me.VisaSession IsNot Nothing AndAlso Not Me.VisaSession.IsDisposed Then
            Me._TcpIpSession = Nothing
            Me._VisaSession.Dispose()
            Me._VisaSession = Nothing
        End If
    End Sub

    ''' <summary> Discards the session. </summary>
    ''' <remarks> David, 1/25/2016. </remarks>
    ''' <exception cref="NativeException"> Thrown when a Native error condition occurs. </exception>
    Protected Overrides Sub DiscardSession()
        If Me.IsSessionOpen Then
            Try
                Me._LastNativeError = NativeError.Success
                Me.VisaSession.DiscardEvents(Ivi.Visa.EventType.AllEnabled)
                Me.DisableServiceRequest()
            Catch ex As Ivi.Visa.NativeVisaException
                Me._LastNativeError = New NativeError(ex.ErrorCode, Me.ResourceName, "@discarding", "discarding session")
                Throw New NativeException(Me._LastNativeError, ex)
            Finally
                Me.DisposeSession()
            End Try
        End If
    End Sub

    ''' <summary>
    ''' Searches for a listeners for the specified <see cref="ResourceName">reasource name</see>.
    ''' </summary>
    ''' <remarks>
    ''' David, 11/27/2015. Updates <see cref="ResourceFound ">Resource Exists</see>. This could
    ''' return a false positive because the resource manager does not always rely on physical check
    ''' of existence. The NI VISA Manager returns affirmative if the resource is listed in the
    ''' configuration file.
    ''' </remarks>
    ''' <returns> <c>true</c> if it the resource exists; otherwise <c>false</c> </returns>
    Public Overrides Function FindResource() As Boolean
        If Me.Enabled Then
            Using rm As New ResourceManager
                Me.ResourceFound = rm.Exists(Me.ResourceName)
            End Using
        Else
            Me.ResourceFound = True
        End If
        Return Me.ResourceFound
    End Function

    ''' <summary>
    ''' Gets or sets the sentinel indicating if call backs are performed in a specific
    ''' synchronization context.
    ''' </summary>
    ''' <remarks>
    ''' For .NET Framework 2.0, use SynchronizeCallbacks to specify that the object marshals
    ''' callbacks across threads appropriately.<para>
    ''' DH: 3339 Setting true prevents display.
    ''' </para><para>
    ''' Note that setting to false also breaks display updates.
    ''' </para>
    ''' </remarks>
    ''' <value>
    ''' The sentinel indicating if call backs are performed in a specific synchronization context.
    ''' </value>
    Public Overrides Property SynchronizeCallBacks As Boolean
        Get
            If Me.IsSessionOpen Then
                MyBase.SynchronizeCallbacks = Me.VisaSession.SynchronizeCallbacks
            End If
            Return MyBase.SynchronizeCallbacks
        End Get
        Set(value As Boolean)
            MyBase.SynchronizeCallbacks = value
            If Me.IsSessionOpen Then
                Me.VisaSession.SynchronizeCallbacks = value
            End If
        End Set
    End Property


#End Region

#Region " ERROR / STATUS "

    Private _LastNativeError As NativeError
    ''' <summary> Gets the last native error. </summary>
    ''' <value> The last native error. </value>
    Protected Overrides ReadOnly Property LastNativeError As NativeErrorBase
        Get
            Return Me._LastNativeError
        End Get
    End Property

#End Region

#Region " ATTRIBUTES "

    <CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")>
    Private Function Get_ReadBufferSize() As Integer
        Dim s As Ivi.Visa.INativeVisaSession = TryCast(Me.VisaSession, Ivi.Visa.INativeVisaSession)
        If s IsNot Nothing Then
            Return s.GetAttributeInt32(Ivi.Visa.NativeVisaAttribute.ReadBufferSize)
        Else
            Return 1024
        End If
    End Function

#End Region

#Region " READ/WRITE "

    ''' <summary> Gets or sets the termination character. </summary>
    ''' <value> The termination character. </value>
    Public Overrides Property TerminationCharacter As Byte
        Get
            If Me.IsSessionOpen Then
                MyBase.TerminationCharacter = Me.VisaSession.TerminationCharacter
            End If
            Return MyBase.TerminationCharacter
        End Get
        Set(value As Byte)
            If value <> Me.TerminationCharacter Then
                MyBase.TerminationCharacter = value
                If Me.IsSessionOpen Then
                    Me.VisaSession.TerminationCharacter = value
                End If
            End If
        End Set
    End Property

    ''' <summary> Gets or sets the timeout. </summary>
    ''' <value> The timeout. </value>
    Public Overrides Property Timeout As TimeSpan
        Get
            If Me.IsSessionOpen Then
                MyBase.Timeout = TimeSpan.FromMilliseconds(Me.VisaSession.TimeoutMilliseconds)
            End If
            Return MyBase.Timeout
        End Get
        Set(value As TimeSpan)
            If value <> Me.Timeout Then
                MyBase.Timeout = value
                If Me.IsSessionOpen Then
                    Me.VisaSession.TimeoutMilliseconds = CInt(value.TotalMilliseconds)
                End If
            End If
        End Set
    End Property

    ''' <summary> Query if 'readStatus' is read ended. </summary>
    ''' <remarks> David, 7/23/2016. </remarks>
    ''' <param name="readStatus"> The read status. </param>
    ''' <returns> <c>true</c> if read ended; otherwise <c>false</c> </returns>
    Private Shared Function IsReadEnded(ByVal readStatus As Ivi.Visa.ReadStatus) As Boolean
        Return readStatus = Ivi.Visa.ReadStatus.EndReceived OrElse
               readStatus = Ivi.Visa.ReadStatus.TerminationCharacterEncountered
    End Function

    ''' <summary>
    ''' Synchronously reads ASCII-encoded string data irrespective of the buffer size. 
    ''' </summary>
    ''' <exception cref="NativeException"> Thrown when a Native error condition occurs. </exception>
    ''' <returns> The received message. </returns>
    Public Overrides Function ReadFreeLine() As String
        Dim builder As New System.Text.StringBuilder
        Try
            Me._LastNativeError = NativeError.Success
            Dim endReadstatus As Ivi.Visa.ReadStatus = Ivi.Visa.ReadStatus.Unknown
            Dim bufferSize As Integer = Me.Get_ReadBufferSize
            If Me.IsSessionOpen Then
                Dim hitEndRead As Boolean = False
                Do
                    Dim msg As String = Me.VisaSession.RawIO.ReadString(bufferSize, endReadstatus)
                    hitEndRead = Session.IsReadEnded(endReadstatus)
                    If hitEndRead Then Me.LastMessageReceived = msg
                    builder.Append(msg)
                    Windows.Forms.Application.DoEvents()
                Loop Until hitEndRead
            Else
                Me.LastMessageReceived = Me.EmulatedReply
            End If
            Return builder.ToString
        Catch ex As Ivi.Visa.NativeVisaException
            If Me.LastNodeNumber.HasValue Then
                Me._LastNativeError = New NativeError(ex.ErrorCode, Me.ResourceName, Me.LastNodeNumber.Value, Me.LastMessageSent, Me.LastAction)
            Else
                Me._LastNativeError = New NativeError(ex.ErrorCode, Me.ResourceName, Me.LastMessageSent, Me.LastAction)
            End If
            Throw New NativeException(Me._LastNativeError, ex)
        Catch ex As Ivi.Visa.IOTimeoutException
            If Me.LastNodeNumber.HasValue Then
                Me._LastNativeError = New NativeError(Ivi.Visa.NativeErrorCode.Timeout, Me.ResourceName,
                                                      Me.LastNodeNumber.Value, Me.LastMessageSent, Me.LastAction)
            Else
                Me._LastNativeError = New NativeError(Ivi.Visa.NativeErrorCode.Timeout, Me.ResourceName,
                                                      Me.LastMessageSent, Me.LastAction)
            End If
            Throw New NativeException(Me._LastNativeError, ex)
        Finally
            ' must clear the reply after each reading otherwise could get cross information.
            Me.EmulatedReply = ""
            Me.LastInputOutputTime = DateTime.Now
        End Try
    End Function

    ''' <summary>
    ''' Synchronously reads ASCII-encoded string data. Reads up to the
    ''' <see cref="TerminationCharacter">termination character</see>.
    ''' </summary>
    ''' <remarks> David, 11/24/2015. </remarks>
    ''' <exception cref="NativeException"> Thrown when a Native error condition occurs. </exception>
    ''' <returns> The received message. </returns>
    Public Overrides Function ReadFiniteLine() As String
        Try
            Me._LastNativeError = NativeError.Success
            If Me.IsSessionOpen Then
                Me.LastMessageReceived = Me.VisaSession.RawIO.ReadString()
            Else
                Me.LastMessageReceived = Me.EmulatedReply
            End If
            ' must clear the reply after each reading otherwise could get cross information.
            Me.EmulatedReply = ""
            Return Me.LastMessageReceived
        Catch ex As Ivi.Visa.NativeVisaException
            If Me.LastNodeNumber.HasValue Then
                Me._LastNativeError = New NativeError(ex.ErrorCode, Me.ResourceName, Me.LastNodeNumber.Value, Me.LastMessageSent, Me.LastAction)
            Else
                Me._LastNativeError = New NativeError(ex.ErrorCode, Me.ResourceName, Me.LastMessageSent, Me.LastAction)
            End If
            Throw New NativeException(Me._LastNativeError, ex)
        Catch ex As Ivi.Visa.IOTimeoutException
            If Me.LastNodeNumber.HasValue Then
                Me._LastNativeError = New NativeError(Ivi.Visa.NativeErrorCode.Timeout, Me.ResourceName,
                                                      Me.LastNodeNumber.Value, Me.LastMessageSent, Me.LastAction)
            Else
                Me._LastNativeError = New NativeError(Ivi.Visa.NativeErrorCode.Timeout, Me.ResourceName,
                                                      Me.LastMessageSent, Me.LastAction)
            End If
            Throw New NativeException(Me._LastNativeError, ex)
        Finally
            Me.LastInputOutputTime = DateTime.Now
        End Try
    End Function

    ''' <summary>
    ''' Synchronously writes ASCII-encoded string data to the device or interface. Terminates the
    ''' data with the <see cref="TerminationCharacter">termination character</see>.
    ''' </summary>
    ''' <remarks> David, 11/24/2015. </remarks>
    ''' <exception cref="NativeException"> Thrown when a Native error condition occurs. </exception>
    ''' <param name="dataToWrite"> The data to write. </param>
    Public Overrides Sub Write(ByVal dataToWrite As String)
        If Not String.IsNullOrWhiteSpace(dataToWrite) Then
            Try
                Me._LastNativeError = NativeError.Success
                If Me.IsSessionOpen Then
                    Me.VisaSession.RawIO.Write(dataToWrite)
                End If
                Me.LastMessageSent = dataToWrite
            Catch ex As Ivi.Visa.NativeVisaException
                If Me.LastNodeNumber.HasValue Then
                    Me._LastNativeError = New NativeError(ex.ErrorCode, Me.ResourceName, Me.LastNodeNumber.Value, dataToWrite, Me.LastAction)
                Else
                    Me._LastNativeError = New NativeError(ex.ErrorCode, Me.ResourceName, dataToWrite, Me.LastAction)
                End If
                Throw New NativeException(Me._LastNativeError, ex)
            Finally
                Me.LastInputOutputTime = DateTime.Now
            End Try
        End If
    End Sub

    ''' <summary> Sends a TCP/IP message to keep the socket connected. </summary>
    ''' <remarks> David, 3/14/2016. </remarks>
    ''' <returns> <c>true</c> if success; otherwise <c>false</c> </returns>
    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Public Overrides Function KeepAlive() As Boolean
        Dim affirmative As Boolean = True
        If Me.IsSessionOpen AndAlso Not Me._TcpIpSession Is Nothing Then
            Try
                If False Then
                    Me.KeepInterfaceAlive()
                Else
                    If Me._TcpIpSession.ResourceLockState = Ivi.Visa.ResourceLockState.NoLock Then Me._TcpIpSession.LockResource()
                    Me._TcpIpSession.UnlockResource()
                End If
            Catch
                affirmative = False
            End Try
        End If
        Return affirmative
    End Function

    Private _InputBufferSize As Integer
    ''' <summary> Gets the size of the input buffer. </summary>
    ''' <value> The size of the input buffer. </value>
    Public Overrides ReadOnly Property InputBufferSize As Integer
        Get
            If Me._InputBufferSize = 0 Then Me._InputBufferSize = Me.Get_ReadBufferSize()
            Return Me._InputBufferSize
        End Get
    End Property

#End Region

#Region " REGISTERS "

    ''' <summary> Reads status byte. </summary>
    ''' <remarks> David, 11/17/2015. </remarks>
    ''' <returns> The status byte. </returns>
    Public Overrides Function ReadStatusByte() As ServiceRequests
        Try
            Me._LastNativeError = NativeError.Success
            Dim value As ServiceRequests = Me.EmulatedStatusByte
            Me.EmulatedStatusByte = 0
            If Me.IsSessionOpen Then
                value = CType(Me.VisaSession.ReadStatusByte, ServiceRequests)
            End If
            Return value
        Catch ex As Ivi.Visa.NativeVisaException
            If Me.LastNodeNumber.HasValue Then
                Me._LastNativeError = New NativeError(ex.ErrorCode, Me.ResourceName, Me.LastNodeNumber.Value, "@STB", Me.LastAction)
            Else
                Me._LastNativeError = New NativeError(ex.ErrorCode, Me.ResourceName, "@STB", Me.LastAction)
            End If
            Throw New NativeException(Me._LastNativeError, ex)
        Finally
            Me.LastInputOutputTime = DateTime.Now
        End Try
    End Function

    ''' <summary> Clears the device. </summary>
    ''' <remarks> David, 11/20/2015. </remarks>
    Public Overrides Sub Clear()
        Try
            Me._LastNativeError = NativeError.Success
            If Me.IsSessionOpen Then Me.VisaSession.Clear()
        Catch ex As Ivi.Visa.NativeVisaException
            If Me.LastNodeNumber.HasValue Then
                Me._LastNativeError = New NativeError(ex.ErrorCode, Me.ResourceName, Me.LastNodeNumber.Value, "@DCL", Me.LastAction)
            Else
                Me._LastNativeError = New NativeError(ex.ErrorCode, Me.ResourceName, "@DCL", Me.LastAction)
            End If
            Throw New NativeException(Me._LastNativeError, ex)
        Finally
            Me.LastInputOutputTime = DateTime.Now
        End Try
    End Sub

#End Region

#Region " EVENTS "

    ''' <summary> Visa session service request. </summary>
    ''' <remarks> David, 11/21/2015. </remarks>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Message based session event information. </param>
    Private Overloads Sub OnServiceRequested(sender As Object, e As Ivi.Visa.VisaEventArgs)
        If sender IsNot Nothing AndAlso e IsNot Nothing AndAlso
             Ivi.Visa.EventType.ServiceRequest = e.EventType Then
            Me.OnServiceRequested()
        End If
    End Sub

    Private _EnabledEventType As Ivi.Visa.EventType
    ''' <summary> Gets the type of the enabled event. </summary>
    ''' <exception cref="NativeException"> Thrown when a Native error condition occurs. </exception>
    ''' <value> The type of the enabled event. </value>
    Private Property EnabledEventType As Ivi.Visa.EventType
        Get
            Return Me._EnabledEventType
        End Get
        Set(value As Ivi.Visa.EventType)
            Me._EnabledEventType = value
            Me.AsyncNotifyPropertyChanged(NameOf(Me.IsServiceRequestEventEnabled))
        End Set
    End Property

    ''' <summary> Gets the sentinel indication if a service request event was enabled. </summary>
    ''' <value> <c>True</c> if service request event is enabled; otherwise, <c>False</c>. </value>
    Public Overrides ReadOnly Property IsServiceRequestEventEnabled As Boolean
        Get
            Return Ivi.Visa.EventType.ServiceRequest = Me.EnabledEventType
        End Get
    End Property

    ''' <summary> Enables the service request. </summary>
    ''' <remarks> David, 11/20/2015. </remarks>
    Public Overrides Sub EnableServiceRequest()
        Try
            If Not Me.IsServiceRequestEventEnabled Then
                Me._LastNativeError = NativeError.Success
                If Me.IsSessionOpen Then
                    ' must define the handler before enabling the events.
                    If Me.HardwareInterfaceType = Ivi.Visa.HardwareInterfaceType.Tcp Then
                        AddHandler Me.TcpIpSession.ServiceRequest, AddressOf Me.OnServiceRequested
                    Else
                        AddHandler Me.VisaSession.ServiceRequest, AddressOf Me.OnServiceRequested
                    End If
                    ' Enabling the VISA session events causes an exception of unsupported mechanism.
                    ' Apparently, the events are already enabled. 
                    ' Me._VisaSession.EnableEvent(Ivi.Visa.EventType.ServiceRequest)
                End If
                Me.EnabledEventType = Ivi.Visa.EventType.ServiceRequest
            End If
        Catch ex As Ivi.Visa.NativeVisaException
            If Me.LastNodeNumber.HasValue Then
                Me._LastNativeError = New NativeError(ex.ErrorCode, Me.ResourceName, Me.LastNodeNumber.Value, "@Enable SRQ", Me.LastAction)
            Else
                Me._LastNativeError = New NativeError(ex.ErrorCode, Me.ResourceName, "@Enable SRQ", Me.LastAction)
            End If
            Throw New NativeException(Me._LastNativeError, ex)
        End Try
    End Sub

    ''' <summary> Disables the service request. </summary>
    ''' <remarks> David, 11/20/2015. </remarks>
    Public Overrides Sub DisableServiceRequest()
        Try
            If Me.IsServiceRequestEventEnabled Then
                Me._LastNativeError = NativeError.Success
                If Me.IsSessionOpen Then
                    Me.VisaSession.DiscardEvents(Ivi.Visa.EventType.ServiceRequest)
                    ' must disable events before removing the handler
                    ' Enabling the VISA session events causes an exception of unsupported mechanism.
                    ' Apparently, the events are already enabled. 
                    ' Me._VisaSession.DisableEvent(Ivi.Visa.EventType.ServiceRequest)
                    RemoveHandler Me.VisaSession.ServiceRequest, AddressOf OnServiceRequested
                End If
                Me.EnabledEventType = Ivi.Visa.EventType.Custom
            End If
        Catch ex As Ivi.Visa.NativeVisaException
            If Me.LastNodeNumber.HasValue Then
                Me._LastNativeError = New NativeError(ex.ErrorCode, Me.ResourceName, Me.LastNodeNumber.Value, "@Disable SRQ", Me.LastAction)
            Else
                Me._LastNativeError = New NativeError(ex.ErrorCode, Me.ResourceName, "@Disable SRQ", Me.LastAction)
            End If
            Throw New NativeException(Me._LastNativeError, ex)
        End Try
    End Sub

#End Region

#Region " TRIGGER "

    ''' <summary> Assert trigger. </summary>
    ''' <remarks> David, 11/20/2015. </remarks>
    Public Overrides Sub AssertTrigger()
        Try
            Me._LastNativeError = NativeError.Success
            If Me.IsSessionOpen Then Me.VisaSession.AssertTrigger()
        Catch ex As Ivi.Visa.NativeVisaException
            If Me.LastNodeNumber.HasValue Then
                Me._LastNativeError = New NativeError(ex.ErrorCode, Me.ResourceName, Me.LastNodeNumber.Value, "@TRG", Me.LastAction)
            Else
                Me._LastNativeError = New NativeError(ex.ErrorCode, Me.ResourceName, "@TRG", Me.LastAction)
            End If
            Throw New NativeException(Me._LastNativeError, ex)
        Finally
            Me.LastInputOutputTime = DateTime.Now
        End Try
    End Sub


#End Region

#Region " INTERFACE "

    ''' <summary> Keeps the TCP/IP interface alive. </summary>
    Public Sub KeepInterfaceAlive()
        If Me.RequiresKeepAlive AndAlso Me.Enabled Then
            Dim sw As System.Diagnostics.Stopwatch = System.Diagnostics.Stopwatch.StartNew
            Using gi As NationalInstruments.Visa.TcpipSocket = New NationalInstruments.Visa.TcpipSocket($"tcpip0::{Me._TcpIpSession.Address}::{Me._TcpIpSession.Port}::socket")
                If gi Is Nothing OrElse gi.IsDisposed Then
                    ' Throw New OperationFailedException($"Failed opening GPIB Interface Session {Me.ResourceInfo.InterfaceResourceName}")
                Else
                    ' gi.KeepAlive = True
                    If gi.ResourceLockState = Ivi.Visa.ResourceLockState.NoLock Then gi.LockResource(1)
                    gi.UnlockResource()
                End If
            End Using
            Debug.Print($"ms: {sw.ElapsedMilliseconds}")
        End If
    End Sub

    ''' <summary> Clears the interface. </summary>
    Public Overrides Sub ClearInterface()
        If Me.SupportsClearInterface AndAlso Me.Enabled Then
            Using gi As GpibInterfaceSession = New GpibInterfaceSession()
                gi.OpenSession(Me.ResourceInfo.InterfaceResourceName)
                If gi.IsOpen Then
                    gi.SelectiveDeviceClear(Me.VisaSession.ResourceName)
                Else
                    Throw New OperationFailedException($"Failed opening GPIB Interface Session {Me.ResourceInfo.InterfaceResourceName}")
                End If
            End Using
        End If
    End Sub

    ''' <summary> Clears the device (SDC). </summary>
    Public Overrides Sub ClearDevice()
        Me.Clear()
        If Me.SupportsClearInterface AndAlso Me.Enabled Then
            Using gi As GpibInterfaceSession = New GpibInterfaceSession()
                gi.OpenSession(Me.ResourceInfo.InterfaceResourceName)
                If gi.IsOpen Then
                    gi.SelectiveDeviceClear(Me.VisaSession.ResourceName)
                Else
                    Throw New OperationFailedException($"Failed opening GPIB Interface Session {Me.ResourceInfo.InterfaceResourceName}")
                End If
            End Using
        End If
    End Sub

#End Region

End Class
