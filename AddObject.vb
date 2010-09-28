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

    Private Items(0) As Node
    Private Structure Node
        Public Children() As Node
        Public Name As String
        Public ClassName As String
        Public Width As Integer
        Public Sub Setup(ByVal Name As String, ByVal ClassName As String, Optional ByVal Width As Integer = 50)
            Me.Name = Name
            Me.ClassName = ClassName
            Me.Width = Width
        End Sub
        Public Sub Setup(ByVal Name As String, Optional ByVal Width As Integer = 50)
            Me.Name = Name
            Me.Width = Width
        End Sub
        Public ReadOnly Property IsGroup() As Boolean
            Get
                Return Children IsNot Nothing
            End Get
        End Property
        Public Overrides Function ToString() As String
            If Name = "" Then
                Return "NoName"
            Else
                Return Name
            End If
        End Function
    End Structure

    'Private StartPos As Point

    'NOTE: This whole sub will be created with the plugin compiler.
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

            Case Else
                Return -1
        End Select

        Return Objects.Count - 1
    End Function

    'NOTE: This whole sub will be created with the plugin compiler.
    Public Sub AddObject_Setup()


        'Create two groups.
        ReDim Items(1)


        'Group 1 is math
        Items(0).Setup("Math >", 50) 'The first node of each list holds the width of the list.
        ReDim Items(0).Children(1) 'Add two nodes to math
        Items(0).Children(0).Setup("Add", "fgadd", 60)
        Items(0).Children(1).Setup("Counter", "fgcounter")

        Items(1).Name = "Misc >"
        ReDim Items(1).Children(4)
        Items(1).Children(0).Setup("Split", "fgsplit", 120)
        Items(1).Children(1).Setup("Display As String", "fgdisplayasstring")

        SelectedGroup = Items

        SetSize()
    End Sub

    Public Sub AddObject_Open()
        AddingObject = True

        'StartPos = Mouse.Location
        Rect.Location = Mouse.Location

        'Set the selected group to the base items.
        SelectedGroup = Items
        SetSize()

        DoDraw(True)
    End Sub

    Private Sub SetSize()
        Rect.Size = New Size(SelectedGroup(0).Width, (SelectedGroup.Length * 12) + 1)
    End Sub

    Private SelectedGroup As Node()
    Public Function AddObject_Select(ByVal b As MouseButtons) As Boolean
        If b <> MouseButtons.Left Then
            AddingObject = False
            DoDraw(True)
            Return True
        End If


        For n As Integer = 0 To SelectedGroup.Length - 1
            If Mouse.IntersectsWith(New Rectangle(Rect.X, Rect.Y + (12 * n), Rect.Width, 12)) Then

                If SelectedGroup(n).IsGroup Then
                    SelectedGroup = SelectedGroup(n).Children
                    SetSize()

                    Return False
                Else
                    AddObject(SelectedGroup(n).ClassName, Rect.Location)

                    Exit For
                End If

            End If
        Next

        AddingObject = False
        DoDraw(True)
        Return True


    End Function

    Public Sub AddObject_Draw(ByVal g As Graphics)
        If Not AddingObject Then Return

        g.FillRectangle(Brushes.LightGray, Rect)


        For n As Integer = 0 To SelectedGroup.Length - 1

            If Mouse.IntersectsWith(New Rectangle(Rect.X, Rect.Y + (12 * n), Rect.Width, 12)) Then
                g.FillRectangle(Brushes.White, Rect.X, Rect.Y + (12 * n), Rect.Width, 12)

            End If

            g.DrawString(SelectedGroup(n).ToString, DefaultFont, Brushes.Black, Rect.X + 1, Rect.Y + (12 * n))
        Next

        g.DrawRectangle(Pens.Black, Rect)
    End Sub




End Module
