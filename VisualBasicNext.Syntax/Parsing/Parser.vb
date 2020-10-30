Imports System.Collections.Immutable
Imports VisualBasicNext.Syntax.Diagnostics
Imports VisualBasicNext.Syntax.Lexing
Imports VisualBasicNext.Syntax.Text

Namespace Parsing
    Public Class Parser

        Public ReadOnly Property Diagnostics As ErrorList
        Public ReadOnly Property Text As Source
        Private ReadOnly _tokens As ImmutableArray(Of SyntaxToken)
        Private _position As Integer = 0

        Public Sub New(text As String)
            Dim source As Source = Source.FromText(text)
            Me.Text = source
            Dim lexer As New Lexer(source)
            Me._tokens = lexer.Where(
            Function(t) t.Kind <> SyntaxKind.WhiteSpaceToken And
                        t.Kind <> SyntaxKind.CommentToken And
                        t.Kind <> SyntaxKind.BadToken
            ).ToImmutableArray
            Me.Diagnostics = New ErrorList(lexer.Diagnostics)
        End Sub

        Private Function _peek(offset As Integer) As SyntaxToken
            Dim index As Integer = Me._position + offset
            If index > Me._tokens.Length Then Return Me._tokens.Last
            Return Me._tokens(index)
        End Function

        Private Function _current() As SyntaxToken
            Return Me._peek(0)
        End Function

        Private Function _next_token() As SyntaxToken
            Dim retval As SyntaxToken = Me._current
            Me._position += 1
            Return retval
        End Function

        Private Function _MatchToken(kind As SyntaxKind) As SyntaxToken
            If Me._current.Kind = kind Then Return Me._next_token
            Me.Diagnostics.ReportUnexpectedToken(kind, Me._current.Kind, Me._current.Span)
            Return New SyntaxToken(kind, Me._current.Span, Nothing)
        End Function

        Public Function Parse() As ScriptNode
            Return New ScriptNode(Me._MatchStatements, Me._MatchToken(SyntaxKind.EndOfSourceToken))
        End Function

        Private Function _MatchStatements() As ImmutableArray(Of StatementNode)
            Dim retval As ImmutableArray(Of StatementNode).Builder = ImmutableArray.CreateBuilder(Of StatementNode)
            While Not Me._current.Kind = SyntaxKind.EndOfSourceToken
                Dim start As SyntaxToken = Me._current
                Dim statement As StatementNode = Me._MatchStatement
                retval.Add(statement)
                If start.Equals(Me._current) Then Me._next_token()
            End While
            Return retval.ToImmutableArray
        End Function

        Private Function _MatchStatement() As StatementNode
            Select Case Me._current.Kind
                'TODO: Add statements - declaration, assignment, ...
                Case SyntaxKind.EndOfLineToken
                    Return New EmptyStatementNode(Me._next_token)
                Case Else
                    Return Me._MatchExpressionStatement
            End Select
        End Function

        Private Function _MatchExpressionStatement() As StatementNode
            Dim expression As ExpressionNode = Me._MatchExpression
            Dim endOfStatement As SyntaxToken = If(
                Me._current.Kind = SyntaxKind.EndOfSourceToken,
                New SyntaxToken(SyntaxKind.EndOfLineToken, New Span(expression.Span.Source, expression.Span.End, 0)),
                Me._MatchToken(SyntaxKind.EndOfLineToken)
            )
            Return New ExpressionStatementNode(expression, endOfStatement)
        End Function

        Private Function _MatchExpression() As ExpressionNode
            'TODO: Binary, Unary, Lambda, MultilineLambda, Action, MultilineAction, Member, New, TypeOf
            Return Me._MatchAtomicExpression
        End Function

        Private Function _MatchAtomicExpression() As ExpressionNode
            Select Case Me._current.Kind
                'TODO:  typecast, typeidentifier, ternary, array 
                Case SyntaxKind.OpenBracketToken
                    Return Me._MatchBlockExpression
                Case SyntaxKind.BoolValueToken
                    Return Me._MatchBooleanExpression
                Case SyntaxKind.NumberValueToken
                    Return Me._MatchNumberExpression
                Case SyntaxKind.StringValueToken
                    Return Me._MatchStringExpression
                Case SyntaxKind.DateValueToken
                    Return Me._MatchDateExpression
                Case SyntaxKind.NothingValueToken
                    Return Me._MatchNothingExpression
                Case Else
                    Return Me._MatchIdentifierExpression()
            End Select
        End Function

        Private Function _MatchBlockExpression() As BlockExpressionNode
            Return New BlockExpressionNode(
                Me._MatchToken(SyntaxKind.OpenBracketToken),
                Me._MatchExpression,
                Me._MatchToken(SyntaxKind.CloseBracketToken)
            )
        End Function

        Private Function _MatchBooleanExpression() As LiteralNode
            Return New LiteralNode(_MatchToken(SyntaxKind.BoolValueToken))
        End Function

        Private Function _MatchStringExpression() As LiteralNode
            Return New LiteralNode(_MatchToken(SyntaxKind.StringValueToken))
        End Function

        Private Function _MatchNumberExpression() As LiteralNode
            Return New LiteralNode(_MatchToken(SyntaxKind.NumberValueToken))
        End Function

        Private Function _MatchDateExpression() As LiteralNode
            Return New LiteralNode(_MatchToken(SyntaxKind.DateValueToken))
        End Function

        Private Function _MatchNothingExpression() As LiteralNode
            Return New LiteralNode(_MatchToken(SyntaxKind.NothingValueToken))
        End Function

        Private Function _MatchIdentifierExpression() As IdentifierExpressionNode
            Return New IdentifierExpressionNode(Me._MatchToken(SyntaxKind.IdentifierToken))
        End Function

    End Class

End Namespace