Imports System.Collections.Immutable
Imports VisualBasicNext.CodeAnalysis.Diagnostics
Imports VisualBasicNext.CodeAnalysis.Text

Namespace Lexing
    Friend Class Lexer : Implements IEnumerable(Of SyntaxToken)

        Public ReadOnly Property Diagnostics As New ErrorList
        Private ReadOnly _source As Source
        Private ReadOnly _text As String

        Private _index As Integer

        Private ReadOnly _partial_string_brace_ballance As New Stack(Of Integer)
        Private _open_brace_ballance As Integer = 0

        Public Sub New(source As Source)
            Me._source = source
            Me._text = source.ToString
            Me._index = 0
        End Sub

        Private Function _peek(Optional offset As Integer = 0) As Char
            Dim index As Integer = Me._index + offset
            Return If(index < Me._text.Count, Me._text(index), vbNullChar)
        End Function

        Private Function _current() As Char
            Return Me._peek(0)
        End Function

        Private Function _next() As Char
            Return Me._peek(1)
        End Function

        Private Sub _move(number As Integer)
            Me._index += number
        End Sub

        Private Sub _move_next()
            Me._move(1)
        End Sub

        Public Function GetNextToken() As SyntaxToken
            Dim kind As SyntaxKind = SyntaxKind.EndOfSourceToken
            Dim start As Integer = Me._index
            Dim value As Object = Nothing
            Select Case True
                Case Me._current = vbNullChar
                Case Me._current = vbCr AndAlso Me._next = vbLf
                    kind = SyntaxKind.EndOfLineToken
                    Me._move(2)
                Case Me._current = ":"c
                    kind = SyntaxKind.EndOfLineToken
                    Me._move_next()
                Case Char.IsWhiteSpace(Me._current)
                    Me._read_whitespace()
                    kind = SyntaxKind.WhiteSpaceToken
                Case Me._current = "'"c
                    Dim line As Line = Me._source(Me._source.GetLineIndex(Me._index))
                    Me._move(line.Start + line.Length - Me._index)
                    kind = SyntaxKind.CommentToken
                Case Me._current = """"c
                    value = Me._read_string()
                    kind = SyntaxKind.StringValueToken
                Case Me._current = "&"c AndAlso {"b"c, "o"c, "h"c}.Contains(Char.ToLower(Me._next))
                    value = _read_base_number()
                    kind = If(value IsNot Nothing, SyntaxKind.NumberValueToken, SyntaxKind.BadToken)
                Case Char.IsDigit(Me._current)
                    value = _read_number()
                    kind = SyntaxKind.NumberValueToken
                Case Me._current = "$"
                    Dim partial_string_count = Me._partial_string_brace_ballance.Count
                    value = Me._read_partial_string
                    If Me._index - start < 2 Then
                        kind = SyntaxKind.BadToken
                    Else
                        kind = If(partial_string_count <> Me._partial_string_brace_ballance.Count, SyntaxKind.PartialStringStartToken, SyntaxKind.StringValueToken)
                    End If
                Case Me._current = "#"
                    value = _read_date()
                    kind = SyntaxKind.DateValueToken
                Case Me._current = "."c
                    kind = SyntaxKind.DotToken
                    Me._move_next()
                Case Me._current = "?"c
                    If Me._next = "." Then
                        kind = SyntaxKind.QuestionmarkDotToken
                        Me._move(2)
                    ElseIf Me._next = "(" Then
                        kind = SyntaxKind.QuestionmarkOpenBracketToken
                        Me._move(2)
                    Else
                        kind = SyntaxKind.QuestionmarkToken
                        Me._move_next()
                    End If
                Case Me._current = ","c
                    kind = SyntaxKind.CommaToken
                    Me._move_next()
                Case Me._current = "("c
                    kind = SyntaxKind.OpenBracketToken
                    Me._move_next()
                Case Me._current = ")"c
                    kind = SyntaxKind.CloseBracketToken
                    Me._move_next()
                Case Me._current = "{"c
                    kind = SyntaxKind.OpenBraceToken
                    Me._move_next()
                    Me._open_brace_ballance += 1
                Case Me._current = "}"c
                    kind = SyntaxKind.CloseBraceToken
                    Me._open_brace_ballance -= 1
                    If Me._partial_string_brace_ballance.Count <> 0 AndAlso Me._open_brace_ballance = Me._partial_string_brace_ballance.Peek Then
                        Dim partial_string_count = Me._partial_string_brace_ballance.Count
                        value = Me._read_partial_string
                        kind = If(Me._partial_string_brace_ballance.Count = partial_string_count, SyntaxKind.PartialStringCenterToken, SyntaxKind.PartialStringEndToken)
                    Else
                        Me._move_next()
                    End If
                Case Me._current = "="c
                    kind = SyntaxKind.EqualsToken
                    Me._move_next()
                Case Me._current() = "+"c
                    If Me._next = "=" Then
                        kind = SyntaxKind.PlusEqualsToken
                        Me._move(2)
                    Else
                        kind = SyntaxKind.PlusToken
                        Me._move_next()
                    End If
                Case Me._current() = "-"c
                    If Me._next = "=" Then
                        kind = SyntaxKind.MinusEqualsToken
                        Me._move(2)
                    Else
                        kind = SyntaxKind.MinusToken
                        Me._move_next()
                    End If
                Case Me._current() = "*"c
                    If Me._next = "=" Then
                        kind = SyntaxKind.StarEqualsToken
                        Me._move(2)
                    Else
                        kind = SyntaxKind.StarToken
                        Me._move_next()
                    End If
                Case Me._current() = "/"c
                    If Me._next = "=" Then
                        kind = SyntaxKind.SlashEqualsToken
                        Me._move(2)
                    Else
                        kind = SyntaxKind.SlashToken
                        Me._move_next()
                    End If
                Case Me._current() = "\"c
                    If Me._next = "=" Then
                        kind = SyntaxKind.BackslashEqualsToken
                        Me._move(2)
                    Else
                        kind = SyntaxKind.BackslashToken
                        Me._move_next()
                    End If
                Case Me._current() = "&"c
                    If Me._next = "=" Then
                        kind = SyntaxKind.AmpersandEqualsToken
                        Me._move(2)
                    Else
                        kind = SyntaxKind.AmpersandToken
                        Me._move_next()
                    End If
                Case Me._current() = "^"c
                    If Me._next = "=" Then
                        kind = SyntaxKind.CircumflexEqualsToken
                        Me._move(2)
                    Else
                        kind = SyntaxKind.CircumflexToken
                        Me._move_next()
                    End If
                Case Me._current() = ">"c
                    If Me._next = ">"c Then
                        Me._move_next()
                        If Me._next = "=" Then
                            kind = SyntaxKind.GreaterGreaterEqualsToken
                            Me._move(2)
                        Else
                            kind = SyntaxKind.GreaterGreaterToken
                            Me._move_next()
                        End If
                    ElseIf Me._next = "="c Then
                        kind = SyntaxKind.GreaterEqualsToken
                        Me._move(2)
                    Else
                        kind = SyntaxKind.GreaterToken
                        Me._move_next()
                    End If
                Case Me._current() = "<"c
                    If Me._next = "<"c Then
                        Me._move_next()
                        If Me._next = "=" Then
                            kind = SyntaxKind.LowerLowerEqualsToken
                            Me._move(2)
                        Else
                            kind = SyntaxKind.LowerLowerToken
                            Me._move_next()
                        End If
                    ElseIf Me._next = "="c Then
                        kind = SyntaxKind.LowerEqualsToken
                        Me._move(2)
                    ElseIf Me._next = ">"c Then
                        kind = SyntaxKind.LowerGreaterToken
                        Me._move(2)
                    Else
                        kind = SyntaxKind.LowerToken
                        Me._move_next()
                    End If
                Case Char.IsLetter(Me._current) Or Me._current = "_"c
                    kind = Me._read_identifier(value)
                Case Else
                    Me.Diagnostics.ReportBadCharakter(Me._current, New Span(Me._source, Me._index, 1))
                    kind = SyntaxKind.BadToken
                    Me._move_next()
            End Select
            Dim span As Span = Span.FromBounds(Me._source, start, Me._index)
            Return New SyntaxToken(kind, span, value)
        End Function

        Private Function _read_partial_string() As Object
            Dim start As Integer = Me._index
            Dim isinital As Boolean = False
            If Me._current = "$" Then
                Me._move_next()
                If Me._current <> """"c Then
                    Me.Diagnostics.ReportBadCharakter(Me._current, New Span(Me._source, Me._index, 1))
                    Return Nothing
                End If
                Me._move_next()
                isinital = True
            ElseIf Me._current = "}" Then
                Me._move_next()
            Else
                Throw New Exception("No partial extrapolated string found.")
            End If
            Dim done As Boolean = False
            Dim text As New System.Text.StringBuilder
            While Not done
                Select Case Me._current
                    Case "{"c
                        Me._move_next()
                        done = True
                        If Not isinital Then Me._partial_string_brace_ballance.Pop()
                        Me._partial_string_brace_ballance.Push(Me._open_brace_ballance)
                        Me._open_brace_ballance += 1
                    Case """"
                        If Me._next = """"c Then
                            text.Append(""""c)
                            Me._move(2)
                        Else
                            If Not isinital Then Me._partial_string_brace_ballance.Pop()
                            Me._move_next()
                            done = True
                        End If
                    Case vbNullChar
                        Me.Diagnostics.ReportMissing("""", New Span(Me._source, Me._index, 1))
                        Return Nothing
                    Case Else
                        text.Append(Me._current)
                        Me._move_next()
                End Select
            End While
            Return text.ToString
        End Function

        Private Sub _read_whitespace()
            While Char.IsWhiteSpace(Me._current) And Not (Me._current = vbCr Or Me._current = vbLf)
                Me._move_next()
            End While
        End Sub

        Private Function _read_base_number() As Object
            Dim base As Integer
            Dim start As Integer = Me._index
            Me._move_next()
            Select Case Char.ToLower(Me._current)
                Case "b"
                    base = 2
                Case "o"
                    base = 8
                Case "h"
                    base = 16
            End Select
            Dim done As Boolean = False
            Dim badchar As Boolean = False
            Dim builder As New System.Text.StringBuilder
            Dim value As Object = Nothing
            Dim unsigned As Boolean = False
            Dim width As Integer = 4
            While Not done
                Me._move_next()
                Select Case Char.ToLower(Me._current)
                    Case "a" To "f"
                        If base = 16 Then
                            builder.Append(Me._current)
                        Else
                            badchar = True
                        End If
                    Case "8", "9"
                        If base >= 10 Then
                            builder.Append(Me._current)
                        Else
                            badchar = True
                        End If
                    Case "2" To "7"
                        If base >= 8 Then
                            builder.Append(Me._current)
                        Else
                            badchar = True
                        End If
                    Case "0", "1"
                        builder.Append(Me._current)
                    Case "u", "s", "i", "l"
                        If builder.Length >= 1 AndAlso Me._read_int_type_suffix(unsigned, width) Then
                            done = True
                        Else
                            badchar = True
                        End If
                    Case Else
                        done = True
                End Select
                If badchar Then
                    Dim span As Span
                    If builder.Length > 0 Then
                        span = New Span(Me._source, Me._index, 1)
                        Me.Diagnostics.ReportBadCharakter(Me._current.ToString, span)
                        Return 0
                    End If
                    Me._move_next()
                    span = New Span(Me._source, start, Me._index - start)
                    Me.Diagnostics.ReportBadLiteral(Me._current.ToString, GetType(Integer), span)
                    Return Nothing
                End If
            End While
            If builder.Length = 0 Then
                Dim span As New Span(Me._source, start, Me._index - start)
                Me.Diagnostics.ReportBadLiteral(Me._current.ToString, GetType(Integer), span)
                Return Nothing
            End If
            Me._TryConvertToInteger(New Span(Me._source, start, Me._index - start), builder.ToString, unsigned, width, base, value)
            Return value
        End Function

        Private Function _read_int_type_suffix(ByRef unsigned As Boolean, ByRef width As Integer) As Boolean
            Dim result As Boolean = True
            If Char.ToLower(Me._current) = "u"c Then
                If {"i"c, "l"c, "s"c, "b"c}.Contains(Char.ToLower(Me._next)) Then
                    unsigned = True
                    Me._move_next()
                End If
            End If
            Select Case Char.ToLower(Me._current)
                Case "s"c
                    If Char.ToLower(Me._next) = "b" And Not unsigned Then
                        Me._move_next()
                        unsigned = False
                        width = 1
                    Else
                        width = 2
                    End If
                Case "b"
                    If unsigned Then
                        unsigned = True
                        width = 1
                    Else
                        result = False
                    End If
                Case "i"
                    width = 4
                Case "l"
                    width = 8
                Case Else
                    result = False
            End Select
            If result Then Me._move_next()
            Return result
        End Function

        Private Function _TryConvertToInteger(span As Span, text As String, unsigned As Boolean, width As Integer, base As Integer, ByRef retval As Object) As Boolean
            Dim results As Boolean = False
            Dim rettype As Type = Nothing
            Try
                Dim data As Object = Nothing
                If unsigned Then
                    Select Case width
                        Case 1
                            rettype = GetType(Byte)
                            data = Convert.ToByte(text, base)
                        Case 2
                            rettype = GetType(UShort)
                            data = Convert.ToUInt16(text, base)
                        Case 4
                            rettype = GetType(UInteger)
                            data = Convert.ToUInt32(text, base)
                        Case 8
                            rettype = GetType(ULong)
                            data = Convert.ToUInt64(text, base)
                        Case Else
                            Throw New ArgumentException($"Invalid integer width {width}.", "width")
                    End Select
                Else
                    Select Case width
                        Case 1
                            rettype = GetType(SByte)
                            data = Convert.ToSByte(text, base)
                        Case 2
                            rettype = GetType(Short)
                            data = Convert.ToInt16(text, base)
                        Case 4
                            rettype = GetType(Integer)
                            data = Convert.ToInt32(text, base)
                        Case 8
                            rettype = GetType(Long)
                            data = Convert.ToInt64(text, base)
                        Case Else
                            Throw New ArgumentException($"Invalid integer width {width}.", "width")
                    End Select
                End If
                retval = data
                results = True
            Catch ex As Exception When rettype IsNot Nothing
                Me.Diagnostics.ReportBadLiteral(text, rettype, span)
            End Try
            Return results
        End Function

        Private Function _read_number() As Object
            Dim float As Boolean = False
            Dim value As Object = Nothing
            Dim exponent As Boolean = False
            Dim builder As New System.Text.StringBuilder
            Dim badchar As Boolean = False
            Dim badtype As Type = Nothing
            Dim done As Boolean = False
            Dim start As Integer = Me._index
            Dim c As Globalization.CultureInfo = Globalization.CultureInfo.InvariantCulture
            While Not done
                Select Case Char.ToLower(Me._current)
                    Case "0" To "9"
                        builder.Append(Me._current)
                    Case "."
                        If Not float And Char.IsDigit(Me._next) Then
                            float = True
                            builder.Append(Me._current)
                        Else
                            badchar = True
                        End If
                    Case "e"
                        If Not exponent Or builder.Length = 0 Then
                            float = True
                            exponent = True
                            builder.Append(Me._current)
                            If Me._next = "+" Or Me._next = "-" Then
                                Me._move_next()
                                builder.Append(Me._current)
                            End If
                            If Not Char.IsDigit(Me._next) Then badchar = True
                        Else
                            badchar = True
                        End If
                    Case "f"
                        If Not Single.TryParse(builder.ToString, Globalization.NumberStyles.AllowDecimalPoint Or Globalization.NumberStyles.AllowExponent, c, value) Then
                            badtype = GetType(Single)
                        Else
                            Me._move_next()
                        End If
                        done = True
                    Case "r"
                        If Not Double.TryParse(builder.ToString, Globalization.NumberStyles.AllowDecimalPoint Or Globalization.NumberStyles.AllowExponent, c, value) Then
                            badtype = GetType(Double)
                        Else
                            Me._move_next()
                        End If
                        done = True
                    Case "d"
                        If exponent OrElse Not Decimal.TryParse(builder.ToString, Globalization.NumberStyles.AllowDecimalPoint, c, value) Then
                            badtype = GetType(Decimal)
                        Else
                            Me._move_next()
                        End If
                        done = True
                    Case "u", "s", "i", "l"
                        Dim unsigned As Boolean = False
                        Dim width As Integer = 4
                        If Me._read_int_type_suffix(unsigned, width) Then
                            Dim span As New Span(Me._source, start, Me._index - start)
                            _TryConvertToInteger(span, builder.ToString, unsigned, width, 10, value)
                            Return value
                        Else
                            badchar = True
                        End If
                    Case Else
                        done = True
                End Select
                If badchar Then
                    Dim span As New Span(Me._source, Me._index, 1)
                    Me.Diagnostics.ReportBadCharakter(Me._current, span)
                    Me._move_next()
                    Return Nothing
                ElseIf badtype IsNot Nothing Then
                    Me._move_next()
                    Dim span As New Span(Me._source, start, Me._index - start)
                    Me.Diagnostics.ReportBadLiteral(builder.ToString, badtype, span)
                    Return Nothing
                End If
                If Not done Then Me._move_next()
            End While
            If value Is Nothing Then
                Dim span As New Span(Me._source, start, Me._index - start)
                If float Then
                    If Not Double.TryParse(builder.ToString, Globalization.NumberStyles.AllowDecimalPoint Or Globalization.NumberStyles.AllowExponent, c, value) Then
                        Me.Diagnostics.ReportBadLiteral(builder.ToString, GetType(Double), span)
                    End If
                Else
                    _TryConvertToInteger(span, builder.ToString, False, 4, 10, value)
                End If
            End If
            Return value
        End Function

        Private Function _read_string() As Object
            Dim start As Integer = Me._index
            Dim done As Boolean = False
            Dim string_builder As New System.Text.StringBuilder()
            Me._move(1)
            While Not done
                If Me._current = """" Then
                    If Me._next = """" Then
                        string_builder.Append("""")
                        Me._move_next()
                    ElseIf Me._next = "c"c Then
                        Me._move(2)
                        If Not string_builder.Length = 1 Then
                            Dim span As New Span(Me._source, start, Me._index - start)
                            Me.Diagnostics.ReportBadLiteral(string_builder.ToString, GetType(Char), span)
                            Return Nothing
                        Else
                            Return string_builder.ToString.First
                        End If
                    Else
                        done = True
                    End If
                ElseIf Me._current = vbNullChar Then
                    Me.Diagnostics.ReportMissing("""", New Span(Me._source, Me._index, 1))
                    Return Nothing
                Else
                    string_builder.Append(Me._current)
                End If
                Me._move_next()
            End While
            Return string_builder.ToString
        End Function

        Private Function _read_date() As Date
            Dim start As Integer = Me._index
            Dim builder As New System.Text.StringBuilder
            Dim done As Boolean = False
            While Not done
                Me._move_next()
                If Me._current = "#" Then
                    Me._move_next()
                    done = True
                ElseIf Me._current = vbNullChar Then
                    Me.Diagnostics.ReportMissing("#", New Span(Me._source, Me._index, 1))
                    Return Nothing
                Else
                    builder.Append(Me._current)
                End If
            End While
            Dim value As Date
            If Not Date.TryParse(builder.ToString, value) Then
                Dim span As New Span(Me._source, start, Me._index - start)
                Me.Diagnostics.ReportBadLiteral(builder.ToString, GetType(Date), span)
                Return Nothing
            Else
                Return value
            End If
        End Function

        Private Function _read_identifier(ByRef value As Object) As SyntaxKind
            Dim done As Boolean = False
            Dim start As Integer = Me._index
            Dim builder As New System.Text.StringBuilder
            Dim retval As SyntaxKind = SyntaxKind.IdentifierToken
            value = Nothing
            While Not done
                If Char.IsLetterOrDigit(Me._current) Or Me._current = "_"c Then
                    builder.Append(Me._current)
                Else
                    done = True
                End If
                If Not done Then Me._move_next()
            End While
            Dim name As String = builder.ToString
            Select Case name.ToLower
                Case "true", "false"
                    value = Boolean.Parse(name)
                    retval = SyntaxKind.BoolValueToken
                Case "nothing"
                    value = Nothing
                    retval = SyntaxKind.NothingKeywordToken
                Case "and"
                    retval = SyntaxKind.AndKeywordToken
                Case "andalso"
                    retval = SyntaxKind.AndAlsoKeywordToken
                Case "or"
                    retval = SyntaxKind.OrKeywordToken
                Case "orelse"
                    retval = SyntaxKind.OrElseKeywordToken
                Case "xor"
                    retval = SyntaxKind.XorKeywordToken
                Case "not"
                    retval = SyntaxKind.NotKeywordToken
                Case "mod"
                    retval = SyntaxKind.ModKeywordToken
                Case "is"
                    retval = SyntaxKind.IsKeywordToken
                Case "isnot"
                    retval = SyntaxKind.IsNotKeywordToken
                Case "like"
                    retval = SyntaxKind.LikeKeywordToken
                Case "if"
                    retval = SyntaxKind.IfKeywordToken
                Case "typeof"
                    retval = SyntaxKind.TypeOfKeywordToken
                Case "new"
                    retval = SyntaxKind.NewKeywordToken
                Case "ctype"
                    retval = SyntaxKind.CTypeKeywordToken
                Case "ctypedynamic"
                    retval = SyntaxKind.CTypeDynamicKeywordToken
                Case "as"
                    retval = SyntaxKind.AsKeywordToken
                Case "of"
                    retval = SyntaxKind.OfKeywordToken
                Case "function"
                    retval = SyntaxKind.FunctionKeywordToken
                Case "sub"
                    retval = SyntaxKind.SubKeywordToken
                Case "dim"
                    retval = SyntaxKind.DimKeywordToken
                Case "imports"
                    retval = SyntaxKind.ImportsKeywordToken
                Case "gettype"
                    retval = SyntaxKind.GetTypeKeywordToken
                Case "trycast"
                    retval = SyntaxKind.GetTryCastKeywordToken
                Case "_"
                    Me.Diagnostics.ReportBadCharakter("_", New Span(Me._source, start, 1))
                Case Else
                    retval = SyntaxKind.IdentifierToken
            End Select
            Return retval
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of SyntaxToken) Implements IEnumerable(Of SyntaxToken).GetEnumerator
            Dim current As SyntaxToken = Nothing
            Do
                current = Me.GetNextToken
                Yield current
            Loop Until current.Kind = SyntaxKind.EndOfSourceToken
        End Function

        Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Return Me.GetEnumerator
        End Function

    End Class

End Namespace
