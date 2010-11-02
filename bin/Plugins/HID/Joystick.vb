'AddMenuObject|Joystick,Plugins.fgJoystick,70|Input
'AddMenuObject|Joystick Axis,Plugins.fgGetJoystickAxis,100|Input
Imports SlimDX.DirectInput

'Input:
'	Keyboard:	In(Enabled, Tick)	Out(Keyboard state, Down)
'	Mouse:		In(Enabled, Tick)	Out(Position, DownButtons, UpButtons)
'	Joystick:	In(Enabled, Tick, Joystick ID) Out(Joystick state)
'	InputHandler: In(Input)		Out(InputState, Axis, IsPressed)

Public Class fgJoystick
    Inherits BaseObject

    Public Enabled As Boolean = True

    Private WithEvents comJoy As New ComboBox

    Public Joystick As Joystick

    Public Sub New(ByVal Position As Point, ByVal UserData As String)
        Setup(UserData, Position, 220) 'Setup the base rectangles.


        'Create the inputs.
        Inputs(New String() {"Enabled|Boolean", "Tick"})
        'Create the output.
        Outputs(New String() {"Joystick State|JoystickState"})

        'Set the title.
        Title = "Joystick"

        comJoy.Width = 190
        comJoy.Location = Position + New Point(15, 20)
        comJoy.DropDownStyle = ComboBoxStyle.DropDownList
        comJoy.Items.AddRange(HID.Joysticks.ToArray)
        comJoy.SelectedIndex = 0
        AddControl(comJoy)

        HID.Create(True)
    End Sub

    Public Overrides Sub Moving()
        comJoy.Location = Rect.Location + New Point(15, 20)
    End Sub

    Public Overrides Sub Dispose()
        MyBase.Dispose()
        HID.Dispose(True)
        comJoy.Dispose()
        If Joystick IsNot Nothing Then
            Joystick.Unacquire()
            Joystick.Dispose()
        End If
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)
        g.Get_Value("Joystick", comJoy.SelectedItem, False)

        MyBase.Load(g)
    End Sub
    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("Joystick", comJoy.SelectedItem.ToString)

        Return g
    End Function

    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Select Case sender.Index
            Case 0
                Enabled = Data

            Case 1
                If Not Enabled Then Return
                Joystick.Poll()
                Dim state As JoystickState = Joystick.GetCurrentState
                If Not state.Equals(LastState) Then
                    LastState = state
                    Send(state)
                End If

        End Select
    End Sub

    Private LastState As New JoystickState

    Private Sub comJoy_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles comJoy.SelectedIndexChanged
        'Try to create the device.
        Try
            Joystick = New Joystick(HID.DirectInput, HID.Joysticks(comJoy.SelectedIndex).Device.InstanceGuid)
            Joystick.SetCooperativeLevel(FormHandle, CooperativeLevel.Exclusive + CooperativeLevel.Background)

        Catch ex As Exception
            MsgBox("Error! Could not create joystick device.", vbOK + vbExclamation, "Error")
        Finally
            Joystick.Acquire()

            For Each deviceObject As DeviceObjectInstance In Joystick.GetObjects()
                If (deviceObject.ObjectType And ObjectDeviceType.Axis) <> 0 Then
                    Joystick.GetObjectPropertiesById(CInt(deviceObject.ObjectType)).SetRange(0, 10000)
                End If
            Next

        End Try
    End Sub
End Class

Public Class fgGetJoystickAxis
    Inherits BaseObject

    Public Sub New(ByVal Position As Point, ByVal UserData As String)
        Setup(UserData, Position, 80) 'Setup the base rectangles.


        'Create the inputs.
        Inputs(New String() {"Joystick State|JoystickState"})
        'Create the output.
        Outputs(New String() {"X|Number", "Y|Number", "Z|Number"})

        'Set the title.
        Title = "Joystick Axis"

    End Sub

    Private LastState As New JoystickState
    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Dim state As JoystickState = DirectCast(Data, JoystickState)

        If Not state.Equals(LastState) Then


            'If state.X <> LastState.X Then Send(state.X * 0.0001, 0)
            If state.Y <> LastState.Y Then Send(state.Y * 0.0001, 1)
            If state.Z <> LastState.Z Then Send(state.Z * 0.0001, 2)

            If state.RotationZ <> LastState.RotationZ Then Send(-(state.RotationZ * 0.0001) + 1, 0)

            LastState = state
        End If

    End Sub
End Class