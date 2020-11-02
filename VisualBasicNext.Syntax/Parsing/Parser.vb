Imports System.Collections.Immutable
Imports VisualBasicNext.Syntax.Diagnostics
Imports VisualBasicNext.Syntax.Lexing
Imports VisualBasicNext.Syntax.Text

Namespace Parsing
    Public Class Parser

        Public ReadOnly Property Diagnostics As ErrorList
        Private ReadOnly _tokens As ImmutableArray(Of SyntaxToken)
        Private _position As Integer = 0

        Public Sub New(source As Source)
            Dim lexer As New Lexer(source)
            Me._tokens = lexer.Where(
            Function(t) t.Kind <> SyntaxKind.WhiteSpaceToken And
                        t.Kind <> SyntaxKind.CommentToken And
                        t.Kind <> SyntaxKind.BadToken
            ).ToImmutableArray
            Me.Diagnostics = New ErrorList(lexer.Diagnostics)
        End Sub

        Public Sub New(text As String)
            Me.New(Source.FromText(text))
        End Sub

        Private Function _peek(offset As Integer) As SyntaxToken
            Dim index As Integer = Me._position + offset
            If index >= Me._tokens.Length Then Return Me._tokens.Last
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
                'TODO: Add statements - import, assignment, ...
                Case SyntaxKind.EndOfLineToken
                    Return New EmptyStatementNode(Me._next_token)
                Case SyntaxKind.DimKeywordToken
                    Return Me._MatchVaraibleDeclarationStatement
                Case Else
                    Return Me._MatchExpressionStatement
            End Select
        End Function

        Private Function _MatchVaraibleDeclarationStatement() As StatementNode
            Dim dim_keyword As SyntaxToken = Me._MatchToken(SyntaxKind.DimKeywordToken)
            Dim identifier As SyntaxToken = Me._MatchToken(SyntaxKind.IdentifierToken)
            Dim as_token As SyntaxToken = Nothing
            Dim typename As TypeNameNode = Nothing
            If Me._current.Kind = SyntaxKind.AsKeywordToken Then
                as_token = Me._MatchToken(SyntaxKind.AsKeywordToken)
                typename = Me._MatchTypeName
            End If
            Dim equals_token As SyntaxToken = Nothing
            Dim expression As ExpressionNode = Nothing
            If Me._current.Kind = SyntaxKind.EqualsToken Then
                equals_token = Me._MatchToken(SyntaxKind.EqualsToken)
                expression = Me._MatchExpression
            End If
            Return New VariableDeclarationStatementNode(
                dim_keyword,
                identifier,
                as_token,
                typename,
                equals_token,
                expression,
                Me._MatchEndOfStatement
            )
        End Function

        Private Function _MatchExpressionStatement() As StatementNode
            Return New ExpressionStatementNode(Me._MatchExpression, Me._MatchEndOfStatement)
        End Function

        Private Function _MatchEndOfStatement() As SyntaxToken
            Dim endOfStatement As SyntaxToken = If(
                Me._current.Kind = SyntaxKind.EndOfSourceToken,
                Nothing,
                Me._MatchToken(SyntaxKind.EndOfLineToken)
            )
            Return endOfStatement
        End Function

        Private Function _MatchExpression() As ExpressionNode
            'TODO: Binary, Unary, Lambda, MultilineLambda, Action, MultilineAction, Member, New, TypeOf
            Return Me._MatchAtomicExpression
        End Function

        Private Function _MatchAtomicExpression() As ExpressionNode
            Select Case Me._current.Kind
                'TODO:  typecast, ternary, nullcheck, array, gettype 
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
                    Dim identifier As SyntaxToken = Me._MatchToken(SyntaxKind.IdentifierToken)
                    'TODO -> Match full quallifiers!
                    Return New VariableExpressionNode(identifier)
            End Select
        End Function

        Private Function _MatchBlockExpression() As BlockExpressionNode
            Return New BlockExpressionNode(
                Me._MatchToken(SyntaxKind.OpenBracketToken),
                Me._MatchExpression,
                Me._MatchToken(SyntaxKind.CloseBracketToken)
            )
        End Function

        Private Function _MatchBooleanExpression() As LiteralExpressionNode
            Return New LiteralExpressionNode(_MatchToken(SyntaxKind.BoolValueToken))
        End Function

        Private Function _MatchStringExpression() As LiteralExpressionNode
            Return New LiteralExpressionNode(_MatchToken(SyntaxKind.StringValueToken))
        End Function

        Private Function _MatchNumberExpression() As LiteralExpressionNode
            Return New LiteralExpressionNode(_MatchToken(SyntaxKind.NumberValueToken))
        End Function

        Private Function _MatchDateExpression() As LiteralExpressionNode
            Return New LiteralExpressionNode(_MatchToken(SyntaxKind.DateValueToken))
        End Function

        Private Function _MatchNothingExpression() As LiteralExpressionNode
            Return New LiteralExpressionNode(_MatchToken(SyntaxKind.NothingValueToken))
        End Function

        Private Function _MatchTypeName() As TypeNameNode
            Dim items As ImmutableArray(Of TypeNameItemNode).Builder = ImmutableArray.CreateBuilder(Of TypeNameItemNode)
            items.Add(_MatchTypenameItem(True))
            While Me._current.Kind = SyntaxKind.DotToken
                items.Add(_MatchTypenameItem)
            End While
            Dim array_dimensions As ArrayDimensionsListNode = Nothing
            If Me._current.Kind = SyntaxKind.OpenBracketToken Then array_dimensions = Me._MatchArrayDimensionsList
            Return New TypeNameNode(items.ToImmutableArray, array_dimensions)
        End Function

        Private Function _MatchTypenameItem(Optional isFirst As Boolean = False) As TypeNameItemNode
            Dim delimeter As SyntaxToken = If(isFirst, Nothing, Me._MatchToken(SyntaxKind.DotToken))
            Dim identifier As SyntaxToken = Me._MatchToken(SyntaxKind.IdentifierToken)
            Dim generics As GenericsListNode = Nothing
            If Me._current.Kind = SyntaxKind.OpenBracketToken And Me._peek(1).Kind = SyntaxKind.OfKeywordToken Then generics = Me._MatchGenericsList
            Return New TypeNameItemNode(delimeter, identifier, generics)
        End Function

        Private Function _MatchGenericsList() As GenericsListNode
            Dim open_bracket As SyntaxToken = Me._MatchToken(SyntaxKind.OpenBracketToken)
            Dim of_keyword As SyntaxToken = Me._MatchToken(SyntaxKind.OfKeywordToken)
            Dim items As ImmutableArray(Of GenericsListItemNode).Builder = ImmutableArray.CreateBuilder(Of GenericsListItemNode)
            items.Add(Me._MatchGenericsListItem(True))
            While Me._current.Kind = SyntaxKind.CommaToken
                items.Add(Me._MatchGenericsListItem)
            End While
            Dim close_bracket As SyntaxToken = Me._MatchToken(SyntaxKind.CloseBracketToken)
            Return New GenericsListNode(open_bracket, of_keyword, items.ToImmutableArray, close_bracket)
        End Function

        Private Function _MatchGenericsListItem(Optional isFirst As Boolean = False) As GenericsListItemNode
            Dim delimeter As SyntaxToken = If(isFirst, Nothing, Me._MatchToken(SyntaxKind.CommaToken))
            Dim type_name As TypeNameNode = Me._MatchTypeName
            Return New GenericsListItemNode(delimeter, type_name)
        End Function

        Private Function _MatchArrayDimensionsList() As ArrayDimensionsListNode
            Dim items As ImmutableArray(Of ArrayDimensionsListItemNode).Builder = ImmutableArray.CreateBuilder(Of ArrayDimensionsListItemNode)
            While Me._current.Kind = SyntaxKind.OpenBracketToken
                items.Add(Me._MatchArrayDimensionsListItem)
            End While
            Return New ArrayDimensionsListNode(items.ToImmutableArray)
        End Function

        Private Function _MatchArrayDimensionsListItem() As ArrayDimensionsListItemNode
            Dim open_bracket As SyntaxToken = Me._MatchToken(SyntaxKind.OpenBracketToken)
            Dim items As ImmutableArray(Of SyntaxToken).Builder = ImmutableArray.CreateBuilder(Of SyntaxToken)
            While Me._current.Kind = SyntaxKind.CommaToken
                items.Add(Me._MatchToken(SyntaxKind.CommaToken))
            End While
            Dim close_bracket As SyntaxToken = Me._MatchToken(SyntaxKind.CloseBracketToken)
            Return New ArrayDimensionsListItemNode(open_bracket, items.ToImmutableArray, close_bracket)
        End Function

    End Class

End Namespace