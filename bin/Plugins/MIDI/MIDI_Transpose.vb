'AddMenuObject|Transpose,Plugins.MIDI_Transpose|MIDI,Channel Message,Note
'AddReferences(Sanford.Slim.dll)
Public Class MIDI_Transpose
    Inherits BaseObject

    Private Enabled As Boolean = True

    Private numOctave As New NumericUpDown

#Region "Object stuff"
    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 95) 'Setup the base rectangles.

        'Create one output.
        Outputs(New String() {"Channel Message,ChannelMessageBuilder"})

        Inputs(New String() {"Enable,Boolean", "Channel Message,ChannelMessageBuilder,ChannelMessage"})

        'Set the title.
        Title = "MIDI Transpose"

        numOctave.Minimum = -4
        numOctave.Maximum = 4
        numOctave.Width = 50
        numOctave.Value = 0
        numOctave.Location = Position + New Point(45, 0)
        AddControl(numOctave)

    End Sub

    Public Overrides Sub Dispose()
        numOctave.Dispose()

        MyBase.Dispose()
    End Sub

    Public Overrides Sub Moving()
        numOctave.Location = Position + New Point(45, 0)
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



                'Is it a note (on or off)?
                If (message.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOn Or message.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOff) Then
                    message.Data1 += 12 * numOctave.Value
                    If message.Data1 > 127 Then message.Data1 = 127
                    If message.Data1 < 0 Then message.Data1 = 0
                End If

                Send(message)

        End Select
    End Sub

    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        MyBase.Draw(g)

        g.DrawString("Octave:", DefaultFont, DefaultFontBrush, Position.X, Position.Y + 3)
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)

        g.Get_Value("Enabled", Enabled, False)
        g.Get_Value("Octave", numOctave.Value, False)
        MyBase.Load(g)
    End Sub

    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("Enabled", Enabled)
        g.Set_Value("Octave", numOctave.value)


        Return g
    End Function
#End Region

End Class
