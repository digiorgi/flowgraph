'AddMenuObject|Input,Plugins.MIDI_Input|MIDI
'AddReferences(Sanford.Slim.dll)
Public Class MIDI_Input
    Inherits BaseObject

    Private Enabled As Boolean = True

    Private WithEvents Device As Sanford.Multimedia.Midi.InputDevice

    Private numChannel As New NumericUpDown
    Private WithEvents chkAllChannels As New CheckBox

    Private WithEvents comDevices As New ComboBox

#Region "Object stuff"
    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 205) 'Setup the base rectangles.

        'Create one output.
        Outputs(New String() {"Channel Message,ChannelMessage", "SysCommonMessage,SysCommonMessage", "SysExMessage,SysExMessage", "SysRealtimeMessage,SysRealtimeMessage"})

        Inputs(New String() {"Enable,Boolean", "Channel,Number"})

        'Set the title.
        Title = "MIDI Input"
        File = "MIDI\MIDI_Input.vb"

        chkAllChannels.Text = "All channels"
        chkAllChannels.Checked = True
        chkAllChannels.Width = 85
        chkAllChannels.Location = Position + New Point(95, 25)
        AddControl(chkAllChannels)


        numChannel.Minimum = 1
        numChannel.Maximum = 16
        numChannel.Width = 40
        numChannel.Location = Position + New Point(45, 25)
        AddControl(numChannel)



        If Sanford.Multimedia.Midi.InputDevice.DeviceCount > 0 Then
            comDevices.Width = 200
            comDevices.Location = Position
            comDevices.DropDownStyle = ComboBoxStyle.DropDownList

            For i As Integer = 0 To Sanford.Multimedia.Midi.InputDevice.DeviceCount - 1
                comDevices.Items.Add(Sanford.Multimedia.Midi.InputDevice.GetDeviceCapabilities(i).name)
            Next

            AddControl(comDevices)
        Else
            MsgBox("Could not find any MIDI input devices!", MsgBoxStyle.Critical, "Error")
        End If

    End Sub

    Public Overrides Sub Dispose()
        chkAllChannels.Dispose()
        numChannel.Dispose()
        comDevices.Dispose()

        If Device IsNot Nothing Then
            Device.Close()
            Device = Nothing
        End If
        MyBase.Dispose()
    End Sub

    Public Overrides Sub Moving()
        chkAllChannels.Location = Position + New Point(95, 25)
        numChannel.Location = Position + New Point(45, 25)
        comDevices.Location = Position
    End Sub

    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Select Case sender.Index
            Case 0 'Enable
                If Data <> Enabled Then
                    Enabled = Data

                    If Device IsNot Nothing Then
                        If Enabled = True Then
                            Device.StartRecording()
                        Else
                            Device.StopRecording()
                        End If
                    End If
                End If


            Case 1 'Channel
                numChannel.Value = Data
        End Select
    End Sub

    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        MyBase.Draw(g)

        g.DrawString("Channel:", DefaultFont, DefaultFontBrush, Position.X, Position.Y + 28)
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)

        g.GetValue("Enabled", Enabled, False)
        g.GetValue("DeviceID", comDevices.SelectedIndex)
        g.GetValue("AllChannels", chkAllChannels.Checked)
        Try
            g.GetValue("Channel", numChannel.Value)
        Catch ex As Exception
        End Try


        MyBase.Load(g)
    End Sub

    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.SetValue("Enabled", Enabled)
        g.SetValue("DeviceID", comDevices.SelectedIndex)
        g.SetValue("AllChannels", chkAllChannels.Checked)
        g.SetValue("Channel", numChannel.Value)


        Return g
    End Function

#End Region

#Region "Control events"

    Private Sub comDevices_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles comDevices.SelectedIndexChanged
        If comDevices.SelectedIndex = -1 Then Return
        If Device IsNot Nothing Then
               Device.Close()
            Device = Nothing
        End If


        Try
            'Create the device.
            Device = New Sanford.Multimedia.Midi.InputDevice(comDevices.SelectedIndex)
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical, "Error!")
            Device = Nothing
        End Try

        If Enabled Then
            Device.StartRecording()
        End If
    End Sub

    Private Sub chkAllChannels_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkAllChannels.CheckedChanged
        numChannel.Enabled = Not chkAllChannels.Checked
    End Sub
#End Region

#Region "MIDI events"
    Private Sub Device_Error(ByVal sender As Object, ByVal e As Sanford.Multimedia.ErrorEventArgs) Handles Device.Error
        MsgBox(e.Error.Message)
    End Sub

    Private Sub Device_ChannelMessageReceived(ByVal sender As Object, ByVal e As Sanford.Multimedia.Midi.ChannelMessageEventArgs) Handles Device.ChannelMessageReceived
        If Not chkAllChannels.Checked Then
            If Not e.Message.MidiChannel = numChannel.Value - 1 Then Return
        End If

        Send(e.Message, 0)
    End Sub

    Private Sub Device_SysCommonMessageReceived(ByVal sender As Object, ByVal e As Sanford.Multimedia.Midi.SysCommonMessageEventArgs) Handles Device.SysCommonMessageReceived
        Send(e.Message, 1)
    End Sub

    Private Sub Device_SysExMessageReceived(ByVal sender As Object, ByVal e As Sanford.Multimedia.Midi.SysExMessageEventArgs) Handles Device.SysExMessageReceived
        Send(e.Message, 2)
    End Sub

    Private Sub Device_SysRealtimeMessageReceived(ByVal sender As Object, ByVal e As Sanford.Multimedia.Midi.SysRealtimeMessageEventArgs) Handles Device.SysRealtimeMessageReceived
        Send(e.Message, 3)
    End Sub
#End Region

End Class
