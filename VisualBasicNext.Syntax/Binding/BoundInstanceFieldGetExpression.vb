Imports System.Reflection
Imports VisualBasicNext.CodeAnalysis.Parsing

Namespace Binding
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
