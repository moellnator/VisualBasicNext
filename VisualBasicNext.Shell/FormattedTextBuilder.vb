Public Class FormattedTextBuilder

    Private _position As Integer = 0
    Private _line As Integer = 0
    Private ReadOnly _internal As New List(Of FormattedChar)
    Private _current_color As Color = ColorPalette.ColorPlainText

    Public Function ToFormattedText() As FormattedText
        Return New FormattedText(Me._internal)
    End Function

    Public Sub Append(text As String)
        Me.Append(text, Me._current_color)
    End Sub

    Public Sub Append(text As String, color As Color)
        Dim brush As New SolidBrush(color)
        For Each c As Char In text
            Select Case c
                Case vbCr
                    Me._position = 0
                    Me._line += 1
                Case Is >= " "
                    Me._internal.Add(New FormattedChar(c, brush, New Point(Me._position, Me._line)))
                    Me._position += 1
            End Select
        Next
    End Sub

    Public Sub SetColor(color As Color)
        Me._current_color = color
    End Sub

    Public Overrides Function ToString() As String
        Return New String(Me._internal.Select(Function(c) c.Glyph).ToArray)
    End Function

End Class

