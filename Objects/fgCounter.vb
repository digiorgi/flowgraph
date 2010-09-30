Public Class fgCounter
    Inherits BaseObject

    Private WithEvents tmr As New Timer

    Private Value As Integer

    Private WithEvents btnReset As New Button

    Public Sub New(ByVal Position As Point)
        Setup("fgCounter", Position, 120, 60) 'Setup the base rectangles.

        'Create one output.
        Outputs(New String() {"Value"})

        'Set the title.
        Title = "Counter"

        tmr.Interval = 1000
        tmr.Enabled = True


        btnReset.Text = "Reset"
        btnReset.Location = Position + New Point(15, 30)
        AddControl(btnReset)

    End Sub

    Public Overrides Sub Distroy()
        btnReset.Dispose()
        MyBase.Distroy()
    End Sub

    Public Overrides Sub Moved()
        MyBase.Moved()

        btnReset.Location = Rect.Location + New Point(15, 30)
    End Sub


    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        'Draw the base stuff like the title outputs etc..
        MyBase.Draw(g)

        'Draw the value.
        g.DrawString("Value= " & Value, DefaultFont, DefaultFontBrush, Rect.X + 15, Rect.Y + 15)

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

    Private Sub btnReset_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnReset.Click
        Value = 0
        Send(Value)
        DoDraw()
    End Sub
End Class
