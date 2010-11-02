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
    Public Shared DirectInput As DirectInput
    Public Shared Keyboard As Keyboard
    Public Shared Mouse As Mouse

    Private Shared Used As Integer = 0
    Private Shared UsedMice As Integer = 0
    Private Shared UsedKeyboards As Integer = 0

    Public Shared Joysticks As New List(Of JoystickInfo)

    Public Shared Sub Create(Optional ByVal CreateKeyboard As Boolean = False, Optional ByVal CreateMouse As Boolean = False)
        Used += 1
        If Used = 1 Then
            DirectInput = New DirectInput

            For Each Device As DeviceInstance In DirectInput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly)
                Joysticks.Add(New JoystickInfo(Device.InstanceName, Device))
            Next
        End If


        If CreateKeyboard Then
            UsedKeyboards += 1
            If UsedKeyboards = 1 Then
                Keyboard = New Keyboard(DirectInput)
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
Public Class JoystickInfo
    Public Name As String
    Public Device As DeviceInstance

    Sub New(ByVal Name As String, ByVal Device As DeviceInstance)
        Me.Name = Name
        Me.Device = Device
    End Sub

    Public Overrides Function ToString() As String
        Return Name
    End Function
End Class