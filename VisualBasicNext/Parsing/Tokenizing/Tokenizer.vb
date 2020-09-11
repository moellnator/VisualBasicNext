Namespace Parsing.Tokenizing
    Public Class Tokenizer

        Private Enum _TokenStates
            Idle = 0
            Text
            Number
            Identifier
        End Enum

        Public Shared Iterator Function Tokenize(text As String) As IEnumerable(Of Token)
            If text.Equals(String.Empty) Then Exit Function
            Dim current As New TextLocation(text, 0)
            Dim start As Integer = -1
            Dim state As _TokenStates = _TokenStates.Idle
            While True
                Dim skip As Integer = 1
                Select Case state
                    Case _TokenStates.Idle
                        Select Case current.GetChar
                            Case " ", vbTab
                            Case "'"
                                skip = current.GetLine.Count - current.Column
                            Case vbCr
                                Yield New Token(TokenTypes.EndOfLine, current, 2)
                                skip = 2
                            Case """"
                                state = _TokenStates.Text
                                start = current.Position
                            Case ","
                                Yield New Token(TokenTypes.Separator, current, 1)
                            Case "<", ">"
                                Select Case current.GetNextChar
                                    Case ">", "="
                                        If current.GetText(3) = ">>=" Then
                                            skip = 3
                                        Else
                                            skip = 2
                                        End If
                                    Case "<"
                                        If Not current.GetChar = ">" Then
                                            If current.GetText(3) = "<<=" Then
                                                skip = 3
                                            Else
                                                skip = 2
                                            End If
                                        End If
                                End Select
                                Yield New Token(TokenTypes.Operator, current, skip)
                            Case ".", ":", "="
                                Yield New Token(TokenTypes.Operator, current, 1)
                                skip = 1
                            Case "+", "-", "*", "/", "\", "^"
                                If current.GetNextChar() = "=" Then skip = 2
                                Yield New Token(TokenTypes.Operator, current, skip)
                            Case "(", "{"
                                Yield New Token(TokenTypes.BlockOpen, current, 1)
                            Case ")", "}"
                                Yield New Token(TokenTypes.BlockClose, current, 1)
                            Case "&"
                                Select Case current.GetNextChar
                                    Case "="
                                        Yield New Token(TokenTypes.Operator, current, 2)
                                        skip = 2
                                    Case "H", "B", "h", "b"
                                        state = _TokenStates.Number
                                        start = current.Position
                                        skip = 2
                                    Case Else
                                        Yield New Token(TokenTypes.Operator, current, 1)
                                End Select
                            Case "0" To "9"
                                state = _TokenStates.Number
                                start = current.Position
                            Case "a" To "z", "A" To "Z", "_"
                                state = _TokenStates.Identifier
                                start = current.Position
                            Case Else
                                Throw New ParserException($"Invalid character '{current.GetChar}' at {current.ToString}.", current)
                        End Select
                    Case _TokenStates.Text
                        If current.GetChar = """" Then
                            If current.GetNextChar <> """" Then
                                If Char.ToLower(current.GetNextChar) = "c"c Then
                                    If current.Position - start + 1 <> 4 Then Throw New ParserException($"Invalid character format at {current.ToString}.", current)
                                    Yield New Token(TokenTypes.Character, New TextLocation(text, start), current.Position - start + 1)
                                    state = _TokenStates.Idle
                                    skip = 2
                                Else
                                    Yield New Token(TokenTypes.String, New TextLocation(text, start), current.Position - start + 1)
                                End If
                                state = _TokenStates.Idle
                            Else
                                skip = 2
                            End If
                        End If
                    Case _TokenStates.Number
                        Dim is_invalid_digit As Boolean = False
                        If text(start) = "&" Then
                            Select Case text(start + 1)
                                Case "H", "h"
                                    Select Case current.GetChar
                                        Case "0" To "9", "a" To "f", "A" To "F"
                                        Case Else
                                            is_invalid_digit = True
                                    End Select
                                Case "B", "b"
                                    Select Case current.GetChar
                                        Case "0", "1"
                                        Case Else
                                            is_invalid_digit = True
                                    End Select
                            End Select
                        Else
                            Select Case current.GetChar
                                Case "0" To "9"
                                Case "."
                                    If text.Substring(start, current.Position - start).Contains(".") Then
                                        is_invalid_digit = True
                                    End If
                                Case "e", "E"
                                    If text.Substring(start, current.Position - start).Any(Function(c) c = "e" Or c = "E") Then
                                        is_invalid_digit = True
                                    Else
                                        Select Case current.GetNextChar
                                            Case "+", "-"
                                                skip = 2
                                        End Select
                                    End If
                                Case Else
                                    is_invalid_digit = True
                            End Select
                        End If
                        If is_invalid_digit Then
                            skip = 0
                            If "drfils".Contains(Char.ToLower(current.GetChar)) Then
                                skip = 1
                            ElseIf Char.ToLower(current.GetChar) = "u" AndAlso "ils".Contains(Char.ToLower(current.GetNextChar)) Then
                                skip = 2
                            End If
                            Yield New Token(TokenTypes.Number, New TextLocation(text, start), current.Position + skip - start)
                            state = _TokenStates.Idle
                        End If
                    Case _TokenStates.Identifier
                        Select Case current.GetChar
                            Case "_", "a" To "z", "A" To "Z", "0" To "9"
                            Case Else
                                Yield _BuildIdentifierToken(text, start, current)
                                state = _TokenStates.Idle
                                skip = 0
                        End Select
                End Select
                If current.CanSkip(skip) Then
                    current += skip
                Else
                    Exit While
                End If
            End While
            If state <> _TokenStates.Idle Then
                Select Case state
                    Case _TokenStates.Number
                        Yield New Token(TokenTypes.Number, New TextLocation(text, start), current.Position - start + 1)
                    Case _TokenStates.Text
                        Throw New ParserException($"String missing closing quotes at the end.", current)
                    Case _TokenStates.Identifier
                        Yield _BuildIdentifierToken(text, start, current, 1)
                End Select
            End If
        End Function

        Private Shared Function _BuildIdentifierToken(text As String, start As Integer, current As TextLocation, Optional append As Integer = 0)
            Dim retval As New TextLocation(text, start)
            Dim ident As String = retval.GetText(current.Position - start).ToLower
            Dim tokentype As TokenTypes = _TokenTypeFromIdentifier(ident)
            If tokentype = TokenTypes.Identifier AndAlso retval.GetText(current.Position - start).ToLower.All(Function(c) c = "_") Then _
                Throw New ParserException($"Invalid identifier {ident} at {current.Position.ToString}.", current)
            Return New Token(tokentype, retval, current.Position - start + append)
        End Function

        Private Shared Function _TokenTypeFromIdentifier(identifier As String) As TokenTypes
            Dim retval As TokenTypes = Nothing
            Select Case identifier
                Case "and", "or", "xor", "not", "mod", "like", "addressof", "is", "isnot", "andalso", "orelse", "typeof"
                    retval = TokenTypes.Operator
                Case "if", "then", "else", "elseif", "end", "while", "do", "loop", "until", "for", "each", "in", "to", "next", "imports",
                        "sub", "function", "dim", "as", "redim", "of", "gettype", "byref", "byval", "return", "nothing", "exit", "continue",
                        "throw", "try", "catch", "when", "finally", "readonly", "class", "enum", "new", "property", "get", "set",
                        "raiseevent", "event", "delegate", "addhandler", "removehandler", "handles",
                        "public", "private", "shared", "protected", "select", "case",
                        "inherits", "implements", "overrides", "mustoverride", "mustinherit"
                    retval = TokenTypes.Keyword
                Case Else
                    retval = TokenTypes.Identifier
            End Select
            Return retval
        End Function

    End Class

End Namespace
