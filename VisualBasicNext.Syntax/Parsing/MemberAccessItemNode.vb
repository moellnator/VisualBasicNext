Imports VisualBasicNext.CodeAnalysis.Lexing

Namespace Parsing
    Public Class MemberAccessItemNode : Inherits ExpressionNode

        Public Sub New(delimeter As SyntaxToken, identifier As SyntaxToken, generics As GenericsListNode, access As AccessListNode)
            MyBase.New(SyntaxKind.MemberAccessItemNode)
            Me.Delimeter = delimeter
            Me.Identifier = identifier
            Me.Generics = generics
            Me.Access = access
        End Sub

        Public Overrides ReadOnly Iterator Property Children As IEnumerable(Of SyntaxNode)
            Get
                If Me.Delimeter IsNot Nothing Then Yield Me.Delimeter
                Yield Me.Identifier
                If Not Me.Generics Is Nothing Then Yield Me.Generics
                If Not Me.Access Is Nothing Then Yield Me.Access
            End Get
        End Property

        Public ReadOnly Property Delimeter As SyntaxToken
        Public ReadOnly Property Identifier As SyntaxToken
        Public ReadOnly Property Generics As GenericsListNode
        Public ReadOnly Property Access As AccessListNode
    End Class

End Namespace
