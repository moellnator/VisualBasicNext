Imports System.Reflection
Imports VisualBasicNext.CodeAnalysis.Parsing

Namespace Binding
    Friend Class BoundClassPropertyGetExpression : Inherits BoundExpression

        Public Sub New(syntax As SyntaxNode, member As PropertyInfo, arguments As IEnumerable(Of BoundExpression))
            MyBase.New(syntax, BoundNodeKind.BoundClassMethodInvokationExpression, member.PropertyType)
            Me.Member = member
            Me.Arguments = arguments
        End Sub

        Public ReadOnly Property Member As PropertyInfo
        Public ReadOnly Property Arguments As IEnumerable(Of BoundExpression)

    End Class

End Namespace
