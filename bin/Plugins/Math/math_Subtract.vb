'AddMenuObject|Subtract,Plugins.math_Subtract,,|Math
'Needed|Name displayed,Class name,Width,UserData|Groups

'ToDo: Needs RequestData.  (Found in old_Ideas.txt) (math_Add could also use it.)

Public Class math_Subtract
    Inherits BaseObject

    Public Sub New(ByVal StartPosition As Point, ByVal UserData As String)
        Setup(UserData, StartPosition, 30) 'Setup the base rectangles.

        Dim InputCount As Integer = -1
        If UserData <> "" Then
            InputCount = Integer.Parse(UserData)
        Else
            Dim tmpS As String = InputBox("Number of inputs", "Subtract - Flowgraph", 2)
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
        Title = "Subtract"
        File = "Math\math_Subtract.vb"

    End Sub

    Private Values() As Integer
    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As DataFlow)

        Values(sender.Index) = Data

        Dim Equals As Integer = Values(0)
        For i As Integer = 1 To Values.Length - 1
            Equals -= Values(i)
        Next
        Send(Equals)
    End Sub

End Class
