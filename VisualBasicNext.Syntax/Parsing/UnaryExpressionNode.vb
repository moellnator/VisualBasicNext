Imports VisualBasicNext.Syntax.Lexing

Namespace Parsing
    Public Class UnaryExpressionNode : Inherits ExpressionNode

        Public Sub New(operatorToken As SyntaxToken, right As ExpressionNode)
            MyBase.New(SyntaxKind.UnaryExpressionNode)
            Me.OperatorToken = operatorToken
            Me.Right = right
        End Sub

        Public Overrides ReadOnly Iterator Property Children As IEnumerable(Of SyntaxNode)
            Get
                Yield Me.OperatorToken
                Yield Me.Right
            End Get
        End Property

        Public ReadOnly Property OperatorToken As SyntaxToken
        Public ReadOnly Property Right As ExpressionNode

    End Class

End Namespace
