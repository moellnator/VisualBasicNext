Imports VisualBasicNext.CodeAnalysis.Lexing

Namespace Parsing
    Public Class AccessListNode : Inherits SyntaxNode

        Public Sub New(items As IEnumerable(Of ArgumentListNode))
            MyBase.New(SyntaxKind.AccessListNode)
            Me.Items = items.ToArray
        End Sub

        Public Overrides ReadOnly Iterator Property Children As IEnumerable(Of SyntaxNode)
            Get
                For Each item As ArgumentListNode In Me.Items
                    Yield item
                Next
            End Get
        End Property

        Public ReadOnly Property Items As ArgumentListNode()

    End Class

End Namespace
