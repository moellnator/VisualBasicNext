Imports VisualBasicNext.CodeAnalysis.Lexing

Namespace Parsing
    Public Class CastDynamicExpressionNode : Inherits ExpressionNode

        Friend Sub New(ctypeDynamicToken As SyntaxToken,
                      openBracket As SyntaxToken,
                      expression As ExpressionNode,
                      delimeterToken As SyntaxToken,
                      typeNode As ExpressionNode,
                      closeBracket As SyntaxToken)
            MyBase.New(SyntaxKind.CastDynamicExpressionNode)
            Me.CTypeDynamicToken = ctypeDynamicToken
            Me.OpenBracket = openBracket
            Me.Expression = expression
            Me.DelimeterToken = delimeterToken
            Me.TypeNode = typeNode
            Me.CloseBracket = closeBracket
        End Sub

        Public Overrides ReadOnly Iterator Property Children As IEnumerable(Of SyntaxNode)
            Get
                Yield Me.CTypeDynamicToken
                Yield Me.OpenBracket
                Yield Me.Expression
                Yield Me.DelimeterToken
                Yield Me.TypeNode
                Yield Me.CloseBracket
            End Get
        End Property

        Public ReadOnly Property CTypeDynamicToken As SyntaxToken
        Public ReadOnly Property OpenBracket As SyntaxToken
        Public ReadOnly Property Expression As ExpressionNode
        Public ReadOnly Property DelimeterToken As SyntaxToken
        Public ReadOnly Property TypeNode As ExpressionNode
        Public ReadOnly Property CloseBracket As SyntaxToken

    End Class

End Namespace
