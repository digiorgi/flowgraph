'AddMenuObject|Add2,Plugins.fgAdd,50,2|Math,Add
'AddMenuObject|Add3,Plugins.fgAdd,50,3|Math,Add
'Needed|Name displayed,Class name,Width|Groups
Public Class fgAdd
    Inherits BaseObject

    Public Sub New(ByVal Position As Point)
        Me.New(Position, "")
    End Sub
    Public Sub New(ByVal Position As Point, ByVal UserData As String)
        Setup(Position, 60) 'Setup the base rectangles.

        Dim InputCount As Integer = 2
        If UserData <> "" Then
            InputCount = UserData
            Me.UserData = UserData
        End If



        Dim inp(InputCount - 1) As String
        For n As Integer = 0 To InputCount - 1
            inp(n) = "Value " & n & "|Number"
        Next

        'Create one input.
        Inputs(inp)
        'Create 4 outputs.
        Outputs(New String() {"Value1+Value2|Number"})

        'Set the title.
        Title = "Add"

    End Sub

    Private Value1, Value2 As Integer
    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)

        If sender.Index = 0 Then
            Value1 = Data
        Else
            Value2 = Data
        End If

        Send(Value1 + Value2)
    End Sub

End Class
