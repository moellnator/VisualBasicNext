Imports VisualBasicNext.Syntax.Parsing

Namespace Binding
    Public Class BoundTryCastExpression : Inherits BoundExpression

        Public Sub New(syntax As SyntaxNode, expression As BoundExpression, target As Type)
            MyBase.New(syntax, BoundNodeKinds.BoundTryCastExpression, target)
            Me.Expression = expression
            Me.Target = target
        End Sub

        Public ReadOnly Property Expression As BoundExpression
        Public ReadOnly Property Target As Type

    End Class

End Namespace
