Imports VisualBasicNext.CodeAnalysis.Lexing

Namespace Parsing
    Public Class TryCastExpressionNode : Inherits ExpressionNode

        Friend Sub New(trycastKeyword As SyntaxToken,
                       openBracket As SyntaxToken,
                       expression As ExpressionNode,
                       delimeter As SyntaxNode,
                       target As TypeNameNode,
                       closeBracket As SyntaxNode)
            MyBase.New(SyntaxKind.TryCastExpressionNode)
            Me.TrycastKeyword = trycastKeyword
            Me.OpenBracket = openBracket
            Me.Expression = expression
            Me.Delimeter = delimeter
            Me.Target = target
            Me.CloseBracket = closeBracket
        End Sub

        Public Overrides ReadOnly Iterator Property Children As IEnumerable(Of SyntaxNode)
            Get
                Yield Me.TrycastKeyword
                Yield Me.OpenBracket
                Yield Me.Expression
                Yield Me.Delimeter
                Yield Me.Target
                Yield Me.CloseBracket
            End Get
        End Property

        Public ReadOnly Property TrycastKeyword As SyntaxToken
        Public ReadOnly Property OpenBracket As SyntaxToken
        Public ReadOnly Property Expression As ExpressionNode
        Public ReadOnly Property Delimeter As SyntaxNode
        Public ReadOnly Property Target As TypeNameNode
        Public ReadOnly Property CloseBracket As SyntaxNode
    End Class

End Namespace
