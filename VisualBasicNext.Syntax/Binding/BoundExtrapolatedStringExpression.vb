Imports System.Collections.Immutable
Imports VisualBasicNext.CodeAnalysis.Parsing

Namespace Binding
    Friend Class BoundExtrapolatedStringExpression : Inherits BoundExpression

        Public Sub New(syntax As SyntaxNode, start As String, expressions As IEnumerable(Of BoundExpression), remainders As IEnumerable(Of String))
            MyBase.New(syntax, BoundNodeKind.BoundExtrapolatedStringExpression, GetType(String))
            Me.Start = start
            Me.Expressions = expressions.ToImmutableArray
            Me.Remainders = remainders.ToImmutableArray
        End Sub

        Public ReadOnly Property Start As String
        Public ReadOnly Property Expressions As ImmutableArray(Of BoundExpression)
        Public ReadOnly Property Remainders As ImmutableArray(Of String)

    End Class

End Namespace
