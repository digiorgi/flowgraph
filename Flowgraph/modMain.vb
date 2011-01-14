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

Module modMain
    Public frmMain As New frmMain
    Public frmAbout As New frmAbout

    Sub Main()


        Application.Run(frmMain)
    End Sub

    Public Sub Load_PluginSystem()
        AddHandler AddControlEvent, AddressOf AddControl
        AddHandler RemoveControlEvent, AddressOf RemoveControl

        AddHandler DrawEvent, AddressOf Draw

        Load_Plugin(frmMain) 'Load the plugin stuff. (auto draw, connector pen, etc..)
    End Sub

    Private Sub Draw(ByVal region As Rectangle)
        If region.IsEmpty Then
            frmMain.Invalidate()
        Else
            frmMain.Invalidate(region)
        End If
    End Sub

    Public Sub AddControl(ByVal Control As Control)
        frmMain.Controls.Add(Control)
        frmMain.lblToolTip.BringToFront()
    End Sub
    Public Sub RemoveControl(ByVal Control As Control)
        frmMain.Controls.Remove(Control)
    End Sub

End Module
