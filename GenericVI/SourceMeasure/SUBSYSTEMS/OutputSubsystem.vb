Imports isr.Core.Pith
Imports isr.Core.Pith.EnumExtensions
''' <summary> Defines a SCPI Output Subsystem for a generic Source Measure instrument. </summary>
''' <license>
''' (c) 2012 Integrated Scientific Resources, Inc.<para>
''' Licensed under The MIT License. </para><para>
''' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
''' BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
''' NON-INFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
''' DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
''' OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
''' </para> </license>
Public Class OutputSubsystem
    Inherits VI.Scpi.OutputSubsystemBase

#Region " CONSTRUCTION + CLEANUP "

    ''' <summary> Initializes a new instance of the <see cref="OutputSubsystem" /> class. </summary>
    ''' <param name="statusSubsystem "> A reference to a <see cref="StatusSubsystemBase">message based
    ''' session</see>. </param>
    Public Sub New(ByVal statusSubsystem As VI.StatusSubsystemBase)
        MyBase.New(statusSubsystem)
    End Sub

#End Region

#Region " PUBLISHER "

    ''' <summary> Publishes all values by raising the property changed events. </summary>
    Public Overrides Sub Publish()
        If Me.Publishable Then
            For Each p As Reflection.PropertyInfo In Reflection.MethodInfo.GetCurrentMethod.DeclaringType.GetProperties()
                Me.SafePostPropertyChanged(p.Name)
            Next
        End If
    End Sub

#End Region

#Region " COMMAND SYNTAX "

#Region " OFF MODE "

    Protected Overrides ReadOnly Property OffModeQueryCommand As String = ":OUTP:SMOD?"


    Protected Overrides ReadOnly Property OffModeCommandFormat As String = ":OUTP:SMOD {0}"

#End Region

#Region " ON/OFF STATE "

    Protected Overrides ReadOnly Property OutputOnStateQueryCommand As String = ":OUTP:STAT?"

    Protected Overrides ReadOnly Property OutputOnStateCommandFormat As String = ":OUTP:STAT {0:'1';'1';'0'}"

#End Region

#Region " TERMINAL MODE "

    ''' <summary> Gets the terminals mode query command. </summary>
    ''' <value> The terminals mode query command. </value>
    Protected Overrides ReadOnly Property TerminalsModeQueryCommand As String = ":OUTP:ROUT:TERM?"

    ''' <summary> Gets the terminals mode command format. </summary>
    ''' <value> The terminals mode command format. </value>
    Protected Overrides ReadOnly Property TerminalsModeCommandFormat As String = ":OUTP:ROUT:TERM {0}"

#End Region

#End Region

End Class
