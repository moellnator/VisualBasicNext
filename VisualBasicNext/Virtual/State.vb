Imports System.Reflection

Namespace Virtual
    Public Class State

        Private ReadOnly _variables As New Dictionary(Of String, Object)
        Private ReadOnly _imports As New List(Of String)
        Private _types As Type()
        Private _namespaces As String()

        Public Sub New()
            Me._update_types()
        End Sub

        Private Sub _update_types()
            Me._types = _GetAllTypes()
            Me._namespaces = _ExtractNameSpaces(Me._types)
        End Sub

        Private Shared Function _GetAllTypes() As Type()
            Dim asm As Assembly() = AppDomain.CurrentDomain.GetAssemblies
            Return asm.SelectMany(Function(a) a.GetTypes).ToArray
        End Function

        Private Shared Function _ExtractNameSpaces(typelist As IEnumerable(Of Type)) As String()
            Return typelist.Select(
                Function(t) t.FullName
            ).Where(
                Function(n) n.Contains(".")
            ).Select(
                Function(n) n.Substring(0, n.LastIndexOf("."))
            ).Distinct.ToArray
        End Function

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

        Public Sub Import(name As String)
            If Not Me._namespaces.Contains(name) Then Throw New ArgumentException($"Name is not a valid namespace '{name}'.")
            If Not Me._imports.Contains(name) Then Me._imports.Add(name)
        End Sub

        Public Function IsNamespace(name As String) As Boolean
            Return Me._namespaces.Contains(name)
        End Function

        Public Overloads Function [GetType](name As String) As Type
            Dim retval As Type = Nothing
            Dim types As IEnumerable(Of Type) = Me._types.Where(Function(t) t.FullName.EndsWith(name))
            If types.Count = 1 AndAlso types.First.FullName.Equals(name) Then
                retval = types.First
            Else
                types = Me._imports.Select(Function(i) i & "." & name).SelectMany(Function(n) Me._types.Where(Function(t) t.FullName.Equals(n)))
                If types.Count = 1 Then
                    retval = types.First
                End If
            End If
            Return retval
        End Function

    End Class

End Namespace
