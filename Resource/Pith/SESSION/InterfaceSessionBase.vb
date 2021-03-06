﻿''' <summary> An interface session base class. </summary>
''' <license>
''' (c) 2015 Integrated Scientific Resources, Inc. All rights reserved.<para>
''' Licensed under The MIT License.</para><para>
''' THE SOFTWARE IS PROVIDED 'AS IS', WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
''' BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
''' NON-INFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
''' DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
''' OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.</para>
''' </license>
''' <history date="11/24/2015" by="David" revision=""> Created. </history>
Public MustInherit Class InterfaceSessionBase
    Inherits isr.Core.Pith.PropertyNotifyBase
    Implements IDisposable

    ''' <summary> Specialized constructor for use only by derived class. </summary>
    Protected Sub New()
        MyBase.New
    End Sub

    ''' <summary> Gets name of the resource. </summary>
    ''' <value> The name of the resource. </value>
    Public ReadOnly Property ResourceName As String

    ''' <summary> Gets the sentinel indicating weather this is a dummy session. </summary>
    ''' <value> The dummy sentinel. </value>
    Public MustOverride ReadOnly Property IsDummy As Boolean

    Private _IsOpen As Boolean
    ''' <summary> Gets or sets the is open. </summary>
    ''' <value> The is open. </value>
    Public Property IsOpen As Boolean
        Get
            Return Me._IsOpen
        End Get
        Protected Set(value As Boolean)
            If value <> Me.IsOpen Then
                Me._IsOpen = value
                Me.SafePostPropertyChanged(NameOf(InterfaceSessionBase.IsOpen))
                Me.SafePostPropertyChanged(NameOf(InterfaceSessionBase.ResourceName))
            End If
        End Set
    End Property

    ''' <summary> Closes the <see cref="InterfaceSessionBase">Interface Session</see>. </summary>
    Public Overridable Sub CloseSession()
        Me.IsOpen = False
    End Sub

    ''' <summary> Opens a <see cref="InterfaceSessionBase">Interface Session</see>. </summary>
    ''' <param name="resourceName"> Name of the resource. </param>
    ''' <param name="timeout">      The timeout. </param>
    Public Overridable Sub OpenSession(ByVal resourceName As String, ByVal timeout As TimeSpan)
        Me._ResourceName = resourceName
        Me.IsOpen = True
    End Sub

    ''' <summary> Opens a <see cref="InterfaceSessionBase">Interface Session</see>. </summary>
    ''' <param name="resourceName"> Name of the resource. </param>
    Public Overridable Sub OpenSession(ByVal resourceName As String)
        Me._ResourceName = resourceName
        Me.IsOpen = True
    End Sub

    ''' <summary> Sends the interface clear. </summary>
    Public MustOverride Sub SendInterfaceClear()

    ''' <summary> Returns all instruments to some default state. </summary>
    Public MustOverride Sub ClearDevices()

    ''' <summary> Clears the specified device. </summary>
    ''' <param name="gpibAddress"> The instrument address. </param>
    Public MustOverride Sub SelectiveDeviceClear(ByVal gpibAddress As Integer)

    ''' <summary> Clears the specified device. </summary>
    ''' <param name="resourceName"> Name of the resource. </param>
    Public MustOverride Sub SelectiveDeviceClear(ByVal resourceName As String)

    ''' <summary> Clears the interface. </summary>
    Public MustOverride Sub ClearInterface()

    ''' <summary> Gets the type of the hardware interface. </summary>
    ''' <value> The type of the hardware interface. </value>
    Public MustOverride ReadOnly Property HardwareInterfaceType As VI.Pith.HardwareInterfaceType

    ''' <summary> Gets the hardware interface number. </summary>
    ''' <value> The hardware interface number. </value>
    Public MustOverride ReadOnly Property HardwareInterfaceNumber As Integer

#Region " Disposable Support"

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
            If Not Me.IsDisposed Then
                If disposing Then
                Else
                    ' put finalize code here.
                End If
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

#End Region

End Class

