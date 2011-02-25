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

'Include(Base\SimpleD.vb,Base\Menu.vb)

Public MustInherit Class BaseObject
    Public Index As Integer = -1

    Private Name As String = "NoName"
    Protected File As String = ""
    Protected CanNoDraw As Boolean = False

    'Output
    Public Output() As DataFlowBase

    'Input
    Public Input() As DataFlowBase

    'This rectangle is the size of the whole object. It's used for collision checking. And the position of the object..
    Public Rect As Rectangle

    Private _Title As String = "Title not set"
    Private TitleRect As RectangleF 'The title rectangle is the size of the title text.
    Public TitleBar As Rectangle 'The title bar is the size of the visual bar.

    'This rectangle is the size of the client drawing area. (plus one pixel on each side.)
    Private BackGround As Rectangle

    Public UserData As String = ""

    Private ClientRect As Rectangle

#Region "Setup & Dispose"

    ''' <summary>
    ''' Create rectangles. using the position and size.
    ''' This should AllWays be called before creating inputs/outputs.
    ''' </summary>
    ''' <param name="UserData">User data is not used by BaseObject, but it needs to be saved so the object will open right.</param>
    ''' <param name="StartPosition">NOT client position.</param>
    ''' <param name="Width">Client size</param>
    ''' <param name="Height">Client size</param>
    Protected Sub Setup(ByVal UserData As String, ByVal StartPosition As Point, ByVal Width As Integer, Optional ByVal Height As Integer = 15)
        Me.UserData = UserData
        Name = MyClass.GetType.FullName 'Needed for every object that is created with the add menu.
        Index = Objects.Count 'Needed for every object!

        StartPosition = SnapToGrid(StartPosition)
        Width = SnapToGrid(Width)
        Height = SnapToGrid(Height)

        ClientRect = New Rectangle(StartPosition + New Point(1, 16), New Size(Width - 2, Height - 2))

        Height += 15

        'Create the main rectangle.
        Rect = New Rectangle(StartPosition, New Size(Width, Height))

        'Set the size of the title.  Used to drag the object around.
        TitleBar = New Rectangle(Rect.Location, New Size(Rect.Width, 15))

        'The backgound size is rect minus the height of the title bar.
        BackGround = New Rectangle(Rect.X, Rect.Y + 15, Rect.Width, Rect.Height - 15)

        'Add remove to the object menu.
        MenuItems.Add(New Menu.Node("Remove", False))
    End Sub

    ''' <summary>
    ''' Distroys everything in the object.
    ''' BaseObject just dissconnects everything.
    ''' </summary>
    Public Overridable Sub Dispose()
        If Output IsNot Nothing Then
            For n As Integer = 0 To Output.Length - 1
                Output(n).Disconnect()
            Next
        End If

        If Input IsNot Nothing Then
            For n As Integer = 0 To Input.Length - 1
                Input(n).Disconnect()
            Next
        End If
    End Sub

    ''' <summary>
    ''' Gets or sets the title shown on the object and the object menu.
    ''' </summary>
    Public Property Title() As String
        Get
            Return _Title
        End Get
        Set(ByVal value As String)
            _Title = value
            MenuItems.Name = value
        End Set
    End Property

#End Region

#Region "Load & Save"
    ''' <summary>
    ''' Because UserData is loaded in New. We just connect everything here.
    ''' </summary>
    ''' <param name="g"></param>
    ''' <remarks></remarks>
    Public Overridable Sub Load(ByVal g As SimpleD.Group)

        Dim tmp As String = ""


        If Output IsNot Nothing Then 'If there is output then save Output=(obj1),(index1),(obj1),etc.. for each output
            g.Get_Value("Output", tmp)
            Dim tmpS As String() = Split(tmp, "`")
            For n As Integer = 0 To tmpS.Length - 1
                If n >= Output.Length Then Exit For
                Output(n).Load(Split(tmpS(n), ","))
            Next
        End If

        If Input IsNot Nothing Then 'Same as output^^^ but for inputs.
            g.Get_Value("Input", tmp)
            Dim tmpS As String() = Split(tmp, ",")
            For n As Integer = 0 To tmpS.Length - 1
                If n >= Input.Length Then Exit For
                Input(n).Connected = tmpS(n)
            Next
        End If


    End Sub

    ''' <summary>
    ''' Creates a new group and adds Name, Position, UserData, Outputs and inputs.
    ''' </summary>
    ''' <returns>The new group to be used.</returns>
    ''' <remarks></remarks>
    Public Overridable Function Save() As SimpleD.Group
        Dim g As New SimpleD.Group("Object" & Index)
        Dim tmp As String = ""

        g.Set_Value("Name", Name)
        g.Set_Value("File", File, "")
        g.Set_Value("CanNoDraw", CanNoDraw, False)
        g.Set_Value("Position", Rect.X & "," & Rect.Y)
        g.Set_Value("UserData", UserData)

        If Output IsNot Nothing Then 'If there is output then save Output=(obj1),(index1),(obj1),etc.. for each output
            tmp = Output(0).Save
            For n As Integer = 1 To Output.Length - 1
                tmp &= "`" & Output(n).Save
            Next
            g.Set_Value("Output", tmp)
        End If

        If Input IsNot Nothing Then 'Same as output^^^ but for inputs...
            tmp = Input(0).Connected
            For n As Integer = 1 To Input.Length - 1
                tmp &= "," & Input(n).Connected
            Next
            g.Set_Value("Input", tmp)
        End If

        Return g
    End Function

#End Region

#Region "Draw"

    ''' <summary>
    ''' Draws the base object stuff.
    ''' </summary>
    ''' <param name="g"></param>
    ''' <remarks></remarks>
    Public Overridable Sub Draw(ByVal g As Graphics)
        'Draw the title and the background. Then we draw the border so it is on top.
        g.FillRectangle(SystemBrushes.GradientActiveCaption, TitleBar)
        g.FillRectangle(SystemBrushes.Control, BackGround)
        g.DrawRectangle(SystemPens.WindowFrame, Rect)

        'Draw the inputs. (if any.)
        If Input IsNot Nothing Then
            For n As Integer = 1 To Input.Length
                'g.FillRectangle(Brushes.Purple, Rect.X + 1, Rect.Y + 15 * n, 15, 15)
                If InputImage IsNot Nothing Then
                    g.DrawImage(InputImage, Rect.X, Rect.Y + 15 * n)
                Else
                    g.FillEllipse(Brushes.Red, Rect.X, Rect.Y + 15 * n, 14, 14)
                End If

            Next
        End If
        'Draw the outputs. (if any.)
        If Output IsNot Nothing Then
            For n As Integer = 1 To Output.Length
                'g.FillRectangle(Brushes.Green, Rect.Right - 15, Rect.Y + 16 * n, 15, 15)
                If OutputImage IsNot Nothing Then
                    g.DrawImage(OutputImage, Rect.Right - 15, Rect.Y + 15 * n)
                Else
                    g.FillEllipse(Brushes.Green, Rect.Right - 15, Rect.Y + 15 * n, 14, 14)
                End If
            Next
        End If



        'If title rect is empty then we will set the position and size of the title string.
        If TitleRect.IsEmpty Then
            TitleRect.Size = g.MeasureString(Title, SystemFonts.DefaultFont)
            TitleRect.Location = New PointF(Rect.X + Rect.Width * 0.5 - TitleRect.Width * 0.5, Rect.Y + 1)
        End If
        g.DrawString(Title, SystemFonts.DefaultFont, SystemBrushes.ActiveCaptionText, TitleRect) 'Draw the title string.

        'Draws the client size.
        'g.DrawRectangle(Pens.Yellow, ClientRect)
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
            End If
        Next
    End Sub

#End Region

#Region "Set Position and Size"

    Friend ReadOnly Property Position As Point
        Get
            Return ClientRect.Location
        End Get
    End Property
    Friend ReadOnly Property Size As Size
        Get
            Return ClientRect.Size
        End Get
    End Property

    ''' <summary>
    ''' Set the size of the object.
    ''' Will call Resizing.
    ''' </summary>
    ''' <param name="Width"></param>
    ''' <param name="Height"></param>
    ''' <param name="IsClientSize"></param>
    Public Sub SetSize(ByVal Width As Integer, ByVal Height As Integer, Optional ByVal IsClientSize As Boolean = False)
        'Change from client size to normal size.
        If IsClientSize Then
            If Input IsNot Nothing Then Width += 15
            If Output IsNot Nothing Then Width += 15
            Height += 16
        End If

        'Snap to the grid.
        Width = SnapToGrid(Width)
        Height = SnapToGrid(Height)

        Rect.Size = SnapToGrid(New Size(Width, Height))

        BackGround.Size = SnapToGrid(New Size(Width, Height - 15))

        Dim inp, out As Short
        If Input IsNot Nothing Then inp = 14
        If Output IsNot Nothing Then out = 14
        ClientRect = New Rectangle(Rect.Location + New Point(inp + 1, 16), New Size((Width - 2) - inp - out, Height - 17))

        TitleBar.Width = Rect.Width
        TitleRect = Nothing
    End Sub

    ''' <summary>
    ''' Is called when the object is being resized.
    ''' </summary>
    ''' <remarks></remarks>
    Public Overridable Sub Resizing()
    End Sub

    ''' <summary>
    ''' Sets the position of the object.
    ''' Will call Moving.
    ''' </summary>
    Public Sub SetPosition(ByVal x As Integer, ByVal y As Integer, Optional ByVal IsClientPosition As Boolean = False)
        If IsClientPosition Then
            If Input IsNot Nothing Then x -= 15
            y -= 15
        End If

        x = SnapToGrid(x)
        y = SnapToGrid(y)

        'There is no point in doing anything if the new position is the same as the old.
        If x = Rect.X And y = Rect.Y Then Return

        'Update the positions of the rectangles.
        Rect.Location = New Point(x, y)
        TitleRect.Location = New PointF(Rect.X + Rect.Width * 0.5 - TitleRect.Width * 0.5, Rect.Y + 1)
        TitleBar.Location = Rect.Location
        BackGround.Location = New Point(Rect.X, Rect.Y + 15)

        Dim inp As Short
        If Input IsNot Nothing Then inp = 14
        ClientRect.Location = Rect.Location + New Point(inp + 1, 16)

        'Tell everyone that wants to know that, we are moving!
        Moving()

        DoDraw(True)
    End Sub

    ''' <summary>
    ''' Is called when the object is moving.
    ''' BaseObject does not use moving.
    ''' </summary>
    Public Overridable Sub Moving()
    End Sub


#End Region

#Region "Send & Receive"
    Public Sub Send(ByVal Data As Object, ByVal ID As Integer)
        If Output Is Nothing Then Throw New Exception("There is no outputs!")
        If ID >= Output.Length Or ID < 0 Then Throw New Exception("ID is not inside the output bounds!")

        Output(ID).Send(Data)
    End Sub
    Public Sub Send(ByVal Data As Object)
        If Output Is Nothing Then Throw New Exception("There is no outputs!")

        For Each obj As DataFlowBase In Output
            obj.Send(Data)
        Next
    End Sub

    ''' <summary>
    ''' Is called when a object sends to this object.
    ''' Is Not used by BaseObject.
    ''' </summary>
    ''' <param name="Data"></param>
    ''' <param name="sender"></param>
    ''' <remarks></remarks>
    Public Overridable Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
    End Sub
#End Region

#Region "Inputs & Outputs"

    ''' <summary>
    ''' Create inputs.
    ''' </summary>
    ''' <param name="Names">array of strings. e.g. {"NameOfInput,Type1,Type2", "Input2"}</param>
    ''' <remarks></remarks>
    Protected Sub Inputs(ByVal Names As String())
        ReDim Input(Names.Length - 1)
        For n As Integer = 0 To Names.Length - 1
            Input(n) = New DataFlowBase(Index, n, Names(n))
        Next


        Dim tmpHeight As Integer = Rect.Height
        'Set the height if the current height is smaller.
        If Rect.Height < 15 + (15 * Input.Length) Then
            tmpHeight = 15 + (15 * Input.Length)
        End If
        SetSize(Rect.Width + 15, tmpHeight)
    End Sub

    ''' <summary>
    ''' Create outputs.
    ''' </summary>
    ''' <param name="Names">array of strings. e.g. {"NameOfOutput,Type1,Type2", "Output2"}</param>
    Protected Sub Outputs(ByVal Names As String())
        ReDim Output(Names.Length - 1)
        For n As Integer = 0 To Names.Length - 1
            Output(n) = New DataFlowBase(Index, n, Names(n), True)
        Next

        Dim tmpHeight As Integer = Rect.Height
        'Set the height if the current height is smaller.
        If Rect.Height < 15 + (15 * Output.Length) Then
            tmpHeight = 15 + (15 * Output.Length)
        End If
        SetSize(Rect.Width + 15, tmpHeight)
    End Sub

    Public Intersection As Integer
    Public Function IntersectsWithInput(ByVal rect As Rectangle) As Boolean
        If Input Is Nothing Then Return False

        For n As Integer = 1 To Input.Length
            Dim r As New Rectangle(Me.Rect.X, Me.Rect.Y + 15 * n, 15, 15)
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
            Dim r As New Rectangle(Me.Rect.Right - 15, Me.Rect.Y + 15 * n, 15, 15)
            If rect.IntersectsWith(r) Then
                Intersection = n - 1
                Return True
            End If
        Next

        Return False
    End Function

    Public Function GetInputPosition(ByVal ID As Integer) As PointF
        Return New PointF(Rect.X + 3, Rect.Y + (15 * (ID + 1)) + 7.5)
    End Function
    Public Function GetOutputPosition(ByVal ID As Integer) As PointF
        Return New PointF(Rect.Right - 3, Rect.Y + (15 * (ID + 1)) + 7.5)
    End Function

#End Region

#Region "Mouse & Menu"
    Friend MenuItems As New Menu.Node("", True)

    ''' <summary>
    ''' Is called when somthing in the menu was selected.
    ''' BaseObject just checks if remove was selected.
    ''' </summary>
    ''' <param name="Result"></param>
    ''' <remarks></remarks>
    Public Overridable Sub MenuSelected(ByVal Result As Menu.Node)
        Select Case LCase(Result.Name)
            Case "remove"
                RemoveAt(Index)

        End Select
    End Sub



    ''' <summary>
    ''' Is called when the mouse has duble clicked on the object.
    ''' Is Not used by BaseObject.
    ''' </summary>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Public Overridable Sub MouseDoubleClick(ByVal e As MouseEventArgs)
    End Sub

    ''' <summary>
    ''' Is called when mouse has moved over the object.
    ''' Is Not used by BaseObject.
    ''' </summary>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Public Overridable Sub MouseMove(ByVal e As MouseEventArgs)
    End Sub

    ''' <summary>
    ''' Is called when a mouse button is down over the object.
    ''' Is Not used by BaseObject.
    ''' </summary>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Public Overridable Sub MouseDown(ByVal e As MouseEventArgs)
    End Sub

    ''' <summary>
    ''' Is called when a mouse button is released over the object.
    ''' BaseObject uses for opening the menu.
    ''' </summary>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Public Overridable Sub MouseUp(ByVal e As MouseEventArgs)
        If e.Button = MouseButtons.Right Then
            Menu.Open(Index, MenuItems)
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
        If Types.Length = 0 Then 'If we did not find anything after spliting.
            Me.Name = Name 'Then we just set the name.
        Else
            'Other wise we have to set the name to the first type.
            Me.Name = Types(0)
            'Then we loop thrugh all of the types and add them to the list. (Starting at one because zero is the name.)
            For n As Integer = 1 To Types.Length - 1
                DataType.Add(Types(n))
            Next
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

            For i As Integer = 1 To data.Length - 1 Step 2
                TryConnect(data(i), data(i + 1))
            Next
            'Make sure everything connected.
            If Connected <> data(0) Then
                Throw New Exception("Connections do not match!" & Environment.NewLine & "Name=" & Name & " ObjectTitle=" & Objects(obj).Title)
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