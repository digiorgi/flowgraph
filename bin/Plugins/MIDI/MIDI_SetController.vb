'AddMenuObject|Set Controller,Plugins.MIDI_SetController|MIDI,Channel Message
'AddReferences(Sanford.Slim.dll)
Public Class MIDI_SetController
    Inherits BaseObject

    Private Enabled As Boolean = True

    Private numChannel As New NumericUpDown

    Private WithEvents comController As New ComboBox
    Private Controller As Integer

#Region "Object stuff"
    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 200, 50) 'Setup the base rectangles.

        'Create one output.
        Outputs(New String() {"Channel Message,ChannelMessageBuilder"})

        Inputs(New String() {"Enable,Boolean", "Value,0-1Normalized,Boolean"})

        'Set the title.
        Title = "Set controller"

        numChannel.Minimum = 1
        numChannel.Maximum = 16
        numChannel.Width = 40
        numChannel.Location = Position + New Point(45, 25)
        AddControl(numChannel)

        comController.Width = 200
        comController.Location = Position
        comController.DropDownStyle = ComboBoxStyle.DropDownList
        comController.Items.AddRange([Enum].GetNames(GetType(Sanford.Multimedia.Midi.ControllerType)))
        comController.SelectedItem = Sanford.Multimedia.Midi.ControllerType.HoldPedal1.ToString
        AddControl(comController)

    End Sub

    Public Overrides Sub Dispose()
        numChannel.Dispose()
        comController.Dispose()

        MyBase.Dispose()
    End Sub

    Public Overrides Sub Moving()
        numChannel.Location = Position + New Point(45, 25)
        comController.Location = Position
    End Sub

    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Select Case sender.Index
            Case 0 'Enable
                If Data <> Enabled Then
                    Enabled = Data
                End If


            Case 1 'Input axis
                If Not Enabled Then Return
                If Data.GetType = GetType(Boolean) Then
                    If Data = True Then
                        Data = 1
                    Else
                        Data = 0
                    End If
                End If
                Dim message As New Sanford.Multimedia.Midi.ChannelMessageBuilder
                message.Command = Sanford.Multimedia.Midi.ChannelCommand.Controller
                message.Data1 = Controller
                message.Data2 = Data * 127
                message.MidiChannel = numChannel.Value - 1
                Send(message)
        End Select
    End Sub

    Public Overrides Sub Draw(ByVal g As System.Drawing.Graphics)
        MyBase.Draw(g)

        g.DrawString("Channel:", DefaultFont, DefaultFontBrush, Position.X, Position.Y + 28)
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)

        g.Get_Value("Enabled", Enabled, False)

        Dim tmpController As Integer = -1
        g.Get_Value("Controller", tmpController, False)
        If Not tmpController = -1 Then
            comController.SelectedItem = [Enum].GetName(GetType(Sanford.Multimedia.Midi.ControllerType), tmpController)
        End If

        g.Get_Value("Channel", numChannel.Value, False)

        MyBase.Load(g)
    End Sub

    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("Enabled", Enabled)
        g.Set_Value("Controller", Controller)
        g.Set_Value("Channel", numChannel.Value)


        Return g
    End Function
#End Region

#Region "Control events"

    Private Sub comController_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles comController.SelectedIndexChanged
        If comController.SelectedIndex = -1 Then Return
        Controller = [Enum].Parse(GetType(Sanford.Multimedia.Midi.ControllerType), comController.SelectedItem.ToString)
    End Sub

#End Region

End Class
