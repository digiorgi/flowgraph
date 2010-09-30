Public Module Menu
    Public Structure MenuNode
        Public Children() As MenuNode
        Public Name As String
        Public ClassName As String
        Public Width As Integer
        Public Result As MenuResult
        Sub New(ByVal Result As MenuResult)
            Me.Result = Result
        End Sub
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
    Public Enum MenuResult
        SelectedItem
        SelectedGroup '= -2
        Closed '= -1
    End Enum

    Public MenuStartPosition As Point
    Private Items() As MenuNode
    Private ObjectIndex As Integer = -1
    Private Rect As Rectangle


    Public Sub Menu_Open(ByVal ObjectIndex As Integer, ByVal Items() As MenuNode)


        Menu.Items = Items
        Menu.ObjectIndex = ObjectIndex

        MenuStartPosition = Mouse.Location

        Tool = ToolType.Menu

        Rect.Location = MenuStartPosition
        UpdateRectSize()


        DoDraw(True)
    End Sub

    Public Function Menu_MouseUp() As MenuNode

        'If the mouse is not in the main rect then return false
        If Rect.IntersectsWith(Mouse) Then


            For n As Integer = 0 To Items.Length - 1
                If Mouse.IntersectsWith(New Rectangle(Rect.X, Rect.Y + (12 * n), Rect.Width, 12)) Then

                    If Items(n).IsGroup Then
                        Items = Items(n).Children

                        UpdateRectSize()

                        Items(n).Result = MenuResult.SelectedGroup
                        Return Items(n)
                    Else

                        If ObjectIndex > -1 Then
                            Objects(ObjectIndex).MenuSelected(Items(n))
                        Else
                            AddObject.AddObject(Items(n).ClassName, MenuStartPosition)
                        End If

                        Items(n).Result = MenuResult.SelectedItem
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


        For n As Integer = 0 To Items.Length - 1

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
        Rect.Size = New Size(Items(0).Width, (Items.Length * 12) + 1)
    End Sub
End Module
