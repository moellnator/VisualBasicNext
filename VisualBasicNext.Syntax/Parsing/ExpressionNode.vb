Imports VisualBasicNext.Syntax.Lexing

Namespace Parsing
    Public MustInherit Class ExpressionNode : Inherits SyntaxNode

        Protected Sub New(kind As SyntaxKind)
            MyBase.New(kind)
        End Sub

    End Class

End Namespace
