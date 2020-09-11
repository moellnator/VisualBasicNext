Namespace Parsing
    Public Class TextLocation : Implements ICloneable

        Public ReadOnly Property Line As Integer
        Public ReadOnly Property Column As Integer
        Public ReadOnly Property Position As Integer

        Private ReadOnly _text As String

        Public Sub New(text As String, position As String)
            Me._text = text
            Me.Position = position
            If Me.Position >= Me._text.Length Then Throw New ArgumentException("Text position must not be larger than text length.")
            Me.Line = Me._text.Take(position).Count(Function(c) c = vbCr)
            Me.Column = position - (Me._text.Substring(0, Me.Position).LastIndexOf(vbLf) + 1)
        End Sub

        Private Sub New(other As TextLocation)
            Me._text = other._text
            Me.Line = other.Line
            Me.Column = other.Column
            Me.Position = other.Position
        End Sub

        Public Function GetLine() As String
            Return Me._text.Split(vbCr)(Me.Line).Trim(vbLf)
        End Function

        Public Function GetChar() As Char
            Return Me._text(Me.Position)
        End Function

        Public Function GetNextChar() As Char
            Return If(Me.CanMoveNext, Me._text(Me.Position + 1), Nothing)
        End Function

        Public Function GetText(length As Integer, Optional offset As Integer = 0) As String
            Return Me._text.Substring(Me.Position + offset, length)
        End Function

        Public Shared Operator +(a As TextLocation, b As Integer) As TextLocation
            Return New TextLocation(a._text, a.Position + b)
        End Operator

        Public Shared Operator -(a As TextLocation, b As Integer) As TextLocation
            Return New TextLocation(a._text, a.Position - b)
        End Operator

        Public ReadOnly Property CanMoveNext() As Boolean
            Get
                Return Me.Position < Me._text.Count - 1
            End Get
        End Property

        Public ReadOnly Property CanSkip(offset As Integer) As Boolean
            Get
                Return Me.Position + offset <= Me._text.Count - 1
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{Me.Line}:{Me.Column}({Me.Position})"
        End Function

        Public Function Clone() As TextLocation
            Return New TextLocation(Me)
        End Function

        Private Function IClonable_Clone() As Object Implements ICloneable.Clone
            Return Me.Clone
        End Function

    End Class

End Namespace
