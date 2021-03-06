''' <summary>  Defines the contract that must be implemented by a Source Measure Unit Measure Subsystem. </summary>
''' <license> (c) 2012 Integrated Scientific Resources, Inc.<para>
''' Licensed under The MIT License. </para><para>
''' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
''' BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
''' NON-INFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
''' DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
''' OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
''' </para> </license>
''' <history date="9/26/2012" by="David" revision="1.0.4652"> Created. </history>
Public Class MeasureVoltageSubsystemBase
    Inherits SourceMeasureUnitBase

#Region " CONSTRUCTION + CLEANUP "

    ''' <summary> Initializes a new instance of the <see cref="StatusSubsystemBase" /> class. </summary>
    ''' <param name="statusSubsystem "> A reference to a <see cref="VI.Tsp.StatusSubsystemBase">status subsystem</see>. </param>
    Public Sub New(ByVal statusSubsystem As VI.StatusSubsystemBase)
        MyBase.New(statusSubsystem)
    End Sub

#End Region

#Region " I PRESETTABLE "

    ''' <summary> Sets the subsystem to its reset state. </summary>
    Public Overrides Sub ResetKnownState()
        MyBase.ResetKnownState()
        Me.AutoRangeVoltageEnabled = True
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

#End Region

#Region " AUTO RANGE VOLTAGE ENABLED "

    ''' <summary> Auto Range Voltage enabled. </summary>
    Private _AutoRangeVoltageEnabled As Boolean?

    ''' <summary> Gets or sets the cached Auto Range Voltage Enabled sentinel. </summary>
    ''' <value> <c>null</c> if Auto Range Voltage Enabled is not known; <c>True</c> if output is on; otherwise,
    ''' <c>False</c>. </value>
    Public Property AutoRangeVoltageEnabled As Boolean?
        Get
            Return Me._AutoRangeVoltageEnabled
        End Get
        Protected Set(ByVal value As Boolean?)
            If Not Boolean?.Equals(Me.AutoRangeVoltageEnabled, value) Then
                Me._AutoRangeVoltageEnabled = value
                Me.SafePostPropertyChanged()
            End If
        End Set
    End Property

    ''' <summary> Writes the enabled state of the current Auto Range Voltage and reads back the value from the
    ''' device. </summary>
    ''' <remarks> This command enables or disables the over-current Auto Range Voltage (OCP)
    ''' function. The enabled state is On (1); the disabled state is Off (0). If the over-current
    ''' AutoRangeVoltage function is enabled and the output goes into constant current operation, the output
    ''' is disabled and OCP is set in the Questionable Condition status register. The *RST value =
    ''' Off. </remarks>
    ''' <param name="value"> Enable if set to <c>true</c>; otherwise, disable. </param>
    ''' <returns> <c>True</c> <see cref="AutoRangeVoltageEnabled">enabled</see>;
    '''           <c>False</c> otherwise. </returns>
    Public Function ApplyAutoRangeVoltageEnabled(ByVal value As Boolean) As Boolean?
        Me.WriteAutoRangeVoltageEnabled(value)
        Return Me.QueryAutoRangeVoltageEnabled()
    End Function

    ''' <summary> Gets the automatic range voltage enabled query command. </summary>
    ''' <value> The automatic range voltage enabled query command. </value>
    Protected Overridable ReadOnly Property AutoRangeVoltageEnabledQueryCommand As String
        Get
            Return String.Format(Me.AutoRangeVoltageEnabledQueryCommandFormat, Me.SourceMeasureUnitReference)
        End Get
    End Property

    Protected Overridable ReadOnly Property AutoRangeVoltageEnabledQueryCommandFormat As String = "print({0}.measure.autorangev)"

    ''' <summary> Queries the current AutoRangeVoltage state. </summary>
    ''' <returns> <c>True</c> <see cref="AutoRangeVoltageEnabled">enabled</see>;
    '''           <c>False</c> otherwise. </returns>
    Public Function QueryAutoRangeVoltageEnabled() As Boolean?
        Me.AutoRangeVoltageEnabled = Me.Session.Query(Me.AutoRangeVoltageEnabled.GetValueOrDefault(True), Me.AutoRangeVoltageEnabledQueryCommand)
        ' Me.AutoRangeVoltageEnabled = Me.Session.Query(Me.AutoRangeVoltageEnabled.GetValueOrDefault(True), "print({0}.measure.autorangev)", Me.SourceMeasureUnitReference)
        Return Me.AutoRangeVoltageEnabled
    End Function

    ''' <summary> Gets the automatic range voltage enabled command format. </summary>
    ''' <value> The automatic range voltage enabled command format. </value>
    Protected Overridable ReadOnly Property AutoRangeVoltageEnabledCommandFormat As String = "{0}.measure.autorangev = {{0:'1';'1';'0'}} "

    ''' <summary> Gets the automatic range voltage enabled command. </summary>
    ''' <value> The automatic range voltage enabled command. </value>
    Protected Overridable ReadOnly Property AutoRangeVoltageEnabledCommand As String
        Get
            Return String.Format(AutoRangeVoltageEnabledCommandFormat, Me.SourceMeasureUnitReference)
        End Get
    End Property

    ''' <summary> Writes the enabled state of the current Auto Range Voltage without reading back the value from
    ''' the device. </summary>
    ''' <remarks> This command enables or disables the over-current AutoRangeVoltage (OCP)
    ''' function. The enabled state is On (1); the disabled state is Off (0). If the over-current
    ''' AutoRangeVoltage function is enabled and the output goes into constant current operation, the output
    ''' is disabled and OCP is set in the Questionable Condition status register. The *RST value =
    ''' Off. </remarks>
    ''' <param name="value"> Enable if set to <c>true</c>; otherwise, disable. </param>
    ''' <returns> <c>True</c> <see cref="AutoRangeVoltageEnabled">enabled</see>;
    '''           <c>False</c> otherwise. </returns>
    Public Function WriteAutoRangeVoltageEnabled(ByVal value As Boolean) As Boolean?
        ' Me.Session.WriteLine(String.Format(Globalization.CultureInfo.InvariantCulture, "{0}.measure.autorangev = {{0:'1';'1';'0'}} ", Me.SourceMeasureUnitReference), CType(value, Integer))
        Me.Session.WriteLine(Me.AutoRangeVoltageEnabledCommand, CType(value, Integer))
        Me.AutoRangeVoltageEnabled = value
        Return Me.AutoRangeVoltageEnabled
    End Function

#End Region

#Region " READING "

    Private _Reading As String
    ''' <summary> Gets  or sets (protected) the reading.  When set, the value is converted to resistance. </summary>
    ''' <value> The reading. </value>
    Public Property Reading() As String
        Get
            Return Me._Reading
        End Get
        Protected Set(ByVal value As String)
            If String.IsNullOrWhiteSpace(value) Then value = ""
            If Not String.Equals(value, Me.Reading, StringComparison.OrdinalIgnoreCase) OrElse
                Me._Reading = value Then
                Me.SafePostPropertyChanged()
            End If
        End Set
    End Property

#End Region

#Region " VOLTAGE "

    Private _Voltage As Double?
    ''' <summary> Gets or sets (protected) the measured resistance. </summary>
    ''' <value> The resistance. </value>
    Public Property Voltage() As Double?
        Get
            Return Me._Voltage
        End Get
        Protected Set(ByVal value As Double?)
            If Not Nullable.Equals(value, Me.Voltage) Then
                Me._Voltage = value
                Me.SafePostPropertyChanged()
            End If
        End Set
    End Property

    ''' <summary> Turns on the source and measures. </summary>
    Public Sub Measure()

        Dim printFormat As String = "%8.5f"
        Me.Session.WriteLine("{0}.source.output = {0}.OUTPUT_ON waitcomplete() print(string.format('{1}',{0}.measure.r())) ",
                                 Me.SourceMeasureUnitReference, printFormat)
        Me.Reading = Me.Session.ReadLine()
        Dim value As Double = 0
        If String.IsNullOrWhiteSpace(Me.Reading) Then
            Me.Voltage = New Double?
        Else
            If Double.TryParse(Me.Reading, Globalization.NumberStyles.Number Or Globalization.NumberStyles.AllowExponent,
                               Globalization.CultureInfo.InvariantCulture, value) Then
                Me.Voltage = value
            Else
                Me.Voltage = New Double?
                Throw New InvalidCastException(String.Format(Globalization.CultureInfo.InvariantCulture,
                                                              "Failed parsing {0} to number reading '{1}'", Me.Reading, Me.Session.LastMessageSent))

            End If
        End If
    End Sub

#End Region

End Class

