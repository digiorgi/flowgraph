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
            Joystick.SetCooperativeLevel(Form.Handle, CooperativeLevel.Exclusive + CooperativeLevel.Background)

        Catch ex As Exception
            MsgBox("Error! Could not create joystick device.", MsgBoxStyle.Critical + MsgBoxStyle.OkOnly, "Error")
            Enabled = False
        Finally
            Joystick.Acquire()

            For Each deviceObject As DeviceObjectInstance In Joystick.GetObjects()
                If (deviceObject.ObjectType And ObjectDeviceType.Axis) <> 0 Then
                    Joystick.GetObjectPropertiesById(CInt(deviceObject.ObjectType)).SetRange(0, 10000)
                End If
            Next

            Enabled = True
        End Try
    End Sub
End Class

Public Class fgGetJoystickAxis
    Inherits BaseObject

    Private chkReverse As New List(Of CheckBox)
    Public Sub New(ByVal Position As Point, ByVal UserData As String)
        Setup(UserData, Position, 80) 'Setup the base rectangles.


        'Create the inputs.
        Inputs(New String() {"Joystick State|JoystickState"})
        'Create the output.
        Outputs(New String() {"X|Number", "Y|Number", "Z|Number", "RotationX|Number", "RotationY|Number", "RotationZ|Number"})

        'Set the title.
        Title = "Joystick Axis"

        For i As Integer = 1 To 6
            Dim chk As New CheckBox
            chk.Text = "Rev"
            chk.Width = 46
            chk.Height = 15
            chk.Tag = i - 1
            chk.Location = New Point(Rect.X + 19, Rect.Y + (15 * i))
            AddHandler chk.CheckedChanged, AddressOf ReverseChange

            chkReverse.Add(chk)
            AddControl(chk)
        Next
    End Sub

    Public Overrides Sub Moving()
        For i As Integer = 1 To 3
            chkReverse(i - 1).Location = New Point(Rect.X + 19, Rect.Y + (15 * i))
        Next
    End Sub

    Public Overrides Sub Dispose()
        MyBase.Dispose()

        For i As Integer = 0 To 2
            chkReverse(i).Dispose()
        Next
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)
        For Each chk As CheckBox In chkReverse
            g.Get_Value(chk.Text & chk.Tag, chk.Checked)
        Next

        MyBase.Load(g)
    End Sub

    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        For Each chk As CheckBox In chkReverse
            g.Set_Value(chk.Text & chk.Tag, chk.Checked)
        Next


        Return g
    End Function

    Public Sub ReverseChange(ByVal sender As Object, ByVal e As EventArgs)

    End Sub

    Private LastState As New JoystickState
    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Dim state As JoystickState = DirectCast(Data, JoystickState)

        If Not state.Equals(LastState) Then


            If state.X <> LastState.X Then SendAxis(state.X, 0)
            If state.Y <> LastState.Y Then SendAxis(state.Y, 1)
            If state.Z <> LastState.Z Then SendAxis(state.Z, 2)

            If state.RotationX <> LastState.RotationX Then SendAxis(state.RotationX, 3)
            If state.RotationY <> LastState.RotationY Then SendAxis(state.RotationY, 4)
            If state.RotationZ <> LastState.RotationZ Then SendAxis(state.RotationZ, 5)

            'If state.RotationZ <> LastState.RotationZ Then SendAxis(state.gets, 0)
            'If state.RotationZ <> LastState.RotationZ Then SendAxis(state.RotationZ, 0)

            LastState = state
        End If

    End Sub

    Public Sub SendAxis(ByVal Axis As Integer, ByVal ID As Integer)
        If chkReverse(ID).Checked = False Then
            Send(Axis * 0.0001, ID)
            Output(ID).Note = "Axis=" & Axis * 0.0001
        Else
            Send((-Axis * 0.0001) + 1, ID)
            Output(ID).Note = "Axis=" & (-Axis * 0.0001) + 1
        End If

    End Sub

End Class