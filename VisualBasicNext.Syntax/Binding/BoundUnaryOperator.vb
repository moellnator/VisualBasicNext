Imports System.Reflection
Imports VisualBasicNext.Syntax.Lexing
Imports VisualBasicNext.Syntax.Parsing

Namespace Binding
    Public Class BoundUnaryOperator : Inherits BoundNode

        Public ReadOnly Property ReturnType As Type
        Private ReadOnly _internal_op As Func(Of Object, Object)

        Private Sub New(syntax As SyntaxNode, internal As Func(Of Object, Object), returnType As Type)
            MyBase.New(syntax, BoundNodeKind.BoundUnaryOperator)
            Me._internal_op = internal
            Me.ReturnType = returnType
        End Sub

        Public Shared Function Bind(syntax As SyntaxNode, operand As BoundExpression) As BoundUnaryOperator
            Dim internal As Func(Of Object, Object) = Nothing
            Dim returntype As Type
            If operand.BoundType.IsPrimitive Then
                internal = _ResolveOperation(syntax.Kind)
                returntype = _ResolveForPrimitive(operand.BoundType, internal)
                If returntype Is Nothing Then Return Nothing
            ElseIf operand.BoundType.Equals(GetType(Object)) Then
                internal = _ResolveOperation(syntax.Kind)
                returntype = GetType(Object)
            Else
                Dim method As MethodInfo = _ResolveCustomOperation(syntax.Kind, operand.BoundType)
                If method Is Nothing Then Return Nothing
                returntype = method.ReturnType
                internal = Function(obj As Object) method.Invoke(Nothing, obj)
            End If
            Return New BoundUnaryOperator(syntax, internal, returntype)
        End Function

        Private Shared Function _ResolveCustomOperation(op As SyntaxKind, boundType As Type) As MethodInfo
            Dim op_name As String = ""
            Select Case op
                Case SyntaxKind.PlusToken
                    op_name = "op_UnaryPlus"
                Case SyntaxKind.MinusToken
                    op_name = "op_UnaryMinus"
                Case SyntaxKind.NotKeywordToken
                    op_name = "op_OnesComplement"
                Case Else
                    Throw New InvalidOperationException($"Invalid syntax kind for unary operation {op.ToString}.")
            End Select
            Dim binding As BindingFlags = BindingFlags.Public Or BindingFlags.Static
            Dim methods As MethodInfo() = boundType.GetMethods(binding).Where(
                Function(m) m.Name = op_name And (m.GetParameters.Count = 1 AndAlso m.GetParameters.First.ParameterType = boundType)
            ).ToArray
            If methods.Count = 1 Then
                Return methods.First
            Else
                Return Nothing
            End If
        End Function

        Private Shared Function _MatchTypes(methods As MethodInfo(), type As Type) As MethodInfo
            Dim retval As MethodInfo = methods.FirstOrDefault(Function(m) m.GetParameters.First.ParameterType.Equals(type))
            If retval IsNot Nothing Then
                Return retval
            Else
                Return methods.FirstOrDefault(Function(m) m.GetParameters.First.ParameterType.IsAssignableFrom(type))
            End If
        End Function

        Private Shared Function _ResolveOperation(op As SyntaxKind) As Func(Of Object, Object)
            Dim retval As Func(Of Object, Object) = Nothing
            Select Case op
                Case SyntaxKind.PlusToken
                    retval = Function(operand As Object) +operand
                Case SyntaxKind.MinusToken
                    retval = Function(operand As Object) -operand
                Case SyntaxKind.NotKeywordToken
                    retval = Function(operand As Object) Not operand
                Case Else
                    Throw New InvalidOperationException($"Invalid syntax kind for unary operation {op.ToString}.")
            End Select
            Return retval
        End Function

        Private Shared Function _ResolveForPrimitive(primitiveType As Type, op As Func(Of Object, Object)) As Type
            Try
                Return op.Invoke(CTypeDynamic(Nothing, primitiveType)).GetType
            Catch ex As Exception
                Return Nothing
            End Try
        End Function

        Public Function Invoke(operand As Object) As Object
            Return Me._internal_op.Invoke(operand)
        End Function

    End Class

End Namespace
