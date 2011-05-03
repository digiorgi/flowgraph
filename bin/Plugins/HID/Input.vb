'AddMenuObject|Axis To Boolean,Plugins.fgAxisToBoolean,85|Input
'AddReferences(SlimDX.dll)

Public Class fgAxisToBoolean
    Inherits BaseObject

    Private WithEvents numSwitchOn As New NumericUpDown

    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 60) 'Setup the base rectangles.


        'Create the inputs.
        Inputs(New String() {"Axis,0-1Normalized"})
        'Create the output.
        Outputs(New String() {"Up,Boolean", "Down,Boolean"})

        'Set the title.
        Title = "Axis to boolean"
        File = "HID\Input.vb"

        numSwitchOn.Minimum = 0
        numSwitchOn.Maximum = 1
        numSwitchOn.Increment = 0.1
        numSwitchOn.DecimalPlaces = 2
        numSwitchOn.Value = 0.5
        numSwitchOn.Width = 60
        numSwitchOn.Location = Position
        AddControl(numSwitchOn)

    End Sub

    Public Overrides Sub Moving()
        numSwitchOn.Location = Position
    End Sub

    Public Overrides Sub Dispose()
        MyBase.Dispose()
        numSwitchOn.Dispose()
    End Sub

    Public LastState As Boolean = False
    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        If Data >= numSwitchOn.Value Then
            If LastState = False Then
                LastState = True
                Send(False, 0)
                Send(True, 1)
            End If
        Else
            If LastState = True Then
                LastState = False
                Send(True, 0)
                Send(False, 1)
            End If
        End If
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)
        g.GetValue("SwitchOn", numSwitchOn.Value, False)

        MyBase.Load(g)
    End Sub
    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.SetValue("SwitchOn", numSwitchOn.Value)

        Return g
    End Function

End Class


''' <summary>
''' Human interface device
''' </summary>
Public Class HID
    Public Shared DirectInput As SlimDX.DirectInput.DirectInput
    Public Shared Keyboard As SlimDX.DirectInput.Keyboard
    Public Shared Mouse As SlimDX.DirectInput.Mouse

    Private Shared Used As Integer = 0
    Private Shared UsedMice As Integer = 0
    Private Shared UsedKeyboards As Integer = 0

    Public Shared Joysticks As New List(Of JoystickInfo)

    Public Shared Sub Create(Optional ByVal CreateKeyboard As Boolean = False, Optional ByVal CreateMouse As Boolean = False)
        Used += 1
        If Used = 1 Then
            DirectInput = New SlimDX.DirectInput.DirectInput

            For Each Device As SlimDX.DirectInput.DeviceInstance In DirectInput.GetDevices(SlimDX.DirectInput.DeviceClass.GameController, SlimDX.DirectInput.DeviceEnumerationFlags.AttachedOnly)
                Joysticks.Add(New JoystickInfo(Device.InstanceName, Device))
            Next
        End If


        If CreateKeyboard Then
            UsedKeyboards += 1
            If UsedKeyboards = 1 Then
                Keyboard = New SlimDX.DirectInput.Keyboard(DirectInput)
                Keyboard.Acquire()
                Keyboard.Poll()
            End If
        End If
        If CreateMouse Then
            UsedMice += 1
            If UsedMice = 1 Then
                Mouse = New SlimDX.DirectInput.Mouse(DirectInput)
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
    Public Device As SlimDX.DirectInput.DeviceInstance

    Sub New(ByVal Name As String, ByVal Device As SlimDX.DirectInput.DeviceInstance)
        Me.Name = Name
        Me.Device = Device
    End Sub

    Public Overrides Function ToString() As String
        Return Name
    End Function
End Class