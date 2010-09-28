Public Module Menu
    Public Structure MenuNode
        Public Children() As MenuNode
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
    Public Enum MenuResult
        SelectedGroup = -2
        Closed = -1
    End Enum


    Public Function Menu_MouseUp(ByVal Items() As MenuNode, ByVal Position As Point) As Integer
        Return Menu_MouseUp(Items, New Rectangle(Position, New Size(Items(0).Width, (Items.Length * 12) + 1)))
    End Function
    Public Function Menu_MouseUp(ByRef Items() As MenuNode, ByVal Rect As Rectangle) As Integer

        'If the mouse is not in the main rect then return false
        If Not Rect.IntersectsWith(Mouse) Then
            Return MenuResult.Closed
        End If

        For n As Integer = 0 To Items.Length - 1
            If Mouse.IntersectsWith(New Rectangle(Rect.X, Rect.Y + (12 * n), Rect.Width, 12)) Then

                If Items(n).IsGroup Then
                    Items = Items(n).Children

                    Return MenuResult.SelectedGroup
                Else


                    Return n
                End If

            End If
        Next


        Return MenuResult.Closed
    End Function

    Public Sub Menu_Draw(ByVal g As Graphics, ByVal Items() As MenuNode, ByVal Position As Point)
        Menu_Draw(g, Items, New Rectangle(Position, New Size(Items(0).Width, (Items.Length * 12) + 1)))
    End Sub
    Public Sub Menu_Draw(ByVal g As Graphics, ByVal Items() As MenuNode, ByVal Rect As Rectangle)

        g.FillRectangle(Brushes.LightGray, Rect)


        For n As Integer = 0 To Items.Length - 1

            If Mouse.IntersectsWith(New Rectangle(Rect.X, Rect.Y + (12 * n), Rect.Width, 12)) Then
                g.FillRectangle(Brushes.White, Rect.X, Rect.Y + (12 * n), Rect.Width, 12)

            End If

            g.DrawString(Items(n).ToString, DefaultFont, Brushes.Black, Rect.X + 1, Rect.Y + (12 * n))
        Next

        g.DrawRectangle(Pens.Black, Rect)
    End Sub
End Module
