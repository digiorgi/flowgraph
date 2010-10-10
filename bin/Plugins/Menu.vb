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

Public Module Menu
    Public Structure MenuNode
        Public Children As List(Of MenuNode)
        Public Name As String
        Public ClassName As String
        Public Width As Integer
        Public Result As MenuResult

        ''' <summary>Is used when the menu sends the object the node.</summary>
        ''' <param name="Result">What was the menus results?</param>
        Sub New(ByVal Result As MenuResult)
            Me.Result = Result
        End Sub

        ''' <summary>Create a node</summary>
        ''' <param name="Name">The name that will display in the menu.</param>
        ''' <param name="ClassName">The name of the class to create for adding objects.</param>
        ''' <param name="Width">The first node in each group. Specifies the width of the menu.</param>
        Public Sub New(ByVal Name As String, ByVal ClassName As String, Optional ByVal Width As Integer = 50)
            Me.Name = Name
            Me.ClassName = ClassName
            Me.Width = Width
        End Sub
        ''' <summary>Create a node</summary>
        ''' <param name="Name">The name that will display in the menu.</param>
        ''' <param name="IsGroup">Is this node going to have children?</param>
        ''' <param name="Width">The first node in each group. Specifies the width of the menu.</param>
        Public Sub New(ByVal Name As String, Optional ByVal IsGroup As Boolean = False, Optional ByVal Width As Integer = 50)
            Me.Name = Name
            Me.Width = Width
            If IsGroup Then Children = New List(Of MenuNode)
        End Sub

        Public Sub SetResult(ByVal Result As MenuResult)
            Me.Result = Result
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
    Public Enum MenuResult
        SelectedItem
        SelectedGroup
        Closed
    End Enum

    Public MenuStartPosition As Point
    Private Items As List(Of MenuNode)
    Private ObjectIndex As Integer = -1
    Private Rect As Rectangle

    ''' <summary>
    ''' Open a dropdown menu with the items.
    ''' </summary>
    ''' <param name="ObjectIndex">The object the menu will call MenuSlected to. -1 will add the object.</param>
    ''' <param name="Items">The items for the menu to use.</param>
    Public Sub Menu_Open(ByVal ObjectIndex As Integer, ByVal Items As List(Of MenuNode))


        Menu.Items = Items
        Menu.ObjectIndex = ObjectIndex

        'Set the menu start position to the current mouse position.
        MenuStartPosition = Mouse.Location

        'Set the tool to menu.
        Tool = ToolType.Menu

        'Update the rectangle position and size.
        Rect.Location = MenuStartPosition
        UpdateRectSize()

        'Draw the newly opened menu.
        DoDraw(True)
    End Sub

    Public Function Menu_MouseUp() As MenuNode

        'If the mouse is not in the main rect. then we close the menu.
        If Rect.IntersectsWith(Mouse) Then


            For n As Integer = 0 To Items.Count - 1
                If Mouse.IntersectsWith(New Rectangle(Rect.X, Rect.Y + (12 * n), Rect.Width, 12)) Then

                    If Items(n).IsGroup Then
                        Items(n).SetResult(MenuResult.SelectedGroup)
                        Dim ReturnNode As MenuNode = Items(n)

                        Items = Items(n).Children
                        UpdateRectSize()

                        Return ReturnNode
                    Else

                        If ObjectIndex > -1 Then
                            Objects(ObjectIndex).MenuSelected(Items(n))
                        Else
                            AddObject(Items(n).ClassName, MenuStartPosition)
                        End If

                        Items(n).SetResult(MenuResult.SelectedItem)
                        Tool = ToolType.None
                        Return Items(n)
                    End If

                End If
            Next

        End If

        Tool = ToolType.None
        Return New MenuNode(MenuResult.Closed)
    End Function

    Public Sub Menu_Draw(ByVal g As Graphics)

        g.FillRectangle(SystemBrushes.Menu, Rect)


        For n As Integer = 0 To Items.Count - 1

            If Mouse.IntersectsWith(New Rectangle(Rect.X, Rect.Y + (12 * n), Rect.Width, 12)) Then
                g.FillRectangle(SystemBrushes.Highlight, Rect.X, Rect.Y + (12 * n), Rect.Width, 12)
                g.DrawString(Items(n).ToString, DefaultFont, SystemBrushes.HighlightText, Rect.X + 1, Rect.Y + (12 * n))
            Else
                g.DrawString(Items(n).ToString, DefaultFont, SystemBrushes.MenuText, Rect.X + 1, Rect.Y + (12 * n))
            End If


        Next

        g.DrawRectangle(Pens.Black, Rect)
    End Sub

    Private Sub UpdateRectSize()
        Dim Width As Integer ' = 40
        For Each item As MenuNode In Items
            If item.Width > Width Then
                Width = item.Width
            End If
        Next
        Rect.Size = New Size(Width, (Items.Count * 12) + 1)
    End Sub


    ''' <summary>
    ''' Add a menu node to a node list.
    ''' Usefull to add groups.
    ''' </summary>
    ''' <param name="Items">The list of items to add the node to.</param>
    ''' <param name="Data">Name,Optional ClassName Or Width, Optional Width</param>
    ''' <param name="Groups"></param>
    ''' <remarks></remarks>
    Public Sub AddNode(ByVal Items As List(Of MenuNode), ByVal Data As String(), ByVal Groups As String())
        Dim Node As New MenuNode
        Node.Width = 50
        Select Case Data.Length
            Case 0
                Return

            Case 2
                'Is it width or a class?
                Try
                    Node.Width = Data(1)
                Catch ex As Exception
                    Node.ClassName = Data(1)
                End Try

            Case 3
                Node.ClassName = Data(1)
                Node.Width = Data(2)

        End Select
        Node.Name = Data(0)

        'Is there any groups?
        If Groups.Length > 0 Then

            Dim CurrentGroup As Integer = 0
            Dim Nodes As List(Of MenuNode) = Items
            Do
                Dim Found As Boolean = False
                For Each n As MenuNode In Nodes
                    If LCase(n.Name) = LCase(Groups(CurrentGroup)) And n.IsGroup Then
                        Nodes = n.Children
                        Found = True
                        Exit For
                    End If
                Next
                If Found = False Then

                    Nodes.Add(New MenuNode(Groups(CurrentGroup), True))
                    Nodes = Nodes(Nodes.Count - 1).Children
                End If


                CurrentGroup += 1
            Loop Until CurrentGroup = Groups.Length

            Nodes.Add(Node)

        Else
            'There was no groups so lets just add the item.
            AddItems.Add(Node)
        End If



    End Sub
End Module
