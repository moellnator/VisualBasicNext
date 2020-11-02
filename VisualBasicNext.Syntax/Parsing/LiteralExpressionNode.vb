Imports VisualBasicNext.Syntax.Lexing

Namespace Parsing
    Public Class LiteralExpressionNode : Inherits ExpressionNode

        Public ReadOnly Property LiteralToken As SyntaxToken

        Public Overrides ReadOnly Iterator Property Children As IEnumerable(Of SyntaxNode)
            Get
                Yield Me.LiteralToken
            End Get
        End Property

        Public Sub New(literalToken As SyntaxToken)
            MyBase.New(SyntaxKind.LiteralNode)
            Me.LiteralToken = literalToken
        End Sub

    End Class

End Namespace
