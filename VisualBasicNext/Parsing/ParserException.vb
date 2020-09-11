Namespace Parsing
    Public Class ParserException : Inherits Exception

        Public ReadOnly Property Location As TextLocation

        Public Sub New(message As String, location As TextLocation)
            MyBase.New(message)
            Me.Location = location
        End Sub

        Public Sub New(message As String, inner As Exception, location As TextLocation)
            MyBase.New(message, inner)
            Me.Location = location
        End Sub

    End Class

End Namespace
