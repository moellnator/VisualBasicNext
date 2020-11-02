Imports VisualBasicNext.Syntax.Lexing

Namespace Parsing
    Public Class VariableExpressionNode : Inherits ExpressionNode

        Public Sub New(identifier As SyntaxToken)
            MyBase.New(SyntaxKind.VariableExpressionNode)
            Me.Identifier = identifier
        End Sub

        Public Overrides ReadOnly Iterator Property Children As IEnumerable(Of SyntaxNode)
            Get
                Yield Me.Identifier
            End Get
        End Property

        Public ReadOnly Property Identifier As SyntaxToken

    End Class

End Namespace
