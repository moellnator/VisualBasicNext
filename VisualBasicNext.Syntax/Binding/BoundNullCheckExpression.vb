Imports VisualBasicNext.Syntax.Parsing

Namespace Binding
    Public Class BoundNullCheckExpression : Inherits BoundExpression

        Public Sub New(syntaxNode As SyntaxNode, expression As BoundExpression, fallbackExpression As BoundExpression, boundType As Type)
            MyBase.New(syntaxNode, BoundNodeKinds.BoundNullCheckExpression, boundType)
            Me.Expression = expression
            Me.FallbackExpression = fallbackExpression
        End Sub

        Public ReadOnly Property Expression As BoundExpression
        Public ReadOnly Property FallbackExpression As BoundExpression

    End Class

End Namespace
