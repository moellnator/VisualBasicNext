Imports VisualBasicNext.Parsing.Tokenizing

Namespace Parsing.Generator
    Public Class State : Implements ICloneable, IEnumerator(Of Token)

        Private ReadOnly _tokens As Token()
        Private _index As Integer
        Private _is_disposed As Boolean = False

        Public Sub New(tokens As IEnumerable(Of Token), index As Integer)
            Me._tokens = tokens.ToArray
            Me._index = index
        End Sub

        Private Sub New(other As State)
            Me._tokens = other._tokens
            Me._index = other._index
        End Sub

        Public Function Clone() As State
            Return New State(Me)
        End Function

        Private Function IClonable_Clone() As Object Implements ICloneable.Clone
            Return Me.Clone
        End Function

        Public Sub Update(newstate As State)
            Me._index = newstate._index
        End Sub

        Public ReadOnly Property Current As Token Implements IEnumerator(Of Token).Current
            Get
                Return Me._tokens(Me._index)
            End Get
        End Property

        Private ReadOnly Property IEnumerator_Current As Object Implements IEnumerator.Current
            Get
                Return Me.Current
            End Get
        End Property

        Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
            Me._index += 1
            Return Me._index < Me._tokens.Count
        End Function

        Public Sub Reset() Implements IEnumerator.Reset
            Me._index = -1
        End Sub

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me._is_disposed Then
                If disposing Then
                End If
            End If
            Me._is_disposed = True
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
        End Sub

    End Class

End Namespace
