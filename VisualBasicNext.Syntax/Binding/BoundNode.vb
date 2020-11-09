Imports VisualBasicNext.CodeAnalysis.Parsing

Namespace Binding
    Friend MustInherit Class BoundNode

        Protected Sub New(syntax As SyntaxNode, kind As BoundNodeKind)
            Me.Syntax = syntax
            Me.Kind = kind
        End Sub

        Public ReadOnly Property Syntax As SyntaxNode
        Public ReadOnly Property Kind As BoundNodeKind

    End Class

End Namespace
