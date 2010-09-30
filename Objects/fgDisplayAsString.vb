Public Class fgDisplayAsString
    Inherits BaseObject

    Public Sub New(ByVal Position As Point)
        Setup("fgDisplayAsString", Position, 120, 40) 'Setup the base rectangles.

        'Create one input.
        Inputs(New String() {"Value to display."})
        Input(0).MaxConnected = 1

        'Set the title.
        Title = "Display as string"

    End Sub

    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Add("Data", Data)

        Return g
    End Function
    Public Overrides Function Load(ByVal g As SimpleD.Group) As SimpleD.Group

        g.Get_Value("Data", Data)

        Return MyBase.Load(g)
    End Function
    Private Data As String
    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As Transmitter)
        Me.Data = Data.ToString()
        DoDraw()
    End Sub

    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        'Draw the base stuff like the title outputs etc..
        MyBase.Draw(g)

        'Draw the value.
        g.DrawString("String= " & Data, DefaultFont, DefaultFontBrush, Rect.X + 16, Rect.Y + 16)
    End Sub

End Class
