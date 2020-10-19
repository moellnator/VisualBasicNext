Imports System.Reflection
Imports System.Reflection.Emit
Imports VisualBasicNext.Virtual

Namespace Parsing.AST
    Public Class Inline : Inherits Expression

        Private Shared _CURRENT_ID As Integer = 0
        Private Shared Function _GET_NEXT_ID() As Integer
            Dim retval As Integer = _CURRENT_ID
            _CURRENT_ID += 1
            Return retval
        End Function

        Private Class _Argument
            Public ReadOnly Property Name As String
            Public ReadOnly Property Type As TypeName

            Private Sub New(name As String, type As TypeName)
                Me.Name = name
                Me.Type = type
            End Sub

            Public Sub New(cst As CST.Node)
                Me.New(cst(0).Content, FromCST(cst(2)))
            End Sub

        End Class

        Private ReadOnly _arg_list As _Argument() = {}
        Private ReadOnly _ret_type As TypeName
        Private ReadOnly _inline As Expression

        Public Sub New(cst As CST.Node)
            Dim arg_list As CST.Node = cst(0)(1)(1)
            If arg_list.Count <> 0 Then
                arg_list = arg_list(0)
                Dim arg_nodes As CST.Node() = {arg_list}.Concat(arg_list.Last.Select(Function(c) c(1))).ToArray
                Me._arg_list = arg_nodes.Select(Function(c) New _Argument(c)).ToArray
            End If
            Me._ret_type = FromCST(cst(0)(3))
            Me._inline = FromCST(cst(0)(4))
        End Sub

        Public Overrides Function Evaluate(target As State) As Object
            Dim ret_type As Type = Me._ret_type.Evaluate(target)
            Dim arg_types As Type() = Me._arg_list.Select(Function(a) DirectCast(a.Type.Evaluate(target), Type)).ToArray
            If arg_types.Count > 9 Then Throw New Exception("Lambda expression cannot have more than 9 arguments.")
            Dim context As New Context(Me._arg_list.Select(Function(a) a.Name).ToArray, arg_types)
            Dim dynamic As New DynamicInvoke(Me._inline, context, target)
            Dim method As New DynamicMethod("", ret_type, {GetType(DynamicInvoke)}.Concat(arg_types).ToArray, GetType(DynamicInvoke))
            Dim invoke As MethodInfo = dynamic.GetType.GetMethod("Invoke")
            Dim il As ILGenerator = method.GetILGenerator
            il.Emit(OpCodes.Ldarg_0)
            il.Emit(OpCodes.Ldc_I4, arg_types.Count)
            il.Emit(OpCodes.Newarr, GetType(Object))
            For index As Integer = 0 To arg_types.Count - 1
                il.Emit(OpCodes.Dup)
                il.Emit(OpCodes.Ldc_I4, index)
                il.Emit(OpCodes.Ldarg, index + 1)
                il.Emit(OpCodes.Box, arg_types(index))
                il.Emit(OpCodes.Stelem_Ref)
            Next
            il.Emit(OpCodes.Callvirt, invoke)
            il.Emit(OpCodes.Unbox_Any, ret_type)
            il.Emit(OpCodes.Ret)
            Dim m_type As Type = Type.GetType("System.Func`" & 1 + arg_types.Count)
            m_type = m_type.MakeGenericType({ret_type}.Concat(arg_types).ToArray)
            Return method.CreateDelegate(m_type, dynamic)
        End Function


        Public Class DynamicInvoke

            Private ReadOnly _anonym As Expression
            Private ReadOnly _context As Context
            Private ReadOnly _target As State

            Public Sub New(node As Expression, context As Context, target As State)
                Me._anonym = node
                Me._context = context
                Me._target = target
            End Sub

            Public Function Invoke(ParamArray arguments As Object()) As Object
                Dim retval As Object = Nothing
                Me._target.Enter(Me._context)
                Me._context.Enter(arguments)
                retval = Me._anonym.Evaluate(Me._target)
                Me._context.Leave()
                Me._target.Leave()
                Return retval
            End Function

        End Class

    End Class

End Namespace
