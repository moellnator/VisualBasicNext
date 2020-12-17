Imports VisualBasicNext.CodeAnalysis.Lexing

Namespace Parsing
    Public Class ConstructorExpressionNode : Inherits ExpressionNode

        Public Sub New(newToken As SyntaxToken, typeName As TypeNameNode, arguments As ArgumentListNode, fromToken As SyntaxToken, arrayNode As ArrayExpressionNode)
            MyBase.New(SyntaxKind.ConstructorExpressionNode)
            Me.NewToken = newToken
            Me.TypeName = typeName
            Me.Arguments = arguments
            Me.FromToken = fromToken
            Me.ArrayNode = arrayNode
        End Sub

        Public Overrides ReadOnly Iterator Property Children As IEnumerable(Of SyntaxNode)
            Get
                Yield Me.NewToken
                Yield Me.TypeName
                If Not Me.Arguments Is Nothing Then Yield Me.Arguments
                If Not Me.FromToken Is Nothing Then Yield Me.FromToken
                If Not Me.ArrayNode Is Nothing Then Yield Me.ArrayNode
            End Get
        End Property

        Public ReadOnly Property NewToken As SyntaxToken
        Public ReadOnly Property TypeName As TypeNameNode
        Public ReadOnly Property Arguments As ArgumentListNode
        Public ReadOnly Property FromToken As SyntaxToken
        Public ReadOnly Property ArrayNode As ArrayExpressionNode
    End Class

End Namespace
