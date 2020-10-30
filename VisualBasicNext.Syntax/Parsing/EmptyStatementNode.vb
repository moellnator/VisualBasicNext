Imports VisualBasicNext.Syntax.Lexing

Namespace Parsing
    Public Class EmptyStatementNode : Inherits StatementNode

        Public Sub New(endOfStatementToken As SyntaxToken)
            MyBase.New(SyntaxKind.EmptyStatementNode, endOfStatementToken)
        End Sub

    End Class

End Namespace
