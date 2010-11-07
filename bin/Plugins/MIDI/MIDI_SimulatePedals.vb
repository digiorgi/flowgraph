'AddMenuObject|Simulate pedals,Plugins.MIDI_SimulatePedals,120|MIDI
Public Class MIDI_SimulatePedals
    Inherits BaseObject

    Private Enabled As Boolean = True


    Private numChannel As New NumericUpDown
    Private WithEvents chkMergeChannels As New CheckBox
    Private chkRemoveOldNotes As New CheckBox

#Region "Object stuff"
    Public Sub New(ByVal Position As Point, ByVal UserData As String)
        Setup(UserData, Position, 230) 'Setup the base rectangles.

        'Create one output.
        Outputs(New String() {"Channel Message|ChannelMessage"})

        Inputs(New String() {"Enable|Boolean", "Channel Message|ChannelMessage", "Sustain|Boolean", "Sostenuto|Boolean", "Soft|Boolean"})

        'Set the title.
        Title = "Simulate Pedals"

        chkMergeChannels.Text = "Merge channels"
        chkMergeChannels.Width = 115
        chkMergeChannels.Checked = True
        chkMergeChannels.Location = Rect.Location + New Point(110, 50)
        AddControl(chkMergeChannels)

        chkRemoveOldNotes.Text = "Remove old notes"
        chkRemoveOldNotes.Width = 115
        chkRemoveOldNotes.Checked = True
        chkRemoveOldNotes.Location = Rect.Location + New Point(20, 15)
        AddControl(chkRemoveOldNotes)

        numChannel.Minimum = 1
        numChannel.Maximum = 16
        numChannel.Width = 40
        numChannel.Enabled = False
        numChannel.Location = Rect.Location + New Point(65, 50)
        AddControl(numChannel)


    End Sub

    Public Overrides Sub Dispose()
        chkRemoveOldNotes.Dispose()
        chkMergeChannels.Dispose()
        numChannel.Dispose()


        MyBase.Dispose()
    End Sub

    Public Overrides Sub Moving()
    End Sub

    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Select Case sender.Index
            Case 0 'Enable
                If Data <> Enabled Then
                    Enabled = Data
                End If


            Case 1 'ChannelMessage
                If Not Enabled Then Return
                Dim message As Sanford.Multimedia.Midi.ChannelMessageBuilder
                If Data.GetType = GetType(Sanford.Multimedia.Midi.ChannelMessage) Then
                    message = New Sanford.Multimedia.Midi.ChannelMessageBuilder(Data)
                Else
                    message = Data
                End If

                'Is it a note (on or off)?
                If (message.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOn Or _
                    message.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOff) Then

                    'Is the note on? (volume more then 0)
                    If message.Data2 > 0 Then

                        'If alter note for left pedal is checked then  if the pedal is down then lower the volume.
                        If SoftPressed Then
                            message.Data2 = 40
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


                Else
                    Send(message)
                End If


            Case 2 'Sustain pedal
                SustainPressed = Data
                If SustainPressed = False Then
                    ReleaseSustain()
                End If

            Case 3 'Sostenuto pedal
                SostenutoPressed = Data
                If SostenutoPressed = False Then
                    ReleaseSostenuto()
                End If


            Case 4 'Soft pedal
                SoftPressed = Data


        End Select
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)

        g.Get_Value("MessageChannels", chkMergeChannels.Checked)
        Try
            g.Get_Value("Channel", numChannel.Value)
        Catch ex As Exception
        End Try

        MyBase.Load(g)
    End Sub

    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("MessageChannels", chkMergeChannels.Checked)
        g.Set_Value("Channel", numChannel.Value)


        Return g
    End Function
#End Region

#Region "Control events"

    Private Sub chkMergeChannels_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkMergeChannels.CheckedChanged
        numChannel.Enabled = Not chkMergeChannels.Checked
    End Sub
#End Region

#Region "Simulate MIDI pedals"
    Private Note(127) As Byte 'Used to hold notes.

    Private SostenutoList As New List(Of Byte)
    Private SustainList As New List(Of Byte)
    Private SustainPressed, SostenutoPressed, SoftPressed As Boolean

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
    Public Sub ReleaseNote(ByVal ID As Integer)
        Dim tmp As New Sanford.Multimedia.Midi.ChannelMessageBuilder
        tmp.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOff
        tmp.Data1 = ID
        tmp.Data2 = 0
        Note(ID) = Notes.Released
        Send(tmp)
    End Sub

    Friend Sub PressSustain()
        'Check for down keys and set them to sustain.
        For n As Byte = 0 To Note.Length - 1
            If Note(n) = Notes.Pressed Then
                Note(n) = Notes.SustainPressed
                SustainList.Add(n)
            End If
        Next
    End Sub
    Friend Sub ReleaseSustain()
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

    Friend Sub PressSostenuto()
        For n As Byte = 0 To Note.Length - 1
            If Note(n) = Notes.Pressed Or Note(n) = Notes.SustainPressed Then
                Note(n) = Notes.Sostenuto
                SostenutoList.Add(n)
            End If
        Next
    End Sub
    Friend Sub ReleaseSostenuto()
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
