<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ReadOnlyElement
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.PanelLineNumbers = New System.Windows.Forms.Panel()
        Me.PanelText = New System.Windows.Forms.Panel()
        Me.PanelTextContent = New System.Windows.Forms.Panel()
        Me.PanelScroll = New System.Windows.Forms.Panel()
        Me.PanelFrame = New System.Windows.Forms.Panel()
        Me.PanelText.SuspendLayout()
        Me.SuspendLayout()
        '
        'PanelLineNumbers
        '
        Me.PanelLineNumbers.Dock = System.Windows.Forms.DockStyle.Left
        Me.PanelLineNumbers.Location = New System.Drawing.Point(2, 0)
        Me.PanelLineNumbers.Name = "PanelLineNumbers"
        Me.PanelLineNumbers.Size = New System.Drawing.Size(57, 157)
        Me.PanelLineNumbers.TabIndex = 0
        '
        'PanelText
        '
        Me.PanelText.BackColor = System.Drawing.SystemColors.Control
        Me.PanelText.Controls.Add(Me.PanelTextContent)
        Me.PanelText.Dock = System.Windows.Forms.DockStyle.Fill
        Me.PanelText.Location = New System.Drawing.Point(59, 0)
        Me.PanelText.Name = "PanelText"
        Me.PanelText.Size = New System.Drawing.Size(616, 157)
        Me.PanelText.TabIndex = 1
        '
        'PanelTextContent
        '
        Me.PanelTextContent.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.PanelTextContent.Location = New System.Drawing.Point(0, 0)
        Me.PanelTextContent.Name = "PanelTextContent"
        Me.PanelTextContent.Size = New System.Drawing.Size(562, 157)
        Me.PanelTextContent.TabIndex = 0
        '
        'PanelScroll
        '
        Me.PanelScroll.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.PanelScroll.Location = New System.Drawing.Point(2, 157)
        Me.PanelScroll.Name = "PanelScroll"
        Me.PanelScroll.Size = New System.Drawing.Size(673, 16)
        Me.PanelScroll.TabIndex = 0
        '
        'PanelFrame
        '
        Me.PanelFrame.Dock = System.Windows.Forms.DockStyle.Left
        Me.PanelFrame.Location = New System.Drawing.Point(0, 0)
        Me.PanelFrame.Name = "PanelFrame"
        Me.PanelFrame.Size = New System.Drawing.Size(2, 173)
        Me.PanelFrame.TabIndex = 0
        '
        'ReadOnlyElement
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(62, Byte), Integer), CType(CType(62, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.Controls.Add(Me.PanelText)
        Me.Controls.Add(Me.PanelLineNumbers)
        Me.Controls.Add(Me.PanelScroll)
        Me.Controls.Add(Me.PanelFrame)
        Me.DoubleBuffered = True
        Me.Font = New System.Drawing.Font("Consolas", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Margin = New System.Windows.Forms.Padding(0)
        Me.Name = "ReadOnlyElement"
        Me.Size = New System.Drawing.Size(675, 173)
        Me.PanelText.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents PanelLineNumbers As Panel
    Friend WithEvents PanelText As Panel
    Friend WithEvents PanelTextContent As Panel
    Friend WithEvents PanelScroll As Panel
    Friend WithEvents PanelFrame As Panel

End Class
