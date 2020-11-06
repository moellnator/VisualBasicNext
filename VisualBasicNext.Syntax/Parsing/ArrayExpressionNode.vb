Imports System.Collections.Immutable
Imports VisualBasicNext.Syntax.Lexing

Namespace Parsing
    Public Class ArrayExpressionNode : Inherits ExpressionNode

        Public Sub New(openBrace As SyntaxToken, items As IEnumerable(Of ArrayItemNode), closeBrace As SyntaxToken)
            MyBase.New(SyntaxKind.ArrayExpressionNode)
            Me.OpenBrace = openBrace
            Me.Items = items.ToImmutableArray
            Me.CloseBrace = closeBrace
        End Sub

        Public Overrides ReadOnly Iterator Property Children As IEnumerable(Of SyntaxNode)
            Get
                Yield Me.OpenBrace
                For Each item As ArrayItemNode In Me.Items
                    Yield item
                Next
                Yield Me.CloseBrace
            End Get
        End Property

        Public ReadOnly Property OpenBrace As SyntaxToken
        Public ReadOnly Property Items As ImmutableArray(Of ArrayItemNode)
        Public ReadOnly Property CloseBrace As SyntaxToken
    End Class

End Namespace