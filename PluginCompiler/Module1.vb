Module Module1

    Sub Main()
        Log("Compileing plugins...")
        'Start compileing.
        CompilePlugins()

        If Environment.GetCommandLineArgs.Length > 1 Then
            If Environment.GetCommandLineArgs(1) = "VBIDE" Then
                Return
            End If
        End If
        'Tell the user to press any key to exit.
        Log(Environment.NewLine & "Press eny key to exit.")
        'Wait for the user to press a button.
        Console.ReadKey()
    End Sub

    ''' <summary>
    ''' Log some text to the console.
    ''' </summary>
    ''' <param name="Text">The text to put on the console</param>
    Public Sub Log(ByVal Text As String)
        Console.WriteLine(Text)
    End Sub

End Module
