Public Class fgAdd
    Inherits BaseObject

    Public Sub New(ByVal Position As Point)
        Setup(Position, 60, 100) 'Setup the base rectangles.

        'Create one input.
        Inputs(New String() {"Value 1", "Value 2"})
        'Create 4 outputs.
        Outputs(New String() {"Out value"})

        'Set the title.
        Title = "Add"

    End Sub

    Private Value1, Value2 As Integer
    Private Value1Sender As Object
    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As Object)
        If Value1Sender Is Nothing Then Value1Sender = sender

        If sender Is Value1Sender Then
            Value1 = Data
        Else
            Value2 = Data
        End If

        Send(Value1 + Value2)
    End Sub

End Class
