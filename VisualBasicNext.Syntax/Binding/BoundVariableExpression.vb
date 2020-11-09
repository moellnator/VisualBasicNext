Imports VisualBasicNext.CodeAnalysis.Parsing
Imports VisualBasicNext.CodeAnalysis.Symbols

Namespace Binding
    Friend Class BoundVariableExpression : Inherits BoundExpression

        Public ReadOnly Property Variable As VariableSymbol

        Public Sub New(expression As ExpressionNode, variable As VariableSymbol)
            MyBase.New(expression, BoundNodeKind.BoundVariableExpression, variable.Type)
            Me.Variable = variable
        End Sub

    End Class

End Namespace
