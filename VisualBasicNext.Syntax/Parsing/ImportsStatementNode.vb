Imports System.Collections.Immutable
Imports VisualBasicNext.CodeAnalysis.Lexing

Namespace Parsing
    Public Class ImportsStatementNode : Inherits StatementNode

        Friend Sub New(importsKeyword As SyntaxToken, identifier As NamespaceNode, endOfStatementToken As SyntaxToken)
            MyBase.New(SyntaxKind.ImportsStatementNode, endOfStatementToken)
            Me.ImportsKeyword = importsKeyword
            Me.Identifier = identifier
        End Sub

        Public Overrides ReadOnly Iterator Property Children As IEnumerable(Of SyntaxNode)
            Get
                Yield Me.ImportsKeyword
                Yield Me.Identifier
                Yield Me.EndOfStatementToken
            End Get
        End Property

        Public ReadOnly Property ImportsKeyword As SyntaxToken
        Public ReadOnly Property Identifier As NamespaceNode
    End Class

    Public Class NamespaceNode : Inherits SyntaxNode

        Public Sub New(items As IEnumerable(Of NamespaceItemNode))
            MyBase.New(SyntaxKind.NamespaceNode)
            Me.Items = items.ToImmutableArray
        End Sub

        Public Overrides ReadOnly Property Children As IEnumerable(Of SyntaxNode)
            Get
                Return Me.Items
            End Get
        End Property

        Public ReadOnly Property Items As ImmutableArray(Of NamespaceItemNode)

    End Class

    Public Class NamespaceItemNode : Inherits SyntaxNode

        Public Sub New(delimeter As SyntaxNode, identifier As SyntaxNode)
            MyBase.New(SyntaxKind.NamespaceItemNode)
            Me.Delimeter = delimeter
            Me.Identifier = identifier
        End Sub

        Public Overrides ReadOnly Iterator Property Children As IEnumerable(Of SyntaxNode)
            Get
                If Not Me.Delimeter Is Nothing Then Yield Me.Delimeter
                Yield Me.Identifier
            End Get
        End Property

        Public ReadOnly Property Delimeter As SyntaxNode
        Public ReadOnly Property Identifier As SyntaxNode

    End Class

End Namespace