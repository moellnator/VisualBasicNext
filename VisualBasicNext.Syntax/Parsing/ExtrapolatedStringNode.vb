Imports System.Collections.Immutable
Imports VisualBasicNext.CodeAnalysis.Lexing

Namespace Parsing
    Public Class ExtrapolatedStringExpressionNode : Inherits ExpressionNode

        Friend Sub New(partialStringStart As SyntaxToken, subnodes As IEnumerable(Of ExtrapolatedStringSubNode))
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

End Namespace
