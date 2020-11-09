Imports System.Collections.Immutable
Imports VisualBasicNext.Syntax.Lexing

Namespace Parsing
    Public Class ExtrapolatedStringExpressionNode : Inherits ExpressionNode

        Public Sub New(partialStringStart As SyntaxToken, subnodes As IEnumerable(Of ExtrapolatedStringSubNode))
            MyBase.New(SyntaxKind.ExtrapolatedStringExpressionNode)
            Me.PartialStringStart = partialStringStart
            Me.Subnodes = subnodes.ToImmutableArray
        End Sub

        Public Overrides ReadOnly Iterator Property Children As IEnumerable(Of SyntaxNode)
            Get
                Yield Me.PartialStringStart
                For Each item As ExtrapolatedStringSubNode In Me.Subnodes
                    Yield item
                Next
            End Get
        End Property

        Public ReadOnly Property PartialStringStart As SyntaxToken
        Public ReadOnly Property Subnodes As ImmutableArray(Of ExtrapolatedStringSubNode)

    End Class

    Public Class ExtrapolatedStringSubNode : Inherits SyntaxNode

        Public Sub New(expression As ExpressionNode, terminatorSyntax As SyntaxToken)
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
