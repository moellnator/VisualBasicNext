Imports System.Reflection
Imports VisualBasicNext.CodeAnalysis
Imports VisualBasicNext.CodeAnalysis.Diagnostics
Imports VisualBasicNext.CodeAnalysis.Evaluating
Imports VisualBasicNext.CodeAnalysis.Lexing
Imports VisualBasicNext.CodeAnalysis.Parsing

Public Class InputElement

    'TODO [implement] -> Add History of submitted documents
    'TODO [suggestion] -> Add auto completition?
    'TODO [suggestion] -> Add auto indenting?

    Public Event SubmittedDocument As SubmittedDocumentEventHandler
    Public Event CursorPositionChanged As CursorPositionChangedEventHandler

    Private ReadOnly _document As New Document

    Private Shared ReadOnly _ErrorPen As Pen = _MakeUnderlineTexture()

    Private _sync As Object = New Object
    Private _suppress_resize As Boolean = False
    Private _previous As Compilation = Nothing
    Private _latest As Compilation
    Private _latest_diagnostics As New ErrorList
    Private _latest_scope As Binding.BoundGlobalScope
    Private _state As VMState = New VMState
    Private _last_text As String = ""
    Private _type_cache As New Dictionary(Of String, Type)

    Public Sub New()
        MyBase.New
        InitializeComponent()
        Me.SetHightlight(ColorPalette.ColorKeyword)
        Me._small_scroll = False
        AddHandler Me._document.DocumentChanged, AddressOf Me._DocumentChangedHandler
        Me._Text = New FormattedText(Me._GetFormattedTextChars, 0)
        Me.TimerCompile.Enabled = True
    End Sub

    Private Sub InputElement_Disposed(sender As Object, e As EventArgs) Handles Me.Disposed
        Me._document.Dispose()
    End Sub

    Private Sub _DocumentChangedHandler(sender As Object, e As EventArgs)
        Me._Text = New FormattedText(Me._GetFormattedTextChars, Me._document.Lines.Count - 1)
        Me.AutoSizeElement()
        Me._FocusCursor()
        Me.Invalidate(True)
    End Sub

    Private Sub _FocusCursor()
        Dim text_size As New SizeF(Me.Font.SizeInPoints / 72 * 96 * _CharRatio, Me.Font.SizeInPoints / 72 * 96 * (1 + _LineSeparation))
        Dim cursor As New PointF(
            text_size.Width * Me._document.CursorPosition.X + _Padding.Left,
            text_size.Height * (Me._document.CursorPosition.Y + 1) + _Padding.Top
        )
        Dim right_edge As Integer = Me.PanelText.Width + Me.ScrollLocation
        Dim left_edge As Integer = Me.ScrollLocation
        If cursor.X + 3 * text_size.Width > right_edge Then
            Me.ScrollLocation = cursor.X + 3 * text_size.Width - Me.PanelText.Width
        ElseIf cursor.X < left_edge Then
            Me.ScrollLocation = cursor.X
        End If
        RaiseEvent CursorPositionChanged(Me, New CursorPositionChangedEventArgs(cursor, text_size))
    End Sub

    Protected Overrides Sub PaintTextContent(r As Rectangle, g As Graphics, textSize As SizeF)
        MyBase.PaintTextContent(r, g, textSize)
        Me._PrintDiagnostics(r, g, textSize)
        Me._PrintCursor(r, g, textSize)
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
        Dim submitted_diagnostics As ErrorList = Me._latest.Diagnostics
        Dim returnvalue As EvaluationResult = Me._latest.Evaluate(Me._state)
        submitted_diagnostics &= returnvalue.Diagnostics
        Dim submitted_value As Object = Nothing
        If Not returnvalue.Diagnostics.HasErrors Then
            submitted_value = returnvalue.Value
            Me._previous = Me._latest
        End If
        Me._ClearAll()
        Me.TimerCompile.Enabled = True
        Me.Invalidate(True)
        RaiseEvent SubmittedDocument(Me, New SubmittedDocumentEventArgs(submitted_text, submitted_diagnostics, submitted_value))
    End Sub

    Private Sub _ClearAll()
        Me._Text = FormattedText.Empty
        Me._latest = Nothing
        Me._latest_scope = Nothing
        Me._latest_diagnostics = New ErrorList
        Me._document.Clear()
        Me._last_text = ""
        Me.AutoSizeElement()
        Me._FocusCursor()
    End Sub

    Private Sub TimerCompile_Tick(sender As Object, e As EventArgs) Handles TimerCompile.Tick
        If Me._document.Text = Me._last_text Then Exit Sub
        Dim task As New Task(
            Sub()
                Me._PerformCompilation()
                Me.Invoke(
                    Sub()
                        TimerCompile.Enabled = True
                        Me._Text = New FormattedText(Me._GetFormattedTextChars, Me._document.Lines.Count - 1)
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

Public Delegate Sub CursorPositionChangedEventHandler(sender As Object, e As CursorPositionChangedEventArgs)

Public Class CursorPositionChangedEventArgs : Inherits EventArgs

    Public Sub New(newCursorLocation As PointF, textSize As SizeF)
        Me.NewCursorLocation = newCursorLocation
        Me.TextSize = textSize
    End Sub

    Public ReadOnly Property NewCursorLocation As PointF
    Public ReadOnly Property TextSize As SizeF

End Class
