'AddMenuObject|Roll,Plugins.MIDI_Roll,120,88-21-9|MIDI
'AddReferences(Sanford.Slim.dll)

Public Class MIDI_Roll
    Inherits BaseObject

    Private Enabled As Boolean = True


    Private numChannel As New NumericUpDown
    Private chkFilterOtherChannels As New CheckBox

    Private NumKeys As Byte
    Private Offset, OctaveOffset As Byte
    Private Width As Integer

    Private speed As Integer = 1

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

        Setup(UserData, StartPosition, Width, 300) 'Setup the base rectangles.

        'Create one output.
        'Outputs(New String() {"Channel Message,ChannelMessageBuilder"})
        Inputs(New String() {"Enable,Boolean", "Channel Message,ChannelMessage,ChannelMessageBuilder", "Tick"})


        'Set the title.
        Title = "MIDI Roll"
        File = "MIDI\MIDI_Roll.vb"

        chkFilterOtherChannels.Text = "Filter out other channels"
        chkFilterOtherChannels.Width = 139
        chkFilterOtherChannels.Height = 15
        chkFilterOtherChannels.Checked = False
        chkFilterOtherChannels.Location = Position + New Point(85, 0)
        AddControl(chkFilterOtherChannels)

        numChannel.Minimum = 1
        numChannel.Maximum = 16
        numChannel.Width = 40
        numChannel.Location = Position + New Point(55, 0)
        AddControl(numChannel)

    End Sub

    Public Overrides Sub Dispose()
        chkFilterOtherChannels.Dispose()
        numChannel.Dispose()

        MyBase.Dispose()
    End Sub

    Public Overrides Sub Moving()
        chkFilterOtherChannels.Location = Position + New Point(85, 0)
        numChannel.Location = Position + New Point(55, 0)
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


                If chkFilterOtherChannels.Checked AndAlso numChannel.Value + 1 <> message.MidiChannel Then Return


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
                    Return
                End If

                'Check to see if the note is within the keyboard size.
                If message.Data1 >= Offset And message.Data1 + 1 <= Offset + NumKeys Then
                    'Is the note on? (volume more then 0)
                    If NoteOn Then
                        Pressed.Add(New Bar(message.Data1, message.MidiChannel))
                    Else
                        For i As Integer = 0 To Pressed.Count - 1
                            If Pressed(i).Note = message.Data1 AndAlso Pressed(i).Channel = message.MidiChannel Then
                                Bars.Insert(0, Pressed(i))
                                Pressed.RemoveAt(i)
                                Exit For
                            End If
                        Next
                    End If
                End If


                'DoDraw(Rect)

            Case 2 'Tick
                doTick()
        End Select
    End Sub

    Private Sub doTick()
        For Each b As Bar In Pressed
            b.Height += speed
        Next
        Dim i As Integer = 0
        Do Until i >= Bars.Count
            If Bars(i).Y > Rect.Height Then
                Bars.RemoveAt(i)
                i -= 1
            Else
                Bars(i).Y += speed
            End If
            i += 1
        Loop
        DoDraw(Rect)
    End Sub

    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        MyBase.Draw(g)

        g.DrawString("Channel:", DefaultFont, DefaultFontBrush, Position.X + 5, Position.Y + 3)

        'g.FillRectangle(Brushes.White, Position.X, Position.Y + 40, Width, 50)

        Dim x As Integer = Position.X
        Dim y As Integer = Position.Y + 40

        For Each b As Bar In Bars
            b.Draw(g, x, y, Position.Y + Rect.Height)
        Next
        For Each b As Bar In Pressed
            b.Draw(g, x, y, Position.Y + Rect.Height)
        Next
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

#End Region

#Region "Bar stuff"
    Private Pressed As New List(Of Bar)
    Private Bars As New List(Of Bar)

    Private Class Bar
        Public Note As Integer = 1
        Public Channel As Integer = 0

        Public Height As Integer
        Public Y As Integer = 0

        Public Sub New(note As Integer, channel As Integer)
            Me.Note = note
            Me.Channel = channel
            Height = 10
        End Sub

        Public Sub Draw(g As Graphics, offsetX As Integer, offsetY As Integer, bottom As Integer)

            Dim bColor As Color = Color.Red
            Dim h As Double = bColor.GetHue
            Dim s As Double = bColor.GetSaturation
            Dim l As Double = bColor.GetBrightness

            h += 21 * Channel
            If Channel / 2 = Channel Then s *= 0.8

            Dim brush As New SolidBrush(HSLColor(h, s, l))
            g.FillRectangle(brush, offsetX + (5 * (Note - 21)), offsetY + Y, 5, Height)
            g.DrawRectangle(Pens.Black, offsetX + (5 * (Note - 21)), offsetY + Y, 5, Height)

        End Sub

        Public Function HSLColor(h As Double, s As Double, l As Double) As Color
            Dim t As Double() = New Double() {0, 0, 0}

            'Try
            Dim tH As Double = h / 360.0F
            Dim tS As Double = s
            Dim tL As Double = l

            If tS.Equals(0) Then
                t(0) = tL 'InlineAssignHelper(t(1), InlineAssignHelper(t(2), tL))
                t(1) = tL
                t(2) = tL
            Else
                Dim q As Double, p As Double

                q = If(tL < 0.5, tL * (1 + tS), tL + tS - (tL * tS))
                p = 2 * tL - q

                t(0) = tH + (1.0 / 3.0)
                t(1) = tH
                t(2) = tH - (1.0 / 3.0)

                For i As Byte = 0 To 2
                    t(i) = If(t(i) < 0, t(i) + 1.0, If(t(i) > 1, t(i) - 1.0, t(i)))

                    If t(i) * 6.0 < 1.0 Then
                        t(i) = p + ((q - p) * 6 * t(i))
                    ElseIf t(i) * 2.0 < 1.0 Then
                        t(i) = q
                    ElseIf t(i) * 3.0 < 2.0 Then
                        t(i) = p + ((q - p) * 6 * ((2.0 / 3.0) - t(i)))
                    Else
                        t(i) = p
                    End If
                Next
            End If
            'Catch ee As Exception

            'End Try

            Return Color.FromArgb(CInt(t(0) * 255), CInt(t(1) * 255), CInt(t(2) * 255))
        End Function
    End Class

    Private Sub ResetNotes()
        Bars.AddRange(Pressed)
        Pressed.Clear()
        DoDraw(True)
    End Sub

#End Region

End Class
