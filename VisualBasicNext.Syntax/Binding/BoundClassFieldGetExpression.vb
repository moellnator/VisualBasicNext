Imports System.Reflection
Imports VisualBasicNext.CodeAnalysis.Parsing

Namespace Binding
    Friend Class BoundClassFieldGetExpression : Inherits BoundExpression

        Public Sub New(syntax As SyntaxNode, member As FieldInfo)
            MyBase.New(syntax, BoundNodeKind.BoundClassMethodInvokationExpression, member.FieldType)
            Me.Member = member
        End Sub

        Public ReadOnly Property Member As FieldInfo

    End Class

End Namespace
