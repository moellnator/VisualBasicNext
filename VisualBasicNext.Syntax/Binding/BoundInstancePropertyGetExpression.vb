Imports System.Collections.Immutable
Imports System.Reflection
Imports VisualBasicNext.CodeAnalysis.Parsing

Namespace Binding
    Friend Class BoundInstancePropertyGetExpression : Inherits BoundExpression

        Public Sub New(syntax As SyntaxNode, source As BoundExpression, member As PropertyInfo, arguments As IEnumerable(Of BoundExpression))
            MyBase.New(syntax, BoundNodeKind.BoundInstancePropertyGetExpression, member.PropertyType)
            Me.Source = source
            Me.Member = member
            Me.Arguments = arguments.ToImmutableArray
        End Sub

        Public ReadOnly Property Source As BoundExpression
        Public ReadOnly Property Member As PropertyInfo
        Public ReadOnly Property Arguments As ImmutableArray(Of BoundExpression)

    End Class

    Friend Class BoundInstanceMethodInvokationExpression : Inherits BoundExpression

        Public Sub New(syntax As SyntaxNode, source As BoundExpression, member As MethodInfo, arguments As IEnumerable(Of BoundExpression))
            MyBase.New(syntax, BoundNodeKind.BoundInstanceMethodInvokationExpression, member.ReturnType)
            Me.Source = source
            Me.Member = member
            Me.Arguments = arguments.ToImmutableArray
        End Sub

        Public ReadOnly Property Source As BoundExpression
        Public ReadOnly Property Member As MethodInfo
        Public ReadOnly Property Arguments As ImmutableArray(Of BoundExpression)

    End Class

    Friend Class BoundInstanceFieldGetExpression : Inherits BoundExpression

        Public Sub New(syntax As SyntaxNode, source As BoundExpression, member As FieldInfo)
            MyBase.New(syntax, BoundNodeKind.BoundInstanceFieldGetExpression, member.FieldType)
            Me.Source = source
            Me.Member = member
        End Sub

        Public ReadOnly Property Source As BoundExpression
        Public ReadOnly Property Member As FieldInfo

    End Class

End Namespace
