Imports VisualBasicNext.Syntax.Parsing

Namespace Binding
    Public Class BoundCastDynamicExpression : Inherits BoundExpression

        Public Sub New(syntax As SyntaxNode, expression As BoundExpression, type As BoundExpression)
            MyBase.New(syntax, BoundNodeKinds.BoundCastDynamicExpression, GetType(Object))
            Me.Expression = expression
            Me.Type = type
        End Sub

        Public ReadOnly Property Expression As BoundExpression
        Public ReadOnly Property Type As BoundExpression

    End Class

End Namespace
