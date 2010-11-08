'AddMenuObject|Raw Mouse,Plugins.fgRawMouse,60|Input,Mouse
'AddMenuObject|Local Mouse,Plugins.fgLocalMouse,65|Input,Mouse
'AddMenuObject|Global Mouse,Plugins.fgGlobalMouse,75|Input,Mouse
Public Class fgRawMouse
    Inherits BaseObject

    Public Enabled As Boolean = True

    Public Sub New(ByVal Position As Point, ByVal UserData As String)
        Setup(UserData, Position, 60) 'Setup the base rectangles.

        'Create the inputs.
        Inputs(New String() {"Enabled|Boolean", "Tick"})
        'Create the output.
        Outputs(New String() {"Mouse State|MouseState", "X|Number", "Y|Number"})

        'Set the title.
        Title = "Raw Mouse"

        HID.Create(, True)
    End Sub

    Public Overrides Sub Dispose()
        MyBase.Dispose()
        HID.Dispose(, True)
    End Sub

    Private LastState As New SlimDX.DirectInput.MouseState
    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        MyBase.Receive(Data, sender)

        Select Case sender.Index
            Case 0
                Enabled = Data

            Case 1
                If Not Enabled Then Return
                HID.Mouse.Poll()
                Dim state As SlimDX.DirectInput.MouseState = HID.Mouse.GetCurrentState
                If StateChanged(state) Then
                    Send(state, 0)
                End If
                If state.X <> LastState.X Then
                    Send(state.X, 1)
                End If
                If state.Y <> LastState.Y Then
                    Send(state.Y, 2)
                End If

                LastState = state
        End Select
    End Sub

    Private Function StateChanged(ByVal State As SlimDX.DirectInput.MouseState) As Boolean
        If State.X <> LastState.X Then Return True
        If State.Y <> LastState.Y Then Return True
        If State.Z <> LastState.Z Then Return True

        For i As Integer = 0 To State.GetButtons.Length - 1
            If State.GetButtons(i) <> LastState.GetButtons(i) Then Return True
        Next
        Return False
    End Function

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

Public Class fgLocalMouse
    Inherits BaseObject

    Public Enabled As Boolean = True

    Public Sub New(ByVal Position As Point, ByVal UserData As String)
        Setup(UserData, Position, 65) 'Setup the base rectangles.

        'Create the inputs.
        Inputs(New String() {"Enabled|Boolean", "Tick"})
        'Create the output.
        Outputs(New String() {"Position|Point", "X|Number", "Y|Number"})

        'Set the title.
        Title = "Local Mouse"
    End Sub

    Public Overrides Sub Dispose()
        MyBase.Dispose()
    End Sub

    Private LastState As Point
    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        MyBase.Receive(Data, sender)

        Select Case sender.Index
            Case 0
                Enabled = Data

            Case 1
                If Not Enabled Then Return

                Dim state As Point = Mouse.Location
                If state <> LastState Then
                    Send(state, 0)
                End If
                If state.X <> LastState.X Then
                    Send(state.X, 1)
                End If
                If state.Y <> LastState.Y Then
                    Send(state.Y, 2)
                End If

                LastState = state
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

Public Class fgGlobalMouse
    Inherits BaseObject

    Public Enabled As Boolean = True

    Public Sub New(ByVal Position As Point, ByVal UserData As String)
        Setup(UserData, Position, 70) 'Setup the base rectangles.

        'Create the inputs.
        Inputs(New String() {"Enabled|Boolean", "Tick"})
        'Create the output.
        Outputs(New String() {"Position|Point", "X|Number,Axis", "Y|Number,Axis"})

        'Set the title.
        Title = "Global Mouse"
    End Sub

    Public Overrides Sub Dispose()
        MyBase.Dispose()
    End Sub

    Private LastState As Point
    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)
        MyBase.Receive(Data, sender)

        Select Case sender.Index
            Case 0
                Enabled = Data

            Case 1
                If Not Enabled Then Return

                Dim state As Point = Cursor.Position
                If state <> LastState Then
                    Send(state, 0)
                End If
                If state.X <> LastState.X Then
                    Send(state.X, 1)
                End If
                If state.Y <> LastState.Y Then
                    Send(state.Y, 2)
                End If

                LastState = state
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