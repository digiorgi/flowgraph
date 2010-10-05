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
'   Email: RaymondEllis@live.com
#End Region

Public MustInherit Class BaseObject
    Public Index As Integer = -1

    Private Name As String = "NoName"

    'Output
    Public Output() As DataFlowBase

    'Input
    Public Input() As DataFlowBase


    Public Rect As Rectangle

    Public Title As String = "Title not set"
    Private TitleRect As RectangleF
    Public TitleBar As Rectangle

    Private BackGround As Rectangle


#Region "Setup & Distroy"
    ''' <summary>
    ''' Create rectangles. using the position and size.
    ''' </summary>
    Protected Sub Setup(ByVal ClassName As String, ByVal Position As Point, ByVal Width As Integer, ByVal Height As Integer, Optional ByVal MenuWidth As Integer = 50)
        Name = ClassName
        'Create the main rectangle.
        Rect = New Rectangle(Position, New Size(Width, Height))

        'Set the size of the title.  Used to drag the object around.
        TitleBar = New Rectangle(Rect.Location, New Size(Rect.Width, 15))

        BackGround = New Rectangle(Rect.X, Rect.Y + 15, Rect.Width, Rect.Height - 15)

        Index = Objects.Count


        Menu.Add(New MenuNode("Remove", False, MenuWidth))
    End Sub

    Public Overridable Sub Distroy()
        If Output IsNot Nothing Then
            For n As Integer = 0 To Output.Length - 1

                Output(n).Disconnect()

                'If Output(n).obj1 > -1 Then
                '    Objects(Output(n).obj1).Input(Output(n).Index1).obj1 = -1
                '    Objects(Output(n).obj1).Input(Output(n).Index1).index1 = -1
                '    Objects(Output(n).obj1).Input(Output(n).Index1).Connected -= 1
                '    Output(n).obj1 = -1
                '    Output(n).Index1 = -1
                'End If

            Next
        End If

        If Input IsNot Nothing Then
            For n As Integer = 0 To Input.Length - 1
                Input(n).Disconnect()
                'If Input(n).Connected > 0 Then
                '    DisconnectInput(Input(n))
                'End If
            Next
        End If
    End Sub
#End Region

#Region "Load & Save"
    Public Overridable Function Load(ByVal g As SimpleD.Group) As SimpleD.Group

        Dim tmp As String = ""


        If Output IsNot Nothing Then 'If there is output then save Output=(obj1),(index1),(obj1),etc.. for each output
            g.Get_Value("Output", tmp)
            Dim tmpS As String() = Split(tmp, "`")
            For n As Integer = 0 To Output.Length - 1
                'Output(n).SetValues(tmpS(n * 2), tmpS((n * 2) + 1))
                Output(n).Load(Split(tmpS(n), ","))
            Next
        End If

        If Input IsNot Nothing Then 'Same as output^^^ but for inputs...
            g.Get_Value("Input", tmp)
            Dim tmpS As String() = Split(tmp, ",")
            For n As Integer = 0 To Input.Length - 1
                Input(n).Connected = tmpS(n)
            Next
        End If

        Return g
    End Function

    Public Overridable Function Save() As SimpleD.Group
        Dim g As New SimpleD.Group("Object" & Index)
        Dim tmp As String = ""

        g.Add("Name", Name)
        g.Add("Position", Rect.X & "," & Rect.Y)

        If Output IsNot Nothing Then 'If there is output then save Output=(obj1),(index1),(obj1),etc.. for each output
            tmp = Output(0).Save
            For n As Integer = 1 To Output.Length - 1
                tmp &= "`" & Output(n).Save
            Next
            g.Add("Output", tmp)
        End If

        If Input IsNot Nothing Then 'Same as output^^^ but for inputs...
            tmp = Input(0).Connected
            For n As Integer = 1 To Input.Length - 1
                tmp &= "," & Input(n).Connected
            Next
            g.Add("Input", tmp)
        End If

        Return g
    End Function

#End Region

#Region "Draw"

    Public Overridable Sub Draw(ByVal g As Graphics)
        'Draw the title and the background. Then we draw teh border so it is on top.
        g.FillRectangle(SystemBrushes.GradientActiveCaption, TitleBar)

        g.FillRectangle(SystemBrushes.Control, BackGround)
        g.DrawRectangle(SystemPens.WindowFrame, Rect)

        'Draw the inputs. (if any.)
        If Input IsNot Nothing Then
            For n As Integer = 1 To Input.Length
                'g.FillRectangle(Brushes.Red, Rect.X + 1, Rect.Y + 16 * n, 15, 15)
                g.FillEllipse(Brushes.Red, Rect.X + 1, Rect.Y + 15 * n, 15, 15)
            Next
        End If
        'Draw the outputs. (if any.)
        If Output IsNot Nothing Then
            For n As Integer = 1 To Output.Length
                'g.FillRectangle(Brushes.Green, Rect.Right - 15, Rect.Y + 16 * n, 15, 15)
                g.FillEllipse(Brushes.Green, Rect.Right - 15, Rect.Y + 15 * n, 15, 15)
            Next
        End If



        'If title rect is empty then we will set the position and size of the title string.
        If TitleRect.IsEmpty Then
            TitleRect.Size = g.MeasureString(Title, SystemFonts.DefaultFont)
            TitleRect.Location = New PointF(Rect.X + Rect.Width * 0.5 - TitleRect.Width * 0.5, Rect.Y + 1)
        End If
        g.DrawString(Title, SystemFonts.DefaultFont, SystemBrushes.ActiveCaptionText, TitleRect) 'Draw the title string.




    End Sub

    ''' <summary>
    ''' Draw lines connecting outputs to inputs.
    ''' </summary>
    Public Sub DrawConnectors(ByVal g As Graphics)
        If Output Is Nothing Then Return


        For n As Integer = 0 To Output.Length - 1
            If Output(n).IsNotEmpty Then
                For Each fd As DataFlow In Output(n).Flow
                    g.DrawLine(ConnectorPen, GetOutputPosition(n), Objects(fd.obj).GetInputPosition(fd.Index))
                Next
                'g.DrawLine(ConnectorPen, GetOutputPosition(n), Objects(Output(n).obj1).GetInputPosition(Output(n).Index1))

            End If
        Next
    End Sub

#End Region


    ''' <summary>
    ''' Is called when the object is moving.
    ''' </summary>
    Public Overridable Sub Moved()


    End Sub



    Public Sub SetPosition(ByVal x As Integer, ByVal y As Integer)
        Rect.Location = New Point(Math.Round(x / GridSize) * GridSize, Math.Round(y / GridSize) * GridSize)

        'Update the title position.
        TitleRect.Location = New PointF(Rect.X + Rect.Width * 0.5 - TitleRect.Width * 0.5, Rect.Y + 1)
        TitleBar.Location = Rect.Location
        BackGround.Location = New Point(Rect.X, Rect.Y + 15)

        Moved()
    End Sub

#Region "Send & Receive"
    Public Sub Send(ByVal Data As Object, ByVal ID As Integer)
        If Output Is Nothing Then Return

        Output(ID).Send(Data)
    End Sub
    Public Sub Send(ByVal Data As Object)
        If Output Is Nothing Then Return

        For Each obj As DataFlowBase In Output
            obj.Send(Data)
        Next
    End Sub

    Public Overridable Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
    End Sub
#End Region

#Region "Inputs & Outputs"

    Protected Sub Inputs(ByVal Names As String())
        'InputNames = Names
        ReDim Input(Names.Length - 1)
        For n As Integer = 0 To Names.Length - 1
            Input(n) = New DataFlowBase(Index, n, Names(n))

        Next
    End Sub
    Protected Sub Outputs(ByVal Names As String())
        ReDim Output(Names.Length - 1)
        For n As Integer = 0 To Names.Length - 1
            Output(n) = New DataFlowBase(Index, n, Names(n), True)
        Next
    End Sub

    Public Intersection As Integer
    Public Function IntersectsWithInput(ByVal rect As Rectangle) As Boolean
        If Input Is Nothing Then Return False

        For n As Integer = 1 To Input.Length
            Dim r As New Rectangle(Me.Rect.X, Me.Rect.Y + 16 * n, 16, 15)
            If rect.IntersectsWith(r) Then
                Intersection = n - 1
                Return True
            End If
        Next

        Return False
    End Function
    Public Function IntersectsWithOutput(ByVal rect As Rectangle) As Boolean
        If Output Is Nothing Then Return False

        For n As Integer = 1 To Output.Length
            Dim r As New Rectangle(Me.Rect.Right - 15, Me.Rect.Y + 16 * n, 16, 15)
            If rect.IntersectsWith(r) Then
                Intersection = n - 1
                Return True
            End If
        Next

        Return False
    End Function

    Public Function GetInputPosition(ByVal ID As Integer) As PointF
        Return New PointF(Rect.X + 7.5, Rect.Y + (15 * (ID + 1)) + 7.5)
    End Function
    Public Function GetOutputPosition(ByVal ID As Integer) As PointF
        Return New PointF(Rect.Right - 7.5, Rect.Y + (15 * (ID + 1)) + 7.5)
    End Function

#End Region


#Region "Mouse & Menu"
    Friend Menu As New List(Of MenuNode)

    Public Overridable Sub MenuSelected(ByVal Result As MenuNode)


        Select Case LCase(Result.Name)
            Case "remove"
                RemoveAt(Index)

        End Select
    End Sub

    Public Overridable Sub MouseDoubleClick(ByVal e As MouseEventArgs)
    End Sub
    Public Overridable Sub MouseMove(ByVal e As MouseEventArgs)

    End Sub
    Public Overridable Sub MouseDown(ByVal e As MouseEventArgs)

    End Sub
    Public Overridable Sub MouseUp(ByVal e As MouseEventArgs)
        If e.Button = MouseButtons.Right Then

            Menu_Open(Index, Menu)

        End If
    End Sub

#End Region


    Public Overrides Function ToString() As String
        Return Title
    End Function

End Class


Public Class DataFlowBase

    'Base object and the output/input index.
    Public obj As Integer = -1
    Public Index As Integer = -1
    Public Name As String = "DataFlowBase"

    Public DataType As New List(Of String)

    'We do not create new because Inputs do not use it.
    Public Flow As List(Of DataFlow) 'We define if it is a input/output by wether flow is nothing or not.

    'How meny can connect?
    Public MaxConnected As Integer = -1
    Private bConnected As Integer = 0 'Holds the number of connections. Only used for inputs.
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

        Dim Types As String() = Split(Name, "|")
        If Types.Length = 0 Then
            Me.Name = Name
        Else
            Me.Name = Types(0)
            For n As Integer = 1 To Types.Length - 1
                DataType.Add(Types(n))
            Next
        End If

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

        Return str
    End Function

#Region "Add & Disconnect"
    ''' <summary>
    ''' Add a input.
    ''' </summary>
    ''' <param name="obj1"></param>
    ''' <param name="Index1"></param>
    ''' <returns>True if successfully added.</returns>
    Public Function Add(ByVal obj1 As Integer, ByVal Index1 As Integer) As Boolean
        If Flow Is Nothing Then Return False 'Return false if its a input.

        'Return false if we are already at the max connections.
        If Connected = MaxConnected Then Return False

        'Are we trying to connect to the same object.
        If obj1 = obj Then Return False

        'Make sure the object we are connecting to is a input.
        If Objects(obj1).Input Is Nothing Then Return False

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
        Else
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
            'So we have to go through all of the objects. and find the ones connecting to it and disconnect them.

            For Each obj As Object In Objects 'Loop through each object.
                If obj.Output IsNot Nothing Then 'Make sure there is some outputs.
                    For Each out As DataFlowBase In obj.Output 'Go through all of the outputs in the object.

                        'This next part could look messy. 
                        'But as far as I know there is no better way do to this.
                        'It is really pretty simple. All it is doing, Is going through each object. One by one.
                        'When it finds one to remove, it removes it.  
                        'But scense it removed a object all the objects past that have fallen down.  (was [a,b,c,d} now {a,b,d})
                        'So we just stay at the same index we removed the object at and go from there.

                        Dim n As Integer = 0 'Even though n is declared here I still set it to 0. Because sometime it seems not to reset it.
                        Do
                            If n > out.Flow.Count - 1 Then Exit Do
                            If out.Flow(n).obj = Me.obj And out.Flow(n).Index = Index Then
                                'Remove the object.
                                out.Flow(n) = Nothing
                                out.Flow.RemoveAt(n)
                                Connected -= 1
                            Else
                                n += 1
                            End If
                        Loop Until n = out.Flow.Count


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
        If Flow Is Nothing Then Return False

        If Flow.Count > subIndex Then Return False

        Objects(Flow(subIndex).obj).Receive(Data, Flow(subIndex))

        Return True
    End Function
    Public Function Send(ByVal Data As Object) As Boolean
        If Flow Is Nothing Then Return False

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

            For i As Integer = 1 To data.Length - 1 Step 2
                Add(data(i), data(i + 1))
            Next
            'Make sure everything connected.
            If Connected <> data(0) Then
                Throw New Exception("Connections do not match!")
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
    Public Function IsEmpty() As Boolean
        If Connected = 0 Then Return True

        Return False
    End Function
    Public Function IsNotEmpty() As Boolean
        Return Not IsEmpty()
    End Function

    Shared Operator =(ByVal left As DataFlowBase, ByVal right As DataFlowBase) As Boolean
        If left Is Nothing Or right Is Nothing Then Return False

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

    Public Sub AddToObj(ByVal value As Integer)
        obj += value
    End Sub

    Shared Operator =(ByVal left As DataFlow, ByVal right As DataFlow) As Boolean
        If right.obj <> left.obj Or right.Index <> left.Index Then Return False

        Return True
    End Operator
    Shared Operator <>(ByVal left As DataFlow, ByVal right As DataFlow) As Boolean
        Return Not left = right
    End Operator
End Class