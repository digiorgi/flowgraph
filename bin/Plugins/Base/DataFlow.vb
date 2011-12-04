#Region "License & Contact"
'License:
'   Copyright (c) 2010 Raymond Ellis
'   
'   This software is provided 'as-is', without any express or implied
'   warranty. In no event will the authors be held liable for any damages
'   arising from the use of this software.
'
'   Permission is granted to anyone to use this software for any purpose,
'   including commercial applications, and to alter it and redistribute it
'   freely, subject to the following restrictions:
'
'       1. The origin of this software must not be misrepresented; you must not
'           claim that you wrote the original software. If you use this software
'           in a product, an acknowledgment in the product documentation would be
'           appreciated but is not required.
'
'       2. Altered source versions must be plainly marked as such, and must not be
'           misrepresented as being the original software.
'
'       3. This notice may not be removed or altered from any source
'           distribution.
'
'
'Contact:
'   Raymond Ellis
'   Email: RaymondEllis*live.com
#End Region

Public Class DataFlowBase

    'Base object and the output/input index.
    Public obj As Integer = -1
    Public Index As Integer = -1
    Public Name As String = "DataFlowBase"
    Public Note As String = ""

    Public DataType As New List(Of String)

    'We do not create new because Inputs do not use it.
    Public Flow As List(Of DataFlow) 'We define if it is a input/output by wether flow is nothing or not.

    'How meny can connect?
    Public MaxConnected As Integer = -1
    Private bConnected As Integer = 0 'Holds the number of connections. (Only used for inputs. We use flow.count for outputs.)
    Public Property Connected() As Integer
        Get
            If Flow Is Nothing Then
                Return bConnected
            Else
                Return Flow.Count
            End If
        End Get
        Set(ByVal value As Integer)
            If Flow Is Nothing Then
                bConnected = value
            Else
                Throw New Exception("Can not set output.Connected!")
            End If
        End Set
    End Property


    Public Sub New(ByVal obj As Integer, ByVal Index As Integer, ByVal Name As String, Optional ByVal IsOutput As Boolean = False)
        Me.obj = obj
        Me.Index = Index

        'We need to get the types(if any) from the name.
        'Frist we split name.
        Dim Types As String() = Split(Name, ",")
        If Types.Length = 1 Then 'If we did not find anything after spliting.
            Me.Name = Name 'Then we just set the name.
        Else
            'Other wise we have to set the name to the first type.
            Me.Name = Types(0)
            If IsOutput Then
                DataType.Add(Types(1))
                If Types.Length > 2 Then
                    Objects(obj).Log("Output #" & Index & "(" & Me.Name & ") can only have One type!")
                End If
            Else
                'Then we loop thrugh all of the types and add them to the list. (Starting at one because zero is the name.)
                For n As Integer = 1 To Types.Length - 1
                    DataType.Add(Types(n))
                Next
            End If

        End If

        'If it's a output then we create flow.
        If IsOutput Then Flow = New List(Of DataFlow)
    End Sub

    Public Overrides Function ToString() As String
        'Let me know if anything here needs comments.
        Dim str As String = "Name: " & Name & vbNewLine & "Types"

        If DataType.Count = 0 Then
            str &= " undefined."
        Else
            For Each Type As String In DataType
                str &= " : " & Type
            Next
        End If

        Return str & vbNewLine & Note
    End Function

#Region "Connect & Disconnect"
    ''' <summary>
    ''' Connect a input to this output.
    ''' </summary>
    ''' <param name="obj1"></param>
    ''' <param name="Index1"></param>
    ''' <returns>True if successfully added.</returns>
    Public Function TryConnect(ByVal obj1 As Integer, ByVal Index1 As Integer) As Boolean
        If Flow Is Nothing Then Return False 'Return false if its a input.

        'Return false if we are already at the max connections.
        If Connected = MaxConnected Then Return False

        'Are we trying to connect to the same object.
        If obj1 = obj Then Return False

        'Make sure the object we are connecting to is a input.
        If Objects(obj1).Input Is Nothing Then Return False
        If Objects(obj1).Input.Length - 1 < Index1 Or Index1 < 0 Then Return False

        'Make sure the object we are connecting to doesnot already have it's max connections.
        If Objects(obj1).input(Index1).MaxConnected > -1 Then
            If Objects(obj1).Input(Index1).Connected >= Objects(obj1).Input(Index1).MaxConnected Then Return False
        End If


        'Make sure the object can connect to that type.
        Dim FoundType As Boolean = False
        If DataType.Count > 0 And Objects(obj1).Input(Index1).DataType.Count > 0 Then
            For Each Type As String In DataType
                For Each objType As String In Objects(obj1).Input(Index1).DataType
                    If LCase(objType) = LCase(Type) Then
                        FoundType = True
                        Exit For
                    End If
                Next
                If FoundType Then Exit For
            Next
        ElseIf Objects(obj1).Input(Index1).DataType.Count > 0 Then
            Return False
        Else 'Both are undefined.
            FoundType = True
        End If
        If Not FoundType Then Return False


        'Look through flow, and check to see if there is already one going to the same place. this one wants to go.
        'Note: they can connect to the same object just not the same place on the object.
        For Each df As DataFlow In Flow
            If df.obj = obj1 And df.Index = Index1 Then
                Return False 'If there is we return false.
            End If
        Next


        'Add the new data flow.
        Flow.Add(New DataFlow(obj1, Index1, Me))

        Return True 'We successfully added the input to the output.
    End Function

    ''' <summary>
    ''' Disconnect all connections.
    ''' </summary>
    Public Sub Disconnect()
        If Connected = 0 Then Return

        If Flow Is Nothing Then
            'Disconnect input.

            'Disconnecting a input is harder. Because we do not hold all of the connections.
            'So we have to go through all of the objects, and find the ones connected to it and disconnect them.

            For Each objectT As Object In Objects 'Loop through each object.
                If objectT.Output IsNot Nothing Then 'Make sure there is some outputs.
                    For Each out As DataFlowBase In objectT.Output 'Go through all of the outputs in the object.

                        'This next part could look messy. 
                        'But as far as I know there is no better way do to this.
                        'It is really pretty simple. All it is doing, Is going through each object. One by one.
                        'When it finds one to remove, it removes it.  
                        'But scense it removed a object all the objects past that have fallen down.  (was [a,b,c,d} now {a,b,d})
                        'So we just stay at the same index we removed the object at and go from there.

                        Dim n As Integer = 0 'Even though n is declared here I still set it to 0. Because sometime it seems not to reset it.
                        Do While n < out.Flow.Count
                            If out.Flow(n).obj = obj And out.Flow(n).Index = Index Then
                                'Remove the connection.
                                out.Flow(n) = Nothing
                                out.Flow.RemoveAt(n)
                                Connected -= 1
                            Else
                                n += 1
                            End If
                        Loop


                    Next
                End If
            Next


        Else 'Disconnect output.

            'Really simple.
            'First we go through each output and subtract one from the input the output is outputing to.
            'Then we just clear the list.

            For Each fd As DataFlow In Flow
                'Subtract one from the inputs connitions.
                Objects(fd.obj).Input(fd.Index).Connected -= 1

            Next

            'Clear the data flow list.
            Flow.Clear()
        End If


    End Sub
#End Region

#Region "Send"
    Public Function Send(ByVal Data As Object, ByVal subIndex As Integer) As Boolean
        'If flow is nothing then it's a input so we can't send.
        If Flow Is Nothing Then Return False

        'Make sure subIndex is with-in the bounds.
        If subIndex >= Flow.Count Or subIndex < 0 Then Return False

        'Because of the way sending is done. We call the Recive sub on the object we are sending to.
        Objects(Flow(subIndex).obj).Receive(Data, Flow(subIndex))

        Return True
    End Function
    Public Function Send(ByVal Data As Object) As Boolean
        If Flow Is Nothing Then Return False

        'Send to each object in flow.
        For Each fd As DataFlow In Flow
            Objects(fd.obj).Receive(Data, fd)
        Next

        Return True
    End Function
#End Region

#Region "Load & Save"
    Public Sub Load(ByVal data() As String)

        If Flow Is Nothing Then
            Connected = data(0)
        Else
            Dim Dummy As Integer = 0
            For i As Integer = 1 To data.Length - 1 Step 2
                If Not TryConnect(data(i), data(i + 1)) Then
                    If Objects(data(i)).GetType Is GetType(ObjectDummy) Then
                        Dummy += 1
                    End If
                End If
            Next
            'Make sure everything connected. (don't cound dummy objects.)
            If Connected + Dummy <> data(0) Then
                Log("Connections do not match!" & Environment.NewLine & "Name=" & Name & " ObjectTitle=" & Objects(obj).Title, LogPriority.Medium)
            End If

        End If
    End Sub
    Public Function Save() As String
        Dim data As String = Connected
        'Input doesn't have anything more then connected. so just return that.
        If Flow Is Nothing Then Return data

        For i As Integer = 0 To Flow.Count - 1
            data &= "," & Flow(i).obj & "," & Flow(i).Index
        Next
        Return data
    End Function
#End Region

#Region "IsEmpty & ="
    ''' <summary>
    ''' Returns true if nothing connected.
    ''' </summary>
    ''' <returns></returns>
    Public Function IsEmpty() As Boolean
        If Connected = 0 Then Return True
        Return False
    End Function
    ''' <summary>
    ''' Not IsEmpty
    ''' </summary>
    Public Function IsNotEmpty() As Boolean
        Return Not IsEmpty()
    End Function

    Shared Operator =(ByVal left As DataFlowBase, ByVal right As DataFlowBase) As Boolean
        If left Is Nothing Or right Is Nothing Then Return False

        'Check obj index and connected.
        If right.obj <> left.obj Or right.Index <> left.Index Or right.Connected <> left.Connected Then Return False

        Return True
    End Operator
    Shared Operator <>(ByVal left As DataFlowBase, ByVal right As DataFlowBase) As Boolean
        Return Not left = right
    End Operator
#End Region
End Class

Public Class DataFlow

    Public obj, Index As Integer

    Public Base As DataFlowBase

    Public Sub New(ByVal obj As Integer, ByVal Index As Integer, ByVal Base As DataFlowBase)
        Me.obj = obj
        Me.Index = Index
        Me.Base = Base
    End Sub

    Shared Operator =(ByVal left As DataFlow, ByVal right As DataFlow) As Boolean
        If right.obj <> left.obj Or right.Index <> left.Index Then Return False
        Return True
    End Operator
    Shared Operator <>(ByVal left As DataFlow, ByVal right As DataFlow) As Boolean
        Return Not left = right
    End Operator
End Class