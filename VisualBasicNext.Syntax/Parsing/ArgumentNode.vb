Imports VisualBasicNext.CodeAnalysis.Lexing

Namespace Parsing
    Public Class ArgumentNode : Inherits SyntaxNode

        Public Sub New(delimeter As SyntaxToken, argument As ExpressionNode)
            MyBase.New(SyntaxKind.ArgumentNode)
            Me.Delimeter = delimeter
            Me.Argument = argument
        End Sub

        Public Overrides ReadOnly Iterator Property Children As IEnumerable(Of SyntaxNode)
            Get
                If Not Me.Delimeter Is Nothing Then Yield Me.Delimeter
                Yield Me.Argument
            End Get
        End Property

        Public ReadOnly Property Delimeter As SyntaxToken
        Public ReadOnly Property Argument As ExpressionNode
    End Class

End Namespace
