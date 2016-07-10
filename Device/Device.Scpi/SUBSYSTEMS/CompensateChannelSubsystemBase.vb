﻿Imports isr.Core.Pith
Imports isr.Core.Pith.EnumExtensions
''' <summary> Defines the contract that must be implemented by a SCPI Compensation Channel Subsystem. </summary>
''' <license> (c) 2012 Integrated Scientific Resources, Inc.<para>
''' Licensed under The MIT License. </para><para>
''' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
''' BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
''' NON-INFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
''' DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
''' OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
''' </para> </license>
''' <history date="7/6/2016" by="David" revision="4.0.6031"> Created. </history>
Public MustInherit Class CompensateChannelSubsystemBase
    Inherits VI.CompensateChannelSubsystemBase

#Region " CONSTRUCTORS  and  DESTRUCTORS "

    ''' <summary>
    ''' Initializes a new instance of the <see cref="Calculate2SubsystemBase" /> class.
    ''' </summary>
    ''' <remarks> David, 7/6/2016. </remarks>
    ''' <param name="compensationType"> Type of the compensation. </param>
    ''' <param name="channelNumber">    A reference to a <see cref="VI.StatusSubsystemBase">status
    '''                                 subsystem</see>. </param>
    ''' <param name="statusSubsystem">  The status subsystem. </param>
    Protected Sub New(ByVal compensationType As CompensationTypes, ByVal channelNumber As Integer, ByVal statusSubsystem As VI.StatusSubsystemBase)
        MyBase.New(compensationType, channelNumber, statusSubsystem)
    End Sub

#End Region

End Class
