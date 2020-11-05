Imports VisualBasicNext.Syntax.Parsing

Namespace Binding
    Public Class BoundGetTypeExpression : Inherits BoundExpression

        Public Sub New(syntax As SyntaxNode, type As Type)
            MyBase.New(syntax, BoundNodeKinds.BoundGetTypeExpression, GetType(Type))
            Me.Type = type
        End Sub

        Public ReadOnly Property Type As Type

    End Class

End Namespace
