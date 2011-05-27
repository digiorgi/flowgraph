'AddMenuObject|Get,Plugins.MIDI_GetInstrument,70|MIDI,Channel Message,Instrument
'AddMenuObject|Set,Plugins.MIDI_SetInstrument,70|MIDI,Channel Message,Instrument
'AddReferences(Sanford.Slim.dll)
Public Class MIDI_GetInstrument
    Inherits BaseObject

    Private Enabled As Boolean = True

    Private Instrument As Byte = 0

#Region "Object stuff"
    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 95) 'Setup the base rectangles.

        'Create one output.
        Outputs(New String() {"Value,0-1Normalized", "Value,0-127"})

        Inputs(New String() {"Enable,Boolean", "Channel Message,ChannelMessageBuilder,ChannelMessage"})

        'Set the title.
        Title = "MIDI get instrument"
        File = "MIDI\MIDI_Instrument.vb"
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
                    Send(message.Data1 / 127, 0)
                    Send(message.Data1, 1)
                    Instrument = message.Data1
                    DoDraw(Rect)
                End If
        End Select
    End Sub

    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        MyBase.Draw(g)

        g.DrawString("Instrument= " & Instrument, DefaultFont, DefaultFontBrush, Position.X, Position.Y + 3)
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

Public Class MIDI_SetInstrument
    Inherits BaseObject

    Private Enabled As Boolean = True
    Private Instrument As Byte = 0

#Region "Object stuff"
    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 95) 'Setup the base rectangles.

        'Create one output.
        Outputs(New String() {"Channel Message,ChannelMessageBuilder"})

        Inputs(New String() {"Enable,Boolean", "Value,0-127,0-1Normalized"})

        'Set the title.
        Title = "MIDI set instrument"
        File = "MIDI\MIDI_Instrument.vb"

    End Sub

    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Select Case sender.Index
            Case 0 'Enable
                If Data <> Enabled Then
                    Enabled = Data
                End If


            Case 1 'Value,0-1Normalized
                If Not Enabled Then Return
                Dim value As Integer = GetValue(Data)
                If value = -1 Then Return
                Dim message As New Sanford.Multimedia.Midi.ChannelMessageBuilder
                message.Command = Sanford.Multimedia.Midi.ChannelCommand.ProgramChange
                message.Data1 = value
                Send(message)
                Instrument = value
                DoDraw(Rect)

        End Select
    End Sub
    Private Function GetValue(data As Object) As Integer
        Dim value As Integer = 0
        Select Case data.GetType
            Case GetType(Boolean) 'Boolean
                If DirectCast(data, Boolean) Then value = 127

            Case GetType(Decimal) '0-1Normalized
                value = CInt(DirectCast(data, Decimal) * 127)
            Case GetType(Double) '0-1Normalized
                value = CInt(DirectCast(data, Double) * 127)
            Case GetType(Single) '0-1Normalized
                value = CInt(DirectCast(data, Single) * 127)

            Case GetType(Integer) '0-127
                value = DirectCast(data, Integer)

            Case Else
                Log("Type (" & data.GetType.ToString() & ") is not supported.")
                Return -1
        End Select

        If value > 127 Then value = 127
        If value < 0 Then value = 0
        Return value
    End Function

    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        MyBase.Draw(g)

        g.DrawString("Instrument= " & Instrument, DefaultFont, DefaultFontBrush, Position.X, Position.Y + 3)
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