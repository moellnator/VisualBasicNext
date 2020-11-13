<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class MainForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.PanelHead = New System.Windows.Forms.Panel()
        Me.PanelTitle = New System.Windows.Forms.Panel()
        Me.ControlButtonMinimize = New VisualBasicNext.Shell.ControlButton()
        Me.ControlButtonClose = New VisualBasicNext.Shell.ControlButton()
        Me.LabelTitle = New System.Windows.Forms.Label()
        Me.TerminalElement = New VisualBasicNext.Shell.TerminalElement()
        Me.PanelHead.SuspendLayout()
        Me.PanelTitle.SuspendLayout()
        Me.SuspendLayout()
        '
        'PanelHead
        '
        Me.PanelHead.Controls.Add(Me.PanelTitle)
        Me.PanelHead.Dock = System.Windows.Forms.DockStyle.Top
        Me.PanelHead.Location = New System.Drawing.Point(1, 1)
        Me.PanelHead.Name = "PanelHead"
        Me.PanelHead.Size = New System.Drawing.Size(798, 48)
        Me.PanelHead.TabIndex = 1
        '
        'PanelTitle
        '
        Me.PanelTitle.Controls.Add(Me.ControlButtonMinimize)
        Me.PanelTitle.Controls.Add(Me.ControlButtonClose)
        Me.PanelTitle.Controls.Add(Me.LabelTitle)
        Me.PanelTitle.Dock = System.Windows.Forms.DockStyle.Top
        Me.PanelTitle.Location = New System.Drawing.Point(0, 0)
        Me.PanelTitle.Name = "PanelTitle"
        Me.PanelTitle.Size = New System.Drawing.Size(798, 24)
        Me.PanelTitle.TabIndex = 0
        '
        'ControlButtonMinimize
        '
        Me.ControlButtonMinimize.BackColorClicked = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(122, Byte), Integer), CType(CType(204, Byte), Integer))
        Me.ControlButtonMinimize.BackColorHovered = System.Drawing.Color.FromArgb(CType(CType(63, Byte), Integer), CType(CType(63, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.ControlButtonMinimize.Dock = System.Windows.Forms.DockStyle.Right
        Me.ControlButtonMinimize.Font = New System.Drawing.Font("Segoe MDL2 Assets", 6.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ControlButtonMinimize.ForeColor = System.Drawing.Color.White
        Me.ControlButtonMinimize.ForeColorClicked = System.Drawing.Color.White
        Me.ControlButtonMinimize.ForeColorHovered = System.Drawing.Color.White
        Me.ControlButtonMinimize.Location = New System.Drawing.Point(734, 0)
        Me.ControlButtonMinimize.Margin = New System.Windows.Forms.Padding(2, 2, 2, 2)
        Me.ControlButtonMinimize.Name = "ControlButtonMinimize"
        Me.ControlButtonMinimize.Size = New System.Drawing.Size(32, 24)
        Me.ControlButtonMinimize.TabIndex = 2
        '
        'ControlButtonClose
        '
        Me.ControlButtonClose.BackColorClicked = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(122, Byte), Integer), CType(CType(204, Byte), Integer))
        Me.ControlButtonClose.BackColorHovered = System.Drawing.Color.FromArgb(CType(CType(63, Byte), Integer), CType(CType(63, Byte), Integer), CType(CType(65, Byte), Integer))
        Me.ControlButtonClose.Dock = System.Windows.Forms.DockStyle.Right
        Me.ControlButtonClose.Font = New System.Drawing.Font("Segoe MDL2 Assets", 6.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ControlButtonClose.ForeColor = System.Drawing.Color.White
        Me.ControlButtonClose.ForeColorClicked = System.Drawing.Color.White
        Me.ControlButtonClose.ForeColorHovered = System.Drawing.Color.White
        Me.ControlButtonClose.Location = New System.Drawing.Point(766, 0)
        Me.ControlButtonClose.Margin = New System.Windows.Forms.Padding(2, 2, 2, 2)
        Me.ControlButtonClose.Name = "ControlButtonClose"
        Me.ControlButtonClose.Size = New System.Drawing.Size(32, 24)
        Me.ControlButtonClose.TabIndex = 1
        '
        'LabelTitle
        '
        Me.LabelTitle.AutoSize = True
        Me.LabelTitle.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LabelTitle.ForeColor = System.Drawing.Color.FromArgb(CType(CType(129, Byte), Integer), CType(CType(129, Byte), Integer), CType(CType(131, Byte), Integer))
        Me.LabelTitle.Location = New System.Drawing.Point(6, 5)
        Me.LabelTitle.Name = "LabelTitle"
        Me.LabelTitle.Size = New System.Drawing.Size(212, 15)
        Me.LabelTitle.TabIndex = 0
        Me.LabelTitle.Text = "Visual Basic .NET Interactive - Terminal "
        '
        'TerminalElement
        '
        Me.TerminalElement.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TerminalElement.Location = New System.Drawing.Point(1, 49)
        Me.TerminalElement.Name = "TerminalElement"
        Me.TerminalElement.Padding = New System.Windows.Forms.Padding(5, 0, 5, 5)
        Me.TerminalElement.Size = New System.Drawing.Size(798, 400)
        Me.TerminalElement.TabIndex = 2
        '
        'MainForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(45, Byte), Integer), CType(CType(45, Byte), Integer), CType(CType(48, Byte), Integer))
        Me.ClientSize = New System.Drawing.Size(800, 450)
        Me.ControlBox = False
        Me.Controls.Add(Me.TerminalElement)
        Me.Controls.Add(Me.PanelHead)
        Me.DoubleBuffered = True
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.MinimizeBox = False
        Me.Name = "MainForm"
        Me.Padding = New System.Windows.Forms.Padding(1)
        Me.ShowIcon = False
        Me.PanelHead.ResumeLayout(False)
        Me.PanelTitle.ResumeLayout(False)
        Me.PanelTitle.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents PanelHead As Panel
    Friend WithEvents PanelTitle As Panel
    Friend WithEvents LabelTitle As Label
    Friend WithEvents ControlButtonClose As ControlButton
    Friend WithEvents ControlButtonMinimize As ControlButton
    Friend WithEvents TerminalElement As TerminalElement
End Class
