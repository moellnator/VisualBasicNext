Imports VisualBasicNext.CodeAnalysis.Parsing

Namespace Binding
    Friend Class BoundGetTypeExpression : Inherits BoundExpression

        Public Sub New(syntax As SyntaxNode, type As Type)
            MyBase.New(syntax, BoundNodeKind.BoundGetTypeExpression, GetType(Type))
            Me.Type = type
        End Sub

        Public ReadOnly Property Type As Type

    End Class

End Namespace
