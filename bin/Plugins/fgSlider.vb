'AddMenuObject|Slider,Plugins.fgSlider
Public Class fgSlider
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
