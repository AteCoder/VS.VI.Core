''' <summary> Implements a measured value amount. </summary>
''' <license> (c) 2013 Integrated Scientific Resources, Inc.<para>
''' Licensed under The MIT License. </para><para>
''' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
''' BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
''' NON-INFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
''' DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
''' OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
''' </para> </license>
''' <history date="11/1/2013" by="David" revision=""> Created. </history>
Public Class MeasuredAmount
    Inherits ReadingAmount

#Region " CONSTRUCTORS  and  DESTRUCTORS "

    ''' <summary> Constructs a measured value without specifying the value or its validity, which must
    ''' be specified for the value to be made valid. </summary>
    Public Sub New()
        MyBase.New()
        Me._MetaStatus = New MetaStatus()
        Me._ComplianceLimitMargin = 0.001
    End Sub

    ''' <summary> Constructs a copy of an existing value. </summary>
    ''' <param name="model"> The model. </param>
    Public Sub New(ByVal model As MeasuredAmount)
        MyBase.New(model)
        If model IsNot Nothing Then
            Me._HighLimit = model._HighLimit
            Me._LowLimit = model._LowLimit
            Me._MetaStatus = New MetaStatus(model.MetaStatus)
            Me._ComplianceLimit = model._ComplianceLimit
            Me._ComplianceLimitMargin = model._ComplianceLimitMargin
        End If
    End Sub

#End Region

#Region " AMOUNT "

    ''' <summary> Resets measured value to nothing. </summary>
    Public Overrides Sub Reset()
        MyBase.Reset()
        Me.MetaStatus.Reset()
        Me.Generator.Min = Me.LowLimit
        Me.Generator.Max = Me.HighLimit
    End Sub

    ''' <summary> Returns the default string representation of the value. </summary>
    ''' <param name="format"> The format string. </param>
    Public Overloads Function ToString(ByVal format As String) As String
        If Me.MetaStatus.IsValid Then
            Return Me.Amount.ToString(format)
        Else
            Return Me._MetaStatus.ToShortDescription("p")
        End If
    End Function

#End Region

#Region " META STATUS "

    ''' <summary> Gets or sets the measured value meta status. </summary>
    ''' <value> The measured value status. </value>
    Public Property MetaStatus() As MetaStatus

#End Region

#Region " STATUS "

    ''' <summary> Gets or sets the high limit. </summary>
    ''' <value> The high limit. </value>
    Public Property HighLimit() As Double

    ''' <summary> Gets or sets the low limit. </summary>
    ''' <value> The low limit. </value>
    Public Property LowLimit() As Double

    ''' <summary> Gets or sets the compliance limit for testing if the reading exceeded the compliance
    ''' level. </summary>
    ''' <value> A <see cref="System.Double">Double</see> value. </value>
    Public Property ComplianceLimit() As Double

    ''' <summary> Gets or sets the margin of how close will allow the measured value to the compliance
    ''' limit.  For instance, if the margin is 0.001, the measured value must not exceed 99.9% of the
    ''' compliance limit. The default is 0.001. </summary>
    ''' <value> A <see cref="System.Double">Double</see> value. </value>
    Public Property ComplianceLimitMargin() As Double

#End Region

#Region " AMOUNT "

    ''' <summary> Parses the reading to create the specific reading type in the inherited class. </summary>
    ''' <remarks> Assumes that reading is a number. </remarks>
    ''' <param name="valueReading"> Specifies the value reading. </param>
    ''' <returns> <c>True</c> if parsed; Otherwise, <c>False</c>. </returns>
    Public Overrides Function TryParse(ByVal valueReading As String, ByVal unitsReading As String) As Boolean
        If MyBase.TryParse(valueReading, unitsReading) Then
            Return Me.TryParse(valueReading)
        Else
            Me.MetaStatus.IsValid = False
        End If
        Return Me.MetaStatus.IsValid
    End Function

    ''' <summary> Parses the reading to create the specific reading type in the inherited class. </summary>
    ''' <remarks> Assumes that reading is a number. </remarks>
    ''' <param name="valueReading"> Specifies the value reading. </param>
    ''' <returns> <c>True</c> if parsed; Otherwise, <c>False</c>. </returns>
    Public Overrides Function TryParse(ByVal valueReading As String) As Boolean
        If MyBase.TryParse(valueReading) Then
            Me._MetaStatus.IsValid = True
            Dim newValue As Double = Me.Value.Value
            Me.MetaStatus.ToggleBit(MetaStatusBit.Infinity, Math.Abs(newValue - Scpi.Syntax.Infinity) < 1)
            Me.MetaStatus.ToggleBit(MetaStatusBit.NegativeInfinity, Math.Abs(newValue - Scpi.Syntax.NegativeInfinity) < 1)
            Me.MetaStatus.ToggleBit(MetaStatusBit.NotANumber, Math.Abs(newValue - Scpi.Syntax.NotANumber) < 1)
            Me.MetaStatus.HitLevelCompliance = Not (newValue >= Me._ComplianceLimit) Xor (Me._ComplianceLimit > 0)
            Me.MetaStatus.IsHigh = Me.Value.Value.CompareTo(Me.HighLimit) > 0
            Me.MetaStatus.IsLow = Me.Value.Value.CompareTo(Me.LowLimit) < 0
        Else
            Me.MetaStatus.IsValid = False
        End If
        Return Me.MetaStatus.IsValid
    End Function

#End Region

End Class