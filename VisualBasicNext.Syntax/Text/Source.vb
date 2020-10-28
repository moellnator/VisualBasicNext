
Namespace Text
    Public Class Source : Implements IReadOnlyList(Of Line)

        Private ReadOnly _text As String
        Private ReadOnly _lines As Line()

        Default Public ReadOnly Property Item(index As Integer) As Line Implements IReadOnlyList(Of Line).Item
            Get
                Return Me._lines(index)
            End Get
        End Property

        Public ReadOnly Property Count As Integer Implements IReadOnlyCollection(Of Line).Count
            Get
                Return Me._lines.Count
            End Get
        End Property

        Private Sub New(text As String)
            Me._text = text
            Me._lines = _GetLines(Me).ToArray
        End Sub

        Private Shared Iterator Function _GetLines(source As Source) As IEnumerable(Of Line)
            Dim start As Integer = 0
            Dim position As Integer = start
            Dim index As Integer = 0
            While position < source._text.Length
                Dim break As Integer = _GetLineBreakWidth(source._text, position)
                If break <> 0 Then
                    Yield New Line(source, index, start, position - start, break)
                    position += break
                    start = position
                    index += 1
                Else
                    position += 1
                End If
            End While
            If start < source._text.Length Then
                Yield New Line(source, index, start, position - start, 0)
            End If
        End Function

        Private Shared Function _GetLineBreakWidth(text As String, position As Integer) As Integer
            Dim current As Char = text(position)
            Dim lookahead As Char = If(position + 1 < text.Length, text(position + 1), vbNullChar)
            If current = vbCr And lookahead = vbLf Then Return 2
            If current = vbCr Or current = vbLf Then Return 1
            Return 0
        End Function

        Public Shared Function FromText(text As String) As Source
            Return New Source(text)
        End Function

        Public Function GetLineIndex(location As Integer) As Integer
            Dim upper As Integer = Me.Count - 1
            Dim lower As Integer = 0
            While lower <= upper
                Dim current As Integer = lower + (upper - lower) / 2
                Dim start As Integer = Me(current).Start
                If location = start Then Return current
                If start > location Then
                    upper = current - 1
                Else
                    lower = current + 1
                End If
            End While
            Return lower - 1
        End Function

        Public Function GetEnumerator() As IEnumerator(Of Line) Implements IEnumerable(Of Line).GetEnumerator
            Return Me._lines.AsEnumerable.GetEnumerator
        End Function

        Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Return Me.GetEnumerator
        End Function

        Public Overrides Function ToString() As String
            Return Me._text
        End Function

        Public Overloads Function ToString(start As Integer, length As Integer) As String
            Return Me._text.Substring(start, length)
        End Function

    End Class

End Namespace
