Public Class ObjectDummy
    Inherits BaseObject

    Private str As String = ""

    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 140, 40)
        File = "Base\ObjectDummy.vb"
        Title = "ObjectDummy"

        Dim g As New SimpleD.Group(UserData, False)


        Dim inCount(Split(g.GetValue("input"), ",").Length - 1) As String
        Inputs(inCount)

        Dim outCount(Split(g.GetValue("output"), "`").Length - 1) As String
        Outputs(outCount)

        str = "Name=" & g.GetValue("name")
        If g.GetValue("file") <> "" Then
            str &= vbNewLine & "File=" & g.GetValue("file")
        End If
        str &= vbNewLine & "UserData=" & g.GetValue("userdata")

        MenuItems.Add(New Menu.Node("Copy to clipboard", False, 100))
        MenuItems.Add(New Menu.Node("Copy fgs snippet", False))
    End Sub

    Public Overrides Sub MenuSelected(Result As Menu.Node)
        MyBase.MenuSelected(Result)

        If Result.Result = Menu.Result.SelectedItem Then
            If Result.Name = "Copy to clipboard" Then
                Clipboard.SetText(str)
            ElseIf Result.Name = "Copy fgs snippet" Then
                Clipboard.SetText(UserData)
            End If
        End If
    End Sub

    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        MyBase.Draw(g)
        g.DrawString(str, DefaultFont, DefaultFontBrush, Position)
    End Sub
End Class