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
    Public Index As Integer

    Private Name As String = "NoName"

    'Output
    Public Output() As Transmitter

    'Input
    Public Input() As Transmitter


    Public Rect As Rectangle

    Public Title As String = "Name not set"
    Private TitleRect As RectangleF
    Public TitleBar As Rectangle

    Private BackGround As Rectangle


#Region "Setup & Distroy"
    ''' <summary>
    ''' Create rectangles. using the position and size.
    ''' </summary>
    Protected Sub Setup(ByVal ClassName As String, ByVal Position As Point, ByVal Width As Integer, ByVal Height As Integer)
        Name = ClassName
        'Create the main rectangle.
        Rect = New Rectangle(Position, New Size(Width, Height))

        'Set the size of the title.  Used to drag the object around.
        TitleBar = New Rectangle(Rect.Location, New Size(Rect.Width, 15))

        BackGround = New Rectangle(Rect.X, Rect.Y + 15, Rect.Width, Rect.Height - 15)

        Index = Objects.Count


        Menu(0).Setup("Remove", 50)
    End Sub

    Public Overridable Sub Distroy()
        If Output IsNot Nothing Then
            For n As Integer = 0 To Output.Length - 1

                If Output(n).obj1 > -1 Then
                    Objects(Output(n).obj1).Input(Output(n).Index1).obj1 = -1
                    Objects(Output(n).obj1).Input(Output(n).Index1).index1 = -1
                    Objects(Output(n).obj1).Input(Output(n).Index1).Connected -= 1
                    Output(n).obj1 = -1
                    Output(n).Index1 = -1
                End If

            Next
        End If

        If Input IsNot Nothing Then
            For n As Integer = 0 To Input.Length - 1
                If Input(n).Connected > 0 Then
                    'Objects(Input(n).obj1).Output(Input(n).Index1).obj1 = -1
                    'Input(n).obj1 = -1
                    DisconnectInput(Input(n))
                End If
            Next
        End If
    End Sub
#End Region

#Region "Load & Save"
    Public Overridable Function Load(ByVal g As SimpleD.Group) As SimpleD.Group

        Dim tmp As String = ""


        If Output IsNot Nothing Then 'If there is output then save Output=(obj1),(index1),(obj1),etc.. for each output
            g.Get_Value("Output", tmp)
            Dim tmpS As String() = Split(tmp, ",")
            For n As Integer = 0 To Output.Length - 1
                Output(n).SetValues(tmpS(n * 2), tmpS((n * 2) + 1))
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
            tmp = Output(0).obj1 & "," & Output(0).Index1
            For n As Integer = 1 To Output.Length - 1
                tmp &= "," & Output(n).obj1 & "," & Output(n).Index1
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
        g.FillRectangle(Brushes.LightBlue, TitleBar)
        g.FillRectangle(Brushes.LightGray, BackGround)
        g.DrawRectangle(Pens.Black, Rect)

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
        g.DrawString(Title, SystemFonts.DefaultFont, Brushes.Black, TitleRect) 'Draw the title string.


        If IsMenuOpen Then
            Menu_Draw(g, MenuCurrent, MenuStart)
        End If


    End Sub

    ''' <summary>
    ''' Draw lines connecting outputs to inputs.
    ''' </summary>
    Public Sub DrawConnectors(ByVal g As Graphics)
        If Output Is Nothing Then Return


        For n As Integer = 0 To Output.Length - 1
            If Output(n).IsNotEmpty Then

                g.DrawLine(ConnectorPen, GetOutputPosition(n), Objects(Output(n).obj1).GetInputPosition(Output(n).Index1))

            End If
        Next
    End Sub

#End Region


    ''' <summary>
    ''' Is called when the object is moving.
    ''' Used to set the position of somethings.
    ''' </summary>
    Public Overridable Sub Moved()

        'Update the title position.
        TitleRect.Location = New PointF(Rect.X + Rect.Width * 0.5 - TitleRect.Width * 0.5, Rect.Y + 1)
        TitleBar.Location = Rect.Location
        BackGround.Location = New Point(Rect.X, Rect.Y + 15)


    End Sub

    Public Overridable Sub DoubleClicked()
    End Sub

    Public Sub SetPosition(ByVal x As Integer, ByVal y As Integer)
        Rect.Location = New Point(Math.Round(x / GridSize) * GridSize, Math.Round(y / GridSize) * GridSize)
        Moved()
    End Sub

#Region "Send & Receive"
    Public Sub Send(ByVal Data As Object, ByVal ID As Integer)
        If Output Is Nothing Then Return

        If Output(ID).IsNotEmpty Then Objects(Output(ID).obj1).Receive(Data, Output(ID))
    End Sub
    Public Sub Send(ByVal Data As Object)
        If Output Is Nothing Then Return

        For Each obj As Transmitter In Output
            If obj.IsNotEmpty Then Objects(obj.obj1).Receive(Data, obj)
        Next
    End Sub

    Public Overridable Sub Receive(ByVal Data As Object, ByVal sender As Transmitter)
    End Sub
#End Region

#Region "Inputs & Outputs"

    Protected Sub Inputs(ByVal Names As String())
        'InputNames = Names
        ReDim Input(Names.Length - 1)
        For n As Integer = 0 To Names.Length - 1
            Input(n) = New Transmitter(Index, n, Names(n))

        Next
    End Sub
    Protected Sub Outputs(ByVal Names As String())
        ReDim Output(Names.Length - 1)
        For n As Integer = 0 To Names.Length - 1
            Output(n) = New Transmitter(Index, n, Names(n))
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
    Private Menu(0) As MenuNode
    Private MenuCurrent() As MenuNode
    Private MenuStart As Point
    Private IsMenuOpen As Boolean = False

    Public Overridable Sub MouseMove(ByVal e As MouseEventArgs)
        If IsMenuOpen Then DoDraw(True)
    End Sub
    Public Overridable Sub MouseUp(ByVal e As MouseEventArgs)
        If e.Button = MouseButtons.Right Then

            IsMenuOpen = True
            MenuCurrent = Menu
            MenuStart = Mouse.Location

            DoDraw(True)
        ElseIf e.Button = MouseButtons.Left Then
            If IsMenuOpen Then
                Dim r As Integer = Menu_MouseUp(MenuCurrent, MenuStart)
                Select Case r
                    Case MenuResult.Closed
                        IsMenuOpen = False
                    Case MenuResult.SelectedGroup

                    Case Else
                        IsMenuOpen = False
                        If r = 0 Then
                            'MsgBox("REMOVE ME")
                            RemoveAt(Index)

                        End If
                End Select

                DoDraw(True)
            End If
        End If
    End Sub

#End Region


    Public Overrides Function ToString() As String
        Return Title
    End Function

End Class


Public Class Transmitter
    '0 defines it is from the base object.

    Public obj0, obj1 As Integer
    Public Index0, Index1 As Integer
    Public Name0, Name1 As String

    'For input
    Public MaxConnected As Integer = -1
    Public Connected As Integer = 0

    Public Sub New(ByVal obj As Integer, ByVal Index As Integer, ByVal Name As String)
        obj0 = obj
        obj1 = -1
        Index0 = Index
        Index1 = -1
        Name0 = Name
    End Sub
    Public Sub New(ByVal obj1 As Integer, ByVal Index1 As Integer)
        obj0 = -1
        Me.obj1 = obj1
        Index0 = -1
        Me.Index1 = Index1
        Name0 = "Not a real transmitter!"
    End Sub

    Public Sub SetValues(ByRef obj1 As Integer, ByVal Index1 As Integer)
        Me.obj1 = obj1
        Me.Index1 = Index1
    End Sub

    Public Function IsEmpty() As Boolean
        Return (obj1 = -1 AndAlso Index1 >= -1)
    End Function
    Public Function IsNotEmpty() As Boolean
        Return Not IsEmpty()
    End Function

    'Public Overrides Function ToString() As String
    '    Return Name0
    'End Function

    Public Shared Operator =(ByVal left As Transmitter, ByVal right As Transmitter) As Boolean
        If left Is Nothing Or right Is Nothing Then Return False
        Return (left.obj0 = right.obj0) And (left.obj1 = right.obj1) And (left.Index0 = right.Index0) And (left.Index1 = right.Index1)
    End Operator
    Public Shared Operator <>(ByVal left As Transmitter, ByVal right As Transmitter) As Boolean
        Return Not left = right
    End Operator

End Class