Imports VisualBasicNext.Syntax.Lexing

Namespace Parsing
    Public Class VariableDeclarationStatementNode : Inherits StatementNode

        Public Sub New(dimKeywordToken As SyntaxToken,
                       identifierToken As SyntaxToken,
                       asKeyWordToken As SyntaxToken,
                       typename As TypeNameNode,
                       equalsToken As SyntaxToken,
                       expression As ExpressionNode,
                       endOfExpressionToken As SyntaxToken)
            MyBase.New(SyntaxKind.VariableDeclarationStatementNode, endOfExpressionToken)
            Me.DimKeywordToken = dimKeywordToken
            Me.IdentifierToken = identifierToken
            Me.AsKeyWordToken = asKeyWordToken
            Me.Typename = typename
            Me.EqualsToken = equalsToken
            Me.Expression = expression
        End Sub

        Public Overrides ReadOnly Iterator Property Children As IEnumerable(Of SyntaxNode)
            Get
                Yield Me.DimKeywordToken
                Yield Me.IdentifierToken
                If Not Me.AsKeyWordToken Is Nothing Then
                    Yield Me.AsKeyWordToken
                    Yield Me.Typename
                End If
                If Not Me.EqualsToken Is Nothing Then
                    Yield Me.EqualsToken
                    Yield Me.Expression
                End If
            End Get
        End Property

        Public ReadOnly Property DimKeywordToken As SyntaxToken
        Public ReadOnly Property IdentifierToken As SyntaxToken
        Public ReadOnly Property AsKeyWordToken As SyntaxToken
        Public ReadOnly Property Typename As TypeNameNode
        Public ReadOnly Property EqualsToken As SyntaxToken
        Public ReadOnly Property Expression As ExpressionNode

    End Class

End Namespace
