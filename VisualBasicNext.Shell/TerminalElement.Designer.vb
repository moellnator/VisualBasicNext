<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class TerminalElement
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
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
        Me.components = New System.ComponentModel.Container()
        Me.FlowLayoutPanel = New System.Windows.Forms.FlowLayoutPanel()
        Me.InputElement = New VisualBasicNext.Shell.InputElement()
        Me.PanelContent = New System.Windows.Forms.Panel()
        Me.PanelScroll = New System.Windows.Forms.Panel()
        Me.TimerUpdate = New System.Windows.Forms.Timer(Me.components)
        Me.FlowLayoutPanel.SuspendLayout()
        Me.PanelContent.SuspendLayout()
        Me.SuspendLayout()
        '
        'FlowLayoutPanel
        '
        Me.FlowLayoutPanel.AutoSize = True
        Me.FlowLayoutPanel.BackColor = System.Drawing.Color.Transparent
        Me.FlowLayoutPanel.Controls.Add(Me.InputElement)
        Me.FlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top
        Me.FlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown
        Me.FlowLayoutPanel.Font = New System.Drawing.Font("Consolas", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FlowLayoutPanel.Location = New System.Drawing.Point(0, 0)
        Me.FlowLayoutPanel.Margin = New System.Windows.Forms.Padding(0)
        Me.FlowLayoutPanel.Name = "FlowLayoutPanel"
        Me.FlowLayoutPanel.Size = New System.Drawing.Size(520, 56)
        Me.FlowLayoutPanel.TabIndex = 3
        Me.FlowLayoutPanel.WrapContents = False
        '
        'InputElement
        '
        Me.InputElement.BackColor = System.Drawing.Color.FromArgb(CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer))
        Me.InputElement.Font = New System.Drawing.Font("Consolas", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.InputElement.ForeColor = System.Drawing.Color.FromArgb(CType(CType(220, Byte), Integer), CType(CType(220, Byte), Integer), CType(CType(220, Byte), Integer))
        Me.InputElement.Location = New System.Drawing.Point(0, 0)
        Me.InputElement.Margin = New System.Windows.Forms.Padding(0)
        Me.InputElement.Name = "InputElement"
        Me.InputElement.Size = New System.Drawing.Size(535, 56)
        Me.InputElement.TabIndex = 1
        '
        'PanelContent
        '
        Me.PanelContent.BackColor = System.Drawing.Color.FromArgb(CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer))
        Me.PanelContent.Controls.Add(Me.FlowLayoutPanel)
        Me.PanelContent.Controls.Add(Me.PanelScroll)
        Me.PanelContent.Dock = System.Windows.Forms.DockStyle.Fill
        Me.PanelContent.Location = New System.Drawing.Point(5, 0)
        Me.PanelContent.Name = "PanelContent"
        Me.PanelContent.Size = New System.Drawing.Size(536, 409)
        Me.PanelContent.TabIndex = 4
        '
        'PanelScroll
        '
        Me.PanelScroll.BackColor = System.Drawing.Color.FromArgb(CType(CType(62, Byte), Integer), CType(CType(62, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.PanelScroll.Dock = System.Windows.Forms.DockStyle.Right
        Me.PanelScroll.Location = New System.Drawing.Point(520, 0)
        Me.PanelScroll.Name = "PanelScroll"
        Me.PanelScroll.Size = New System.Drawing.Size(16, 409)
        Me.PanelScroll.TabIndex = 0
        '
        'TimerUpdate
        '
        '
        'TerminalElement
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.PanelContent)
        Me.Name = "TerminalElement"
        Me.Padding = New System.Windows.Forms.Padding(5, 0, 5, 5)
        Me.Size = New System.Drawing.Size(546, 414)
        Me.FlowLayoutPanel.ResumeLayout(False)
        Me.PanelContent.ResumeLayout(False)
        Me.PanelContent.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents FlowLayoutPanel As FlowLayoutPanel
    Friend WithEvents PanelContent As Panel
    Friend WithEvents InputElement As InputElement
    Friend WithEvents PanelScroll As Panel
    Friend WithEvents TimerUpdate As Timer
End Class
