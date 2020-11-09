Imports VisualBasicNext.Syntax.Parsing

Namespace Binding
    Public Class BoundBinaryExpression : Inherits BoundExpression

        Public Sub New(syntax As SyntaxNode, left As BoundExpression, op As BoundBinaryOperator, right As BoundExpression)
            MyBase.New(syntax, BoundNodeKind.BoundBinaryExpression, op.ReturnType)
            Me.Left = left
            Me.Op = op
            Me.Right = right
        End Sub

        Public ReadOnly Property Left As BoundExpression
        Public ReadOnly Property Op As BoundBinaryOperator
        Public ReadOnly Property Right As BoundExpression

    End Class

End Namespace
