Imports VisualBasicNext.CodeAnalysis.Lexing

Namespace Parsing
    Public Class TernaryExpressionNode : Inherits ExpressionNode

        Friend Sub New(ifToken As SyntaxToken,
                       openBracket As SyntaxToken,
                       condition As ExpressionNode,
                       firstDelimeter As SyntaxToken,
                       trueExpression As ExpressionNode,
                       secondDelimeter As SyntaxToken,
                       falseExpression As ExpressionNode,
                       closeBracket As SyntaxToken)
            MyBase.New(SyntaxKind.TernaryExpressionNode)
            Me.IfToken = ifToken
            Me.OpenBracket = openBracket
            Me.Condition = condition
            Me.FirstDelimeter = firstDelimeter
            Me.TrueExpression = trueExpression
            Me.SecondDelimeter = secondDelimeter
            Me.FalseExpression = falseExpression
            Me.CloseBracket = closeBracket
        End Sub

        Public Overrides ReadOnly Iterator Property Children As IEnumerable(Of SyntaxNode)
            Get
                Yield Me.IfToken
                Yield Me.OpenBracket
                Yield Me.Condition
                Yield Me.FirstDelimeter
                Yield Me.TrueExpression
                Yield Me.SecondDelimeter
                Yield Me.FalseExpression
                Yield Me.CloseBracket
            End Get
        End Property

        Public ReadOnly Property IfToken As SyntaxToken
        Public ReadOnly Property OpenBracket As SyntaxToken
        Public ReadOnly Property Condition As ExpressionNode
        Public ReadOnly Property FirstDelimeter As SyntaxToken
        Public ReadOnly Property TrueExpression As ExpressionNode
        Public ReadOnly Property SecondDelimeter As SyntaxToken
        Public ReadOnly Property FalseExpression As ExpressionNode
        Public ReadOnly Property CloseBracket As SyntaxToken

    End Class

End Namespace