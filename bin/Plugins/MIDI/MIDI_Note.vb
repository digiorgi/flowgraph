'AddMenuObject|Get,Plugins.MIDI_GetNote|MIDI,Channel Message,Note
'AddMenuObject|Set,Plugins.MIDI_SetNote|MIDI,Channel Message,Note
'AddReferences(Sanford.Slim.dll)
Public Class MIDI_GetNote
    Inherits BaseObject

    Private Enabled As Boolean = True


    Private numChannel As New NumericUpDown
    Private WithEvents chkAllChannels As New CheckBox

    Private numNote As New NumericUpDown

#Region "Object stuff"
    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 130, 50) 'Setup the base rectangles.

        'Create one output.
        Outputs(New String() {"Pressed,Boolean", "Volume,0-1Normalized"})
        Inputs(New String() {"Enable,Boolean", "Channel Message,ChannelMessage,ChannelMessageBuilder"})

        'Set the title.
        Title = "MIDI get note"
        File = "MIDI\MIDI_Note.vb"

        numChannel.Minimum = 1
        numChannel.Maximum = 16
        numChannel.Width = 40
        numChannel.Enabled = False
        numChannel.Location = Position + New Point(45, 0)
        AddControl(numChannel)

        chkAllChannels.Text = "All"
        chkAllChannels.AutoSize = True
        chkAllChannels.Checked = True
        chkAllChannels.Location = Position + New Point(86, 0)
        AddControl(chkAllChannels)

        numNote.Location = Position + New Point(45, 20)
        numNote.Minimum = 0
        numNote.Maximum = 127
        numNote.Value = 60
        numNote.Width = 50
        AddControl(numNote)

    End Sub

    Public Overrides Sub Dispose()
        numChannel.Dispose()
        chkAllChannels.Dispose()
        numNote.Dispose()

        MyBase.Dispose()
    End Sub

    Public Overrides Sub Moving()
        numChannel.Location = Position + New Point(45, 0)
        chkAllChannels.Location = Position + New Point(86, 0)
        numNote.Location = Position + New Point(45, 20)
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

                If Not (Data.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOn Or Data.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOff) Then Return
                If Not Data.Data1 = numNote.Value Then Return

                Dim Pressed As Boolean = False
                If Data.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOn And Data.data2 > 0 Then
                    Pressed = True
                End If

                Send(Pressed, 0)
                Send(Data.data2, 1)
        End Select
    End Sub

    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        MyBase.Draw(g)

        g.DrawString("Channel:", DefaultFont, DefaultFontBrush, Position.X, Position.Y + 3)
        g.DrawString("Note:", DefaultFont, DefaultFontBrush, Position.X, Position.Y + 23)
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)
        g.GetValue("Enabled", Enabled, False)
        g.GetValue("Note", numNote.Value, False)
        g.GetValue("AllChannels", chkAllChannels.Checked, False)
        g.GetValue("Channel", numChannel.Value, False)

        MyBase.Load(g)
    End Sub

    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.SetValue("Enabled", Enabled)
        g.SetValue("Note", numNote.Value)
        g.SetValue("AllChannels", chkAllChannels.Checked)
        g.SetValue("Channel", numChannel.Value)

        Return g
    End Function
#End Region

#Region "Control events"
    Private Sub chkAllChannels_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkAllChannels.CheckedChanged
        numChannel.Enabled = Not chkAllChannels.Checked
    End Sub
#End Region

End Class



Public Class MIDI_SetNote
    Inherits BaseObject

    Private Enabled As Boolean = True


    Private numChannel, numNote, numVolume As New NumericUpDown

#Region "Object stuff"
    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 90, 65) 'Setup the base rectangles.

        'Create one output.
        Outputs(New String() {"Channel Message,ChannelMessageBuilder"})
        Inputs(New String() {"Enable,Boolean", "Pressed,Boolean", "Note,0-1Normalized", "Volume,0-1Normalized"})

        'Set the title.
        Title = "MIDI set note"
        File = "MIDI\MIDI_Note.vb"

        numChannel.Minimum = 1
        numChannel.Maximum = 16
        numChannel.Width = 40
        numChannel.Location = Position + New Point(45, 0)
        AddControl(numChannel)

        numNote.Location = Position + New Point(45, 20)
        numNote.Minimum = 0
        numNote.Maximum = 127
        numNote.Value = 60
        numNote.Width = 50
        AddControl(numNote)

        numVolume.Location = Position + New Point(45, 40)
        numVolume.Minimum = 0
        numVolume.Maximum = 127
        numVolume.Value = 127
        numVolume.Width = 50
        AddControl(numVolume)
    End Sub

    Public Overrides Sub Dispose()
        numChannel.Dispose()
        numVolume.Dispose()
        numNote.Dispose()

        MyBase.Dispose()
    End Sub

    Public Overrides Sub Moving()
        numChannel.Location = Position + New Point(45, 0)
        numNote.Location = Position + New Point(45, 20)
        numVolume.Location = Position + New Point(45, 40)
    End Sub

    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Select Case sender.Index
            Case 0 'Enable
                If Data <> Enabled Then
                    Enabled = Data
                End If


            Case 1 'Note pressed
                If Not Enabled Then Return

                Dim message As New Sanford.Multimedia.Midi.ChannelMessageBuilder
                message.MidiChannel = numChannel.Value - 1
                If Data = True Then
                    message.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOn
                Else
                    message.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOff
                End If
                message.Data1 = numNote.Value
                message.Data2 = numVolume.Value
                Send(message)

            Case 2 'Note
                If Not Enabled Then Return
                numNote = Data * 127

            Case 3 'Volume
                If Not Enabled Then Return
                numVolume.Value = Data * 127
        End Select
    End Sub

    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        MyBase.Draw(g)

        g.DrawString("Channel:", DefaultFont, DefaultFontBrush, Position.X, Position.Y + 3)
        g.DrawString("Note:", DefaultFont, DefaultFontBrush, Position.X, Position.Y + 23)
        g.DrawString("Volume:", DefaultFont, DefaultFontBrush, Position.X, Position.Y + 43)
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)
        g.GetValue("Enabled", Enabled, False)
        g.GetValue("Note", numNote.Value, False)
        g.GetValue("Volume", numVolume.Value, False)
        g.GetValue("Channel", numChannel.Value, False)

        MyBase.Load(g)
    End Sub

    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.SetValue("Enabled", Enabled)
        g.SetValue("Note", numNote.Value)
        g.SetValue("Volume", numVolume.Value)
        g.SetValue("Channel", numChannel.Value)

        Return g
    End Function
#End Region

End Class