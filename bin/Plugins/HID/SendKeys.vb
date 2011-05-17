'AddMenuObject|SendKeys,Plugins.hidSendKeys,100|Input,Keyboard
Public Class hidSendKeys
    Inherits BaseObject

    Private txtKeys As New TextBox

    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 110, 25)
        File = "HID\SendKeys.vb"


        Inputs(New String() {"Send,Boolean,Tick"})

        'Set the title.
        Title = "SendKeys"

        txtKeys.Location = Me.Position

        AddControl(txtKeys)
    End Sub

    Public Overrides Sub Moving()
        txtKeys.Location = Me.Position
        MyBase.Moving()
    End Sub

    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()
        g.SetValue("Keys", txtKeys.Text)
        Return g
    End Function
    Public Overrides Sub Load(ByVal g As SimpleD.Group)
        g.GetValue("Keys", txtKeys.Text)
        MyBase.Load(g)
    End Sub

    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Select Case sender.Index
            Case 0 'Send
                If Data Is Nothing OrElse Data = True Then
                    SendKeys.Send(txtKeys.Text)
                End If
        End Select
    End Sub
End Class