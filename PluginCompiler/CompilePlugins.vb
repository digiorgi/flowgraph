Imports System.CodeDom.Compiler
Imports System.Reflection

Module CompilePlugins

    Private vbReferences As New List(Of String)

    Private Function CompileVbPlugins() As CompilerResults
        Dim Params As New CompilerParameters()
        Dim Results As CompilerResults

        'Set the parameters.
        With Params
            .OutputAssembly = "Plugins.dll"
            .GenerateExecutable = False
            .GenerateInMemory = False

#If DEBUG Then
            .IncludeDebugInformation = True
#Else
            .IncludeDebugInformation = false
#End If
            'Add the references.
            .ReferencedAssemblies.Add("System.Drawing.dll")
            .ReferencedAssemblies.Add("System.Windows.Forms.dll")
            .ReferencedAssemblies.AddRange(vbReferences.ToArray)
        End With

        'Set the provider to VB.
        Dim Provider As CodeDomProvider = New Microsoft.VisualBasic.VBCodeProvider()

        'Compile the plugin.
        Results = Provider.CompileAssemblyFromFile(Params, "Source.vb")

        'Return the results.
        Return Results
    End Function



    'The main source.
    Private Source As String
    Public Function Compile() As Boolean


        'Backup the current Plugins.dll if any.
        If Not BackupFile("Plugins.dll") Then Return False


        Log("Loading plugins ")
        'Add the main things to the source.
        Source = "Imports Microsoft.VisualBasic"
        sAdd("Imports System")
        sAdd("Imports System.Drawing")
        sAdd("Imports System.Collections")
        sAdd("Imports System.Collections.Generic")
        sAdd("Imports System.Windows.Forms")
        sAdd("Imports System.Diagnostics")

        'Create the namespace.
        sAdd("Namespace Plugins")


        'Get the plugin files that are in the plugins folder.
        Dim PluginFiles As String() = IO.Directory.GetFiles("Plugins\", "*.vb", IO.SearchOption.AllDirectories)
        Dim ObjectNames As New List(Of String)

        'Loop thru all of the plugins.
        For Each plugin As String In PluginFiles


            'Open the file.
            Dim sr As New IO.StreamReader(plugin)
            'Get every thing in the file.
            Dim pluginSource As String = sr.ReadToEnd
            'Close the file.
            sr.Close()

            sAdd(pluginSource)

            'Find references.
            Dim StartIndex As Integer = -1
            Do
                StartIndex = pluginSource.IndexOf("AddReferences(", StartIndex + 1) + 14
                If StartIndex = 13 Then Exit Do
                Dim EndIndex As Integer = pluginSource.IndexOf(")", StartIndex)
                Dim References() As String = Split(pluginSource.Substring(StartIndex, EndIndex - StartIndex), ",")
                vbReferences.AddRange(References)

            Loop Until StartIndex = 13

        Next 'Go to the next plugin.

        'End the namespace.
        sAdd("End Namespace")

        'Save the source to a file for debuging.
        Dim sw As New IO.StreamWriter("Source.vb")
        sw.Write(Source)
        sw.Close()
        Log("Done!", False)

        Log("Compiling ")
        'Compile the source and get all of the errors.
        Dim Errors As CodeDom.Compiler.CompilerErrorCollection = CompileVbPlugins().Errors


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

            'ToDo:#1 I think this can be done better.
            'Show the errors/warnings.
            If tErrors = "Errors:" Then
                Log("Compleated with " & Errors.Count & " warnings.", False)
                Log(Warnings)

            ElseIf Warnings = "Warnings:" Then
                Log("Could not compile there is: " & Errors.Count & " errors", False)
                Log(tErrors)
                RestoreFile("Plugins.dll")

            Else
                Log("Could not compile there is: " & Errors.Count & " errors/warnings", False)
                Log(tErrors)
                Log(Warnings)
                RestoreFile("Plugins.dll")

            End If
            Log("Please note that line numbers are form the Source.vb file.")
            Log("")


            Return False
        Else
            Log("Done!", False)
            RemoveBackup("Plugins.dll")
            If Not KeepSource Then
                IO.File.Delete("Source.vb")
            End If
            Return True
        End If


    End Function

    ''' <summary>
    ''' Add some text to the source.
    ''' </summary>
    ''' <param name="Text"></param>
    ''' <remarks></remarks>
    Private Sub sAdd(ByVal Text As String)
        Source &= Environment.NewLine & Text
    End Sub

End Module

Module CompilePlugins2
    Private vbReferences As New List(Of String)

    Private Function CompileVbPlugins(Data As String()) As CompilerResults
        Dim Params As New CompilerParameters()
        Dim Results As CompilerResults

        'Set the parameters.
        With Params
            .OutputAssembly = "Plugins.dll"
            .GenerateExecutable = False
            .GenerateInMemory = False


#If DEBUG Then
            .IncludeDebugInformation = True
#Else
            .IncludeDebugInformation = false
#End If
            'Add the references.
            .ReferencedAssemblies.Add("System.Drawing.dll")
            .ReferencedAssemblies.Add("System.Windows.Forms.dll")
            .ReferencedAssemblies.AddRange(vbReferences.ToArray)
        End With

        'Set the provider to VB.
        Dim Provider As CodeDomProvider = New Microsoft.VisualBasic.VBCodeProvider()

        'Tests that didn't work.. 'ToDo: Remove at somepoint.
        'Dim c As CodeDom.Compiler.ICodeCompiler = Provider.CreateCompiler
        'Dim n As New CodeDom.CodeNamespace
        'n.
        'Dim u(Files.Count) As CodeDom.CodeCompileUnit
        'For i As Integer = 0 To Files.Count - 1
        '    u(i) = (Provider.Parse(New System.IO.StreamReader(Files(i))))
        'Next
        'Results = Provider.CompileAssemblyFromDom(Params, u.ToArray)


        'Compile the plugin.
        ' Results = Provider.CreateCompiler.CompileAssemblyFromFileBatch(Params, Files)
        Results = Provider.CompileAssemblyFromSource(Params, Data) '"Source.vb")
        'ToDo: Make plugin compiler compile from the files. (Will have to brake all plugins unliss I can find a way to set included and namespace.)

        'Return the results.
        Return Results
    End Function

    Public Function Compile() As Boolean


        'Backup the current Plugins.dll if any.
        If Not BackupFile("Plugins.dll") Then Return False


        Log("Loading plugins ", , False)
        'Add the main things to the source.
        Dim BaseImports As String = "Imports Microsoft.VisualBasic, System, System.Drawing, System.Collections" & _
                                    ", System.Collections.Generic, System.Windows.Forms, System.Diagnostics" & Environment.NewLine & _
                                    "Namespace Plugins"


        Dim data As New List(Of String)

        'Get the plugin files that are in the plugins folder.
        Dim PluginFiles As String() = IO.Directory.GetFiles("Plugins\", "*.vb", IO.SearchOption.AllDirectories)
        'Dim ObjectNames As New List(Of String)

        'Loop thru all of the plugins.
        For Each plugin As String In PluginFiles


            'Open the file.
            Dim sr As New IO.StreamReader(plugin)
            'Get every thing in the file.
            Dim pluginSource As String = sr.ReadToEnd
            'Close the file.
            sr.Close()

            'ToDo: Needs to atleast be commented... or somthing.
            If Not pluginSource.Contains("Imports") Then
                Dim Strict As Integer = pluginSource.IndexOf("Option Strict On")
                Dim Explicit As Integer = pluginSource.IndexOf("Option Explicit On")
                If Strict = -1 And Explicit = -1 Then
                    pluginSource = BaseImports & Environment.NewLine & pluginSource

                ElseIf Strict > Explicit Then
                    pluginSource = pluginSource.Insert(Strict + 16, Environment.NewLine & BaseImports)

                Else
                    pluginSource = pluginSource.Insert(Explicit + 18, Environment.NewLine & BaseImports)

                End If

            Else
                Dim EndImports As Integer = pluginSource.LastIndexOf("Imports")
                EndImports = pluginSource.IndexOf(Environment.NewLine, EndImports)
                pluginSource = pluginSource.Insert(EndImports, Environment.NewLine & "Namespace Plugins" & Environment.NewLine)
            End If




            data.Add(pluginSource & Environment.NewLine & "End Namespace")



            'Find references.
            Dim StartIndex As Integer = -1
            Do
                StartIndex = pluginSource.IndexOf("AddReferences(", StartIndex + 1) + 14
                If StartIndex = 13 Then Exit Do
                Dim EndIndex As Integer = pluginSource.IndexOf(")", StartIndex)
                Dim References() As String = Split(pluginSource.Substring(StartIndex, EndIndex - StartIndex), ",")
                vbReferences.AddRange(References)

            Loop Until StartIndex = 13

        Next 'Go to the next plugin.

        'End the namespace.
        ' sAdd("End Namespace")


        Log("Done!", False, False)

        Log("Compiling ", , False)
        'Compile the source and get all of the errors.
        Dim Errors As CodeDom.Compiler.CompilerErrorCollection = CompileVbPlugins(data.ToArray).Errors


        'Get the errors.
        If Errors.Count > 0 Then
            Dim tErrors = "Errors:", Warnings As String = "Warnings:"
            'Loop thru all of the errors.
            For Each tmp As CodeDom.Compiler.CompilerError In Errors
                'If the error is a warning then.
                If tmp.IsWarning Then
                    'Add the warning.
                    Warnings &= Environment.NewLine & tmp.ErrorText & "  At line:" & tmp.Line & " File:" & IO.Path.GetFileNameWithoutExtension(tmp.FileName)
                Else
                    'Othor wise it is a error then add the error.
                    tErrors &= Environment.NewLine & tmp.ErrorText & "  At line:" & tmp.Line & " File:" & IO.Path.GetFileNameWithoutExtension(tmp.FileName)
                End If
            Next

            'ToDo:#1 I think this can be done better.
            'Show the errors/warnings.
            If tErrors = "Errors:" Then
                Log("Compleated with " & Errors.Count & " warnings.", False)
                Log(Warnings)

            ElseIf Warnings = "Warnings:" Then
                Log("Could not compile there is: " & Errors.Count & " errors", False)
                Log(tErrors)
                RestoreFile("Plugins.dll")

            Else
                Log("Could not compile there is: " & Errors.Count & " errors/warnings", False)
                Log(tErrors)
                Log(Warnings)
                RestoreFile("Plugins.dll")

            End If
            Log("Please note that line numbers are form the Source.vb file.")
            Log("")


            Return False
        Else
            Log("Done!", False, False)
            RemoveBackup("Plugins.dll")
            If Not KeepSource Then
                IO.File.Delete("Source.vb")
            End If
            Return True
        End If


    End Function
End Module