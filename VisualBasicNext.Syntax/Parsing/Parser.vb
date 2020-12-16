Imports System.Collections.Immutable
Imports VisualBasicNext.CodeAnalysis.Diagnostics
Imports VisualBasicNext.CodeAnalysis.Lexing
Imports VisualBasicNext.CodeAnalysis.Text

Namespace Parsing
    Friend Class Parser

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
                'TODO [implementation] -> Assignment
                'TODO [implementation] -> Redim and Erase
                'TODO [implementation] -> Define Delegates
                'TODO [implementation] -> More Statements... 
                '        if, else, elseif, end if
                '        select case, case, case is, case else, end select
                '        for, each in, next
                '        while, end while
                '        do, until, while, loop
                Case SyntaxKind.EndOfLineToken
                    Return New EmptyStatementNode(Me._next_token)
                Case SyntaxKind.DimKeywordToken
                    Return Me._MatchVaraibleDeclarationStatement
                Case SyntaxKind.ImportsKeywordToken
                    Return Me._MatchImportStatement
                Case Else
                    Return Me._MatchExpressionStatement
            End Select
        End Function

        Private Function _MatchImportStatement() As ImportsStatementNode
            Dim keyword As SyntaxNode = Me._MatchToken(SyntaxKind.ImportsKeywordToken)
            Dim node As NamespaceNode = Me._MatchNamespace
            Return New ImportsStatementNode(keyword, node, Me._MatchEndOfStatement)
        End Function

        Private Function _MatchNamespace() As NamespaceNode
            Dim items As New List(Of NamespaceItemNode) From {Me._MatchNamespaceItem(True)}
            While Me._current.Kind = SyntaxKind.DotToken
                items.Add(Me._MatchNamespaceItem)
            End While
            Return New NamespaceNode(items.ToImmutableArray)
        End Function

        Private Function _MatchNamespaceItem(Optional isFirst As Boolean = False) As NamespaceItemNode
            Dim delimeter As SyntaxToken = If(isFirst, Nothing, Me._MatchToken(SyntaxKind.DotToken))
            Dim identifier As SyntaxToken = Me._MatchToken(SyntaxKind.IdentifierToken)
            Return New NamespaceItemNode(delimeter, identifier)
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
            'TODO [implementation] -> Lambda, MultilineLambda
            'TODO [implementation] -> Action, MultilineAction
            'TODO [implementation] -> New (including from and array notation)
            'TODO [implementation] -> TypeOf ... Is ...
            Return Me._MatchBinaryExpression
        End Function

        Private Function _MatchBinaryExpression(Optional parentPrecedence As Integer = 0) As ExpressionNode
            Dim left As ExpressionNode = Nothing
            Dim unary_operator_precedence As Integer = Me._current.Kind.GetUnaryOperatorPrecedence
            If unary_operator_precedence <> 0 And unary_operator_precedence >= parentPrecedence Then
                Dim operatorToken As SyntaxToken = Me._next_token
                Dim operand As ExpressionNode = Me._MatchBinaryExpression(unary_operator_precedence)
                left = New UnaryExpressionNode(operatorToken, operand)
            Else
                left = Me._MatchMemberAccessExpression
            End If
            While True
                Dim precedence As Integer = Me._current.Kind.GetBinaryOperatorPrecedence
                If precedence = 0 Or precedence <= parentPrecedence Then Exit While
                Dim opertorToken As SyntaxToken = Me._next_token
                Dim right As ExpressionNode = Me._MatchBinaryExpression(precedence)
                left = New BinaryExpressionNode(left, opertorToken, right)
            End While
            Return left
        End Function

        Private Function _MatchMemberAccessExpression() As ExpressionNode
            Dim left As ExpressionNode = Me._MatchAtomicAccessExpression
            Dim items As New List(Of MemberAccessItemNode)
            While (Me._current.Kind = SyntaxKind.DotToken Or Me._current.Kind = SyntaxKind.QuestionmarkDotToken)
                items.Add(Me._MatchMemberAccessItemExpression(False))
            End While
            If items.Count = 0 Then Return left
            Return New MemberAccessListNode(left, items.ToImmutableList)
        End Function

        Private Function _MatchMemberAccessItemExpression(Optional isFirst As Boolean = False) As ExpressionNode
            Dim delimeter As SyntaxToken = If(isFirst, Nothing, Me._MatchToken(Me._current.Kind))
            Dim identifier As SyntaxToken = Me._MatchToken(SyntaxKind.IdentifierToken)
            Dim generics As GenericsListNode = Nothing
            If Me._current.Kind = SyntaxKind.OpenBracketToken AndAlso Me._peek(1).Kind = SyntaxKind.OfKeywordToken Then generics = Me._MatchGenericsList()
            Dim access As AccessListNode = Nothing
            If Me._current.Kind = SyntaxKind.OpenBracketToken Or Me._current.Kind = SyntaxKind.QuestionmarkOpenBracketToken Then access = Me._MatchAccessList()
            Return New MemberAccessItemNode(delimeter, identifier, generics, access)
        End Function

        Private Function _MatchAccessList() As AccessListNode
            Dim items As New List(Of ArgumentListNode)
            While (Me._current.Kind = SyntaxKind.OpenBracketToken Or Me._current.Kind = SyntaxKind.QuestionmarkOpenBracketToken)
                items.Add(Me._MatchArgumentList)
            End While
            Return New AccessListNode(items.ToImmutableList)
        End Function

        Private Function _MatchArgumentList() As ArgumentListNode
            Dim openBracket As SyntaxToken = Me._MatchToken(
                If(
                    Me._current.Kind = SyntaxKind.OpenBracketToken,
                    SyntaxKind.OpenBracketToken,
                    SyntaxKind.QuestionmarkOpenBracketToken
                )
            )
            Dim arguments As New List(Of ArgumentNode)
            Dim closeBracket As SyntaxToken = Nothing
            If Me._current.Kind = SyntaxKind.CloseBracketToken Then
                closeBracket = Me._MatchToken(SyntaxKind.CloseBracketToken)
                If openBracket.Kind = SyntaxKind.QuestionmarkOpenBracketToken Then _
                    Me.Diagnostics.ReportMismatchingDimensions(0, 0, closeBracket.Span)
                Return New ArgumentListNode(openBracket, ImmutableList(Of ArgumentNode).Empty, closeBracket)
            End If
            arguments.Add(Me._MatchArgument(True))
            While Me._current.Kind = SyntaxKind.CommaToken
                arguments.Add(Me._MatchArgument)
            End While
            closeBracket = Me._MatchToken(SyntaxKind.CloseBracketToken)
            Return New ArgumentListNode(openBracket, arguments.ToImmutableList, closeBracket)
        End Function

        Private Function _MatchArgument(Optional isFirst As Boolean = False) As ArgumentNode
            Dim delimeter As SyntaxToken = If(isFirst, Nothing, Me._MatchToken(SyntaxKind.CommaToken))
            Dim argument As ExpressionNode = Me._MatchExpression
            Return New ArgumentNode(delimeter, argument)
        End Function

        Private Function _MatchAtomicAccessExpression() As ExpressionNode
            Dim atom As ExpressionNode = Me._MatchAtomicExpression
            If atom.Kind = SyntaxKind.MemberAccessItemNode Then Return atom
            Dim access As AccessListNode = Nothing
            If Me._current.Kind = SyntaxKind.OpenBracketToken Or Me._current.Kind = SyntaxKind.QuestionmarkOpenBracketToken Then access = Me._MatchAccessList()
            If access Is Nothing Then Return atom
            Return New MemberAccessItemNode(atom, access)
        End Function

        Private Function _MatchAtomicExpression() As ExpressionNode
            'TODO [suggestion] -> Add the AddressOf operator -> Delegates
            'TODO [suggestion] -> Add Await operator
            Select Case Me._current.Kind
                Case SyntaxKind.OpenBraceToken
                    Return Me._MatchArrayExpression
                Case SyntaxKind.OpenBracketToken
                    Return Me._MatchBlockExpression
                Case SyntaxKind.CTypeKeywordToken
                    Return Me._MatchCastExpression
                Case SyntaxKind.CTypeDynamicKeywordToken
                    Return Me._MatchCastDynamicExpression
                Case SyntaxKind.GetTryCastKeywordToken
                    Return Me._MatchTryCastExpression
                Case SyntaxKind.GetTypeKeywordToken
                    Return Me._MatchGetTypeExpression
                Case SyntaxKind.IfKeywordToken
                    Return Me._MatchIfExpression
                Case SyntaxKind.BoolValueToken
                    Return Me._MatchBooleanExpression
                Case SyntaxKind.NumberValueToken
                    Return Me._MatchNumberExpression
                Case SyntaxKind.StringValueToken
                    Return Me._MatchStringExpression
                Case SyntaxKind.DateValueToken
                    Return Me._MatchDateExpression
                Case SyntaxKind.NothingKeywordToken
                    Return Me._MatchNothingExpression
                Case SyntaxKind.PartialStringStartToken
                    Return Me._MatchExtrapolatedString
                Case Else
                    Return Me._MatchMemberAccessItemExpression(True)
            End Select
        End Function

        Private Function _MatchTryCastExpression() As TryCastExpressionNode
            Return New TryCastExpressionNode(
                Me._MatchToken(SyntaxKind.GetTryCastKeywordToken),
                Me._MatchToken(SyntaxKind.OpenBracketToken),
                Me._MatchExpression,
                Me._MatchToken(SyntaxKind.CommaToken),
                Me._MatchTypeName,
                Me._MatchToken(SyntaxKind.CloseBracketToken)
            )
        End Function

        Private Function _MatchExtrapolatedString() As ExtrapolatedStringExpressionNode
            Dim start As SyntaxToken = Me._MatchToken(SyntaxKind.PartialStringStartToken)
            Dim items As ImmutableArray(Of ExtrapolatedStringSubNode).Builder = ImmutableArray.CreateBuilder(Of ExtrapolatedStringSubNode)
            Dim done As Boolean = False
            While Not done
                Dim sub_expression As ExpressionNode = Me._MatchExpression
                Dim terminator As SyntaxToken
                Select Case Me._current.Kind
                    Case SyntaxKind.PartialStringCenterToken
                        terminator = Me._MatchToken(SyntaxKind.PartialStringCenterToken)
                    Case Else
                        terminator = Me._MatchToken(SyntaxKind.PartialStringEndToken)
                        done = True
                End Select
                items.Add(New ExtrapolatedStringSubNode(sub_expression, terminator))
            End While
            Return New ExtrapolatedStringExpressionNode(start, items.ToImmutableArray)
        End Function

        Private Function _MatchArrayExpression() As ArrayExpressionNode
            Dim openBrace As SyntaxToken = Me._MatchToken(SyntaxKind.OpenBraceToken)
            If Me._current.Kind <> SyntaxKind.CloseBraceToken Then
                Dim items As New List(Of ArrayItemNode) From {Me._MatchArrayItem(True)}
                While Me._current.Kind = SyntaxKind.CommaToken
                    items.Add(Me._MatchArrayItem)
                End While
                Return New ArrayExpressionNode(openBrace, items, Me._MatchToken(SyntaxKind.CloseBraceToken))
            Else
                Return New ArrayExpressionNode(openBrace, Array.Empty(Of ArrayItemNode), Me._MatchToken(SyntaxKind.CloseBraceToken))
            End If
        End Function

        Private Function _MatchArrayItem(Optional isFirst As Boolean = False) As ArrayItemNode
            Dim delimeter As SyntaxToken = If(isFirst, Nothing, Me._MatchToken(SyntaxKind.CommaToken))
            Dim item As ExpressionNode = Me._MatchExpression
            Return New ArrayItemNode(delimeter, item)
        End Function

        Private Function _MatchIfExpression() As ExpressionNode
            Dim ifKeyword As SyntaxToken = Me._MatchToken(SyntaxKind.IfKeywordToken)
            Dim openBracket As SyntaxToken = Me._MatchToken(SyntaxKind.OpenBracketToken)
            Dim condition As ExpressionNode = Me._MatchExpression
            Dim delimeter As SyntaxToken = Me._MatchToken(SyntaxKind.CommaToken)
            Dim trueExpression As ExpressionNode = Me._MatchExpression
            If Me._current.Kind = SyntaxKind.CommaToken Then
                Return New TernaryExpressionNode(
                    ifKeyword,
                    openBracket,
                    condition,
                    delimeter,
                    trueExpression,
                    Me._MatchToken(SyntaxKind.CommaToken),
                    Me._MatchExpression,
                    Me._MatchToken(SyntaxKind.CloseBracketToken)
                )
            Else
                Return New NullCheckExpressionNode(
                    ifKeyword,
                    openBracket,
                    condition,
                    delimeter,
                    trueExpression,
                    Me._MatchToken(SyntaxKind.CloseBracketToken)
                )
            End If
        End Function

        Private Function _MatchGetTypeExpression() As GetTypeExpressionNode
            Return New GetTypeExpressionNode(
                Me._MatchToken(SyntaxKind.GetTypeKeywordToken),
                Me._MatchToken(SyntaxKind.OpenBracketToken),
                Me._MatchTypeName,
                Me._MatchToken(SyntaxKind.CloseBracketToken)
            )
        End Function

        Private Function _MatchCastExpression() As CastExpressionNode
            Return New CastExpressionNode(
                Me._MatchToken(SyntaxKind.CTypeKeywordToken),
                Me._MatchToken(SyntaxKind.OpenBracketToken),
                Me._MatchExpression,
                Me._MatchToken(SyntaxKind.CommaToken),
                Me._MatchTypeName,
                Me._MatchToken(SyntaxKind.CloseBracketToken)
            )
        End Function

        Private Function _MatchCastDynamicExpression() As CastDynamicExpressionNode
            Return New CastDynamicExpressionNode(
                Me._MatchToken(SyntaxKind.CTypeDynamicKeywordToken),
                Me._MatchToken(SyntaxKind.OpenBracketToken),
                Me._MatchExpression,
                Me._MatchToken(SyntaxKind.CommaToken),
                Me._MatchExpression,
                Me._MatchToken(SyntaxKind.CloseBracketToken)
            )
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
            Return New LiteralExpressionNode(_MatchToken(SyntaxKind.NothingKeywordToken))
        End Function

        Private Function _MatchTypeName() As TypeNameNode
            Dim items As ImmutableArray(Of TypeNameItemNode).Builder = ImmutableArray.CreateBuilder(Of TypeNameItemNode)
            items.Add(_MatchTypenameItem(True))
            While Me._current.Kind = SyntaxKind.DotToken
                items.Add(_MatchTypenameItem)
            End While
            Dim nullable_token As SyntaxToken = If(Me._current.Kind = SyntaxKind.QuestionmarkToken, Me._MatchToken(SyntaxKind.QuestionmarkToken), Nothing)
            Dim array_dimensions As ArrayDimensionsListNode = Nothing
            If Me._current.Kind = SyntaxKind.OpenBracketToken Then array_dimensions = Me._MatchArrayDimensionsList
            Return New TypeNameNode(items.ToImmutableArray, nullable_token, array_dimensions)
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