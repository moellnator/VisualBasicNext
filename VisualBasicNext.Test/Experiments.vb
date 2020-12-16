<TestClass>
Public Class Experiments

    <TestMethod>
    Public Sub Experiment()
        Dim a As Func(Of Integer) = Function() 0
        Dim t As Type = a.GetType
        Dim b As Char() = "hello"

    End Sub

    Public Class TestClass

        Public ReadOnly Property TestProperty As Integer() = {1, 2, 3}

        Public Function TestFunction(a As Integer) As Integer
            Return a
        End Function

    End Class

End Class
