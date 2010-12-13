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

Namespace Menu
    Public Enum Result
        SelectedItem
        SelectedGroup
        Closed
    End Enum

    Public Module Menu

        Public MenuStartPosition As Point

        Private Item As Node
        Private ObjectIndex As Integer = -1

        Public Rect As Rectangle

        Private Title As String = "Main"
        Private TitleRect As RectangleF

        ''' <summary>
        ''' Open a dropdown menu with the items.
        ''' </summary>
        ''' <param name="ObjectIndex">The object the menu will call MenuSlected to. -1 will add the object.</param>
        ''' <param name="Item">The item for the menu to use.</param>
        Public Sub Open(ByVal ObjectIndex As Integer, ByVal Item As Node)
            Title = Item.Name

            Menu.Item = Item
            Menu.ObjectIndex = ObjectIndex

            'Set the menu start position to the current mouse position.
            MenuStartPosition = Mouse.Location

            'Set the tool to menu.
            Tool = ToolType.Menu

            UpdateRect()

            'Draw the newly opened menu.
            DoDraw(True)
        End Sub

        Public Function MouseUp() As Node

            'If the mouse is not in the main rect. then we close the menu.
            If Rect.IntersectsWith(Mouse) Then

                If Mouse.IntersectsWith(New Rectangle(Rect.X, Rect.Y, Rect.Width, 12)) Then
                    If Item.Parent IsNot Nothing Then

                        Item = Item.Parent
                        Title = Item.Name
                        UpdateRect()
                    Else
                    End If
                    Return New Node(Result.SelectedGroup)
                End If

                For n As Integer = 0 To Item.Children.Count - 1
                    If Mouse.IntersectsWith(New Rectangle(Rect.X, Rect.Y + (12 * (n + 1)), Rect.Width, 12)) Then

                        If Item.Children(n).IsGroup Then
                            Item.Children(n).SetResult(Result.SelectedGroup)
                            Dim ReturnNode As Node = Item.Children(n)

                            Item = Item.Children(n)
                            Title = Item.Name
                            UpdateRect()

                            Return ReturnNode
                        Else

                            If ObjectIndex > -1 Then
                                Objects(ObjectIndex).MenuSelected(Item.Children(n))
                            Else
                                AddObject(Item.Children(n).ClassName, MenuStartPosition, Item.Children(n).UserData)
                            End If

                            Item.Children(n).SetResult(Result.SelectedItem)
                            Tool = ToolType.None
                            Return Item.Children(n)
                        End If

                    End If
                Next

            End If

            Tool = ToolType.None
            Return New Node(Result.Closed)
        End Function


        Private LastSelected As Integer
        ''' <summary>
        ''' Returns false if noting changed.
        ''' </summary>
        Public Function MouseMove() As Boolean
            If Not Mouse.IntersectsWith(Rect) Then
                If LastSelected > -1 Then
                    LastSelected = -1
                    Return True
                Else
                    Return False
                End If
            End If
            For n As Integer = 1 To Item.Children.Count
                If Mouse.IntersectsWith(New Rectangle(Rect.X, Rect.Y + (12 * n), Rect.Width, 12)) And n <> LastSelected Then
                    LastSelected = n
                    Return True
                End If
            Next
            Return False
        End Function

        Public Sub Draw(ByVal g As Graphics)
            'Resize the title rectangle if needed.
            If TitleRect.Size = Nothing Then
                TitleRect.Size = g.MeasureString(Title, DefaultFont)
                If TitleRect.Size.Width > Rect.Width Then
                    Rect.Width = TitleRect.Width + 4
                End If
                TitleRect.Location = New PointF(Rect.X + (Rect.Width * 0.5) - (TitleRect.Width * 0.5), Rect.Y - 1)
            End If

            'Draw the background.
            g.FillRectangle(SystemBrushes.Menu, Rect)

            'Draw deviding line betwen the title and the items.
            g.DrawLine(Pens.Black, Rect.Location + New Point(0, 12), Rect.Location + New Point(Rect.Width, 12))
            g.DrawString(Title, DefaultFont, SystemBrushes.MenuText, TitleRect) 'Draw the title.


            For n As Integer = 1 To Item.Children.Count

                If Mouse.IntersectsWith(New Rectangle(Rect.X, Rect.Y + (12 * n), Rect.Width, 12)) Then
                    g.FillRectangle(SystemBrushes.Highlight, Rect.X, Rect.Y + (12 * n), Rect.Width, 12)
                    g.DrawString(Item.Children(n - 1).ToString, DefaultFont, SystemBrushes.HighlightText, Rect.X + 1, Rect.Y + (12 * n))
                Else
                    g.DrawString(Item.Children(n - 1).ToString, DefaultFont, SystemBrushes.MenuText, Rect.X + 1, Rect.Y + (12 * n))
                End If


            Next

            g.DrawRectangle(Pens.Black, Rect)
        End Sub

        Private Sub UpdateRect()
            'Set the min width to 60.
            Dim Width As Integer = 60
            'Look in each node and see if there is any with a biger width.
            For Each Node As Node In Item.Children
                If Node.Width > Width Then
                    Width = Node.Width
                End If
            Next
            'Set the size of the rectangle.
            Rect.Size = New Size(Width, (Item.Children.Count * 12) + 13)
            TitleRect.Size = Nothing 'Set the title rectangle to nothing so next draw it will find it.

            'Center over mouse start position.
            Rect.X = MenuStartPosition.X - (Rect.Width * 0.5)
            Rect.Y = MenuStartPosition.Y - (Rect.Height * 0.5)

            'Clip the menu to the window.
            If Rect.X < 0 Then Rect.X = 1
            If Rect.Y < 0 Then Rect.Y = 1
            If Rect.Right > Form.ClientSize.Width Then Rect.X = Form.ClientSize.Width - Rect.Width - 1
            If Rect.Bottom > Form.ClientSize.Height Then Rect.Y = Form.ClientSize.Height - Rect.Height - 1
        End Sub

        ''' <summary>
        ''' Add a menu node to a node list.
        ''' Usefull to add groups.
        ''' </summary>
        ''' <param name="Item">The item to add the node to.</param>
        ''' <param name="Data">Name,Optional ClassName Or Width, Optional Width</param>
        ''' <param name="Groups"></param>
        ''' <remarks></remarks>
        Public Function AddNode(ByVal Item As Node, ByVal Data As String(), ByVal Groups As String()) As Node
            Dim Node As New Node
            Select Case Data.Length
                Case 0
                    Return Nothing

                Case 2
                    'Is it width or a class?
                    Try
                        If Data(1) <> "" Then Node.Width = Data(1)
                    Catch ex As Exception
                        Node.ClassName = Data(1)
                    End Try

                Case 3
                    Node.ClassName = Data(1)
                    If Data(2) <> "" Then Node.Width = Data(2)

                Case 4
                    Node.ClassName = Data(1)
                    If Data(2) <> "" Then Node.Width = Data(2)
                    Node.UserData = Data(3)
            End Select
            Node.Name = Data(0)

            'Is there any groups?
            If Groups.Length > 0 Then

                Dim CurrentGroup As Integer = 0
                Dim CurrentNode As Node = Item
                Do
                    Dim Found As Boolean = False
                    If CurrentNode.Children IsNot Nothing Then
                        For Each n As Node In CurrentNode.Children
                            If LCase(n.Name) = LCase(Groups(CurrentGroup)) And n.IsGroup Then
                                CurrentNode = n
                                Found = True
                                Exit For
                            End If
                        Next
                    End If
                    If Found = False Then

                        CurrentNode.Add(New Node(Groups(CurrentGroup), True))
                        CurrentNode = CurrentNode.Children(CurrentNode.Children.Count - 1)
                    End If


                    CurrentGroup += 1
                Loop Until CurrentGroup = Groups.Length

                CurrentNode.Add(Node)

            Else
                'There was no groups so lets just add the item.
                AddItem.Add(Node)
            End If


            Return Node
        End Function
    End Module

    Public Class Node
        Public Parent As Node
        Public Children As List(Of Node)
        Public Name As String
        Public ClassName As String
        Public Width As Integer
        Public Result As Result
        Public UserData As String

#Region "New"
        Sub New()
        End Sub

        ''' <summary>Is used when the menu sends the object the node.</summary>
        ''' <param name="Result">What was the menus results?</param>
        Sub New(ByVal Result As Result)
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
            If IsGroup Then Children = New List(Of Node)
        End Sub
#End Region

        Public Sub Add(ByVal Item As Node)
            Item.Parent = Me
            Children.Add(Item)
        End Sub

        Public Sub SetResult(ByVal Result As Result)
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
    End Class
End Namespace
