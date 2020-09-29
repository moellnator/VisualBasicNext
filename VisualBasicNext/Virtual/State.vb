Imports System.Reflection

Namespace Virtual
    Public Class State

        Private ReadOnly _variables As New Dictionary(Of String, LocalVariable)
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

        Public Sub DeclareLocal(name As String, type As Type, Optional value As Object = Nothing)
            If Me._variables.ContainsKey(name) Then Throw New Exception("Cannot declare variable '{name}': Variable already exists.")
            Me._variables.Add(name, New LocalVariable(name, type, value))
        End Sub

        Public ReadOnly Property Variable(name As String) As LocalVariable
            Get
                If Not Me._variables.ContainsKey(name) Then Throw New Exception($"Variable '{name}' is not declared.")
                Return Me._variables(name)
            End Get
        End Property

        Public Function IsDeclared(name As String) As Boolean
            Return Me._variables.ContainsKey(name)
        End Function

        Public Sub Import(name As String)
            If Not Me._namespaces.Contains(name) AndAlso Not Me._namespaces.Any(Function(ns) ns.StartsWith(name & ".")) Then _
                Throw New ArgumentException($"Name is not a valid namespace '{name}'.")
            If Not Me._imports.Contains(name) Then Me._imports.Add(name)
        End Sub

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

        Public Function GetNamespace(name As String) As String
            Dim retval As String = ""
            If Me._namespaces.Contains(name) Then Return name
            Dim parts As String() = name.Split("."c)
            Dim matches As New List(Of String)
            Dim current As String = ""
            For Each part As String In parts
                current &= If(current = "", part, "." & part)
                matches.AddRange(Me._imports.Where(Function(imp) Me._namespaces.Contains(imp & "." & current)).Select(Function(imp) imp & "|" & current).ToArray)
            Next
            If Not matches.Count > 0 Then
                Dim tn As String = parts.First
                matches = Me._imports.Where(Function(imp) Me._types.Any(Function(t) t.FullName.Equals(imp & "." & tn) Or t.FullName.StartsWith(imp & "." & tn & "`"))).ToList
                If Not matches.Count = 0 Then retval = matches.OrderByDescending(Function(m) m.Count).First & "|"
            Else
                retval = matches.OrderByDescending(Function(m) m.Count).First
            End If
            Return retval
        End Function

    End Class

End Namespace
