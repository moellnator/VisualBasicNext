Imports System.Reflection
Imports VisualBasicNext.Virtual

Namespace Parsing.AST
    Public Class Identifier : Inherits Expression

        Private ReadOnly _items As TypeName.Atom()

        Public Sub New(cst As CST.Node)
            Dim subitems As CST.Node() = {cst.First}.Concat(cst.First()(2).Select(Function(c) c.Last)).ToArray
            Me._items = subitems.Select(Function(s) New TypeName.Atom(s)).ToArray
        End Sub

        Public Overrides Function Evaluate(target As State) As Object
            Dim obj As Object = Nothing
            Dim base As Type = GetType(Object)
            Dim atoms As TypeName.Atom() = Me._items
            Dim generic As Type() = Me._items.SelectMany(Function(c) c.Generics.Select(Function(g) DirectCast(g.Evaluate(target), Type))).ToArray
            Dim generic_offset As Integer = 0
            If target.IsDeclared(atoms.First.Name) AndAlso Not atoms.First.Generics.Count <> 0 Then
                atoms = atoms.Skip(1).ToArray
                obj = target.Variable(atoms.First.Name)
                base = obj.GetType
            Else
                Dim result As KeyValuePair(Of TypeName.Atom(), Type) = TypeName.TypeFromAtoms(atoms, target)
                base = result.Value
                atoms = atoms.Skip(result.Key.Count).ToArray
                generic_offset = result.Key.Sum(Function(a) a.Generics.Count)
                For Each a As TypeName.Atom In atoms
                    Dim name As String = TypeName.GetName(a)
                    Dim nested As Type = base
                    nested = nested.GetNestedType(name)
                    If nested Is Nothing Then Exit For
                    If a.Generics.Count <> 0 Or generic_offset <> 0 Then
                        If Not nested.IsGenericType Then Throw New Exception($"Unable to make type '{nested.Name}' a generic type.")
                        generic_offset += a.Generics.Count
                        nested = nested.MakeGenericType(generic.Take(generic_offset).ToArray)
                    End If
                    atoms = atoms.Skip(1).ToArray
                    base = nested
                Next
            End If
            Dim binding As BindingFlags = BindingFlags.Public Or If(obj Is Nothing, BindingFlags.Static, 0)
            If obj Is Nothing And atoms.Count = 0 Then Throw New Exception("Identifier expected.")
            Dim retval As Object = obj
            While atoms.Count <> 0
                Dim name As String = atoms.First.Name
                Dim mem As MemberInfo() = base.GetMembers(binding).Where(Function(m) m.Name.Equals(name)).ToArray
                If mem.Count = 0 Then Throw New Exception($"No member '{name}' found in '{base.Name}'.")
                Select Case mem.First.MemberType
                    Case MemberTypes.Field
                        Dim fld As FieldInfo = base.GetField(name, binding)
                        If atoms.First.Generics.Count <> 0 Then Throw New Exception("Generic arguments not valid for fields.")
                        obj = fld.GetValue(obj)
                        base = obj.GetType
                        binding = BindingFlags.Public Or BindingFlags.Instance
                        If atoms.Count = 1 Then retval = obj
                    Case MemberTypes.Property
                        Dim pro As PropertyInfo() = base.GetProperties(binding).Where(Function(p) p.Name.Equals(name)).ToArray
                        If atoms.Count <> 1 Then Throw New Exception("Property access required an arguent list.")
                        If atoms.First.Generics.Count <> 0 Then Throw New Exception("Generic arguments not valid for properties.")
                        retval = New Tuple(Of Object, PropertyInfo())(obj, pro)
                    Case MemberTypes.Method
                        Dim meth As MethodInfo() = base.GetMethods(binding).Where(Function(p) p.Name.Equals(name)).ToArray
                        If atoms.First.Generics.Count <> 0 Then
                            meth = meth.Where(Function(m) m.IsGenericMethod AndAlso m.GetGenericArguments.Count = atoms.First.Generics.Count).ToArray
                            meth = meth.Select(Function(m) m.MakeGenericMethod(atoms.First.Generics.Select(Function(g) DirectCast(g.Evaluate(target), Type)).ToArray)).ToArray
                        End If
                        If atoms.Count <> 1 Then Throw New Exception("Method access required an arguent list.")
                        retval = New Tuple(Of Object, MethodInfo())(obj, meth)
                    Case MemberTypes.Event
                        Dim evt As EventInfo = base.GetEvent(name)
                        If atoms.Count <> 1 Then Throw New Exception("Events can not be accessed directly.")
                        retval = New Tuple(Of Object, EventInfo)(obj, evt)
                End Select
                atoms = atoms.Skip(1).ToArray
            End While
            Return retval
        End Function

    End Class

End Namespace
