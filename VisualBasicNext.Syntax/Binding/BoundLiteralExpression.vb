Imports VisualBasicNext.CodeAnalysis.Parsing

Namespace Binding
    Friend Class BoundLiteralExpression : Inherits BoundExpression

        Public Sub New(syntax As SyntaxNode, value As Object)
            MyBase.New(syntax, BoundNodeKind.BoundLiteral, If(value IsNot Nothing, value.GetType, GetType(Object)))
            Me.Value = value
        End Sub

        Public ReadOnly Property Value As Object

        Public Overrides ReadOnly Property Constant As BoundConstant
            Get
                Return New BoundConstant(Me.Value)
            End Get
        End Property

    End Class

End Namespace