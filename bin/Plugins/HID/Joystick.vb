'AddMenuObject|Device,Plugins.fgJoystick,70|Input,Joystick
'AddMenuObject|Get Axis,Plugins.fgGetJoystickAxis,70|Input,Joystick
'AddMenuObject|Get Buttons,Plugins.fgGetJoystickButtons,70|Input,Joystick
'Include(HID\Input.vb)

Public Class fgJoystick
    Inherits BaseObject

    Public Enabled As Boolean = True

    Private WithEvents comJoy As New ComboBox

    Public Joystick As SlimDX.DirectInput.Joystick

    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 190) 'Setup the base rectangles.


        'Create the inputs.
        Inputs(New String() {"Enabled,Boolean", "Tick"})
        'Create the output.
        Outputs(New String() {"Joystick State,JoystickState"})

        'Set the title.
        Title = "Joystick"

        HID.Create()

        comJoy.Width = 190
        comJoy.Location = Position
        comJoy.DropDownStyle = ComboBoxStyle.DropDownList
        comJoy.Items.AddRange(HID.Joysticks.ToArray)
        'comJoy.SelectedIndex = 0
        AddControl(comJoy)


    End Sub

    Public Overrides Sub Moving()
        comJoy.Location = Position
    End Sub

    Public Overrides Sub Dispose()
        MyBase.Dispose()
        HID.Dispose()
        comJoy.Dispose()
        If Joystick IsNot Nothing Then
            Joystick.Unacquire()
            Joystick.Dispose()
        End If
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)

        g.GetValue("Enabled", Enabled, False)
        'g.GetValue("Joystick", comJoy.SelectedText, False)
        Dim tmpJoy As String = g.GetValue("Joystick")
        If tmpJoy <> "" Then
            For i As Integer = 0 To comJoy.Items.Count - 1
                If LCase(comJoy.Items(i).ToString) = LCase(tmpJoy) Then
                    comJoy.SelectedIndex = i
                    Exit For
                End If
            Next
        End If

        MyBase.Load(g)
    End Sub
    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.SetValue("Enabled", Enabled)
        If comJoy.SelectedItem IsNot Nothing Then g.SetValue("Joystick", comJoy.SelectedItem.ToString)

        Return g
    End Function

    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        Select Case sender.Index
            Case 0
                Enabled = Data

            Case 1
                If Not Enabled Then Return
                If Joystick Is Nothing Then Return
                Joystick.Poll()
                Dim state As SlimDX.DirectInput.JoystickState = Joystick.GetCurrentState
                If Not JoystickStateEquals(state, LastState) Then
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

            Joystick.Poll()
            LastState = Joystick.GetCurrentState

            Enabled = True
        End Try
    End Sub
End Class

Public Module JoystickHelper
    Public Function JoystickStateEquals(ByVal s1 As SlimDX.DirectInput.JoystickState, ByVal s2 As SlimDX.DirectInput.JoystickState) As Boolean
        'Still needs the rest of the values.
        If s1.X <> s2.X Then Return False
        If s1.Y <> s2.Y Then Return False
        If s1.Z <> s2.Z Then Return False

        If s1.RotationX <> s2.RotationX Then Return False
        If s1.RotationY <> s2.RotationY Then Return False
        If s1.RotationZ <> s2.RotationZ Then Return False

        If s1.GetSliders(0) <> s2.GetSliders(0) Then Return False
        If s1.GetSliders(1) <> s2.GetSliders(1) Then Return False

        For i As Integer = 0 To 127
            If s1.IsPressed(i) <> s2.IsPressed(i) Then Return False
        Next


        Return True
    End Function
End Module

Public Class fgGetJoystickAxis
    Inherits BaseObject

    Private chkReverse(7) As CheckBox
    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 50, 125) 'Setup the base rectangles.


        'Create the inputs.
        Inputs(New String() {"Joystick State,JoystickState"})
        'Create the output.
        Outputs(New String() {"X,0-1Normalized", "Y,0-1Normalized", "Z,0-1Normalized", _
                              "RotationX,0-1Normalized", "RotationY,0-1Normalized", "RotationZ,0-1Normalized", _
                              "Slider1,0-1Normalized", "Slider2,0-1Normalized"})

        'Set the title.
        Title = "Joystick Axis"

        For i As Integer = 0 To 7
            Dim chk As New CheckBox
            chk.Text = "Rev"
            chk.Width = 46
            chk.Height = 15
            chk.Tag = i
            chk.Location = Position + New Point(0, (15 * i))
            AddHandler chk.CheckedChanged, AddressOf ReverseChange

            chkReverse(i) = chk
            AddControl(chk)
        Next
    End Sub

    Public Overrides Sub Moving()
        For i As Integer = 0 To 7
            chkReverse(i).Location = Position + New Point(4, (15 * i))
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
            g.GetValue(chk.Text & chk.Tag, chk.Checked, False)
        Next

        MyBase.Load(g)
    End Sub

    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        'Save all the reverse check boxs state.
        For Each chk As CheckBox In chkReverse
            g.SetValue(chk.Text & chk.Tag, chk.Checked, False)
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

    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 60) 'Setup the base rectangles.


        'Create the inputs.
        Inputs(New String() {"Joystick State,JoystickState"})
        'Create the output.
        Outputs(New String() {"Released,Boolean", "Pressed,Boolean"})

        'Set the title.
        Title = "Joystick Buttons"

        numButtons.Minimum = 0
        numButtons.Maximum = 1000
        numButtons.Width = 60
        numButtons.Location = Position
        AddControl(numButtons)

    End Sub

    Public Overrides Sub Moving()
        numButtons.Location = Position
    End Sub

    Public Overrides Sub Dispose()
        MyBase.Dispose()
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
        g.GetValue("Button", numButtons.Value, False)

        MyBase.Load(g)
    End Sub
    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.SetValue("Button", numButtons.Value)

        Return g
    End Function

End Class
