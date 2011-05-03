'AddMenuObject|Switch,Plugins.Common.Switch|Common
Namespace Common
    Public Class Switch
        Inherits BaseObject

        Private Enabled As Boolean = True

        Private Value As Boolean = True


        Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
            Setup(UserData, StartPosition, 70) 'Setup the base rectangles.

            Outputs(New String() {"Value,Boolean"})
            Inputs(New String() {"Enable,Boolean", "Value,Boolean"})

            'Set the title.
            Title = "Switch"
            File = "Common\Switch.vb"
        End Sub

        Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
            Select Case sender.Index
                Case 0 'Enable
                    Enabled = Data

                Case 1 'Set value
                    If Not Enabled Then Return
                    If Data = True Then
                        Value = Not Value
                        Send(Value)
                    End If


            End Select

            DoDraw(Rect)
        End Sub

        Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
            MyBase.Draw(g)

            g.DrawString("Value= " & Value.ToString, DefaultFont, Brushes.Black, Position)
        End Sub

        Public Overrides Sub Load(ByVal g As SimpleD.Group)

            g.GetValue("Enabled", Enabled, False)
            g.GetValue("Value", Value, False)
            MyBase.Load(g)
        End Sub

        Public Overrides Function Save() As SimpleD.Group
            Dim g As SimpleD.Group = MyBase.Save()

            g.SetValue("Enabled", Enabled)
            g.SetValue("Value", Value)


            Return g
        End Function
    End Class


End Namespace