Imports VisualBasicNext.CodeAnalysis.Parsing

Namespace Binding
    Friend MustInherit Class BoundStatement : Inherits BoundNode

        Protected Sub New(syntax As SyntaxNode, kind As BoundNodeKind)
            MyBase.New(syntax, kind)
        End Sub

    End Class

End Namespace
