'AddMenuObject|Frame counter,Plugins.fgFPS,80|Misc
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
