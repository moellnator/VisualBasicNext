Namespace Text
    Public Class Span : Implements IEquatable(Of Span)

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

        Public Overrides Function Equals(obj As Object) As Boolean
            Return If(TypeOf obj Is Span, Me._IEquatable_Equals(obj), False)
        End Function

        Private Function _IEquatable_Equals(other As Span) As Boolean Implements IEquatable(Of Span).Equals
            Return Me.ToString = other.ToString AndAlso (Me.Start = other.Start And Me.Length = other.Length)
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return New CombinedHashCode(Me.ToString, Me.Start, Me.Length).GetHashCode
        End Function

    End Class

End Namespace
