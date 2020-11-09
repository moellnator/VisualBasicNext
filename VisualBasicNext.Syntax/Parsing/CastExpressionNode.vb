Imports VisualBasicNext.CodeAnalysis.Lexing

Namespace Parsing
    Public Class CastExpressionNode : Inherits ExpressionNode

        Friend Sub New(ctypeToken As SyntaxToken,
                       openBracket As SyntaxToken,
                       expression As ExpressionNode,
                       delimeterToken As SyntaxToken,
                       typename As TypeNameNode,
                       closeBracket As SyntaxToken)
            MyBase.New(SyntaxKind.CastExpressionNode)
            Me.CtypeToken = ctypeToken
            Me.OpenBracket = openBracket
            Me.Expression = expression
            Me.DelimeterToken = delimeterToken
            Me.Typename = typename
            Me.CloseBracket = closeBracket
        End Sub

        Public Overrides ReadOnly Iterator Property Children As IEnumerable(Of SyntaxNode)
            Get
                Yield Me.CtypeToken
                Yield Me.OpenBracket
                Yield Me.Expression
                Yield Me.DelimeterToken
                Yield Me.Typename
                Yield Me.CloseBracket
            End Get
        End Property

        Public ReadOnly Property CtypeToken As SyntaxToken
        Public ReadOnly Property OpenBracket As SyntaxToken
        Public ReadOnly Property Expression As ExpressionNode
        Public ReadOnly Property DelimeterToken As SyntaxToken
        Public ReadOnly Property Typename As TypeNameNode
        Public ReadOnly Property CloseBracket As SyntaxToken
    End Class

End Namespace
