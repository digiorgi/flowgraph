'AddMenuObject|Set,Plugins.MIDI_SetController|MIDI,Channel Message,Controller
'AddReferences(Sanford.Slim.dll)
'Option Strict On
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

        Inputs(New String() {"Enable,Boolean", "Value,0-127,0-1Normalized,Boolean"})

        'Set the title.
        Title = "Set controller"
        File = "MIDI\MIDI_SetController.vb"

        numChannel.Minimum = 1
        numChannel.Maximum = 16
        numChannel.Width = 40
        numChannel.Location = Position + New Size(45, 25)
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
        numChannel.Location = Position + New Size(45, 25)
        comController.Location = Position
    End Sub

    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Select Case sender.Index
            Case 0 'Enable
                Enabled = DirectCast(Data, Boolean)


            Case 1 'Input axis
                If Not Enabled Then Return
                Dim value As Integer = GetValue(Data)
                If value = -1 Then Return

                Dim message As New Sanford.Multimedia.Midi.ChannelMessageBuilder
                message.Command = Sanford.Multimedia.Midi.ChannelCommand.Controller
                message.Data1 = Controller
                message.Data2 = value
                message.MidiChannel = CInt(numChannel.Value - 1)
                Send(message)
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

        g.DrawString("Channel:", DefaultFont, DefaultFontBrush, Position.X, Position.Y + 28)
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)

        g.GetValue("Enabled", Enabled, False)

        Dim tmpController As Integer = -1
        g.GetValue("Controller", tmpController, False)
        If Not tmpController = -1 Then
            comController.SelectedItem = [Enum].GetName(GetType(Sanford.Multimedia.Midi.ControllerType), tmpController)
        End If

        g.GetValue("Channel", numChannel.Value, False)

        MyBase.Load(g)
    End Sub

    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.SetValue("Enabled", Enabled)
        g.SetValue("Controller", Controller)
        g.SetValue("Channel", numChannel.Value)


        Return g
    End Function
#End Region

#Region "Control events"

    Private Sub comController_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles comController.SelectedIndexChanged
        If comController.SelectedIndex = -1 Then Return
        Controller = DirectCast([Enum].Parse(GetType(Sanford.Multimedia.Midi.ControllerType), comController.SelectedItem.ToString), Sanford.Multimedia.Midi.ControllerType)
    End Sub

#End Region

End Class
