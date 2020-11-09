Imports VisualBasicNext.CodeAnalysis.Lexing

Namespace Parsing
    Public Class ExtrapolatedStringSubNode : Inherits SyntaxNode

        Friend Sub New(expression As ExpressionNode, terminatorSyntax As SyntaxToken)
            MyBase.New(SyntaxKind.ExtrapolatedStringSubNode)
            Me.Expression = expression
            Me.TerminatorSyntax = terminatorSyntax
        End Sub

        Public Overrides ReadOnly Iterator Property Children As IEnumerable(Of SyntaxNode)
            Get
                Yield Me.Expression
                Yield Me.TerminatorSyntax
            End Get
        End Property

        Public ReadOnly Property Expression As ExpressionNode
        Public ReadOnly Property TerminatorSyntax As SyntaxToken

    End Class

End Namespace
