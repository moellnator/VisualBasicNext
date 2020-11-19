Imports System.Collections.Immutable
Imports VisualBasicNext.CodeAnalysis.Parsing

Namespace Binding
    Friend Class BoundArrayAccessExpression : Inherits BoundExpression

        Friend Sub New(syntax As SyntaxNode, source As BoundExpression, index As IEnumerable(Of BoundExpression), elementType As Type)
            MyBase.New(syntax, BoundNodeKind.BoundArrayAccessExpression, elementType)
            Me.Source = source
            Me.Index = index.ToImmutableArray
        End Sub

        Public ReadOnly Property Source As BoundExpression
        Public ReadOnly Property Index As ImmutableArray(Of BoundExpression)

    End Class

End Namespace
