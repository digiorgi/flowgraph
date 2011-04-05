'AddMenuObject|Debug,Plugins.MIDI_Debug|MIDI,Channel Message
'AddReferences(Sanford.Slim.dll)

Public Class MIDI_Debug
    Inherits BaseObject

    Private Enabled As Boolean = True

    Private Items As New List(Of String)
    Private StartID As Short = -1

#Region "Object stuff"
    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 200, 145) 'Setup the base rectangles.

        Inputs(New String() {"Enable,Boolean", "Channel Message,ChannelMessage,ChannelMessageBuilder"})

        'Set the title.
        Title = "MIDI Debug"


    End Sub


    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Select Case sender.Index
            Case 0 'Enable
                If Data <> Enabled Then
                    Enabled = Data
                End If


            Case 1 'Channel message
                If Not Enabled Then Return

                If Items.Count = 13 Then
                    Items.RemoveAt(0)
                    StartID += 1
                    If StartID > 3 Then StartID = 1
                End If

                Items.Add(Data.MidiChannel + 1 & vbTab & _
                                  Data.Command.ToString.PadRight(15) & vbTab & _
                                  Data.Data1.ToString & vbTab & _
                                  Data.Data2.ToString)

                DoDraw(Rect)

        End Select
    End Sub

    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        MyBase.Draw(g)

        'Draw the background colors.


        Dim ColorID As Short = StartID
        'Draw the debug items.
        For i As Integer = 0 To Items.Count - 1
            ColorID += 1
            If ColorID > 3 Then ColorID = 1
            Select Case ColorID
                Case 1
                    g.FillRectangle(Brushes.LightGray, Position.X, Position.Y + (11 * i), Size.Width, 11)

                Case 2
                    g.FillRectangle(Brushes.LightSkyBlue, Position.X, Position.Y + (11 * i), Size.Width, 11)
                Case 3
                    g.FillRectangle(Brushes.LightCyan, Position.X, Position.Y + (11 * i), Size.Width, 11)
            End Select


            g.DrawString(Items(i), DefaultFont, Brushes.Black, Position + New Point(0, 11 * i))
        Next
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
