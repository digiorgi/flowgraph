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
                            AddObject.AddObject(Items(n).ClassName, MenuStartPosition)
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
        Rect.Size = New Size(Items(0).Width, (Items.Count * 12) + 1)
    End Sub
End Module
