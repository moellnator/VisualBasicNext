Imports VisualBasicNext.Syntax.Parsing

Namespace Binding
    Public MustInherit Class BoundNode

        Protected Sub New(syntax As SyntaxNode, kind As BoundNodeKinds)
            Me.Syntax = syntax
            Me.Kind = kind
        End Sub

        Public ReadOnly Property Syntax As SyntaxNode
        Public ReadOnly Property Kind As BoundNodeKinds

    End Class

End Namespace
