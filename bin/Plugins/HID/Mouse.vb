'AddMenuObject|Mouse,Plugins.fgMouse,70|Input
Imports SlimDX.DirectInput

'Input:
'	Keyboard:	In(Enabled, Tick)	Out(Keyboard state, Down)
'	Mouse:		In(Enabled, Tick)	Out(Position, DownButtons, UpButtons)
'	Joystick:	In(Enabled, Tick, Joystick ID) Out(Joystick state)
'	InputHandler: In(Input)		Out(InputState, Axis, IsPressed)

Public Class fgMouse
    Inherits BaseObject

    Public Enabled As Boolean = True

    Public Sub New(ByVal Position As Point, ByVal UserData As String)
        Setup(UserData, Position, 60) 'Setup the base rectangles.

        'Create the inputs.
        Inputs(New String() {"Enabled|Boolean", "Tick"})
        'Create the output.
        Outputs(New String() {"Mouse State|MouseState", "X|Number", "Y|Number"})

        'Set the title.
        Title = "Mouse"

        HID.Create(, True)
    End Sub

    Public Overrides Sub Dispose()
        MyBase.Dispose()
        HID.Dispose(, True)
    End Sub

    Private LastState As New MouseState
    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        MyBase.Receive(Data, sender)

        Select Case sender.Index
            Case 0
                Enabled = Data

            Case 1
                If Not Enabled Then Return
                HID.Mouse.Poll()
                Dim state As MouseState = HID.Mouse.GetCurrentState
                If StateChanged(state) Then
                    Send(state, 0)
                End If
                If state.X <> LastState.X Then
                    Send(state.X, 1)
                End If
                If state.Y <> LastState.Y Then
                    Send(state.Y, 2)
                End If

                LastState = state
        End Select
    End Sub

    Private Function StateChanged(ByVal State As MouseState) As Boolean
        If State.X <> LastState.X Then Return True
        If State.Y <> LastState.Y Then Return True
        If State.Z <> LastState.Z Then Return True

        For i As Integer = 0 To State.GetButtons.Length - 1
            If State.GetButtons(i) <> LastState.GetButtons(i) Then Return True
        Next
        Return False
    End Function

End Class