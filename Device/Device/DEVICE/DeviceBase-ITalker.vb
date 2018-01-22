﻿Imports System.ComponentModel
Imports isr.Core.Pith
Imports isr.Core.Pith.ExceptionExtensions
Partial Public Class DeviceBase
    Implements ITalker

    ''' <summary> Constructor-Safe talker setter. </summary>
    ''' <param name="talker"> The talker. </param>
    Private Sub ConstructorSafeTalkerSetter(ByVal talker As ITraceMessageTalker)
        Me._Talker = talker
        If Me._Talker IsNot Nothing Then
            AddHandler Me._Talker.DateChanged, AddressOf Me.HandleTalkerDateChange
        End If
        Me.Subsystems.AssignTalker(talker)
    End Sub

    Private _Talker As ITraceMessageTalker
    ''' <summary> Gets the trace message talker. </summary>
    ''' <value> The trace message talker. </value>
    Public Property Talker As ITraceMessageTalker
        Get
            Return Me._Talker
        End Get
        Private Set(value As ITraceMessageTalker)
            If Me._Talker IsNot Nothing Then
                RemoveHandler Me._Talker.DateChanged, AddressOf Me.HandleTalkerDateChange
                Me.RemoveListeners()
            End If
            Me.ConstructorSafeTalkerSetter(value)
        End Set
    End Property

    Private IsAssignedTalker As Boolean
    ''' <summary> Assigns a talker. </summary>
    ''' <param name="talker"> The talker. </param>
    Public Overridable Sub AssignTalker(ByVal talker As ITraceMessageTalker)
        Me.IsAssignedTalker = talker IsNot Nothing
        Me.Talker = talker
        If talker IsNot Nothing Then Me.IdentifyTalkers()
    End Sub

    ''' <summary> Handles the talker date change. </summary>

    ''' <param name="log">          The log. </param>
    ''' <param name="assemblyInfo"> Information describing my assembly. </param>
    Protected Overridable Sub HandleTalkerDateChange(ByVal log As MyLog, ByVal assemblyInfo As MyAssemblyInfo)
        If log Is Nothing Then Throw New ArgumentNullException(NameOf(log))
        If assemblyInfo Is Nothing Then Throw New ArgumentNullException(NameOf(assemblyInfo))
        Me.Talker.PublishOverride(TraceEventType.Information, My.MyLibrary.TraceEventId, $"Log at;. {log.FullLogFileName}")
        Me.Talker.PublishOverride(TraceEventType.Information, My.MyLibrary.TraceEventId, $"Local time {assemblyInfo.LocalUtcCaption}")
    End Sub

    ''' <summary> Handles the talker date change. </summary>
    Protected Overridable Sub HandleTalkerDateChange()
        Me.IdentifyTalkers()
    End Sub

    ''' <summary> Identifies talkers. </summary>
    Protected Overridable Sub IdentifyTalkers()
        My.MyLibrary.Identify(Me.Talker)
    End Sub

    ''' <summary> Handles the talker date change. </summary>
    ''' <param name="sender"> Source of the event. </param>
    ''' <param name="e">      Event information. </param>
    <CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
    Private Sub HandleTalkerDateChange(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Me.HandleTalkerDateChange()
        Catch ex As Exception
            Debug.Assert(Not Debugger.IsAttached, ex.ToFullBlownString)
        End Try
    End Sub

    ''' <summary> Clears the listeners. </summary>
    Public Overridable Sub RemoveListeners() Implements ITalker.RemoveListeners
        If Not Me.IsAssignedTalker Then Me.Talker.Listeners?.Clear()
        Me.Subsystems.RemoveListeners()
    End Sub

    ''' <param name="listener"> The listener. </param>
    Public Overridable Sub AddListener(ByVal listener As IMessageListener) Implements ITalker.AddListener
        Me.Subsystems?.AddListener(listener)
        Me.Talker.AddListener(listener)
        Me.IdentifyTalkers()
    End Sub

    ''' <summary> Adds the listeners. </summary>
    ''' <param name="listeners"> The listeners. </param>
    Public Overridable Sub AddListeners(ByVal listeners As IEnumerable(Of IMessageListener)) Implements ITalker.AddListeners
        If listeners Is Nothing Then Throw New ArgumentNullException(NameOf(listeners))
        For Each listener As IMessageListener In listeners
            Me.AddListener(listener)
        Next
    End Sub

    ''' <summary> Adds the listeners. </summary>
    ''' <param name="talker"> The talker. </param>
    Public Overridable Sub AddListeners(ByVal talker As ITraceMessageTalker) Implements ITalker.AddListeners
        If talker Is Nothing Then Throw New ArgumentNullException(NameOf(talker))
        Me.AddListeners(talker.Listeners)
    End Sub

    ''' <summary> Applies the trace level to all listeners to the specified type. </summary>
    ''' <param name="listenerType"> Type of the listener. </param>
    ''' <param name="value">        The value. </param>
    Public Overridable Sub ApplyListenerTraceLevel(ByVal listenerType As ListenerType, ByVal value As TraceEventType) Implements ITalker.ApplyListenerTraceLevel
        ' this should apply only to the listeners associated with this form
        ' Not this: Me.Talker.ApplyListenerTraceLevel(listenerType, value)
        Me.Talker.ApplyListenerTraceLevel(listenerType, value)
        Me.Subsystems?.ApplyListenerTraceLevel(listenerType, value)
        Me.IdentifyTalkers()
    End Sub

    ''' <summary> Applies the trace level type to all talkers. </summary>
    ''' <param name="listenerType"> Type of the trace level. </param>
    ''' <param name="value">        The value. </param>
    Public Overridable Sub ApplyTalkerTraceLevel(ByVal listenerType As ListenerType, ByVal value As TraceEventType) Implements ITalker.ApplyTalkerTraceLevel
        Me.Talker.ApplyTalkerTraceLevel(listenerType, value)
        Me.Subsystems?.ApplyTalkerTraceLevel(listenerType, value)
    End Sub

    ''' <summary> Applies the talker trace levels described by talker. </summary>
    ''' <param name="talker"> The talker. </param>
    Public Overridable Sub ApplyTalkerTraceLevels(ByVal talker As ITraceMessageTalker) Implements ITalker.ApplyTalkerTraceLevels
        Me.Talker.ApplyTalkerTraceLevels(talker)
        Me.Subsystems?.ApplyTalkerTraceLevels(talker)
    End Sub

    ''' <summary> Applies the talker listeners trace levels described by talker. </summary>
    ''' <param name="talker"> The talker. </param>
    Public Overridable Sub ApplyListenerTraceLevels(ByVal talker As ITraceMessageTalker) Implements ITalker.ApplyListenerTraceLevels
        Me.Talker.ApplyListenerTraceLevels(talker)
        Me.Subsystems?.ApplyListenerTraceLevels(talker)
    End Sub

End Class

#Region " UNUSED "
#If False Then
    ''' <summary> Updates the trace log and show level described by the talker. </summary>
    ''' <param name="talker"> The talker. </param>
    Public Overridable Sub UpdateTraceLevels(ByVal talker As ITraceMessageTalker) Implements ITalker.UpdateTraceLevels
        If talker Is Nothing Then Throw New ArgumentNullException(NameOf(talker))
        Me.UpdateTraceLogLevel(talker.TraceLogLevel)
        Me.UpdateTraceShowLevel(talker.TraceShowLevel)
    End Sub

    ''' <summary> Updates the trace log level described by traceLevel. </summary>
    ''' <param name="traceLevel"> The trace level. </param>
    Public Overridable Sub UpdateTraceLogLevel(ByVal traceLevel As TraceEventType) Implements ITalker.UpdateTraceLogLevel
        Me.Talker.UpdateTraceLogLevel(traceLevel)
    End Sub

    ''' <summary> Updates the trace show level described by traceLevel. </summary>
    ''' <param name="traceLevel"> The trace level. </param>
    Public Overridable Sub UpdateTraceShowLevel(ByVal traceLevel As TraceEventType) Implements ITalker.UpdateTraceShowLevel
        Me.Talker.UpdateTraceShowLevel(traceLevel)
    End Sub
#End If
#End Region
