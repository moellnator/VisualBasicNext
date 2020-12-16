Imports System.Reflection
Imports VisualBasicNext.CodeAnalysis.Parsing

Namespace Binding
    Friend Class BoundClassMethodInvokationExpression : Inherits BoundExpression

        Public Sub New(syntax As SyntaxNode, member As MethodInfo, arguments As IEnumerable(Of BoundExpression))
            MyBase.New(syntax, BoundNodeKind.BoundClassMethodInvokationExpression, member.ReturnType)
            Me.Member = member
            Me.Arguments = arguments
        End Sub

        Public ReadOnly Property Member As MethodInfo
        Public ReadOnly Property Arguments As IEnumerable(Of BoundExpression)

    End Class

End Namespace
