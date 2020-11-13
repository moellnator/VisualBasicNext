Imports System.ComponentModel

Public Class ControlButton

    Private _old_backcolor As Color = Me.BackColor
    Private _old_forecolor As Color = Me.ForeColor

    Public Property BackColorHovered As Color = Me.BackColor
    Public Property ForeColorHovered As Color = Me.ForeColor
    Public Property BackColorClicked As Color = Me.BackColor
    Public Property ForeColorClicked As Color = Me.ForeColor

    Private _text As String
    <Browsable(True)>
    Public Overrides Property Text As String
        Get
            Return Me._text
        End Get
        Set(value As String)
            Me._text = value.First
            Me.Invalidate()
        End Set
    End Property

    Private Sub LabelText_MouseEnter(sender As Object, e As EventArgs) Handles Me.MouseEnter
        Me._old_backcolor = Me.BackColor
        Me.BackColor = Me.BackColorHovered
        Me._old_forecolor = Me.ForeColor
        Me.ForeColor = Me.ForeColorHovered
        Me.Invalidate()
    End Sub

    Private Sub LabelText_MouseLeave(sender As Object, e As EventArgs) Handles Me.MouseLeave
        Me.BackColor = Me._old_backcolor
        Me.ForeColor = Me._old_forecolor
        Me.Invalidate()
    End Sub

    Private Sub LabelText_MouseUp(sender As Object, e As MouseEventArgs) Handles Me.MouseUp
        Me.BackColor = Me.BackColorHovered
        Me.ForeColor = Me.ForeColorHovered
        Me.Invalidate()
    End Sub

    Private Sub LabelText_MouseDown(sender As Object, e As MouseEventArgs) Handles Me.MouseDown
        Me.BackColor = Me.BackColorClicked
        Me.ForeColor = Me.ForeColorClicked
        Me.Invalidate()
    End Sub

    Private Sub ControlButton_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint
        Dim g As Graphics = e.Graphics
        Dim size As SizeF = g.MeasureString(Me.Text, Me.Font)
        g.DrawString(Me.Text, Me.Font, New SolidBrush(Me.ForeColor), New PointF((Me.Width - size.Width) / 2, (Me.Height - size.Height) / 2))
    End Sub

End Class
