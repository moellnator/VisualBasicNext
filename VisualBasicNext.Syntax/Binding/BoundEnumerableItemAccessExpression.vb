Imports System.Reflection
Imports VisualBasicNext.CodeAnalysis.Parsing

Namespace Binding
    Friend Class BoundEnumerableItemAccessExpression : Inherits BoundExpression

        Friend Sub New(syntax As SyntaxNode, source As BoundExpression, index As BoundExpression, elementType As Type)
            MyBase.New(syntax, BoundNodeKind.BoundEnumerableItemAccessExpression, elementType)
            Me.Source = source
            Me.Index = index
            Dim acc As MethodInfo = GetType(Enumerable).GetMethod("ElementAtOrDefault").MakeGenericMethod(elementType)
            Me.AccessMethod = Function(obj As IEnumerable, ind As Integer) acc.Invoke(Nothing, {obj, ind})
        End Sub

        Public ReadOnly Property Source As BoundExpression
        Public ReadOnly Property Index As BoundExpression
        Public ReadOnly Property AccessMethod As Func(Of IEnumerable, Integer, Object)

    End Class

End Namespace
