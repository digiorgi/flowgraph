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

Module AddObject

    Public AddingObject As Boolean = False

    Private Rect As Rectangle

    Private ObjectTypes() As String

    Private StartPos As Point

    Public Function AddObject(ByVal Name As String, ByVal Position As Point) As Integer
        Select Case LCase(Name)
            Case "fgadd"
                Objects.Add(New fgAdd(Position))

            Case "fgsplit"
                Objects.Add(New fgSplit(Position))

            Case "fgcounter"
                Objects.Add(New fgCounter(Position))

            Case "fgdisplayasstring"
                Objects.Add(New fgDisplayAsString(Position))

        End Select

        Return Objects.Count - 1
    End Function

    Public Sub AddObject_Setup()
        ReDim ObjectTypes(3)
        ObjectTypes(0) = "Add"
        ObjectTypes(1) = "Split"
        ObjectTypes(2) = "Counter"
        ObjectTypes(3) = "Display"

        Rect.Size = New Size(50, ObjectTypes.Length * 12)

    End Sub

    Private Sub AddObject(ByVal Index As Integer)
        Dim Name As String = LCase(ObjectTypes(Index))

        Select Case Name
            Case "add"
                Objects.Add(New fgAdd(StartPos))

            Case "split"
                Objects.Add(New fgSplit(StartPos))

            Case "counter"
                Objects.Add(New fgCounter(StartPos))

            Case "display"
                Objects.Add(New fgDisplayAsString(StartPos))

        End Select

    End Sub

    Public Sub AddObject_Open()
        AddingObject = True

        StartPos = Mouse.Location
        Rect.Location = Mouse.Location

        DoDraw(True)
    End Sub

    Public Sub AddObject_Select()
        AddingObject = False

        For n As Integer = 0 To ObjectTypes.Length - 1
            If Mouse.IntersectsWith(New Rectangle(Rect.X, Rect.Y + (12 * n), Rect.Width, 12)) Then
                AddObject(n)
                'ResetObjectIndexs()
                Exit For
            End If
        Next

        DoDraw(True)
    End Sub

    Public Sub AddObject_Draw(ByVal g As Graphics)
        If Not AddingObject Then Return

        g.FillRectangle(Brushes.LightGray, Rect)

        For n As Integer = 0 To ObjectTypes.Length - 1
            If Mouse.IntersectsWith(New Rectangle(Rect.X, Rect.Y + (12 * n), Rect.Width, 12)) Then
                g.FillRectangle(Brushes.White, Rect.X, Rect.Y + (12 * n), Rect.Width, 12)
            End If

            g.DrawString(ObjectTypes(n), DefaultFont, Brushes.Black, Rect.X + 1, Rect.Y + (12 * n))
        Next
    End Sub




End Module
