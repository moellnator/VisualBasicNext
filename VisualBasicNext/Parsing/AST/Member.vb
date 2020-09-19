Imports System.Reflection
Imports VisualBasicNext.Virtual

Namespace Parsing.AST
    Public Class Member : Inherits Expression

        Private Class _MemberAccess

            Public ReadOnly Property Name As String
            Public ReadOnly Property Generics As TypeName() = {}
            Public ReadOnly Property Access As Expression()() = {}

            Public Sub New(cst As CST.Node)
                Me.Name = cst(1).Content
                If cst(2).Count <> 0 Then
                    Dim genericnodes As CST.Node() = {cst(2)(0)(2)}.Concat(cst(2)(0)(3).Select(Function(n) n(1))).ToArray
                    Me.Generics = genericnodes.Select(Function(n) DirectCast(FromCST(n), TypeName)).ToArray
                End If
                Dim arglists As CST.Node = cst(3)
                Me.Access = arglists.Select(Function(a) _GetArgList(a)).ToArray
            End Sub

            Private Shared Function _GetArgList(cst As CST.Node) As Expression()
                If cst(1).Count = 0 Then Return {}
                Dim args As CST.Node() = {cst(1)(0)(0)}.Concat(cst(1)(0)(1).Select(Function(c) c(1))).ToArray
                Return args.Select(Function(a) DirectCast(FromCST(a), Expression)).ToArray
            End Function

        End Class

        Private ReadOnly _atom As Expression
        Private ReadOnly _members As _MemberAccess() = {}

        Private Sub New(cst As CST.Node)
            Me._atom = FromCST(cst.First.First)
            Dim membernodes As CST.Node() = cst.First.Last.ToArray
            If membernodes.Count <> 0 Then Me._members = membernodes.Select(Function(m) New _MemberAccess(m)).ToArray
        End Sub

        Public Shared Function BuildNode(cst As CST.Node) As Expression
            Dim retval As Expression = Nothing
            If cst.First.Last.Count = 0 Then
                retval = FromCST(cst.First.First)
            Else
                retval = New Member(cst)
            End If
            Return retval
        End Function

        Public Overrides Function Evaluate(target As State) As Object
            Dim retval As Object = Me._atom.Evaluate(target)
            If Me._members.Count = 0 Then Return retval
            For Each m As _MemberAccess In Me._members
                Dim type As Type = retval.GetType
                Dim members As MemberInfo() = type.GetMember(m.Name)
                If members.Count = 0 Then Throw New Exception($"Member '{m.Name}' not found in type '{type.Name}'.")
                Dim arguments As Object()() = m.Access.Select(Function(a) a.Select(Function(b) b.Evaluate(target)).ToArray).ToArray
                Dim types As Type()() = arguments.Select(Function(a) a.Select(Function(b) b.GetType).ToArray).ToArray
                Dim binding As BindingFlags = BindingFlags.Public Or BindingFlags.Instance
                Select Case members.First.MemberType
                    Case MemberTypes.Field
                        Dim fld As FieldInfo = type.GetField(m.Name, binding)
                        If m.Generics.Count <> 0 Then Throw New Exception("Generic arguments not valid for fields.")
                        retval = fld.GetValue(retval)
                        type = retval.GetType
                    Case MemberTypes.Property
                        Dim pro As PropertyInfo() = type.GetProperties(binding).Where(Function(p) p.Name.Equals(m.Name)).ToArray
                        If m.Generics.Count <> 0 Then Throw New Exception("Generic arguments not valid for properties.")
                        retval = New Tuple(Of Object, PropertyInfo())(retval, pro)
                    Case MemberTypes.Method
                        Dim meth As MethodInfo() = type.GetMethods(binding).Where(Function(p) p.Name.Equals(m.Name)).ToArray
                        If m.Generics.Count <> 0 Then
                            meth = meth.Where(Function(met) met.IsGenericMethod AndAlso met.GetGenericArguments.Count = m.Generics.Count).ToArray
                            meth = meth.Select(Function(met) met.MakeGenericMethod(m.Generics.Select(Function(g) DirectCast(g.Evaluate(target), Type)).ToArray)).ToArray
                        End If
                        retval = New Tuple(Of Object, MethodInfo())(retval, meth)
                    Case MemberTypes.Event
                        Dim evt As EventInfo = type.GetEvent(m.Name)
                        retval = New Tuple(Of Object, EventInfo)(retval, evt)
                End Select
                If arguments.Count <> 0 Then
                    For i As Integer = 0 To arguments.Count - 1
                        Dim args As Object() = arguments(i)
                        Dim spec As Type() = types(i)
                        Select Case retval.GetType
                            Case GetType(Tuple(Of Object, MethodInfo()))
                                Dim arg As Tuple(Of Object, MethodInfo()) = retval
                                Dim method As MethodInfo = AtomAccess.MatchingMethod(arg.Item2, spec)
                                If method Is Nothing Then Throw New Exception("No matching method signature found.")
                                retval = method.Invoke(arg.Item1, args)
                            Case GetType(Tuple(Of Object, PropertyInfo()))
                                Dim arg As Tuple(Of Object, PropertyInfo()) = retval
                                Dim prop As MethodInfo() = arg.Item2.Select(Function(d) d.GetMethod).ToArray
                                Dim method As MethodInfo = AtomAccess.MatchingMethod(prop, spec)
                                If method Is Nothing Then Throw New Exception("No matching property signature found.")
                                retval = method.Invoke(arg.Item1, args)
                            Case Else
                                If GetType(Array).IsAssignableFrom(retval.GetType) Then
                                    retval = DirectCast(retval, Array).GetValue(args.Select(Function(c) CInt(c)).ToArray)
                                Else
                                    Dim defaults As MethodInfo = AtomAccess.GetDefaultMethod(target, retval, spec)
                                    If defaults Is Nothing Then Throw New Exception($"No default method for type '{retval.GetType.Name}' found.")
                                    retval = defaults.Invoke(retval, args)
                                End If
                        End Select
                    Next
                End If
            Next
            Return retval
        End Function


    End Class

End Namespace
