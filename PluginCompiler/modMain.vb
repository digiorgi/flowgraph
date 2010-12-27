Module modMain

    Sub Main()
        Dim fgsFile As String = ""
        'Get the command line args.
        Dim args As String() = Environment.GetCommandLineArgs
        For i As Integer = 1 To args.Count - 1
            Select Case LCase(args(i))
                Case ""

                Case Else
                    If IO.File.Exists(args(i)) Then
                        If LCase(IO.Path.GetExtension(args(i))) = ".fgs" Then
                            fgsFile = args(i)
                        End If
                    End If
            End Select
        Next

        If fgsFile = "" Then
            Log("Compileing plugins...")
            'Start compileing.
            CompilePlugins()
        Else
            Log("Loading fgs file...")
            CompileFGS.CompileFGS(fgsFile)
        End If

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
    Public Sub Log(ByVal Text As String, Optional ByVal NewLine As Boolean = True)
        If NewLine Then Console.Write(Environment.NewLine & Text)
        If Not NewLine Then Console.Write(Text)
    End Sub

End Module
