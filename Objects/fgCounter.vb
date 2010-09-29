Public Class fgCounter
    Inherits BaseObject

    Private WithEvents tmr As New Timer

    Private Value As Integer

    Private Reset As Rectangle

    Public Sub New(ByVal Position As Point)
        Setup("fgCounter", Position, 120, 60) 'Setup the base rectangles.

        'Create one output.
        Outputs(New String() {"Value"})

        'Set the title.
        Title = "Counter"

        tmr.Interval = 1000
        tmr.Enabled = True


        Reset = New Rectangle(Rect.X + 15, Rect.Y + 35, 40, 15)
    End Sub

    Public Overrides Sub MouseUp(ByVal e As System.Windows.Forms.MouseEventArgs)
        MyBase.MouseUp(e)

        If e.Button = MouseButtons.Left Then
            If Mouse.IntersectsWith(Reset) Then
                Value = 0
                Send(Value)
                DoDraw()
            End If
        End If
    End Sub

    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        'Draw the base stuff like the title outputs etc..
        MyBase.Draw(g)

        'Draw the value.
        g.DrawString("Value= " & Value, DefaultFont, Brushes.Black, Rect.X + 15, Rect.Y + 15)

        g.DrawString("Reset ", DefaultFont, Brushes.Black, Reset.X + 1, Reset.Y)
        g.DrawRectangle(Pens.Black, Reset)
    End Sub

    Public Overrides Sub DoubleClicked()
        On Error Resume Next
        Value = InputBox("Enter value", "Counter", 0)

    End Sub


    Private Sub tmr_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles tmr.Tick

        Value += 1

        If Value > 100 Then Value = 0

        Send(Value)

        DoDraw()
    End Sub
End Class
