'AddMenuObject|Input,Plugins.MIDI_Input|MIDI
Public Class MIDI_Input
    Inherits BaseObject

    Private Enabled As Boolean = False

    Private WithEvents Device As Sanford.Multimedia.Midi.InputDevice

    Private WithEvents numChannel As New NumericUpDown
    Private WithEvents comDevices As New ComboBox

#Region "Object stuff"
    Public Sub New(ByVal Position As Point, ByVal UserData As String)
        Setup(UserData, Position, 230) 'Setup the base rectangles.

        'Create one output.
        Outputs(New String() {"Channel Message|ChannelMessage", "SysCommonMessage|SysCommonMessage", "SysExMessage|SysExMessage", "SysRealtimeMessage|SysRealtimeMessage"})

        Inputs(New String() {"Enable|Boolean", "Channel|Number"})

        'Set the title.
        Title = "Timer"


        numChannel.Minimum = 1
        numChannel.Maximum = 16
        numChannel.Width = 60
        numChannel.Location = Rect.Location + New Point(70, 50)
        AddControl(numChannel)

      

        If Sanford.Multimedia.Midi.InputDevice.DeviceCount > 0 Then
            comDevices.Width = 200
            comDevices.Location = Rect.Location + New Point(15, 25)
            comDevices.DropDownStyle = ComboBoxStyle.DropDownList

            For i As Integer = 0 To Sanford.Multimedia.Midi.InputDevice.DeviceCount - 1
                comDevices.Items.Add(Sanford.Multimedia.Midi.InputDevice.GetDeviceCapabilities(i).name)
            Next
            comDevices.SelectedIndex = 0

            AddControl(comDevices)
        Else
            MsgBox("Could not find any MIDI input devices!", MsgBoxStyle.Critical, "Error")
        End If

    End Sub

    Public Overrides Sub Dispose()
        numChannel.Dispose()
        comDevices.Dispose()

        If Device IsNot Nothing Then
            Device.StopRecording()
            Device.Dispose()
            Device = Nothing
        End If
        MyBase.Dispose()
    End Sub

    Public Overrides Sub Moving()
        numChannel.Location = Rect.Location + New Point(70, 50)
        comDevices.Location = Rect.Location + New Point(15, 25)
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

#End Region

#Region "Control events"

    Private Sub comDevices_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles comDevices.SelectedIndexChanged
        If comDevices.SelectedIndex = 0 Then Return
        If Device IsNot Nothing Then
            Device.Dispose()
        End If


        Try
            'Create the device.
            Device = New Sanford.Multimedia.Midi.InputDevice(comDevices.SelectedIndex - 1)
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical, "Error!")
            Device = Nothing
        End Try

        If Enabled Then
            Device.StartRecording()
        End If
    End Sub
    Private Sub numChannel_ValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles numChannel.ValueChanged

    End Sub

#End Region

#Region "MIDI events"
    Private Sub Device_Error(ByVal sender As Object, ByVal e As Sanford.Multimedia.ErrorEventArgs) Handles Device.Error
        MsgBox(e.Error.Message)
    End Sub


    Private Sub Device_ChannelMessageReceived(ByVal sender As Object, ByVal e As Sanford.Multimedia.Midi.ChannelMessageEventArgs) Handles Device.ChannelMessageReceived
        MsgBox("Input")
    End Sub

    Private Sub Device_SysCommonMessageReceived(ByVal sender As Object, ByVal e As Sanford.Multimedia.Midi.SysCommonMessageEventArgs) Handles Device.SysCommonMessageReceived
        MsgBox("Input")
    End Sub

    Private Sub Device_SysExMessageReceived(ByVal sender As Object, ByVal e As Sanford.Multimedia.Midi.SysExMessageEventArgs) Handles Device.SysExMessageReceived
        MsgBox("Input")
    End Sub

    Private Sub Device_SysRealtimeMessageReceived(ByVal sender As Object, ByVal e As Sanford.Multimedia.Midi.SysRealtimeMessageEventArgs) Handles Device.SysRealtimeMessageReceived
        MsgBox("Input")
    End Sub
#End Region

End Class
