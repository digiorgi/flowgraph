'AddMenuObject|Output,Plugins.MIDI_Output|MIDI
Public Class MIDI_Output
    Inherits BaseObject

    Private Enabled As Boolean = True

    Private WithEvents Device As Sanford.Multimedia.Midi.OutputDevice

    Private numChannel As New NumericUpDown
    Private WithEvents chkMessageChannels As New CheckBox

    Private WithEvents comDevices As New ComboBox

#Region "Object stuff"
    Public Sub New(ByVal Position As Point, ByVal UserData As String)
        Setup(UserData, Position, 230) 'Setup the base rectangles.

        Inputs(New String() {"Enable,Boolean", _
                             "Channel Message,ChannelMessage,ChannelMessageBuilder", "SysCommonMessage,SysCommonMessage,SysCommonMessageBuilder", _
                             "SysExMessage,SysExMessage", "SysRealtimeMessage,SysRealtimeMessage"})

        'Set the title.
        Title = "MIDI Output"

        chkMessageChannels.Text = "Same as message"
        chkMessageChannels.Width = 115
        chkMessageChannels.Checked = True
        chkMessageChannels.Location = Rect.Location + New Point(110, 50)
        AddControl(chkMessageChannels)


        numChannel.Minimum = 1
        numChannel.Maximum = 16
        numChannel.Width = 40
        numChannel.Enabled = False
        numChannel.Location = Rect.Location + New Point(65, 50)
        AddControl(numChannel)



        If Sanford.Multimedia.Midi.OutputDevice.DeviceCount > 0 Then
            comDevices.Width = 200
            comDevices.Location = Rect.Location + New Point(15, 25)
            comDevices.DropDownStyle = ComboBoxStyle.DropDownList

            For i As Integer = 0 To Sanford.Multimedia.Midi.OutputDevice.DeviceCount - 1
                comDevices.Items.Add(Sanford.Multimedia.Midi.OutputDevice.GetDeviceCapabilities(i).name)
            Next
            comDevices.SelectedIndex = 0

            AddControl(comDevices)
        Else
            MsgBox("Could not find any MIDI output devices!", MsgBoxStyle.Critical, "Error")
        End If

    End Sub

    Public Overrides Sub Dispose()
        chkMessageChannels.Dispose()
        numChannel.Dispose()
        comDevices.Dispose()

        If Device IsNot Nothing Then
            Device.Close()
            Device = Nothing
        End If
        MyBase.Dispose()
    End Sub

    Public Overrides Sub Moving()
        chkMessageChannels.Location = Rect.Location + New Point(110, 50)
        numChannel.Location = Rect.Location + New Point(65, 50)
        comDevices.Location = Rect.Location + New Point(15, 25)
    End Sub

    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Select Case sender.Index
            Case 0 'Enable
                If Data <> Enabled Then
                    Enabled = Data
                End If


            Case 1 'ChannelMessage
                If Not Enabled Then Return

                If chkMessageChannels.Checked Then
                    If Data.GetType = GetType(Sanford.Multimedia.Midi.ChannelMessageBuilder) Then
                        Data.Build()
                        Device.Send(Data.Result)
                    ElseIf Data.GetType = GetType(Sanford.Multimedia.Midi.ChannelMessage) Then
                        Device.Send(Data)
                    Else
                        MsgBox("Not ChannelMessageBuilder or ChannelMessage", MsgBoxStyle.Critical, "Error")
                        Return
                    End If
                Else

                    Dim message As Sanford.Multimedia.Midi.ChannelMessageBuilder
                    If Data.GetType = GetType(Sanford.Multimedia.Midi.ChannelMessageBuilder) Then
                        message = Data
                    ElseIf Data.GetType = GetType(Sanford.Multimedia.Midi.ChannelMessage) Then
                        message = New Sanford.Multimedia.Midi.ChannelMessageBuilder(Data)
                    Else
                        MsgBox("Not ChannelMessageBuilder or ChannelMessage", MsgBoxStyle.Critical, "Error")
                        Return
                    End If

                    message.MidiChannel = numChannel.Value - 1
                    message.Build()
                    Device.Send(message.Result)

                End If


            Case 2 'SysCommonMessage
                If Not Enabled Then Return
                If Data.GetType = GetType(Sanford.Multimedia.Midi.SysCommonMessageBuilder) Then
                    Dim message As Sanford.Multimedia.Midi.SysCommonMessageBuilder = Data
                    message.Build()
                    Device.Send(message.Result)
                ElseIf Data.GetType = GetType(Sanford.Multimedia.Midi.SysCommonMessage) Then
                    Device.Send(Data)
                Else
                    MsgBox("Not SysCommonMessageBuilder or SysCommonMessage", MsgBoxStyle.Critical, "Error")
                    Return
                End If

            Case 3 'SysExMessage
                If Not Enabled Then Return
                Device.Send(Data)

            Case 4 'SysRealtimeMessage
                If Not Enabled Then Return
                Device.Send(Data)
        End Select
    End Sub

    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        MyBase.Draw(g)

        g.DrawString("Channel:", DefaultFont, DefaultFontBrush, Rect.X + 15, Rect.Y + 53)
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)

        g.Get_Value("Enabled", Enabled, False)
        g.Get_Value("DeviceID", comDevices.SelectedIndex)
        g.Get_Value("MessageChannels", chkMessageChannels.Checked)
        Try
            g.Get_Value("Channel", numChannel.Value)
        Catch ex As Exception
        End Try

        MyBase.Load(g)
    End Sub

    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("Enabled", Enabled)
        g.Set_Value("DeviceID", comDevices.SelectedIndex)
        g.Set_Value("MessageChannels", chkMessageChannels.Checked)
        g.Set_Value("Channel", numChannel.Value)


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
            Device = New Sanford.Multimedia.Midi.OutputDevice(comDevices.SelectedIndex)
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical, "Error!")
            Device = Nothing
        End Try
    End Sub

    Private Sub chkAllChannels_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkMessageChannels.CheckedChanged
        numChannel.Enabled = Not chkMessageChannels.Checked
    End Sub
#End Region

#Region "MIDI events"
    Private Sub Device_Error(ByVal sender As Object, ByVal e As Sanford.Multimedia.ErrorEventArgs) Handles Device.Error
        MsgBox(e.Error.Message)
    End Sub
#End Region

End Class
