<TestClass>
Public Class Experiments

    <TestMethod>
    Public Sub Experiment()
        Dim b As Integer() = New Integer() {}
    End Sub

    Public Class TestClass : Implements IEnumerable

        Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Throw New NotImplementedException()
        End Function

        Public Sub Add(value As Integer, value2 As Integer, value3 As Integer)
        End Sub

        Public Sub Add(value As Integer, value2 As Integer)
        End Sub

    End Class

End Class
