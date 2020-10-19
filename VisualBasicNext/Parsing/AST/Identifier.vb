Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports VisualBasicNext.Virtual

Namespace Parsing.AST
    Public Class Identifier : Inherits Expression

        Public Class Element
            Public ReadOnly Property Item As Expression = Nothing
            Public ReadOnly Property Name As String = Nothing
            Public ReadOnly Property Generic As TypeName() = {}
            Public ReadOnly Property Access As Expression()() = {}

            Public ReadOnly Property IsIdentifier As Boolean
                Get
                    Return Me.Item Is Nothing
                End Get
            End Property

            Public Sub New(cst As CST.Node)
                If cst.First.NodeType = Parsing.CST.NodeTypes.Atom Then
                    Me.Item = FromCST(cst.First)
                Else
                    Me.Name = cst.First.Content
                End If
                If IsIdentifier Then
                    Me.Generic = _GetGeneric(cst(1))
                    Me.Access = _GetAccess(cst(2))
                Else
                    Me.Access = _GetAccess(cst(1))
                End If
            End Sub

            Private Shared Function _GetGeneric(cst As CST.Node) As TypeName()
                Dim retval As TypeName() = {}
                If cst.Count <> 0 Then
                    Dim generic_nodes As CST.Node() = {cst(0)(2)}.Concat(cst(0)(3).Select(Function(m) m.Last)).ToArray
                    retval = generic_nodes.Select(Function(g) DirectCast(FromCST(g), TypeName)).ToArray
                End If
                Return retval
            End Function

            Private Shared Function _GetAccess(cst As CST.Node) As Expression()()
                Dim access As New List(Of Expression())
                If cst.Count <> 0 Then
                    For Each n As CST.Node In cst
                        If n(1).Count = 0 Then
                            access.Add({})
                        Else
                            Dim a_nodes As CST.Node() = {n(1)(0)(0)}.Concat(n(1)(0)(1).Select(Function(m) m(1))).ToArray
                            access.Add(a_nodes.Select(Function(a) DirectCast(FromCST(a), Expression)).ToArray)
                        End If
                    Next
                End If
                Return access.ToArray
            End Function

        End Class

        Public Class Reference

            Public ReadOnly Property [Object] As Object
            Public ReadOnly Property Type As Type

            Public Sub New(obj As Object, Optional type As Type = Nothing)
                If type IsNot Nothing Then
                    If obj IsNot Nothing Then Throw New Exception("Static class cannot be defined with object reference.")
                    Me.Object = Nothing
                    Me.Type = type
                Else
                    Me.Object = obj
                    Me.Type = If(obj Is Nothing, GetType(Object), obj.GetType)
                End If
            End Sub

            Public ReadOnly Property IsStatic As Boolean
                Get
                    Return Me.Object Is Nothing
                End Get
            End Property

            Public ReadOnly Property IsLocal As Boolean
                Get
                    Return TypeOf Me.Object Is LocalVariable
                End Get
            End Property

        End Class

        Public Class Member

            Public ReadOnly Property Reference As Reference
            Public ReadOnly Property Member As MemberInfo()
            Public ReadOnly Property Getter As Func(Of Object)
            Public ReadOnly Property Setter As Action(Of Object)

            Public Sub New(ref As Reference, mem As MemberInfo(), getter As Func(Of Object), setter As Action(Of Object))
                Me.Reference = ref
                Me.Member = mem
                Me.Getter = getter
                Me.Setter = setter
            End Sub

        End Class

        Private ReadOnly _elements As Element()

        Public Sub New(cst As CST.Node)
            Dim element_nodes As CST.Node() = {cst.First.First}.Concat(cst.First.Last.Select(Function(m) m.Last)).ToArray
            Me._elements = element_nodes.Select(Function(e) New Element(e)).ToArray
        End Sub

        Public Overrides Function Evaluate(target As State) As Object
            Dim atoms As Element() = Me._elements
            Dim ref As Reference = GetReference(target, atoms)
            If ref.IsLocal Then ref = New Reference(DirectCast(ref.Object, LocalVariable).Value)
            If ref.IsStatic AndAlso atoms.Count = 0 Then Throw New Exception("Static type does not yield a valid expression value.")
            ref = ResolveAtomAccess(ref, target, atoms.First)
            atoms = atoms.Skip(1).ToArray
            For Each atom As Element In atoms
                Dim mem As Member = GetMember(ref, target, atom)
                ref = ResolveMemberAccess(mem, target, atom)
            Next
            Return ref.Object
        End Function

        Public Shared Function GetReference(target As State, ByRef atoms As Element()) As Reference
            Dim retval As Reference = Nothing
            Dim generics As New List(Of Type)
            If atoms.First.IsIdentifier Then
                If target.IsDeclared(atoms.First.Name) Then
                    retval = New Reference(target.Variable(atoms.First.Name))
                    If atoms.First.Generic.Count <> 0 Then Throw New Exception("Local variable can not have generic parameters.")
                Else
                    Dim ns As String = target.GetNamespace(String.Join("."c, atoms.Select(Function(e) e.Name).ToArray))
                    Dim import As String = If(ns.Contains("|"c), ns.Split("|").First, "")
                    ns = ns.Split("|").Last
                    Dim skip As Integer = If(ns.Equals(String.Empty), 0, ns.Split(".").Count)
                    If atoms.Take(skip).Any(Function(n) n.Access.Count <> 0 Or n.Generic.Count <> 0) Then _
                        Throw New Exception("Namespace cannot have generic types or array.")
                    atoms = atoms.Skip(skip).ToArray
                    If atoms.Count = 0 Then Throw New Exception("Namespaces are not valid as qualifiers.")
                    Dim t As Type = target.GetType(import & "." & ns & "." & GetTypeName(atoms.First))
                    If t Is Nothing Then Throw New Exception($"Type {atoms.First.Name} is not defined.")
                    generics.AddRange(atoms.First.Generic.Select(Function(g) DirectCast(g.Evaluate(target), Type)).ToArray)
                    If atoms.First.Access.Count <> 0 Then Throw New Exception("Static type cannot be array.")
                    While atoms.Count > 1
                        If t.GetNestedType(atoms(1).Name) IsNot Nothing Then
                            atoms = atoms.Skip(1).ToArray
                            t = t.GetNestedType(atoms.First.Name)
                            If atoms.First.Generic.Count <> 0 AndAlso Not t.IsGenericType Then _
                                Throw New Exception("Type is not a generic type.")
                            generics.AddRange(atoms.First.Generic.Select(Function(g) DirectCast(g.Evaluate(target), Type)).ToArray)
                            If atoms.First.Access.Count <> 0 Then Throw New Exception("Nested static class is not method or array.")
                        Else
                            Exit While
                        End If
                    End While
                    If t.IsGenericType Then t = t.MakeGenericType(generics.ToArray)
                    retval = New Reference(Nothing, t)
                End If
            Else
                Dim obj As Object = atoms.First.Item.Evaluate(target)
                retval = New Reference(obj)
            End If
            Return retval
        End Function

        Public Shared Function ResolveAtomAccess(ref As Reference, target As State, atom As Element) As Reference
            Dim retval As Reference = ref
            For Each access As Expression() In atom.Access
                Dim values As Object() = access.Select(Function(a) a.Evaluate(target)).ToArray
                If retval.Type.IsArray Then
                    retval = New Reference(DirectCast(retval.Object, Array).GetValue(values.Select(Function(v) CInt(v)).ToArray))
                Else
                    Dim def As Member = GetDefaultMember(retval, values)
                    If def Is Nothing Then Throw New Exception($"No default member found in {retval.Type.Name}.")
                    retval = New Reference(def.Getter.Invoke)
                End If
            Next
            Return retval
        End Function

        Public Shared Function GetDefaultMember(ref As Reference, arguments As Object()) As Member
            Dim cand As MemberInfo() = ref.Type.GetDefaultMembers()
            Dim retval As Member = Nothing
            If cand.Count <> 0 Then
                Dim signature As Type() = arguments.Select(Function(a) a.GetType).ToArray
                Select Case cand.First.MemberType
                    Case MemberTypes.Property
                        Dim m As PropertyInfo = cand.First(Function(c) DirectCast(c, PropertyInfo).GetIndexParameters.Select(Function(p) p.ParameterType).SequenceEqual(signature))
                        retval = New Member(ref, {m}, Function() m.GetValue(ref.Object, arguments), Sub(o As Object) m.SetValue(ref.Object, o, arguments))
                    Case Else
                        Throw New Exception("Invalid default member type.")
                End Select
            End If
            Return retval
        End Function

        Public Shared Function GetMember(ref As Reference, target As State, atom As Element) As Member
            Dim binding As BindingFlags = BindingFlags.Public Or If(ref.IsStatic, BindingFlags.Static, BindingFlags.Instance)
            Dim members As MemberInfo() = ref.Type.GetMembers(binding).Where(Function(m) m.Name.Equals(atom.Name)).ToArray
            If members.Count = 0 AndAlso Not ref.IsStatic Then members = GetExtionsMethod(target, ref, atom)
            If members.Count = 0 Then Throw New Exception($"{atom.Name} is not a member of {ref.Type.Name}.")
            Dim retval As Member = Nothing
            Select Case members.First.MemberType
                Case MemberTypes.Field
                    If atom.Generic.Count <> 0 Then Throw New Exception("Field identifier cannot have generic arguments.")
                    Dim field As FieldInfo = members.First
                    retval = New Member(ref, members, Function() field.GetValue(ref.Object), Sub(o As Object) field.SetValue(ref.Object, o))
                Case MemberTypes.Property
                    If atom.Generic.Count <> 0 Then Throw New Exception("Property identifier cannot have generic arguments.")
                    retval = New Member(ref, members, Nothing, Nothing)
                Case MemberTypes.Method
                    If atom.Generic.Count <> 0 Then
                        Dim gtypes As Type() = atom.Generic.Select(Function(g) DirectCast(g.Evaluate(target), Type)).ToArray
                        members = members.Select(Function(c) DirectCast(DirectCast(c, MethodInfo).MakeGenericMethod(gtypes), MemberInfo)).ToArray
                    End If
                    retval = New Member(ref, members, Nothing, Nothing)
                Case MemberTypes.Event
                    If atom.Generic.Count <> 0 Then Throw New Exception("Event identifier cannot have generic arguments.")
                    retval = New Member(ref, members, Nothing, Nothing)
            End Select
            Return retval
        End Function

        Public Shared Function GetExtionsMethod(target As State, ref As Reference, atom As Element) As MemberInfo()
            Dim methods As MethodInfo() = target.GetExtensions(ref.Type)
            Return methods.Where(Function(m) m.Name.Equals(atom.Name)).ToArray
        End Function

        Public Shared Function ResolveMemberAccess(member As Member, target As State, atom As Element) As Reference
            Dim retval As Member = member
            If member.Getter IsNot Nothing Then retval = New Member(New Reference(member.Getter.Invoke), {}, Nothing, Nothing)
            For Each access As Expression() In atom.Access
                Dim arguments As Object() = access.Select(Function(a) a.Evaluate(target)).ToArray
                If retval.Member.Count = 0 Then
                    If retval.Reference.Type.IsArray Then
                        retval = New Member(New Reference(DirectCast(retval.Reference.Object, Array).GetValue(arguments.Select(Function(v) CInt(v)).ToArray)), {}, Nothing, Nothing)
                    Else
                        Dim def As Member = GetDefaultMember(retval.Reference, arguments)
                        If def Is Nothing Then Throw New Exception($"No default member found in {retval.Reference.Type.Name}.")
                        retval = New Member(New Reference(def.Getter.Invoke), {}, Nothing, Nothing)
                    End If
                Else
                    Dim signature As Type() = arguments.Select(Function(a) a.GetType).ToArray
                    Select Case retval.Member.First.MemberType
                        Case MemberTypes.Property
                            Dim p As PropertyInfo = member.Member.First(Function(c) _SignatureMatch(DirectCast(c, PropertyInfo), signature))
                            retval = New Member(New Reference(p.GetValue(retval.Reference.Object, arguments)), {}, Nothing, Nothing)
                        Case MemberTypes.Method
                            'TODO: check if argument types are assignable from signature instead of sequence equals
                            If member.Member.First.IsDefined(GetType(ExtensionAttribute), False) Then
                                signature = {member.Reference.Type}.Concat(signature).ToArray
                                arguments = {member.Reference.Object}.Concat(arguments).ToArray
                                Dim m As MethodInfo = member.Member.First(Function(c) _SignatureMatch(DirectCast(c, MethodInfo), signature))
                                retval = New Member(New Reference(m.Invoke(retval.Reference.Object, arguments)), {}, Nothing, Nothing)
                            Else
                                Dim m As MethodInfo = member.Member.First(Function(c) _SignatureMatch(DirectCast(c, MethodInfo), signature))
                                retval = New Member(New Reference(m.Invoke(retval.Reference.Object, arguments)), {}, Nothing, Nothing)
                            End If
                        Case Else
                            Throw New Exception($"Unable to evaluate member of type {retval.Member.First.MemberType.ToString()}")
                    End Select
                End If
            Next
            If retval.Member.Count <> 0 Then
                If retval.Member.Count = 1 Then
                    Select Case retval.Member.First.MemberType
                        Case MemberTypes.Property
                            Dim p As PropertyInfo = retval.Member.First
                            If p.GetIndexParameters.Count = 0 Then
                                retval = New Member(New Reference(p.GetValue(retval.Reference.Object)), {}, Nothing, Nothing)
                            Else
                                Throw New Exception("No property without paramters found.")
                            End If
                        Case MemberTypes.Method
                            Dim m As MethodInfo = retval.Member.First
                            If m.GetParameters.Count = 0 Then
                                retval = New Member(New Reference(m.Invoke(retval.Reference.Object, {})), {}, Nothing, Nothing)
                            Else
                                Throw New Exception("No matching method declaration found.")
                            End If
                        Case Else
                            Throw New Exception("Unable to access object member.")
                    End Select
                End If
            End If
            Return retval.Reference
        End Function

        Private Shared Function _SignatureMatch(method As MethodInfo, signature As Type()) As Boolean
            Dim args As ParameterInfo() = method.GetParameters
            If args.Count <> signature.Count Then Return False
            For i As Integer = 0 To args.Count - 1
                If Not args(i).ParameterType.IsAssignableFrom(signature(i)) Then Return False
            Next
            Return True
        End Function

        Private Shared Function _SignatureMatch(prop As PropertyInfo, signature As Type()) As Boolean
            Dim args As ParameterInfo() = prop.GetIndexParameters
            If args.Count <> signature.Count Then Return False
            For i As Integer = 0 To args.Count - 1
                If Not args(i).ParameterType.IsAssignableFrom(signature(i)) Then Return False
            Next
            Return True
        End Function

        Private Shared Function GetTypeName(atom As Element) As String
            Return atom.Name & If(atom.Generic.Count <> 0, "`" & atom.Generic.Count.ToString, "")
        End Function

    End Class

End Namespace
