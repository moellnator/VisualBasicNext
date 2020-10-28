Namespace Diagnostics
    Public Class ErrorList : Implements IReadOnlyList(Of ErrorObject)

        Private ReadOnly _content As List(Of ErrorObject)

        Public Sub New()
            Me._content = New List(Of ErrorObject)
        End Sub

        Public Sub New(previous As ErrorList)
            Me._content.AddRange(previous)
        End Sub

        Public Sub ReportBadCharakter(input As Char, span As Text.Span)
            Me._content.Add(New ErrorObject($"Invalid character '{input}' in source at {span.GetStartPosition.ToString}.", span))
        End Sub

        Public Sub ReportBadConversion(fromType As Type, toType As Type, span As Text.Span)
            Me._content.Add(New ErrorObject($"Invalid conversion from type ({fromType.ToString}) to ({toType.ToString}) in source at {span.GetStartPosition.ToString}.", span))
        End Sub

        Public Sub ReportBadLiteral(literal As String, target As Type, span As Text.Span)
            Me._content.Add(New ErrorObject($"Literal '{literal}' does not represent a value of type ({target}) in source at {span.GetStartPosition.ToString}.", span))
        End Sub

        Public Sub ReportMissing(text As String, span As Text.Span)
            Me._content.Add(New ErrorObject($"Expected missing '{text}' in source at {span.GetStartPosition.ToString}.", span))
        End Sub

        Default Public ReadOnly Property Item(index As Integer) As ErrorObject Implements IReadOnlyList(Of ErrorObject).Item
            Get
                Return Me._content.Item(index)
            End Get
        End Property

        Public ReadOnly Property Count As Integer Implements IReadOnlyCollection(Of ErrorObject).Count
            Get
                Return Me._content.Count
            End Get
        End Property

        Public Function GetEnumerator() As IEnumerator(Of ErrorObject) Implements IEnumerable(Of ErrorObject).GetEnumerator
            Return Me._content.GetEnumerator
        End Function

        Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Return Me.GetEnumerator
        End Function

        Public Sub Clear()
            Me._content.Clear()
        End Sub

    End Class

End Namespace
