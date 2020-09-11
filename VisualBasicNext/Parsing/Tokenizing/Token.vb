Namespace Parsing.Tokenizing

    Public Enum TokenTypes
        EndOfLine
        [Operator]
        Separator
        BlockOpen
        BlockClose
        [String]
        Number
        Identifier
        Keyword
        Character
    End Enum

    Public Class Token
        Public ReadOnly Property TokenType As TokenTypes
        Public ReadOnly Property Location As TextLocation
        Public ReadOnly Property Length As Integer
        Public ReadOnly Property Content As String
            Get
                Return If(Me.Length = 0, "", Me.Location.GetText(Me.Length))
            End Get
        End Property

        Public Sub New(type As TokenTypes, location As TextLocation, length As Integer)
            Me.TokenType = type
            Me.Location = location
            Me.Length = length
        End Sub

        Public Overrides Function ToString() As String
            Return $"({Me.TokenType.ToString}) {Me.Content} @{If(Me.Location Is Nothing, "end", Me.Location.ToString)}"
        End Function

    End Class

End Namespace
