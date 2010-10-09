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
    Event DrawEvent()
    Event AddControlEvent(ByVal Control As Control)
    Event RemoveControlEvent(ByVal Control As Control)

    'The snap grid size.
    Public GridSize As Integer = 5

    'The default font to use.
    Public DefaultFont As Font = SystemFonts.DefaultFont
    Public DefaultFontBrush As Brush = SystemBrushes.ControlText

    'The pen used to connect objects.
    Public ConnectorPen As New Pen(Color.FromArgb(80, 80, 80), 3)

    'Used to check if the mouse is inside a rectangle.
    Public Mouse As Rectangle

    'The list of objects.
    Public Objects As New List(Of Object)

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

    Public Sub ResetObjectIndexs(ByVal RemovedIndex As Integer)
        For n As Integer = 0 To Objects.Count - 1
            Objects(n).Index = n


            If Objects(n).Output IsNot Nothing Then
                For o As Integer = 0 To Objects(n).Output.Length - 1
                    Dim i As Integer = 0
                    Do
                        If Objects(n).Output(o).Flow(i).obj = RemovedIndex Then
                            Objects(n).Output(o).Flow(i) = Nothing
                            Objects(n).Output(o).Flow.RemoveAt(i)
                        ElseIf Objects(n).Output(o).Flow(i).obj > RemovedIndex Then
                            Objects(n).Output(o).Flow(i).obj -= 1
                            i += 1
                        Else
                            i += 1
                        End If
                    Loop Until i = Objects(n).Output(o).Flow.Count
                Next
            End If
        Next
    End Sub

    Public Sub RemoveAt(ByVal Index As Integer)
        Objects(Index).Distroy()
        Objects(Index) = Nothing
        Objects.RemoveAt(Index)

        ResetObjectIndexs(Index)
    End Sub


    ''' <summary>
    ''' Loads the stuff in modMain.
    ''' </summary>
    Public Sub Load_Main()
        'Setup the auto draw timmer.
        tmrDraw.Interval = 200
        tmrDraw.Enabled = True


        AddObject_Setup()


    End Sub

#Region "Open & Save"
    Public LoadedFile As String = ""

    Public Sub Open(ByVal File As String)
        If Not IO.File.Exists(File) Then
            MsgBox("Could not find file:" & vbNewLine & File, , "Error loading")
            Return
        End If

        ClearObjects()

        Dim sd As New SimpleD.SimpleD
        sd.FromFile(File)

        Dim g As SimpleD.Group = sd.Get_Group("Main")
        Dim numObj As Integer = g.Get_Value("Objects")
        For n As Integer = 0 To numObj
            g = sd.Get_Group("Object" & n)
            Dim pos As String() = Split(g.Get_Value("position"), ",")
            Dim obj As Integer = AddObject(g.Get_Value("name"), New Point(pos(0), pos(1)))
            'Objects(obj).Load(g)

        Next

        For n As Integer = 0 To numObj
            g = sd.Get_Group("Object" & n)
            Objects(n).Load(g)
        Next

        LoadedFile = File
    End Sub

    Public Sub Save(ByVal File As String)

        Dim sd As New SimpleD.SimpleD
        Dim g As SimpleD.Group = sd.Create_Group("Main")
        g.Add("Objects", Objects.Count - 1)

        For Each obj As Object In Objects
            sd.Add_Group(obj.Save)
        Next

        sd.ToFile(File)

        LoadedFile = File
    End Sub

    Public Sub ClearObjects()
        For Each obj As Object In Objects
            obj.Distroy()
        Next
        Objects.Clear()
    End Sub
#End Region

#Region "Auto draw"
    Private DoNotDraw As Boolean = True

    ''' <summary>
    ''' Tells auto draw to draw when the time comes.
    ''' </summary>
    ''' <param name="HeighPriority">If it's a heigh priority, then it will draw as soon as possible.</param>
    Public Sub DoDraw(Optional ByVal HeighPriority As Boolean = False)

        'If it is a heigh priority. then we will not wait for the next timmer tick and just draw.
        If HeighPriority Then
            RaiseEvent DrawEvent()

        Else 'Other wise we wait for the timer.
            DoNotDraw = False 'Tell the timer it can draw.
        End If

    End Sub

    Private WithEvents tmrDraw As New Timer
    Private Sub tmrDraw_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles tmrDraw.Tick
        If DoNotDraw Then Return

        RaiseEvent DrawEvent()

        DoNotDraw = True
    End Sub
#End Region

    Public Sub AddControl(ByVal Control As Control)
        RaiseEvent AddControlEvent(Control)
    End Sub
    Public Sub RemoveControl(ByVal Control As Control)
        RaiseEvent RemoveControlEvent(Control)
    End Sub


    Public AddItems As New List(Of MenuNode)

    'NOTE: This whole sub will be created with the plugin compiler.
    Public Function AddObject(ByVal Name As String, ByVal Position As Point) As Integer
        Select Case LCase(Name)
            Case "fgadd"
                Objects.Add(New fgAdd(Position))

            Case "fgcounter"
                Objects.Add(New fgCounter(Position))

            Case "fgdisplayasstring"
                Objects.Add(New fgDisplayAsString(Position))

            Case Else
                Return -1
        End Select

        Return Objects.Count - 1
    End Function

    'NOTE: This whole sub will be created with the plugin compiler.
    Public Sub AddObject_Setup()

        'Group 1 is math
        AddItems.Add(New MenuNode("Math >", True, 50)) 'The first node of each list holds the width of the list.
        'Add two nodes to math
        AddItems(0).Children.Add(New MenuNode("Add", "fgadd", 60))
        AddItems(0).Children.Add(New MenuNode("Counter", "fgcounter"))

        AddItems.Add(New MenuNode("Misc >", True))
        AddItems(1).Children.Add(New MenuNode("Display As String", "fgdisplayasstring", 100))

    End Sub
End Module