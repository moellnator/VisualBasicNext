Imports VisualBasicNext.CodeAnalysis.Lexing

Namespace Parsing
    Public Class ArrayItemNode : Inherits SyntaxNode

        Friend Sub New(delimeter As SyntaxToken, expression As ExpressionNode)
            MyBase.New(SyntaxKind.ArrayItemNode)
            Me.Delimeter = delimeter
            Me.Expression = expression
        End Sub

        Public ReadOnly Property Delimeter As SyntaxToken
        Public ReadOnly Property Expression As ExpressionNode

        Public Overrides ReadOnly Iterator Property Children As IEnumerable(Of SyntaxNode)
            Get
                If Not Me.Delimeter Is Nothing Then Yield Me.Delimeter
                Yield Me.Expression
            End Get
        End Property

    End Class

End Namespace