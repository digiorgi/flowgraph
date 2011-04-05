Imports System.CodeDom.Compiler
Imports System.Reflection

'This will compile the FlowGraphSave files.
Module CompileFGS
    Private IncludeBaseObject As Boolean = False

    Private OutputAssembly As String = ""

    Private Source As String
    ''' <summary>
    ''' Add some text to the source.
    ''' </summary>
    Private Sub sAdd(ByVal Text As String)
        Source &= Environment.NewLine & Text
    End Sub

#Region "Getting ready to compile"
    Public Function Compile(ByVal fgsFile As String) As Boolean
        Environment.CurrentDirectory = IO.Path.GetDirectoryName(Windows.Forms.Application.ExecutablePath)

        If ClassLibrary Then
            OutputAssembly = IO.Path.GetFileNameWithoutExtension(fgsFile) & ".dll"
        Else
            OutputAssembly = IO.Path.GetFileNameWithoutExtension(fgsFile) & ".exe"
        End If

        'try and backup the old file. (if any)
        If Not BackupFile(OutputAssembly) Then Return False


        Log("Loading fgs file ")

        'Some basic needed stuff to start the file with.
        Source = "'Compiled using CompileFGS"
        sAdd("Imports Microsoft.VisualBasic")
        sAdd("Imports System")
        sAdd("Imports System.Collections")
        sAdd("Imports System.Collections.Generic")
        'If Not RemoveGUI Then
        sAdd("Imports System.Windows.Forms")
        sAdd("Imports System.Drawing")
        'End If
        sAdd("Imports System.Diagnostics")
        sAdd("Imports Plugins")
        'Create the namespace.
        sAdd("Namespace Plugins")

        Dim Files As New List(Of String)

        'Go throgh each object and make sure the object is supported and it can find the plugin file.
        Dim sd As New SimpleD.SimpleD(fgsFile, True) 'Load the fgs file.
        Dim g As SimpleD.Group = sd.GetGroup("Main")
        Dim numObj As Integer = g.GetValue("Objects")
        For i As Integer = 0 To numObj 'Loop thrugh each object.
            g = sd.GetGroup("object" & i)
            If RemoveGUI Then 'Are we removing drawing?
                'Does the object support NoDraw?
                If g.GetValue("CanNoDraw") = False Then
                    Log("Object does not support NoDrawing: " & g.GetValue("name"), False)
                    Return False
                End If
            End If
            'Check and make sure the file does exist.
            Dim file As String = "Plugins\" & g.GetValue("File")
            If file = "Plugins\" Then
                Log("Object " & g.GetValue("name") & " does not support compiling.", False)
                Return False
            ElseIf Not IO.File.Exists(file) Then
                Log("Object " & g.GetValue("name") & " file(" & file & ") does NOT exist.", False)
                Return False
            End If

            'Add the file to the list if the file is not already there.
            Dim FoundFile As Boolean = False
            For f As Integer = 0 To Files.Count - 1
                If LCase(Files(f)) = LCase(file) Then
                    FoundFile = True
                    Exit For
                End If
            Next
            If Not FoundFile Then Files.Add(file)
        Next

        Log("Done!", False)

        Log("Prasing #1: loading scripts ")
        Dim NewFiles As New List(Of String) 'If one of the file wants a file that we are not getting already, we use this list to put the new files.
        Dim StartI As Integer = 0 'We dont clear the old file list so we need something to keep track of what files we already loaded.

Restart:
        For i As Integer = StartI To Files.Count - 1
            Dim sr As New IO.StreamReader(Files(i))
            Dim FileSource As String = sr.ReadToEnd
            sr.Close()

            'Add the file to the source.
            sAdd(FileSource)

            If FileSource.Contains("BaseObject") Then IncludeBaseObject = True

            'Find references.
            Dim ReferencesStart As Integer = -1
            Dim IncludeStart As Integer = -1
            Do
                'Get the references.
                ReferencesStart = FileSource.IndexOf("AddReferences(", ReferencesStart + 1) + 14
                If ReferencesStart > 13 Then
                    Dim EndIndex As Integer = FileSource.IndexOf(")", ReferencesStart)
                    Dim References() As String = Split(FileSource.Substring(ReferencesStart, EndIndex - ReferencesStart), ",")
                    vbReferences.AddRange(References)
                End If
                'Get included files.
                IncludeStart = FileSource.IndexOf("Include(", IncludeStart + 1) + 8
                If IncludeStart > 7 Then
                    Dim EndIndex As Integer = FileSource.IndexOf(")", IncludeStart)
                    Dim Include() As String = Split(FileSource.Substring(IncludeStart, EndIndex - IncludeStart), ",")
                    NewFiles.AddRange(Include)
                End If
            Loop Until ReferencesStart = 13 And IncludeStart = 7
        Next
        'If there are any new(included) files. Then we add them to the file list and check them.
        If NewFiles.Count > 0 Then
            StartI = Files.Count ' - 1
            For Each nFile As String In NewFiles
                nFile = "Plugins\" & nFile
                Dim Exists As Boolean = False
                For Each oFile As String In Files
                    If UCase(oFile) = UCase(nFile) Then
                        Exists = True
                        Exit For
                    End If
                Next
                If Not Exists Then Files.Add(nFile)
            Next
            NewFiles.Clear()
            GoTo Restart
        End If
        'If IncludeBaseObject And StartI = Files.Count - 2 Then
        '    StartI = Files.Count - 1
        '    Files.Add("Plugins\Base\BaseObject.vb")
        '    Files.Add("Plugins\Base\SimpleD.vb")
        '    GoTo Restart
        'End If




        Log("Done!", False)
        'Show the files we added.
        Log("RemoveGUI=" & RemoveGUI & " BaseObject=" & IncludeBaseObject & " Files found:")
        For Each f As String In Files
            Log(vbTab & f)
        Next

        Log("")
        Log("Prasing #2: Removing/Enabling code ")
        'End the namespace.
        sAdd("End Namespace")

        sAdd("Namespace Flowgraph")
        Files = New List(Of String)
        Files.AddRange(New String() {"..\Flowgraph\frmMain.vb", "..\Flowgraph\frmMain.Designer.vb", _
                                    "..\Flowgraph\frmAbout.vb", "..\Flowgraph\frmAbout.Designer.vb"})

        For Each file As String In Files
            Dim sr As New IO.StreamReader(file)
            sAdd(sr.ReadToEnd)
            sr.Close()
        Next

        sAdd("End Namespace")

        'Add the fgs to the code.
        Source = Source.Replace("If FileToOpen <> """" Then Open(FileToOpen)", FGS_ToCode(sd))

        'Remove stuff from code.
        Do
            Dim Start As Integer = Source.IndexOf("'RemoveFromFGS")
            Dim Count As Integer = 17 + Source.IndexOf("'EndRemoveFromFGS", Start) - Start

            Source = Source.Remove(Start, Count)
        Loop Until Source.Contains("'RemoveFromFGS") = False

        If RemoveGUI Then
            'Remove drawing.
            Do
                Dim Start As Integer = Source.IndexOf("'RemoveDraw")
                Dim Count As Integer = 14 + Source.IndexOf("'EndRemoveDraw", Start) - Start

                Source = Source.Remove(Start, Count)
            Loop Until Source.Contains("'RemoveDraw") = False
        End If

        'Add any extras.
        Do
            Source = Source.Replace("'AddToFGS", "")
        Loop Until Source.Contains("'AddToFGS") = False

        Log("Done!", False)

        Log("Compiling ")
        'Save the source to a file for debuging.
        Dim sw As New IO.StreamWriter("fgsSource.vb")
        sw.Write(Source)
        sw.Close()

        'Compile the source and get all of the errors.
        Dim Errors As CodeDom.Compiler.CompilerErrorCollection = CompileVbPlugins.Errors

        'ToDo: When done with #1 replace with #1 code.
        Log("Done!", False)
        'Get the errors.
        If Errors.Count > 0 Then
            Dim tErrors = "Errors:", Warnings As String = "Warnings:"
            'Loop thru all of the errors.
            For Each tmp As CodeDom.Compiler.CompilerError In Errors
                'If the error is a warning then.
                If tmp.IsWarning Then
                    'Add the warning.
                    Warnings &= Environment.NewLine & tmp.ErrorText & "  At line:" & tmp.Line
                Else
                    'Othor wise it is a error then add the error.
                    tErrors &= Environment.NewLine & tmp.ErrorText & "  At line:" & tmp.Line
                End If
            Next
            'Show all of the errors/warnings.
            Log(Environment.NewLine & tErrors & Environment.NewLine & Warnings & Environment.NewLine)

            If tErrors = "Errors:" Then
                Log("Compleated with " & Errors.Count & " warnings.")
            Else
                Log("Could not compile there is: " & Errors.Count & " errors")
                RestoreFile(OutputAssembly)
            End If
            Return False
        Else
            RemoveBackup(OutputAssembly)
            Return True
        End If
    End Function

    Public Const FileVersion = 0.5
    Public Function FGS_ToCode(ByVal sd As SimpleD.SimpleD) As String
        Dim g As SimpleD.Group = sd.GetGroup("Main")


        'Make sure the versions match.
        If g.GetValue("FileVersion") <> FileVersion Then
            MsgBox("Wrong file version." & Environment.NewLine _
                   & "File version: " & g.GetValue("FileVersion") & Environment.NewLine _
                   & "Requires  version: " & FileVersion, MsgBoxStyle.Critical, "Error loading")
            Return ""
        End If

        Dim Code As String = "Me.ClientSize = New Size(" & g.GetValue("Width") & "," & g.GetValue("Height") & ")"

        'Get the number of objects.
        Dim numObj As Integer = g.GetValue("Objects")
        For n As Integer = 0 To numObj 'Loop thrugh each object.
            g = sd.GetGroup("Object" & n) 'Get the object.
            If g Is Nothing Then
                MsgBox("Could not find object# " & n & " in file.", MsgBoxStyle.Critical + MsgBoxStyle.OkOnly, "Error loading file")
                Return ""
            End If

            Code &= vbNewLine
            Code &= "Objects.Add(New " & g.GetValue("name") & "(New Point(" & g.GetValue("position") & "),""" & g.GetValue("userdata") & """))"

        Next
        'Dim a As New SimpleD.Group

        Code &= vbNewLine & "Dim sd As New SimpleD.SimpleD(""" & sd.ToString(False).Replace(Environment.NewLine, "") & """)"

        'Load each object.
        For n As Integer = 0 To numObj
            g = sd.GetGroup("Object" & n)
            'Try and load each object.
            Code &= vbNewLine & "Try" & vbNewLine
            Code &= "Objects(" & n & ").Load(sd.GetGroup(""Object" & n & """))"
            Code &= vbNewLine & "Catch ex As Exception"
            Code &= vbNewLine & "MsgBox(""Could not load object# " & n & """ & Environment.NewLine & ""Name: " & g.GetValue("name") & """ & Environment.NewLine" _
                  & " & ""Execption="" & ex.Message, MsgBoxStyle.Critical + MsgBoxStyle.OkOnly, ""Error loading object"")"
            Code &= vbNewLine & "End Try"
        Next

        Return Code
    End Function
#End Region

#Region "The real compiling"
    Private vbReferences As New List(Of String)

    Private Function CompileVbPlugins() As CompilerResults
        Dim Params As New CompilerParameters()
        Dim Results As CompilerResults

        'Set the parameters.
        With Params
            .OutputAssembly = OutputAssembly
            .GenerateInMemory = False

            If ClassLibrary Then
                .CompilerOptions = "/target:library"
            Else
                .CompilerOptions = "/target:winexe"
                .GenerateExecutable = True
                .MainClass = "Flowgraph.frmMain"
            End If


#If DEBUG Then
            .IncludeDebugInformation = True
#Else
            .IncludeDebugInformation = false
#End If
            'Add the references.
            .ReferencedAssemblies.Add("System.dll")
            .ReferencedAssemblies.Add("System.Core.dll")
            'If Not RemoveGUI Then
            .ReferencedAssemblies.Add("System.Drawing.dll")
            .ReferencedAssemblies.Add("System.Windows.Forms.dll")
            'End If
            .ReferencedAssemblies.AddRange(vbReferences.ToArray)
        End With

        'Set the provider to VB.
        Dim Provider As CodeDomProvider = New Microsoft.VisualBasic.VBCodeProvider()

        'Compile the plugin.
        'Results = Provider.CompileAssemblyFromFile(Params, { _
        '                                           "..\Flowgraph\modMain.vb", _
        '                                           "..\Flowgraph\frmMain.vb", "..\Flowgraph\frmMain.Designer.vb", "..\Flowgraph\frmMain.resx", _
        '                                                    "fgsSource.vb"})

        Results = Provider.CompileAssemblyFromFile(Params, "fgsSource.vb")


        'Return the results.
        Return Results
    End Function
#End Region

End Module
