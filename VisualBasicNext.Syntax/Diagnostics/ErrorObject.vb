Namespace Diagnostics
    Public Class ErrorObject

        Public ReadOnly Property Message As String
        Public ReadOnly Property Content As Text.Span

        Public Sub New(message As String, content As Text.Span)
            Me.Message = message
            Me.Content = content
        End Sub

    End Class

End Namespace
