Imports SlimDX.DirectInput

'Input:
'	Keyboard:	In(Enabled, Tick)	Out(Keyboard state, Down)
'	Mouse:		In(Enabled, Tick)	Out(Position, DownButtons, UpButtons)
'	Joystick:	In(Enabled, Tick, Joystick ID) Out(Joystick state)
'	InputHandler: In(Input)		Out(InputState, Axis, IsPressed)

Public Structure InputState
    ''' <summary>
    ''' currently from 0 to 1
    ''' </summary>
    Public Axis As Single
    Public Parent As Object

    Sub New(ByVal Axis As Single, ByVal Parent As Object)
        Me.Axis = Axis
        Me.Parent = Parent
    End Sub

    Public Overrides Function ToString() As String
        Return Axis
    End Function
End Structure

''' <summary>
''' Human interface device
''' </summary>
Public Class HID
    Private Shared DirectInput As DirectInput
    Public Shared Keyboard As Keyboard

    Private Shared Used As Integer = 0
    Public Shared Sub Create(Optional ByVal CreateKeyboard As Boolean = True)
        Used += 1
        If Used = 1 Then
            DirectInput = New DirectInput
        End If


        If CreateKeyboard Then
            Keyboard = New Keyboard(DirectInput)
            Keyboard.Acquire()
            Keyboard.Poll()
        End If
    End Sub

    Public Shared Sub Dispose(Optional ByVal DisposeKeyboard As Boolean = True)
        Used -= 1
        If Used > 0 Then Return

        If Keyboard IsNot Nothing And DisposeKeyboard Then
            Keyboard.Unacquire()
            Keyboard.Dispose()
        End If

        DirectInput.Dispose()
    End Sub
End Class