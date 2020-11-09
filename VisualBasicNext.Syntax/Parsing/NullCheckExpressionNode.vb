Imports VisualBasicNext.CodeAnalysis.Lexing

Namespace Parsing
    Public Class NullCheckExpressionNode : Inherits ExpressionNode

        Friend Sub New(ifToken As SyntaxToken,
                       openBracket As SyntaxToken,
                       expression As ExpressionNode,
                       delimeter As SyntaxToken,
                       fallbackExpression As ExpressionNode,
                       closeBracket As SyntaxToken)
            MyBase.New(SyntaxKind.NullCheckExpression)
            Me.IfToken = ifToken
            Me.OpenBracket = openBracket
            Me.Expression = expression
            Me.Delimeter = delimeter
            Me.FallbackExpression = fallbackExpression
            Me.CloseBracket = closeBracket
        End Sub

        Public Overrides ReadOnly Iterator Property Children As IEnumerable(Of SyntaxNode)
            Get
                Yield Me.IfToken
                Yield Me.OpenBracket
                Yield Me.Expression
                Yield Me.Delimeter
                Yield Me.FallbackExpression
                Yield Me.CloseBracket
            End Get
        End Property

        Public ReadOnly Property IfToken As SyntaxToken
        Public ReadOnly Property OpenBracket As SyntaxToken
        Public ReadOnly Property Expression As ExpressionNode
        Public ReadOnly Property Delimeter As SyntaxToken
        Public ReadOnly Property FallbackExpression As ExpressionNode
        Public ReadOnly Property CloseBracket As SyntaxToken

    End Class

End Namespace