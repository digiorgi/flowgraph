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
   

#Region "Load & Close"

    Private Sub frmMain_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If MsgBox("Do you want to save your work?", MsgBoxStyle.YesNo, "Save") = MsgBoxResult.Yes Then
            btnSave_Click(sender, e)
        End If

    End Sub

    Private Sub frmMain_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        Me.Text = "Flowgraph v" & Application.ProductVersion.ToString

        Load_Main() 'Load all the stuff in mod main. (auto draw, connector pen, etc..)

        Dim args As String() = Environment.GetCommandLineArgs
        If args.Length = 2 Then
            Open(args(1))
        End If
    End Sub

#End Region

#Region "Mouse"

    Private Sub frmMain_MouseDoubleClick(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseDoubleClick
        Mouse.Location = e.Location

        If e.Button = Windows.Forms.MouseButtons.Left Then
            For Each obj As Object In Objects
                If obj.IntersectsWithOutput(Mouse) Then

                    obj.Output(obj.Intersection).Disconnect()

                    Return
                ElseIf obj.IntersectsWithInput(Mouse) Then
                    obj.Input(obj.Intersection).Disconnect()

                    Return
                End If

                'If the mouse intersects with the title bar then move the object.
                If Mouse.IntersectsWith(obj.Rect) Then
                    obj.MouseDoubleClick(e)

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
                            ToolObject = obj.Index
                            ToolOffset = Mouse.Location - DirectCast(obj.Rect, Rectangle).Location

                            Return
                        End If


                        If obj.IntersectsWithOutput(Mouse) Then
                            Tool = ToolType.Connect
                            ToolObject = obj.Index
                            ToolInt = obj.Intersection
                            ToolOffset = e.Location

                            Return
                        End If
                    Next

                End If

                For Each obj As Object In Objects
                    If Mouse.IntersectsWith(obj.Rect) Then
                        obj.MouseDown(e)
                        Return
                    End If
                Next



        End Select


    End Sub

    Private Sub frmMain_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseUp
        Mouse.Location = e.Location

        Select Case Tool
            Case ToolType.None
                'Check to see if the mouse is in a object.
                For Each obj As Object In Objects
                    If Mouse.IntersectsWith(obj.Rect) Then
                        obj.MouseUp(e)
                        Return
                    End If
                Next
                If e.Button = Windows.Forms.MouseButtons.Right Then
                    Menu_Open(-1, AddItems)
                End If

            Case ToolType.Menu
                If e.Button = Windows.Forms.MouseButtons.Left Then
                    Menu_MouseUp()
                    DoDraw(True)
                ElseIf e.Button = Windows.Forms.MouseButtons.Right Then
                    For Each obj As Object In Objects
                        If Mouse.IntersectsWith(obj.Rect) Then
                            Tool = ToolType.None
                            obj.MouseUp(e)
                            DoDraw(True)
                            Return
                        End If
                    Next
                    Menu_Open(-1, AddItems)
                End If


            Case ToolType.Move
                    Tool = ToolType.None



            Case ToolType.Connect
                Tool = ToolType.None

                For Each obj As Object In Objects
                    If obj.Index <> ToolObject Then
                        If obj.IntersectsWithInput(Mouse) Then

                            'Try and connect.
                            If Objects(ToolObject).Output(ToolInt).Add(obj.Index, obj.Intersection) Then
                                'Add one to connected if it successfully connected.
                                obj.Input(obj.Intersection).Connected += 1
                            End If


                            Exit For
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
        For Each obj As Object In Objects 'Loop through each object until we found a input/output or we made it through them all.
            If obj.IntersectsWithInput(Mouse) Then 'Check input.
                ToolTipText = obj.Input(obj.Intersection).ToString

                DoDraw()
                Exit For
            ElseIf obj.IntersectsWithOutput(Mouse) Then 'Check output.
                ToolTipText = obj.Output(obj.Intersection).ToString

                DoDraw()
                Exit For
            End If
        Next




        Select Case Tool
            Case ToolType.None
                'Check to see if the mouse is in a object.
                For Each obj As Object In Objects
                    If Mouse.IntersectsWith(obj.Rect) Then
                        obj.MouseMove(e)
                        Return
                    End If
                Next

            Case ToolType.Menu
                DoDraw(True)

            Case ToolType.Move

                Objects(ToolObject).SetPosition(e.X - ToolOffset.X, e.Y - ToolOffset.Y)
                DoDraw(True)

            Case ToolType.Connect
                DoDraw(True)


        End Select

    End Sub
#End Region

    Private ToolTipText As String = ""

    'Draw everything.
    Private Sub frmMain_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint
        e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality

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

            Case ToolType.Menu
                Menu_Draw(e.Graphics)

        End Select

        'If there is tooltip text to draw then draw it.
        If Not ToolTipText = "" Then
            Dim s As SizeF = e.Graphics.MeasureString(ToolTipText, DefaultFont)
            e.Graphics.FillRectangle(SystemBrushes.Info, Mouse.X + 10, Mouse.Y + 17, s.Width + 2, s.Height + 2)
            e.Graphics.DrawString(ToolTipText, DefaultFont, SystemBrushes.InfoText, Mouse.X + 11, Mouse.Y + 18)
        End If


    End Sub

    Private Sub btnOpen_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOpen.Click
        Dim ofd As New OpenFileDialog
        If ofd.ShowDialog = Windows.Forms.DialogResult.OK Then
            Open(ofd.FileName)
        End If
    End Sub

    Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
        If LoadedFile = "" Then
            btnSaveAs_Click(sender, e)
        Else
            Save(LoadedFile)
        End If
    End Sub

    Private Sub btnSaveAs_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSaveAs.Click
        Dim sfd As New SaveFileDialog
        'sfd.Title'
        If sfd.ShowDialog = Windows.Forms.DialogResult.OK Then
            Save(sfd.FileName)
        End If
    End Sub
End Class
