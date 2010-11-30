'AddMenuObject|Keyboard 61keys,Plugins.MIDI_Keyboard,120,61-40|MIDI

Public Class MIDI_Keyboard
    Inherits BaseObject

    Private Enabled As Boolean = True


    Private numChannel As New NumericUpDown
    Private chkRemoveOldNotes As New CheckBox
    Private chkFilterOtherChannels As New CheckBox

    Private NumKeys As Byte
    Private Offset As Byte
    Private Width As Integer

#Region "Object stuff"
    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Try
            Dim data() As String = Split(UserData, "-")
            NumKeys = data(0)
            Offset = data(1)
        Catch ex As Exception
            NumKeys = 12
            Offset = 64
        End Try
        Width = NumKeys * 10

        Setup(UserData, StartPosition, Width, 115) 'Setup the base rectangles.

        'Create one output.
        Outputs(New String() {"Channel Message,ChannelMessage,ChannelMessageBuilder"})

        Inputs(New String() {"Enable,Boolean", "Channel Message,ChannelMessage"})



        'Set the title.
        Title = "MIDI Keyboard"

        chkRemoveOldNotes.Text = "Remove old notes"
        chkRemoveOldNotes.Width = 115
        chkRemoveOldNotes.Checked = True
        chkRemoveOldNotes.Location = Position + New Point(5, 35)
        AddControl(chkRemoveOldNotes)

        chkFilterOtherChannels.Text = "Filter out other channels"
        chkFilterOtherChannels.Width = 139
        chkFilterOtherChannels.Checked = False
        chkFilterOtherChannels.Location = Position + New Point(5, 19)
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
        chkRemoveOldNotes.Location = Position + New Point(5, 35)
        chkFilterOtherChannels.Location = Position + New Point(5, 19)
        numChannel.Location = Position + New Point(55, 0)
    End Sub

    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        MyBase.Draw(g)

        g.DrawString("Channel:", DefaultFont, DefaultFontBrush, Position.X + 5, Position.Y + 3)

        g.FillRectangle(Brushes.White, Position.X, Position.Y + 60, Width, 50)

        Dim OctavePos As Byte = SetBounds(Offset, 0, 11) + 4
        Dim x As Integer = Position.X
        Dim y As Integer = Position.Y + 60

        For i As Integer = 0 To NumKeys - 1
            OctavePos += 1
            If OctavePos = 12 Then OctavePos = 0

            Select Case OctavePos

                Case 0, 2, 4, 5, 7, 9, 11
                    If Note(Offset + i - 4) Then
                        g.FillRectangle(Brushes.Green, x + (10 * i), y + 20, 10, 30)
                    End If

                Case 1, 3, 6, 8, 10

                    Dim b As Brush = Brushes.Black
                    If Note(Offset + i - 4) = True Then b = Brushes.Green
                    g.FillRectangle(b, x + (10 * i), y, 7, 20)

            End Select
            'g.FillRectangle(Brushes.White
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



                'Is it a note (on or off)?
                If (message.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOn Or _
                    message.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOff) Then

                    'Is the note on? (volume more then 0)
                    If message.Data2 > 0 Then
                        Note(message.Data1) = True
                    Else
                        Note(message.Data1) = False
                    End If
                    DoDraw(True)

                End If

                Send(message)
        End Select
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)

        g.Get_Value("Enabled", Enabled, False)
        g.Get_Value("RemoveOldNotes", chkRemoveOldNotes.Checked)
        g.Get_Value("FilterOtherChannels", chkFilterOtherChannels.Checked)

        g.Get_Value("Channel", numChannel.Value, False)
        MyBase.Load(g)
    End Sub

    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("Enabled", Enabled)
        g.Set_Value("RemoveOldNotes", chkRemoveOldNotes.Checked)
        g.Set_Value("FilterOtherChannels", chkFilterOtherChannels.Checked)
        g.Set_Value("Channel", numChannel.Value)


        Return g
    End Function
#End Region

#Region "Misc stuff"
    Private Note(127) As Boolean

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
        Note(ID) = False
        Send(tmp)
    End Sub

    Private Sub PressNote(ByVal ID As Integer)
        Dim tmp As New Sanford.Multimedia.Midi.ChannelMessageBuilder
        tmp.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOn
        tmp.Data1 = ID
        tmp.Data2 = 128
        Note(ID) = True
        Send(tmp)
    End Sub

    ''' <summary>
    ''' This will wrap a number around so it is in the boundaries.
    ''' v1 changed to byte and removed max trys.
    ''' </summary>
    Public Function SetBounds(ByVal value As Byte, ByVal Min As Byte, ByVal Max As Byte) As Single
Restart:
        If value < Min Then
            value = Max + (value - Min)
            GoTo Restart
        ElseIf value > Max Then
            value = Min + (value - Max)
            GoTo Restart
        End If
        Return value
    End Function
#End Region

End Class
