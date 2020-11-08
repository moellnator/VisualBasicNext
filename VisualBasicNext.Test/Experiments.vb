<TestClass>
Public Class Experiments

    <TestMethod>
    Public Sub Experiment()
        Dim a As String = "1"
        a = a & 2
    End Sub

    Public Class TestClass

        Public Shared Operator ^(a As TestClass, b As TestClass) As TestClass
            Return a
        End Operator

    End Class

End Class
