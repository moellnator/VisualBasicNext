Imports System.Collections.Immutable
Imports VisualBasicNext.Syntax.Lexing

Namespace Parsing
    Public Class GenericsListNode : Inherits SyntaxNode

        Public Sub New(openBracketToken As SyntaxToken, ofKeyWordToken As SyntaxToken, listItems As IEnumerable(Of GenericsListItemNode), closeBracketToken As SyntaxToken)
            MyBase.New(SyntaxKind.GenericsListNode)
            Me.OpenBracketToken = openBracketToken
            Me.OfKeyWordToken = ofKeyWordToken
            Me.CloseBracketToken = closeBracketToken
            Me.ListItems = listItems.ToImmutableArray
        End Sub

        Public Overrides ReadOnly Iterator Property Children As IEnumerable(Of SyntaxNode)
            Get
                Yield Me.OpenBracketToken
                Yield Me.OfKeyWordToken
                For Each i As GenericsListItemNode In Me.ListItems
                    Yield i
                Next
                Yield Me.CloseBracketToken
            End Get
        End Property

        Public ReadOnly Property OpenBracketToken As SyntaxToken
        Public ReadOnly Property OfKeyWordToken As SyntaxToken
        Public ReadOnly Property ListItems As ImmutableArray(Of GenericsListItemNode)
        Public ReadOnly Property CloseBracketToken As SyntaxToken

    End Class

End Namespace
