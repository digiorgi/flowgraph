'AddMenuObject|Stuff,Plugins.MIDI_Stuff,70|MIDI,Channel Message,Instrument
'AddReferences(Sanford.Slim.dll)
Public Class MIDI_Stuff
    Inherits BaseObject

    Private Enabled As Boolean = True


    

    Private Enum states
        Spliter_Enable = 0
        Spliter_Note = 1
        Spliter_LeftInstrument = 2
        Spliter_RightInstrument = 3

        ExtraInstrument_Enable = 10
        ExtraInstrument_Set = 11
        ExtraInstrument_SetVolume = 12

    End Enum

    Private setState As Boolean = False
    Private state As states = states.Spliter_LeftInstrument

    Private splitter_Enabled As Boolean = True
    Private splitter_Note As Byte = 59
    Private extraInstrument_Enabled As Boolean = False

    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 150, 50) 'Setup the base rectangles.

        'Create one output.
        Outputs(New String() {"Out,ChannelMessageBuilder"})

        Inputs(New String() {"Enable,Boolean", "Channel Message,ChannelMessageBuilder,ChannelMessage"})

        'Set the title.
        Title = "MIDI Stuff"
        File = "MIDI\MIDI_Stuff.vb"
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
                    If setState Then
                        state = message.Data1
                        setState = False
                        DoDraw(Rect)
                        Return
                    Else
                        Select Case state
                            'Spliter
                            Case states.Spliter_Enable
                                splitter_Enabled = message.Data1
                                DoDraw(Rect)
                                Return
                            Case states.Spliter_Note
                                splitter_Note = message.Data1
                                DoDraw(Rect)
                                Return
                            Case states.Spliter_LeftInstrument
                                message.MidiChannel = 0
                            Case states.Spliter_RightInstrument
                                message.MidiChannel = 1

                                'ExtraInstrument
                            Case states.ExtraInstrument_Enable
                                extraInstrument_Enabled = message.Data1
                                DoDraw(Rect)
                                Return
                            Case states.ExtraInstrument_Set
                                message.MidiChannel = 2
                            Case states.ExtraInstrument_SetVolume
                                Dim m As New Sanford.Multimedia.Midi.ChannelMessageBuilder()
                                m.Command = Sanford.Multimedia.Midi.ChannelCommand.Controller
                                m.Data1 = Sanford.Multimedia.Midi.ControllerType.Volume
                                m.Data2 = message.Data1
                                m.MidiChannel = 2
                                Send(m, 0)
                                Return
                        End Select
                    End If


                ElseIf message.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOn Or message.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOff Then
                    If message.Data1 = 96 Then 'Set state to true
                        setState = True
                        DoDraw(Rect)
                        Return
                    End If

                    If splitter_Enabled Then
                        If message.Data1 > splitter_Note Then
                            message.MidiChannel = 1
                        Else
                            message.MidiChannel = 0
                        End If
                    End If

                    If extraInstrument_Enabled Then
                        Dim m As New Sanford.Multimedia.Midi.ChannelMessageBuilder()
                        m.Command = message.Command
                        m.Data1 = message.Data1
                        m.Data2 = message.Data2
                        m.MidiChannel = 2
                        Send(m, 0)
                    End If

                End If
                Send(message, 0)
        End Select
    End Sub

    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        MyBase.Draw(g)

        'g.DrawString("settingLeft= " & setLeft, DefaultFont, DefaultFontBrush, Position.X, Position.Y + 3)
        g.DrawString("State= " & state.ToString, DefaultFont, DefaultFontBrush, Position.X, Position.Y + 3)
        g.DrawString("Setting State= " & setState.ToString, DefaultFont, DefaultFontBrush, Position.X, Position.Y + 18)
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

End Class