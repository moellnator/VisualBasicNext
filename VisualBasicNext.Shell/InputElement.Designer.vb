<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class InputElement
    Inherits VisualBasicNext.Shell.ReadOnlyElement

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
        Me.TimerCompile = New System.Windows.Forms.Timer(Me.components)
        Me.SuspendLayout()
        '
        'TimerCompile
        '
        Me.TimerCompile.Enabled = True
        Me.TimerCompile.Interval = 500
        '
        'InputElement
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(62, Byte), Integer), CType(CType(62, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.Controls.Add(Me.PanelText)
        Me.Controls.Add(Me.PanelLineNumbers)
        Me.Controls.Add(Me.PanelScroll)
        Me.Controls.Add(Me.PanelFrame)
        Me.DoubleBuffered = True
        Me.Font = New System.Drawing.Font("Consolas", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Name = "InputElement"
        Me.Size = New System.Drawing.Size(675, 173)
        Me.PanelLineNumbers.ResumeLayout(False)
        Me.PanelText.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents TimerCompile As Timer

End Class
