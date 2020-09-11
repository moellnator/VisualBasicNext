Imports VisualBasicNext.Parsing.Tokenizing

Namespace Parsing.CST

    Public Enum NodeTypes
        Generic = 0
        Statement
        EndOfStatement
        DeclarationStatement
        Expression
        Literal
        TypeName
        Block
        Identifier
        [Array]
        Script
    End Enum

    Public Class Node : Implements IEnumerable(Of Node)

        Public ReadOnly Property NodeType As NodeTypes
        Public ReadOnly Property Children As Node()

        Private ReadOnly _tokens As Token()
        Public ReadOnly Property Tokens As Token()
            Get
                Dim retval As IEnumerable(Of Token) = Nothing
                If Me.IsLeaf Then
                    retval = Me._tokens
                Else
                    retval = Me.Children.SelectMany(Function(child) child.Tokens)
                End If
                Return retval.ToArray
            End Get
        End Property

        Public ReadOnly Property IsLeaf As Boolean
            Get
                Return Me._tokens IsNot Nothing
            End Get
        End Property

        Public ReadOnly Property Origin As TextLocation
            Get
                Return Me.Tokens.First.Location
            End Get
        End Property

        Public ReadOnly Property Length As Integer
            Get
                Dim last As Token = Me.Tokens.Last
                Return last.Location.Position - Me.Origin.Position + last.Length
            End Get
        End Property

        Public ReadOnly Property Content As String
            Get
                Return Me.Origin.GetText(Me.Length)
            End Get
        End Property

        Private Sub New(type As NodeTypes, children As Node(), tokens As Token())
            Me.NodeType = type
            Me.Children = children
            Me._tokens = tokens
        End Sub

        Public Sub New(type As NodeTypes, children As IEnumerable(Of Node))
            Me.New(type, children.ToArray, Nothing)
        End Sub

        Public Sub New(type As NodeTypes, tokens As IEnumerable(Of Token))
            Me.New(type, {}, tokens.ToArray)
        End Sub

        Public Function GetEnumerator() As IEnumerator(Of Node) Implements IEnumerable(Of Node).GetEnumerator
            Return Me.Children.AsEnumerable.GetEnumerator
        End Function

        Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Return Me.GetEnumerator
        End Function

    End Class

End Namespace
