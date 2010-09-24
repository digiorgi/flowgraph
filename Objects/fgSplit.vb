Public Class fgSplit
    Inherits BaseObject

    Public Sub New(ByVal Position As Point)
        Setup(Position, 60, 100) 'Setup the base rectangles.

        'Create one input.
        Inputs(New String() {"Value"})
        'Create 4 outputs.
        Outputs(New String() {"Out1", "Out2", "Out3", "Out4"})

        'Set the title.
        Title = "Split"

    End Sub

    Public Overrides Sub Receive(ByVal Data As Object, ByVal sender As Transmitter)
        Send(Data)
    End Sub

End Class
