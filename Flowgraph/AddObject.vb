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
