'AddMenuObject|Get key,Plugins.fgGetKey,70|Input,Keyboard
'AddMenuObject|Get key2,Plugins.fgGetKey2,70|Input,Keyboard
'AddMenuObject|Device,Plugins.fgKeyboard,70|Input,Keyboard
'Include(HID\Input.vb)
'Include(Base\Plugins.vb,Base\BaseObject.vb)
Public Class fgGetKey
    Inherits BaseObject

    Public Enabled As Boolean = True

    Public WithEvents comKey As New ComboBox

    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 120) 'Setup the base rectangles.
        File = "HID\Keyboard.vb"

        'Create the inputs.
        Inputs(New String() {"Enabled,Boolean", "Tick", "Keyboard State,KeyboardState"})
        'Create the output.
        Outputs(New String() {"Released,Boolean", "Pressed,Boolean"})

        'Set the title.
        Title = "Get key"


        comKey.Location = Position
        comKey.Items.AddRange([Enum].GetNames(GetType(SlimDX.DirectInput.Key)))
        comKey.SelectedItem = SlimDX.DirectInput.Key.Pause.ToString
        comKey.DropDownStyle = ComboBoxStyle.DropDownList
        comKey.Width = Size.Width
        AddControl(comKey)

        HID.Create(True)
    End Sub

    Public Overrides Sub Moving()
        comKey.Location = Position
    End Sub

    Public Overrides Sub Dispose()
        MyBase.Dispose()
        HID.Dispose(True)
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
                If HID.Keyboard.GetCurrentState.IsPressed([Enum].Parse(GetType(SlimDX.DirectInput.Key), comKey.SelectedItem.ToString)) Then
                    If LastState = False Then
                        Send(False, 0)
                        Send(True, 1)
                        LastState = True
                    End If

                Else
                    If LastState = True Then
                        Send(True, 0)
                        Send(False, 1)
                        LastState = False
                    End If
                End If

            Case 2
                If Not Enabled Then Return
                If DirectCast(Data, SlimDX.DirectInput.KeyboardState).IsPressed([Enum].Parse(GetType(SlimDX.DirectInput.Key), comKey.SelectedItem.ToString)) Then
                    If LastState = False Then
                        Send(False, 0)
                        Send(True, 1)
                        LastState = True
                    End If

                Else
                    If LastState = True Then
                        Send(True, 0)
                        Send(False, 1)
                        LastState = False
                    End If
                End If

        End Select
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)
        g.Get_Value("Enabled", Enabled, False)
        g.Get_Value("Key", comKey.SelectedItem, False)

        MyBase.Load(g)
    End Sub
    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("Enabled", Enabled)
        g.Set_Value("Key", comKey.SelectedItem)

        Return g
    End Function

End Class

Public Class fgGetKey2
    Inherits BaseObject

    Public Enabled As Boolean = True

    Private WithEvents comKey As New ComboBox
    Private Key As Short

    Private Declare Function GetKeyState Lib "user32" (ByVal nVirtKey As Short) As Integer


    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 120) 'Setup the base rectangles.
        File = "HID\Keyboard.vb"

        'Create the inputs.
        Inputs(New String() {"Enabled,Boolean", "Tick"})
        'Create the output.
        Outputs(New String() {"Released,Boolean", "Pressed,Boolean"})

        'Set the title.
        Title = "Get key2"


        comKey.Location = Position
        comKey.Items.AddRange([Enum].GetNames(GetType(Keys)))
        comKey.SelectedItem = Keys.Pause.ToString
        comKey.DropDownStyle = ComboBoxStyle.DropDownList
        comKey.Width = Size.Width
        AddControl(comKey)

    End Sub

    Public Overrides Sub Moving()
        comKey.Location = Position
    End Sub

    Public Overrides Sub Dispose()
        MyBase.Dispose()
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

                Dim State As Integer = GetKeyState(Key)
                'Form.Text = State
                If State = 0 Or State = 1 Then
                    If LastState = True Then
                        Send(True, 0)
                        Send(False, 1)
                        LastState = False
                    End If

                Else
                    If LastState = False Then
                        Send(False, 0)
                        Send(True, 1)
                        LastState = True
                    End If
                End If

        End Select
    End Sub

    Public Overrides Sub Load(ByVal g As SimpleD.Group)
        g.Get_Value("Enabled", Enabled, False)
        g.Get_Value("Key", comKey.SelectedItem, False)

        MyBase.Load(g)
    End Sub
    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("Enabled", Enabled)
        g.Set_Value("Key", comKey.SelectedItem)

        Return g
    End Function

    Private Sub comKey_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles comKey.SelectedIndexChanged
        Key = [Enum].Parse(GetType(Keys), comKey.SelectedItem.ToString)
    End Sub
End Class
Public Class fgKeyboard
    Inherits BaseObject

    Public Enabled As Boolean = True

    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 30) 'Setup the base rectangles.

        'Create the inputs.
        Inputs(New String() {"Enabled,Boolean", "Tick"})
        'Create the output.
        Outputs(New String() {"Keyboard State,KeyboardState"})

        'Set the title.
        Title = "Keyboard"

        HID.Create(True)
    End Sub

    Public Overrides Sub Dispose()
        MyBase.Dispose()
        HID.Dispose(True)
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

    Public Overrides Sub Load(ByVal g As SimpleD.Group)
        g.Get_Value("Enabled", Enabled, False)

        MyBase.Load(g)
    End Sub
    Public Overrides Function Save() As SimpleD.Group
        Dim g As SimpleD.Group = MyBase.Save()

        g.Set_Value("Enabled", Enabled)

        Return g
    End Function
End Class