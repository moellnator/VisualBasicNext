Imports VisualBasicNext.Syntax.Parsing
Imports VisualBasicNext.Syntax.Symbols

Namespace Binding
    Friend Class BoundVariableExpression : Inherits BoundExpression

        Public ReadOnly Property Variable As VariableSymbol

        Public Sub New(expression As ExpressionNode, variable As VariableSymbol)
            MyBase.New(expression, BoundNodeKinds.BoundVariableExpression, variable.Type)
            Me.Variable = variable
        End Sub

    End Class

End Namespace
