Public Class ObjectDummy
    Inherits BaseObject
    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 80)
        File = "Base\ObjectDummy.vb"
        Title = "ObjectDummy"

        Dim g As New SimpleD.Group(UserData, False)


        Dim inp(Split(g.GetValue("input"), ",").Length - 1) As String
        Inputs(inp)

        Dim o(Split(g.GetValue("output"), "`").Length - 1) As String
        Outputs(o)
    End Sub
    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        MyBase.Draw(g)
        'g.DrawString("RemoveMe", DefaultFont, DefaultFontBrush, Position)
        g.DrawString(UserData, DefaultFont, DefaultFontBrush, Position)
    End Sub
End Class