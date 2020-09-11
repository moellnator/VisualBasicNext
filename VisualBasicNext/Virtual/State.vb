Namespace Virtual
    Public Class State

        Private ReadOnly _variables As New Dictionary(Of String, Object)

        Public Sub DeclareLocal(name As String, Optional value As Object = Nothing)
            If Me._variables.ContainsKey(name) Then Throw New Exception("Cannot declare variable '{name}': Variable already exists.")
            Me._variables.Add(name, value)
        End Sub

        Public Property Variable(name As String) As Object
            Get
                If Not Me._variables.ContainsKey(name) Then Throw New Exception($"Variable '{name}' is not declared.")
                Return Me._variables(name)
            End Get
            Set(value As Object)
                If Me._variables.ContainsKey(name) Then Me._variables.Remove(name)
                DeclareLocal(name, value)
            End Set
        End Property

        Public Function IsDeclared(name As String) As Boolean
            Return Me._variables.ContainsKey(name)
        End Function

    End Class

End Namespace
