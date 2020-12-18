Imports System.Reflection
Imports VisualBasicNext.CodeAnalysis.Parsing

Namespace Binding
    Friend Class BoundCollectionConstructorExpression : Inherits BoundExpression

        Friend Sub New(
                      syntax As SyntaxNode,
                      type As Type,
                      ctor As ConstructorInfo,
                      arguments As BoundExpression(),
                      init As IEnumerable(Of Tuple(Of MethodInfo, BoundExpression())))
            MyBase.New(syntax, BoundNodeKind.BoundCollectionConstructorExpression, type)
            Me.Type = type
            Me.Ctor = ctor
            Me.Arguments = arguments
            Me.Init = init.ToArray
        End Sub

        Public ReadOnly Property Type As Type
        Public ReadOnly Property Ctor As ConstructorInfo
        Public ReadOnly Property Arguments As BoundExpression()
        Public ReadOnly Property Init As Tuple(Of MethodInfo, BoundExpression())()

    End Class

End Namespace