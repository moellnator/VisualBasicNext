Namespace Text
    Public Class Position

        Public ReadOnly Property LineNumber As Integer
        Public ReadOnly Property Offset As Integer

        Private Sub New(lineNumber As Integer, offset As Integer)
            Me.LineNumber = lineNumber
            Me.Offset = offset
        End Sub

        Public Shared Function FromSourceLocation(source As Source, location As Integer) As Position
            Dim lineIndex As Integer = source.GetLineIndex(location)
            Dim offset As Integer = location - source(lineIndex).Start
            Return New Position(lineIndex, offset)
        End Function

        Public Overrides Function ToString() As String
            Return $"(line: {Me.LineNumber}, offset: {Me.Offset})"
        End Function

    End Class

End Namespace
