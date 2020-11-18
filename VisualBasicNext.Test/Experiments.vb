<TestClass>
Public Class Experiments

    <TestMethod>
    Public Sub Experiment()
        Dim a As Object = New TestClass
        a = a.TestFunction(0)
    End Sub


    Public Class TestClass

        Public ReadOnly Property TestProperty As Integer() = {1, 2, 3}

        Public Function TestFunction(a As Integer) As Integer
            Return a
        End Function

    End Class

End Class
