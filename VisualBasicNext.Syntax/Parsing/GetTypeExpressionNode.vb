Imports VisualBasicNext.Syntax.Lexing

Namespace Parsing
    Public Class GetTypeExpressionNode : Inherits ExpressionNode

        Public Sub New(getTypeToken As SyntaxToken, openBracket As SyntaxToken, typeName As TypeNameNode, closeBracket As SyntaxToken)
            MyBase.New(SyntaxKind.GetTypeExpressionNode)
            Me.GetTypeToken = getTypeToken
            Me.OpenBracket = openBracket
            Me.TypeName = typeName
            Me.CloseBracket = closeBracket
        End Sub

        Public Overrides ReadOnly Iterator Property Children As IEnumerable(Of SyntaxNode)
            Get
                Yield Me.GetTypeToken
                Yield Me.OpenBracket
                Yield Me.TypeName
                Yield Me.CloseBracket
            End Get
        End Property

        Public ReadOnly Property GetTypeToken As SyntaxToken
        Public ReadOnly Property OpenBracket As SyntaxToken
        Public ReadOnly Property TypeName As TypeNameNode
        Public ReadOnly Property CloseBracket As SyntaxToken
    End Class

End Namespace