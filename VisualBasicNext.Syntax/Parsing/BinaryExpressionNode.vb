Imports VisualBasicNext.CodeAnalysis.Lexing

Namespace Parsing
    Public Class BinaryExpressionNode : Inherits ExpressionNode

        Friend Sub New(left As ExpressionNode, operatorToken As SyntaxToken, right As ExpressionNode)
            MyBase.New(SyntaxKind.BinaryExpressionNode)
            Me.Left = left
            Me.OperatorToken = operatorToken
            Me.Right = right
        End Sub

        Public Overrides ReadOnly Iterator Property Children As IEnumerable(Of SyntaxNode)
            Get
                Yield Me.Left
                Yield Me.OperatorToken
                Yield Me.Right
            End Get
        End Property

        Public ReadOnly Property Left As ExpressionNode
        Public ReadOnly Property OperatorToken As SyntaxToken
        Public ReadOnly Property Right As ExpressionNode
    End Class

End Namespace
