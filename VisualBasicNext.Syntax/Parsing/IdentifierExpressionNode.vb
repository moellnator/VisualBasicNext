Imports VisualBasicNext.Syntax.Lexing

Namespace Parsing
    Public Class IdentifierExpressionNode : Inherits ExpressionNode

        Public ReadOnly Property IdentifierToken As SyntaxToken

        Public Overrides ReadOnly Iterator Property Children As IEnumerable(Of SyntaxNode)
            Get
                Yield Me.IdentifierToken
            End Get
        End Property

        Public Sub New(identifier As SyntaxToken)
            MyBase.New(SyntaxKind.IdentifierExpressionNode)
            Me.IdentifierToken = identifier
        End Sub

    End Class

End Namespace
