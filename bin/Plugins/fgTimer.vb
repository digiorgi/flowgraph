'AddMenuObject|Timer,Plugins.fgTimer|Math
Public Class fgTimer
    Inherits BaseObject

    Private WithEvents tmr As New Timer

    Private WithEvents numInterval As New NumericUpDown
    Public Sub New(ByVal Position As Point, ByVal UserData As String)
        Setup(UserData, Position, 120) 'Setup the base rectangles.

        'Create one output.
        Outputs(New String() {"Tick"})

        Inputs(New String() {"Enable|Boolean", "Interval|Number"})

        'Set the title.
        Title = "Timer"


        numInterval.Minimum = 0
        numInterval.Maximum = 1000000
        numInterval.Width = 85
        numInterval.Location = Position + New Point(15, 20)
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
        numInterval.Location = Rect.Location + New Point(15, 20)
    End Sub

    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Select Case sender.Index
            Case 0 'Enable
                tmr.Enabled = Data

            Case 1 'Interval
                numInterval.Value = Data
        End Select
    End Sub



    Private Sub tmr_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles tmr.Tick
        Send(Nothing)
    End Sub

    Private Sub numInterval_ValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles numInterval.ValueChanged
        tmr.Interval = numInterval.Value
        UserData = numInterval.Value
    End Sub
End Class
