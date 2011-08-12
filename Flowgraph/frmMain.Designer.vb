<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmMain
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.lblToolTip = New System.Windows.Forms.Label()
        Me.MenuStrip1 = New System.Windows.Forms.MenuStrip()
        Me.menuFile = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuNew = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuOpen = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuSave = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuSaveAs = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuExit = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuView = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuDisableUI = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuSimpleLines = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuHelp = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuBasicUse = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuAbout = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuSource = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'lblToolTip
        '
        Me.lblToolTip.AutoSize = True
        Me.lblToolTip.BackColor = System.Drawing.SystemColors.Info
        Me.lblToolTip.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblToolTip.CausesValidation = False
        Me.lblToolTip.ForeColor = System.Drawing.SystemColors.InfoText
        Me.lblToolTip.Location = New System.Drawing.Point(0, 24)
        Me.lblToolTip.Name = "lblToolTip"
        Me.lblToolTip.Size = New System.Drawing.Size(45, 15)
        Me.lblToolTip.TabIndex = 3
        Me.lblToolTip.Text = "ToolTip"
        Me.lblToolTip.UseMnemonic = False
        Me.lblToolTip.Visible = False
        '
        'MenuStrip1
        '
        Me.MenuStrip1.BackColor = System.Drawing.Color.FromArgb(CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer))
        Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.menuFile, Me.menuView, Me.menuHelp})
        Me.MenuStrip1.Location = New System.Drawing.Point(0, 0)
        Me.MenuStrip1.Name = "MenuStrip1"
        Me.MenuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional
        Me.MenuStrip1.Size = New System.Drawing.Size(485, 24)
        Me.MenuStrip1.TabIndex = 5
        Me.MenuStrip1.Text = "MenuStrip1"
        '
        'menuFile
        '
        Me.menuFile.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.menuNew, Me.menuOpen, Me.menuSave, Me.menuSaveAs, Me.menuExit})
        Me.menuFile.Name = "menuFile"
        Me.menuFile.Size = New System.Drawing.Size(35, 20)
        Me.menuFile.Text = "&File"
        '
        'menuNew
        '
        Me.menuNew.Name = "menuNew"
        Me.menuNew.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.N), System.Windows.Forms.Keys)
        Me.menuNew.Size = New System.Drawing.Size(139, 22)
        Me.menuNew.Text = "&New"
        '
        'menuOpen
        '
        Me.menuOpen.Name = "menuOpen"
        Me.menuOpen.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.O), System.Windows.Forms.Keys)
        Me.menuOpen.Size = New System.Drawing.Size(139, 22)
        Me.menuOpen.Text = "&Open"
        '
        'menuSave
        '
        Me.menuSave.Name = "menuSave"
        Me.menuSave.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.S), System.Windows.Forms.Keys)
        Me.menuSave.Size = New System.Drawing.Size(139, 22)
        Me.menuSave.Text = "&Save"
        '
        'menuSaveAs
        '
        Me.menuSaveAs.Name = "menuSaveAs"
        Me.menuSaveAs.Size = New System.Drawing.Size(139, 22)
        Me.menuSaveAs.Text = "Save As"
        '
        'menuExit
        '
        Me.menuExit.Name = "menuExit"
        Me.menuExit.ShortcutKeys = CType((System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.F4), System.Windows.Forms.Keys)
        Me.menuExit.Size = New System.Drawing.Size(139, 22)
        Me.menuExit.Text = "Exit"
        '
        'menuView
        '
        Me.menuView.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.menuDisableUI, Me.menuSimpleLines, Me.mnuSource})
        Me.menuView.Name = "menuView"
        Me.menuView.Size = New System.Drawing.Size(42, 20)
        Me.menuView.Text = "&View"
        '
        'menuDisableUI
        '
        Me.menuDisableUI.Name = "menuDisableUI"
        Me.menuDisableUI.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.D), System.Windows.Forms.Keys)
        Me.menuDisableUI.Size = New System.Drawing.Size(170, 22)
        Me.menuDisableUI.Text = "Disable UI"
        '
        'menuSimpleLines
        '
        Me.menuSimpleLines.Name = "menuSimpleLines"
        Me.menuSimpleLines.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.L), System.Windows.Forms.Keys)
        Me.menuSimpleLines.Size = New System.Drawing.Size(170, 22)
        Me.menuSimpleLines.Text = "Simple lines"
        '
        'menuHelp
        '
        Me.menuHelp.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.menuBasicUse, Me.menuAbout})
        Me.menuHelp.Name = "menuHelp"
        Me.menuHelp.Size = New System.Drawing.Size(41, 20)
        Me.menuHelp.Text = "&Help"
        '
        'menuBasicUse
        '
        Me.menuBasicUse.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Underline)
        Me.menuBasicUse.ForeColor = System.Drawing.Color.Blue
        Me.menuBasicUse.Name = "menuBasicUse"
        Me.menuBasicUse.ShortcutKeys = System.Windows.Forms.Keys.F1
        Me.menuBasicUse.Size = New System.Drawing.Size(142, 22)
        Me.menuBasicUse.Text = "Basic use"
        '
        'menuAbout
        '
        Me.menuAbout.Name = "menuAbout"
        Me.menuAbout.Size = New System.Drawing.Size(142, 22)
        Me.menuAbout.Text = "&About"
        '
        'mnuSource
        '
        Me.mnuSource.Name = "mnuSource"
        Me.mnuSource.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.U), System.Windows.Forms.Keys)
        Me.mnuSource.Size = New System.Drawing.Size(170, 22)
        Me.mnuSource.Text = "Source (fgs)"
        '
        'frmMain
        '
        Me.AllowDrop = True
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(485, 410)
        Me.Controls.Add(Me.lblToolTip)
        Me.Controls.Add(Me.MenuStrip1)
        Me.DoubleBuffered = True
        Me.MainMenuStrip = Me.MenuStrip1
        Me.MinimumSize = New System.Drawing.Size(280, 50)
        Me.Name = "frmMain"
        Me.Text = "Flowgraph"
        Me.MenuStrip1.ResumeLayout(False)
        Me.MenuStrip1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents lblToolTip As System.Windows.Forms.Label
    Friend WithEvents MenuStrip1 As System.Windows.Forms.MenuStrip
    Friend WithEvents menuFile As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents menuNew As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents menuOpen As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents menuSave As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents menuSaveAs As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents menuExit As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents menuView As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents menuDisableUI As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents menuSimpleLines As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents menuHelp As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents menuBasicUse As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents menuAbout As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuSource As System.Windows.Forms.ToolStripMenuItem

End Class
