Imports VisualBasicNext.Syntax.Parsing

Namespace Binding
    Public Class BoundCastExpression : Inherits BoundExpression

        Public Sub New(syntax As SyntaxNode, expression As BoundExpression, type As Type)
            MyBase.New(syntax, BoundNodeKind.BoundCastExpression, type)
            Me.Expression = expression
            Me.Type = type
        End Sub

        Public ReadOnly Property Expression As BoundExpression
        Public ReadOnly Property Type As Type
    End Class

End Namespace
