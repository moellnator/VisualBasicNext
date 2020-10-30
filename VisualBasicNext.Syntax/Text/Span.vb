Namespace Text
    Public Class Span

        Public ReadOnly Property Source As Source
        Public ReadOnly Property Start As Integer
        Public ReadOnly Property Length As Integer
        Public ReadOnly Property [End] As Integer
            Get
                Return Me.Start + Length
            End Get
        End Property

        Public Sub New(source As Source, start As Integer, length As Integer)
            Me.Source = source
            Me.Start = start
            If Me.Start < 0 Then Throw New ArgumentException("Argument can not be negative.", NameOf(start))
            Me.Length = length
            If Me.Length < 0 Then Throw New ArgumentException("Argument can not be negative.", NameOf(length))
        End Sub

        Public Shared Function FromBounds(source As Source, start As Integer, [end] As Integer) As Span
            Return New Span(source, start, [end] - start)
        End Function


        Public Shared Function FromBounds(start As Span, [end] As Span) As Span
            Return FromBounds(start.Source, start.Start, [end].End)
        End Function

        Public Overrides Function ToString() As String
            Return If(Me.Start >= Source.ToString.Count, "", Me.Source.ToString.Substring(Me.Start, Me.Length))
        End Function

        Public Function GetStartPosition() As Position
            Return Position.FromSourceLocation(Me.Source, Me.Start)
        End Function

    End Class

End Namespace
