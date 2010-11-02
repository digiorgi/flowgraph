'Add#MenuObject|Joystick,Plugins.fgJoystick,70|Input
Imports SlimDX.DirectInput

'Input:
'	Keyboard:	In(Enabled, Tick)	Out(Keyboard state, Down)
'	Mouse:		In(Enabled, Tick)	Out(Position, DownButtons, UpButtons)
'	Joystick:	In(Enabled, Tick, Joystick ID) Out(Joystick state)
'	InputHandler: In(Input)		Out(InputState, Axis, IsPressed)

Public Class fgJoystick
    Inherits BaseObject

    Public Enabled As Boolean = True

    Public Sub New(ByVal Position As Point, ByVal UserData As String)
        Setup(UserData, Position, 60) 'Setup the base rectangles.

        'Create the inputs.
        Inputs(New String() {"Enabled|Boolean", "Tick"})
        'Create the output.
        Outputs(New String() {"Keyboard State|KeyboardState"})

        'Set the title.
        Title = "Keyboard"

        HID.Create()
    End Sub

    Public Overrides Sub Dispose()
        MyBase.Dispose()
        HID.Dispose()
    End Sub

    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        MyBase.Receive(Data, sender)

        Select Case sender.Index
            Case 0
                Enabled = Data

            Case 1
                If Not Enabled Then Return
                HID.Keyboard.Poll()
                Send(HID.Keyboard.GetCurrentState)

        End Select
    End Sub
End Class