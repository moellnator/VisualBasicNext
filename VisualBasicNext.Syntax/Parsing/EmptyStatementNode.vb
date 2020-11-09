Imports VisualBasicNext.CodeAnalysis.Lexing

Namespace Parsing
    Public Class EmptyStatementNode : Inherits StatementNode

        Friend Sub New(endOfStatementToken As SyntaxToken)
            MyBase.New(SyntaxKind.EmptyStatementNode, endOfStatementToken)
        End Sub

    End Class

End Namespace
