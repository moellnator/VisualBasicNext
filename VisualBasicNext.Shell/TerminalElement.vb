Public Class TerminalElement

    'TODO -> Add vertical scrolling + Vertical Cursor Focussing

    Private Sub InputElement_Load(sender As Object, e As EventArgs) Handles InputElement.Load
        Me.ActiveControl = Me.InputElement
        Me.InputElement.Focus()
    End Sub

    Private Sub FlowLayoutPanel_Resize(sender As Object, e As EventArgs) Handles FlowLayoutPanel.Resize
        For Each c As Control In Me.FlowLayoutPanel.Controls
            c.Width = Me.FlowLayoutPanel.ClientSize.Width
        Next
    End Sub

    Private Sub InputElement_SubmittedDocument(sender As Object, e As SubmittedDocumentEventArgs) Handles InputElement.SubmittedDocument
        If e.Text.Count <> 0 Then
            Me.FlowLayoutPanel.Controls.Remove(Me.InputElement)
            Dim history_element As New ReadOnlyElement With {
                .Visible = True,
                .Width = Me.FlowLayoutPanel.Width
            }
            history_element.SetText(e.Text)
            history_element.RemoveLineOverhead()
            history_element.SetHightlight(ColorPalette.ColorOperator)
            Me.FlowLayoutPanel.Controls.Add(history_element)
            If e.Value IsNot Nothing OrElse e.Diagnostics.HasErrors Then
                Dim output_element As New OutputElement With {
                .Visible = True,
                .Width = Me.FlowLayoutPanel.Width
            }
                output_element.SetValue(e.Value, e.Diagnostics)
                Me.FlowLayoutPanel.Controls.Add(output_element)
            End If
            Me.FlowLayoutPanel.Controls.Add(Me.InputElement)
        End If
        Me.ActiveControl = Me.InputElement
        Me.InputElement.Focus()
    End Sub

    Private Sub PanelContent_Paint(sender As Object, e As PaintEventArgs) Handles PanelContent.Paint
        If Me._IsVerticalScroll Then
            Throw New NotImplementedException
        End If
    End Sub

    Private Function _IsVerticalScroll()
        Return Me.FlowLayoutPanel.Height < Me.PanelContent.ClientSize.Height
    End Function

End Class
