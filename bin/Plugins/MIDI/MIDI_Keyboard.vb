﻿'AddMenuObject|128keys,Plugins.MIDI_Keyboard,120,128-0-0|MIDI,Keyboards
'AddMenuObject|88 keys,Plugins.MIDI_Keyboard,120,88-21-9|MIDI,Keyboards
'AddMenuObject|61 keys,Plugins.MIDI_Keyboard,120,61-36-0|MIDI,Keyboards
'AddMenuObject|24 keys,Plugins.MIDI_Keyboard,120,24-60-0|MIDI,Keyboards
'AddReferences(Sanford.Slim.dll)

Public Class MIDI_Keyboard
    Inherits BaseObject

    Private Enabled As Boolean = True


    Private numChannel As New NumericUpDown
    Private chkFilterOtherChannels As New CheckBox

    Private NumKeys As Byte
    Private Offset, OctaveOffset As Byte
    Private Width As Integer

#Region "Object stuff"
    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Try
            Dim data() As String = Split(UserData, "-")
            NumKeys = data(0)
            Offset = data(1)
            OctaveOffset = data(2)
        Catch ex As Exception
            NumKeys = 24
            Offset = 60
            OctaveOffset = 0
        End Try
        Width = NumKeys * 5.9

        Setup(UserData, StartPosition, Width, 95) 'Setup the base rectangles.

        'Create one output.
        Outputs(New String() {"Channel Message,ChannelMessageBuilder"})
        Inputs(New String() {"Enable,Boolean", "Channel Message,ChannelMessage,ChannelMessageBuilder"})


        'Set the title.
        Title = "MIDI Keyboard"

        chkFilterOtherChannels.Text = "Filter out other channels"
        chkFilterOtherChannels.Width = 139
        chkFilterOtherChannels.Height = 15
        chkFilterOtherChannels.Checked = False
        chkFilterOtherChannels.Location = Position + New Point(5, 19)
        AddControl(chkFilterOtherChannels)

        numChannel.Minimum = 1
        numChannel.Maximum = 16
        numChannel.Width = 40
        numChannel.Location = Position + New Point(55, 0)
        AddControl(numChannel)

        ReDim Note(NumKeys - 1)
        For i As Integer = 0 To Note.Length - 1
            Note(i) = New NoteT
        Next
    End Sub

    Public Overrides Sub Dispose()
        chkFilterOtherChannels.Dispose()
        numChannel.Dispose()

        MyBase.Dispose()
    End Sub

    Public Overrides Sub Moving()
        chkFilterOtherChannels.Location = Position + New Point(5, 19)
        numChannel.Location = Position + New Point(55, 0)
    End Sub

    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        MyBase.Draw(g)

        g.DrawString("Channel:", DefaultFont, DefaultFontBrush, Position.X + 5, Position.Y + 3)

        g.FillRectangle(Brushes.White, Position.X, Position.Y + 40, Width, 50)

        Dim OctavePos As SByte = OctaveOffset - 1
        Dim x As Integer = Position.X
        Dim y As Integer = Position.Y + 40
        Dim p As SByte = -1

        For n As Integer = 0 To NumKeys - 1
            OctavePos += 1
            If OctavePos = 12 Then OctavePos = 0
            Select Case OctavePos
                Case 0, 2, 4, 5, 7, 9, 11
                    p += 1
            End Select

            Dim Note As NoteT = Me.Note(n)
            Dim brush As Brush = Brushes.Black
            If Not chkFilterOtherChannels.Checked Then
                If Note.Channel(0) Then
                    brush = Brushes.Blue
                ElseIf Note.Channel(1) Then
                    brush = Brushes.Aqua
                ElseIf Note.Channel(2) Then
                    brush = Brushes.Green
                ElseIf Note.Channel(3) Then
                    brush = Brushes.Red
                ElseIf Note.Channel(4) Then
                    brush = Brushes.Purple
                ElseIf Note.Channel(5) Then
                    brush = Brushes.Brown
                ElseIf Note.Channel(6) Then
                    brush = Brushes.Gray
                ElseIf Note.Channel(7) Then
                    brush = Brushes.Orange
                ElseIf Note.Channel(8) Then
                    brush = Brushes.Teal
                ElseIf Note.Channel(9) Then
                    brush = Brushes.Yellow
                ElseIf Note.Channel(10) Then
                    brush = Brushes.BlueViolet
                ElseIf Note.Channel(11) Then
                    brush = Brushes.LawnGreen
                ElseIf Note.Channel(12) Then
                    brush = Brushes.Pink
                ElseIf Note.Channel(13) Then
                    brush = Brushes.Tan
                ElseIf Note.Channel(14) Then
                    brush = Brushes.DarkGreen
                ElseIf Note.Channel(15) Then
                    brush = Brushes.Coral
                End If
            Else
                If Note.Channel(numChannel.Value - 1) Then
                    brush = Brushes.Blue
                End If
            End If


            Select Case OctavePos
                Case 0, 5 'C & F
                    If brush IsNot Brushes.Black Then
                        g.FillRectangle(brush, x + (10 * p), y, 6, 25)
                        g.FillRectangle(brush, x + (10 * p), y + 25, 10, 25)
                    End If

                Case 1, 6 'C# & F#
                    g.FillRectangle(brush, x + (10 * p) + 6, y, 6, 25)

                Case 2 'D
                    If brush IsNot Brushes.Black Then
                        g.FillRectangle(brush, x + (10 * p) + 2, y, 5, 25)
                        g.FillRectangle(brush, x + (10 * p), y + 25, 10, 25)
                    End If

                Case 3, 10 'D# & A#
                    g.FillRectangle(brush, x + (10 * p) + 7, y, 6, 25)

                Case 4, 11 'E & B
                    If brush IsNot Brushes.Black Then
                        g.FillRectangle(brush, x + (10 * p) + 3, y, 7, 25)
                        g.FillRectangle(brush, x + (10 * p), y + 25, 10, 25)
                    End If

                Case 7 'G
                    If brush IsNot Brushes.Black Then
                        g.FillRectangle(brush, x + (10 * p) + 2, y, 5, 25)
                        g.FillRectangle(brush, x + (10 * p), y + 25, 10, 25)
                    End If

                Case 8 'G#
                    g.FillRectangle(brush, x + (10 * p) + 7, y, 6, 25)

                Case 9 'A
                    If brush IsNot Brushes.Black Then
                        g.FillRectangle(brush, x + (10 * p) + 2, y, 5, 25)
                        g.FillRectangle(brush, x + (10 * p), y + 25, 10, 25)
                    End If
            End Select
        Next

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
                    Else
                        NoteOn = False
                    End If

                ElseIf message.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOff Then
                    NoteOn = False


                ElseIf message.Command = Sanford.Multimedia.Midi.ChannelCommand.Controller And _
                    (message.Data1 = Sanford.Multimedia.Midi.ControllerType.AllSoundOff Or message.Data1 = Sanford.Multimedia.Midi.ControllerType.AllNotesOff) Then
                    ResetNotes()
                    GoTo Send
                End If

                'Check to see if the note is within the keyboard size.
                If message.Data1 >= Offset And message.Data1 + 1 <= Offset + NumKeys Then
                    'Is the note on? (volume more then 0)
                    If NoteOn Then
                        Note(message.Data1 - Offset).Press(message.MidiChannel)
                    Else
                        Note(message.Data1 - Offset).Release(message.MidiChannel)
                    End If
                End If




Send:
                Send(message)
                DoDraw(Rect)
                'DoDraw(True)
        End Select
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)

        g.GetValue("Enabled", Enabled, False)
        g.GetValue("FilterOtherChannels", chkFilterOtherChannels.Checked, False)
        g.GetValue("Channel", numChannel.Value, False)

        MyBase.Load(g)
    End Sub

    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.SetValue("Enabled", Enabled, True)
        g.SetValue("FilterOtherChannels", chkFilterOtherChannels.Checked, False)
        g.SetValue("Channel", numChannel.Value, 1)


        Return g
    End Function

    Private LastNote As SByte = -1
    Public Overrides Sub MouseDown(ByVal e As System.Windows.Forms.MouseEventArgs)
        PressNote(GetNote)
        MyBase.MouseDown(e)
    End Sub
    Public Overrides Sub MouseUp(ByVal e As System.Windows.Forms.MouseEventArgs)
        ReleaseNote(GetNote)
        MyBase.MouseUp(e)
    End Sub
    Public Overrides Sub MouseMove(ByVal e As System.Windows.Forms.MouseEventArgs)
        If e.Button <> MouseButtons.None Then
            Dim note As Integer = GetNote()
            If note <> LastNote Then
                ReleaseNote(LastNote)
                PressNote(note)
            End If
        End If
        MyBase.MouseMove(e)
    End Sub
    Public Function GetNote() As Integer
        'Is the mouse over the keyboard?
        Dim x As Integer = Position.X
        Dim y As Integer = Position.Y + 40
        If Mouse.IntersectsWith(New Rectangle(x, y, Width, 50)) And Enabled Then
            Dim OctavePos As SByte = OctaveOffset - 1
            Dim p As SByte = -1

            For n As Integer = 0 To NumKeys - 1
                OctavePos += 1
                If OctavePos = 12 Then OctavePos = 0
                Select Case OctavePos
                    Case 0, 2, 4, 5, 7, 9, 11
                        p += 1
                End Select

                Select Case OctavePos
                    Case 0, 5 'C & F
                        If Mouse.IntersectsWith(New Rectangle(x + (10 * p), y, 6, 25)) Or _
                            Mouse.IntersectsWith(New Rectangle(x + (10 * p), y + 25, 10, 25)) Then
                            Return n
                        End If

                    Case 1, 6 'C# & F#
                        If Mouse.IntersectsWith(New Rectangle(x + (10 * p) + 6, y, 6, 25)) Then
                            Return n
                        End If


                    Case 2 'D
                        If Mouse.IntersectsWith(New Rectangle(x + (10 * p) + 2, y, 5, 25)) Or _
                            Mouse.IntersectsWith(New Rectangle(x + (10 * p), y + 25, 10, 25)) Then
                            Return n
                        End If

                    Case 3, 10 'D# & A#
                        If Mouse.IntersectsWith(New Rectangle(x + (10 * p) + 7, y, 6, 25)) Then
                            Return n
                        End If

                    Case 4, 11 'E & B
                        If Mouse.IntersectsWith(New Rectangle(x + (10 * p) + 3, y, 7, 25)) Or _
                            Mouse.IntersectsWith(New Rectangle(x + (10 * p), y + 25, 10, 25)) Then
                            Return n
                        End If

                    Case 7 'G
                        If Mouse.IntersectsWith(New Rectangle(x + (10 * p) + 2, y, 5, 25)) Or _
                            Mouse.IntersectsWith(New Rectangle(x + (10 * p), y + 25, 10, 25)) Then
                            Return n
                        End If

                    Case 8 'G#
                        If Mouse.IntersectsWith(New Rectangle(x + (10 * p) + 7, y, 6, 25)) Then
                            Return n
                        End If

                    Case 9 'A
                        If Mouse.IntersectsWith(New Rectangle(x + (10 * p) + 2, y, 5, 25)) Or _
                            Mouse.IntersectsWith(New Rectangle(x + (10 * p), y + 25, 10, 25)) Then
                            Return n
                        End If


                End Select
            Next
        End If
        Return -1
    End Function
#End Region

#Region "Misc stuff"
    Private Note() As NoteT

    Private Class NoteT
        Public Channel(15) As Boolean

        Public Function Pressed() As Boolean
            For Each ch As Boolean In Channel
                If ch Then Return True
            Next
            Return False
        End Function

        Public Sub Press(ByVal Channel As Byte)
            Me.Channel(Channel) = True
        End Sub
        Public Sub Release(ByVal Channel As Byte)
            Me.Channel(Channel) = False
        End Sub
        Public Sub Reset()
            For i As Integer = 0 To 15
                Channel(i) = False
            Next
        End Sub

    End Class

    Private Sub ResetNotes()
        For i As Integer = 0 To Note.Length - 1
            Note(i).Reset()
        Next
        DoDraw(True)
    End Sub

    ''' <summary>
    ''' Release note at ID
    ''' </summary>
    ''' <param name="ID"></param>
    ''' <remarks></remarks>
    Private Sub ReleaseNote(ByVal ID As Integer)
        If ID < 0 Then Return
        If ID = LastNote Then LastNote = -1
        Dim tmp As New Sanford.Multimedia.Midi.ChannelMessageBuilder
        tmp.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOff
        tmp.MidiChannel = numChannel.Value - 1
        tmp.Data1 = ID + Offset
        tmp.Data2 = 0
        Note(ID).Release(tmp.MidiChannel)
        Send(tmp)
        DoDraw(Rect)
    End Sub

    Private Sub PressNote(ByVal ID As Integer)
        If ID < 0 Then Return
        LastNote = ID
        Dim tmp As New Sanford.Multimedia.Midi.ChannelMessageBuilder
        tmp.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOn
        tmp.MidiChannel = numChannel.Value - 1
        tmp.Data1 = ID + Offset
        tmp.Data2 = 127
        Note(ID).Press(tmp.MidiChannel)
        Send(tmp)
        DoDraw(Rect)
    End Sub
#End Region

End Class
