﻿Namespace My

    ''' <summary> Provides assembly information for the class library. </summary>
    ''' <remarks> David, 11/26/2015. </remarks>
    Public NotInheritable Class MyLibrary

        ''' <summary> Constructor that prevents a default instance of this class from being created. </summary>
        Private Sub New()
            MyBase.New()
        End Sub

        ''' <summary> Gets the identifier of the trace source. </summary>
        Public Const TraceEventId As Integer = VI.My.ProjectTraceEventId.Instrument

        Public Const AssemblyTitle As String = "VI Instrument Library"
        Public Const AssemblyDescription As String = "Instrument Virtual Instrument Library"
        Public Const AssemblyProduct As String = "VI.Instrument.2016"

        ''' <summary> Identifies this talker. </summary>
        ''' <remarks> David, 1/21/2016. </remarks>
        ''' <param name="talker"> The talker. </param>
        Public Shared Sub Identify(ByVal talker As isr.Core.Pith.ITraceMessageTalker)
            talker?.Publish(TraceEventType.Information, MyLibrary.TraceEventId, $"{MyLibrary.AssemblyProduct} ID = {MyLibrary.TraceEventId:X}")
            isr.VI.My.MyLibrary.Identify(talker)
        End Sub

    End Class

End Namespace

