Imports System.Collections.Immutable
Imports VisualBasicNext.CodeAnalysis.Lexing

Namespace Parsing
    Public Class ArrayDimensionsListItemNode : Inherits SyntaxNode

        Friend Sub New(openBracketToken As SyntaxToken, delimeters As IEnumerable(Of SyntaxToken), closeBracketToken As SyntaxToken)
            MyBase.New(SyntaxKind.ArrayDimensionsListItemNode)
            Me.OpenBracketToken = openBracketToken
            Me.Delimeters = delimeters.ToImmutableArray
            Me.CloseBracketToken = closeBracketToken
        End Sub

        Public Overrides ReadOnly Iterator Property Children As IEnumerable(Of SyntaxNode)
            Get
                Yield Me.OpenBracketToken
                For Each d As SyntaxToken In Me.Delimeters
                    Yield d
                Next
                Yield CloseBracketToken
            End Get
        End Property

        Public ReadOnly Property OpenBracketToken As SyntaxToken
        Public ReadOnly Property Delimeters As ImmutableArray(Of SyntaxToken)
        Public ReadOnly Property CloseBracketToken As SyntaxToken

    End Class

End Namespace
