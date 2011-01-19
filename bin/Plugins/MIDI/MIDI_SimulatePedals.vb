'AddMenuObject|Simulate pedals,Plugins.MIDI_SimulatePedals,120|MIDI
'AddReferences(Sanford.Slim.dll)

Public Class MIDI_SimulatePedals
    Inherits BaseObject

    Private Enabled As Boolean = True


    Private numChannel As New NumericUpDown
    Private chkRemoveOldNotes As New CheckBox
    Private chkFilterOtherChannels As New CheckBox

#Region "Object stuff"
    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 145) 'Setup the base rectangles.

        'Create one output.
        Outputs(New String() {"Channel Message,ChannelMessageBuilder"})

        Inputs(New String() {"Enable,Boolean", "Channel Message,ChannelMessage,ChannelMessageBuilder", "Sustain,Boolean", "Sostenuto,Boolean", "Soft,Boolean"})

        'Set the title.
        Title = "Simulate Pedals"

        chkRemoveOldNotes.Text = "Remove old notes"
        chkRemoveOldNotes.Width = 115
        chkRemoveOldNotes.Checked = True
        chkRemoveOldNotes.Location = Position + New Point(5, 40)
        AddControl(chkRemoveOldNotes)

        chkFilterOtherChannels.Text = "Filter out other channels"
        chkFilterOtherChannels.Width = 145
        chkFilterOtherChannels.Checked = False
        chkFilterOtherChannels.Location = Position + New Point(5, 20)
        AddControl(chkFilterOtherChannels)

        numChannel.Minimum = 1
        numChannel.Maximum = 16
        numChannel.Width = 40
        numChannel.Location = Position + New Point(55, 0)
        AddControl(numChannel)


    End Sub

    Public Overrides Sub Dispose()
        chkRemoveOldNotes.Dispose()
        chkFilterOtherChannels.Dispose()
        numChannel.Dispose()


        MyBase.Dispose()
    End Sub

    Public Overrides Sub Moving()
        chkRemoveOldNotes.Location = Position + New Point(5, 40)
        chkFilterOtherChannels.Location = Position + New Point(5, 20)
        numChannel.Location = Position + New Point(55, 0)
    End Sub

    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        MyBase.Draw(g)

        g.DrawString("Channel:", DefaultFont, DefaultFontBrush, Position.X + 5, Position.Y + 3)
    End Sub

    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Select Case sender.Index
            Case 0 'Enable
                If Data <> Enabled Then
                    Enabled = Data
                End If


            Case 1 'ChannelMessage
                If Not Enabled Then Return
                If Data.MidiChannel <> numChannel.Value - 1 Then
                    If chkFilterOtherChannels.Checked Then
                        Return
                    Else
                        Send(Data)
                        Return
                    End If
                End If

                Dim message As Sanford.Multimedia.Midi.ChannelMessageBuilder
                If Data.GetType = GetType(Sanford.Multimedia.Midi.ChannelMessage) Then
                    message = New Sanford.Multimedia.Midi.ChannelMessageBuilder(Data)
                Else
                    message = Data
                End If



                Dim NoteOn As Boolean = False

                'Is it a note (on or off)?
                If (message.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOn) Then
                    If message.Data2 > 0 Then
                        NoteOn = True
                    End If
                End If
                'Is the note on?
                If NoteOn Then
                    'If the pedal is down then lower the volume.
                    If SoftPressed Then
                        message.Data2 = message.Data2 * 0.3
                    End If

                    'Pedals
                    If Note(message.Data1) = Notes.Sostenuto Then
                        If Not SostenutoPressed Then
                            Note(message.Data1) = Notes.Pressed

                        ElseIf chkRemoveOldNotes.Checked Then
                            ReleaseNote(message.Data1)
                            Note(message.Data1) = Notes.Sostenuto
                        End If

                    ElseIf SustainPressed Then
                        If Note(message.Data1) = Notes.SustainReleased And chkRemoveOldNotes.Checked Then
                            ReleaseNote(message.Data1)
                        Else
                            SustainList.Add(message.Data1)
                        End If
                        Note(message.Data1) = Notes.SustainPressed

                    Else
                        Note(message.Data1) = Notes.Pressed
                    End If

                Else

                    Select Case Note(message.Data1)
                        Case Notes.Sostenuto
                            If SostenutoPressed Then
                                Return
                            Else
                                Note(message.Data1) = Notes.Released
                            End If

                        Case Notes.SustainPressed
                            If SustainPressed Then
                                Note(message.Data1) = Notes.SustainReleased
                                Return
                            Else
                                Note(message.Data1) = Notes.Released
                            End If
                        Case Notes.SustainReleased
                            If SustainPressed Then
                                Return
                            Else
                                Note(message.Data1) = Notes.Released
                            End If

                        Case Notes.Pressed
                            Note(message.Data1) = Notes.Released
                            message.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOff
                    End Select
                End If

                Send(message)

            Case 2 'Sustain pedal
                SustainPressed = Data
                If SustainPressed Then
                    PressSustain()
                Else
                    ReleaseSustain()
                End If

            Case 3 'Sostenuto pedal
                SostenutoPressed = Data
                If SostenutoPressed Then
                    PressSostenuto()
                Else
                    ReleaseSostenuto()
                End If


            Case 4 'Soft pedal
                SoftPressed = Data


        End Select
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)

        g.Get_Value("Enabled", Enabled, False)
        g.Get_Value("RemoveOldNotes", chkRemoveOldNotes.Checked, False)
        g.Get_Value("FilterOtherChannels", chkFilterOtherChannels.Checked, False)
        g.Get_Value("Channel", numChannel.Value, False)

        MyBase.Load(g)
    End Sub

    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("Enabled", Enabled, True)
        g.Set_Value("RemoveOldNotes", chkRemoveOldNotes.Checked, True)
        g.Set_Value("FilterOtherChannels", chkFilterOtherChannels.Checked, False)
        g.Set_Value("Channel", numChannel.Value, 1)


        Return g
    End Function
#End Region

#Region "Simulate MIDI pedals"
    Private Note(127) As Byte 'Used to hold notes.

    Private SostenutoList As New List(Of Byte)
    Private SustainList As New List(Of Byte)
    Private SustainPressed As Boolean = False
    Private SostenutoPressed As Boolean = False
    Private SoftPressed As Boolean = False

    Private Enum Notes
        Released = 0
        Pressed = 1
        Sostenuto = 2 'Is the sostenuto pedal holding the note.
        SustainPressed = 3 'Sustain pedal is down And the note.
        SustainReleased = 4 'Meaning the note was released but the sustain pedal is still down.
    End Enum

    ''' <summary>
    ''' Release note at ID
    ''' </summary>
    ''' <param name="ID"></param>
    ''' <remarks></remarks>
    Private Sub ReleaseNote(ByVal ID As Integer)
        Dim tmp As New Sanford.Multimedia.Midi.ChannelMessageBuilder
        tmp.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOff
        tmp.Data1 = ID
        tmp.Data2 = 0
        Note(ID) = Notes.Released
        Send(tmp)
    End Sub

    Private Sub PressSustain()
        'Check for down keys and set them to sustain.
        For n As Byte = 0 To Note.Length - 1
            If Note(n) = Notes.Pressed Then
                Note(n) = Notes.SustainPressed
                SustainList.Add(n)
            End If
        Next

    End Sub
    Private Sub ReleaseSustain()
        Dim tmp As New Sanford.Multimedia.Midi.ChannelMessageBuilder
        tmp.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOff
        tmp.Data2 = 0
        For Each n As Byte In SustainList
            If Note(n) = Notes.SustainReleased Then
                Note(n) = Notes.Released
                tmp.Data1 = n
                Send(tmp)
            End If
        Next
        SustainList.Clear()
    End Sub

    Private Sub PressSostenuto()
        For n As Byte = 0 To Note.Length - 1
            If Note(n) = Notes.Pressed Or Note(n) = Notes.SustainPressed Then
                Note(n) = Notes.Sostenuto
                SostenutoList.Add(n)
            End If
        Next
    End Sub
    Private Sub ReleaseSostenuto()
        Dim tmp As New Sanford.Multimedia.Midi.ChannelMessageBuilder
        tmp.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOff
        tmp.MidiChannel = 0
        tmp.Data2 = 0
        For Each n As Byte In SostenutoList
            Note(n) = Notes.Released
            tmp.Data1 = n
            Send(tmp)
        Next
        SostenutoList.Clear()
    End Sub

#End Region

End Class
