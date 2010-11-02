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
    Public Shared Mouse As Mouse

    Private Shared Used As Integer = 0
    Private Shared UsedMice As Integer = 0
    Private Shared UsedKeyboards As Integer = 0

    Public Shared Sub Create(Optional ByVal CreateKeyboard As Boolean = False, Optional ByVal CreateMouse As Boolean = False)
        Used += 1
        If Used = 1 Then
            DirectInput = New DirectInput
        End If


        If CreateKeyboard Then
            UsedKeyboards += 1
            If UsedKeyboards = 1 Then
                Keyboard = New Keyboard(DirectInput)
                'Keyboard.SetCooperativeLevel(FormHandle, CooperativeLevel.Background)
                Keyboard.Acquire()
                Keyboard.Poll()
            End If
        End If
        If CreateMouse Then
            UsedMice += 1
            If UsedMice = 1 Then
                Mouse = New Mouse(DirectInput)
                Mouse.Acquire()
                Mouse.Poll()
            End If
        End If
    End Sub

    Public Shared Sub Dispose(Optional ByVal DisposeKeyboard As Boolean = False, Optional ByVal DisposeMouse As Boolean = False)


        If DisposeKeyboard Then
            UsedKeyboards -= 1
            If UsedKeyboards = 0 Then
                Keyboard.Unacquire()
                Keyboard.Dispose()
            End If
        End If

        If DisposeMouse Then
            UsedMice -= 1
            If UsedMice = 0 Then
                Mouse.Unacquire()
                Mouse.Dispose()
            End If
        End If

        Used -= 1
        If Used > 0 Then Return
        DirectInput.Dispose()
    End Sub
End Class