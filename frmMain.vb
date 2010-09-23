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

Public Class frmMain
    Enum ToolType
        None
        Move
        Connect
        Add
        Remove
    End Enum
    Private Tool As ToolType
    Private ToolOffset As Point
    Private ToolObject As Object
    Private ToolInt As Integer



    Private Sub frmMain_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        Me.Text = "Flowgraph v" & Application.ProductVersion.ToString

        Load_Main() 'Load all the stuff in mod main. (auto draw, connector pen, etc..)

        'Objects.Add(New fgCounter(New Point(50, 70)))
        'Objects.Add(New fgDisplayAsString(New Point(250, 90)))
        'Objects.Add(New fgDisplayAsString(New Point(250, 200)))
        'Objects.Add(New fgSplit(New Point(50, 200)))
        'Objects.Add(New fgAdd(New Point(50, 300)))
    End Sub


    Private Sub frmMain_MouseDoubleClick(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseDoubleClick
        Mouse.Location = e.Location

        If e.Button = Windows.Forms.MouseButtons.Left Then
            For Each obj As Object In Objects
                If obj.IntersectsWithOutput(Mouse) Then
                    'obj.Output(obj.Intersection).Output(obj.OutputsInput(obj.Intersection)) = Nothing
                    obj.Output(obj.Intersection) = Nothing

                    Return
                ElseIf obj.IntersectsWithInput(Mouse) Then
                    Return
                End If

                'If the mouse intersects with the title bar then move the object.
                If Mouse.IntersectsWith(obj.Rect) Then
                    obj.DoubleClicked()

                    Return
                End If



            Next
        End If
    End Sub

    Private Sub frmMain_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseDown
        Mouse.Location = e.Location

        Select Case Tool
            Case ToolType.None
                If e.Button = Windows.Forms.MouseButtons.Left Then

                    For Each obj As Object In Objects
                        'If the mouse intersects with the title bar then move the object.
                        If Mouse.IntersectsWith(obj.TitleBar) Then
                            Tool = ToolType.Move
                            ToolObject = obj
                            ToolOffset = Mouse.Location - DirectCast(obj.Rect, Rectangle).Location

                            Return
                        End If


                        If obj.IntersectsWithOutput(Mouse) Then
                            Tool = ToolType.Connect
                            ToolObject = obj
                            ToolInt = obj.Intersection
                            ToolOffset = e.Location

                            Return
                        End If
                    Next

                End If



        End Select


    End Sub

    Private Sub frmMain_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseUp
        Mouse.Location = e.Location

        Select Case Tool
            Case ToolType.None
                If e.Button = Windows.Forms.MouseButtons.Right Then
                    Tool = ToolType.Add

                    AddObject_Open()
                End If

            Case ToolType.Add
                If e.Button = Windows.Forms.MouseButtons.Left Then
                    Tool = ToolType.None

                    AddObject_Select()
                End If

            Case ToolType.Move
                Tool = ToolType.None

            Case ToolType.Connect
                Tool = ToolType.None
                Dim m As New Rectangle(e.Location, New Size(1, 1))

                For Each obj As Object In Objects
                    If obj IsNot ToolObject Then
                        If obj.IntersectsWithinput(m) Then
                            'obj.Input(obj.Intersection) = ToolObject
                            ToolObject.Output(ToolInt) = obj
                            ToolObject.OutputsInput(ToolInt) = obj.Intersection

                            DoDraw(True)

                            Return
                        End If
                    End If
                Next


                DoDraw(True)

        End Select

    End Sub


    Private Sub frmMain_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseMove
        Mouse.Location = e.Location

        'Reset tooltip text to noting.
        If Not ToolTipText = "" Then
            ToolTipText = ""
            DoDraw()
        End If

        'Check it see if the mouse is hovering over a input or a output.
        For Each obj As Object In Objects 'Loop thru each object until we found a input/output or we made it thru them all.
            If obj.IntersectsWithInput(Mouse) Then 'Check input.
                ToolTipText = obj.InputNames(obj.Intersection)

                DoDraw()
                Exit For
            ElseIf obj.IntersectsWithOutput(Mouse) Then 'Check output.
                ToolTipText = obj.OutputNames(obj.Intersection)

                DoDraw()
                Exit For
            End If
        Next




        Select Case Tool
            Case ToolType.Add
                DoDraw(True)

            Case ToolType.Move

                ToolObject.SetPosition(e.X - ToolOffset.X, e.Y - ToolOffset.Y)
                DoDraw(True)

            Case ToolType.Connect
                DoDraw(True)


        End Select

    End Sub

    Private ToolTipText As String = ""

    'Draw everything.
    Private Sub frmMain_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint
        'Draw the objects.
        For Each obj As Object In Objects
            obj.Draw(e.Graphics)
        Next

        'Draw the connectors.
        For Each obj As Object In Objects
            obj.DrawConnectors(e.Graphics)
        Next


        Select Case Tool
            Case ToolType.Connect 'If we are using teh connect tool then draw the line.
                e.Graphics.DrawLine(ConnectorPen, ToolOffset, Mouse.Location)

        End Select

        'If there is tooltip text to draw then draw it.
        If Not ToolTipText = "" Then
            Dim s As SizeF = e.Graphics.MeasureString(ToolTipText, DefaultFont)
            e.Graphics.FillRectangle(Brushes.White, Mouse.X + 10, Mouse.Y + 17, s.Width + 2, s.Height + 2)
            e.Graphics.DrawString(ToolTipText, DefaultFont, Brushes.Black, Mouse.X + 11, Mouse.Y + 18)
        End If

        AddObject_Draw(e.Graphics)
    End Sub

End Class
