''' <summary> Defines a Multimeter Subsystem for a TSP System. </summary>
''' <license> (c) 2016 Integrated Scientific Resources, Inc.<para>
''' Licensed under The MIT License. </para><para>
''' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
''' BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
''' NON-INFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
''' DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
''' OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
''' </para> </license>
''' <history date="01/15/2016" by="David" revision=""> Created. </history>
Public MustInherit Class MultimeterSubsystemBase
    Inherits SubsystemPlusStatusBase

#Region " CONSTRUCTORS  and  DESTRUCTORS "

    ''' <summary> Initializes a new instance of the <see cref="DisplaySubsystemBase" /> class. </summary>
    ''' <param name="statusSubsystem"> A reference to a <see cref="statusSubsystemBase">status
    ''' Subsystem</see>. </param>
    Protected Sub New(ByVal statusSubsystem As StatusSubsystemBase)
        MyBase.New(statusSubsystem)
        Me._Amount = New MeasuredAmount
        Me._FunctionModeRanges = New FunctionRangeDictionary
        For Each fm As MultimeterFunctionMode In [Enum].GetValues(GetType(MultimeterFunctionMode))
            Me._FunctionModeRanges.Add(fm, New Core.Pith.RangeR(0, 1))
        Next
        Me._OpenDetectorKnownStates = New MultimeterFunctionEnabledDictionary
        For Each fm As MultimeterFunctionMode In [Enum].GetValues(GetType(MultimeterFunctionMode))
            Me._OpenDetectorKnownStates.Add(fm, False)
        Next
    End Sub

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
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

#End Region

#Region " APERTURE "

    ''' <summary> The aperture range in seconds. </summary>
    Public MustOverride ReadOnly Property ApertureRange As Core.Pith.RangeR

    ''' <summary> The Aperture. </summary>
    Private _Aperture As Double?

    ''' <summary> Gets or sets the cached sense Aperture. Set to
    ''' <see cref="Scpi.Syntax.Infinity">infinity</see> to set to maximum or to
    ''' <see cref="Scpi.Syntax.NegativeInfinity">negative infinity</see> for minimum. </summary>
    ''' <value> <c>null</c> if value is not known. </value>
    Public Overloads Property Aperture As Double?
        Get
            Return Me._Aperture
        End Get
        Protected Set(ByVal value As Double?)
            If Not Nullable.Equals(Me.Aperture, value) Then
                Me._Aperture = value
                If value.HasValue Then
                    Me.PowerLineCycles = StatusSubsystemBase.PowerLineCycles(TimeSpan.FromTicks(CLng(TimeSpan.TicksPerSecond * Me._Aperture.Value)))
                Else
                    Me.PowerLineCycles = New Double?
                End If
                Me.AsyncNotifyPropertyChanged(NameOf(Me.Aperture))
            End If
        End Set
    End Property

    ''' <summary> Writes and reads back the sense Aperture. </summary>
    ''' <param name="value"> The Aperture. </param>
    ''' <returns> The Aperture. </returns>
    Public Function ApplyAperture(ByVal value As Double) As Double?
        Me.WriteAperture(value)
        Return Me.QueryAperture
    End Function

    ''' <summary> Gets or sets The Aperture query command. </summary>
    ''' <value> The Aperture query command. </value>
    Protected Overridable ReadOnly Property ApertureQueryCommand As String

    ''' <summary> Queries The Aperture. </summary>
    ''' <returns> The Aperture or none if unknown. </returns>
    Public Function QueryAperture() As Double?
        Me.Aperture = Me.Query(Me.Aperture, Me.ApertureQueryCommand)
        Return Me.Aperture
    End Function

    ''' <summary> Gets or sets The Aperture command format. </summary>
    ''' <value> The Aperture command format. </value>
    Protected Overridable ReadOnly Property ApertureCommandFormat As String

    ''' <summary> Writes The Aperture without reading back the value from the device. </summary>
    ''' <remarks> This command sets The Aperture. </remarks>
    ''' <param name="value"> The Aperture. </param>
    ''' <returns> The Aperture. </returns>
    Public Function WriteAperture(ByVal value As Double) As Double?
        Me.Aperture = Me.Write(Math.Min(ApertureRange.Max, Math.Max(ApertureRange.Min, value)), Me.ApertureCommandFormat)
        Return Me.Aperture
    End Function

#End Region

#Region " AUTO DELAY MODE "

    ''' <summary> The Auto Delay Mode. </summary>
    Private _AutoDelayMode As MultimeterAutoDelayMode?

    ''' <summary> Gets or sets the cached Auto Delay Mode. </summary>
    ''' <value> The <see cref="MultimeterAutoDelayMode">Multimeter Auto Delay Mode</see> or none if not set or
    ''' unknown. </value>
    Public Overloads Property AutoDelayMode As MultimeterAutoDelayMode?
        Get
            Return Me._AutoDelayMode
        End Get
        Protected Set(ByVal value As MultimeterAutoDelayMode?)
            If Not Nullable.Equals(Me.AutoDelayMode, value) Then
                Me._AutoDelayMode = value
                Me.AsyncNotifyPropertyChanged(NameOf(Me.AutoDelayMode))
            End If
        End Set
    End Property

    ''' <summary> Queries the Multimeter Auto Delay Mode. </summary>
    ''' <returns> The <see cref="MultimeterAutoDelayMode">Multimeter Auto Delay Mode</see> or none if unknown. </returns>
    Public MustOverride Function QueryAutoDelayMode() As MultimeterAutoDelayMode?

    ''' <summary> Writes and reads back the Multimeter Auto Delay Mode. </summary>
    ''' <param name="value"> The  Multimeter Auto Delay Mode. </param>
    ''' <returns> The <see cref="MultimeterAutoDelayMode">Multimeter Auto Delay Mode</see> or none if unknown. </returns>
    Public Function ApplyAutoDelayMode(ByVal value As MultimeterAutoDelayMode) As MultimeterAutoDelayMode?
        Me.WriteAutoDelayMode(value)
        Return Me.QueryAutoDelayMode()
    End Function

    ''' <summary> Writes the Multimeter Auto Delay Mode without reading back the value from the device. </summary>
    ''' <param name="value"> The Auto Delay Mode. </param>
    ''' <returns> The <see cref="MultimeterAutoDelayMode">Multimeter Auto Delay Mode</see> or none if unknown. </returns>
    Public MustOverride Function WriteAutoDelayMode(ByVal value As MultimeterAutoDelayMode) As MultimeterAutoDelayMode?

#End Region

#Region " AUTO RANGE ENABLED "

    ''' <summary> Auto Range enabled. </summary>
    Private _AutoRangeEnabled As Boolean?

    ''' <summary> Gets or sets the cached Auto Range Enabled sentinel. </summary>
    ''' <value> <c>null</c> if Auto Range Enabled is not known; <c>True</c> if output is on; otherwise,
    ''' <c>False</c>. </value>
    Public Property AutoRangeEnabled As Boolean?
        Get
            Return Me._AutoRangeEnabled
        End Get
        Protected Set(ByVal value As Boolean?)
            If Not Boolean?.Equals(Me.AutoRangeEnabled, value) Then
                Me._AutoRangeEnabled = value
                Me.AsyncNotifyPropertyChanged(NameOf(Me.AutoRangeEnabled))
            End If
        End Set
    End Property

    ''' <summary> Writes and reads back the Auto Range Enabled sentinel. </summary>
    ''' <param name="value">  if set to <c>True</c> if enabling; False if disabling. </param>
    ''' <returns> <c>True</c> if enabled; otherwise <c>False</c>. </returns>
    Public Function ApplyAutoRangeEnabled(ByVal value As Boolean) As Boolean?
        Me.WriteAutoRangeEnabled(value)
        Return Me.QueryAutoRangeEnabled()
    End Function

    ''' <summary> Gets or sets the automatic Range enabled query command. </summary>
    ''' <value> The automatic Range enabled query command. </value>
    ''' <remarks> SCPI: "system:RANG:AUTO?" </remarks>
    Protected Overridable ReadOnly Property AutoRangeEnabledQueryCommand As String

    ''' <summary> Queries the Auto Range Enabled sentinel. Also sets the
    ''' <see cref="AutoRangeEnabled">Enabled</see> sentinel. </summary>
    ''' <returns> <c>True</c> if enabled; otherwise <c>False</c>. </returns>
    Public Function QueryAutoRangeEnabled() As Boolean?
        Me.AutoRangeEnabled = Me.Query(Me.AutoRangeEnabled, Me.AutoRangeEnabledQueryCommand)
        Return Me.AutoRangeEnabled
    End Function

    ''' <summary> Gets or sets the automatic Range enabled command Format. </summary>
    ''' <value> The automatic Range enabled query command. </value>
    ''' <remarks> SCPI: "system:RANGE:AUTO {0:'ON';'ON';'OFF'}" </remarks>
    Protected Overridable ReadOnly Property AutoRangeEnabledCommandFormat As String

    ''' <summary> Writes the Auto Range Enabled sentinel. Does not read back from the instrument. </summary>
    ''' <param name="value"> if set to <c>True</c> is enabled. </param>
    ''' <returns> <c>True</c> if enabled; otherwise <c>False</c>. </returns>
    Public Function WriteAutoRangeEnabled(ByVal value As Boolean) As Boolean?
        Me.AutoRangeEnabled = Me.Write(value, Me.AutoRangeEnabledCommandFormat)
        Return Me.AutoRangeEnabled
    End Function

#End Region

#Region " AUTO ZERO ENABLED "

    ''' <summary> Auto Zero enabled. </summary>
    Private _AutoZeroEnabled As Boolean?

    ''' <summary> Gets or sets the cached Auto Zero Enabled sentinel. </summary>
    ''' <value> <c>null</c> if Auto Zero Enabled is not known; <c>True</c> if output is on; otherwise,
    ''' <c>False</c>. </value>
    Public Property AutoZeroEnabled As Boolean?
        Get
            Return Me._AutoZeroEnabled
        End Get
        Protected Set(ByVal value As Boolean?)
            If Not Boolean?.Equals(Me.AutoZeroEnabled, value) Then
                Me._AutoZeroEnabled = value
                Me.AsyncNotifyPropertyChanged(NameOf(Me.AutoZeroEnabled))
            End If
        End Set
    End Property

    ''' <summary> Writes and reads back the Auto Zero Enabled sentinel. </summary>
    ''' <param name="value">  if set to <c>True</c> if enabling; False if disabling. </param>
    ''' <returns> <c>True</c> if enabled; otherwise <c>False</c>. </returns>
    Public Function ApplyAutoZeroEnabled(ByVal value As Boolean) As Boolean?
        Me.WriteAutoZeroEnabled(value)
        Return Me.QueryAutoZeroEnabled()
    End Function

    ''' <summary> Gets or sets the automatic Zero enabled query command. </summary>
    ''' <value> The automatic Zero enabled query command. </value>
    ''' <remarks> SCPI: "system:RANG:AUTO?" </remarks>
    Protected Overridable ReadOnly Property AutoZeroEnabledQueryCommand As String

    ''' <summary> Queries the Auto Zero Enabled sentinel. Also sets the
    ''' <see cref="AutoZeroEnabled">Enabled</see> sentinel. </summary>
    ''' <returns> <c>True</c> if enabled; otherwise <c>False</c>. </returns>
    Public Function QueryAutoZeroEnabled() As Boolean?
        Me.AutoZeroEnabled = Me.Query(Me.AutoZeroEnabled, Me.AutoZeroEnabledQueryCommand)
        Return Me.AutoZeroEnabled
    End Function

    ''' <summary> Gets or sets the automatic Zero enabled command Format. </summary>
    ''' <value> The automatic Zero enabled query command. </value>
    ''' <remarks> SCPI: "system:Zero:AUTO {0:'ON';'ON';'OFF'}" </remarks>
    Protected Overridable ReadOnly Property AutoZeroEnabledCommandFormat As String

    ''' <summary> Writes the Auto Zero Enabled sentinel. Does not read back from the instrument. </summary>
    ''' <param name="value"> if set to <c>True</c> is enabled. </param>
    ''' <returns> <c>True</c> if enabled; otherwise <c>False</c>. </returns>
    Public Function WriteAutoZeroEnabled(ByVal value As Boolean) As Boolean?
        Me.AutoZeroEnabled = Me.Write(value, Me.AutoZeroEnabledCommandFormat)
        Return Me.AutoZeroEnabled
    End Function

#End Region

#Region " FILTER "

#Region " FILTER COUNT "

    ''' <summary> The Filter Count range in seconds. </summary>
    Public MustOverride ReadOnly Property FilterCountRange As Core.Pith.RangeI

    ''' <summary> The FilterCount. </summary>
    Private _FilterCount As Integer?

    ''' <summary> Gets or sets the cached sense Filter Count. Set to
    ''' <see cref="Scpi.Syntax.Infinity">infinity</see> to set to maximum or to
    ''' <see cref="Scpi.Syntax.NegativeInfinity">negative infinity</see> for minimum. </summary>
    ''' <value> <c>null</c> if value is not known. </value>
    Public Overloads Property FilterCount As Integer?
        Get
            Return Me._FilterCount
        End Get
        Protected Set(ByVal value As Integer?)
            If Not Nullable.Equals(Me.FilterCount, value) Then
                Me._FilterCount = value
                Me.AsyncNotifyPropertyChanged(NameOf(Me.FilterCount))
            End If
        End Set
    End Property

    ''' <summary> Writes and reads back the sense Filter Count. </summary>
    ''' <param name="value"> The Filter Count. </param>
    ''' <returns> The Filter Count. </returns>
    Public Function ApplyFilterCount(ByVal value As Integer) As Integer?
        Me.WriteFilterCount(value)
        Return Me.QueryFilterCount
    End Function

    ''' <summary> Gets or sets The Filter Count query command. </summary>
    ''' <value> The FilterCount query command. </value>
    Protected Overridable ReadOnly Property FilterCountQueryCommand As String

    ''' <summary> Queries The Filter Count. </summary>
    ''' <returns> The Filter Count or none if unknown. </returns>
    Public Function QueryFilterCount() As Integer?
        Me.FilterCount = Me.Query(Me.FilterCount, Me.FilterCountQueryCommand)
        Return Me.FilterCount
    End Function

    ''' <summary> Gets or sets The Filter Count command format. </summary>
    ''' <value> The FilterCount command format. </value>
    Protected Overridable ReadOnly Property FilterCountCommandFormat As String

    ''' <summary> Writes The Filter Count without reading back the value from the device. </summary>
    ''' <remarks> This command sets The Filter Count. </remarks>
    ''' <param name="value"> The Filter Count. </param>
    ''' <returns> The Filter Count. </returns>
    Public Function WriteFilterCount(ByVal value As Integer) As Integer?
        Me.FilterCount = Me.Write(Math.Min(FilterCountRange.Max, Math.Max(FilterCountRange.Min, value)), Me.FilterCountCommandFormat)
        Return Me.FilterCount
    End Function

#End Region

#Region " FILTER ENABLED "

    ''' <summary> Filter enabled. </summary>
    Private _FilterEnabled As Boolean?

    ''' <summary> Gets or sets the cached Filter Enabled sentinel. </summary>
    ''' <value> <c>null</c> if Filter Enabled is not known; <c>True</c> if output is on; otherwise,
    ''' <c>False</c>. </value>
    Public Property FilterEnabled As Boolean?
        Get
            Return Me._FilterEnabled
        End Get
        Protected Set(ByVal value As Boolean?)
            If Not Boolean?.Equals(Me.FilterEnabled, value) Then
                Me._FilterEnabled = value
                Me.AsyncNotifyPropertyChanged(NameOf(Me.FilterEnabled))
            End If
        End Set
    End Property

    ''' <summary> Writes and reads back the Filter Enabled sentinel. </summary>
    ''' <param name="value">  if set to <c>True</c> if enabling; False if disabling. </param>
    ''' <returns> <c>True</c> if enabled; otherwise <c>False</c>. </returns>
    Public Function ApplyFilterEnabled(ByVal value As Boolean) As Boolean?
        Me.WriteFilterEnabled(value)
        Return Me.QueryFilterEnabled()
    End Function

    ''' <summary> Gets or sets the Filter enabled query command. </summary>
    ''' <value> The Filter enabled query command. </value>
    ''' <remarks> TSP: "print(dmm.filter.enable==1)" </remarks>
    Protected Overridable ReadOnly Property FilterEnabledQueryCommand As String

    ''' <summary> Queries the Filter Enabled sentinel. Also sets the
    ''' <see cref="FilterEnabled">Enabled</see> sentinel. </summary>
    ''' <returns> <c>True</c> if enabled; otherwise <c>False</c>. </returns>
    Public Function QueryFilterEnabled() As Boolean?
        Me.FilterEnabled = Me.Query(Me.FilterEnabled, Me.FilterEnabledQueryCommand)
        Return Me.FilterEnabled
    End Function

    ''' <summary> Gets or sets the Filter enabled command Format. </summary>
    ''' <value> The Filter enabled query command. </value>
    ''' <remarks> TSP "dmm.filter.enable={0:'1';'1';'0'}" </remarks>
    Protected Overridable ReadOnly Property FilterEnabledCommandFormat As String

    ''' <summary> Writes the Filter Enabled sentinel. Does not read back from the instrument. </summary>
    ''' <param name="value"> if set to <c>True</c> is enabled. </param>
    ''' <returns> <c>True</c> if enabled; otherwise <c>False</c>. </returns>
    Public Function WriteFilterEnabled(ByVal value As Boolean) As Boolean?
        Me.FilterEnabled = Me.Write(value, Me.FilterEnabledCommandFormat)
        Return Me.FilterEnabled
    End Function

#End Region

#Region " MOVING AVERAGE FILTER ENABLED "

    ''' <summary> Moving Average Filter enabled. </summary>
    Private _MovingAverageFilterEnabled As Boolean?

    ''' <summary> Gets or sets the cached Moving Average Filter Enabled sentinel. </summary>
    ''' <value> <c>null</c> if Moving Average Filter Enabled is not known; <c>True</c> if output is on; otherwise,
    ''' <c>False</c>. </value>
    Public Property MovingAverageFilterEnabled As Boolean?
        Get
            Return Me._MovingAverageFilterEnabled
        End Get
        Protected Set(ByVal value As Boolean?)
            If Not Boolean?.Equals(Me.MovingAverageFilterEnabled, value) Then
                Me._MovingAverageFilterEnabled = value
                Me.AsyncNotifyPropertyChanged(NameOf(Me.MovingAverageFilterEnabled))
            End If
        End Set
    End Property

    ''' <summary> Writes and reads back the Moving Average Filter Enabled sentinel. </summary>
    ''' <param name="value">  if set to <c>True</c> if enabling; False if disabling. </param>
    ''' <returns> <c>True</c> if enabled; otherwise <c>False</c>. </returns>
    Public Function ApplyMovingAverageFilterEnabled(ByVal value As Boolean) As Boolean?
        Me.WriteMovingAverageFilterEnabled(value)
        Return Me.QueryMovingAverageFilterEnabled()
    End Function

    ''' <summary> Gets or sets the Moving Average Filter enabled query command. </summary>
    ''' <value> The Moving Average Filter enabled query command. </value>
    ''' <remarks> TSP: "print(dmm.filter.type=0)" </remarks>
    Protected Overridable ReadOnly Property MovingAverageFilterEnabledQueryCommand As String

    ''' <summary> Queries the Moving Average Filter Enabled sentinel. Also sets the
    ''' <see cref="MovingAverageFilterEnabled">Enabled</see> sentinel. </summary>
    ''' <returns> <c>True</c> if enabled; otherwise <c>False</c>. </returns>
    Public Function QueryMovingAverageFilterEnabled() As Boolean?
        Me.MovingAverageFilterEnabled = Me.Query(Me.MovingAverageFilterEnabled, Me.MovingAverageFilterEnabledQueryCommand)
        Return Me.MovingAverageFilterEnabled
    End Function

    ''' <summary> Gets or sets the Moving Average Filter enabled command Format. </summary>
    ''' <value> The Moving Average Filter enabled query command. </value>
    ''' <remarks> TSP: "dmm.filter.type={0:'0';'0';'1'}" </remarks>
    Protected Overridable ReadOnly Property MovingAverageFilterEnabledCommandFormat As String

    ''' <summary> Writes the Moving Average Filter Enabled sentinel. Does not read back from the instrument. </summary>
    ''' <param name="value"> if set to <c>True</c> is enabled. </param>
    ''' <returns> <c>True</c> if enabled; otherwise <c>False</c>. </returns>
    Public Function WriteMovingAverageFilterEnabled(ByVal value As Boolean) As Boolean?
        Me.MovingAverageFilterEnabled = Me.Write(value, Me.MovingAverageFilterEnabledCommandFormat)
        Return Me.MovingAverageFilterEnabled
    End Function

#End Region

#Region " FILTER WINDOW "

    ''' <summary> The Filter Window range in seconds. </summary>
    Public MustOverride ReadOnly Property FilterWindowRange As Core.Pith.RangeR

    ''' <summary> The FilterWindow. </summary>
    Private _FilterWindow As Double?

    ''' <summary> Gets or sets the cached sense Filter Window. Set to
    ''' <see cref="Scpi.Syntax.Infinity">infinity</see> to set to maximum or to
    ''' <see cref="Scpi.Syntax.NegativeInfinity">negative infinity</see> for minimum. </summary>
    ''' <value> <c>null</c> if value is not known. </value>
    Public Overloads Property FilterWindow As Double?
        Get
            Return Me._FilterWindow
        End Get
        Protected Set(ByVal value As Double?)
            If Not Nullable.Equals(Me.FilterWindow, value) Then
                Me._FilterWindow = value
                Me.AsyncNotifyPropertyChanged(NameOf(Me.FilterWindow))
            End If
        End Set
    End Property

    ''' <summary> Writes and reads back the sense Filter Window. </summary>
    ''' <param name="value"> The Filter Window. </param>
    ''' <returns> The Filter Window. </returns>
    Public Function ApplyFilterWindow(ByVal value As Double) As Double?
        Me.WriteFilterWindow(value)
        Return Me.QueryFilterWindow
    End Function

    ''' <summary> Gets or sets The Filter Window query command. </summary>
    ''' <value> The FilterWindow query command. </value>
    Protected Overridable ReadOnly Property FilterWindowQueryCommand As String

    ''' <summary> Queries The Filter Window. </summary>
    ''' <returns> The Filter Window or none if unknown. </returns>
    Public Function QueryFilterWindow() As Double?
        Dim value As Double? = Me.Query(Me.FilterWindow, Me.FilterWindowQueryCommand)
        If value.HasValue Then Me.FilterWindow = 100 * value.Value Else Me.FilterWindow = New Double?
        Return Me.FilterWindow
    End Function

    ''' <summary> Gets or sets The Filter Window command format. </summary>
    ''' <value> The FilterWindow command format. </value>
    Protected Overridable ReadOnly Property FilterWindowCommandFormat As String

    ''' <summary> Writes The Filter Window without reading back the value from the device. </summary>
    ''' <remarks> This command sets The Filter Window. </remarks>
    ''' <param name="value"> The Filter Window. </param>
    ''' <returns> The Filter Window. </returns>
    Public Function WriteFilterWindow(ByVal value As Double) As Double?
        Me.FilterWindow = Me.Write(Math.Min(FilterWindowRange.Max, Math.Max(FilterWindowRange.Min, 100 * value)), Me.FilterWindowCommandFormat)
        Return Me.FilterWindow
    End Function

#End Region

#End Region

#Region " FUNCTION MODE "

    ''' <summary> The function mode. </summary>
    Private _FunctionMode As MultimeterFunctionMode?

    ''' <summary> Parse units. </summary>
    ''' <remarks> David, 1/15/2016. </remarks>
    ''' <param name="value"> The  Multimeter Function Mode. </param>
    ''' <returns> An Arebis.TypedUnits.Unit. </returns>
    Public Shared Function ParseUnits(ByVal value As MultimeterFunctionMode) As Arebis.TypedUnits.Unit
        Select Case value
            Case MultimeterFunctionMode.CurrentAC, MultimeterFunctionMode.CurrentDC
                Return Arebis.StandardUnits.ElectricUnits.Ampere
            Case MultimeterFunctionMode.VoltageAC, MultimeterFunctionMode.VoltageDC
                Return Arebis.StandardUnits.ElectricUnits.Volt
            Case MultimeterFunctionMode.ResistanceCommonWire, MultimeterFunctionMode.ResistanceFourWire, MultimeterFunctionMode.ResistanceTwoWire
                Return Arebis.StandardUnits.ElectricUnits.Ohm
            Case Else
                Return Arebis.StandardUnits.ElectricUnits.Volt
        End Select
    End Function

    ''' <summary> Gets or sets the cached function mode. </summary>
    ''' <value> The <see cref="MultimeterFunctionMode">Multimeter Function Mode</see> or none if not set or
    ''' unknown. </value>
    Public Overloads Property FunctionMode As MultimeterFunctionMode?
        Get
            Return Me._FunctionMode
        End Get
        Protected Set(ByVal value As MultimeterFunctionMode?)
            If Not Nullable.Equals(Me.FunctionMode, value) Then
                Me._FunctionMode = value
                If value.HasValue Then
                    Me.Amount.Unit = MultimeterSubsystemBase.ParseUnits(value.Value)
                    Me._RangeRange = Me.FunctionModeRanges(value.Value)
                End If
                Me.AsyncNotifyPropertyChanged(NameOf(Me.FunctionMode))
            End If
        End Set
    End Property

    ''' <summary> Queries the Multimeter Function Mode. </summary>
    ''' <returns> The <see cref="MultimeterFunctionMode">Multimeter Function Mode</see> or none if unknown. </returns>
    Public MustOverride Function QueryFunctionMode() As MultimeterFunctionMode?

    ''' <summary> Writes and reads back the Multimeter Function Mode. </summary>
    ''' <param name="value"> The  Multimeter Function Mode. </param>
    ''' <returns> The <see cref="MultimeterFunctionMode">Multimeter Function Mode</see> or none if unknown. </returns>
    Public Function ApplyFunctionMode(ByVal value As MultimeterFunctionMode) As MultimeterFunctionMode?
        Me.WriteFunctionMode(value)
        ' changing the function mode changes range, auto delay mode and open detector enabled. 
        Me.QueryRange()
        Me.QueryAutoDelayMode()
        Me.QueryOpenDetectorEnabled()
        Return FunctionMode
    End Function

    ''' <summary> Writes the Multimeter Function Mode without reading back the value from the device. </summary>
    ''' <param name="value"> The Function Mode. </param>
    ''' <returns> The <see cref="MultimeterFunctionMode">Multimeter Function Mode</see> or none if unknown. </returns>
    Public MustOverride Function WriteFunctionMode(ByVal value As MultimeterFunctionMode) As MultimeterFunctionMode?

#End Region

#Region " MEASURE "

    ''' <summary> Gets or sets the amount. </summary>
    ''' <value> The amount. </value>
    Public ReadOnly Property Amount As MeasuredAmount

    ''' <summary> The Reading. </summary>
    Private _Reading As Double?

    ''' <summary> Gets or sets the cached sense Reading. Set to
    ''' <see cref="Scpi.Syntax.Infinity">infinity</see> to set to maximum or to
    ''' <see cref="Scpi.Syntax.NegativeInfinity">negative infinity</see> for minimum. </summary>
    ''' <value> <c>null</c> if value is not known. </value>
    Public Overloads Property Reading As Double?
        Get
            Return Me._Reading
        End Get
        Protected Set(ByVal value As Double?)
            If Not Nullable.Equals(Me.Reading, value) Then
                Me._Reading = value
                If value.HasValue Then
                    Me.Amount.Value = value
                End If
                Me.AsyncNotifyPropertyChanged(NameOf(Me.Reading))
            End If
        End Set
    End Property

    ''' <summary> Gets or sets The Measure query command. </summary>
    ''' <value> The Measure query command. </value>
    Protected Overridable ReadOnly Property MeasureQueryCommand As String

    ''' <summary> Queries The reading. </summary>
    ''' <returns> The reading or none if unknown. </returns>
    Public Function Measure() As Double?
        Me.Reading = Me.Query(Me.Reading, Me.MeasureQueryCommand)
        Return Me.Reading
    End Function

#End Region

#Region " OPEN DETECTOR ENABLED "

    ''' <summary> Gets or sets a list of states of the open detector knowns. </summary>
    ''' <value> The open detector known states. </value>
    Public ReadOnly Property OpenDetectorKnownStates As MultimeterFunctionEnabledDictionary

    ''' <summary> Open Detector enabled. </summary>
    Private _OpenDetectorEnabled As Boolean?

    ''' <summary> Gets or sets the cached Open Detector Enabled sentinel. </summary>
    ''' <value> <c>null</c> if Open Detector Enabled is not known; <c>True</c> if output is on; otherwise,
    ''' <c>False</c>. </value>
    Public Property OpenDetectorEnabled As Boolean?
        Get
            Return Me._OpenDetectorEnabled
        End Get
        Protected Set(ByVal value As Boolean?)
            If Not Boolean?.Equals(Me.OpenDetectorEnabled, value) Then
                Me._OpenDetectorEnabled = value
                Me.AsyncNotifyPropertyChanged(NameOf(Me.OpenDetectorEnabled))
            End If
        End Set
    End Property

    ''' <summary> Writes and reads back the Open Detector Enabled sentinel. </summary>
    ''' <param name="value">  if set to <c>True</c> if enabling; False if disabling. </param>
    ''' <returns> <c>True</c> if enabled; otherwise <c>False</c>. </returns>
    Public Function ApplyOpenDetectorEnabled(ByVal value As Boolean) As Boolean?
        Me.WriteOpenDetectorEnabled(value)
        Return Me.QueryOpenDetectorEnabled()
    End Function

    ''' <summary> Gets or sets the automatic Zero enabled query command. </summary>
    ''' <value> The automatic Zero enabled query command. </value>
    ''' <remarks> TSP: _G.print(_G.dmm.opendetector==1) </remarks>
    Protected Overridable ReadOnly Property OpenDetectorEnabledQueryCommand As String

    ''' <summary> Queries the Open Detector Enabled sentinel. Also sets the
    ''' <see cref="OpenDetectorEnabled">Enabled</see> sentinel. </summary>
    ''' <returns> <c>True</c> if enabled; otherwise <c>False</c>. </returns>
    Public Function QueryOpenDetectorEnabled() As Boolean?
        Me.OpenDetectorEnabled = Me.Query(Me.OpenDetectorEnabled, Me.OpenDetectorEnabledQueryCommand)
        Return Me.OpenDetectorEnabled
    End Function

    ''' <summary> Gets or sets the automatic Zero enabled command Format. </summary>
    ''' <value> The automatic Zero enabled query command. </value>
    ''' <remarks> TSP: _G.opendetector={0:'1';'1';'0'}" </remarks>
    Protected Overridable ReadOnly Property OpenDetectorEnabledCommandFormat As String

    ''' <summary> Writes the Open Detector Enabled sentinel. Does not read back from the instrument. </summary>
    ''' <param name="value"> if set to <c>True</c> is enabled. </param>
    ''' <returns> <c>True</c> if enabled; otherwise <c>False</c>. </returns>
    Public Function WriteOpenDetectorEnabled(ByVal value As Boolean) As Boolean?
        Me.OpenDetectorEnabled = Me.Write(value, Me.OpenDetectorEnabledCommandFormat)
        Return Me.OpenDetectorEnabled
    End Function

#End Region

#Region " POWER LINE CYCLES (NPLC) "

    ''' <summary> The power line cycles range in seconds. </summary>
    Public MustOverride ReadOnly Property PowerLineCyclesRange As Core.Pith.RangeR

    ''' <summary> The Power Line Cycles. </summary>
    Private _PowerLineCycles As Double?

    ''' <summary> Gets or sets the cached sense PowerLineCycles. Set to
    ''' <see cref="Scpi.Syntax.Infinity">infinity</see> to set to maximum or to
    ''' <see cref="Scpi.Syntax.NegativeInfinity">negative infinity</see> for minimum. </summary>
    ''' <value> <c>null</c> if value is not known. </value>
    Public Overloads Property PowerLineCycles As Double?
        Get
            Return Me._PowerLineCycles
        End Get
        Protected Set(ByVal value As Double?)
            If Not Nullable.Equals(Me.PowerLineCycles, value) Then
                Me._PowerLineCycles = value
                Me._Aperture = StatusSubsystemBase.IntegrationPeriod(Me._PowerLineCycles.Value).TotalSeconds
                Me.AsyncNotifyPropertyChanged(NameOf(Me.PowerLineCycles))
            End If
        End Set
    End Property

    ''' <summary> Writes and reads back the sense PowerLineCycles. </summary>
    ''' <param name="value"> The Power Line Cycles. </param>
    ''' <returns> The Power Line Cycles. </returns>
    Public Function ApplyPowerLineCycles(ByVal value As Double) As Double?
        Me.WritePowerLineCycles(value)
        Return Me.QueryPowerLineCycles
    End Function

    ''' <summary> Gets or sets The Power Line Cycles query command. </summary>
    ''' <value> The Power Line Cycles query command. </value>
    Protected Overridable ReadOnly Property PowerLineCyclesQueryCommand As String

    ''' <summary> Queries The Power Line Cycles. </summary>
    ''' <returns> The Power Line Cycles or none if unknown. </returns>
    Public Function QueryPowerLineCycles() As Double?
        Me.PowerLineCycles = Me.Query(Me.PowerLineCycles, Me.PowerLineCyclesQueryCommand)
        Return Me.PowerLineCycles
    End Function

    ''' <summary> Gets or sets The Power Line Cycles command format. </summary>
    ''' <value> The Power Line Cycles command format. </value>
    Protected Overridable ReadOnly Property PowerLineCyclesCommandFormat As String

    ''' <summary> Writes The Power Line Cycles without reading back the value from the device. </summary>
    ''' <remarks> This command sets The Power Line Cycles. </remarks>
    ''' <param name="value"> The Power Line Cycles. </param>
    ''' <returns> The Power Line Cycles. </returns>
    Public Function WritePowerLineCycles(ByVal value As Double) As Double?
        Me.PowerLineCycles = Me.Write(Math.Min(PowerLineCyclesRange.Max, Math.Max(PowerLineCyclesRange.Min, value)), Me.PowerLineCyclesCommandFormat)
        Return Me.PowerLineCycles
    End Function

#End Region

#Region " RANGE "

    ''' <summary> Gets or sets the function mode ranges. </summary>
    ''' <value> The function mode ranges. </value>
    Public ReadOnly Property FunctionModeRanges As FunctionRangeDictionary

    ''' <summary> The Range of the range. </summary>
    Public ReadOnly Property RangeRange As Core.Pith.RangeR

    ''' <summary> The Range. </summary>
    Private _Range As Double?

    ''' <summary> Gets or sets the cached sense Range. Set to
    ''' <see cref="Scpi.Syntax.Infinity">infinity</see> to set to maximum or to
    ''' <see cref="Scpi.Syntax.NegativeInfinity">negative infinity</see> for minimum. </summary>
    ''' <value> <c>null</c> if value is not known. </value>
    Public Overloads Property Range As Double?
        Get
            Return Me._Range
        End Get
        Protected Set(ByVal value As Double?)
            If Not Nullable.Equals(Me.Range, value) Then
                Me._Range = value
                Me.AsyncNotifyPropertyChanged(NameOf(Me.Range))
            End If
        End Set
    End Property

    ''' <summary> Writes and reads back the sense Range. </summary>
    ''' <param name="value"> The Range. </param>
    ''' <returns> The Range. </returns>
    Public Function ApplyRange(ByVal value As Double) As Double?
        Me.WriteRange(value)
        Return Me.QueryRange
    End Function

    ''' <summary> Gets or sets The Range query command. </summary>
    ''' <value> The Range query command. </value>
    Protected Overridable ReadOnly Property RangeQueryCommand As String

    ''' <summary> Queries The Range. </summary>
    ''' <returns> The Range or none if unknown. </returns>
    Public Function QueryRange() As Double?
        Me.Range = Me.Query(Me.Range, Me.RangeQueryCommand)
        Return Me.Range
    End Function

    ''' <summary> Gets or sets The Range command format. </summary>
    ''' <value> The Range command format. </value>
    Protected Overridable ReadOnly Property RangeCommandFormat As String

    ''' <summary> Writes The Range without reading back the value from the device. </summary>
    ''' <remarks> This command sets The Range. </remarks>
    ''' <param name="value"> The Range. </param>
    ''' <returns> The Range. </returns>
    Public Function WriteRange(ByVal value As Double) As Double?
        Me.Range = Me.Write(Math.Min(RangeRange.Max, Math.Max(RangeRange.Min, value)), Me.RangeCommandFormat)
        Return Me.Range
    End Function

#End Region

End Class

''' <summary> Specifies the Auto Delay modes. </summary>
Public Enum MultimeterAutoDelayMode
    <ComponentModel.Description("Off (_G.dmm.OFF)")> [Off] = 0
    <ComponentModel.Description("On (_G.dmm.OFF)")> [On] = 1
    <ComponentModel.Description("Once (_G.dmm.AUTODELAY_ONCE)")> [Once] = 2
End Enum


''' <summary> Specifies the function modes. </summary>
Public Enum MultimeterFunctionMode
    <ComponentModel.Description("None")> None
    <ComponentModel.Description("Resistance Common Side (commonsideohms)")> ResistanceCommonWire
    <ComponentModel.Description("Resistance 2-Wire (twowireohms)")> ResistanceTwoWire
    <ComponentModel.Description("Resistance 4-Wire (fourwireohms)")> ResistanceFourWire
    <ComponentModel.Description("DC Voltage (dcvolts)")> VoltageDC
    <ComponentModel.Description("DC Current (dccurrent)")> CurrentDC
    <ComponentModel.Description("AC Voltage (acvolts)")> VoltageAC
    <ComponentModel.Description("AC Current (accurrent)")> CurrentAC
End Enum

''' <summary> Dictionary of function ranges. </summary>
''' <remarks> David, 1/16/2016. </remarks>
''' <license>
''' (c) 2016 Integrated Scientific Resources, Inc. All rights reserved.<para>
''' Licensed under The MIT License.</para><para>
''' THE SOFTWARE IS PROVIDED 'AS IS', WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
''' BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
''' NON-INFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
''' DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
''' OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.</para>
''' </license>
''' <history date="1/16/2016" by="David" revision=""> Created. </history>
<CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")>
Public Class FunctionRangeDictionary
    Inherits Collections.Generic.Dictionary(Of MultimeterFunctionMode, Core.Pith.RangeR)
End Class

''' <summary> Dictionary of function-related enabled functionality. </summary>
''' <remarks> David, 2/8/2016. </remarks>
''' <license>
''' (c) 2016 Integrated Scientific Resources, Inc. All rights reserved.<para>
''' Licensed under The MIT License.</para><para>
''' THE SOFTWARE IS PROVIDED 'AS IS', WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
''' BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
''' NON-INFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
''' DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
''' OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.</para>
''' </license>
''' <history date="2/8/2016" by="David" revision=""> Created. </history>
<CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")>
Public Class MultimeterFunctionEnabledDictionary
    Inherits Collections.Generic.Dictionary(Of MultimeterFunctionMode, Boolean)
End Class
