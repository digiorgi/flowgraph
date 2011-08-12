'AddMenuObject|Timer,Plugins.Common.Timer|Common
Namespace Common
    Public Class Timer
        Inherits BaseObject

        Private WithEvents tmr As New Windows.Forms.Timer

        Private WithEvents numInterval As New NumericUpDown
        Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
            Setup(UserData, StartPosition, 85) 'Setup the base rectangles.


            'Create one output.
            Outputs(New String() {"Tick,"})

            Inputs(New String() {"Enable,Boolean", "Interval,Number"})

            'Set the title.
            Title = "Timer"
            File = "Common\Timer.vb"

            numInterval.Minimum = 0
            numInterval.Maximum = 1000000
            numInterval.Width = 85
            numInterval.Location = Position
            AddControl(numInterval)


            If UserData <> "" Then
                numInterval.Value = UserData
            Else
                numInterval.Value = 1000
            End If

            tmr.Enabled = True
        End Sub

        Public Overrides Sub Dispose()
            numInterval.Dispose()
            MyBase.Dispose()
        End Sub

        Public Overrides Sub Moving()
            numInterval.Location = Position
        End Sub

        Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
            Select Case sender.Index
                Case 0 'Enable
                    tmr.Enabled = Data

                Case 1 'Interval
                    numInterval.Value = Data
            End Select
        End Sub

        Public Overrides Sub Load(ByVal g As SimpleD.Group)
            g.GetValue("Enabled", tmr.Enabled, False)

            MyBase.Load(g)
        End Sub
        Public Overrides Function Save() As SimpleD.Group
            Dim g As SimpleD.Group = MyBase.Save()

            g.SetValue("Enabled", tmr.Enabled)

            Return g
        End Function

        Private Sub tmr_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles tmr.Tick
            Send(Nothing)
        End Sub

        Private Sub numInterval_ValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles numInterval.ValueChanged
            tmr.Interval = numInterval.Value
            UserData = numInterval.Value
        End Sub
    End Class
End Namespace