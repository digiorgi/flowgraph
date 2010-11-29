'AddMenuObject|FPS,Plugins.fgFPS,60|Misc
Public Class fgFPS
    Inherits BaseObject

    Private WithEvents tmr As New Timer

    Private FrameCount, FPS, LastFPS As Integer

    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 120, 30) 'Setup the base rectangles.

        Inputs(New String() {"Enable,Boolean"})

        'Set the title.
        Title = "FPS"

        tmr.Interval = 1000
        tmr.Enabled = True
    End Sub

    Public Overrides Sub Dispose()
        tmr.Dispose()
        MyBase.Dispose()
    End Sub

    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Select Case sender.Index
            Case 0 'Enable
                tmr.Enabled = Data
        End Select
    End Sub


    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        'Draw the base stuff like the title outputs etc..
        MyBase.Draw(g)

        'Draw the value.
        g.DrawString("FPS= " & LastFPS, DefaultFont, DefaultFontBrush, Position.X + 1, Position.Y + 1)
        g.DrawString("Total Frames= " & FrameCount, DefaultFont, DefaultFontBrush, Position.X + 1, Position.Y + 12)

        FPS += 1
        FrameCount += 1
    End Sub

    Private Sub tmr_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles tmr.Tick
        If FPS = 1 And LastFPS = 1 Then
            FPS = 0
            LastFPS = 0
        ElseIf FPS = 1 And LastFPS = 0 Then
            Return
        Else
            LastFPS = FPS
            FPS = 0
        End If


        DoDraw()
    End Sub
End Class
