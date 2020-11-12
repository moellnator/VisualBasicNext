<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class MainForm
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
        Me.FlowLayoutPanel = New System.Windows.Forms.FlowLayoutPanel()
        Me.InputElement = New VisualBasicNext.Shell.InputElement()
        Me.FlowLayoutPanel.SuspendLayout()
        Me.SuspendLayout()
        '
        'FlowLayoutPanel
        '
        Me.FlowLayoutPanel.AutoScroll = True
        Me.FlowLayoutPanel.Controls.Add(Me.InputElement)
        Me.FlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.FlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown
        Me.FlowLayoutPanel.Font = New System.Drawing.Font("Consolas", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FlowLayoutPanel.Location = New System.Drawing.Point(0, 0)
        Me.FlowLayoutPanel.Name = "FlowLayoutPanel"
        Me.FlowLayoutPanel.Padding = New System.Windows.Forms.Padding(5, 5, 0, 0)
        Me.FlowLayoutPanel.Size = New System.Drawing.Size(821, 450)
        Me.FlowLayoutPanel.TabIndex = 2
        Me.FlowLayoutPanel.WrapContents = False
        '
        'InputElement
        '
        Me.InputElement.BackColor = System.Drawing.Color.FromArgb(CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer), CType(CType(30, Byte), Integer))
        Me.InputElement.Font = New System.Drawing.Font("Consolas", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.InputElement.ForeColor = System.Drawing.Color.FromArgb(CType(CType(220, Byte), Integer), CType(CType(220, Byte), Integer), CType(CType(220, Byte), Integer))
        Me.InputElement.Location = New System.Drawing.Point(8, 8)
        Me.InputElement.Name = "InputElement"
        Me.InputElement.Size = New System.Drawing.Size(792, 56)
        Me.InputElement.TabIndex = 0
        '
        'MainForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(45, Byte), Integer), CType(CType(45, Byte), Integer), CType(CType(48, Byte), Integer))
        Me.ClientSize = New System.Drawing.Size(821, 450)
        Me.Controls.Add(Me.FlowLayoutPanel)
        Me.Name = "MainForm"
        Me.Text = "Form1"
        Me.FlowLayoutPanel.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents FlowLayoutPanel As FlowLayoutPanel
    Friend WithEvents InputElement As InputElement
End Class
