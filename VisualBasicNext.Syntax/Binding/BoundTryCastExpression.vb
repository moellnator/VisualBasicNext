Imports VisualBasicNext.CodeAnalysis.Parsing

Namespace Binding
    Friend Class BoundTryCastExpression : Inherits BoundExpression

        Public Sub New(syntax As SyntaxNode, expression As BoundExpression, target As Type)
            MyBase.New(syntax, BoundNodeKind.BoundTryCastExpression, target)
            Me.Expression = expression
            Me.Target = target
        End Sub

        Public ReadOnly Property Expression As BoundExpression
        Public ReadOnly Property Target As Type

    End Class

End Namespace
