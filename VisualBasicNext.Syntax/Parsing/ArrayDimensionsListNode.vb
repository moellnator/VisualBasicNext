Imports System.Collections.Immutable
Imports VisualBasicNext.Syntax.Lexing

Namespace Parsing
    Public Class ArrayDimensionsListNode : Inherits SyntaxNode

        Public Sub New(listItems As IEnumerable(Of ArrayDimensionsListItemNode))
            MyBase.New(SyntaxKind.ArrayDimensionsListNode)
            Me.ListItems = listItems.ToImmutableArray
        End Sub

        Public Overrides ReadOnly Property Children As IEnumerable(Of SyntaxNode)
            Get
                Return Me.ListItems
            End Get
        End Property

        Public ReadOnly Property ListItems As ImmutableArray(Of ArrayDimensionsListItemNode)

    End Class

End Namespace
