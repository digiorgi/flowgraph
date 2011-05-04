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
        File = "fgCounter.vb"

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
        DoDraw(Rect)
    End Sub

    Private Sub btnReset_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnReset.Click
        Value = 0
        Send(Value)
        DoDraw(Rect)
    End Sub
End Class
