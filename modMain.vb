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

Module modMain
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


            If Objects(n).output IsNot Nothing Then
                For Each out As DataFlowBase In Objects(n).output
                    Dim i As Integer
                    Do
                        If out.Flow(i).obj = RemovedIndex Then
                            out.Flow(i) = Nothing
                            out.Flow.RemoveAt(i)
                        ElseIf out.Flow(i).obj > RemovedIndex Then
                            out.Flow(i).AddToObj(-1)
                            i += 1
                        Else
                            i += 1
                        End If
                    Loop Until i = out.Flow.Count
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

#Region "Auto draw"
    Private DoNotDraw As Boolean = True

    ''' <summary>
    ''' Tells auto draw to draw when the time comes.
    ''' </summary>
    ''' <param name="HeighPriority">If it's a heigh priority, then it will draw as soon as possible.</param>
    Public Sub DoDraw(Optional ByVal HeighPriority As Boolean = False)

        'If it is a heigh priority. then we will not wait for the next timmer tick and just draw.
        If HeighPriority Then
            frmMain.Invalidate()

        Else 'Other wise we wait for the timer.
            DoNotDraw = False 'Tell the timer it can draw.
        End If

    End Sub

    Private WithEvents tmrDraw As New Timer
    Private Sub tmrDraw_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles tmrDraw.Tick
        If DoNotDraw Then Return

        frmMain.Invalidate()

        DoNotDraw = True
    End Sub
#End Region

    Public Sub AddControl(ByVal Control As Control)
        frmMain.Controls.Add(Control)
    End Sub
    Public Sub RemoveControl(ByVal Control As Control)
        frmMain.Controls.Remove(Control)
    End Sub

End Module
