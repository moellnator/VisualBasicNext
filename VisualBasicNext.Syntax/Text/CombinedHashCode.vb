Namespace Text
    Public Class CombinedHashCode

        Private ReadOnly _objects As Object()

        Public Sub New(ParamArray objects As Object())
            Me._objects = objects.ToArray
            If Me._objects.Count = 0 Then Throw New ArgumentException("Objects must at least contain one object.", NameOf(objects))
        End Sub

        Public Overrides Function GetHashCode() As Integer
            Dim retval As Integer = Me._objects.First.GetHashCode
            For Each o As Object In Me._objects.Skip(1)
                retval = _CombineHashcodes(retval, o.GetHashCode)
            Next
            Return retval
        End Function

        Private Shared Function _CombineHashcodes(h1 As Integer, h2 As Integer) As Integer
            Return BitConverter.ToInt32(BitConverter.GetBytes(((CLng(h1) << 5) + h1) Xor h2), 0)
        End Function

    End Class

End Namespace
