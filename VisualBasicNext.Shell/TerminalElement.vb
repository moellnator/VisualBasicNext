Imports VisualBasicNext.Shell

Public Class TerminalElement

    'TODO [bugfix] -> Vertical resize does not update scrolling
    'TODO [bugfix] -> Scroll to cursor after input element moved on document submission

    Private _scroll_location As Single = 0
    Private _scroll_start As Integer = 0
    Private _scroll_down As Boolean = False
    Private _scroll_size As Single
    Private _scroll_is_hover_up As Boolean
    Private _scroll_is_hover_down As Boolean
    Private _scroll_is_hover_bar As Boolean

    Public Sub New()
        InitializeComponent()
        ReadOnlyElement.MakeDoubleBuffered(Me.PanelScroll)
    End Sub

    Private Sub InputElement_Load(sender As Object, e As EventArgs) Handles InputElement.Load
        Me.ActiveControl = Me.InputElement
        Me.InputElement.Focus()
        Me.TimerUpdate.Enabled = Not IsDesignTime()
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

    Private Sub PanelScroll_Paint(sender As Object, e As PaintEventArgs) Handles PanelScroll.Paint
        Dim g As Graphics = e.Graphics
        Dim r As Rectangle = Me.PanelScroll.ClientRectangle
        g.Clear(Me.PanelScroll.BackColor)
        g.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit
        g.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
        g.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
        Me._PrintScrollbar(r, g)
    End Sub

    Private Function _IsVerticalScroll()
        Return Me.FlowLayoutPanel.Height > Me.PanelContent.ClientSize.Height
    End Function

    Private Sub TimerUpdate_Tick(sender As Object, e As EventArgs) Handles TimerUpdate.Tick
        Me.ActiveControl = Me.InputElement
        Me.InputElement.Focus()
    End Sub

    Private Shared Function IsDesignTime() As Boolean
        Return System.ComponentModel.LicenseManager.UsageMode = System.ComponentModel.LicenseUsageMode.Designtime
    End Function

    Private Sub _PrintScrollbar(r As Rectangle, g As Graphics)
        If Me._IsVerticalScroll Then
            Dim button_offset As Integer = 20
            Dim scroll_size As Single = (Me.FlowLayoutPanel.Height - Me.PanelContent.Height)
            Dim scroll_rect As New RectangleF
            If scroll_size >= Me.PanelScroll.Height - 2 * button_offset - 8 Then
                Dim offset As Single = Me._scroll_location / scroll_size
                Me._scroll_size = 8
                scroll_rect = New RectangleF(4, button_offset + offset * (Me.PanelScroll.Height - 2 * button_offset - 8), 8, 8)
            Else
                Me._scroll_size = Me.PanelScroll.Height - 2 * button_offset - scroll_size
                scroll_rect = New RectangleF(4, button_offset + Me._scroll_location, 8, Me._scroll_size)
            End If
            g.FillRectangle(New SolidBrush(ColorPalette.ColorScrollbar), scroll_rect)
            g.FillPolygon(
                New SolidBrush(If(Me._scroll_is_hover_up, ColorPalette.ColorHighlight, ColorPalette.ColorOperator)),
                {
                    New Point(8, button_offset / 2 - 2),
                    New Point(4, button_offset / 2 + 2),
                    New Point(12, button_offset / 2 + 2)
                }
            )
            g.FillPolygon(
                New SolidBrush(If(Me._scroll_is_hover_down, ColorPalette.ColorHighlight, ColorPalette.ColorOperator)),
                {
                    New Point(8, r.Height - button_offset / 2 + 2),
                    New Point(4, r.Height - button_offset / 2 - 2),
                    New Point(12, r.Height - button_offset / 2 - 2)
                }
            )
        End If
    End Sub

    Private Sub FlowLayoutPanel_Invalidated(sender As Object, e As InvalidateEventArgs) Handles InputElement.Invalidated
        Me.PanelScroll.Invalidate()
    End Sub

    Private Sub PanelScroll_MouseMove(sender As Object, e As MouseEventArgs) Handles PanelScroll.MouseMove
        If Me._IsVerticalScroll Then
            If Me._scroll_down Then
                Dim delta As Integer = e.Location.Y - Me._scroll_start
                Me._scroll_start = e.Location.Y
                Dim max As Integer = (Me.PanelScroll.Height - 40) - Me._scroll_size
                Dim scroll_max As Integer = Me.FlowLayoutPanel.Height - Me.PanelContent.Height
                Me.ScrollLocation = Me._scroll_location + delta / max * scroll_max
            Else
                If e.Location.Y < 20 Then
                    Me._scroll_is_hover_up = True
                    Me.PanelScroll.Invalidate()
                ElseIf e.Location.Y > Me.PanelScroll.Height - 20 Then
                    Me._scroll_is_hover_down = True
                    Me.PanelScroll.Invalidate()
                Else
                    Me._scroll_is_hover_up = False
                    Me._scroll_is_hover_down = False
                    Me._scroll_is_hover_bar = True
                    Me.PanelScroll.Invalidate()
                End If
            End If
        End If
    End Sub

    Private Sub PanelScroll_MouseDown(sender As Object, e As MouseEventArgs) Handles PanelScroll.MouseDown
        If Me._IsVerticalScroll Then
            If e.Location.Y > 20 And e.Location.Y < Me.PanelScroll.Height - 20 Then
                Me._scroll_start = e.Location.Y
                Me._scroll_down = True
            End If
        End If
    End Sub

    Private Sub PanelScroll_MouseUp(sender As Object, e As MouseEventArgs) Handles PanelScroll.MouseUp
        Me._scroll_down = False
    End Sub

    Private Sub PanelScroll_MouseClick(sender As Object, e As MouseEventArgs) Handles PanelScroll.MouseClick
        If Me._IsVerticalScroll Then
            Dim scroll_max As Integer = Me.FlowLayoutPanel.Height - Me.PanelContent.Height
            If e.Location.Y < 20 Then
                Me.ScrollLocation = Me._scroll_location - 0.05 * scroll_max
            ElseIf e.Location.Y > Me.PanelScroll.Height - 20 Then
                Me.ScrollLocation = Me._scroll_location + 0.05 * scroll_max
            End If
        End If
    End Sub

    Private Sub PanelScroll_MouseLeave(sender As Object, e As EventArgs) Handles PanelScroll.MouseLeave
        Me._scroll_is_hover_bar = False
        Me._scroll_is_hover_up = False
        Me._scroll_is_hover_down = False
        Me.PanelScroll.Invalidate()
    End Sub

    Private Sub TerminalElement_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        Me.FlowLayoutPanel.Width = Me.PanelContent.Width - Me.PanelScroll.Width

    End Sub

    Private Sub InputElement_CursorPositionChanged(sender As Object, e As CursorPositionChangedEventArgs) Handles InputElement.CursorPositionChanged
        Dim cursor As PointF = e.NewCursorLocation + Me.InputElement.Location
        Dim text_size As SizeF = e.TextSize
        Dim bottom_edge As Integer = Me.PanelContent.Height + Me.ScrollLocation
        Dim top_edge As Integer = Me.ScrollLocation
        If cursor.Y + (Me.InputElement.Height - Me.InputElement.PanelText.Height + 24) > bottom_edge Then
            Me.ScrollLocation = cursor.Y + (Me.InputElement.Height - Me.InputElement.PanelText.Height + 24) - Me.PanelContent.Height
        ElseIf cursor.X - text_size.Height - 5 < top_edge Then
            Me.ScrollLocation = cursor.Y - text_size.Height - 5
        End If
    End Sub

    Private Sub FlowLayoutPanel_MouseWheel(sender As Object, e As MouseEventArgs) Handles FlowLayoutPanel.MouseWheel
        If Me._IsVerticalScroll Then
            Dim scroll_max As Integer = Me.FlowLayoutPanel.Height - Me.PanelContent.Height
            Me.ScrollLocation = Me._scroll_location - 0.1 * scroll_max * e.Delta / 120
        End If
    End Sub

    Protected Property ScrollLocation As Single
        Get
            Return Me._scroll_location
        End Get
        Set(value As Single)
            Dim scroll_max As Integer = Me.FlowLayoutPanel.Height - Me.PanelContent.Height
            Me._scroll_location = Math.Max(0, Math.Min(scroll_max, value))
            Me.FlowLayoutPanel.Top = -Me._scroll_location
            Me.PanelScroll.Invalidate()
        End Set
    End Property

End Class
