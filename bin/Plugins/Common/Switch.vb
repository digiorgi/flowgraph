'AddMenuObject|Switch,Plugins.Common.Switch|Common
Namespace Common
    Public Class Switch
        Inherits BaseObject

        Private Enabled As Boolean = True

        Private Value As Boolean = True


        Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
            Setup(UserData, StartPosition, 40) 'Setup the base rectangles.

            Outputs(New String() {"Value,Boolean"})
            Inputs(New String() {"Enable,Boolean", "Value,,Boolean"})

            'Set the title.
            Title = "Switch"
            File = "Common\Switch.vb"
        End Sub

        Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
            Select Case sender.Index
                Case 0 'Enable
                    Enabled = DirectCast(Data, Boolean)

                Case 1 'Value
                    If Not Enabled Then Return

                    If Data Is Nothing Then  'Tick
                        Value = Not Value
                        Send(Value)
                    Else 'Set value
                        Value = DirectCast(Data, Boolean)
                        Send(Value)
                    End If
            End Select

            DoDraw(Rect)
        End Sub

        Public Overrides Sub MouseDoubleClick(e As System.Windows.Forms.MouseEventArgs)
            MyBase.MouseDoubleClick(e)
            If Not Enabled Then Return
            Value = Not Value
            Send(Value)
            DoDraw(Rect)
        End Sub

        Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
            MyBase.Draw(g)

            Dim bOn As Brush = Brushes.Green
            Dim bOff As Brush = Brushes.Red
            If Not Enabled Then
                bOn = Brushes.Gray
                bOff = Brushes.Gray
            End If

            If Value Then
                g.FillRectangle(bOn, Position.X, Position.Y, 16, 16)
            Else
                g.FillRectangle(bOff, Position.X + 20, Position.Y, 16, 16)
            End If

            g.DrawString("On Off", DefaultFont, Brushes.White, Position)
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