Imports VisualBasicNext.CodeAnalysis.Lexing

Namespace Parsing
    Public Class ArgumentListNode : Inherits SyntaxNode

        Public Sub New(openBracket As SyntaxToken, items As IEnumerable(Of ArgumentNode), closeBracket As SyntaxToken)
            MyBase.New(SyntaxKind.ArgumentListNode)
            Me.OpenBracket = openBracket
            Me.Items = items.ToArray
            Me.CloseBracket = closeBracket
        End Sub

        Public Overrides ReadOnly Iterator Property Children As IEnumerable(Of SyntaxNode)
            Get
                Yield Me.OpenBracket
                For Each item In Me.Items
                    Yield item
                Next
                Yield Me.CloseBracket
            End Get
        End Property

        Public ReadOnly Property OpenBracket As SyntaxToken
        Public ReadOnly Property Items As ArgumentNode()
        Public ReadOnly Property CloseBracket As SyntaxToken
    End Class

End Namespace
