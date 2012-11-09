'AddMenuObject|Spliter,Plugins.MIDI_Instrument_Spliter,70|MIDI,Channel Message,Instrument
'AddReferences(Sanford.Slim.dll)
Public Class MIDI_Instrument_Spliter
    Inherits BaseObject

    Private Enabled As Boolean = True

    Private splitPoint As Byte = 59
    Private setLeft As Boolean = True
    'Right hand is on channel 1 (left is 0)
#Region "Object stuff"
    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 95) 'Setup the base rectangles.

        'Create one output.
        Outputs(New String() {"Channel Message,ChannelMessageBuilder"})

        Inputs(New String() {"Enable,Boolean", "Channel Message,ChannelMessageBuilder,ChannelMessage"})

        'Set the title.
        Title = "MIDI instrument spliter"
        File = "MIDI\MIDI_Instrument_Spliter.vb"
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

                'Is it a program change?
                If message.Command = Sanford.Multimedia.Midi.ChannelCommand.ProgramChange Then
                    If Not setLeft Then message.MidiChannel = 1

                ElseIf message.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOn Or message.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOff Then
                    Select Case message.Data1
                        'setLeft
                        Case 95
                            setLeft = True
                            DoDraw(Rect)
                            Return
                        Case 96 'Set to right
                            setLeft = False
                            DoDraw(Rect)
                            Return

                            'Move splitPoint
                        Case 92 'Down
							if message.Data2>0 then splitPoint -= 1
                            If splitPoint < 2 Then splitPoint = 2
							Return
                        Case 94 'Up
                            if message.Data2>0 then splitPoint += 1
                            If splitPoint > 126 Then splitPoint = 126
							Return
						
                            'Do keys
                        Case Is > splitPoint
                            message.MidiChannel = 1
                    End Select


                End If
                Send(message, 0)
        End Select
    End Sub

    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        MyBase.Draw(g)

        g.DrawString("settingLeft= " & setLeft, DefaultFont, DefaultFontBrush, Position.X, Position.Y + 3)
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)
        g.GetValue("Enabled", Enabled, False)
        MyBase.Load(g)
    End Sub

    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()
        g.SetValue("Enabled", Enabled)
        Return g
    End Function
#End Region
End Class