Imports System.Reflection
Imports VisualBasicNext.CodeAnalysis
Imports VisualBasicNext.CodeAnalysis.Diagnostics
Imports VisualBasicNext.CodeAnalysis.Evaluating
Imports VisualBasicNext.CodeAnalysis.Lexing
Imports VisualBasicNext.CodeAnalysis.Parsing

Public Class InputElement

    'TODO -> Add History of submitted documents
    'TODO -> Add auto completition?
    'TODO -> Add auto indenting?

    Public Event SubmittedDocument As SubmittedDocumentEventHandler

    Private ReadOnly _document As New Document

    Private Shared ReadOnly _Padding As New Padding(5)
    Private Shared ReadOnly _LineSeparation As Single = 0.15
    Private Shared ReadOnly _CharRatio As Single = 9.5 / 17
    Private Shared ReadOnly _LineNumber_BG_Color As New SolidBrush(ColorPalette.ColorBackground)
    Private Shared ReadOnly _LineNumber_FG_Color As New SolidBrush(ColorPalette.ColorLinenumber)
    Private Shared ReadOnly _ErrorPen As Pen = _MakeUnderlineTexture()

    Private _sync As Object = New Object
    Private _text As FormattedText = FormattedText.Empty
    Private _suppress_resize As Boolean = False
    Private _previous As Compilation = Nothing
    Private _latest As Compilation
    Private _latest_diagnostics As New ErrorList
    Private _latest_scope As Binding.BoundGlobalScope
    Private _state As VMState = New VMState
    Private _last_text As String = ""
    Private _type_cache As New Dictionary(Of String, Type)

    Private _scroll_location As Single = 0
    Private _scroll_start As Integer = 0
    Private _scroll_down As Boolean = False
    Private _scroll_size As Single
    Private _scroll_is_hover_left As Boolean
    Private _scroll_is_hover_right As Boolean
    Private _scroll_is_hover_bar As Boolean

    Public Sub New()
        InitializeComponent()
        Me.BackColor = ColorPalette.ColorBackground
        Me.ForeColor = ColorPalette.ColorPlainText
        Me.PanelScroll.BackColor = ColorPalette.ColorFrame
        Me.PanelFrame.BackColor = ColorPalette.ColorFrame
        Me.Font = New Font("Consolas", 9.75!, FontStyle.Regular, GraphicsUnit.Point, 0)
        MakeDoubleBuffered(Me.PanelLineNumbers)
        MakeDoubleBuffered(Me.PanelTextContent)
        MakeDoubleBuffered(Me.PanelScroll)
        AddHandler Me._document.DocumentChanged, AddressOf Me._DocumentChangedHandler
        Me.TimerCompile.Enabled = True
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

    Private Sub InputElement_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        If Not Me._suppress_resize Then Me._AutoSizeElement()
        Me._suppress_resize = False
    End Sub

    Private Sub InputElement_Disposed(sender As Object, e As EventArgs) Handles Me.Disposed
        Me._document.Dispose()
    End Sub

    Private Sub _DocumentChangedHandler(sender As Object, e As EventArgs)
        Me._AutoSizeElement()
        Me._text = New FormattedText(Me._GetFormattedTextChars)
        Me._FocusCursor()
        Me.Invalidate(True)
    End Sub

    Private Sub _FocusCursor()
        Dim text_size As New SizeF(Me.Font.SizeInPoints / 72 * 96 * _CharRatio, Me.Font.SizeInPoints / 72 * 96)
        Dim cursor As New PointF(
            text_size.Width * Me._document.CursorPosition.X + _Padding.Left,
            text_size.Height * (Me._document.CursorPosition.Y + 1) + _Padding.Top
        )
        Dim right_edge As Integer = Me.PanelText.Width + Me._scroll_location
        Dim left_edge As Integer = Me._scroll_location
        If cursor.X + 3 * text_size.Width > right_edge Then
            Me._scroll_location = cursor.X + 3 * text_size.Width - Me.PanelText.Width
        ElseIf cursor.X < left_edge Then
            Me._scroll_location = cursor.X
        End If
        Me.PanelTextContent.Location = New Point(-Me._scroll_location, Me.PanelTextContent.Location.Y)
    End Sub

    Private Sub _AutoSizeElement()
        Me._suppress_resize = True
        Dim text_size As New SizeF(Me.Font.SizeInPoints / 72 * 96 * _CharRatio, Me.Font.SizeInPoints / 72 * 96)
        Dim max_width As Integer = text_size.Width * (3 + Me._document.Lines.Max(Function(l) l.Count)) + _Padding.Left
        Me.PanelTextContent.Width = Math.Max(max_width, Me.PanelText.Width)
        Me.PanelTextContent.Height = Me.PanelText.Height
        Me.Height = text_size.Height * (1 + _LineSeparation) * (Me._document.Lines.Count + 1) + _Padding.Vertical + 16
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
        Me._PrintDiagnostics(r, g, text_size)
        Me._PrintCursor(r, g, text_size)
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

    Private Shared Function _MakeUnderlineTexture() As Pen
        Dim b As New Drawing2D.HatchBrush(Drawing2D.HatchStyle.DarkDownwardDiagonal, ColorPalette.ColorSyntaxError, Color.Transparent)
        Return New Pen(b, 3.0F) With {.StartCap = Drawing2D.LineCap.Round, .EndCap = Drawing2D.LineCap.Round}
    End Function

    Private Sub _PrintDiagnostics(r As Rectangle, g As Graphics, textSize As SizeF)
        Dim diagnostics As ErrorList
        SyncLock Me._sync
            diagnostics = Me._latest_diagnostics
        End SyncLock
        For Each e As ErrorObject In diagnostics
            Dim span As Text.Span = e.Content
            Dim position As Text.Position = span.GetStartPosition
            Dim length As Integer = Math.Max(span.Length, 1)
            Dim offset As New Point(r.Left + _Padding.Left, _Padding.Top + r.Top)
            Dim startp As New Point((position.Offset + 0.2) * textSize.Width + offset.X, (position.LineNumber + 0.925) * textSize.Height + offset.Y)
            Dim endp As New Point(startp.X + length * textSize.Width, startp.Y)
            g.DrawLine(_ErrorPen, startp, endp)
        Next
    End Sub

    Private Function _PrintLineNumbers(r As Rectangle, g As Graphics, textSize As SizeF) As Single
        Dim width As Single = textSize.Width * 4 + _Padding.Horizontal
        g.FillRectangle(_LineNumber_BG_Color, New Rectangle(r.Left, r.Top, width, r.Height))
        Dim padding_left As Single = r.Left + _Padding.Horizontal / 2 + textSize.Width / 2
        For i = 0 To Me._document.Lines.Count - 1
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

    Private Iterator Function _GetFormattedTextChars() As IEnumerable(Of FormattedChar)
        Dim syntax As SyntaxNode
        Dim scope As Binding.BoundGlobalScope
        SyncLock Me._sync
            syntax = Me._latest?.SyntaxTree
            scope = Me._latest_scope
        End SyncLock
        Dim tokens As SyntaxToken() = Me._document.Tokens
        Dim line As Integer = 0
        Dim pos As Integer = 0
        For Each t As SyntaxToken In tokens
            Dim brush As Brush = New SolidBrush(ColorPalette.ColorOperator)
            Select Case t.Kind
                Case SyntaxKind.StringValueToken,
                                SyntaxKind.PartialStringStartToken,
                                SyntaxKind.PartialStringCenterToken,
                                SyntaxKind.PartialStringEndToken
                    brush = New SolidBrush(ColorPalette.ColorString)
                Case SyntaxKind.NumberValueToken,
                                SyntaxKind.DateValueToken
                    brush = New SolidBrush(ColorPalette.ColorNumber)
                Case SyntaxKind.IdentifierToken
                    Dim parent As SyntaxNode = syntax?.GetParent(t)
                    If parent IsNot Nothing AndAlso parent.Kind = SyntaxKind.TypeNameItemNode Then
                        Dim typename As TypeNameNode = syntax.GetParent(parent)
                        If t.Equals(typename.Items.Last.Identifier) Then
                            Dim type As Type = Nothing
                            Dim typestring As String = typename.Span.ToString.ToLower
                            If Not Me._type_cache.TryGetValue(typestring, type) Then
                                Dim resolver As New Binding.TypeResolver(typename, scope?.Imports)
                                type = resolver.ResolveType()
                                If type IsNot Nothing Then Me._type_cache.Add(typestring, type)
                            End If
                            If type IsNot Nothing Then
                                If SystemTypeName(t.Span.ToString) <> "" Then
                                    brush = New SolidBrush(ColorPalette.ColorKeyword)
                                Else
                                    If type.IsValueType Then
                                        brush = New SolidBrush(ColorPalette.ColorStructure)
                                    Else
                                        brush = New SolidBrush(ColorPalette.ColorTypeName)
                                    End If
                                End If
                            End If
                        Else
                            brush = New SolidBrush(ColorPalette.ColorIdentifier)
                        End If
                    Else
                        brush = New SolidBrush(ColorPalette.ColorIdentifier)
                    End If
                Case SyntaxKind.CommentToken
                    brush = New SolidBrush(ColorPalette.ColorComment)
                Case SyntaxKind.BoolValueToken
                    brush = New SolidBrush(ColorPalette.ColorKeyword)
                Case Else
                    If t.IsKeywordToken Then
                        brush = New SolidBrush(ColorPalette.ColorKeyword)
                    End If
            End Select
            For Each c As Char In t.Span.ToString
                Select Case c
                    Case vbCr
                        line += 1
                        pos = 0
                    Case Is >= " "c
                        Yield New FormattedChar(c, brush, New Point(pos, line))
                        pos += 1
                End Select
            Next
        Next
    End Function

    Private Sub _PrintCursor(r As Rectangle, g As Graphics, textSize As SizeF)
        Dim c As Point = Me._document.CursorPosition
        g.DrawLine(
                    New Pen(ColorPalette.ColorPlainText),
                    New PointF(
                        r.Left + _Padding.Left + (c.X + 0.2) * textSize.Width,
                        r.Top + _Padding.Top + (c.Y + 0.95) * textSize.Height
                    ),
                    New PointF(
                        r.Left + _Padding.Left + (c.X + 1.2) * textSize.Width,
                        r.Top + _Padding.Top + (c.Y + 0.95) * textSize.Height
                    )
                )
    End Sub

    Private Sub InputElement_KeyPress(sender As Object, e As KeyPressEventArgs) Handles Me.KeyPress
        Dim c As Char = e.KeyChar
        Select Case c
            Case Is >= " "
                Me._document.InsertAtCursor(c)
        End Select
    End Sub

    Private Sub InputElement_KeyDown(sender As Object, e As PreviewKeyDownEventArgs) Handles Me.PreviewKeyDown
        If e.Modifiers = Keys.None Then
            Select Case e.KeyCode
                Case Keys.Enter
                    Me._document.InsertNewLine()
                Case Keys.Tab
                    Me._document.InsertAtCursor("   ")
                Case Keys.Back
                    Me._document.RemoveBeforeCursor()
                Case Keys.Delete
                    Me._document.RemoveAfterCursor()
                Case Keys.Home
                    Me._document.CursorMoveHome()
                Case Keys.End
                    Me._document.CursorMoveEnd()
                Case Keys.Up
                    Me._document.CursorMoveUp()
                Case Keys.Down
                    Me._document.CursorMoveDown()
                Case Keys.Left
                    Me._document.CursorMoveLeft()
                Case Keys.Right
                    Me._document.CursorMoveRight()
            End Select
        ElseIf e.Modifiers = Keys.Control Then
            Select Case e.KeyCode
                Case Keys.Enter
                    Me._SubmitDocument()
            End Select
        End If
    End Sub

    Private Sub _SubmitDocument()
        Me.TimerCompile.Enabled = False
        Me._PerformCompilation()
        Dim submitted_text As New FormattedText(Me._GetFormattedTextChars())
        Dim returnvalue As EvaluationResult = Me._latest.Evaluate(Me._state)
        Dim submitted_diagnostics As ErrorList
        submitted_diagnostics = returnvalue.Diagnostics
        Dim submitted_value As Object = Nothing
        If Not returnvalue.Diagnostics.HasErrors Then submitted_value = returnvalue.Value
        Me._ClearAll()
        Me.TimerCompile.Enabled = True
        Me.Invalidate(True)
        RaiseEvent SubmittedDocument(Me, New SubmittedDocumentEventArgs(submitted_text, submitted_diagnostics, submitted_value))
    End Sub

    Private Sub _ClearAll()
        Me._latest = Nothing
        Me._latest_scope = Nothing
        Me._latest_diagnostics = New ErrorList
        Me._document.Clear()
        Me._last_text = ""
        Me._text = FormattedText.Empty
    End Sub

    Private Sub TimerCompile_Tick(sender As Object, e As EventArgs) Handles TimerCompile.Tick
        If Me._document.Text = Me._last_text Then Exit Sub
        Dim task As New Task(
            Sub()
                Me._PerformCompilation()
                Me.Invoke(
                    Sub()
                        TimerCompile.Enabled = True
                        Me._text = New FormattedText(Me._GetFormattedTextChars)
                        Me.Invalidate(True)
                    End Sub
                )
            End Sub
        )
        task.Start()
        TimerCompile.Enabled = False
        Me._last_text = Me._document.Text
        Me.Invalidate(True)
    End Sub

    Private Sub _PerformCompilation()
        Dim latest As Compilation = Compilation.CreateFromText(Me._previous, Me._document.Text)
        Dim scope As Binding.BoundGlobalScope = latest.TryBindProgram
        SyncLock Me._sync
            Me._latest = latest
            Me._latest_scope = scope
            Me._latest_diagnostics = New ErrorList(Me._latest.Diagnostics & Me._latest_scope.Diagnostics)
        End SyncLock
    End Sub

    Private Sub PanelScroll_MouseMove(sender As Object, e As MouseEventArgs) Handles PanelScroll.MouseMove
        If Me.PanelTextContent.Width > Me.PanelText.Width Then
            If Me._scroll_down Then
                Dim delta As Integer = e.Location.X - Me._scroll_start
                Me._scroll_start = e.Location.X
                Dim max As Integer = (Me.PanelScroll.Width - 40) - Me._scroll_size
                Dim scroll_max As Integer = Me.PanelTextContent.Width - Me.PanelText.Width
                Me._scroll_location = Math.Max(0, Math.Min(scroll_max, Me._scroll_location + delta / max * scroll_max))
                Me.PanelTextContent.Left = -Me._scroll_location
                Me.PanelScroll.Invalidate()
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
        If e.Location.X > 20 And e.Location.X < Me.PanelScroll.Width - 20 Then
            Me._scroll_start = e.Location.X
            Me._scroll_down = True
        End If
    End Sub

    Private Sub PanelScroll_MouseUp(sender As Object, e As MouseEventArgs) Handles PanelScroll.MouseUp
        Me._scroll_down = False
    End Sub

    Private Sub PanelScroll_MouseClick(sender As Object, e As MouseEventArgs) Handles PanelScroll.MouseClick
        If Me.PanelTextContent.Width > Me.PanelText.Width Then
            Dim scroll_max As Integer = Me.PanelTextContent.Width - Me.PanelText.Width
            If e.Location.X < 20 Then
                Me._scroll_location = Math.Max(0, Math.Min(scroll_max, Me._scroll_location - 0.05 * scroll_max))
                Me.PanelTextContent.Left = -Me._scroll_location
                Me.PanelScroll.Invalidate()
            ElseIf e.Location.X > Me.PanelScroll.Width - 20 Then
                Me._scroll_location = Math.Max(0, Math.Min(scroll_max, Me._scroll_location + 0.05 * scroll_max))
                Me.PanelTextContent.Left = -Me._scroll_location
                Me.PanelScroll.Invalidate()
            End If
        End If
    End Sub

    Private Sub PanelScroll_MouseLeave(sender As Object, e As EventArgs) Handles PanelScroll.MouseLeave
        Me._scroll_is_hover_bar = False
        Me._scroll_is_hover_left = False
        Me._scroll_is_hover_right = False
        Me.PanelScroll.Invalidate()
    End Sub

End Class

Public Delegate Sub SubmittedDocumentEventHandler(sender As Object, e As SubmittedDocumentEventArgs)

Public Class SubmittedDocumentEventArgs : Inherits EventArgs

    Public Sub New(text As FormattedText, diagnostics As ErrorList, value As Object)
        Me.Text = text
        Me.Diagnostics = diagnostics
        Me.Value = value
    End Sub

    Public ReadOnly Property Text As FormattedText
    Public ReadOnly Property Diagnostics As ErrorList
    Public ReadOnly Property Value As Object

End Class
