Imports System.Collections.Immutable
Imports VisualBasicNext.CodeAnalysis.Lexing

Namespace Parsing
    Public Class ScriptNode : Inherits SyntaxNode

        Public ReadOnly Property Statements As ImmutableArray(Of StatementNode)
        Public ReadOnly Property EndOfFileToken As SyntaxToken

        Public Overrides ReadOnly Iterator Property Children As IEnumerable(Of SyntaxNode)
            Get
                For Each s As StatementNode In Me.Statements
                    Yield s
                Next
                Yield Me.EndOfFileToken
            End Get
        End Property

        Friend Sub New(statements As IEnumerable(Of StatementNode), endOfFileToken As SyntaxToken)
            MyBase.New(SyntaxKind.ScriptNode)
            Me.Statements = statements.ToImmutableArray
            Me.EndOfFileToken = endOfFileToken
        End Sub

        Public Shared Function FromSource(source As Text.Source) As ScriptNode
            Dim parser As New Parser(source)
            Return parser.Parse
        End Function

    End Class

End Namespace
