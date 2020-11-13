Imports VisualBasicNext.Shell

Public Class MainForm

    Private Enum _DragDirection
        None = -1
        Top
        TopRight
        Right
        BottomRight
        Bottom
        BottomLeft
        Left
        TopLeft
    End Enum

    Private _is_moving As Boolean = False
    Private _start_moving As Point = Nothing
    Private _direction_drag As _DragDirection = _DragDirection.None
    Private _start_drag As Point = Nothing

    Public Sub New()
        InitializeComponent()
        Me.ControlButtonClose.Text = ChrW(&HE8BB)
        Me.ControlButtonMinimize.Text = ChrW(&HE921)
        Me.SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer, True)
    End Sub

    Private Sub ControlButtonClose_Click(sender As Object, e As EventArgs) Handles ControlButtonClose.Click
        Me.Close()
    End Sub

    Private Sub ControlButtonMinimize_Click(sender As Object, e As EventArgs) Handles ControlButtonMinimize.Click
        Me.WindowState = FormWindowState.Minimized
    End Sub

    Private Sub PanelTitle_MouseDown(sender As Object, e As MouseEventArgs) Handles PanelTitle.MouseDown
        Me._is_moving = True
        Me._start_moving = e.Location
    End Sub

    Private Sub PanelTitle_MouseUp(sender As Object, e As MouseEventArgs) Handles PanelTitle.MouseUp
        Me._is_moving = False
    End Sub

    Private Sub PanelTitle_MouseMove(sender As Object, e As MouseEventArgs) Handles PanelTitle.MouseMove
        If Me._is_moving Then
            Dim delta As Point = e.Location - Me._start_moving
            Me.Location += delta
        End If
    End Sub

    Private Sub MainForm_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint
        e.Graphics.DrawRectangle(New Pen(Color.FromArgb(255, 83, 83, 85)), New Rectangle(0, 0, Me.ClientSize.Width - 1, Me.ClientSize.Height - 1))
    End Sub

    Private Sub TerminalElement_MouseDown(sender As Object, e As MouseEventArgs) Handles TerminalElement.MouseDown
        If e.Location.X + TerminalElement.Location.X > Me.ClientSize.Width - 5 Then
            If e.Location.Y + TerminalElement.Location.Y > Me.ClientSize.Height - 5 Then
                Me._direction_drag = _DragDirection.BottomRight
            Else
                Me._direction_drag = _DragDirection.Right
            End If
        ElseIf e.Location.Y + TerminalElement.Location.Y > Me.ClientSize.Height - 5 Then
            Me._direction_drag = _DragDirection.Bottom
        End If
        If Me._direction_drag <> _DragDirection.None Then
            Me._start_drag = e.Location
        End If
    End Sub

    Private Sub TerminalElement_MouseMove(sender As Object, e As MouseEventArgs) Handles TerminalElement.MouseMove
        If Not Me._direction_drag <> _DragDirection.None Then
            If e.Location.X + TerminalElement.Location.X > Me.ClientSize.Width - 5 Then
                If e.Location.Y + TerminalElement.Location.Y > Me.ClientSize.Height - 5 Then
                    Me.Cursor = Cursors.SizeNWSE
                Else
                    Me.Cursor = Cursors.SizeWE
                End If
            ElseIf e.Location.Y + TerminalElement.Location.Y > Me.ClientSize.Height - 5 Then
                Me.Cursor = Cursors.SizeNS
            Else
                Me.Cursor = Cursors.Default
            End If
        Else
            Select Case Me._direction_drag
                Case _DragDirection.Right
                    Me.Width = Math.Max(Me.Width + e.Location.X - Me._start_drag.X, 800)
                Case _DragDirection.BottomRight
                    Me.Width = Math.Max(Me.Width + e.Location.X - Me._start_drag.X, 800)
                    Me.Height = Math.Max(Me.Height + e.Location.Y - Me._start_drag.Y, 450)
                Case _DragDirection.Bottom
                    Me.Height = Math.Max(Me.Height + e.Location.Y - Me._start_drag.Y, 450)
            End Select
            Me._start_drag = e.Location
        End If
    End Sub

    Private Sub FlowLayoutPanel_MouseLeave(sender As Object, e As EventArgs) Handles TerminalElement.MouseLeave
        Me.Cursor = Cursors.Default
    End Sub

    Private Sub FlowLayoutPanel_MouseUp(sender As Object, e As MouseEventArgs) Handles TerminalElement.MouseUp
        Me._direction_drag = _DragDirection.None
        Me.Invalidate(True)
    End Sub

    Private Sub MainForm_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        Me.Refresh()
    End Sub

    Private Sub TerminalElement_Load(sender As Object, e As EventArgs) Handles TerminalElement.Load
        Me.ActiveControl = Me.TerminalElement
        Me.TerminalElement.Focus()
    End Sub

End Class
