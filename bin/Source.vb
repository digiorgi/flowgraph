Imports Microsoft.VisualBasic
Imports System
Imports System.Drawing
Imports System.Collections
Imports System.Collections.Generic
Imports System.Windows.Forms
Imports System.Diagnostics
Namespace Plugins
'AddMenuObject|Add2,Plugins.fgAdd,,2|Math,Add
'AddMenuObject|Add3,Plugins.fgAdd,,3|Math,Add
'AddMenuObject|Add4,Plugins.fgAdd,,4|Math,Add
'AddMenuObject|Add5,Plugins.fgAdd,,5|Math,Add
'AddMenuObject|Add6,Plugins.fgAdd,,6|Math,Add
'AddMenuObject|Add7,Plugins.fgAdd,,7|Math,Add
'AddMenuObject|Add8,Plugins.fgAdd,,8|Math,Add
'AddMenuObject|Add9,Plugins.fgAdd,,9|Math,Add
'AddMenuObject|Add10,Plugins.fgAdd,,10|Math,Add
'Needed|Name displayed,Class name,Width,UserData|Groups
Public Class fgAdd
    Inherits BaseObject

    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 30) 'Setup the base rectangles.

        Dim InputCount As Integer = 2
        If UserData <> "" Then
            InputCount = UserData
        End If


        ReDim Values(InputCount - 1)
        Dim inp(InputCount - 1) As String
        For n As Integer = 0 To InputCount - 1
            inp(n) = "Value " & n & ",Number"
        Next

        'Create the inputs.
        Inputs(inp)
        'Create the output.
        Outputs(New String() {"Equals,Number"})

        'Set the title.
        Title = "Add"

    End Sub

    Private Values() As Integer
    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)

        Values(sender.Index) = Data

        Dim Equals As Integer = 0
        For Each Value As Integer In Values
            Equals += Value
        Next
        Send(Equals)
    End Sub

End Class

'AddMenuObject|Counter,Plugins.fgCounter,60|Math
Public Class fgCounter
    Inherits BaseObject

    Private WithEvents tmr As New Timer

    Private Value As Integer

    Private WithEvents btnReset As New Button

    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 105, 45) 'Setup the base rectangles.

        'Create one output.
        Outputs(New String() {"Value,Number"})

        Inputs(New String() {"Enable,Boolean", "Reset"})

        'Set the title.
        Title = "Counter"

        tmr.Interval = 1000
        tmr.Enabled = True


        btnReset.Text = "Reset"
        btnReset.Location = Position + New Point(5, 15)
        AddControl(btnReset)

    End Sub

    Public Overrides Sub Dispose()
        btnReset.Dispose()
        MyBase.Dispose()
    End Sub

    Public Overrides Sub Moving()
        btnReset.Location = Position + New Point(5, 15)
    End Sub

    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Select Case sender.Index
            Case 0 'Enable
                tmr.Enabled = Data

            Case 1 'Reset
                Value = 0
                Send(Value)
                DoDraw()
        End Select
    End Sub


    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        'Draw the base stuff like the title outputs etc..
        MyBase.Draw(g)

        'Draw the value.
        g.DrawString("Value= " & Value, DefaultFont, DefaultFontBrush, Rect.X + 15, Rect.Y + 15)

    End Sub

    Public Overrides Sub MouseDoubleClick(ByVal e As System.Windows.Forms.MouseEventArgs)
        On Error Resume Next
        Value = InputBox("Enter value", "Counter", 0)

    End Sub


    Private Sub tmr_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles tmr.Tick

        Value += 1

        Send(Value)

        DoDraw()
    End Sub

    Private Sub btnReset_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnReset.Click
        Value = 0
        Send(Value)
        DoDraw()
    End Sub
End Class

'AddMenuObject|Frame counter,Plugins.fgFPS,160|Misc
Public Class fgFPS
    Inherits BaseObject

    Private FrameCount As Integer

    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 120, 15) 'Setup the base rectangles.

        'Set the title.
        Title = "Frames"

    End Sub


    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        'Draw the base stuff like the title outputs etc..
        MyBase.Draw(g)
        FrameCount += 1

        g.DrawString("Total Frames= " & FrameCount, DefaultFont, DefaultFontBrush, Position.X, Position.Y)
    End Sub
End Class

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

Namespace Menu
    Public Enum Result
        SelectedItem
        SelectedGroup
        Closed
    End Enum

    Public Module Menu
        Public MenuStartPosition As Point

        Private Item As Node
        Private ObjectIndex As Integer = -1

        Public Rect As Rectangle

        Private Title As String = "Main"
        Private TitleRect As RectangleF

        ''' <summary>
        ''' Open a dropdown menu with the items.
        ''' </summary>
        ''' <param name="ObjectIndex">The object the menu will call MenuSlected to. -1 will add the object.</param>
        ''' <param name="Item">The item for the menu to use.</param>
        Public Sub Open(ByVal ObjectIndex As Integer, ByVal Item As Node)
            Menu.ObjectIndex = ObjectIndex

            'Set the menu start position to the current mouse position.
            MenuStartPosition = Mouse.Location

            'Set the tool to menu.
            Tool = ToolType.Menu

            Update(Item)

            'Draw the newly opened menu.
            DoDraw(True)
        End Sub

        Public Function MouseUp() As Node

            'If the mouse is not in the main rect. then we close the menu.
            If Rect.IntersectsWith(Mouse) Then

                If Mouse.IntersectsWith(New Rectangle(Rect.X, Rect.Y, Rect.Width, 12)) Then
                    If Item.Parent IsNot Nothing Then
                        Update(Item.Parent)
                    Else
                    End If
                    Return New Node(Result.SelectedGroup)
                End If

                For n As Integer = 0 To Item.Children.Count - 1
                    If Mouse.IntersectsWith(New Rectangle(Rect.X, Rect.Y + (12 * (n + 1)), Rect.Width, 12)) Then

                        If Item.Children(n).IsGroup Then
                            Item.Children(n).SetResult(Result.SelectedGroup)
                            Dim ReturnNode As Node = Item.Children(n)

                            Update(Item.Children(n))
                            Return ReturnNode
                        Else

                            If ObjectIndex > -1 Then
                                Objects(ObjectIndex).MenuSelected(Item.Children(n))
                            Else
                                AddObject(Item.Children(n).ClassName, MenuStartPosition, Item.Children(n).UserData)
                            End If

                            Item.Children(n).SetResult(Result.SelectedItem)
                            Tool = ToolType.None
                            Return Item.Children(n)
                        End If

                    End If
                Next

            End If

            Tool = ToolType.None
            Return New Node(Result.Closed)
        End Function


        Private LastSelected As Integer
        ''' <summary>
        ''' Returns false if noting changed.
        ''' </summary>
        Public Function MouseMove() As Boolean
            If Not Mouse.IntersectsWith(Rect) Then
                If LastSelected > -1 Then
                    LastSelected = -1
                    Return True
                Else
                    Return False
                End If
            End If
            For n As Integer = 1 To Item.Children.Count
                If Mouse.IntersectsWith(New Rectangle(Rect.X, Rect.Y + (12 * n), Rect.Width, 12)) And n <> LastSelected Then
                    LastSelected = n
                    Return True
                End If
            Next
            Return False
        End Function

        Public Sub Draw(ByVal g As Graphics)
            'Resize the title rectangle if needed.
            If TitleRect.Size = Nothing Then
                TitleRect.Size = g.MeasureString(Title, DefaultFont)
                If TitleRect.Size.Width > Rect.Width Then
                    Rect.Width = TitleRect.Width + 4
                End If
                TitleRect.Location = New PointF(Rect.X + (Rect.Width * 0.5) - (TitleRect.Width * 0.5), Rect.Y - 1)
            End If

            'Draw the background.
            g.FillRectangle(SystemBrushes.Menu, Rect)

            'Draw deviding line betwen the title and the items.
            g.DrawLine(Pens.Black, Rect.Location + New Point(0, 12), Rect.Location + New Point(Rect.Width, 12))
            g.DrawString(Title, DefaultFont, SystemBrushes.MenuText, TitleRect) 'Draw the title.


            For n As Integer = 1 To Item.Children.Count

                If Mouse.IntersectsWith(New Rectangle(Rect.X, Rect.Y + (12 * n), Rect.Width, 12)) Then
                    g.FillRectangle(SystemBrushes.Highlight, Rect.X, Rect.Y + (12 * n), Rect.Width, 12)
                    g.DrawString(Item.Children(n - 1).ToString, DefaultFont, SystemBrushes.HighlightText, Rect.X + 1, Rect.Y + (12 * n))
                Else
                    g.DrawString(Item.Children(n - 1).ToString, DefaultFont, SystemBrushes.MenuText, Rect.X + 1, Rect.Y + (12 * n))
                End If


            Next

            g.DrawRectangle(Pens.Black, Rect)
        End Sub

        Private Sub Update(ByVal Item As Node)
            Title = Item.Name
            Menu.Item = Item


            'Set the min width to 60.
            Dim Width As Integer = 60
            'Look in each node and see if there is any with a biger width.
            For Each Node As Node In Item.Children
                If Node.Width > Width Then
                    Width = Node.Width
                End If
            Next
            'Set the size of the rectangle.
            Rect.Size = New Size(Width, (Item.Children.Count * 12) + 13)
            TitleRect.Size = Nothing 'Set the title rectangle to nothing so next draw it will find it.

            'Center over mouse start position.
            Rect.X = MenuStartPosition.X - (Rect.Width * 0.5)
            Rect.Y = MenuStartPosition.Y - (Rect.Height * 0.5)

            'Clip the menu to the window.
            If Rect.X < 0 Then Rect.X = 1
            If Rect.Y < 0 Then Rect.Y = 1
            If Rect.Right > Form.ClientSize.Width Then Rect.X = Form.ClientSize.Width - Rect.Width - 1
            If Rect.Bottom > Form.ClientSize.Height Then Rect.Y = Form.ClientSize.Height - Rect.Height - 1
        End Sub

        ''' <summary>
        ''' Add a menu node to a node list.
        ''' Usefull to add groups.
        ''' </summary>
        ''' <param name="Item">The item to add the node to.</param>
        ''' <param name="Data">Name,(Optional ClassName) Or Width, Optional Width</param>
        ''' <param name="Groups"></param>
        ''' <remarks></remarks>
        Public Function AddNode(ByVal Item As Node, ByVal Data As String(), ByVal Groups As String()) As Node
            Dim Node As New Node
            Select Case Data.Length
                Case 0
                    Return Nothing

                Case 2
                    'Is it width or a class?
                    If Data(1).GetType = GetType(String) Then
                        Node.ClassName = Data(1)
                    Else
                        Node.Width = Data(1)
                    End If

                Case 3
                    Node.ClassName = Data(1)
                    If Data(2) <> "" Then Node.Width = Data(2)

                Case 4
                    Node.ClassName = Data(1)
                    If Data(2) <> "" Then Node.Width = Data(2)
                    Node.UserData = Data(3)
            End Select
            Node.Name = Data(0)

            'Is there any groups?
            If Groups.Length > 0 Then

                Dim CurrentGroup As Integer = 0
                Dim CurrentNode As Node = Item
                Do
                    'Find width in name.  GroupName'95
                    Dim tmpWidth As Integer = 50
                    Dim tmp() As String = Split(Groups(CurrentGroup), "'")
                    If tmp.Length = 2 Then
                        Groups(CurrentGroup) = tmp(0)
                        tmpWidth = tmp(1)
                    End If

                    Dim Found As Boolean = False
                    If CurrentNode.Children IsNot Nothing Then
                        For Each n As Node In CurrentNode.Children
                            If LCase(n.Name) = LCase(Groups(CurrentGroup)) And n.IsGroup Then
                                If n.Width < tmpWidth Then n.Width = tmpWidth
                                CurrentNode = n
                                Found = True
                                Exit For
                            End If
                        Next
                    End If
                    If Found = False Then
                        CurrentNode.Add(New Node(Groups(CurrentGroup), True, tmpWidth))
                        CurrentNode = CurrentNode.Children(CurrentNode.Children.Count - 1)
                    End If


                    CurrentGroup += 1
                Loop Until CurrentGroup = Groups.Length

                CurrentNode.Add(Node)

            Else
                'There was no groups so lets just add the item.
                AddItem.Add(Node)
            End If


            Return Node
        End Function
    End Module

    Public Class Node
        Public Parent As Node
        Public Children As List(Of Node)
        Public Name As String
        Public ClassName As String
        Public Width As Integer
        Public Result As Result
        Public UserData As String

#Region "New"
        Sub New()
        End Sub

        ''' <summary>Is used when the menu sends the object the node.</summary>
        ''' <param name="Result">What was the menus results?</param>
        Sub New(ByVal Result As Result)
            Me.Result = Result
        End Sub

        ''' <summary>Create a node</summary>
        ''' <param name="Name">The name that will display in the menu.</param>
        ''' <param name="ClassName">The name of the class to create for adding objects.</param>
        ''' <param name="Width">The first node in each group. Specifies the width of the menu.</param>
        Public Sub New(ByVal Name As String, ByVal ClassName As String, Optional ByVal Width As Integer = 50)
            Me.Name = Name
            Me.ClassName = ClassName
            Me.Width = Width
        End Sub
        ''' <summary>Create a node</summary>
        ''' <param name="Name">The name that will display in the menu.</param>
        ''' <param name="IsGroup">Is this node going to have children?</param>
        ''' <param name="Width">The first node in each group. Specifies the width of the menu.</param>
        Public Sub New(ByVal Name As String, Optional ByVal IsGroup As Boolean = False, Optional ByVal Width As Integer = 50)
            Me.Name = Name
            Me.Width = Width
            If IsGroup Then Children = New List(Of Node)
        End Sub
#End Region

        Public Sub Add(ByVal Item As Node)
            Item.Parent = Me
            Children.Add(Item)
        End Sub

        Public Sub SetResult(ByVal Result As Result)
            Me.Result = Result
        End Sub
        Public ReadOnly Property IsGroup() As Boolean
            Get
                Return Children IsNot Nothing
            End Get
        End Property
        Public Overrides Function ToString() As String
            If Name = "" Then
                Return "NoName"
            Else
                Return Name
            End If
        End Function

        ''' <summary>
        ''' Sorts All children. even the childrens children+.
        ''' </summary>
        Public Sub Sort()
            If Not IsGroup Then Return
            Children.Sort(Function(a As Node, b As Node)
                              Return a.Name.CompareTo(b.Name)
                          End Function)

            For Each child As Node In Children
                If child.IsGroup Then
                    child.Sort()
                End If
            Next
        End Sub

    End Class
End Namespace

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

Public Module Plugins


    'The default font to use.
    Public DefaultFont As Font = SystemFonts.DefaultFont
    Public DefaultFontBrush As Brush = SystemBrushes.ControlText
    Public InputImage, OutputImage As Image

    'The pen used to connect objects.
    Public ConnectorPen As New Pen(Color.FromArgb(80, 80, 80), 3)

    'Used to check if the mouse is inside a rectangle.
    Public Mouse As Rectangle

    Public Form As Control

    
#Region "Grid"
    'The snap grid size.
    Public GridSize As Integer = 5

    Public Function SnapToGrid(ByVal value As Decimal) As Decimal
        Return Math.Round(value / GridSize) * GridSize
    End Function
    Public Function SnapToGrid(ByVal value As Double) As Double
        Return Math.Round(value / GridSize) * GridSize
    End Function

    Public Function SnapToGrid(ByVal point As Point) As Point
        Return New Point(Math.Round(point.X / GridSize) * GridSize, Math.Round(point.Y / GridSize) * GridSize)
    End Function
    Public Function SnapToGrid(ByVal point As PointF) As PointF
        Return New PointF(Math.Round(point.X / GridSize) * GridSize, Math.Round(point.Y / GridSize) * GridSize)
    End Function

    Public Function SnapToGrid(ByVal rect As Rectangle) As Rectangle
        Return New Rectangle(Math.Round(rect.X / GridSize) * GridSize, Math.Round(rect.Y / GridSize) * GridSize, _
                             Math.Round(rect.Width / GridSize) * GridSize, Math.Round(rect.Height / GridSize) * GridSize)
    End Function
    Public Function SnapToGrid(ByVal rect As RectangleF) As RectangleF
        Return New RectangleF(Math.Round(rect.X / GridSize) * GridSize, Math.Round(rect.Y / GridSize) * GridSize, _
                              Math.Round(rect.Width / GridSize) * GridSize, Math.Round(rect.Height / GridSize) * GridSize)
    End Function
#End Region

#Region "Tool stuff"

    Public Enum ToolType
        None
        Move
        Connect
        Menu
    End Enum
    Public Tool As ToolType
    Public ToolOffset As Point
    Public ToolObject As Integer
    Public ToolInt As Integer
#End Region

#Region "Object stuff"
    'The list of objects.
    Public Objects As New List(Of Object)

    Public Sub ResetObjectIndexs(ByVal RemovedIndex As Integer)
        For n As Integer = 0 To Objects.Count - 1
            Objects(n).Index = n


            If Objects(n).Output IsNot Nothing Then
                For o As Integer = 0 To Objects(n).Output.Length - 1
                    Dim i As Integer = 0
                    Do Until i >= Objects(n).Output(o).Flow.Count
                        If Objects(n).Output(o).Flow(i).obj = RemovedIndex Then
                            Objects(n).Output(o).Flow(i) = Nothing
                            Objects(n).Output(o).Flow.RemoveAt(i)
                        ElseIf Objects(n).Output(o).Flow(i).obj > RemovedIndex Then
                            Objects(n).Output(o).Flow(i).obj -= 1
                            i += 1
                        Else
                            i += 1
                        End If
                    Loop
                Next
            End If
        Next
    End Sub

    Public Sub RemoveAt(ByVal Index As Integer)
        Objects(Index).Dispose()
        Objects(Index) = Nothing
        Objects.RemoveAt(Index)

        ResetObjectIndexs(Index)
    End Sub

    Public Sub ClearObjects()
        For Each obj As Object In Objects
            obj.Dispose()
        Next
        Objects.Clear()
    End Sub
#End Region

#Region "Open & Save"

    ''' <summary>
    ''' Loads the main plugin stuff.
    ''' </summary>
    Public Sub Load_Plugin(ByVal form As Control)
        'Setup the auto draw timmer.
        tmrDraw.Interval = 200
        tmrDraw.Enabled = True

        If IO.File.Exists("Plugins\Base\Input.png") Then
            InputImage = Image.FromFile("Plugins\Base\Input.png")
        End If
        If IO.File.Exists("Plugins\Base\Output.png") Then
            OutputImage = Image.FromFile("Plugins\Base\Output.png")
        End If

        Plugins.Form = form
        'RemoveFromFGS
        AddObject_Setup()'EndRemoveFromFGS
    End Sub

    'RemoveFromFGS

    Public LoadedFile As String = ""
    Public Const FileVersion = 0.5

    Public Sub Open(ByVal File As String)
        If Not IO.File.Exists(File) Then
            MsgBox("Could not find file:" & vbNewLine & File, , "Error loading")
            Return
        End If

        ClearObjects()

        Dim sd As New SimpleD.SimpleD
        sd.FromFile(File)

        Dim g As SimpleD.Group = sd.Get_Group("Main")
        Form.ClientSize = New Size(g.Get_Value("Width"), g.Get_Value("Height"))


        'Make sure the versions match.
        If g.Get_Value("FileVersion") <> FileVersion Then
            MsgBox("Wrong file version." & Environment.NewLine _
                   & "File version: " & g.Get_Value("FileVersion") & Environment.NewLine _
                   & "Requires  version: " & FileVersion, MsgBoxStyle.Critical, "Error loading")
            Return
        End If

        'Get the number of objects.
        Dim numObj As Integer = g.Get_Value("Objects")
        For n As Integer = 0 To numObj 'Loop thrugh each object.
            g = sd.Get_Group("Object" & n) 'Get the object.
            If g Is Nothing Then
                MsgBox("Could not find object# " & n & " in file.", MsgBoxStyle.Critical + MsgBoxStyle.OkOnly, "Error loading file")
                ClearObjects()
                LoadedFile = ""
                Return
            End If

            'Get the position.
            Dim pos As String() = Split(g.Get_Value("position"), ",")
            Dim obj As Integer = AddObject(g.Get_Value("name"), New Point(pos(0), pos(1)), g.Get_Value("userdata")) 'Get the object.

            'Show error if could not create object.
            If obj = -1 Then
                MsgBox("Could not create object# " & n & Environment.NewLine & "Name: " & g.Get_Value("name"), MsgBoxStyle.Critical + MsgBoxStyle.OkOnly, "Error loading file")
                ClearObjects()
                LoadedFile = ""
                Return
            End If
        Next

        'Load each object.
        For n As Integer = 0 To numObj
            g = sd.Get_Group("Object" & n)
            'Try and load each object.
            Try
                Objects(n).Load(g)
            Catch ex As Exception
                MsgBox("Could not load object# " & n & Environment.NewLine & "Name: " & g.Get_Value("name") & vbNewLine _
                      & "Execption=" & ex.Message, MsgBoxStyle.Critical + MsgBoxStyle.OkOnly, "Error loading object")
            End Try
        Next

        'Set the loaded file
        LoadedFile = File

        DoDraw()
    End Sub

    Public Sub Save(ByVal File As String)

        Dim sd As New SimpleD.SimpleD
        Dim g As SimpleD.Group = sd.Create_Group("Main")
        g.Set_Value("Width", Form.ClientSize.Width)
        g.Set_Value("Height", Form.ClientSize.Height)
        g.Set_Value("Objects", Objects.Count - 1)
        g.Set_Value("FileVersion", FileVersion)

        'Save each object.
        For Each obj As Object In Objects
            sd.Add_Group(obj.Save)
        Next

        'Save to file.
        sd.ToFile(File)

        LoadedFile = File
    End Sub

    'EndRemoveFromFGS
#End Region

#Region "Auto draw"
    Event DrawEvent(ByVal region As Rectangle)
    Public Draw As Boolean = True
    Private DoNotDraw As Boolean = True

    ''' <summary>
    ''' Tells auto draw to draw when the time comes.
    ''' </summary>
    ''' <param name="HeighPriority">If it's a heigh priority, then it will draw as soon as possible.</param>
    Public Sub DoDraw(Optional ByVal HeighPriority As Boolean = False)
        If Not Draw Then Return

        'If it is a heigh priority. then we will not wait for the next timmer tick and just draw.
        If HeighPriority Then
            RaiseEvent DrawEvent(Rectangle.Empty)
            DoNotDraw = True

        Else 'Other wise we wait for the timer.
            DoNotDraw = False 'Tell the timer it can draw.
        End If
    End Sub
    Public Sub DoDraw(ByVal region As Rectangle)
        RaiseEvent DrawEvent(region)
    End Sub

    Private WithEvents tmrDraw As New Timer
    Private Sub tmrDraw_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles tmrDraw.Tick
        If DoNotDraw Then Return

        RaiseEvent DrawEvent(Rectangle.Empty)

        DoNotDraw = True
    End Sub
#End Region

#Region "Add & Remove Control"
    Event AddControlEvent(ByVal Control As Control)
    Event RemoveControlEvent(ByVal Control As Control)

    Public Sub AddControl(ByVal Control As Control)
        RaiseEvent AddControlEvent(Control)
    End Sub
    Public Sub RemoveControl(ByVal Control As Control)
        RaiseEvent RemoveControlEvent(Control)
    End Sub
#End Region

#Region "Adding objects"
    'The items in the add object menu.
    Public AddItem As New Menu.Node("Add object", True)

    ''' <summary>
    ''' Add a new object from the class name.
    ''' </summary>
    ''' <param name="Name">ex: 'Plugins.fgAdd'</param>
    ''' <param name="StartPosition">You shouldn't need help here.</param>
    ''' <returns>-1 if not found. other wise returns object index.</returns>
    ''' <remarks></remarks>
    Public Function AddObject(ByVal Name As String, ByVal StartPosition As Point, Optional ByVal UserData As String = "") As Integer
        'NOTE: I am pretty sure there is a faster way to do this.
        'But I got this working first, so until it is a problem it will stay like this.
        Try
            Objects.Add(Activator.CreateInstance(Type.[GetType](Name), New Object() {StartPosition, UserData}))
            Return Objects.Count - 1
        Catch ex As Exception
            'MsgBox("Could not create object: " & Name)
            Return -1
        End Try
    End Function

    'RemoveFromFGS
    Private Sub AddObject_Setup()
        'Is the plugins library newer then the objects file?
        If IO.File.GetLastWriteTime("Plugins.dll") > IO.File.GetLastWriteTime("Plugins\MenuObjects.list") Then

            'The plugins have changed. So lets find all of the objects.

            Dim Scripts As String() = IO.Directory.GetFiles("Plugins\", "*.vb", IO.SearchOption.AllDirectories)
            Dim ObjectList As String = ""
            For Each File As String In Scripts
                SearchForItems(File, ObjectList)
            Next

            'Write all of the objects found to the file.
            Dim sw As New IO.StreamWriter("Plugins\MenuObjects.list", False)
            sw.Write(ObjectList)
            sw.Close()

        Else
            'Objects.list is newer, so lets get the items from it.
            SearchForItems("Plugins\MenuObjects.list")
        End If
    End Sub

    ''' <summary>
    ''' Search thru a file and fill the add object menu. With found objects.
    ''' </summary>
    ''' <param name="File">The file to search thru.</param>
    ''' <param name="ObjectList">Will fill string with each line that has "AddMenuObject".
    ''' Unlis set to "DoNotFill"</param>
    ''' <remarks></remarks>
    Private Sub SearchForItems(ByVal File As String, Optional ByRef ObjectList As String = "DoNotFill", Optional ByVal SearchWholeFile As Boolean = False)
        Dim sr As New IO.StreamReader(File)
        Dim StartIndex As Integer
        Do
            Dim line As String = sr.ReadLine 'Get the next line out of the file.
            StartIndex = line.IndexOf("AddMenuObject", StringComparison.OrdinalIgnoreCase) 'Get the index of "AddMenuObject".

            'If we found "AddMenuObject" then.
            If StartIndex > -1 Then
                'Should split like:
                'AddMenuObject|Name,ClassName,Optional Width|Group1,Group2,Group3,etc..
                'Groups are optional.
                Dim SplitLine As String() = Split(line, "|")
                Select Case SplitLine.Length
                    Case 2 'No groups. 
                        Menu.AddNode(AddItem, Split(SplitLine(1), ","), New String() {})

                    Case 3 'Has Group(s) 
                        Menu.AddNode(AddItem, Split(SplitLine(1), ","), Split(SplitLine(2), ","))

                End Select

                'Fill object list(if not "DoNotFill").
                If ObjectList = "DoNotFill" Then
                ElseIf ObjectList = "" Then
                    ObjectList = line.Remove(0, 1)
                Else
                    ObjectList &= vbNewLine & line.Remove(0, 1)
                End If
            End If

        Loop Until sr.EndOfStream Or (StartIndex = -1 And Not SearchWholeFile)
        sr.Close()

        AddItem.Sort()
    End Sub
    'EndRemoveFromFGS
#End Region

End Module



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


Namespace SimpleD
    Module Info
        Public Const IllegalCharacters As String = "{}=;"
        Public Const Version = 0.99
        Public Const FileVersion = 1
        '0.99
        'Added  : Can now have groups inside of groups.
        'Added  : GetValue(Control,Value)  Gets the property from the control name. Then sets the contols value, if the control is known.
        'Changed: Spliting is now done with a few options not a string.
        'Fixed  : SetValue(Control) now check throgh known controls for the right value.
        '0.986
        'Added: default value to Set_Value.
        'Changed: Control to Windows.Forms.Control
        '0.985
        'Added  : FileVersion So I can easley tell if the file has changed.
        'Added  : IllegalCharacters property names and values can NOT have any of the characters in IllegalCharacters.
        'Fixed  : Only allows one group with the same names. will combine groups if names match.
        'Changed: Prop from a class to a structure.
        'Changed: Everything returns empty if not found.
        'Changed: Does not add if a value or name is empty.
        'Changed: Get_Group returns Nothing if no group found.
        'Removed: Group.Add because set_value will create if not found.
        '0.983
        'Fixed: Spelling.
        '0.982
        'Added: Add_Group
        '0.981
        'Changed: Get_Value(Name, Value) No longer throws a error if no value found.
        'Clean up.
        'Added: Linense and contact.
        '0.98
        'Fixed: Spelling
        'Added: New get value with byref value
        '0.97
        'Added: ToFile
        'Added: Check exists in FromFile
        '0.96
        'Added: FromFile
    End Module

    Public Class SimpleD
        Private Groups As New List(Of Group)

#Region "New"
        ''' <summary>
        ''' Load from string.
        ''' </summary>
        ''' <param name="Data"></param>
        ''' <param name="FromFile">If set to true then it will load from the file specfied in data</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal Data As String, Optional ByVal FromFile As Boolean = False)
            If Not FromFile Then
                FromString(Data)
            Else
                Me.FromFile(Data)
            End If

        End Sub
        Public Sub New()
        End Sub
#End Region

#Region "Group"
        ''' <summary>
        ''' Create a group.
        ''' Will return other group if names match.
        ''' </summary>
        ''' <param name="Name">The name of the group.</param>
        Public Function Create_Group(ByVal Name As String) As Group
            Dim tmp As Group = Get_Group(Name) 'Search for a group with the name.
            If tmp Is Nothing Then 'If group not found then.
                tmp = New Group(Name) 'Create a new group.
                Groups.Add(tmp) 'Add the new group to the list.
            End If
            Return tmp 'Return the group.
        End Function
        Public Sub Add_Group(ByVal Group As Group)
            'First lets see if we can find a group.
            Dim tmp As Group = Get_Group(Group.Name)
            If tmp IsNot Nothing Then
                'We found a group so lets combine them.
                tmp.Combine(Group)
            Else
                'We did not find any other groups so add it to the list.
                Groups.Add(Group)
            End If
        End Sub
        Public Function Get_Group(ByVal Name As String) As Group
            Name = LCase(Name)
            For Each Group As Group In Groups
                If Name = LCase(Group.Name) Then
                    Return Group
                End If
            Next
            Return Nothing
        End Function

#End Region

#Region "To String/File"

        Public Sub ToFile(ByVal File As String, Optional ByVal SplitWithNewLine As Boolean = True, Optional ByVal SplitWithTabs As Boolean = True)
            Dim sw As New IO.StreamWriter(File)
            sw.Write(ToString(SplitWithNewLine, SplitWithTabs))
            sw.Close()
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="SplitWithNewLine">Split propertys and groups using a newline?</param>
        ''' <param name="SplitWithTabs">Split propertys and groups using tabs?
        ''' Does not use tabs if newline is disabled.</param>
        Public Overloads Function ToString(Optional ByVal SplitWithNewLine As Boolean = True, Optional ByVal SplitWithTabs As Boolean = True) As String
            If Groups.Count = 0 Then Return ""
            If SplitWithNewLine = False Then SplitWithTabs = False

            Dim tmp As String = "//Version=" & Version & " FileVersion=" & FileVersion & "\\"
            For n As Integer = 0 To Groups.Count - 1
                tmp &= vbNewLine & Groups(n).ToString(SplitWithNewLine, If(SplitWithTabs, 1, 0))
            Next

            Return tmp

        End Function
#End Region

#Region "From String/File"
        ''' <summary>
        ''' Load the SimpleData from a file.
        ''' </summary>
        ''' <param name="File"></param>
        ''' <returns>True if loaded false if not.</returns>
        ''' <remarks></remarks>
        Public Function FromFile(ByVal File As String) As Boolean
            If Not IO.File.Exists(File) Then Return False
            Dim sr As New IO.StreamReader(File)
            FromString(sr.ReadToEnd)
            sr.Close()
            Return True
        End Function
        Public Sub FromString(ByVal Data As String)
            If Data = "" Then Return

            Dim InComment As Boolean = False

            Dim n As Integer
            Do
                Dim tmp As String = Data.Substring(n, 2)
                If tmp = "//" Then
                    InComment = True
                    n += 1
                ElseIf tmp = "\\" Then
                    InComment = False
                    n += 1
                    'ElseIf tmp = vbNewLine Then
                    '    InComment = False
                    '    n += 1
                    '    GoTo NextG


                ElseIf Not InComment Then

                    'Find the start so we can get the name.
                    Dim Start As Integer = Data.IndexOf("{", n)
                    If Start = -1 Then Return
                    'Now get the name.
                    Dim gName As String = Trim(Data.Substring(n, Start - n).Trim(New Char() {vbCr, vbLf, vbTab}))
                    n = Start + 1

                    Dim Group As New Group(gName)
                    Groups.Add(Group)

                    GetGroup(Data, n, Group)
                    If n + 2 > Data.Length Then Return
                End If


                n += 1
            Loop Until n >= Data.Length - 1


        End Sub

        Private Sub GetGroup(ByVal Data As String, ByRef n As Integer, ByVal Group As Group)
            Dim tmp As String
            Dim InComment As Boolean = False
            'Now lets get all of the propertys from the group.
            Do
                If n + 2 > Data.Length Then Return
                tmp = Data.Substring(n, 2)
                If tmp = "//" Then
                    InComment = True
                    n += 1
                ElseIf tmp = "\\" Then
                    InComment = False
                    n += 1


                ElseIf Not InComment Then
                    Dim Equals As Integer = Data.IndexOf("=", n)
                    Dim GroupStart As Integer = Data.IndexOf("{", n)
                    Dim GroupEnd As Integer = Data.IndexOf("}", n)
                    If Equals = -1 And GroupStart = -1 Then Return
                    If GroupEnd < GroupStart And GroupEnd < Equals Then
                        n = GroupEnd
                        Return
                    End If
                    'Is the next thing a group or property?
                    If Equals > -1 And ((Equals < GroupStart) Or GroupStart = -1) Then
                        Dim PropName As String = Trim(Data.Substring(n, Equals - n).Trim(New Char() {vbCr, vbLf, vbTab}))
                        n = Equals
                        Dim PropEnd As Integer = Data.IndexOf(";", n)
                        Dim PropValue As String = Trim(Data.Substring(n + 1, PropEnd - n - 1).Trim(New Char() {vbCr, vbLf, vbTab}))
                        n = PropEnd
                        Group.Set_Value(PropName, PropValue)
                    ElseIf GroupStart > -1 Then

                        Dim gName As String = Trim(Data.Substring(n, GroupStart - n).Trim(New Char() {vbCr, vbLf, vbTab}))
                        n = GroupStart + 1

                        Dim NewGroup As New Group(gName)
                        Group.Add_Group(NewGroup)
                        GetGroup(Data, n, NewGroup)
                    End If

                End If

                n += 1
            Loop Until Data.Substring(n, 1) = "}"
        End Sub
#End Region

    End Class

    Public Class Group
        Public Name As String

        Private Propertys As New List(Of Prop)
        Private Groups As New List(Of Group)

        Public Sub New(ByVal Name As String)
            Me.Name = Name
        End Sub


#Region "Group"


        ''' <summary>
        ''' Create a group.
        ''' Will return other group if names match.
        ''' </summary>
        ''' <param name="Name">The name of the group.</param>
        Public Function Create_Group(ByVal Name As String) As Group
            Dim tmp As Group = Get_Group(Name) 'Search for a group with the name.
            If tmp Is Nothing Then 'If group not found then.
                tmp = New Group(Name) 'Create a new group.
                Groups.Add(tmp) 'Add the new group to the list.
            End If
            Return tmp 'Return the group.
        End Function
        Public Sub Add_Group(ByVal Group As Group)
            'First lets see if we can find a group.
            Dim tmp As Group = Get_Group(Group.Name)
            If tmp IsNot Nothing Then
                'We found a group so lets combine them.
                tmp.Combine(Group)
            Else
                'We did not find any other groups so add it to the list.
                Groups.Add(Group)
            End If
        End Sub
        Public Function Get_Group(ByVal Name As String) As Group
            Name = LCase(Name)
            For Each Group As Group In Groups
                If Name = LCase(Group.Name) Then
                    Return Group
                End If
            Next
            Return Nothing
        End Function

#End Region

        ''' <summary>
        ''' Conbines the group with this group.
        ''' </summary>
        ''' <param name="Group">Overides all the propertys with the propertys in the group.</param>
        Public Sub Combine(ByVal Group As Group)
            For Each Prop As Prop In Group.Propertys
                Set_Value(Prop.Name, Prop.Value)
            Next

            For Each Grp As Group In Group.Groups
                Add_Group(Grp)
            Next
        End Sub


#Region "SetValue"
        ''' <summary>
        ''' This sets the value of a property.
        ''' If it can not find the property it creates it.
        ''' </summary>
        Public Sub Set_Value(ByVal Name As String, ByVal Value As String)
            If Name = "" Or Value = "" Then Return
            Dim tmp As Prop = Find(Name) 'Find the property.
            If tmp = Nothing Then 'If it could not find the property then.
                Propertys.Add(New Prop(Name, Value)) 'Add the property.
            Else
                tmp.Value = Value 'Set the value.
            End If
        End Sub
        ''' <summary>
        ''' This sets the value of a property.
        ''' If it can not find the property it creates it.
        ''' Does not create if value is eaqual to devault value.
        ''' </summary>
        Public Sub Set_Value(ByVal Name As String, ByVal Value As String, ByVal DefaultValue As String)
            If Name = "" Or Value = "" Then Return
            If Value = DefaultValue Then Return 'Return if the value is the default value.
            Dim tmp As Prop = Find(Name) 'Find the property.
            If tmp = Nothing Then 'If it could not find the property then.
                Propertys.Add(New Prop(Name, Value)) 'Add the property.
            Else
                tmp.Value = Value 'Set the value.
            End If
        End Sub
        ''' <summary>
        ''' This sets the value of a property.
        ''' If it can not find the property it creates it.
        ''' </summary>
        Public Sub Set_Value(ByVal Control As Windows.Forms.Control)
            Dim Value As String = GetValueFromObject(Control) 'Find the property from a object and set the value.
            Dim tmp As Prop = Find(Control.Name) 'Find the property.
            If tmp = Nothing Then 'If it could not find the property then.
                Propertys.Add(New Prop(Control.Name, Value)) 'Add the property.
            Else
                tmp.Value = Value 'Set the value.
            End If
        End Sub
#End Region

#Region "GetValue"
        ''' <summary>
        ''' Get the value from a property.
        ''' </summary>
        ''' <param name="Name">The name of the property to get the value from.</param>
        Public Function Get_Value(ByVal Name As String) As String
            Return Find(Name).Value 'Find the property and return the value.
        End Function
        ''' <summary>
        ''' Will not get value if no value found.
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <param name="Value"></param>
        ''' <param name="EmptyIfNotFound">Set value to nothing, if it can't find the property.</param>
        Public Sub Get_Value(ByVal Name As String, ByRef Value As Object, Optional ByVal EmptyIfNotFound As Boolean = True)
            Dim prop As Prop = Find(Name)
            If prop = Nothing Then
                If EmptyIfNotFound Then Value = Nothing
            Else
                Value = prop.Value 'Find the property and return the value.
            End If
        End Sub

        ''' <summary>
        ''' Sets the value of the control to the proprety with the same name.
        ''' Known controls: TextBox,Label,CheckBox,RadioButton,NumericUpDown,NumericUpDownAcceleration,ProgressBar
        ''' </summary>
        ''' <param name="Control">The control to get the property from.</param>
        ''' <param name="Value">Returns value if control is unknown.</param>
        Public Sub Get_Value(ByRef Control As Windows.Forms.Control, ByRef Value As String)
            Dim TempValue As String = Find(Control.Name).Value 'Find the property from the control name.

            Dim obj As Object = Control
            Select Case Control.GetType
                Case GetType(Windows.Forms.TextBox), GetType(Windows.Forms.Label)
                    obj.Text = TempValue

                Case GetType(Windows.Forms.CheckBox), GetType(Windows.Forms.RadioButton)
                    obj.Checked = TempValue

                Case GetType(Windows.Forms.NumericUpDown), GetType(Windows.Forms.NumericUpDownAcceleration), GetType(Windows.Forms.ProgressBar)
                    obj.Value = TempValue

                Case Else
                    'Throw New Exception("Could not find object type.")
                    Value = TempValue
            End Select
        End Sub
        ''' <summary>
        ''' Uses the name of the control to find the property value.
        ''' </summary>
        ''' <param name="Control"></param>
        ''' <returns>Property value.</returns>
        Public Function Get_Value(ByVal Control As Windows.Forms.Control) As String
            Return Find(Control.Name).Value 'Find the property from a object and return the value.
        End Function
#End Region


        ''' <summary>
        ''' Retuns "Could_Not_Find_Value" if it can not find the value.
        ''' </summary>
        ''' <param name="Obj"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function GetValueFromObject(ByVal Obj As Object) As String

            Select Case Obj.GetType
                Case GetType(Windows.Forms.TextBox), GetType(Windows.Forms.Label)
                    Return Obj.Text

                Case GetType(Windows.Forms.CheckBox), GetType(Windows.Forms.RadioButton)
                    Return Obj.Checked

                Case GetType(Windows.Forms.NumericUpDown), GetType(Windows.Forms.NumericUpDownAcceleration), GetType(Windows.Forms.ProgressBar)
                    Return Obj.Value

            End Select

            'Unknown control, so lets see if we can find the right value.
            Dim Value As String = "Could_Not_Find_Value"
            Try 'Try and get the value.
                Value = Obj.Value
            Catch
                Try 'Try and get checked.
                    Value = Obj.Checked
                Catch
                    Try 'Try and get the text.
                        Value = Obj.Text
                    Catch
                        Try
                            Value = Obj.ToString
                        Catch
                            Throw New Exception("Could not get value from object!")
                        End Try
                    End Try
                End Try
            End Try
            Return Value
        End Function

        ''' <summary>
        ''' Find a property from the name.
        ''' </summary>
        ''' <param name="Name">The name of the property.</param>
        ''' <returns>The property.</returns>
        Public Function Find(ByVal Name As String) As Prop
            'Very simple,  loop through each property until the names match. then return the matching property.
            For Each Prop As Prop In Propertys
                If LCase(Prop.Name) = LCase(Name) Then
                    Return Prop
                End If
            Next
            Return Nothing
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="SplitWithNewLine">Split propertys and groups using a newline?</param>
        ''' <param name="TabCount">Split propertys and groups using tabs?
        ''' Does not use tabs if newline is disabled.</param>
        Public Overloads Function ToString(Optional ByVal SplitWithNewLine As Boolean = True, Optional ByVal TabCount As Integer = 1) As String
            If Propertys.Count = 0 Then Return ""
            If TabCount < 0 Then TabCount = 0

            'Setup spliting.
            Dim Split As String = ""
            If SplitWithNewLine Then
                Split = vbNewLine & New String(vbTab, TabCount)
            End If

            'Name and start of group.
            Dim tmp As String = Name & "{"

            'Add the properys from the group.
            For n As Integer = 0 To Propertys.Count - 1
                tmp &= Split & Propertys(n).Name & "=" & Propertys(n).Value & ";"
            Next

            'Get all the groups in the group.
            For Each Grp As Group In Groups
                tmp &= Split & Grp.ToString(SplitWithNewLine, If(TabCount = 0, 0, TabCount + 1))
            Next

            '} end of group.
            tmp &= If(SplitWithNewLine, vbNewLine, "") & If(TabCount - 1 > 0, New String(vbTab, TabCount - 1), "") & "}"

            Return tmp
        End Function

        Overloads Shared Operator +(ByVal left As Group, ByVal right As Group) As Group
            left.Combine(right)
            Return left
        End Operator
    End Class

    Public Structure Prop
        Public Name As String
        Public Value As String
        Public Sub New(ByVal Name As String, ByVal Value As String)
            Me.Name = Name
            Me.Value = Value
        End Sub

        Shared Operator =(ByVal left As Prop, ByVal right As Prop) As Boolean
            Return left.Name = right.Name And left.Value = right.Value
        End Operator
        Shared Operator <>(ByVal left As Prop, ByVal right As Prop) As Boolean
            Return Not left = right
        End Operator

    End Structure


End Namespace

'AddMenuObject|Display as string,Plugins.DisplayAsString,100|Misc
Public Class DisplayAsString
    Inherits BaseObject

    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 105) 'Setup the base rectangles.
        File = "Common\DisplayAsString.vb"

        'Create one input.
        Inputs(New String() {"Value to display."})
        'Input(0).MaxConnected = 1 'Only allow one connection.

        'Set the title.
        Title = "Display as string"

        MenuItems.Add(New Menu.Node("Set String", , 70))
    End Sub

    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("Data", Data)

        Return g
    End Function
    Public Overrides Sub Load(ByVal g As SimpleD.Group)
        g.Get_Value("Data", Data)

        MyBase.Load(g)
    End Sub

    Public Overrides Sub MenuSelected(ByVal Result As Menu.Node)
        MyBase.MenuSelected(Result)

        If Result.Result = Global.Plugins.Menu.Result.SelectedItem Then
            If Result.Name = "Set String" Then
                Me.Data = InputBox("Set string", "THIS IS THE TITLE")
            End If
        End If
    End Sub

    Private Data As String
    Private DataSize, OldSize As SizeF
    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Me.Data = Data.ToString() 'Set the data.
        DataSize = Nothing 'Set the data size to nothing so we will check the size later.

        'Tell auto draw we want to draw.
        DoDraw(Rect)
    End Sub

    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        'Lets measure the size if data size is nothing.
        If DataSize = Nothing Then
            DataSize = g.MeasureString("String= " & Data, DefaultFont) 'Measure the string.
            If DataSize.Width < 75 Then DataSize.Width = 75 'Set the min width.

            'Did the size change?
            If DataSize <> OldSize Then
                'If so then we set the size of the base object
                MyBase.SetSize(DataSize.Width, DataSize.Height, True)
                OldSize = DataSize 'Then set the old size.
            End If

        End If

        'Draw the base stuff like the title outputs etc..
        MyBase.Draw(g)


        'Draw the value.
        g.DrawString("String= " & Data, DefaultFont, DefaultFontBrush, Position)
    End Sub

End Class

'AddMenuObject|Slider,Plugins.Slider
Public Class Slider
    Inherits BaseObject

    Private Enabled As Boolean = True

    Private Value As Integer


    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 100) 'Setup the base rectangles.

        Outputs(New String() {"Value,Number"})
        Inputs(New String() {"Enable,Boolean", "Value,Number,Boolean"})

        'Set the title.
        Title = "Slider"

        Value = MyBase.Size.Width * 0.5
    End Sub

    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Select Case sender.Index
            Case 0 'Enable
                Enabled = Data

            Case 1 'Set value
                If Not Enabled Then Return
                If Data.GetType Is GetType(Boolean) Then
                    If Data = True Then
                        Value = Size.Width
                    Else
                        Value = 0
                    End If
                Else
                    Value = Data * Size.Width
                End If


        End Select

        DoDraw(Rect)
    End Sub

    Public Overrides Sub MouseDown(ByVal e As System.Windows.Forms.MouseEventArgs)
        MyBase.MouseDown(e)
        If Not Enabled Then Return
        If e.Button = MouseButtons.Left Then
            Dim x As Integer = e.X - Position.X
            If x >= Size.Width Then
                Value = Size.Width
            ElseIf x <= 0 Then
                Value = 0
            Else
                Value = x
            End If
            Send(x / Size.Width)
            DoDraw(Rect)
        End If
    End Sub

    Public Overrides Sub MouseMove(ByVal e As System.Windows.Forms.MouseEventArgs)
        MyBase.MouseMove(e)
        If Not Enabled Then Return
        If e.Button = MouseButtons.Left Then
            Dim x As Integer = e.X - Position.X
            If X >= Size.Width Then
                Value = Size.Width
            ElseIf x <= 0 Then
                Value = 0
            Else
                Value = x
            End If
            Send(x / Size.Width)
            DoDraw(Rect)
        End If
    End Sub

    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        MyBase.Draw(g)

        If Enabled Then
            g.FillRectangle(Brushes.Blue, Position.X, Position.Y, Value, 20)
        Else
            g.FillRectangle(Brushes.Gray, Position.X, Position.Y, Value, 20)
        End If
        g.DrawString(Math.Round(Value / Size.Width * 100) & "%", DefaultFont, Brushes.White, Position + New Point(Size.Width * 0.5, 3))
    End Sub
End Class

'AddMenuObject|Switch,Plugins.Switch
Namespace Common
    Public Class Switch
        Inherits BaseObject

        Private Enabled As Boolean = True

        Private Value As Boolean = True


        Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
            Setup(UserData, StartPosition, 100) 'Setup the base rectangles.

            Outputs(New String() {"Value,Boolean"})
            Inputs(New String() {"Enable,Boolean", "Value,Boolean"})

            'Set the title.
            Title = "Slider"

            Value = MyBase.Size.Width * 0.5
        End Sub

        Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
            Select Case sender.Index
                Case 0 'Enable
                    Enabled = Data

                Case 1 'Set value
                    If Not Enabled Then Return
                    If Data = True Then
                        Value = Not Value
                        Send(Value)
                    End If


            End Select

            DoDraw(Rect)
        End Sub

        Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
            MyBase.Draw(g)

            g.DrawString("Value= " & Value.ToString, DefaultFont, Brushes.Black, Position)
        End Sub

        Public Overrides Sub Load(ByVal g As SimpleD.Group)

            g.Get_Value("Enabled", Enabled, False)
            g.Get_Value("Value", Value, False)
            MyBase.Load(g)
        End Sub

        Public Overrides Function Save() As SimpleD.Group
            Dim g As SimpleD.Group = MyBase.Save()

            g.Set_Value("Enabled", Enabled)
            g.Set_Value("Value", Value)


            Return g
        End Function
    End Class


End Namespace
'AddMenuObject|Timer,Plugins.Common.Timer|Math
Namespace Common
    Public Class Timer
        Inherits BaseObject

        Private WithEvents tmr As New Windows.Forms.Timer

        Private WithEvents numInterval As New NumericUpDown
        Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
            Setup(UserData, StartPosition, 85) 'Setup the base rectangles.
            File = "Common\Timer.vb"

            'Create one output.
            Outputs(New String() {"Tick"})

            Inputs(New String() {"Enable,Boolean", "Interval,Number"})

            'Set the title.
            Title = "Timer"


            numInterval.Minimum = 0
            numInterval.Maximum = 1000000
            numInterval.Width = 85
            numInterval.Location = Position
            AddControl(numInterval)


            If UserData <> "" Then
                numInterval.Value = UserData
            Else
                numInterval.Value = 1000
            End If

            tmr.Enabled = True
        End Sub

        Public Overrides Sub Dispose()
            numInterval.Dispose()
            MyBase.Dispose()
        End Sub

        Public Overrides Sub Moving()
            numInterval.Location = Position
        End Sub

        Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
            Select Case sender.Index
                Case 0 'Enable
                    tmr.Enabled = Data

                Case 1 'Interval
                    numInterval.Value = Data
            End Select
        End Sub

        Public Overrides Sub Load(ByVal g As SimpleD.Group)
            g.Get_Value("Enabled", tmr.Enabled, False)

            MyBase.Load(g)
        End Sub
        Public Overrides Function Save() As SimpleD.Group
            Dim g As SimpleD.Group = MyBase.Save()

            g.Set_Value("Enabled", tmr.Enabled)

            Return g
        End Function

        Private Sub tmr_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles tmr.Tick
            Send(Nothing)
        End Sub

        Private Sub numInterval_ValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles numInterval.ValueChanged
            tmr.Interval = numInterval.Value
            UserData = numInterval.Value
        End Sub
    End Class
End Namespace
'AddMenuObject|Axis To Boolean,Plugins.fgAxisToBoolean,85|Input
'AddReferences(SlimDX.dll)

Public Class fgAxisToBoolean
    Inherits BaseObject

    Private WithEvents numSwitchOn As New NumericUpDown

    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 60) 'Setup the base rectangles.


        'Create the inputs.
        Inputs(New String() {"Axis,Number,Axis"})
        'Create the output.
        Outputs(New String() {"Up,Boolean", "Down,Boolean"})

        'Set the title.
        Title = "Axis to boolean"

        numSwitchOn.Minimum = 0
        numSwitchOn.Maximum = 1
        numSwitchOn.Increment = 0.1
        numSwitchOn.DecimalPlaces = 2
        numSwitchOn.Value = 0.5
        numSwitchOn.Width = 60
        numSwitchOn.Location = Position
        AddControl(numSwitchOn)

        HID.Create(True)
    End Sub

    Public Overrides Sub Moving()
        numSwitchOn.Location = Position
    End Sub

    Public Overrides Sub Dispose()
        MyBase.Dispose()
        HID.Dispose(True)
        numSwitchOn.Dispose()
    End Sub

    Public LastState As Boolean = False
    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        If Data >= numSwitchOn.Value Then
            If LastState = False Then
                LastState = True
                Send(False, 0)
                Send(True, 1)
            End If
        Else
            If LastState = True Then
                LastState = False
                Send(True, 0)
                Send(False, 1)
            End If
        End If
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)
        g.Get_Value("SwitchOn", numSwitchOn.Value, False)

        MyBase.Load(g)
    End Sub
    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("SwitchOn", numSwitchOn.Value)

        Return g
    End Function

End Class


''' <summary>
''' Human interface device
''' </summary>
Public Class HID
    Public Shared DirectInput As SlimDX.DirectInput.DirectInput
    Public Shared Keyboard As SlimDX.DirectInput.Keyboard
    Public Shared Mouse As SlimDX.DirectInput.Mouse

    Private Shared Used As Integer = 0
    Private Shared UsedMice As Integer = 0
    Private Shared UsedKeyboards As Integer = 0

    Public Shared Joysticks As New List(Of JoystickInfo)

    Public Shared Sub Create(Optional ByVal CreateKeyboard As Boolean = False, Optional ByVal CreateMouse As Boolean = False)
        Used += 1
        If Used = 1 Then
            DirectInput = New SlimDX.DirectInput.DirectInput

            For Each Device As SlimDX.DirectInput.DeviceInstance In DirectInput.GetDevices(SlimDX.DirectInput.DeviceClass.GameController, SlimDX.DirectInput.DeviceEnumerationFlags.AttachedOnly)
                Joysticks.Add(New JoystickInfo(Device.InstanceName, Device))
            Next
        End If


        If CreateKeyboard Then
            UsedKeyboards += 1
            If UsedKeyboards = 1 Then
                Keyboard = New SlimDX.DirectInput.Keyboard(DirectInput)
                Keyboard.Acquire()
                Keyboard.Poll()
            End If
        End If
        If CreateMouse Then
            UsedMice += 1
            If UsedMice = 1 Then
                Mouse = New SlimDX.DirectInput.Mouse(DirectInput)
                Mouse.Acquire()
                Mouse.Poll()
            End If
        End If
    End Sub

    Public Shared Sub Dispose(Optional ByVal DisposeKeyboard As Boolean = False, Optional ByVal DisposeMouse As Boolean = False)


        If DisposeKeyboard Then
            UsedKeyboards -= 1
            If UsedKeyboards = 0 Then
                Keyboard.Unacquire()
                Keyboard.Dispose()
            End If
        End If

        If DisposeMouse Then
            UsedMice -= 1
            If UsedMice = 0 Then
                Mouse.Unacquire()
                Mouse.Dispose()
            End If
        End If

        Used -= 1
        If Used > 0 Then Return
        DirectInput.Dispose()
    End Sub
End Class
Public Class JoystickInfo
    Public Name As String
    Public Device As SlimDX.DirectInput.DeviceInstance

    Sub New(ByVal Name As String, ByVal Device As SlimDX.DirectInput.DeviceInstance)
        Me.Name = Name
        Me.Device = Device
    End Sub

    Public Overrides Function ToString() As String
        Return Name
    End Function
End Class
'AddMenuObject|Device,Plugins.fgJoystick,70|Input,Joystick
'AddMenuObject|Get Axis,Plugins.fgGetJoystickAxis,70|Input,Joystick
'AddMenuObject|Get Buttons,Plugins.fgGetJoystickButtons,70|Input,Joystick
'Include(HID\Input.vb)

Public Class fgJoystick
    Inherits BaseObject

    Public Enabled As Boolean = True

    Private WithEvents comJoy As New ComboBox

    Public Joystick As SlimDX.DirectInput.Joystick

    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 190) 'Setup the base rectangles.


        'Create the inputs.
        Inputs(New String() {"Enabled,Boolean", "Tick"})
        'Create the output.
        Outputs(New String() {"Joystick State,JoystickState"})

        'Set the title.
        Title = "Joystick"

        HID.Create()

        comJoy.Width = 190
        comJoy.Location = Position
        comJoy.DropDownStyle = ComboBoxStyle.DropDownList
        comJoy.Items.AddRange(HID.Joysticks.ToArray)
        'comJoy.SelectedIndex = 0
        AddControl(comJoy)


    End Sub

    Public Overrides Sub Moving()
        comJoy.Location = Position
    End Sub

    Public Overrides Sub Dispose()
        MyBase.Dispose()
        HID.Dispose()
        comJoy.Dispose()
        If Joystick IsNot Nothing Then
            Joystick.Unacquire()
            Joystick.Dispose()
        End If
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)

        g.Get_Value("Enabled", Enabled, False)
        'g.Get_Value("Joystick", comJoy.SelectedText, False)
        Dim tmpJoy As String = g.Get_Value("Joystick")
        If tmpJoy <> "" Then
            For i As Integer = 0 To comJoy.Items.Count - 1
                If LCase(comJoy.Items(i).ToString) = LCase(tmpJoy) Then
                    comJoy.SelectedIndex = i
                    Exit For
                End If
            Next
        End If

        MyBase.Load(g)
    End Sub
    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("Enabled", Enabled)
        If comJoy.SelectedItem IsNot Nothing Then g.Set_Value("Joystick", comJoy.SelectedItem.ToString)

        Return g
    End Function

    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Select Case sender.Index
            Case 0
                Enabled = Data

            Case 1
                If Not Enabled Then Return
                If Joystick Is Nothing Then Return
                Joystick.Poll()
                Dim state As SlimDX.DirectInput.JoystickState = Joystick.GetCurrentState
                If Not JoystickStateEquals(state, LastState) Then
                    LastState = state
                    Send(state)
                End If

        End Select
    End Sub

    Private LastState As New SlimDX.DirectInput.JoystickState

    Private Sub comJoy_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles comJoy.SelectedIndexChanged
        'Try to create the device.
        Try
            Joystick = New SlimDX.DirectInput.Joystick(HID.DirectInput, HID.Joysticks(comJoy.SelectedIndex).Device.InstanceGuid)
            Joystick.SetCooperativeLevel(Form.Handle, SlimDX.DirectInput.CooperativeLevel.Exclusive + SlimDX.DirectInput.CooperativeLevel.Background)

        Catch ex As Exception
            MsgBox("Error! Could not create joystick device.", MsgBoxStyle.Critical + MsgBoxStyle.OkOnly, "Error")
            Enabled = False
        Finally
            Joystick.Acquire()

            For Each deviceObject As SlimDX.DirectInput.DeviceObjectInstance In Joystick.GetObjects()
                If (deviceObject.ObjectType And SlimDX.DirectInput.ObjectDeviceType.Axis) <> 0 Then
                    Joystick.GetObjectPropertiesById(CInt(deviceObject.ObjectType)).SetRange(0, 10000)
                End If
            Next

            Joystick.Poll()
            LastState = Joystick.GetCurrentState

            Enabled = True
        End Try
    End Sub
End Class

Public Module JoystickHelper
    Public Function JoystickStateEquals(ByVal s1 As SlimDX.DirectInput.JoystickState, ByVal s2 As SlimDX.DirectInput.JoystickState) As Boolean
        'Still needs the rest of the values.
        If s1.X <> s2.X Then Return False
        If s1.Y <> s2.Y Then Return False
        If s1.Z <> s2.Z Then Return False

        If s1.RotationX <> s2.RotationX Then Return False
        If s1.RotationY <> s2.RotationY Then Return False
        If s1.RotationZ <> s2.RotationZ Then Return False

        If s1.GetSliders(0) <> s2.GetSliders(0) Then Return False
        If s1.GetSliders(1) <> s2.GetSliders(1) Then Return False

        For i As Integer = 0 To 127
            If s1.IsPressed(i) <> s2.IsPressed(i) Then Return False
        Next


        Return True
    End Function
End Module

Public Class fgGetJoystickAxis
    Inherits BaseObject

    Private chkReverse(7) As CheckBox
    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 50, 125) 'Setup the base rectangles.


        'Create the inputs.
        Inputs(New String() {"Joystick State,JoystickState"})
        'Create the output.
        Outputs(New String() {"X,Number,Axis", "Y,Number,Axis", "Z,Number,Axis", _
                              "RotationX,Number,Axis", "RotationY,Number,Axis", "RotationZ,Number,Axis", _
                              "Slider1,Number,Axis", "Slider2,Number,Axis"})

        'Set the title.
        Title = "Joystick Axis"

        For i As Integer = 0 To 7
            Dim chk As New CheckBox
            chk.Text = "Rev"
            chk.Width = 46
            chk.Height = 15
            chk.Tag = i
            chk.Location = Position + New Point(0, (15 * i))
            AddHandler chk.CheckedChanged, AddressOf ReverseChange

            chkReverse(i) = chk
            AddControl(chk)
        Next
    End Sub

    Public Overrides Sub Moving()
        For i As Integer = 0 To 7
            chkReverse(i).Location = Position + New Point(4, (15 * i))
        Next
    End Sub

    Public Overrides Sub Dispose()
        MyBase.Dispose()


        For Each chk As CheckBox In chkReverse
            chk.Dispose()
        Next
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)
        For Each chk As CheckBox In chkReverse
            g.Get_Value(chk.Text & chk.Tag, chk.Checked, False)
        Next

        MyBase.Load(g)
    End Sub

    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        'Save all the reverse check boxs state.
        For Each chk As CheckBox In chkReverse
            g.Set_Value(chk.Text & chk.Tag, chk.Checked, False)
        Next

        Return g
    End Function

    Public Sub ReverseChange(ByVal sender As Object, ByVal e As EventArgs)
        Select Case sender.tag
            Case 0
                SendAxis(LastState.X, 0)
            Case 1
                SendAxis(LastState.Y, 1)
            Case 2
                SendAxis(LastState.Z, 2)

            Case 3
                SendAxis(LastState.RotationX, 3)
            Case 4
                SendAxis(LastState.RotationY, 4)
            Case 5
                SendAxis(LastState.RotationZ, 5)

            Case 6
                SendAxis(LastState.GetSliders(0), 6)
            Case 7
                SendAxis(LastState.GetSliders(1), 7)
        End Select
    End Sub

    Private LastState As New SlimDX.DirectInput.JoystickState
    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Dim state As SlimDX.DirectInput.JoystickState = DirectCast(Data, SlimDX.DirectInput.JoystickState)

        If Not state.Equals(LastState) Then

            'Get axis.
            If state.X <> LastState.X Then SendAxis(state.X, 0)
            If state.Y <> LastState.Y Then SendAxis(state.Y, 1)
            If state.Z <> LastState.Z Then SendAxis(state.Z, 2)

            If state.RotationX <> LastState.RotationX Then SendAxis(state.RotationX, 3)
            If state.RotationY <> LastState.RotationY Then SendAxis(state.RotationY, 4)
            If state.RotationZ <> LastState.RotationZ Then SendAxis(state.RotationZ, 5)

            If state.GetSliders(0) <> LastState.GetSliders(0) Then SendAxis(state.GetSliders(0), 6)
            If state.GetSliders(1) <> LastState.GetSliders(1) Then SendAxis(state.GetSliders(1), 7)


            LastState = state
        End If

    End Sub

    Public Sub SendAxis(ByVal Axis As Integer, ByVal ID As Integer)
        If chkReverse(ID).Checked = False Then
            Send(Axis * 0.0001, ID)
            Output(ID).Note = "Axis=" & Axis * 0.0001
        Else
            Send((-Axis * 0.0001) + 1, ID)
            Output(ID).Note = "Axis=" & (-Axis * 0.0001) + 1
        End If

    End Sub

End Class

Public Class fgGetJoystickButtons
    Inherits BaseObject

    Private WithEvents numButtons As New NumericUpDown

    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 60) 'Setup the base rectangles.


        'Create the inputs.
        Inputs(New String() {"Joystick State,JoystickState"})
        'Create the output.
        Outputs(New String() {"Released,Boolean", "Pressed,Boolean"})

        'Set the title.
        Title = "Joystick Buttons"

        numButtons.Minimum = 0
        numButtons.Maximum = 1000
        numButtons.Width = 60
        numButtons.Location = Position
        AddControl(numButtons)

    End Sub

    Public Overrides Sub Moving()
        numButtons.Location = Position
    End Sub

    Public Overrides Sub Dispose()
        MyBase.Dispose()
        numButtons.Dispose()
    End Sub

    Public LastState As Boolean = False
    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Dim state As SlimDX.DirectInput.JoystickState = DirectCast(Data, SlimDX.DirectInput.JoystickState)
        numButtons.Maximum = state.GetButtons.Length - 1

        If state.GetButtons(numButtons.Value) <> LastState Then
            If state.GetButtons(numButtons.Value) = 0 Then
                Send(True, 0)
                Send(False, 1)
                LastState = False
            Else
                Send(False, 0)
                Send(True, 1)
                LastState = True
            End If

        End If

    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)
        g.Get_Value("Button", numButtons.Value, False)

        MyBase.Load(g)
    End Sub
    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("Button", numButtons.Value)

        Return g
    End Function

End Class

'AddMenuObject|Get key,Plugins.fgGetKey,70|Input,Keyboard
'AddMenuObject|Device,Plugins.fgKeyboard,70|Input,Keyboard
'Include(HID\Input.vb)
'Include(Base\Plugins.vb,Base\BaseObject.vb)
Public Class fgGetKey
    Inherits BaseObject

    Public Enabled As Boolean = True

    Public WithEvents comKey As New ComboBox

    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 120) 'Setup the base rectangles.
        File = "HID\Keyboard.vb"

        'Create the inputs.
        Inputs(New String() {"Enabled,Boolean", "Tick", "Keyboard State,KeyboardState"})
        'Create the output.
        Outputs(New String() {"Released,Boolean", "Pressed,Boolean"})

        'Set the title.
        Title = "Get key"


        comKey.Location = Position
        comKey.Items.AddRange([Enum].GetNames(GetType(SlimDX.DirectInput.Key)))
        comKey.SelectedItem = SlimDX.DirectInput.Key.Pause.ToString
        comKey.DropDownStyle = ComboBoxStyle.DropDownList
        comKey.Width = Size.Width
        AddControl(comKey)

        HID.Create(True)
    End Sub

    Public Overrides Sub Moving()
        comKey.Location = Position
    End Sub

    Public Overrides Sub Dispose()
        MyBase.Dispose()
        HID.Dispose(True)
        comKey.Dispose()
    End Sub

    Public LastState As Boolean = False
    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        MyBase.Receive(Data, sender)

        Select Case sender.Index
            Case 0
                Enabled = Data

            Case 1
                If Not Enabled Then Return
                HID.Keyboard.Poll()
                If HID.Keyboard.GetCurrentState.IsPressed([Enum].Parse(GetType(SlimDX.DirectInput.Key), comKey.SelectedItem.ToString)) Then
                    If LastState = False Then
                        Send(False, 0)
                        Send(True, 1)
                        LastState = True
                    End If

                Else
                    If LastState = True Then
                        Send(True, 0)
                        Send(False, 1)
                        LastState = False
                    End If
                End If

            Case 2
                If Not Enabled Then Return
                If DirectCast(Data, SlimDX.DirectInput.KeyboardState).IsPressed([Enum].Parse(GetType(SlimDX.DirectInput.Key), comKey.SelectedItem.ToString)) Then
                    If LastState = False Then
                        Send(False, 0)
                        Send(True, 1)
                        LastState = True
                    End If

                Else
                    If LastState = True Then
                        Send(True, 0)
                        Send(False, 1)
                        LastState = False
                    End If
                End If

        End Select
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)
        g.Get_Value("Enabled", Enabled, False)
        g.Get_Value("Key", comKey.SelectedItem, False)

        MyBase.Load(g)
    End Sub
    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("Enabled", Enabled)
        g.Set_Value("Key", comKey.SelectedItem)

        Return g
    End Function

End Class

Public Class fgKeyboard
    Inherits BaseObject

    Public Enabled As Boolean = True

    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 30) 'Setup the base rectangles.

        'Create the inputs.
        Inputs(New String() {"Enabled,Boolean", "Tick"})
        'Create the output.
        Outputs(New String() {"Keyboard State,KeyboardState"})

        'Set the title.
        Title = "Keyboard"

        HID.Create(True)
    End Sub

    Public Overrides Sub Dispose()
        MyBase.Dispose()
        HID.Dispose(True)
    End Sub

    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        MyBase.Receive(Data, sender)

        Select Case sender.Index
            Case 0
                Enabled = Data

            Case 1
                If Not Enabled Then Return
                HID.Keyboard.Poll()
                Send(HID.Keyboard.GetCurrentState)

        End Select
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)
        g.Get_Value("Enabled", Enabled, False)

        MyBase.Load(g)
    End Sub
    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("Enabled", Enabled)

        Return g
    End Function
End Class
'AddMenuObject|Raw Mouse,Plugins.fgRawMouse,60|Input,Mouse
'AddMenuObject|Local Mouse,Plugins.fgLocalMouse,65|Input,Mouse
'AddMenuObject|Global Mouse,Plugins.fgGlobalMouse,75|Input,Mouse
Public Class fgRawMouse
    Inherits BaseObject

    Public Enabled As Boolean = True

    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 40) 'Setup the base rectangles.

        'Create the inputs.
        Inputs(New String() {"Enabled,Boolean", "Tick"})
        'Create the output.
        Outputs(New String() {"Mouse State,MouseState", "X,Number", "Y,Number"})

        'Set the title.
        Title = "Raw Mouse"

        HID.Create(, True)
    End Sub

    Public Overrides Sub Dispose()
        MyBase.Dispose()
        HID.Dispose(, True)
    End Sub

    Private LastState As New SlimDX.DirectInput.MouseState
    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        MyBase.Receive(Data, sender)

        Select Case sender.Index
            Case 0
                Enabled = Data

            Case 1
                If Not Enabled Then Return
                HID.Mouse.Poll()
                Dim state As SlimDX.DirectInput.MouseState = HID.Mouse.GetCurrentState
                If StateChanged(state) Then
                    Send(state, 0)
                End If
                If state.X <> LastState.X Then
                    Send(state.X, 1)
                End If
                If state.Y <> LastState.Y Then
                    Send(state.Y, 2)
                End If

                LastState = state
        End Select
    End Sub

    Private Function StateChanged(ByVal State As SlimDX.DirectInput.MouseState) As Boolean
        If State.X <> LastState.X Then Return True
        If State.Y <> LastState.Y Then Return True
        If State.Z <> LastState.Z Then Return True

        For i As Integer = 0 To State.GetButtons.Length - 1
            If State.GetButtons(i) <> LastState.GetButtons(i) Then Return True
        Next
        Return False
    End Function

    Public Overrides Sub Load(ByVal g As SimpleD.Group)
        g.Get_Value("Enabled", Enabled, False)

        MyBase.Load(g)
    End Sub
    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("Enabled", Enabled)

        Return g
    End Function
End Class

Public Class fgLocalMouse
    Inherits BaseObject

    Public Enabled As Boolean = True

    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 40) 'Setup the base rectangles.

        'Create the inputs.
        Inputs(New String() {"Enabled,Boolean", "Tick"})
        'Create the output.
        Outputs(New String() {"Position,Point", "X,Number", "Y,Number"})

        'Set the title.
        Title = "Local Mouse"
    End Sub

    Public Overrides Sub Dispose()
        MyBase.Dispose()
    End Sub

    Private LastState As Point
    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        MyBase.Receive(Data, sender)

        Select Case sender.Index
            Case 0
                Enabled = Data

            Case 1
                If Not Enabled Then Return

                Dim state As Point = Mouse.Location
                If state <> LastState Then
                    Send(state, 0)
                End If
                If state.X <> LastState.X Then
                    Send(state.X, 1)
                End If
                If state.Y <> LastState.Y Then
                    Send(state.Y, 2)
                End If

                LastState = state
        End Select
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)
        g.Get_Value("Enabled", Enabled, False)

        MyBase.Load(g)
    End Sub
    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("Enabled", Enabled)

        Return g
    End Function
End Class

Public Class fgGlobalMouse
    Inherits BaseObject

    Public Enabled As Boolean = True

    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 40) 'Setup the base rectangles.

        'Create the inputs.
        Inputs(New String() {"Enabled,Boolean", "Tick"})
        'Create the output.
        Outputs(New String() {"Position,Point", "X,Number,Axis", "Y,Number,Axis"})

        'Set the title.
        Title = "Global Mouse"
    End Sub

    Public Overrides Sub Dispose()
        MyBase.Dispose()
    End Sub

    Private LastState As Point
    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        MyBase.Receive(Data, sender)

        Select Case sender.Index
            Case 0
                Enabled = Data

            Case 1
                If Not Enabled Then Return

                Dim state As Point = Cursor.Position
                If state <> LastState Then
                    Send(state, 0)
                End If
                If state.X <> LastState.X Then
                    Send(state.X, 1)
                End If
                If state.Y <> LastState.Y Then
                    Send(state.Y, 2)
                End If

                LastState = state
        End Select
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)
        g.Get_Value("Enabled", Enabled, False)

        MyBase.Load(g)
    End Sub
    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("Enabled", Enabled)

        Return g
    End Function
End Class
'AddMenuObject|Debug,Plugins.MIDI_Debug|MIDI,Channel Message
'AddReferences(Sanford.Slim.dll)

Public Class MIDI_Debug
    Inherits BaseObject

    Private Enabled As Boolean = True

    Private Items As New List(Of String)
    Private StartID As Short = -1

#Region "Object stuff"
    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 200, 145) 'Setup the base rectangles.

        Inputs(New String() {"Enable,Boolean", "Channel Message,ChannelMessage,ChannelMessageBuilder"})

        'Set the title.
        Title = "MIDI Debug"


    End Sub


    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Select Case sender.Index
            Case 0 'Enable
                If Data <> Enabled Then
                    Enabled = Data
                End If


            Case 1 'Channel message
                If Not Enabled Then Return

                If Items.Count = 13 Then
                    Items.RemoveAt(0)
                    StartID += 1
                    If StartID > 3 Then StartID = 1
                End If

                Items.Add(Data.MidiChannel + 1 & vbTab & _
                                  Data.Command.ToString.PadRight(15) & vbTab & _
                                  Data.Data1.ToString & vbTab & _
                                  Data.Data2.ToString)

                DoDraw(Rect)

        End Select
    End Sub

    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        MyBase.Draw(g)

        'Draw the background colors.


        Dim ColorID As Short = StartID
        'Draw the debug items.
        For i As Integer = 0 To Items.Count - 1
            ColorID += 1
            If ColorID > 3 Then ColorID = 1
            Select Case ColorID
                Case 1
                    g.FillRectangle(Brushes.LightGray, Position.X, Position.Y + (11 * i), Size.Width, 11)

                Case 2
                    g.FillRectangle(Brushes.LightSkyBlue, Position.X, Position.Y + (11 * i), Size.Width, 11)
                Case 3
                    g.FillRectangle(Brushes.LightCyan, Position.X, Position.Y + (11 * i), Size.Width, 11)
            End Select


            g.DrawString(Items(i), DefaultFont, Brushes.Black, Position + New Point(0, 11 * i))
        Next
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)

        g.Get_Value("Enabled", Enabled, False)


        MyBase.Load(g)
    End Sub

    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("Enabled", Enabled)

        Return g
    End Function
#End Region

End Class

'AddMenuObject|Get Controller,Plugins.MIDI_GetController|MIDI,Channel Message
'AddReferences(Sanford.Slim.dll)
Public Class MIDI_GetController
    Inherits BaseObject

    Private Enabled As Boolean = True


    Private numChannel As New NumericUpDown
    Private WithEvents chkChannels As New CheckBox

    Private WithEvents comController As New ComboBox
    Private Controller As Integer

#Region "Object stuff"
    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 200, 50) 'Setup the base rectangles.

        'Create one output.
        Outputs(New String() {"Value,Number,Boolean"})
        Inputs(New String() {"Enable,Boolean", "Channel Message,ChannelMessage,ChannelMessageBuilder"})

        'Set the title.
        Title = "Get controller"

        numChannel.Minimum = 1
        numChannel.Maximum = 16
        numChannel.Width = 40
        numChannel.Enabled = False
        numChannel.Location = Position + New Point(45, 25)
        AddControl(numChannel)

        chkChannels.Text = "Any channel"
        chkChannels.Width = 113
        chkChannels.Checked = True
        chkChannels.Location = Position + New Point(86, 24)
        AddControl(chkChannels)

        comController.Width = 200
        comController.Location = Position
        comController.DropDownStyle = ComboBoxStyle.DropDownList
        comController.Items.AddRange([Enum].GetNames(GetType(Sanford.Multimedia.Midi.ControllerType)))
        comController.SelectedItem = Sanford.Multimedia.Midi.ControllerType.HoldPedal1.ToString
        AddControl(comController)

    End Sub

    Public Overrides Sub Dispose()
        numChannel.Dispose()
        chkChannels.Dispose()
        comController.Dispose()

        MyBase.Dispose()
    End Sub

    Public Overrides Sub Moving()
        numChannel.Location = Position + New Point(45, 25)
        chkChannels.Location = Position + New Point(86, 24)
        comController.Location = Position
    End Sub

    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Select Case sender.Index
            Case 0 'Enable
                If Data <> Enabled Then
                    Enabled = Data
                End If


            Case 1 'Channel message
                If Not Enabled Then Return
                If chkChannels.Checked Then
                    If Data.MidiChannel <> numChannel.Value - 1 Then
                        Return
                    End If
                End If

                If Data.Command = Sanford.Multimedia.Midi.ChannelCommand.Controller And Data.Data1 = Controller Then
                    Send(Data.Data2 / 127)
                End If
        End Select
    End Sub

    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        MyBase.Draw(g)

        g.DrawString("Channel:", DefaultFont, DefaultFontBrush, Position.X, Position.Y + 28)
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)

        g.Get_Value("Enabled", Enabled, False)

        Dim tmpController As Integer = -1
        g.Get_Value("Controller", tmpController, False)
        If Not tmpController = -1 Then
            comController.SelectedItem = [Enum].GetName(GetType(Sanford.Multimedia.Midi.ControllerType), tmpController)
        End If

        g.Get_Value("AnyChannels", chkChannels.Checked, False)

        g.Get_Value("Channel", numChannel.Value, False)

        MyBase.Load(g)
    End Sub

    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("Enabled", Enabled)
        g.Set_Value("Controller", Controller)
        g.Set_Value("AnyChannels", chkChannels.Checked)
        g.Set_Value("Channel", numChannel.Value)


        Return g
    End Function
#End Region

#Region "Control events"
    Private Sub chkChannels_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkChannels.CheckedChanged
        numChannel.Enabled = Not chkChannels.Checked
    End Sub

    Private Sub comController_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles comController.SelectedIndexChanged
        If comController.SelectedIndex = -1 Then Return
        Controller = [Enum].Parse(GetType(Sanford.Multimedia.Midi.ControllerType), comController.SelectedItem.ToString)
    End Sub
#End Region

End Class

'AddMenuObject|Input,Plugins.MIDI_Input|MIDI
'AddReferences(Sanford.Slim.dll)
Public Class MIDI_Input
    Inherits BaseObject

    Private Enabled As Boolean = True

    Private WithEvents Device As Sanford.Multimedia.Midi.InputDevice

    Private numChannel As New NumericUpDown
    Private WithEvents chkAllChannels As New CheckBox

    Private WithEvents comDevices As New ComboBox

#Region "Object stuff"
    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 205) 'Setup the base rectangles.

        'Create one output.
        Outputs(New String() {"Channel Message,ChannelMessage", "SysCommonMessage,SysCommonMessage", "SysExMessage,SysExMessage", "SysRealtimeMessage,SysRealtimeMessage"})

        Inputs(New String() {"Enable,Boolean", "Channel,Number"})

        'Set the title.
        Title = "MIDI Input"

        chkAllChannels.Text = "All channels"
        chkAllChannels.Checked = True
        chkAllChannels.Width = 85
        chkAllChannels.Location = Position + New Point(95, 25)
        AddControl(chkAllChannels)


        numChannel.Minimum = 1
        numChannel.Maximum = 16
        numChannel.Width = 40
        numChannel.Location = Position + New Point(45, 25)
        AddControl(numChannel)



        If Sanford.Multimedia.Midi.InputDevice.DeviceCount > 0 Then
            comDevices.Width = 200
            comDevices.Location = Position
            comDevices.DropDownStyle = ComboBoxStyle.DropDownList

            For i As Integer = 0 To Sanford.Multimedia.Midi.InputDevice.DeviceCount - 1
                comDevices.Items.Add(Sanford.Multimedia.Midi.InputDevice.GetDeviceCapabilities(i).name)
            Next

            AddControl(comDevices)
        Else
            MsgBox("Could not find any MIDI input devices!", MsgBoxStyle.Critical, "Error")
        End If

    End Sub

    Public Overrides Sub Dispose()
        chkAllChannels.Dispose()
        numChannel.Dispose()
        comDevices.Dispose()

        If Device IsNot Nothing Then
            Device.Close()
            Device = Nothing
        End If
        MyBase.Dispose()
    End Sub

    Public Overrides Sub Moving()
        chkAllChannels.Location = Position + New Point(95, 25)
        numChannel.Location = Position + New Point(45, 25)
        comDevices.Location = Position
    End Sub

    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Select Case sender.Index
            Case 0 'Enable
                If Data <> Enabled Then
                    Enabled = Data

                    If Device IsNot Nothing Then
                        If Enabled = True Then
                            Device.StartRecording()
                        Else
                            Device.StopRecording()
                        End If
                    End If
                End If


            Case 1 'Channel
                numChannel.Value = Data
        End Select
    End Sub

    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        MyBase.Draw(g)

        g.DrawString("Channel:", DefaultFont, DefaultFontBrush, Position.X, Position.Y + 28)
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)

        g.Get_Value("Enabled", Enabled, False)
        g.Get_Value("DeviceID", comDevices.SelectedIndex)
        g.Get_Value("AllChannels", chkAllChannels.Checked)
        Try
            g.Get_Value("Channel", numChannel.Value)
        Catch ex As Exception
        End Try


        MyBase.Load(g)
    End Sub

    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("Enabled", Enabled)
        g.Set_Value("DeviceID", comDevices.SelectedIndex)
        g.Set_Value("AllChannels", chkAllChannels.Checked)
        g.Set_Value("Channel", numChannel.Value)


        Return g
    End Function

#End Region

#Region "Control events"

    Private Sub comDevices_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles comDevices.SelectedIndexChanged
        If comDevices.SelectedIndex = -1 Then Return
        If Device IsNot Nothing Then
               Device.Close()
            Device = Nothing
        End If


        Try
            'Create the device.
            Device = New Sanford.Multimedia.Midi.InputDevice(comDevices.SelectedIndex)
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical, "Error!")
            Device = Nothing
        End Try

        If Enabled Then
            Device.StartRecording()
        End If
    End Sub

    Private Sub chkAllChannels_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkAllChannels.CheckedChanged
        numChannel.Enabled = Not chkAllChannels.Checked
    End Sub
#End Region

#Region "MIDI events"
    Private Sub Device_Error(ByVal sender As Object, ByVal e As Sanford.Multimedia.ErrorEventArgs) Handles Device.Error
        MsgBox(e.Error.Message)
    End Sub

    Private Sub Device_ChannelMessageReceived(ByVal sender As Object, ByVal e As Sanford.Multimedia.Midi.ChannelMessageEventArgs) Handles Device.ChannelMessageReceived
        If Not chkAllChannels.Checked Then
            If Not e.Message.MidiChannel = numChannel.Value - 1 Then Return
        End If

        Send(e.Message, 0)
    End Sub

    Private Sub Device_SysCommonMessageReceived(ByVal sender As Object, ByVal e As Sanford.Multimedia.Midi.SysCommonMessageEventArgs) Handles Device.SysCommonMessageReceived
        Send(e.Message, 1)
    End Sub

    Private Sub Device_SysExMessageReceived(ByVal sender As Object, ByVal e As Sanford.Multimedia.Midi.SysExMessageEventArgs) Handles Device.SysExMessageReceived
        Send(e.Message, 2)
    End Sub

    Private Sub Device_SysRealtimeMessageReceived(ByVal sender As Object, ByVal e As Sanford.Multimedia.Midi.SysRealtimeMessageEventArgs) Handles Device.SysRealtimeMessageReceived
        Send(e.Message, 3)
    End Sub
#End Region

End Class

'AddMenuObject|128keys,Plugins.MIDI_Keyboard,120,128-0-0|MIDI,Keyboards
'AddMenuObject|88 keys,Plugins.MIDI_Keyboard,120,88-21-9|MIDI,Keyboards
'AddMenuObject|61 keys,Plugins.MIDI_Keyboard,120,61-36-0|MIDI,Keyboards
'AddMenuObject|24 keys,Plugins.MIDI_Keyboard,120,24-60-0|MIDI,Keyboards
'AddReferences(Sanford.Slim.dll)

Public Class MIDI_Keyboard
    Inherits BaseObject

    Private Enabled As Boolean = True


    Private numChannel As New NumericUpDown
    Private chkFilterOtherChannels As New CheckBox

    Private NumKeys As Byte
    Private Offset, OctaveOffset As Byte
    Private Width As Integer

#Region "Object stuff"
    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Try
            Dim data() As String = Split(UserData, "-")
            NumKeys = data(0)
            Offset = data(1)
            OctaveOffset = data(2)
        Catch ex As Exception
            NumKeys = 24
            Offset = 60
            OctaveOffset = 0
        End Try
        Width = NumKeys * 5.9

        Setup(UserData, StartPosition, Width, 95) 'Setup the base rectangles.

        'Create one output.
        Outputs(New String() {"Channel Message,ChannelMessageBuilder"})
        Inputs(New String() {"Enable,Boolean", "Channel Message,ChannelMessage,ChannelMessageBuilder"})


        'Set the title.
        Title = "MIDI Keyboard"

        chkFilterOtherChannels.Text = "Filter out other channels"
        chkFilterOtherChannels.Width = 139
        chkFilterOtherChannels.Height = 15
        chkFilterOtherChannels.Checked = False
        chkFilterOtherChannels.Location = Position + New Point(5, 19)
        AddControl(chkFilterOtherChannels)

        numChannel.Minimum = 1
        numChannel.Maximum = 16
        numChannel.Width = 40
        numChannel.Location = Position + New Point(55, 0)
        AddControl(numChannel)

        ReDim Note(NumKeys - 1)
        For i As Integer = 0 To Note.Length - 1
            Note(i) = New NoteT
        Next
    End Sub

    Public Overrides Sub Dispose()
        chkFilterOtherChannels.Dispose()
        numChannel.Dispose()

        MyBase.Dispose()
    End Sub

    Public Overrides Sub Moving()
        chkFilterOtherChannels.Location = Position + New Point(5, 19)
        numChannel.Location = Position + New Point(55, 0)
    End Sub

    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        MyBase.Draw(g)

        g.DrawString("Channel:", DefaultFont, DefaultFontBrush, Position.X + 5, Position.Y + 3)

        g.FillRectangle(Brushes.White, Position.X, Position.Y + 40, Width, 50)

        Dim OctavePos As SByte = OctaveOffset - 1
        Dim x As Integer = Position.X
        Dim y As Integer = Position.Y + 40
        Dim p As SByte = -1

        For n As Integer = 0 To NumKeys - 1
            OctavePos += 1
            If OctavePos = 12 Then OctavePos = 0
            Select Case OctavePos
                Case 0, 2, 4, 5, 7, 9, 11
                    p += 1
            End Select

            Dim Note As NoteT = Me.Note(n)
            Dim brush As Brush = Brushes.Black
            If Not chkFilterOtherChannels.Checked Then
                If Note.Channel(0) Then
                    brush = Brushes.Blue
                ElseIf Note.Channel(1) Then
                    brush = Brushes.Aqua
                ElseIf Note.Channel(2) Then
                    brush = Brushes.Green
                ElseIf Note.Channel(3) Then
                    brush = Brushes.Red
                ElseIf Note.Channel(4) Then
                    brush = Brushes.Purple
                ElseIf Note.Channel(5) Then
                    brush = Brushes.Brown
                ElseIf Note.Channel(6) Then
                    brush = Brushes.Gray
                ElseIf Note.Channel(7) Then
                    brush = Brushes.Orange
                ElseIf Note.Channel(8) Then
                    brush = Brushes.Teal
                ElseIf Note.Channel(9) Then
                    brush = Brushes.Yellow
                ElseIf Note.Channel(10) Then
                    brush = Brushes.BlueViolet
                ElseIf Note.Channel(11) Then
                    brush = Brushes.LawnGreen
                ElseIf Note.Channel(12) Then
                    brush = Brushes.Pink
                ElseIf Note.Channel(13) Then
                    brush = Brushes.Tan
                ElseIf Note.Channel(14) Then
                    brush = Brushes.DarkGreen
                ElseIf Note.Channel(15) Then
                    brush = Brushes.Coral
                End If
            Else
                If Note.Channel(numChannel.Value - 1) Then
                    brush = Brushes.Blue
                End If
            End If


            Select Case OctavePos
                Case 0, 5 'C & F
                    If brush IsNot Brushes.Black Then
                        g.FillRectangle(brush, x + (10 * p), y, 6, 25)
                        g.FillRectangle(brush, x + (10 * p), y + 25, 10, 25)
                    End If

                Case 1, 6 'C# & F#
                    g.FillRectangle(brush, x + (10 * p) + 6, y, 6, 25)

                Case 2 'D
                    If brush IsNot Brushes.Black Then
                        g.FillRectangle(brush, x + (10 * p) + 2, y, 5, 25)
                        g.FillRectangle(brush, x + (10 * p), y + 25, 10, 25)
                    End If

                Case 3, 10 'D# & A#
                    g.FillRectangle(brush, x + (10 * p) + 7, y, 6, 25)

                Case 4, 11 'E & B
                    If brush IsNot Brushes.Black Then
                        g.FillRectangle(brush, x + (10 * p) + 3, y, 7, 25)
                        g.FillRectangle(brush, x + (10 * p), y + 25, 10, 25)
                    End If

                Case 7 'G
                    If brush IsNot Brushes.Black Then
                        g.FillRectangle(brush, x + (10 * p) + 2, y, 5, 25)
                        g.FillRectangle(brush, x + (10 * p), y + 25, 10, 25)
                    End If

                Case 8 'G#
                    g.FillRectangle(brush, x + (10 * p) + 7, y, 6, 25)

                Case 9 'A
                    If brush IsNot Brushes.Black Then
                        g.FillRectangle(brush, x + (10 * p) + 2, y, 5, 25)
                        g.FillRectangle(brush, x + (10 * p), y + 25, 10, 25)
                    End If
            End Select
        Next

    End Sub

    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Select Case sender.Index
            Case 0 'Enable
                If Data <> Enabled Then
                    Enabled = Data
                End If


            Case 1 'ChannelMessage
                If Not Enabled Then Return
                If Data.MidiChannel <> numChannel.Value - 1 Then
                    If chkFilterOtherChannels.Checked Then
                        Return
                    End If
                End If

                Dim message As Sanford.Multimedia.Midi.ChannelMessageBuilder
                If Data.GetType = GetType(Sanford.Multimedia.Midi.ChannelMessage) Then
                    message = New Sanford.Multimedia.Midi.ChannelMessageBuilder(Data)
                Else
                    message = Data
                End If

               

                Dim NoteOn As Boolean = False

                'Is it a note (on or off)?
                If (message.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOn) Then
                    If message.Data2 > 0 Then
                        NoteOn = True
                    Else
                        NoteOn = False
                    End If

                ElseIf message.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOff Then
                    NoteOn = False


                ElseIf message.Command = Sanford.Multimedia.Midi.ChannelCommand.Controller And _
                    (message.Data1 = Sanford.Multimedia.Midi.ControllerType.AllSoundOff Or message.Data1 = Sanford.Multimedia.Midi.ControllerType.AllNotesOff) Then
                    ResetNotes()
                    GoTo Send
                End If

                'Check to see if the note is within the keyboard size.
                If message.Data1 >= Offset And message.Data1 + 1 <= Offset + NumKeys Then
                    'Is the note on? (volume more then 0)
                    If NoteOn Then
                        Note(message.Data1 - Offset).Press(message.MidiChannel)
                    Else
                        Note(message.Data1 - Offset).Release(message.MidiChannel)
                    End If
                End If




Send:
                Send(message)
                DoDraw(Rect)
                'DoDraw(True)
        End Select
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)

        g.Get_Value("Enabled", Enabled, False)
        g.Get_Value("FilterOtherChannels", chkFilterOtherChannels.Checked, False)
        g.Get_Value("Channel", numChannel.Value, False)

        MyBase.Load(g)
    End Sub

    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("Enabled", Enabled, True)
        g.Set_Value("FilterOtherChannels", chkFilterOtherChannels.Checked, False)
        g.Set_Value("Channel", numChannel.Value, 1)


        Return g
    End Function

    Public Overrides Sub MouseDown(ByVal e As System.Windows.Forms.MouseEventArgs)
        'Is the mouse over the keyboard?
        Dim x As Integer = Position.X
        Dim y As Integer = Position.Y + 40
        If Mouse.IntersectsWith(New Rectangle(x, y, Width, 50)) And Enabled Then
            Dim OctavePos As SByte = OctaveOffset - 1
            Dim p As SByte = -1

            For n As Integer = 0 To NumKeys - 1
                OctavePos += 1
                If OctavePos = 12 Then OctavePos = 0
                Select Case OctavePos
                    Case 0, 2, 4, 5, 7, 9, 11
                        p += 1
                End Select

                Select Case OctavePos
                    Case 0, 5 'C & F
                        If Mouse.IntersectsWith(New Rectangle(x + (10 * p), y, 6, 25)) Or _
                            Mouse.IntersectsWith(New Rectangle(x + (10 * p), y + 25, 10, 25)) Then
                            PressNote(n)
                            Return
                        End If

                    Case 1, 6 'C# & F#
                        If Mouse.IntersectsWith(New Rectangle(x + (10 * p) + 6, y, 6, 25)) Then
                            PressNote(n)
                            Return
                        End If


                    Case 2 'D
                        If Mouse.IntersectsWith(New Rectangle(x + (10 * p) + 2, y, 5, 25)) Or _
                            Mouse.IntersectsWith(New Rectangle(x + (10 * p), y + 25, 10, 25)) Then
                            PressNote(n)
                            Return
                        End If

                    Case 3, 10 'D# & A#
                        If Mouse.IntersectsWith(New Rectangle(x + (10 * p) + 7, y, 6, 25)) Then
                            PressNote(n)
                            Return
                        End If

                    Case 4, 11 'E & B
                        If Mouse.IntersectsWith(New Rectangle(x + (10 * p) + 3, y, 7, 25)) Or _
                            Mouse.IntersectsWith(New Rectangle(x + (10 * p), y + 25, 10, 25)) Then
                            PressNote(n)
                            Return
                        End If

                    Case 7 'G
                        If Mouse.IntersectsWith(New Rectangle(x + (10 * p) + 2, y, 5, 25)) Or _
                            Mouse.IntersectsWith(New Rectangle(x + (10 * p), y + 25, 10, 25)) Then
                            PressNote(n)
                            Return
                        End If

                    Case 8 'G#
                        If Mouse.IntersectsWith(New Rectangle(x + (10 * p) + 7, y, 6, 25)) Then
                            PressNote(n)
                            Return
                        End If

                    Case 9 'A
                        If Mouse.IntersectsWith(New Rectangle(x + (10 * p) + 2, y, 5, 25)) Or _
                            Mouse.IntersectsWith(New Rectangle(x + (10 * p), y + 25, 10, 25)) Then
                            PressNote(n)
                            Return
                        End If


                End Select
            Next

            Return
        End If
        MyBase.MouseDown(e)
    End Sub
    Public Overrides Sub MouseUp(ByVal e As System.Windows.Forms.MouseEventArgs)
        'Is the mouse over the keyboard?
        Dim x As Integer = Position.X
        Dim y As Integer = Position.Y + 40
        If Mouse.IntersectsWith(New Rectangle(x, y, Width, 50)) And Enabled Then
            Dim OctavePos As SByte = OctaveOffset - 1
            Dim p As SByte = -1

            For n As Integer = 0 To NumKeys - 1
                OctavePos += 1
                If OctavePos = 12 Then OctavePos = 0
                Select Case OctavePos
                    Case 0, 2, 4, 5, 7, 9, 11
                        p += 1
                End Select

                Select Case OctavePos
                    Case 0, 5 'C & F
                        If Mouse.IntersectsWith(New Rectangle(x + (10 * p), y, 6, 25)) Or _
                            Mouse.IntersectsWith(New Rectangle(x + (10 * p), y + 25, 10, 25)) Then
                            ReleaseNote(n)
                            Return
                        End If

                    Case 1, 6 'C# & F#
                        If Mouse.IntersectsWith(New Rectangle(x + (10 * p) + 6, y, 6, 25)) Then
                            ReleaseNote(n)
                            Return
                        End If


                    Case 2 'D
                        If Mouse.IntersectsWith(New Rectangle(x + (10 * p) + 2, y, 5, 25)) Or _
                            Mouse.IntersectsWith(New Rectangle(x + (10 * p), y + 25, 10, 25)) Then
                            ReleaseNote(n)
                            Return
                        End If

                    Case 3, 10 'D# & A#
                        If Mouse.IntersectsWith(New Rectangle(x + (10 * p) + 7, y, 6, 25)) Then
                            ReleaseNote(n)
                            Return
                        End If

                    Case 4, 11 'E & B
                        If Mouse.IntersectsWith(New Rectangle(x + (10 * p) + 3, y, 7, 25)) Or _
                            Mouse.IntersectsWith(New Rectangle(x + (10 * p), y + 25, 10, 25)) Then
                            ReleaseNote(n)
                            Return
                        End If

                    Case 7 'G
                        If Mouse.IntersectsWith(New Rectangle(x + (10 * p) + 2, y, 5, 25)) Or _
                            Mouse.IntersectsWith(New Rectangle(x + (10 * p), y + 25, 10, 25)) Then
                            ReleaseNote(n)
                            Return
                        End If

                    Case 8 'G#
                        If Mouse.IntersectsWith(New Rectangle(x + (10 * p) + 7, y, 6, 25)) Then
                            ReleaseNote(n)
                            Return
                        End If

                    Case 9 'A
                        If Mouse.IntersectsWith(New Rectangle(x + (10 * p) + 2, y, 5, 25)) Or _
                            Mouse.IntersectsWith(New Rectangle(x + (10 * p), y + 25, 10, 25)) Then
                            ReleaseNote(n)
                            Return
                        End If


                End Select
            Next
            Return
        End If
        MyBase.MouseUp(e)
    End Sub
#End Region

#Region "Misc stuff"
    Private Note() As NoteT

    Private Class NoteT
        Public Channel(15) As Boolean

        Public Function Pressed() As Boolean
            For Each ch As Boolean In Channel
                If ch Then Return True
            Next
            Return False
        End Function

        Public Sub Press(ByVal Channel As Byte)
            Me.Channel(Channel) = True
        End Sub
        Public Sub Release(ByVal Channel As Byte)
            Me.Channel(Channel) = False
        End Sub
        Public Sub Reset()
            For i As Integer = 0 To 15
                Channel(i) = False
            Next
        End Sub

    End Class

    Private Sub ResetNotes()
        For i As Integer = 0 To Note.Length - 1
            Note(i).Reset()
        Next
        DoDraw(True)
    End Sub

    ''' <summary>
    ''' Release note at ID
    ''' </summary>
    ''' <param name="ID"></param>
    ''' <remarks></remarks>
    Private Sub ReleaseNote(ByVal ID As Integer)
        Dim tmp As New Sanford.Multimedia.Midi.ChannelMessageBuilder
        tmp.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOff
        tmp.MidiChannel = numChannel.Value - 1
        tmp.Data1 = ID + Offset
        tmp.Data2 = 0
        Note(ID).Release(tmp.MidiChannel)
        Send(tmp)
        DoDraw(Rect)
    End Sub

    Private Sub PressNote(ByVal ID As Integer)
        Dim tmp As New Sanford.Multimedia.Midi.ChannelMessageBuilder
        tmp.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOn
        tmp.MidiChannel = numChannel.Value - 1
        tmp.Data1 = ID + Offset
        tmp.Data2 = 127
        Note(ID).Press(tmp.MidiChannel)
        Send(tmp)
        DoDraw(Rect)
    End Sub
#End Region

End Class

'AddMenuObject|Output,Plugins.MIDI_Output|MIDI
'AddReferences(Sanford.Slim.dll)
Public Class MIDI_Output
    Inherits BaseObject

    Private Enabled As Boolean = True

    Private WithEvents Device As Sanford.Multimedia.Midi.OutputDevice

    Private numChannel As New NumericUpDown
    Private WithEvents chkMessageChannels As New CheckBox

    Private WithEvents comDevices As New ComboBox

#Region "Object stuff"
    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 205) 'Setup the base rectangles.

        Inputs(New String() {"Enable,Boolean", _
                             "Channel Message,ChannelMessage,ChannelMessageBuilder", "SysCommonMessage,SysCommonMessage,SysCommonMessageBuilder", _
                             "SysExMessage,SysExMessage", "SysRealtimeMessage,SysRealtimeMessage"})

        'Set the title.
        Title = "MIDI Output"

        chkMessageChannels.Text = "Same as message"
        chkMessageChannels.Width = 117
        chkMessageChannels.Checked = True
        chkMessageChannels.Location = Position + New Point(86, 25)
        AddControl(chkMessageChannels)


        numChannel.Minimum = 1
        numChannel.Maximum = 16
        numChannel.Width = 40
        numChannel.Enabled = False
        numChannel.Location = Position + New Point(45, 25)
        AddControl(numChannel)



        If Sanford.Multimedia.Midi.OutputDevice.DeviceCount > 0 Then
            comDevices.Width = 200
            comDevices.Location = Position
            comDevices.DropDownStyle = ComboBoxStyle.DropDownList

            For i As Integer = 0 To Sanford.Multimedia.Midi.OutputDevice.DeviceCount - 1
                comDevices.Items.Add(Sanford.Multimedia.Midi.OutputDevice.GetDeviceCapabilities(i).name)
            Next
            'comDevices.SelectedIndex = 0

            AddControl(comDevices)
        Else
            MsgBox("Could not find any MIDI output devices!", MsgBoxStyle.Critical, "Error")
        End If

    End Sub

    Public Overrides Sub Dispose()
        chkMessageChannels.Dispose()
        numChannel.Dispose()
        comDevices.Dispose()

        If Device IsNot Nothing Then
            Device.Close()
            Device = Nothing
        End If
        MyBase.Dispose()
    End Sub

    Public Overrides Sub Moving()
        chkMessageChannels.Location = Position + New Point(86, 25)
        numChannel.Location = Position + New Point(45, 25)
        comDevices.Location = Position
    End Sub

    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Select Case sender.Index
            Case 0 'Enable
                If Data <> Enabled Then
                    Enabled = Data
                End If


            Case 1 'ChannelMessage
                If Not Enabled Then Return

                If chkMessageChannels.Checked Then
                    If Data.GetType = GetType(Sanford.Multimedia.Midi.ChannelMessageBuilder) Then
                        Data.Build()
                        Device.Send(Data.Result)
                    ElseIf Data.GetType = GetType(Sanford.Multimedia.Midi.ChannelMessage) Then
                        Device.Send(Data)
                    Else
                        MsgBox("Not ChannelMessageBuilder or ChannelMessage", MsgBoxStyle.Critical, "Error")
                        Return
                    End If
                Else

                    Dim message As Sanford.Multimedia.Midi.ChannelMessageBuilder
                    If Data.GetType = GetType(Sanford.Multimedia.Midi.ChannelMessageBuilder) Then
                        message = Data
                    ElseIf Data.GetType = GetType(Sanford.Multimedia.Midi.ChannelMessage) Then
                        message = New Sanford.Multimedia.Midi.ChannelMessageBuilder(Data)
                    Else
                        MsgBox("Not ChannelMessageBuilder or ChannelMessage", MsgBoxStyle.Critical, "Error")
                        Return
                    End If

                    message.MidiChannel = numChannel.Value - 1
                    message.Build()
                    Device.Send(message.Result)

                End If


            Case 2 'SysCommonMessage
                If Not Enabled Then Return
                If Data.GetType = GetType(Sanford.Multimedia.Midi.SysCommonMessageBuilder) Then
                    Dim message As Sanford.Multimedia.Midi.SysCommonMessageBuilder = Data
                    message.Build()
                    Device.Send(message.Result)
                ElseIf Data.GetType = GetType(Sanford.Multimedia.Midi.SysCommonMessage) Then
                    Device.Send(Data)
                Else
                    MsgBox("Not SysCommonMessageBuilder or SysCommonMessage", MsgBoxStyle.Critical, "Error")
                    Return
                End If

            Case 3 'SysExMessage
                If Not Enabled Then Return
                Device.Send(Data)

            Case 4 'SysRealtimeMessage
                If Not Enabled Then Return
                Device.Send(Data)
        End Select
    End Sub

    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        MyBase.Draw(g)

        g.DrawString("Channel:", DefaultFont, DefaultFontBrush, Position.X, Position.Y + 28)
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)

        g.Get_Value("Enabled", Enabled, False)
        g.Get_Value("DeviceID", comDevices.SelectedIndex)
        g.Get_Value("MessageChannels", chkMessageChannels.Checked)
        Try
            g.Get_Value("Channel", numChannel.Value)
        Catch ex As Exception
        End Try

        MyBase.Load(g)
    End Sub

    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("Enabled", Enabled)
        g.Set_Value("DeviceID", comDevices.SelectedIndex)
        g.Set_Value("MessageChannels", chkMessageChannels.Checked)
        g.Set_Value("Channel", numChannel.Value)


        Return g
    End Function
#End Region

#Region "Control events"

    Private Sub comDevices_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles comDevices.SelectedIndexChanged
        If comDevices.SelectedIndex = -1 Then Return
        If Device IsNot Nothing Then
            Device.Close()
            Device = Nothing
        End If


        Try
            'Create the device.
            Device = New Sanford.Multimedia.Midi.OutputDevice(comDevices.SelectedIndex)
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical, "Error!")
            Device = Nothing
        End Try
    End Sub

    Private Sub chkMessageChannels_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkMessageChannels.CheckedChanged
        numChannel.Enabled = Not chkMessageChannels.Checked
    End Sub
#End Region

#Region "MIDI events"
    Private Sub Device_Error(ByVal sender As Object, ByVal e As Sanford.Multimedia.ErrorEventArgs) Handles Device.Error
        MsgBox(e.Error.Message)
    End Sub
#End Region

End Class

'AddMenuObject|Set Controller,Plugins.MIDI_SetController|MIDI,Channel Message
'AddReferences(Sanford.Slim.dll)
Public Class MIDI_SetController
    Inherits BaseObject

    Private Enabled As Boolean = True

    Private numChannel As New NumericUpDown

    Private WithEvents comController As New ComboBox
    Private Controller As Integer

#Region "Object stuff"
    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 200, 50) 'Setup the base rectangles.

        'Create one output.
        Outputs(New String() {"Channel Message,ChannelMessageBuilder"})

        Inputs(New String() {"Enable,Boolean", "Input axis,Number,Boolean"})

        'Set the title.
        Title = "Set controller"

        numChannel.Minimum = 1
        numChannel.Maximum = 16
        numChannel.Width = 40
        numChannel.Location = Position + New Point(45, 25)
        AddControl(numChannel)

        comController.Width = 200
        comController.Location = Position
        comController.DropDownStyle = ComboBoxStyle.DropDownList
        comController.Items.AddRange([Enum].GetNames(GetType(Sanford.Multimedia.Midi.ControllerType)))
        comController.SelectedItem = Sanford.Multimedia.Midi.ControllerType.HoldPedal1.ToString
        AddControl(comController)

    End Sub

    Public Overrides Sub Dispose()
        numChannel.Dispose()
        comController.Dispose()

        MyBase.Dispose()
    End Sub

    Public Overrides Sub Moving()
        numChannel.Location = Position + New Point(45, 25)
        comController.Location = Position
    End Sub

    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Select Case sender.Index
            Case 0 'Enable
                If Data <> Enabled Then
                    Enabled = Data
                End If


            Case 1 'Input axis
                If Not Enabled Then Return
                If Data.GetType = GetType(Boolean) Then
                    If Data = True Then
                        Data = 1
                    Else
                        Data = 0
                    End If
                End If
                Dim message As New Sanford.Multimedia.Midi.ChannelMessageBuilder
                message.Command = Sanford.Multimedia.Midi.ChannelCommand.Controller
                message.Data1 = Controller
                message.Data2 = Data * 127
                message.MidiChannel = numChannel.Value - 1
                Send(message)
        End Select
    End Sub

    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        MyBase.Draw(g)

        g.DrawString("Channel:", DefaultFont, DefaultFontBrush, Position.X, Position.Y + 28)
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)

        g.Get_Value("Enabled", Enabled, False)

        Dim tmpController As Integer = -1
        g.Get_Value("Controller", tmpController, False)
        If Not tmpController = -1 Then
            comController.SelectedItem = [Enum].GetName(GetType(Sanford.Multimedia.Midi.ControllerType), tmpController)
        End If

        g.Get_Value("Channel", numChannel.Value, False)

        MyBase.Load(g)
    End Sub

    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("Enabled", Enabled)
        g.Set_Value("Controller", Controller)
        g.Set_Value("Channel", numChannel.Value)


        Return g
    End Function
#End Region

#Region "Control events"

    Private Sub comController_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles comController.SelectedIndexChanged
        If comController.SelectedIndex = -1 Then Return
        Controller = [Enum].Parse(GetType(Sanford.Multimedia.Midi.ControllerType), comController.SelectedItem.ToString)
    End Sub

#End Region

End Class

'AddMenuObject|Simulate pedals,Plugins.MIDI_SimulatePedals,120|MIDI,Channel Message
'AddReferences(Sanford.Slim.dll)

Public Class MIDI_SimulatePedals
    Inherits BaseObject

    Private Enabled As Boolean = True


    Private numChannel As New NumericUpDown
    Private chkRemoveOldNotes As New CheckBox
    Private chkFilterOtherChannels As New CheckBox

#Region "Object stuff"
    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 145) 'Setup the base rectangles.

        'Create one output.
        Outputs(New String() {"Channel Message,ChannelMessageBuilder"})

        Inputs(New String() {"Enable,Boolean", "Channel Message,ChannelMessage,ChannelMessageBuilder", "Sustain,Boolean", "Sostenuto,Boolean", "Soft,Boolean"})

        'Set the title.
        Title = "Simulate Pedals"

        chkRemoveOldNotes.Text = "Remove old notes"
        chkRemoveOldNotes.Width = 115
        chkRemoveOldNotes.Checked = True
        chkRemoveOldNotes.Location = Position + New Point(5, 40)
        AddControl(chkRemoveOldNotes)

        chkFilterOtherChannels.Text = "Filter out other channels"
        chkFilterOtherChannels.Width = 145
        chkFilterOtherChannels.Checked = False
        chkFilterOtherChannels.Location = Position + New Point(5, 20)
        AddControl(chkFilterOtherChannels)

        numChannel.Minimum = 1
        numChannel.Maximum = 16
        numChannel.Width = 40
        numChannel.Location = Position + New Point(55, 0)
        AddControl(numChannel)


    End Sub

    Public Overrides Sub Dispose()
        chkRemoveOldNotes.Dispose()
        chkFilterOtherChannels.Dispose()
        numChannel.Dispose()


        MyBase.Dispose()
    End Sub

    Public Overrides Sub Moving()
        chkRemoveOldNotes.Location = Position + New Point(5, 40)
        chkFilterOtherChannels.Location = Position + New Point(5, 20)
        numChannel.Location = Position + New Point(55, 0)
    End Sub

    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        MyBase.Draw(g)

        g.DrawString("Channel:", DefaultFont, DefaultFontBrush, Position.X + 5, Position.Y + 3)
    End Sub

    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Select Case sender.Index
            Case 0 'Enable
                If Data <> Enabled Then
                    Enabled = Data
                End If


            Case 1 'ChannelMessage
                If Not Enabled Then Return
                If Data.MidiChannel <> numChannel.Value - 1 Then
                    If chkFilterOtherChannels.Checked Then
                        Return
                    Else
                        Send(Data)
                        Return
                    End If
                End If

                Dim message As Sanford.Multimedia.Midi.ChannelMessageBuilder
                If Data.GetType = GetType(Sanford.Multimedia.Midi.ChannelMessage) Then
                    message = New Sanford.Multimedia.Midi.ChannelMessageBuilder(Data)
                Else
                    message = Data
                End If



                Dim NoteOn As Boolean = False

                'Is it a note (on or off)?
                If (message.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOn) Then
                    If message.Data2 > 0 Then
                        NoteOn = True
                    End If
                End If
                'Is the note on?
                If NoteOn Then
                    'If the pedal is down then lower the volume.
                    If SoftPressed Then
                        message.Data2 = message.Data2 * 0.3
                    End If

                    'Pedals
                    If Note(message.Data1) = Notes.Sostenuto Then
                        If Not SostenutoPressed Then
                            Note(message.Data1) = Notes.Pressed

                        ElseIf chkRemoveOldNotes.Checked Then
                            ReleaseNote(message.Data1)
                            Note(message.Data1) = Notes.Sostenuto
                        End If

                    ElseIf SustainPressed Then
                        If Note(message.Data1) = Notes.SustainReleased And chkRemoveOldNotes.Checked Then
                            ReleaseNote(message.Data1)
                        Else
                            SustainList.Add(message.Data1)
                        End If
                        Note(message.Data1) = Notes.SustainPressed

                    Else
                        Note(message.Data1) = Notes.Pressed
                    End If

                Else

                    Select Case Note(message.Data1)
                        Case Notes.Sostenuto
                            If SostenutoPressed Then
                                Return
                            Else
                                Note(message.Data1) = Notes.Released
                            End If

                        Case Notes.SustainPressed
                            If SustainPressed Then
                                Note(message.Data1) = Notes.SustainReleased
                                Return
                            Else
                                Note(message.Data1) = Notes.Released
                            End If
                        Case Notes.SustainReleased
                            If SustainPressed Then
                                Return
                            Else
                                Note(message.Data1) = Notes.Released
                            End If

                        Case Notes.Pressed
                            Note(message.Data1) = Notes.Released
                            message.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOff
                    End Select
                End If

                Send(message)

            Case 2 'Sustain pedal
                SustainPressed = Data
                If SustainPressed Then
                    PressSustain()
                Else
                    ReleaseSustain()
                End If

            Case 3 'Sostenuto pedal
                SostenutoPressed = Data
                If SostenutoPressed Then
                    PressSostenuto()
                Else
                    ReleaseSostenuto()
                End If


            Case 4 'Soft pedal
                SoftPressed = Data


        End Select
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)

        g.Get_Value("Enabled", Enabled, False)
        g.Get_Value("RemoveOldNotes", chkRemoveOldNotes.Checked, False)
        g.Get_Value("FilterOtherChannels", chkFilterOtherChannels.Checked, False)
        g.Get_Value("Channel", numChannel.Value, False)

        MyBase.Load(g)
    End Sub

    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("Enabled", Enabled, True)
        g.Set_Value("RemoveOldNotes", chkRemoveOldNotes.Checked, True)
        g.Set_Value("FilterOtherChannels", chkFilterOtherChannels.Checked, False)
        g.Set_Value("Channel", numChannel.Value, 1)


        Return g
    End Function
#End Region

#Region "Simulate MIDI pedals"
    Private Note(127) As Byte 'Used to hold notes.

    Private SostenutoList As New List(Of Byte)
    Private SustainList As New List(Of Byte)
    Private SustainPressed As Boolean = False
    Private SostenutoPressed As Boolean = False
    Private SoftPressed As Boolean = False

    Private Enum Notes
        Released = 0
        Pressed = 1
        Sostenuto = 2 'Is the sostenuto pedal holding the note.
        SustainPressed = 3 'Sustain pedal is down And the note.
        SustainReleased = 4 'Meaning the note was released but the sustain pedal is still down.
    End Enum

    ''' <summary>
    ''' Release note at ID
    ''' </summary>
    ''' <param name="ID"></param>
    ''' <remarks></remarks>
    Private Sub ReleaseNote(ByVal ID As Integer)
        Dim tmp As New Sanford.Multimedia.Midi.ChannelMessageBuilder
        tmp.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOff
        tmp.Data1 = ID
        tmp.Data2 = 0
        Note(ID) = Notes.Released
        Send(tmp)
    End Sub

    Private Sub PressSustain()
        'Check for down keys and set them to sustain.
        For n As Byte = 0 To Note.Length - 1
            If Note(n) = Notes.Pressed Then
                Note(n) = Notes.SustainPressed
                SustainList.Add(n)
            End If
        Next

    End Sub
    Private Sub ReleaseSustain()
        Dim tmp As New Sanford.Multimedia.Midi.ChannelMessageBuilder
        tmp.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOff
        tmp.Data2 = 0
        For Each n As Byte In SustainList
            If Note(n) = Notes.SustainReleased Then
                Note(n) = Notes.Released
                tmp.Data1 = n
                Send(tmp)
            End If
        Next
        SustainList.Clear()
    End Sub

    Private Sub PressSostenuto()
        For n As Byte = 0 To Note.Length - 1
            If Note(n) = Notes.Pressed Or Note(n) = Notes.SustainPressed Then
                Note(n) = Notes.Sostenuto
                SostenutoList.Add(n)
            End If
        Next
    End Sub
    Private Sub ReleaseSostenuto()
        Dim tmp As New Sanford.Multimedia.Midi.ChannelMessageBuilder
        tmp.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOff
        tmp.MidiChannel = 0
        tmp.Data2 = 0
        For Each n As Byte In SostenutoList
            Note(n) = Notes.Released
            tmp.Data1 = n
            Send(tmp)
        Next
        SostenutoList.Clear()
    End Sub

#End Region

End Class

'AddMenuObject|Transpose,Plugins.MIDI_Transpose|MIDI,Channel Message,Note
'AddReferences(Sanford.Slim.dll)
Public Class MIDI_Transpose
    Inherits BaseObject

    Private Enabled As Boolean = True

    Private numOctave As New NumericUpDown

#Region "Object stuff"
    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 95) 'Setup the base rectangles.

        'Create one output.
        Outputs(New String() {"Channel Message,ChannelMessageBuilder"})

        Inputs(New String() {"Enable,Boolean", "Channel Message,ChannelMessageBuilder,ChannelMessage"})

        'Set the title.
        Title = "MIDI Transpose"

        numOctave.Minimum = -4
        numOctave.Maximum = 4
        numOctave.Width = 50
        numOctave.Value = 0
        numOctave.Location = Position + New Point(45, 0)
        AddControl(numOctave)

    End Sub

    Public Overrides Sub Dispose()
        numOctave.Dispose()

        MyBase.Dispose()
    End Sub

    Public Overrides Sub Moving()
        numOctave.Location = Position + New Point(45, 0)
    End Sub

    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Select Case sender.Index
            Case 0 'Enable
                If Data <> Enabled Then
                    Enabled = Data
                End If


            Case 1 'Channel Message
                If Not Enabled Then Return

                Dim message As Sanford.Multimedia.Midi.ChannelMessageBuilder
                If Data.GetType = GetType(Sanford.Multimedia.Midi.ChannelMessage) Then
                    message = New Sanford.Multimedia.Midi.ChannelMessageBuilder(Data)
                Else
                    message = Data
                End If



                'Is it a note (on or off)?
                If (message.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOn Or message.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOff) Then
                    message.Data1 += 12 * numOctave.Value
                    If message.Data1 > 127 Then message.Data1 = 127
                    If message.Data1 < 0 Then message.Data1 = 0
                End If

                Send(message)

        End Select
    End Sub

    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        MyBase.Draw(g)

        g.DrawString("Octave:", DefaultFont, DefaultFontBrush, Position.X, Position.Y + 3)
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)

        g.Get_Value("Enabled", Enabled, False)
        g.Get_Value("Octave", numOctave.Value, False)
        MyBase.Load(g)
    End Sub

    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("Enabled", Enabled)
        g.Set_Value("Octave", numOctave.value)


        Return g
    End Function
#End Region

End Class

'AddMenuObject|Set volume,Plugins.MIDI_Volume,70|MIDI,Channel Message'110,Note
'AddReferences(Sanford.Slim.dll)
Public Class MIDI_Volume
    Inherits BaseObject

    Private Enabled As Boolean = True

    Private numVolume As New NumericUpDown

#Region "Object stuff"
    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 95) 'Setup the base rectangles.

        'Create one output.
        Outputs(New String() {"Channel Message,ChannelMessageBuilder"})

        Inputs(New String() {"Enable,Boolean", "Channel Message,ChannelMessageBuilder,ChannelMessage", "Volume,Number,Boolean"})

        'Set the title.
        Title = "MIDI Volume"

        numVolume.Minimum = 0
        numVolume.Maximum = 127
        numVolume.Width = 50
        numVolume.Value = 127
        numVolume.Location = Position + New Point(45, 0)
        AddControl(numVolume)

    End Sub

    Public Overrides Sub Dispose()
        numVolume.Dispose()

        MyBase.Dispose()
    End Sub

    Public Overrides Sub Moving()
        numVolume.Location = Position + New Point(45, 0)
    End Sub

    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Select Case sender.Index
            Case 0 'Enable
                If Data <> Enabled Then
                    Enabled = Data
                End If


            Case 1 'Channel Message
                If Not Enabled Then Return

                Dim message As Sanford.Multimedia.Midi.ChannelMessageBuilder
                If Data.GetType = GetType(Sanford.Multimedia.Midi.ChannelMessage) Then
                    message = New Sanford.Multimedia.Midi.ChannelMessageBuilder(Data)
                Else
                    message = Data
                End If



                'Is it a note (on or off)?
                If (message.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOn) Then
                    If message.Data2 > 0 Then
                        message.Data2 = numVolume.Value 
                    End If
                End If

                Send(message)

            Case 2 'Volume
                If Not Enabled Then Return
                numVolume.Value = (Data * 127) 
        End Select
    End Sub

    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        MyBase.Draw(g)

        g.DrawString("Volume:", DefaultFont, DefaultFontBrush, Position.X, Position.Y + 3)
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)

        g.Get_Value("Enabled", Enabled, False)
        g.Get_Value("Volume", numVolume.Value, False)
        MyBase.Load(g)
    End Sub

    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("Enabled", Enabled)
        g.Set_Value("Volume", numvolume.value)


        Return g
    End Function
#End Region

End Class

End Namespace