'AddMenuObject|Add2,Plugins.fgAdd,50,2|Math,Add
'AddMenuObject|Add3,Plugins.fgAdd,50,3|Math,Add
'AddMenuObject|Add4,Plugins.fgAdd,50,4|Math,Add
'Needed|Name displayed,Class name,Width,UserData|Groups
Public Class fgAdd
    Inherits BaseObject

    Public Sub New(ByVal Position As Point, ByVal UserData As String)
        Setup(UserData, Position, 60) 'Setup the base rectangles.

        Dim InputCount As Integer = 2
        If UserData <> "" Then
            InputCount = UserData
        End If


        ReDim Values(InputCount - 1)
        Dim inp(InputCount - 1) As String
        For n As Integer = 0 To InputCount - 1
            inp(n) = "Value " & n & "|Number"
        Next

        'Create the inputs.
        Inputs(inp)
        'Create the output.
        Outputs(New String() {"Equals|Number"})

        'Set the title.
        Title = "Add"

    End Sub

    Private Values() As Integer
    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)

        Values(sender.Index) = Data

        Dim Equals As Integer = 0
        For Each Value As Integer In Values
            Equals += Value
        Next
        Send(Equals)
    End Sub

End Class
