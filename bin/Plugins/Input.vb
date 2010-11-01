'AddMenuObject|KeyPressed,Plugins.fgKeyPressed|Input
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
        Inputs(New String() {"Enabled|Boolean", "Tick"})
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

    Public Overrides Sub Moved()
        MyBase.Moved()

        comKey.Location = Rect.Location + New Point(15, 20)
    End Sub

    Public Overrides Sub Distroy()
        MyBase.Distroy()
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
                HID.Keyboard.Poll()
                If HID.Keyboard.GetCurrentState.IsPressed([Enum].Parse(GetType(SlimDX.DirectInput.Key), comKey.SelectedItem.ToString)) And Not LastState Then
                    LastState = True

                    Send(New InputState(1000, Me))
                ElseIf LastState Then
                    Send(New InputState(0, Me))
                    LastState = False
                End If

        End Select
    End Sub

    Public Overrides Function Load(ByVal g As SimpleD.Group) As SimpleD.Group
        g.Get_Value("Key", comKey.SelectedItem, False)

        Return MyBase.Load(g)
    End Function
    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("Key", comKey.SelectedItem)

        Return g
    End Function

End Class

Public Structure InputState
    ''' <summary>
    ''' currently from 0 to 1000
    ''' </summary>
    Public Axis As Integer
    Public Parent As Object

    Sub New(ByVal Axis As Integer, ByVal Parent As Object)
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

    Private Shared Created As Integer = 0
    Public Shared Sub Create()
        Created += 1
        If Created > 1 Then Return

        DirectInput = New DirectInput

        Keyboard = New Keyboard(DirectInput)
        Keyboard.Acquire()
        Keyboard.Poll()
    End Sub

    Public Shared Sub Dispose()
        Created -= 1
        If Created > 0 Then Return

        Keyboard.Unacquire()
        Keyboard.Dispose()

        DirectInput.Dispose()
    End Sub
End Class