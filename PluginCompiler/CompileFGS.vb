Imports System.CodeDom.Compiler
Imports System.Reflection

'This will compile the FlowGraphSave files.
Module CompileFGS
    Private Draw As Boolean = True
    Private IncludeBaseObject As Boolean = False

    Private Source As String = "'Compiled using CompileFGS"
    ''' <summary>
    ''' Add some text to the source.
    ''' </summary>
    Private Sub sAdd(ByVal Text As String)
        Source &= Environment.NewLine & Text
    End Sub

#Region "Getting ready to compile"
    Public Sub CompileFGS(ByVal fgsFile As String)
        Environment.CurrentDirectory = IO.Path.GetDirectoryName(Windows.Forms.Application.ExecutablePath)
        Dim Files As New List(Of String)

        Dim sd As New SimpleD.SimpleD(fgsFile, True)
        Dim g As SimpleD.Group = sd.Get_Group("Main")
        Dim numObj As Integer = g.Get_Value("Objects")
        For i As Integer = 0 To numObj 'Loop thrugh each object.
            g = sd.Get_Group("object" & i)
            Dim name As String = g.Get_Value("name")
            Dim file As String = "Plugins\" & g.Get_Value("File")
            If file = "Plugins\" Then
                Console.WriteLine("Object " & name & " does not support compiling.")
                Return
            ElseIf Not IO.File.Exists(file) Then
                Console.WriteLine("Object " & name & " file(" & file & ") does NOT exist.")
                Return
            End If
            Files.Add(file)
        Next

        Log(" Done", False)

        Log("Checking files...")
        Dim NewFiles As New List(Of String)
        Dim StartI As Integer = 0

Restart:
        For i As Integer = StartI To Files.Count - 1
            Dim sr As New IO.StreamReader(Files(i))
            Dim FileSource As String = sr.ReadToEnd
            sr.Close()

            sAdd(FileSource)

            If FileSource.Contains("BaseObject") Then IncludeBaseObject = True

            'Find references.
            Dim ReferencesStart As Integer = -1
            Dim IncludeStart As Integer = -1
            Do
                ReferencesStart = FileSource.IndexOf("AddReferences(", ReferencesStart + 1) + 14
                If ReferencesStart = 13 Then GoTo Include
                Dim EndIndex As Integer = FileSource.IndexOf(")", ReferencesStart)
                Dim References() As String = Split(FileSource.Substring(ReferencesStart, EndIndex - ReferencesStart), ",")
                vbReferences.AddRange(References)

Include:
                IncludeStart = FileSource.IndexOf("Include(", IncludeStart + 1) + 8
                If IncludeStart = 7 Then GoTo EndInclude
                EndIndex = FileSource.IndexOf(")", IncludeStart)
                Dim Include() As String = Split(FileSource.Substring(IncludeStart, EndIndex - IncludeStart), ",")
                NewFiles.AddRange(Include)
EndInclude:
            Loop Until ReferencesStart = 13 And IncludeStart = 7
        Next
        If NewFiles.Count > 0 Then
            StartI = Files.Count - 1
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





        Log("")
        Log("Draw=" & Draw & " BaseObject=" & IncludeBaseObject & " Files:")
        For Each f As String In Files
            Log(f)
        Next

    End Sub
#End Region

#Region "The real compiling"
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
            If Draw Then .ReferencedAssemblies.Add("System.Drawing.dll")
            .ReferencedAssemblies.Add("System.Windows.Forms.dll")
            .ReferencedAssemblies.AddRange(vbReferences.ToArray)
        End With

        'Set the provider to VB.
        Dim Provider As CodeDomProvider = New Microsoft.VisualBasic.VBCodeProvider()

        'Compile the plugin.
        Results = Provider.CompileAssemblyFromFile(Params, "fgsSource.vb")

        'Return the results.
        Return Results
    End Function
#End Region

End Module
