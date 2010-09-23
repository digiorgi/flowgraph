Public Class fgDisplayAsString
    Inherits BaseObject

    Public Sub New(ByVal Position As Point)
        Setup(Position, 120, 60) 'Setup the base rectangles.

        'Create one input.
        Inputs(New String() {"Value to display."})

        'Set the title.
        Title = "Display as string"

    End Sub

    Private Data As String
    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As Object)
        Me.Data = Data.ToString
        DoDraw()
    End Sub

    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        'Draw the base stuff like the title outputs etc..
        MyBase.Draw(g)

        'Draw the value.
        g.DrawString("String= " & Data, DefaultFont, Brushes.Black, Rect.X + 16, Rect.Y + 16)
    End Sub

End Class
