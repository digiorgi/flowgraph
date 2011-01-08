'AddMenuObject|Slider,Plugins.fgSlider
Public Class fgSlider
    Inherits BaseObject

    Private WithEvents prg As New ProgressBar
    Private Enabled As Boolean = True

    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 105) 'Setup the base rectangles.

        'Create one output.
        Outputs(New String() {"Value,Number"})

        Inputs(New String() {"Enable,Boolean", "Value"})

        'Set the title.
        Title = "Slider"

        prg.Width = 100
        prg.Minimum = 0
        prg.Maximum = prg.Width
        prg.Value = prg.Maximum * 0.5
        prg.Style = ProgressBarStyle.Continuous
        prg.Location = Position
        AddControl(prg)
    End Sub

    Public Overrides Sub Dispose()
        prg.Dispose()
        MyBase.Dispose()
    End Sub

    Public Overrides Sub Moving()
        prg.Location = Position
    End Sub

    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Select Case sender.Index
            Case 0 'Enable
                Enabled = Data

            Case 1 'Set value
                prg.Value = Data * prg.Maximum
        End Select
    End Sub

    Private Sub prg_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles prg.MouseDown
        If e.Button = MouseButtons.Left Then
            If e.X >= prg.Width Then
                prg.Value = prg.Maximum
            ElseIf e.X <= 0 Then
                prg.Value = prg.Minimum
            Else
                prg.Value = e.X
            End If
            Send(prg.Value / prg.Maximum)
        End If
    End Sub

    Private Sub prg_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles prg.MouseMove
        If e.Button = MouseButtons.Left Then
            If e.X >= prg.Width Then
                prg.Value = prg.Maximum
            ElseIf e.X <= 0 Then
                prg.Value = prg.Minimum
            Else
                prg.Value = e.X
            End If
            Send(prg.Value / prg.Maximum)
        End If
    End Sub
End Class
