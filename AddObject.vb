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

Public Module AddObject

    ' Private Rect As Rectangle

    Private Items(0) As MenuNode


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

        'SelectedGroup = Items

        'SetSize()
    End Sub

    Public Sub AddObject_Open()
        Menu_Open(-1, Items)
    End Sub

End Module
