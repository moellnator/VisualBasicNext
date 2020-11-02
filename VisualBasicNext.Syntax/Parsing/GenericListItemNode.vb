Imports VisualBasicNext.Syntax.Lexing

Namespace Parsing
    Public Class GenericsListItemNode : Inherits SyntaxNode

        Public Sub New(delimeterToken As SyntaxToken, typeName As TypeNameNode)
            MyBase.New(SyntaxKind.GenericListItemNode)
            Me.DelimeterToken = delimeterToken
            Me.TypeName = typeName
        End Sub

        Public Overrides ReadOnly Iterator Property Children As IEnumerable(Of SyntaxNode)
            Get
                If Me.DelimeterToken IsNot Nothing Then Yield Me.DelimeterToken
                Yield Me.TypeName
            End Get
        End Property

        Public ReadOnly Property DelimeterToken As SyntaxToken
        Public ReadOnly Property TypeName As TypeNameNode

    End Class

End Namespace
