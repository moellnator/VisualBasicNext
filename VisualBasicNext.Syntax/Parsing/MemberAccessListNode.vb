Imports VisualBasicNext.CodeAnalysis.Lexing

Namespace Parsing
    Public Class MemberAccessListNode : Inherits ExpressionNode

        Public Sub New(source As ExpressionNode, items As IEnumerable(Of MemberAccessItemNode))
            MyBase.New(SyntaxKind.MemberAccessListNode)
            Me.Source = source
            Me.Items = items.ToArray
        End Sub

        Public Overrides ReadOnly Iterator Property Children As IEnumerable(Of SyntaxNode)
            Get
                Yield Me.Source
                For Each item As MemberAccessItemNode In Me.Items
                    Yield item
                Next
            End Get
        End Property

        Public ReadOnly Property Source As ExpressionNode
        Public ReadOnly Property Items As MemberAccessItemNode()

    End Class

End Namespace
