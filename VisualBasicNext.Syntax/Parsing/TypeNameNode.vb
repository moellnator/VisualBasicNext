Imports System.Collections.Immutable
Imports VisualBasicNext.Syntax.Lexing

Namespace Parsing
    Public Class TypeNameNode : Inherits SyntaxNode

        Public Sub New(items As IEnumerable(Of TypeNameItemNode), arrayDimensions As ArrayDimensionsListNode)
            MyBase.New(SyntaxKind.TypeNameNode)
            Me.Items = items.ToImmutableArray
            Me.ArrayDimensions = arrayDimensions
        End Sub

        Public Overrides ReadOnly Iterator Property Children As IEnumerable(Of SyntaxNode)
            Get
                For Each i As TypeNameItemNode In Me.Items
                    Yield i
                Next
                If Not ArrayDimensions Is Nothing Then Yield ArrayDimensions
            End Get
        End Property

        Public ReadOnly Property Items As ImmutableArray(Of TypeNameItemNode)
        Public ReadOnly Property ArrayDimensions As ArrayDimensionsListNode

        Public ReadOnly Property HasArrayDimensions As Boolean
            Get
                Return If(Me.ArrayDimensions IsNot Nothing, Me.ArrayDimensions.ListItems.Any(), False)
            End Get
        End Property

    End Class

End Namespace
