Public Class ReadOnlyElement

    Private Shared ReadOnly _Padding As New Padding(5)
    Private Shared ReadOnly _LineSeparation As Single = 0.15
    Private Shared ReadOnly _CharRatio As Single = 9.5 / 17

    Private Shared ReadOnly _LineNumber_BG_Color As New SolidBrush(ColorPalette.ColorBackground)
    Private Shared ReadOnly _LineNumber_FG_Color As New SolidBrush(ColorPalette.ColorLinenumber)

    Private _text As FormattedText = FormattedText.Empty
    Private _suppress_resize As Boolean = False

    Public Sub New()
        InitializeComponent()
        Me.BackColor = ColorPalette.ColorBackground
        Me.ForeColor = ColorPalette.ColorPlainText
        Me.Font = New Font("Consolas", 9.75!, FontStyle.Regular, GraphicsUnit.Point, 0)
    End Sub

    Public Property FormattedText As FormattedText
        Get
            Return Me._text
        End Get
        Set(value As FormattedText)
            Me._text = value
            Me.InputElement_Resize(Me, EventArgs.Empty)
        End Set
    End Property

    Private Sub InputElement_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        If Not Me._suppress_resize Then Me.Height = Me.Font.SizeInPoints / 72 * 96 * (1 + _LineSeparation) * (Me._text.MaxLines + 1) + _Padding.Vertical
        Me.Invalidate()
        Me._suppress_resize = False
    End Sub

    Private Sub InputElement_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint
        Dim g As Graphics = e.Graphics
        Dim r As Rectangle = Me.ClientRectangle
        g.Clear(Me.BackColor)
        g.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit
        g.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
        g.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
        Dim text_size As SizeF = New SizeF(Me.Font.SizeInPoints / 72 * g.DpiX * _CharRatio, Me.Font.SizeInPoints / 72 * g.DpiY * (1 + _LineSeparation))
        Dim offset As Single = Me._PrintLineNumbers(r, g, text_size)
        Dim text_rect As New Rectangle(offset, r.Top, r.Width - offset, r.Height)
        Me._PrintText(text_rect, g, text_size)
    End Sub

    Private Function _PrintLineNumbers(r As Rectangle, g As Graphics, textSize As SizeF) As Single
        Dim width As Single = textSize.Width * 4 + _Padding.Horizontal
        g.FillRectangle(_LineNumber_BG_Color, New Rectangle(r.Left, r.Top, width, r.Height))
        Dim padding_left As Single = r.Left + _Padding.Horizontal / 2 + textSize.Width / 2
        For i = 0 To Me._text.MaxLines - 1
            g.DrawString(
                        i.ToString.PadLeft(3, " "c),
                        Me.Font,
                        _LineNumber_FG_Color,
                        New PointF(padding_left, i * textSize.Height + _Padding.Top)
                    )
        Next
        Return width
    End Function

    Private Sub _PrintText(r As Rectangle, g As Graphics, textSize As SizeF)
        For Each c As FormattedChar In Me._text
            g.DrawString(
                    c.Glyph,
                    Me.Font,
                    c.Color,
                    New PointF(
                        c.Location.X * textSize.Width + _Padding.Left + r.Left,
                        c.Location.Y * textSize.Height + _Padding.Top + r.Top
                    )
                )
        Next
    End Sub

End Class
