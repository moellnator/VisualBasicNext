Imports System.Reflection

Public Class ReadOnlyElement

    Protected Shared ReadOnly _Padding As New Padding(5)
    Protected Shared ReadOnly _LineSeparation As Single = 0.15
    Protected Shared ReadOnly _CharRatio As Single = 9.5 / 17
    Protected Shared ReadOnly _LineNumber_BG_Color As New SolidBrush(ColorPalette.ColorBackground)
    Protected Shared ReadOnly _LineNumber_FG_Color As New SolidBrush(ColorPalette.ColorLinenumber)

    Protected _Text As FormattedText = FormattedText.Empty

    Private _suppress_resize As Boolean = False

    Private _scroll_location As Single = 0
    Private _scroll_start As Integer = 0
    Private _scroll_down As Boolean = False
    Private _scroll_size As Single
    Private _scroll_is_hover_left As Boolean
    Private _scroll_is_hover_right As Boolean
    Private _scroll_is_hover_bar As Boolean

    Private _highlight As Color = ColorPalette.ColorBackground
    Protected _hide_linenumber As Boolean = False
    Protected _line_overhead As Integer = 1
    Protected _small_scroll As Boolean = True

    Public Sub SetHightlight(color As Color)
        Me._highlight = color
        Me.PanelLineNumbers.Invalidate()
    End Sub

    Public Sub RemoveLineOverhead()
        Me._line_overhead = 0
        Me.AutoSizeElement()
    End Sub

    Public Sub RemoveLineNumbers()
        Me._hide_linenumber = True
        Me.PanelLineNumbers.Invalidate()
    End Sub

    Public Sub New()
        InitializeComponent()
        Me.BackColor = ColorPalette.ColorBackground
        Me.ForeColor = ColorPalette.ColorPlainText
        Me.PanelText.BackColor = Me.BackColor
        Me.PanelTextContent.BackColor = Me.BackColor
        Me.PanelScroll.BackColor = ColorPalette.ColorFrame
        Me.PanelFrame.BackColor = ColorPalette.ColorFrame
        Me.Font = New Font("Consolas", 9.75!, FontStyle.Regular, GraphicsUnit.Point, 0)
        MakeDoubleBuffered(Me.PanelLineNumbers)
        MakeDoubleBuffered(Me.PanelTextContent)
        MakeDoubleBuffered(Me.PanelScroll)
    End Sub

    Public Sub SetText(text As FormattedText)
        Me._Text = text
        Me.AutoSizeElement()
        Me.Invalidate(True)
    End Sub

    Private Shared Sub MakeDoubleBuffered(control As Panel)
        Dim type As Type = GetType(Panel)
        type.InvokeMember(
            "DoubleBuffered",
            BindingFlags.SetProperty Or BindingFlags.Instance Or BindingFlags.NonPublic,
            Nothing,
            control,
            New Object() {True}
        )
    End Sub

    Protected Property ScrollLocation As Single
        Get
            Return Me._scroll_location
        End Get
        Set(value As Single)
            Dim scroll_max As Integer = Me.PanelTextContent.Width - Me.PanelText.Width
            Me._scroll_location = Math.Max(0, Math.Min(scroll_max, value))
            Me.PanelTextContent.Left = -Me._scroll_location
            Me.PanelScroll.Invalidate()
        End Set
    End Property

    Private Sub InputElement_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        If Not Me._suppress_resize Then Me.AutoSizeElement()
        Me._suppress_resize = False
    End Sub

    Protected Sub AutoSizeElement()
        Me._suppress_resize = True
        Dim text_size As New SizeF(Me.Font.SizeInPoints / 72 * 96 * _CharRatio, Me.Font.SizeInPoints / 72 * 96)
        Dim max_width As Integer = text_size.Width * (3 + Me._Text.MaxPos) + _Padding.Left
        If Me._small_scroll And Not Me._IsVerticalScrolling Then
            Me.PanelScroll.Height = 2
        Else
            Me.PanelScroll.Height = 16
        End If
        Me.PanelTextContent.Width = Math.Max(max_width, Me.PanelText.Width)
        Me.PanelTextContent.Height = Me.PanelText.Height
        Me.Height = text_size.Height * (1 + _LineSeparation) * (Math.Max(0, Me._Text.MaxLines) + 1 + Me._line_overhead) + _Padding.Vertical + Me.PanelScroll.Height
        Me.PanelLineNumbers.Width = 4 * text_size.Width + _Padding.Horizontal
        Me.Invalidate(True)
    End Sub

    Private Sub PanelTextContent_Paint(sender As Object, e As PaintEventArgs) Handles PanelTextContent.Paint
        Dim g As Graphics = e.Graphics
        Dim r As Rectangle = Me.PanelTextContent.ClientRectangle
        g.Clear(Me.BackColor)
        g.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit
        g.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
        g.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
        Dim text_size As SizeF = New SizeF(Me.Font.SizeInPoints / 72 * g.DpiX * _CharRatio, Me.Font.SizeInPoints / 72 * g.DpiY * (1 + _LineSeparation))
        Me._PrintText(r, g, text_size)
        Me.PaintTextContent(r, g, text_size)
    End Sub

    Protected Overridable Sub PaintTextContent(r As Rectangle, g As Graphics, textSize As SizeF)
    End Sub

    Private Sub PanelLineNumbers_Paint(sender As Object, e As PaintEventArgs) Handles PanelLineNumbers.Paint
        Dim g As Graphics = e.Graphics
        Dim r As Rectangle = Me.ClientRectangle
        g.Clear(Me.BackColor)
        g.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit
        g.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
        g.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
        Dim text_size As SizeF = New SizeF(Me.Font.SizeInPoints / 72 * g.DpiX * _CharRatio, Me.Font.SizeInPoints / 72 * g.DpiY * (1 + _LineSeparation))
        Dim offset As Single = Me._PrintLineNumbers(r, g, text_size)
    End Sub

    Private Sub PanelScroll_Paint(sender As Object, e As PaintEventArgs) Handles PanelScroll.Paint
        Dim g As Graphics = e.Graphics
        Dim r As Rectangle = Me.PanelScroll.ClientRectangle
        g.Clear(Me.PanelScroll.BackColor)
        g.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit
        g.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
        g.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
        Me._PrintScrollbar(r, g)
    End Sub

    Private Sub _PrintScrollbar(r As Rectangle, g As Graphics)
        If Me.PanelTextContent.Width > Me.PanelText.Width Then
            Dim button_offset As Integer = 20
            Dim scroll_size As Single = (Me.PanelTextContent.Width - Me.PanelText.Width)
            Dim scroll_rect As New RectangleF
            If scroll_size >= Me.PanelScroll.Width - 2 * button_offset - 8 Then
                Dim offset As Single = Me._scroll_location / scroll_size
                Me._scroll_size = 8
                scroll_rect = New RectangleF(button_offset + offset * (Me.PanelScroll.Width - 2 * button_offset - 8), 4, 8, 8)
            Else
                Me._scroll_size = Me.PanelScroll.Width - 2 * button_offset - scroll_size
                scroll_rect = New RectangleF(button_offset + Me._scroll_location, 4, Me._scroll_size, 8)
            End If
            g.FillRectangle(New SolidBrush(ColorPalette.ColorScrollbar), scroll_rect)
            g.FillPolygon(
                New SolidBrush(If(Me._scroll_is_hover_left, ColorPalette.ColorHighlight, ColorPalette.ColorOperator)),
                {
                    New Point(button_offset / 2 - 2, 8),
                    New Point(button_offset / 2 + 2, 4),
                    New Point(button_offset / 2 + 2, 12)
                }
            )
            g.FillPolygon(
                New SolidBrush(If(Me._scroll_is_hover_right, ColorPalette.ColorHighlight, ColorPalette.ColorOperator)),
                {
                    New Point(r.Width - button_offset / 2 + 2, 8),
                    New Point(r.Width - button_offset / 2 - 2, 4),
                    New Point(r.Width - button_offset / 2 - 2, 12)
                }
            )
        End If
    End Sub

    Private Function _PrintLineNumbers(r As Rectangle, g As Graphics, textSize As SizeF) As Single
        Dim width As Single = textSize.Width * 4 + _Padding.Horizontal
        g.FillRectangle(_LineNumber_BG_Color, New Rectangle(r.Left, r.Top, width, r.Height))
        If Not Me._hide_linenumber Then
            Dim padding_left As Single = r.Left + _Padding.Horizontal / 2 + textSize.Width / 2
            For i = 0 To Math.Max(0, Me._Text.MaxLines)
                g.DrawString(
                        i.ToString.PadLeft(3, " "c),
                        Me.Font,
                        _LineNumber_FG_Color,
                        New PointF(padding_left, i * textSize.Height + _Padding.Top)
                    )
            Next
        End If
        g.FillRectangle(New SolidBrush(Me._highlight), New Rectangle(0, _Padding.Top, _Padding.Left, (Me._Text.MaxLines + 1) * textSize.Height))
        Return width
    End Function

    Private Sub _PrintText(r As Rectangle, g As Graphics, textSize As SizeF)
        For Each c As FormattedChar In Me._Text
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

    Private Sub PanelScroll_MouseMove(sender As Object, e As MouseEventArgs) Handles PanelScroll.MouseMove
        If Me._IsVerticalScrolling Then
            If Me._scroll_down Then
                Dim delta As Integer = e.Location.X - Me._scroll_start
                Me._scroll_start = e.Location.X
                Dim max As Integer = (Me.PanelScroll.Width - 40) - Me._scroll_size
                Dim scroll_max As Integer = Me.PanelTextContent.Width - Me.PanelText.Width
                Me.ScrollLocation = Me._scroll_location + delta / max * scroll_max
            Else
                If e.Location.X < 20 Then
                    Me._scroll_is_hover_left = True
                    Me.PanelScroll.Invalidate()
                ElseIf e.Location.X > Me.PanelScroll.Width - 20 Then
                    Me._scroll_is_hover_right = True
                    Me.PanelScroll.Invalidate()
                Else
                    Me._scroll_is_hover_right = False
                    Me._scroll_is_hover_right = False
                    Me._scroll_is_hover_bar = True
                    Me.PanelScroll.Invalidate()
                End If
            End If
        End If
    End Sub

    Private Sub PanelScroll_MouseDown(sender As Object, e As MouseEventArgs) Handles PanelScroll.MouseDown
        If Me._IsVerticalScrolling Then
            If e.Location.X > 20 And e.Location.X < Me.PanelScroll.Width - 20 Then
                Me._scroll_start = e.Location.X
                Me._scroll_down = True
            End If
        End If
    End Sub

    Private Sub PanelScroll_MouseUp(sender As Object, e As MouseEventArgs) Handles PanelScroll.MouseUp
        Me._scroll_down = False
    End Sub

    Private Sub PanelScroll_MouseClick(sender As Object, e As MouseEventArgs) Handles PanelScroll.MouseClick
        If Me._IsVerticalScrolling Then
            Dim scroll_max As Integer = Me.PanelTextContent.Width - Me.PanelText.Width
            If e.Location.X < 20 Then
                Me.ScrollLocation = Me._scroll_location - 0.05 * scroll_max
            ElseIf e.Location.X > Me.PanelScroll.Width - 20 Then
                Me.ScrollLocation = Me._scroll_location + 0.05 * scroll_max
            End If
        End If
    End Sub

    Private Sub PanelScroll_MouseLeave(sender As Object, e As EventArgs) Handles PanelScroll.MouseLeave
        Me._scroll_is_hover_bar = False
        Me._scroll_is_hover_left = False
        Me._scroll_is_hover_right = False
        Me.PanelScroll.Invalidate()
    End Sub

    Private Function _IsVerticalScrolling() As Boolean
        Return Me.PanelTextContent.Width > Me.PanelText.Width
    End Function

End Class
