'AddMenuObject|Device,Plugins.fgJoystick,70|Input,Joystick
'AddMenuObject|Get Axis,Plugins.fgGetJoystickAxis,70|Input,Joystick
'AddMenuObject|Get Buttons,Plugins.fgGetJoystickButtons,70|Input,Joystick

Public Class fgJoystick
    Inherits BaseObject

    Public Enabled As Boolean = True

    Private WithEvents comJoy As New ComboBox

    Public Joystick As SlimDX.DirectInput.Joystick

    Public Sub New(ByVal Position As Point, ByVal UserData As String)
        Setup(UserData, Position, 220) 'Setup the base rectangles.


        'Create the inputs.
        Inputs(New String() {"Enabled|Boolean", "Tick"})
        'Create the output.
        Outputs(New String() {"Joystick State|JoystickState"})

        'Set the title.
        Title = "Joystick"

        HID.Create(True)

        comJoy.Width = 190
        comJoy.Location = Position + New Point(15, 20)
        comJoy.DropDownStyle = ComboBoxStyle.DropDownList
        comJoy.Items.AddRange(HID.Joysticks.ToArray)
        comJoy.SelectedIndex = 0
        AddControl(comJoy)


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

        g.Get_Value("Enabled", Enabled, False)
        g.Get_Value("Joystick", comJoy.SelectedItem, False)

        MyBase.Load(g)
    End Sub
    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("Enabled", Enabled)
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
                Dim state As SlimDX.DirectInput.JoystickState = Joystick.GetCurrentState
                If Not state.Equals(LastState) Then
                    LastState = state
                    Send(state)
                End If

        End Select
    End Sub

    Private LastState As New SlimDX.DirectInput.JoystickState

    Private Sub comJoy_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles comJoy.SelectedIndexChanged
        'Try to create the device.
        Try
            Joystick = New SlimDX.DirectInput.Joystick(HID.DirectInput, HID.Joysticks(comJoy.SelectedIndex).Device.InstanceGuid)
            Joystick.SetCooperativeLevel(Form.Handle, SlimDX.DirectInput.CooperativeLevel.Exclusive + SlimDX.DirectInput.CooperativeLevel.Background)

        Catch ex As Exception
            MsgBox("Error! Could not create joystick device.", MsgBoxStyle.Critical + MsgBoxStyle.OkOnly, "Error")
            Enabled = False
        Finally
            Joystick.Acquire()

            For Each deviceObject As SlimDX.DirectInput.DeviceObjectInstance In Joystick.GetObjects()
                If (deviceObject.ObjectType And SlimDX.DirectInput.ObjectDeviceType.Axis) <> 0 Then
                    Joystick.GetObjectPropertiesById(CInt(deviceObject.ObjectType)).SetRange(0, 10000)
                End If
            Next

            Enabled = True
        End Try
    End Sub
End Class

Public Class fgGetJoystickAxis
    Inherits BaseObject

    Private chkReverse(7) As CheckBox
    Public Sub New(ByVal Position As Point, ByVal UserData As String)
        Setup(UserData, Position, 80) 'Setup the base rectangles.


        'Create the inputs.
        Inputs(New String() {"Joystick State|JoystickState"})
        'Create the output.
        Outputs(New String() {"X|Number,Axis", "Y|Number,Axis", "Z|Number,Axis", _
                              "RotationX|Number,Axis", "RotationY|Number,Axis", "RotationZ|Number,Axis", _
                              "Slider1|Number,Axis", "Slider2|Number,Axis"})

        'Set the title.
        Title = "Joystick Axis"

        For i As Integer = 1 To 8
            Dim chk As New CheckBox
            chk.Text = "Rev"
            chk.Width = 46
            chk.Height = 15
            chk.Tag = i - 1
            chk.Location = New Point(Rect.X + 19, Rect.Y + (15 * i))
            AddHandler chk.CheckedChanged, AddressOf ReverseChange

            chkReverse(i - 1) = chk
            AddControl(chk)
        Next
    End Sub

    Public Overrides Sub Moving()
        For i As Integer = 1 To 8
            chkReverse(i - 1).Location = New Point(Rect.X + 19, Rect.Y + (15 * i))
        Next
    End Sub

    Public Overrides Sub Dispose()
        MyBase.Dispose()


        For Each chk As CheckBox In chkReverse
            chk.Dispose()
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

        'Save all the reverse check boxs state.
        For Each chk As CheckBox In chkReverse
            g.Set_Value(chk.Text & chk.Tag, chk.Checked)
        Next

        Return g
    End Function

    Public Sub ReverseChange(ByVal sender As Object, ByVal e As EventArgs)
        Select Case sender.tag
            Case 0
                SendAxis(LastState.X, 0)
            Case 1
                SendAxis(LastState.Y, 1)
            Case 2
                SendAxis(LastState.Z, 2)

            Case 3
                SendAxis(LastState.RotationX, 3)
            Case 4
                SendAxis(LastState.RotationY, 4)
            Case 5
                SendAxis(LastState.RotationZ, 5)

            Case 6
                SendAxis(LastState.GetSliders(0), 6)
            Case 7
                SendAxis(LastState.GetSliders(1), 7)
        End Select
    End Sub

    Private LastState As New SlimDX.DirectInput.JoystickState
    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Dim state As SlimDX.DirectInput.JoystickState = DirectCast(Data, SlimDX.DirectInput.JoystickState)

        If Not state.Equals(LastState) Then

            'Get axis.
            If state.X <> LastState.X Then SendAxis(state.X, 0)
            If state.Y <> LastState.Y Then SendAxis(state.Y, 1)
            If state.Z <> LastState.Z Then SendAxis(state.Z, 2)

            If state.RotationX <> LastState.RotationX Then SendAxis(state.RotationX, 3)
            If state.RotationY <> LastState.RotationY Then SendAxis(state.RotationY, 4)
            If state.RotationZ <> LastState.RotationZ Then SendAxis(state.RotationZ, 5)

            If state.GetSliders(0) <> LastState.GetSliders(0) Then SendAxis(state.GetSliders(0), 6)
            If state.GetSliders(1) <> LastState.GetSliders(1) Then SendAxis(state.GetSliders(1), 7)


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

Public Class fgGetJoystickButtons
    Inherits BaseObject

    Private WithEvents numButtons As New NumericUpDown

    Public Sub New(ByVal Position As Point, ByVal UserData As String)
        Setup(UserData, Position, 90, 35) 'Setup the base rectangles.


        'Create the inputs.
        Inputs(New String() {"Joystick State|JoystickState"})
        'Create the output.
        Outputs(New String() {"Up|Boolean", "Down|Boolean"})

        'Set the title.
        Title = "Joystick Buttons"

        numButtons.Minimum = 0
        numButtons.Maximum = 1000
        numButtons.Width = 60
        numButtons.Location = Position + New Point(15, 20)
        AddControl(numButtons)

        HID.Create(True)
    End Sub

    Public Overrides Sub Moving()
        numButtons.Location = Rect.Location + New Point(15, 20)
    End Sub

    Public Overrides Sub Dispose()
        MyBase.Dispose()
        HID.Dispose(True)
        numButtons.Dispose()
    End Sub

    Public LastState As Boolean = False
    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Dim state As SlimDX.DirectInput.JoystickState = DirectCast(Data, SlimDX.DirectInput.JoystickState)
        numButtons.Maximum = state.GetButtons.Length - 1

        If state.GetButtons(numButtons.Value) <> LastState Then
            If state.GetButtons(numButtons.Value) = 0 Then
                Send(True, 0)
                Send(False, 1)
                LastState = False
            Else
                Send(False, 0)
                Send(True, 1)
                LastState = True
            End If

        End If

    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)
        g.Get_Value("Button", numButtons.Value, False)

        MyBase.Load(g)
    End Sub
    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("Button", numButtons.Value)

        Return g
    End Function

End Class
