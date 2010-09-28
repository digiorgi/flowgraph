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

    Private Items() As Node
    Private Structure Node
        Public Children() As Node
        Public Name As String
        Public ClassName As String
        Public Sub Setup(ByVal Name As String, ByVal ClassName As String)
            Me.Name = Name
            Me.ClassName = ClassName
        End Sub
        Public ReadOnly Property IsGroup() As Boolean
            Get
                Return Children IsNot Nothing
            End Get
        End Property
        Public Overrides Function ToString() As String
            Return Name
        End Function
    End Structure

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

        'Create two groups.
        ReDim Items(1)

        'Group 1 is math
        Items(0).Name = "< Math >"
        ReDim Items(0).Children(1) 'Add two nodes to math
        Items(0).Children(0).Setup("Add", "fgadd")
        Items(0).Children(1).Setup("Counter", "fgcounter")

        Items(1).Name = "< Misc >"
        ReDim Items(1).Children(1)
        Items(1).Children(0).Setup("Split", "fgsplit")
        Items(1).Children(1).Setup("Display", "fgdisplayasstring")

        SelectedGroup = Items

        ' Rect.Size = New Size(50, ObjectTypes.Length * 12)
        Rect.Size = New Size(50, Items.Length * 12)
    End Sub

    Private Sub AddObject(ByVal Index As Integer)

        AddObject(LCase(ObjectTypes(Index)))

    End Sub

    Public Sub AddObject_Open()
        AddingObject = True

        StartPos = Mouse.Location
        Rect.Location = Mouse.Location

        SelectedGroup = Items

        DoDraw(True)
    End Sub

    Private SelectedGroup As Node()
    Public Function AddObject_Select() As Boolean




        Dim Found As Boolean = False
        For n As Integer = 0 To SelectedGroup.Length - 1
            If Mouse.IntersectsWith(New Rectangle(Rect.X, Rect.Y + (12 * n), Rect.Width, 12)) Then

                If SelectedGroup(n).IsGroup Then
                    SelectedGroup = SelectedGroup(n).Children
                    Rect.Size = New Size(50, Items.Length * 12)

                    Return False
                Else
                    AddObject(SelectedGroup(n).ClassName, StartPos)

                    Exit For
                End If

                'Found = True
            End If
        Next

        AddingObject = False
        DoDraw(True)
        Return True

        'For n As Integer = 0 To ObjectTypes.Length - 1
        '    If Mouse.IntersectsWith(New Rectangle(Rect.X, Rect.Y + (12 * n), Rect.Width, 12)) Then
        '        AddObject(n)
        '        'ResetObjectIndexs()
        '        Exit For
        '    End If
        'Next


    End Function

    Public Sub AddObject_Draw(ByVal g As Graphics)
        If Not AddingObject Then Return

        g.FillRectangle(Brushes.LightGray, Rect)

        For n As Integer = 0 To SelectedGroup.Length - 1

            If Mouse.IntersectsWith(New Rectangle(Rect.X, Rect.Y + (12 * n), Rect.Width, 12)) Then
                g.FillRectangle(Brushes.White, Rect.X, Rect.Y + (12 * n), Rect.Width, 12)

            End If

            g.DrawString(SelectedGroup(n).Name, DefaultFont, Brushes.Black, Rect.X + 1, Rect.Y + (12 * n))
        Next


        'For n As Integer = 0 To ObjectTypes.Length - 1
        '    If Mouse.IntersectsWith(New Rectangle(Rect.X, Rect.Y + (12 * n), Rect.Width, 12)) Then
        '        g.FillRectangle(Brushes.White, Rect.X, Rect.Y + (12 * n), Rect.Width, 12)
        '    End If

        '    g.DrawString(ObjectTypes(n), DefaultFont, Brushes.Black, Rect.X + 1, Rect.Y + (12 * n))
        'Next
    End Sub




End Module
