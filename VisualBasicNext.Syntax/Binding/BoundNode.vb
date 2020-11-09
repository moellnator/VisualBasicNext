Imports VisualBasicNext.Syntax.Parsing

Namespace Binding
    Public MustInherit Class BoundNode

        Protected Sub New(syntax As SyntaxNode, kind As BoundNodeKind)
            Me.Syntax = syntax
            Me.Kind = kind
        End Sub

        Public ReadOnly Property Syntax As SyntaxNode
        Public ReadOnly Property Kind As BoundNodeKind

    End Class

End Namespace
