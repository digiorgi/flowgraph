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

    Public Output() As Object
    ''' <summary>
    ''' The input the output is sending to.
    ''' </summary>
    Public OutputsInput() As Integer
    Public OutputNames() As String
    Public InputNames() As String
    'Public Input() As Object

    Public Rect As Rectangle

    Public Title As String = "Name not set"
    Private TitleRect As RectangleF
    Public TitleBar As Rectangle

    Private BackGround As Rectangle

#Region "Setup/Load"
    ''' <summary>
    ''' Create rectangles. using the position and size.
    ''' </summary>
    Protected Sub Setup(ByVal Position As Point, ByVal Width As Integer, ByVal Height As Integer)
        'Create the main rectangle.
        Rect = New Rectangle(Position, New Size(Width, Height))

        'Set the size of the title.  Used to drag the object around.
        TitleBar = New Rectangle(Rect.Location, New Size(Rect.Width, 15))

        BackGround = New Rectangle(Rect.X, Rect.Y + 15, Rect.Width, Rect.Height - 15)
    End Sub

#End Region

#Region "Draw"

    Public Overridable Sub Draw(ByVal g As Graphics)
        'Draw the title and the background. Then we draw teh border so it is on top.
        g.FillRectangle(Brushes.LightBlue, TitleBar)
        g.FillRectangle(Brushes.LightGray, BackGround)
        g.DrawRectangle(Pens.Black, Rect)

        'Draw the inputs. (if any.)
        If InputNames IsNot Nothing Then
            For n As Integer = 1 To InputNames.Length
                g.FillRectangle(Brushes.Red, Rect.X + 1, Rect.Y + 16 * n, 15, 15)
            Next
        End If
        'Draw the outputs. (if any.)
        If Output IsNot Nothing Then
            For n As Integer = 1 To Output.Length
                g.FillRectangle(Brushes.Green, Rect.Right - 15, Rect.Y + 16 * n, 15, 15)
            Next
        End If




        'If title rect is empty then we will set the position and size of the title string.
        If TitleRect.IsEmpty Then
            TitleRect.Size = g.MeasureString(Title, SystemFonts.DefaultFont)
            TitleRect.Location = New PointF(Rect.X + Rect.Width * 0.5 - TitleRect.Width * 0.5, Rect.Y + 1)
        End If
        g.DrawString(Title, SystemFonts.DefaultFont, Brushes.Black, TitleRect) 'Draw the title string.



    End Sub

    ''' <summary>
    ''' Draw lines connecting outputs to inputs.
    ''' </summary>
    Public Sub DrawConnectors(ByVal g As Graphics)
        If Output Is Nothing Then Return


        For n As Integer = 0 To Output.Length - 1
            If Output(n) IsNot Nothing Then

                g.DrawLine(ConnectorPen, GetOutputPosition(n), Output(n).GetInputPosition(OutputsInput(n)))

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

        If Output(ID) IsNot Nothing Then Output(ID).Receive(Data, Me)
    End Sub
    Public Sub Send(ByVal Data As Object)
        If Output Is Nothing Then Return

        For Each obj As Object In Output
            If obj IsNot Nothing Then obj.Receive(Data, Me)
        Next
    End Sub

    Public Overridable Sub Receive(ByVal Data As Object, ByVal sender As Object)
    End Sub
#End Region

#Region "Inputs & Outputs"

    Protected Sub Inputs(ByVal Names As String())
        InputNames = Names
        'ReDim Input(Names.Length - 1)
    End Sub
    Protected Sub Outputs(ByVal Names As String())
        ReDim Output(Names.Length - 1)
        ReDim OutputsInput(Names.Length - 1)
        OutputNames = Names
    End Sub

    Public Intersection As Integer
    Public Function IntersectsWithInput(ByVal rect As Rectangle) As Boolean
        If InputNames Is Nothing Then Return False

        For n As Integer = 1 To InputNames.Length
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
        Return New PointF(Rect.X + 7.5, Rect.Y + (16 * (ID + 1)) + 8)
    End Function
    Public Function GetOutputPosition(ByVal ID As Integer) As PointF
        Return New PointF(Rect.Right - 7.5, Rect.Y + (16 * (ID + 1)) + 8)
    End Function

#End Region


End Class
