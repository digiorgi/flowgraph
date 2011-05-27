'AddMenuObject|Set volume,Plugins.MIDI_Volume,70|MIDI,Channel Message'110,Note
'AddReferences(Sanford.Slim.dll)
Public Class MIDI_Volume
    Inherits BaseObject

    Private Enabled As Boolean = True

    Private numVolume As New NumericUpDown

#Region "Object stuff"
    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 95) 'Setup the base rectangles.

        'Create one output.
        Outputs(New String() {"Channel Message,ChannelMessageBuilder"})

        Inputs(New String() {"Enable,Boolean", "Channel Message,ChannelMessageBuilder,ChannelMessage", "Volume,0-127,0-1Normalized,Boolean"})

        'Set the title.
        Title = "MIDI Volume"
        File = "MIDI\MIDI_Volume.vb"

        numVolume.Minimum = 0
        numVolume.Maximum = 127
        numVolume.Width = 50
        numVolume.Value = 127
        numVolume.Location = Position + New Point(45, 0)
        AddControl(numVolume)

    End Sub

    Public Overrides Sub Dispose()
        numVolume.Dispose()

        MyBase.Dispose()
    End Sub

    Public Overrides Sub Moving()
        numVolume.Location = Position + New Point(45, 0)
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
                If (message.Command = Sanford.Multimedia.Midi.ChannelCommand.NoteOn) Then
                    If message.Data2 > 0 Then
                        message.Data2 = numVolume.Value 
                    End If
                End If

                Send(message)

            Case 2 'Volume
                If Not Enabled Then Return
                Dim value As Integer = GetValue(Data)
                If value = -1 Then Return
                numVolume.Value = value
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

        g.DrawString("Volume:", DefaultFont, DefaultFontBrush, Position.X, Position.Y + 3)
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)

        g.GetValue("Enabled", Enabled, False)
        g.GetValue("Volume", numVolume.Value, False)
        MyBase.Load(g)
    End Sub

    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.SetValue("Enabled", Enabled)
        g.SetValue("Volume", numvolume.value)


        Return g
    End Function
#End Region

End Class
