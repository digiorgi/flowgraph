Module modMain


    Public ClassLibrary As Boolean = False
    Public RemoveGUI As Boolean = False
    Public KeepSource As Boolean = False
    Public LogFile As String = "CompilerLog.txt"
    Public LogString As String
    Public LogHasError As Boolean = False
    Sub Main()
        Dim ExitOnSuccessfulCompile As Boolean = False


        Dim fgsFile As String = ""
        'Get the command line args.
        Dim args As String() = Environment.GetCommandLineArgs
        For i As Integer = 1 To args.Count - 1
            Select Case LCase(args(i))
                Case ""

                Case "exitonsuccessfulcompile", "exitnoerror"
                    ExitOnSuccessfulCompile = True

                Case "classlibrary", "dll", "library"
                    ClassLibrary = True

                Case "removegui", "nodraw"
                    RemoveGUI = True

                Case "keepsource", "savesource"
                    KeepSource = True

                Case Else
                    If IO.File.Exists(args(i)) Then
                        If LCase(IO.Path.GetExtension(args(i))) = ".fgs" Then
                            fgsFile = args(i)
                        End If
                    End If
            End Select
        Next


        If IO.File.Exists(LogFile) Then IO.File.Delete(LogFile) 'Delete the old log. (if any)

Restart:

        If fgsFile <> "" Then
            'Compile fgs file.
            If Not CompileFGS.Compile(fgsFile) Then
                Log("")
                Log("Could not compile, would you like to retry? y/n: ")
                SaveLog()

                Select Case Console.ReadKey.Key
                    Case ConsoleKey.Y
                        GoTo Restart
                    Case Else
                        Return
                End Select
            End If
        Else

            'Compile plugins.
            If Not CompilePlugins2.Compile Then
                Log("")
                Log("Could not compile, would you like to retry? y/n: ")
                SaveLog()

                Select Case Console.ReadKey.Key
                    Case ConsoleKey.Y
                        GoTo Restart
                    Case Else
                        Return
                End Select
            End If
        End If

        SaveLog()

        If ExitOnSuccessfulCompile Then Return

        'Tell the user to press any key to exit.
        Log(Environment.NewLine & "Press eny key to exit.", , False)
        'Wait for the user to press a button.
        Console.ReadKey()

   
    End Sub

    ''' <summary>
    ''' Log some text to the console.
    ''' </summary>
    ''' <param name="Text">The text to put on the console</param>
    Public Sub Log(ByVal Text As String, Optional ByVal NewLine As Boolean = True, Optional ByVal IsError As Boolean = True)
        If NewLine Then Console.Write(Environment.NewLine & Text)
        If Not NewLine Then Console.Write(Text)

        If IsError Then
            LogHasError = True
        End If

        If LogFile <> "" Then
            If NewLine Then LogString &= (Environment.NewLine & Text)
            If Not NewLine Then LogString &= (Text)
        End If
    End Sub

    Public Sub SaveLog()
        If Not LogHasError Then Return 'Don't save if the log doesn't contain any errors/warnings.

        If LogFile <> "" Then
            Dim sw As New IO.StreamWriter(LogFile)
            sw.Write(LogString)
            sw.Close()
        End If
        LogString = ""
    End Sub

#Region "BackupFile"

    Public Function BackupFile(ByVal File As String) As Boolean
        If IO.File.Exists(File) Then 'Make sure there is a file to backup.
            Log("Backing up " & File & " ")
            Try
                If IO.File.Exists(File & ".bak") Then IO.File.Delete(File & ".bak") 'Remove any old backups.
                IO.File.Move(File, File & ".bak") 'Backup the file. (rename it)
                Log("Done!", False)

            Catch ex As Exception
                Log("")
                Log("Error backing up " & File)
                Log(ex.Message)
                Return False
            End Try

        Else 'There was no file to backup, make sure there is no old backups.
            Try
                'Remove any old backups.
                If IO.File.Exists(File & ".bak") Then
                    IO.File.Delete(File & ".bak")
                    Log("Removed old backup " & File)
                End If

            Catch ex As Exception
                Log("Could not remove old backup!")
                Return False
            End Try

        End If
        Return True
    End Function

    Public Function RestoreFile(ByVal File As String) As Boolean
        If IO.File.Exists(File & ".bak") Then
            Log("Restoring backup ")
            'Try and restore the backup.
            Try
                If IO.File.Exists(File) Then IO.File.Delete(File)
                IO.File.Move(File & ".bak", File)
                Log("Done!", False)
            Catch ex As Exception
                Log("Error restoring backup!", False)
                Log(ex.Message)
                Return False
            End Try

        End If

        Return True
    End Function

    Public Function RemoveBackup(ByVal File As String) As Boolean
        If IO.File.Exists(File & ".bak") Then
            Log("Removing backup ")
            'Try and remove the backup.
            Try
                If IO.File.Exists(File & ".bak") Then IO.File.Delete(File & ".bak")
                Log("Done!", False)
            Catch ex As Exception
                Log("Error removing backup!", False)
                Log(ex.Message)
                Return False
            End Try
        End If
        Return True
    End Function
#End Region

End Module
