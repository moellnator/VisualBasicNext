Imports System.Reflection
Imports VisualBasicNext.Virtual

Namespace Parsing.AST
    Public Class AtomAccess : Inherits Expression

        Private ReadOnly _atom As Expression
        Private ReadOnly _arguments As Expression()()

        Public Sub New(cst As CST.Node)
            Me._atom = FromCST(cst.First.First)
            Dim arglists As CST.Node = cst.First.Last
            Me._arguments = arglists.Select(Function(a) _GetArgList(a)).ToArray
        End Sub

        Private Shared Function _GetArgList(cst As CST.Node) As Expression()
            If cst(1).Count = 0 Then Return {}
            Dim args As CST.Node() = {cst(1)(0)(0)}.Concat(cst(1)(0)(1).Select(Function(c) c(1))).ToArray
            Return args.Select(Function(a) DirectCast(FromCST(a), Expression)).ToArray
        End Function

        Public Overrides Function Evaluate(target As State) As Object
            Dim retval As Object = Me._atom.Evaluate(target)
            If _arguments.Count = 0 Then Return retval
            For Each arglist As Expression() In Me._arguments
                Dim arguments As Object() = arglist.Select(Function(a) a.Evaluate(target)).ToArray
                Dim signature As Type() = arguments.Select(Function(a) a.GetType).ToArray
                Select Case retval.GetType
                    Case GetType(Tuple(Of Object, MethodInfo()))
                        Dim arg As Tuple(Of Object, MethodInfo()) = retval
                        Dim method As MethodInfo = MatchingMethod(arg.Item2, signature)
                        If method Is Nothing Then Throw New Exception("No matching method signature found.")
                        retval = method.Invoke(arg.Item1, arguments)
                    Case GetType(Tuple(Of Object, PropertyInfo()))
                        Dim arg As Tuple(Of Object, PropertyInfo()) = retval
                        Dim prop As MethodInfo() = arg.Item2.Select(Function(d) d.GetMethod).ToArray
                        Dim method As MethodInfo = MatchingMethod(prop, signature)
                        If method Is Nothing Then Throw New Exception("No matching property signature found.")
                        retval = method.Invoke(arg.Item1, arguments)
                    Case Else
                        If GetType(Array).IsAssignableFrom(retval.GetType) Then
                            retval = DirectCast(retval, Array).GetValue(arguments.Select(Function(c) CInt(c)).ToArray)
                        Else
                            Dim defaults As MethodInfo = GetDefaultMethod(target, retval, signature)
                            If defaults Is Nothing Then Throw New Exception($"No default method for type '{retval.GetType.Name}' found.")
                            retval = defaults.Invoke(retval, arguments)
                        End If
                End Select
            Next
            Return retval
        End Function

        Public Shared Function GetDefaultMethod(target As State, obj As Object, arguments As Type()) As MethodInfo
            Dim type As Type = obj.GetType
            Dim defaults As MemberInfo() = type.GetDefaultMembers
            Dim retval As MethodInfo = Nothing
            If defaults.Count <> 0 Then
                Select Case defaults.First.MemberType
                    Case MemberTypes.Property
                        Dim prop As MethodInfo() = defaults.Select(Function(d) DirectCast(d, PropertyInfo).GetMethod).ToArray
                        retval = MatchingMethod(prop, arguments)
                    Case MemberTypes.Method
                        retval = MatchingMethod(defaults.Select(Function(d) DirectCast(d, MethodInfo)).ToArray, arguments)
                    Case Else
                        Throw New Exception("Unexpted default member type.")
                End Select
            End If
            Return retval
        End Function

        Public Shared Function MatchingMethod(methods As MethodInfo(), arguments As Type()) As MethodInfo
            Return methods.FirstOrDefault(Function(m) m.GetParameters.Select(Function(p) p.ParameterType).SequenceEqual(arguments))
        End Function

    End Class

End Namespace
