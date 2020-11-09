Imports System.Collections.Immutable
Imports VisualBasicNext.CodeAnalysis.Parsing

Namespace Binding
    Friend Class BoundArrayExpression : Inherits BoundExpression

        Public Sub New(syntaxNode As SyntaxNode, items As IEnumerable(Of BoundExpression), boundType As Type, Optional rank As Integer = 1)
            MyBase.New(syntaxNode, BoundNodeKind.BoundArrayExpression, boundType)
            Me.Items = items.ToImmutableArray
            Me.Rank = rank
        End Sub

        Public ReadOnly Property Items As ImmutableArray(Of BoundExpression)
        Public ReadOnly Property Rank As Integer

    End Class

End Namespace
