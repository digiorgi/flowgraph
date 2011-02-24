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
        Log("Backing up Plugins.dll ")
        'Backup the current Plugins.dll if any.
        If IO.File.Exists("Plugins.dll") Then
            Try
                If IO.File.Exists("Plugins_bak.dll") Then IO.File.Delete("Plugins_bak.dll")
                IO.File.Move("Plugins.dll", "Plugins_bak.dll")
                Log("Done!", False)
            Catch ex As Exception
                Log("")
                Log("Error backingup Plugins.dll")
                Log(ex.Message)

                Return False
            End Try
        End If

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

            'ToDo: I think this can be done better.
            'Show the errors/warnings.
            If tErrors = "Errors:" Then
                Log("Compleated with " & Errors.Count & " warnings.", False)
                Log(Warnings)

            ElseIf Warnings = "Warnings:" Then
                Log("Could not compile there is: " & Errors.Count & " errors", False)
                Log(tErrors)

            Else
                Log("Could not compile there is: " & Errors.Count & " errors/warnings", False)
                Log(tErrors)
                Log(Warnings)

            End If
            Log("Please note that line numbers are form the Source.vb file.")
            Log("")

            Log("Restoring backup ")
            'Try and restore the backup.
            Try
                If IO.File.Exists("Plugins.dll") Then IO.File.Delete("Plugins.dll")
                IO.File.Move("Plugins_bak.dll", "Plugins.dll")
                Log("Done!", False)
            Catch ex As Exception
                Log("Error restoring backup!", False)
                Log(ex.Message)
            End Try
            Return False
        Else
            Log("Done!", False)
            Log("Removing backup ")
            'Try and remove the backup.
            Try
                If IO.File.Exists("Plugins_bak.dll") Then IO.File.Delete("Plugins_bak.dll")
                Log("Done!", False)
            Catch ex As Exception
                Log("Error removing backup!", False)
                Log(ex.Message)
            End Try
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
