'AddMenuObject|Normalizer,Plugins.math_Normalizer,,|Math
'AddMenuObject|Denormalizer,Plugins.math_Denormalizer,,|Math
'Needed|Name displayed,Class name,Width,UserData|Groups


Public Class math_Normalizer
    Inherits BaseObject

    Private Max As Single = 100
    Private Value As Single = 0

    Private WithEvents txtMax As New TextBox

    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 50) 'Setup the base rectangles.



        'Create the inputs.
        Inputs(New String() {"Value,Number,Boolean", "Max,Number"})
        'Create the output.
        Outputs(New String() {"Value,0-1Normalized"})

        'Set the title.
        Title = "Normalizer"
        File = "Math\math_Normalizer.vb"

        txtMax.Text = Max
        txtMax.Width = 50

        AddControl(txtMax)

        Moving()
    End Sub

    Public Overrides Sub Moving()
        txtMax.Location = Position + New Point(0, 10)
    End Sub

    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)

        Select Case sender.Index
            Case 0 'Value
                Select Case Data.GetType
                    Case GetType(Boolean)
                        If DirectCast(Data, Boolean) = True Then
                            Value = Max
                        Else
                            Value = 0
                        End If

                    Case Else
                        Value = CSng(Data)
                End Select


                Send(Value / Max)

            Case 1 'Max
                txtMax.Text = Data.ToString

        End Select
    End Sub

    Public Overrides Sub Load(g As SimpleD.Group)
        MyBase.Load(g)
        g.GetValue("value", Value, False)
        g.GetValue("max", Max, False)
        txtMax.Text = Max
    End Sub
    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()
        g.AddValue("Value", Value)
        g.AddValue("Max", Max)
        Return g
    End Function

    Public Overrides Sub Dispose()
        MyBase.Dispose()

        txtMax.Dispose()
    End Sub


    Private Sub txtMax_KeyUp(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles txtMax.KeyUp
        If Single.TryParse(txtMax.Text, Max) = False Then
            MsgBox("Needs to be proper number!", MsgBoxStyle.Exclamation, "Normalizer - Flowgraph")
            Return
        End If
        If e.KeyCode = Keys.Enter Then
            Send(Value / Max)
        End If
    End Sub

    Private Sub txtMax_TextChanged(sender As Object, e As System.EventArgs) Handles txtMax.TextChanged
        Single.TryParse(txtMax.Text, Max)
    End Sub
End Class

Public Class math_Denormalizer
    Inherits BaseObject

    Private Max As Single = 100
    Private Value As Single = 0

    Private WithEvents txtMax As New TextBox
    Private WithEvents chkRound As New CheckBox

    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 60) 'Setup the base rectangles.



        'Create the inputs.
        Inputs(New String() {"Value,0-1Normalized", "Max,Number"})
        'Create the output.
        Outputs(New String() {"Value,Number"})

        'Set the title.
        Title = "Denormalizer"
        File = "Math\math_Normalizer.vb"

        txtMax.Text = Max
        txtMax.Width = Size.Width
        AddControl(txtMax)

        chkRound.Text = "Round"
        chkRound.Width = Size.Width
        chkRound.Height = 12
        AddControl(chkRound)


        Moving()
    End Sub
    Public Overrides Sub Dispose()
        MyBase.Dispose()

        txtMax.Dispose()
        chkRound.Dispose()
    End Sub
    Public Overrides Sub Moving()
        chkRound.Location = Position + New Point(0, -2)
        txtMax.Location = Position + New Point(0, 10)
    End Sub

    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)

        Select Case sender.Index
            Case 0 'Value
                value = CSng(Data)
                SendValue()


            Case 1 'Max
                txtMax.Text = Data.ToString

        End Select
    End Sub

    Public Overrides Sub Load(g As SimpleD.Group)
        MyBase.Load(g)
        g.GetValue("value", Value, False)
        g.GetValue("max", Max, False)
        txtMax.Text = Max
        g.GetValue("round", chkRound.Checked, False)
    End Sub
    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()
        g.AddValue("Value", Value)
        g.AddValue("Max", Max)
        g.AddValue("Round", chkRound.Checked.ToString)
        Return g
    End Function

    Private Sub SendValue()
        If chkRound.Checked Then
            Send(Math.Round(Value * Max))
        Else
            Send(Value * Max)
        End If
    End Sub

    Private Sub txtMax_KeyUp(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles txtMax.KeyUp
        If Single.TryParse(txtMax.Text, Max) = False Then
            MsgBox("Needs to be proper number!", MsgBoxStyle.Exclamation, "Normalizer - Flowgraph")
            Return
        End If
        If e.KeyCode = Keys.Enter Then
            SendValue()
        End If
    End Sub

    Private Sub txtMax_TextChanged(sender As Object, e As System.EventArgs) Handles txtMax.TextChanged
        Single.TryParse(txtMax.Text, Max)
    End Sub

    Private Sub chkRound_CheckedChanged(sender As Object, e As System.EventArgs) Handles chkRound.CheckedChanged
        SendValue()
    End Sub
End Class
