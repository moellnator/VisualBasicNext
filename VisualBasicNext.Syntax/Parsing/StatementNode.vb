Imports VisualBasicNext.Syntax.Lexing

Namespace Parsing
    Public MustInherit Class StatementNode : Inherits SyntaxNode

        Public ReadOnly Property EndOfStatementToken As SyntaxToken

        Public Overrides ReadOnly Iterator Property Children As IEnumerable(Of SyntaxNode)
            Get
                Yield Me.EndOfStatementToken
            End Get
        End Property

        Protected Sub New(kind As SyntaxKind, endOfStatementToken As SyntaxToken)
            MyBase.New(kind)
            Me.EndOfStatementToken = endOfStatementToken
        End Sub

    End Class

End Namespace
