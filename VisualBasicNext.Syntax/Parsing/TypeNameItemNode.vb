Imports VisualBasicNext.CodeAnalysis.Lexing

Namespace Parsing
    Public Class TypeNameItemNode : Inherits SyntaxNode

        Friend Sub New(delimeter As SyntaxToken, identifier As SyntaxToken, generics As GenericsListNode)
            MyBase.New(SyntaxKind.TypeNameItemNode)
            Me.Delimeter = delimeter
            Me.Identifier = identifier
            Me.Generics = generics
        End Sub

        Public Overrides ReadOnly Iterator Property Children As IEnumerable(Of SyntaxNode)
            Get
                If Not Me.Delimeter Is Nothing Then Yield Me.Delimeter
                Yield Me.Identifier
                If Not Me.Generics Is Nothing Then Yield Me.Generics
            End Get
        End Property

        Public ReadOnly Property Delimeter As SyntaxToken
        Public ReadOnly Property Identifier As SyntaxToken
        Public ReadOnly Property Generics As GenericsListNode

        Public ReadOnly Property HasGenerics As Boolean
            Get
                Return If(Me.Generics IsNot Nothing, Me.Generics.ListItems.Any(), Nothing)
            End Get
        End Property

    End Class

End Namespace
