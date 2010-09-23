Public Class fgCounter
    Inherits BaseObject

    Private WithEvents tmr As New Timer

    Private Value As Integer


    Public Sub New(ByVal Position As Point)
        Setup(Position, 120, 60) 'Setup the base rectangles.

        'Create one output.
        Outputs(New String() {"Value"})

        'Set the title.
        Title = "Counter"

        tmr.Interval = 1000
        tmr.Enabled = True

    End Sub

    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        'Draw the base stuff like the title outputs etc..
        MyBase.Draw(g)

        'Draw the value.
        g.DrawString("Value= " & Value, DefaultFont, Brushes.Black, Rect.X + 16, Rect.Y + 16)
    End Sub

    Public Overrides Sub DoubleClicked()
        On Error Resume Next
        Value = InputBox("Enter value", "Counter", 0)

    End Sub


    Private Sub tmr_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles tmr.Tick

        Value += 1

        Send(Value)

        DoDraw()
    End Sub
End Class
