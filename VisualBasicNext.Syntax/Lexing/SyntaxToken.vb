Imports VisualBasicNext.Syntax.Parsing
Imports VisualBasicNext.Syntax.Text

Namespace Lexing

    Public Enum SyntaxKind
        BadToken = -1
        WhiteSpaceToken = 0
        EndOfSourceToken
        EndOfLineToken
        PlusEqualsToken
        CommentToken
        StringValueToken
        NumberValueToken
        DateValueToken
        QuestionmarkToken
        DotToken
        QuestionmarkDotToken
        CommaToken
        OpenBracketToken
        CloseBracketToken
        OpenBraceToken
        CloseBraceToken
        Questionmark
        EqualsToken
        PlusToken
        MinusEqualsToken
        MinusToken
        StarToken
        StarEqualsToken
        SlashEqualsToken
        SlashToken
        BackslashEqualsToken
        AmpersandToken
        AmpersandEqualsToken
        BackslashToken
        CircumflexEqualsToken
        CircumflexToken
        GreatethanToken
        CircuamflexToken
        GreaterToken
        GreaterGreaterToken
        GreaterGreaterEqualsToken
        GreaterEqualsToken
        LowerLowerEqualsToken
        LowerLowerToken
        LowerEqualsToken
        LowerGreaterToken
        LowerToken
        IdentifierToken
        BoolValueToken
        NothingValueToken
        NotKeywordToken
        OrKeywordToken
        XorKeywordToken
        ModKeywordToken
        AndAlsoKeywordToken
        OrElseKeywordToken
        IsKeywordToken
        IsNotKeywordToken
        LikeKeywordToken
        IfKeywordToken
        TypeOfKeywordToken
        NewKeywordToken
        CTypeKeywordToken
        AsKeywordToken
        OfKeywordToken
        FunctionKeywordToken
        SubKeywordToken
        LiteralNode
        ScriptNode
        ExpressionStatementNode
        EmptyStatementNode
        BlockExpressionNode
        TypeNameNode
        TypeNameItemNode
        GenericsListNode
        GenericListItemNode
        ArrayDimensionsListNode
        ArrayDimensionsListItemNode
        VariableDeclarationStatementNode
        DimKeywordToken
        VariableExpressionNode
    End Enum

    Public Class SyntaxToken : Inherits SyntaxNode

        Private ReadOnly _token_span As Span
        Public ReadOnly Property Value As Object = Nothing

        Public Overrides ReadOnly Property Children As IEnumerable(Of SyntaxNode)
            Get
                Return Enumerable.Empty(Of SyntaxNode)
            End Get
        End Property

        Public Overrides ReadOnly Property Span As Span
            Get
                Return Me._token_span
            End Get
        End Property

        Public Sub New(tokenType As SyntaxKind, span As Span, Optional value As Object = Nothing)
            MyBase.New(tokenType)
            Me._token_span = span
            Me.Value = value
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Me.Kind.ToString} ({If(Value IsNot Nothing, "<" & Value.GetType.ToString & "> " & Value.ToString, "")})"
        End Function

    End Class

End Namespace
