'AddMenuObject|KeyPressed,Plugins.fgKeyPressed,70|Input
'AddMenuObject|Keyboard,Plugins.fgKeyboard,70|Input
Imports SlimDX.DirectInput

'Input:
'	Keyboard:	In(Enabled, Tick)	Out(Keyboard state, Down)
'	Mouse:		In(Enabled, Tick)	Out(Position, DownButtons, UpButtons)
'	Joystick:	In(Enabled, Tick, Joystick ID) Out(Joystick state)
'	InputHandler: In(Input)		Out(InputState, Axis, IsPressed)

Public Class fgKeyPressed
    Inherits BaseObject

    Public Enabled As Boolean = True

    Public WithEvents comKey As New ComboBox

    Public Sub New(ByVal Position As Point, ByVal UserData As String)
        Setup(UserData, Position, 160) 'Setup the base rectangles.


        'Create the inputs.
        Inputs(New String() {"Enabled|Boolean", "Tick", "Keyboard State|KeyboardState"})
        'Create the output.
        Outputs(New String() {"Input State|InputState"})

        'Set the title.
        Title = "Is key pressed"


        comKey.Location = Position + New Point(15, 20)
        comKey.Items.AddRange([Enum].GetNames(GetType(SlimDX.DirectInput.Key)))
        comKey.SelectedItem = SlimDX.DirectInput.Key.DownArrow.ToString
        comKey.DropDownStyle = ComboBoxStyle.DropDownList
        AddControl(comKey)

        HID.Create()
    End Sub

    Public Overrides Sub Moving()
        comKey.Location = Rect.Location + New Point(15, 20)
    End Sub

    Public Overrides Sub Dispose()
        MyBase.Dispose()
        HID.Dispose()
        comKey.Dispose()
    End Sub

    Public LastState As Boolean = False
    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        MyBase.Receive(Data, sender)

        Select Case sender.Index
            Case 0
                Enabled = Data

            Case 1
                If Not Enabled Then Return
                HID.Keyboard.Poll()
                If HID.Keyboard.GetCurrentState.IsPressed([Enum].Parse(GetType(SlimDX.DirectInput.Key), comKey.SelectedItem.ToString)) And Not LastState Then
                    LastState = True

                    Send(New InputState(1, Me))
                ElseIf LastState Then
                    Send(New InputState(0, Me))
                    LastState = False
                End If

            Case 2
                If Not Enabled Then Return
                If DirectCast(Data, KeyboardState).IsPressed([Enum].Parse(GetType(SlimDX.DirectInput.Key), comKey.SelectedItem.ToString)) And Not LastState Then
                    LastState = True

                    Send(New InputState(1, Me))
                ElseIf LastState Then
                    Send(New InputState(0, Me))
                    LastState = False
                End If

        End Select
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)
        g.Get_Value("Key", comKey.SelectedItem, False)

        MyBase.Load(g)
    End Sub
    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("Key", comKey.SelectedItem)

        Return g
    End Function

End Class

Public Class fgKeyboard
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