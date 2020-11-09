Imports VisualBasicNext.Syntax.Parsing

Namespace Binding
    Public Class BoundExpressionStatement : Inherits BoundStatement

        Public Sub New(syntax As SyntaxNode, expression As BoundExpression)
            MyBase.New(syntax, BoundNodeKind.BoundExpressionStatement)
            Me.Expression = expression
        End Sub

        Public ReadOnly Property Expression As BoundExpression

    End Class

End Namespace
