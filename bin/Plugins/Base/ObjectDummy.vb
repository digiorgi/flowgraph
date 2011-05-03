Public Class ObjectDummy
    Inherits BaseObject
    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 80)
        File = "Base\ObjectDummy.vb"
        Title = "ObjectDummy"
    End Sub
    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        MyBase.Draw(g)
        g.DrawString("RemoveMe", DefaultFont, DefaultFontBrush, Position)
    End Sub
End Class