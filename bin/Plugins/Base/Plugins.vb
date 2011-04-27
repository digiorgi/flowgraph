#Region "License & Contact"
'License:
'   Copyright (c) 2010 Raymond Ellis
'   
'   This software is provided 'as-is', without any express or implied
'   warranty. In no event will the authors be held liable for any damages
'   arising from the use of this software.
'
'   Permission is granted to anyone to use this software for any purpose,
'   including commercial applications, and to alter it and redistribute it
'   freely, subject to the following restrictions:
'
'       1. The origin of this software must not be misrepresented; you must not
'           claim that you wrote the original software. If you use this software
'           in a product, an acknowledgment in the product documentation would be
'           appreciated but is not required.
'
'       2. Altered source versions must be plainly marked as such, and must not be
'           misrepresented as being the original software.
'
'       3. This notice may not be removed or altered from any source
'           distribution.
'
'
'Contact:
'   Raymond Ellis
'   Email: RaymondEllis@live.com
#End Region

Public Module Plugins


    'The default font to use.
    Public DefaultFont As Font = SystemFonts.DefaultFont
    Public DefaultFontBrush As Brush = SystemBrushes.ControlText
    Public InputImage, OutputImage As Image

    'The pen used to connect objects.
    Public ConnectorPen As New Pen(Color.FromArgb(80, 80, 80), 3)

    'Used to check if the mouse is inside a rectangle.
    Public Mouse As Rectangle

    Public Form As Control
    Public WindowSize As Size

    
#Region "Grid"
    'The snap grid size.
    Public GridSize As Integer = 5

    Public Function SnapToGrid(ByVal value As Decimal) As Decimal
        Return Math.Round(value / GridSize) * GridSize
    End Function
    Public Function SnapToGrid(ByVal value As Double) As Double
        Return Math.Round(value / GridSize) * GridSize
    End Function

    Public Function SnapToGrid(ByVal point As Point) As Point
        Return New Point(Math.Round(point.X / GridSize) * GridSize, Math.Round(point.Y / GridSize) * GridSize)
    End Function
    Public Function SnapToGrid(ByVal point As PointF) As PointF
        Return New PointF(Math.Round(point.X / GridSize) * GridSize, Math.Round(point.Y / GridSize) * GridSize)
    End Function

    Public Function SnapToGrid(ByVal rect As Rectangle) As Rectangle
        Return New Rectangle(Math.Round(rect.X / GridSize) * GridSize, Math.Round(rect.Y / GridSize) * GridSize, _
                             Math.Round(rect.Width / GridSize) * GridSize, Math.Round(rect.Height / GridSize) * GridSize)
    End Function
    Public Function SnapToGrid(ByVal rect As RectangleF) As RectangleF
        Return New RectangleF(Math.Round(rect.X / GridSize) * GridSize, Math.Round(rect.Y / GridSize) * GridSize, _
                              Math.Round(rect.Width / GridSize) * GridSize, Math.Round(rect.Height / GridSize) * GridSize)
    End Function
#End Region

#Region "Tool stuff"

    Public Enum ToolType
        None
        Move
        Connect
        Menu
    End Enum
    Public Tool As ToolType
    Public ToolOffset As Point
    Public ToolObject As Integer
    Public ToolInt As Integer
#End Region

#Region "Object stuff"
    'The list of objects.
    Public Objects As New List(Of Object)

    Public Sub ResetObjectIndexs(ByVal RemovedIndex As Integer)
        For n As Integer = 0 To Objects.Count - 1
            Objects(n).Index = n

            'Set the output objects.
            If Objects(n).Output IsNot Nothing Then
                For o As Integer = 0 To Objects(n).Output.Length - 1
                    'Output object.
                    If Objects(n).Output(o).obj > RemovedIndex Then
                        Objects(n).Output(o).obj -= 1
                    End If

                    'Flow objects.
                    Dim i As Integer = 0
                    Do While i < Objects(n).Output(o).Flow.Count
                        If Objects(n).Output(o).Flow(i).obj = RemovedIndex Then
                            Objects(n).Output(o).Flow(i) = Nothing
                            Objects(n).Output(o).Flow.RemoveAt(i)
                        ElseIf Objects(n).Output(o).Flow(i).obj > RemovedIndex Then
                            Objects(n).Output(o).Flow(i).obj -= 1
                            i += 1
                        Else
                            i += 1
                        End If
                    Loop
                Next

                
            End If

            'Set the input objects.
            If Objects(n).Input IsNot Nothing Then
                For i As Integer = 0 To Objects(n).Input.Length - 1
                    If Objects(n).Input(i).obj > RemovedIndex Then
                        Objects(n).Input(i).obj -= 1
                    End If
                Next
            End If
        Next
    End Sub

    Public Sub RemoveAt(ByVal Index As Integer)
        Objects(Index).Dispose()
        Objects(Index) = Nothing
        Objects.RemoveAt(Index)

        ResetObjectIndexs(Index)
    End Sub

    Public Sub ClearObjects()
        For Each obj As Object In Objects
            obj.Dispose()
        Next
        Objects.Clear()
    End Sub
#End Region

#Region "Open & Save"

    ''' <summary>
    ''' Loads the main plugin stuff.
    ''' </summary>
    Public Sub Load_Plugin(ByVal form As Control)
        'Setup the auto draw timmer.
        tmrDraw.Interval = 200
        tmrDraw.Enabled = True

        If IO.File.Exists("Plugins\Base\Input.png") Then
            InputImage = Image.FromFile("Plugins\Base\Input.png")
        End If
        If IO.File.Exists("Plugins\Base\Output.png") Then
            OutputImage = Image.FromFile("Plugins\Base\Output.png")
        End If

        Plugins.Form = form
        'RemoveFromFGS
        AddObject_Setup()'EndRemoveFromFGS
    End Sub

    'RemoveFromFGS

    Public LoadedFile As String = ""
    Public Const FileVersion As Short = 1 'If you change this do not for get to change the suported versions. (inside the open sub.)
    Private SplitFileWithNewLine As Boolean = True
    Private SplitFileWithTabs As Boolean = True

    Public Event OpenedEvent()

    Public Sub Open(ByVal File As String)
        If Not IO.File.Exists(File) Then
            MsgBox("Could not find file:" & vbNewLine & File, , "Error loading")
            Return
        End If

        ClearObjects()

        Dim sd As New SimpleD.SimpleD
        sd.FromFile(File)

        Dim g As SimpleD.Group = sd.GetGroup("Main")

        WindowSize = New Size(g.GetValue("Width"), g.GetValue("Height"))
        If Not g.GetValue("DisableUI") = "" AndAlso g.GetValue("DisableUI") = True Then
            UpdateUI = False
        End If
        Form.ClientSize = WindowSize
        'Make sure the form is still in the screen.
        Dim x As Integer = Form.Location.X
        Dim y As Integer = Form.Location.Y
        Dim scr As Rectangle = Screen.GetWorkingArea(Form.Location)
        If x + Form.Width > scr.Right Then x = scr.Right - Form.Width
        If y + Form.Height > scr.Bottom Then y = scr.Bottom - Form.Height
        Form.Location = New Point(x, y) 'Set the window location.

        'Make sure the versions is supported.
        Dim CurrentFileVersion As Single = g.GetValue("FileVersion")
        Select Case CurrentFileVersion
            Case 0.5, 1 'Supported versions.
            Case Else
                MsgBox("Wrong file version." & Environment.NewLine _
                                  & "File version: " & g.GetValue("FileVersion") & Environment.NewLine _
                                  & "Requires  version 0.5 or 1", MsgBoxStyle.Critical, "Error loading")
                Return
        End Select

        g.GetValue("SplitFileWithNewLine", SplitFileWithNewLine, False)
        g.GetValue("SplitFileWithTabs", SplitFileWithTabs, False)

        'Get the number of objects.
        Dim numObj As Integer
        Select Case CurrentFileVersion
            Case 1
                numObj = sd.GetGroupArray("object").Length - 1
            Case 0.5
                numObj = g.GetValue("Objects")
        End Select
        'For n As Integer = 0 To numObj 'Loop thrugh each object.
        Dim i As Integer = -1
        Do
            i += 1
            'Get the object.
            If CurrentFileVersion = 1 Then
                g = sd.GetGroupArray("object")(i)
            ElseIf CurrentFileVersion = 0.5 Then
                g = sd.GetGroup("Object" & i)
            End If
            If g Is Nothing Then
                MsgBox("Could not find object# " & i & " in file.", MsgBoxStyle.Critical + MsgBoxStyle.OkOnly, "Error loading file")
                ClearObjects()
                LoadedFile = ""
                Return
            End If

            'Get the position.
            Dim pos As String() = Split(g.GetValue("position"), ",")
            Dim obj As Integer = AddObject(g.GetValue("name"), New Point(pos(0), pos(1)), g.GetValue("userdata")) 'Get the object.

            'Show error if could not create object.
            If obj = -1 Then
                MsgBox("Could not create object# " & i & Environment.NewLine & "Name: " & g.GetValue("name"), MsgBoxStyle.Critical + MsgBoxStyle.OkOnly, "Error loading file")
                ClearObjects()
                LoadedFile = ""
                Return
            End If
        Loop Until i >= numObj
        'Next

        'Load each object.
        For n As Integer = 0 To numObj
            Select Case CurrentFileVersion
                Case 1
                    g = sd.GetGroupArray("object")(n)
                Case 0.5
                    g = sd.GetGroup("Object" & n)
            End Select

            'Try and load each object.
            Try
                Objects(n).Load(g)
            Catch ex As Exception
                MsgBox("Could not load object# " & n & Environment.NewLine & "Name: " & g.GetValue("name") & vbNewLine _
                      & "Execption=" & ex.Message, MsgBoxStyle.Critical + MsgBoxStyle.OkOnly, "Error loading object")
            End Try
        Next

        'Set the loaded file
        LoadedFile = File
        RaiseEvent OpenedEvent()
        DoDraw()
    End Sub

    Public Sub Save(ByVal File As String)

        Dim sd As New SimpleD.SimpleD
        Dim g As SimpleD.Group = sd.CreateGroup("Main")
        If UpdateUI = False Then
            g.SetValue("DisableUI", "True")
        End If
        g.SetValue("Width", WindowSize.Width)
        g.SetValue("Height", WindowSize.Height)

        'g.SetValue("Objects", Objects.Count - 1) 0.5
        g.SetValue("FileVersion", FileVersion)

        g.SetValue("SplitFileWithNewLine", SplitFileWithNewLine)
        g.SetValue("SplitFileWithTabs", SplitFileWithTabs)

        'Save each object.
        For Each obj As Object In Objects
            sd.AddGroup(obj.Save, False)
        Next

        'Save to file.
        sd.ToFile(File, SplitFileWithNewLine, SplitFileWithTabs)

        LoadedFile = File
    End Sub

    'EndRemoveFromFGS
#End Region

#Region "Auto draw"
    Public ComplexLines As Boolean = True
    Event DrawEvent(ByVal region As Rectangle)
    Public UpdateUI As Boolean = True
    Private DoNotDraw As Boolean = True

    ''' <summary>
    ''' Tells auto draw to draw when the time comes.
    ''' </summary>
    ''' <param name="HeighPriority">If it's a heigh priority, then it will draw as soon as possible.</param>
    Public Sub DoDraw(Optional ByVal HeighPriority As Boolean = False)
        If Not UpdateUI Then Return

        'If it is a heigh priority. then we will not wait for the next timmer tick and just draw.
        If HeighPriority Then
            RaiseEvent DrawEvent(Rectangle.Empty)
            DoNotDraw = True

        Else 'Other wise we wait for the timer.
            DoNotDraw = False 'Tell the timer it can draw.
        End If
    End Sub
    Public Sub DoDraw(ByVal region As Rectangle)
        If Not UpdateUI Then Return
        RaiseEvent DrawEvent(region)
    End Sub

    Private WithEvents tmrDraw As New Timer
    Private Sub tmrDraw_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles tmrDraw.Tick
        If DoNotDraw Then Return

        RaiseEvent DrawEvent(Rectangle.Empty)

        DoNotDraw = True
    End Sub
#End Region

#Region "Add & Remove Control"
    Event AddControlEvent(ByVal Control As Control)
    Event RemoveControlEvent(ByVal Control As Control)

    Public Sub AddControl(ByVal Control As Control)
        RaiseEvent AddControlEvent(Control)
    End Sub
    Public Sub RemoveControl(ByVal Control As Control)
        RaiseEvent RemoveControlEvent(Control)
    End Sub
#End Region

#Region "Adding objects"
    'The items in the add object menu.
    Public AddItem As New Menu.Node("Add object", True)

    ''' <summary>
    ''' Add a new object from the class name.
    ''' </summary>
    ''' <param name="Name">ex: 'Plugins.fgAdd'</param>
    ''' <param name="StartPosition">You shouldn't need help here.</param>
    ''' <returns>-1 if not found. other wise returns object index.</returns>
    ''' <remarks></remarks>
    Public Function AddObject(ByVal Name As String, ByVal StartPosition As Point, Optional ByVal UserData As String = "") As Integer
        'NOTE: I am pretty sure there is a faster way to do this.
        'But I got this working first, so until it is a problem it will stay like this.
        Try
            Objects.Add(Activator.CreateInstance(Type.[GetType](Name), New Object() {StartPosition, UserData}))
            Return Objects.Count - 1
        Catch ex As Exception
            'MsgBox("Could not create object: " & Name)
            Return -1
        End Try
    End Function

    'RemoveFromFGS
    Private Sub AddObject_Setup()
        'Is the plugins library newer then the objects file?
        If IO.File.GetLastWriteTime("Plugins.dll") > IO.File.GetLastWriteTime("Plugins\MenuObjects.list") Then

            'The plugins have changed. So lets find all of the objects.

            Dim Scripts As String() = IO.Directory.GetFiles("Plugins\", "*.vb", IO.SearchOption.AllDirectories)
            Dim ObjectList As String = ""
            For Each File As String In Scripts
                SearchForItems(File, ObjectList)
            Next

            'Write all of the objects found to the file.
            Dim sw As New IO.StreamWriter("Plugins\MenuObjects.list", False)
            sw.Write(ObjectList)
            sw.Close()

        Else
            'Objects.list is newer, so lets get the items from it.
            SearchForItems("Plugins\MenuObjects.list")
        End If
    End Sub

    ''' <summary>
    ''' Search thru a file and fill the add object menu. With found objects.
    ''' </summary>
    ''' <param name="File">The file to search thru.</param>
    ''' <param name="ObjectList">Will fill string with each line that has "AddMenuObject".
    ''' Unlis set to "DoNotFill"</param>
    ''' <remarks></remarks>
    Private Sub SearchForItems(ByVal File As String, Optional ByRef ObjectList As String = "DoNotFill", Optional ByVal SearchWholeFile As Boolean = False)
        Dim sr As New IO.StreamReader(File)
        Dim StartIndex As Integer
        Do
            Dim line As String = sr.ReadLine 'Get the next line out of the file.
            StartIndex = line.IndexOf("AddMenuObject", StringComparison.OrdinalIgnoreCase) 'Get the index of "AddMenuObject".

            'If we found "AddMenuObject" then.
            If StartIndex > -1 Then
                'Should split like:
                'AddMenuObject|Name,ClassName,Optional Width|Group1,Group2,Group3,etc..
                'Groups are optional.
                Dim SplitLine As String() = Split(line, "|")
                Select Case SplitLine.Length
                    Case 2 'No groups. 
                        Menu.AddNode(AddItem, Split(SplitLine(1), ","), New String() {})

                    Case 3 'Has Group(s) 
                        Menu.AddNode(AddItem, Split(SplitLine(1), ","), Split(SplitLine(2), ","))

                End Select

                'Fill object list(if not "DoNotFill").
                If ObjectList = "DoNotFill" Then
                ElseIf ObjectList = "" Then
                    ObjectList = line.Remove(0, 1)
                Else
                    ObjectList &= vbNewLine & line.Remove(0, 1)
                End If
            End If

        Loop Until sr.EndOfStream Or (StartIndex = -1 And Not SearchWholeFile)
        sr.Close()

        AddItem.Sort()
    End Sub
    'EndRemoveFromFGS
#End Region

End Module

