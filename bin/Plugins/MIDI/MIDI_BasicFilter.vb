'AddMenuObject|Basic,Plugins.MIDI_BasicFilter,70|MIDI,Channel Message,Filters
'AddReferences(Sanford.Slim.dll)
Public Class MIDI_BasicFilter
    Inherits BaseObject

    Private Enabled As Boolean = True

    Private WithEvents chkAllChannels As New CheckBox
    Private numChannel As New NumericUpDown

    Private chkNotes As New CheckBox
    Private chkControllers As New CheckBox
    Private chkInstrument As New CheckBox

#Region "Object stuff"
    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 180, 80) 'Setup the base rectangles.

        'Create one output.
        Outputs(New String() {"Channel Message,ChannelMessageBuilder"})

        Inputs(New String() {"Enable,Boolean", "Channel Message,ChannelMessageBuilder,ChannelMessage"})

        'Set the title.
        Title = "MIDI Basic filter"
        File = "MIDI\MIDI_BasicFilter.vb"

        chkAllChannels.Text = "All channels"
        chkAllChannels.Checked = True
        chkAllChannels.AutoSize = True
        chkAllChannels.Location = Position + New Point(85, 0)
        AddControl(chkAllChannels)

        numChannel.Minimum = 1
        numChannel.Maximum = 16
        numChannel.Width = 40
        numChannel.Location = Position + New Point(45, 0)
        AddControl(numChannel)


        chkNotes.Text = "Enable notes"
        chkNotes.Checked = True
        chkNotes.AutoSize = True
        chkNotes.Location = Position + New Point(0, 20)
        AddControl(chkNotes)

        chkControllers.Text = "Enable controllers"
        chkControllers.Checked = True
        chkControllers.AutoSize = True
        chkControllers.Location = Position + New Point(0, 40)
        AddControl(chkControllers)

        chkInstrument.Text = "Enable instrument"
        chkInstrument.Checked = True
        chkInstrument.AutoSize = True
        chkInstrument.Location = Position + New Point(0, 60)
        AddControl(chkInstrument)
    End Sub

    Public Overrides Sub Dispose()
        chkAllChannels.Dispose()
        numChannel.Dispose()

        chkNotes.Dispose()
        chkControllers.Dispose()
        chkInstrument.Dispose()

        MyBase.Dispose()
    End Sub

    Public Overrides Sub Moving()
        chkAllChannels.Location = Position + New Point(95, 0)
        numChannel.Location = Position + New Point(45, 0)
        chkNotes.Location = Position + New Point(0, 20)
        chkInstrument.Location = Position + New Point(0, 40)
        chkControllers.Location = Position + New Point(0, 60)
    End Sub

    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Select Case sender.Index
            Case 0 'Enable
                If Data <> Enabled Then
                    Enabled = Data
                End If


            Case 1 'Channel Message
                If Not Enabled Then Return

                Dim message As Sanford.Multimedia.Midi.ChannelMessageBuilder
                If Data.GetType = GetType(Sanford.Multimedia.Midi.ChannelMessage) Then
                    message = New Sanford.Multimedia.Midi.ChannelMessageBuilder(Data)
                Else
                    message = Data
                End If

                If Not (chkAllChannels.Checked OrElse message.MidiChannel = numChannel.Value - 1) Then
                    Send(message) 'ToDo: Should really have filterd and unfilterd but I do not know the proper names...
                    Return
                End If

                Select Case message.Command
                    Case Sanford.Multimedia.Midi.ChannelCommand.NoteOn, Sanford.Multimedia.Midi.ChannelCommand.NoteOff
                        If chkNotes.Checked Then Send(message)

                    Case Sanford.Multimedia.Midi.ChannelCommand.ProgramChange
                        If chkInstrument.Checked Then Send(message)

                    Case Sanford.Multimedia.Midi.ChannelCommand.Controller
                        If chkControllers.Checked Then Send(message)


                    Case Else
                        Send(message)
                End Select

        End Select
    End Sub

    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        MyBase.Draw(g)

        g.DrawString("Channel:", DefaultFont, DefaultFontBrush, Position.X, Position.Y + 3)
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)

        g.GetValue("Enabled", Enabled, False)
        g.GetValue("AllChannels", chkAllChannels.Checked, False)
        g.GetValue("Channel", numChannel.Value, False)
        g.GetValue("Controllers", chkControllers.Checked, False)
        g.GetValue("Instrument", chkInstrument.Checked, False)
        g.GetValue("Notes", chkNotes.Checked, False)
        MyBase.Load(g)
    End Sub

    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.setValue("Enabled", Enabled, False)
        g.SetValue("AllChannels", chkAllChannels.Checked, False)
        g.SetValue("Channel", numChannel.Value, False)
        g.SetValue("Controllers", chkControllers.Checked, False)
        g.SetValue("Instrument", chkInstrument.Checked, False)
        g.SetValue("Notes", chkNotes.Checked, False)
        Return g
    End Function

    Private Sub chkAllChannels_CheckedChanged(sender As Object, e As System.EventArgs) Handles chkAllChannels.CheckedChanged
        numChannel.Enabled = Not chkAllChannels.Checked
    End Sub
#End Region


End Class
