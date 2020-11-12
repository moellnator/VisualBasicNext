Public Class MainForm
    Private Sub InputElement_Load(sender As Object, e As EventArgs) Handles InputElement.Load
        Me.ActiveControl = Me.InputElement
        Me.InputElement.Focus()
    End Sub

    Private Sub FlowLayoutPanel_Resize(sender As Object, e As EventArgs) Handles FlowLayoutPanel.Resize
        For Each c As Control In Me.FlowLayoutPanel.Controls
            c.Width = Me.ClientSize.Width - 29
        Next
    End Sub

End Class
