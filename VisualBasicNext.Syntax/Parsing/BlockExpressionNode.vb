Imports VisualBasicNext.CodeAnalysis.Lexing

Namespace Parsing
    Public Class BlockExpressionNode : Inherits ExpressionNode

        Public ReadOnly Property OpenBracketToken As SyntaxToken
        Public ReadOnly Property Expression As ExpressionNode
        Public ReadOnly Property ClosingBracketToken As SyntaxToken

        Public Overrides ReadOnly Iterator Property Children As IEnumerable(Of SyntaxNode)
            Get
                Yield Me.OpenBracketToken
                Yield Me.Expression
                Yield Me.ClosingBracketToken
            End Get
        End Property

        Friend Sub New(openBracketToken As SyntaxToken, expression As ExpressionNode, closingBracketToken As SyntaxToken)
            MyBase.New(SyntaxKind.BlockExpressionNode)
            Me.OpenBracketToken = openBracketToken
            Me.Expression = expression
            Me.ClosingBracketToken = closingBracketToken
        End Sub

    End Class

End Namespace
