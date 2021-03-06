''' <summary> Defines a Trace Subsystem for a Keithley 7500 Meter. </summary>
''' <license> (c) 2012 Integrated Scientific Resources, Inc.<para>
''' Licensed under The MIT License. </para><para>
''' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
''' BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
''' NON-INFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
''' DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
''' OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
''' </para> </license>
''' <history date="9/26/2012" by="David" revision="1.0.4652"> Created. </history>
Public Class TraceSubsystem
    Inherits VI.Scpi.TraceSubsystemBase

#Region " CONSTRUCTION + CLEANUP "

    ''' <summary> Initializes a new instance of the <see cref="TraceSubsystem" /> class. </summary>
    ''' <param name="statusSubsystem "> A reference to a <see cref="StatusSubsystemBase">message based
    ''' session</see>. </param>
    Public Sub New(ByVal statusSubsystem As VI.StatusSubsystemBase)
        MyBase.New(statusSubsystem)
    End Sub

#End Region

#Region " I PRESETTABLE "

    ''' <summary> Sets the subsystem to its reset state. </summary>
    Public Overrides Sub ResetKnownState()
        MyBase.ResetKnownState()
        Me.PointsCount = 10
        Me.FeedSource = VI.Scpi.FeedSource.None
        Me.FeedControl = VI.Scpi.FeedControl.Never
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

    ''' <summary> Gets or sets the Clear Buffer command. </summary>
    ''' <value> The ClearBuffer command. </value>
    Protected Overrides ReadOnly Property ClearBufferCommand As String = ":TRAC:CLE"

    ''' <summary> Gets the points count query command. </summary>
    ''' <value> The points count query command. </value>
    Protected Overrides ReadOnly Property PointsCountQueryCommand As String = ":TRAC:POIN?"

    ''' <summary> Gets the points count command format. </summary>
    ''' <value> The points count command format. </value>
    Protected Overrides ReadOnly Property PointsCountCommandFormat As String = ":TRAC:POIN 10; *WAI; :TRAC:POIN {0}"

    ''' <summary> Gets or sets the ActualPoint count query command. </summary>
    ''' <value> The ActualPoint count query command. </value>
    Protected Overrides ReadOnly Property ActualPointCountQueryCommand As String = ":TRAC:ACT?"

    ''' <summary> Gets or sets The First Point Number query command. </summary>
    ''' <value> The First Point Number query command. </value>
    Protected Overrides ReadOnly Property FirstPointNumberQueryCommand As String = ":TRAC:ACT:STAR?"

    ''' <summary> Gets or sets The Last Point Number query command. </summary>
    ''' <value> The Last Point Number query command. </value>
    Protected Overrides ReadOnly Property LastPointNumberQueryCommand As String = ":TRAC:ACT:END?"

#End Region

#Region " DATA "

    Protected Overrides ReadOnly Property DataQueryCommand As String = ":TRAC:DATA?"

#End Region

End Class

