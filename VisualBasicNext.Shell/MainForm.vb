Public Class MainForm
    Private Sub InputElement_Load(sender As Object, e As EventArgs) Handles InputElement.Load
        Me.ActiveControl = Me.InputElement
        Me.InputElement.Focus()
    End Sub

End Class
