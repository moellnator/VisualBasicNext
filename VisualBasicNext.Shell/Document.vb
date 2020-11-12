Imports System.Collections.ObjectModel
Imports VisualBasicNext.CodeAnalysis.Lexing

Public Class Document : Implements IDisposable

    Public Event DocumentChanged As EventHandler

    Private ReadOnly _lines As New ObservableCollection(Of String)
    Private _cursor_top As Integer = 0
    Private _cursor_left As Integer = 0
    Private _tokens As SyntaxToken() = Array.Empty(Of SyntaxToken)

    Private _is_disposed As Boolean = False

    Public Sub New()
        Me._lines.Add(String.Empty)
        AddHandler Me._lines.CollectionChanged, AddressOf Me._HandleLinesChanged
    End Sub

    Public ReadOnly Property Lines As IReadOnlyCollection(Of String)
        Get
            Return Me._lines
        End Get
    End Property

    Public ReadOnly Property Text As String
        Get
            Return String.Join(vbNewLine, Me._lines)
        End Get
    End Property

    Private Sub _HandleLinesChanged(sender As Object, e As Specialized.NotifyCollectionChangedEventArgs)
        Me._tokens = SyntaxToken.GetTokensFromSource(CodeAnalysis.Text.Source.FromText(Me.Text), Nothing).ToArray
    End Sub

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me._is_disposed Then
            If disposing Then
                RemoveHandler Me._lines.CollectionChanged, AddressOf Me._HandleLinesChanged
            End If
        End If
        Me._is_disposed = True
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
    End Sub

    Public ReadOnly Property Tokens() As IReadOnlyCollection(Of SyntaxToken)
        Get
            Return Me._tokens
        End Get
    End Property

    Public ReadOnly Property CursorPosition As Point
        Get
            Return New Point(Me._cursor_left, Me._cursor_top)
        End Get
    End Property

    Private Function _CurrentLine() As String
        Return Me._lines(Me._cursor_top)
    End Function

    Public Sub InsertAtCursor(c As String)
        Me._lines(Me._cursor_top) = Me._CurrentLine.Insert(Me._cursor_left, c)
        Me._cursor_left += c.Length
        RaiseEvent DocumentChanged(Me, EventArgs.Empty)
    End Sub

    Public Sub InsertNewLine()
        Dim line As String = Me._CurrentLine
        Me._lines(Me._cursor_top) = line.Substring(0, Me._cursor_left)
        Me._lines.Insert(Me._cursor_top + 1, line.Substring(Me._cursor_left))
        Me._cursor_left = 0
        Me._cursor_top += 1
        RaiseEvent DocumentChanged(Me, EventArgs.Empty)
    End Sub

    Public Sub RemoveBeforeCursor()
        If Me._cursor_left = 0 Then
            If Me._cursor_top > 0 Then
                Dim line_before As String = Me._lines(Me._cursor_top - 1)
                Me._lines(Me._cursor_top - 1) = line_before & Me._CurrentLine
                Me._lines.RemoveAt(Me._cursor_top)
                Me._cursor_top = Me._cursor_top - 1
                Me._cursor_left = line_before.Count
            End If
        Else
            Me._lines(Me._cursor_top) = Me._CurrentLine.Remove(Me._cursor_left - 1, 1)
            Me._cursor_left -= 1
        End If
        RaiseEvent DocumentChanged(Me, EventArgs.Empty)
    End Sub

    Public Sub RemoveAfterCursor()
        If Me._cursor_left = Me._CurrentLine.Count Then
            If Me._cursor_top < Me._lines.Count - 1 Then
                Me._lines(Me._cursor_top) = Me._CurrentLine & Me._lines(Me._cursor_top + 1)
                Me._lines.RemoveAt(Me._cursor_top + 1)
            End If
        Else
            Me._lines(Me._cursor_top) = Me._CurrentLine.Remove(Me._cursor_left, 1)
        End If
        RaiseEvent DocumentChanged(Me, EventArgs.Empty)
    End Sub

    Public Sub CursorMoveLeft()
        If Me._cursor_left > 0 Then
            Me._cursor_left -= 1
        Else
            If Me._cursor_top > 0 Then
                Me._cursor_top -= 1
                Me._cursor_left = Me._CurrentLine.Count
            End If
        End If
        RaiseEvent DocumentChanged(Me, EventArgs.Empty)
    End Sub

    Public Sub CursorMoveRight()
        If Me._cursor_left <= Me._CurrentLine.Length - 1 Then
            Me._cursor_left += 1
        Else
            If Me._cursor_top < Me._lines.Count - 1 Then
                Me._cursor_left = 0
                Me._cursor_top += 1
            End If
        End If
        RaiseEvent DocumentChanged(Me, EventArgs.Empty)
    End Sub

    Public Sub CursorMoveUp()
        If Me._cursor_top > 0 Then
            Me._cursor_top -= 1
            If Me._cursor_left > Me._CurrentLine.Count Then Me._cursor_left = Me._CurrentLine.Count
        End If
        RaiseEvent DocumentChanged(Me, EventArgs.Empty)
    End Sub

    Public Sub CursorMoveDown()
        If Me._cursor_top < Me._lines.Count - 1 Then
            Me._cursor_top += 1
            If Me._cursor_left > Me._CurrentLine.Count Then Me._cursor_left = Me._CurrentLine.Count
        End If
        RaiseEvent DocumentChanged(Me, EventArgs.Empty)
    End Sub

    Public Sub CursorMoveHome()
        Me._cursor_left = 0
        RaiseEvent DocumentChanged(Me, EventArgs.Empty)
    End Sub

    Public Sub CursorMoveEnd()
        Me._cursor_left = Me._CurrentLine.Count
        RaiseEvent DocumentChanged(Me, EventArgs.Empty)
    End Sub

    Public Sub Clear()
        Me._lines.Clear()
        Me._lines.Add(String.Empty)
        Me._cursor_left = 0
        Me._cursor_top = 0
    End Sub

End Class
