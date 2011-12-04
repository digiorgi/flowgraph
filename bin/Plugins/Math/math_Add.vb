'AddMenuObject|Add,Plugins.math_Add,,|Math
'Needed|Name displayed,Class name,Width,UserData|Groups


Public Class math_Add
    Inherits BaseObject

    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 30) 'Setup the base rectangles.

        Dim InputCount As Integer = -1
        If UserData <> "" Then
            InputCount = Integer.Parse(UserData)
        Else
            Dim tmpS As String = InputBox("Number of inputs", "Add - Flowgraph", 2)
            InputCount = Integer.Parse(tmpS)
        End If

        If InputCount < 1 Then
            Log("Need more then zero inputs!")
            MsgBox("Need more then zero inputs!")
            RemoveAt(Me.Index)
            Return
        End If

        MyBase.UserData = InputCount.ToString

        ReDim Values(InputCount - 1)
        Dim inp(InputCount - 1) As String
        For n As Integer = 0 To InputCount - 1
            inp(n) = "Value " & n & ",Number"
        Next

        'Create the inputs.
        Inputs(inp)
        'Create the output.
        Outputs(New String() {"Equals,Number"})

        'Set the title.
        Title = "Add"
        File = "Math\math_Add.vb"

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