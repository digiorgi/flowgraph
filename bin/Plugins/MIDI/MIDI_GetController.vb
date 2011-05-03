'AddMenuObject|Get,Plugins.MIDI_GetController|MIDI,Channel Message,Controller
'AddReferences(Sanford.Slim.dll)
Public Class MIDI_GetController
    Inherits BaseObject

    Private Enabled As Boolean = True


    Private numChannel As New NumericUpDown
    Private WithEvents chkAllChannels As New CheckBox

    Private WithEvents comController As New ComboBox
    Private Controller As Integer

#Region "Object stuff"
    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 200, 50) 'Setup the base rectangles.

        'Create one output.
        Outputs(New String() {"Value,0-1Normalized,Boolean"})
        Inputs(New String() {"Enable,Boolean", "Channel Message,ChannelMessage,ChannelMessageBuilder"})

        'Set the title.
        Title = "Get controller"
        File = "MIDI\MIDI_GetController.vb"

        numChannel.Minimum = 1
        numChannel.Maximum = 16
        numChannel.Width = 40
        numChannel.Enabled = False
        numChannel.Location = Position + New Point(45, 25)
        AddControl(numChannel)

        chkAllChannels.Text = "All channels"
        chkAllChannels.Width = 113
        chkAllChannels.Checked = True
        chkAllChannels.Location = Position + New Point(86, 24)
        AddControl(chkAllChannels)

        comController.Width = 200
        comController.Location = Position
        comController.DropDownStyle = ComboBoxStyle.DropDownList
        comController.Items.AddRange([Enum].GetNames(GetType(Sanford.Multimedia.Midi.ControllerType)))
        comController.SelectedItem = Sanford.Multimedia.Midi.ControllerType.HoldPedal1.ToString
        AddControl(comController)

    End Sub

    Public Overrides Sub Dispose()
        numChannel.Dispose()
        chkAllChannels.Dispose()
        comController.Dispose()

        MyBase.Dispose()
    End Sub

    Public Overrides Sub Moving()
        numChannel.Location = Position + New Point(45, 25)
        chkAllChannels.Location = Position + New Point(86, 24)
        comController.Location = Position
    End Sub

    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Select Case sender.Index
            Case 0 'Enable
                If Data <> Enabled Then
                    Enabled = Data
                End If


            Case 1 'Channel message
                If Not Enabled Then Return
                If Not chkAllChannels.Checked Then
                    If Data.MidiChannel <> numChannel.Value - 1 Then
                        Return
                    End If
                End If

                If Data.Command = Sanford.Multimedia.Midi.ChannelCommand.Controller And Data.Data1 = Controller Then
                    Send(Data.Data2 / 127)
                End If
        End Select
    End Sub

    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        MyBase.Draw(g)

        g.DrawString("Channel:", DefaultFont, DefaultFontBrush, Position.X, Position.Y + 28)
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)

        g.GetValue("Enabled", Enabled, False)

        Dim tmpController As Integer = -1
        g.GetValue("Controller", tmpController, False)
        If Not tmpController = -1 Then
            comController.SelectedItem = [Enum].GetName(GetType(Sanford.Multimedia.Midi.ControllerType), tmpController)
        End If

        g.GetValue("AllChannels", chkAllChannels.Checked, False)

        g.GetValue("Channel", numChannel.Value, False)

        MyBase.Load(g)
    End Sub

    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.SetValue("Enabled", Enabled)
        g.SetValue("Controller", Controller)
        g.SetValue("AllChannels", chkAllChannels.Checked)
        g.SetValue("Channel", numChannel.Value)


        Return g
    End Function
#End Region

#Region "Control events"
    Private Sub chkAllChannels_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkAllChannels.CheckedChanged
        numChannel.Enabled = Not chkAllChannels.Checked
    End Sub

    Private Sub comController_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles comController.SelectedIndexChanged
        If comController.SelectedIndex = -1 Then Return
        Controller = [Enum].Parse(GetType(Sanford.Multimedia.Midi.ControllerType), comController.SelectedItem.ToString)
    End Sub
#End Region

End Class
