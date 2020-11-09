Imports VisualBasicNext.Syntax.Parsing
Imports VisualBasicNext.Syntax.Symbols

Namespace Binding
    Public Class BoundVariableDeclarationStatement : Inherits BoundStatement

        Public Sub New(syntax As VariableDeclarationStatementNode, symbol As Symbol, initializer As BoundExpression)
            MyBase.New(syntax, BoundNodeKind.BoundVariableDeclarationStatement)
            Me.Symbol = symbol
            Me.Initializer = initializer
        End Sub

        Public ReadOnly Property Symbol As Symbol
        Public ReadOnly Property Initializer As BoundExpression

    End Class

End Namespace