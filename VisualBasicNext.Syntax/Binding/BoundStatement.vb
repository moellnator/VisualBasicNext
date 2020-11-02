Imports VisualBasicNext.Syntax.Parsing

Namespace Binding
    Public MustInherit Class BoundStatement : Inherits BoundNode

        Protected Sub New(syntax As SyntaxNode, kind As BoundNodeKinds)
            MyBase.New(syntax, kind)
        End Sub

    End Class

End Namespace
