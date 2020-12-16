Imports System.Collections.Immutable
Imports System.Reflection
Imports VisualBasicNext.CodeAnalysis.Parsing

Namespace Binding
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

End Namespace
