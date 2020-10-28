Namespace Text
    Public Class Line

        Public ReadOnly Property Index As Integer
        Private ReadOnly _source As Source
        Private ReadOnly _length_break As Integer

        Public ReadOnly Property Length As Integer
        Public ReadOnly Property Start As Integer

        Public Sub New(source As Source, index As Integer, start As Integer, length As Integer, lengthBreak As Integer)
            Me._source = source
            Me.Start = start
            Me.Length = length
            Me._length_break = lengthBreak
            Me.Index = index
        End Sub

        Public ReadOnly Property Content As Span
            Get
                Return New Span(Me._source, Me.Start, Me.Length)
            End Get
        End Property

        Public ReadOnly Property ContentWithLineBreak As Span
            Get
                Return New Span(Me._source, Me.Start, Me.LengthWithLineBreak)
            End Get
        End Property

        Public ReadOnly Property LengthWithLineBreak As Integer
            Get
                Return Me._length_break + Me.Length
            End Get
        End Property

    End Class

End Namespace
