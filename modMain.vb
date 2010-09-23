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
    Public GridSize As Integer = 10

    Public DefaultFont As Font = SystemFonts.DefaultFont

    Public ConnectorPen As New Pen(Color.FromArgb(80, 80, 80), 3)

    Public Mouse As Rectangle

    Public Objects As New List(Of Object)

    ''' <summary>
    ''' Loads the stuff in modMain.
    ''' </summary>
    Public Sub Load_Main()
        SetupAutoDraw()

        Mouse = New Rectangle(0, 0, 1, 1)

        AddObject_Setup()

    End Sub

#Region "Auto draw"
    Private DoNotDraw As Boolean = True

    ''' <summary>
    ''' Tells auto draw to draw when the time comes.
    ''' </summary>
    ''' <param name="HeighPriority">If it's a heigh priority then it will instantly draw</param>
    Public Sub DoDraw(Optional ByVal HeighPriority As Boolean = False)
        DoNotDraw = False
        If HeighPriority Then tmrDraw_Tick(Nothing, Nothing)
    End Sub

    Public Sub SetupAutoDraw()
        tmrDraw.Interval = 200
        tmrDraw.Enabled = True
    End Sub

    Private WithEvents tmrDraw As New Timer
    Private Sub tmrDraw_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles tmrDraw.Tick
        If DoNotDraw Then Return

        frmMain.Invalidate()

        DoNotDraw = True
    End Sub
#End Region


End Module
