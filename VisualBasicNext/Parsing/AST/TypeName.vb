Imports VisualBasicNext.Virtual

Namespace Parsing.AST
    Public Class TypeName : Inherits Node

        Public Class Atom
            Public ReadOnly Property Name As String
            Public ReadOnly Property Generics As TypeName() = {}

            Public Sub New(cst As CST.Node)
                Me.Name = cst(0).Content
                If cst(1).Count <> 0 Then
                    Dim generic_items As CST.Node() = {cst(1)(0)(2)}.Concat(cst(1)(0)(3).Select(Function(c) c(1))).ToArray
                    Me.Generics = generic_items.Select(Function(c) DirectCast(FromCST(c), TypeName)).ToArray
                End If
            End Sub

        End Class

        Private ReadOnly _items As Atom()
        Private ReadOnly _ranks As Integer() = {}

        Public Sub New(cst As CST.Node)
            Dim items As CST.Node() = {cst.First}.Concat(cst.First()(2).Select(Function(c) c.Last)).ToArray
            Me._items = items.Select(Function(c) New Atom(c)).ToArray
            Dim rank_node As CST.Node = cst.First.Last
            If rank_node.Children.Count <> 0 Then Me._ranks = rank_node.Select(Function(r) r.Content.Count(Function(c) c = ","c) + 1).ToArray
        End Sub

        Public Overrides Function Evaluate(target As State) As Object
            Dim base As KeyValuePair(Of Atom(), Type) = TypeFromAtoms(Me._items, target)
            Dim retval As Type = Nothing
            Dim generic As Type() = Me._items.SelectMany(Function(c) c.Generics.Select(Function(g) DirectCast(g.Evaluate(target), Type))).ToArray
            Dim generic_offset As Integer = base.Key.Sum(Function(a) a.Generics.Count)
            If base.Key.Count = Me._items.Count Then
                retval = base.Value
            Else
                Dim nested As Type = base.Value
                For Each a As Atom In Me._items.Skip(base.Key.Count)
                    Dim name As String = GetName(a)
                    nested = nested.GetNestedType(name)
                    If nested Is Nothing Then Throw New Exception($"Nested type '{name}' not found.")
                    If a.Generics.Count <> 0 Or generic_offset <> 0 Then
                        If Not nested.IsGenericType Then Throw New Exception($"Unable to make type '{nested.Name}' a generic type.")
                        generic_offset += a.Generics.Count
                        nested = nested.MakeGenericType(generic.Take(generic_offset).ToArray)
                    End If
                Next
                retval = nested
            End If
            If Me._ranks.Count <> 0 Then
                For i As Integer = 0 To Me._ranks.Count - 1
                    If Me._ranks(i) <> 1 Then
                        retval = retval.MakeArrayType(Me._ranks(i))
                    Else
                        retval = retval.MakeArrayType
                    End If
                Next
            End If
            Return retval
        End Function

        Public Overloads Shared Function TypeFromAtoms(atoms As Atom(), state As State) As KeyValuePair(Of Atom(), Type)
            Dim names As String() = _GetNames(atoms).Reverse.ToArray
            Dim retval As Type = Nothing
            For i = 0 To names.Count - 1
                retval = state.GetType(names(i))
                If retval IsNot Nothing Then
                    Dim subatoms As Atom() = atoms.Take(names.Count - i).ToArray
                    If subatoms.Last.Generics.Count <> 0 Then
                        If Not retval.IsGenericType Then Throw New Exception($"Unable to make type '{retval.Name}' a generic type.")
                        retval = retval.MakeGenericType(subatoms.Last.Generics.Select(Function(a) DirectCast(a.Evaluate(state), Type)).ToArray)
                    End If
                    Return New KeyValuePair(Of Atom(), Type)(subatoms, retval)
                End If
            Next
            Throw New Exception($"No matching type found for '{names.First}'.")
        End Function

        Private Shared Iterator Function _GetNames(atoms As Atom()) As IEnumerable(Of String)
            If atoms.Count = 0 Then Exit Function
            Dim retval As New Text.StringBuilder
            retval.Append(GetName(atoms.First))
            Yield retval.ToString
            For Each a As Atom In atoms.Skip(1)
                retval.Append("." & GetName(a))
                Yield retval.ToString
                If a.Generics.Count <> 0 Then Exit For
            Next
        End Function

        Public Shared Function GetName(atom As Atom) As String
            Return atom.Name & If(atom.Generics.Count <> 0, "`" & atom.Generics.Count.ToString, "")
        End Function

    End Class

End Namespace
