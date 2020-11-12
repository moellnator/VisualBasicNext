Imports VisualBasicNext.Shell

Public Class FormattedText : Implements IEnumerable(Of FormattedChar)

    Private ReadOnly _chars As FormattedChar()
    Public ReadOnly Property MaxLines As Integer

    Public Sub New(chars As IEnumerable(Of FormattedChar))
        Me._chars = chars.ToArray
        Me.MaxLines = If(Me._chars.Count > 1, Me._chars.Max(Function(c) c.Location.Y), 0)
    End Sub

    Public Function GetEnumerator() As IEnumerator(Of FormattedChar) Implements IEnumerable(Of FormattedChar).GetEnumerator
        Return Me._chars.AsEnumerable.GetEnumerator
    End Function

    Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Return Me.GetEnumerator
    End Function

    Public Shared ReadOnly Property Empty As FormattedText
        Get
            Return New FormattedText(Enumerable.Empty(Of FormattedChar))
        End Get
    End Property

End Class
