Imports System.Reflection
Imports VisualBasicNext.Syntax.Lexing
Imports VisualBasicNext.Syntax.Parsing
Imports VisualBasicNext.TypeExtensions

Namespace Binding
    Public Class BoundBinaryOperator : Inherits BoundNode

        Private ReadOnly _internal As Func(Of Object, Object, Object)
        Public Property ReturnType As Type


        Public Sub New(syntax As SyntaxNode, internal As Func(Of Object, Object, Object), returnType As Type)
            MyBase.New(syntax, BoundNodeKind.BoundBinaryOperator)
            Me._internal = internal
            Me.ReturnType = returnType
        End Sub

        Public Shared Function Bind(syntax As SyntaxNode, left As BoundExpression, right As BoundExpression)
            Dim returntype As Type = Nothing
            Dim internal As Func(Of Object, Object, Object) = Nothing
            If left.BoundType.IsPrimitive And right.BoundType.IsPrimitive Then
                internal = _ResolveOperation(syntax.Kind, left.BoundType, right.BoundType)
                returntype = _ResolveForPrimitive(left.BoundType, right.BoundType, internal)
                If internal Is Nothing Then Return Nothing
            ElseIf left.BoundType.Equals(GetType(Object)) Or right.BoundType.Equals(GetType(Object)) Then
                internal = _ResolveOperation(syntax.Kind, left.BoundType, right.BoundType)
                returntype = GetType(Object)
            Else
                If syntax.Kind = SyntaxKind.IsKeywordToken Or syntax.Kind = SyntaxKind.IsNotKeywordToken Then
                    internal = If(
                        syntax.Kind = SyntaxKind.IsKeywordToken,
                        Function(a As Object, b As Object) a Is b,
                        Function(a As Object, b As Object) a IsNot b
                    )
                    returntype = GetType(Boolean)
                ElseIf syntax.Kind = SyntaxKind.AmpersandToken AndAlso (left.BoundType.Equals(GetType(String)) Or right.BoundType.Equals(GetType(String))) Then
                    Dim can_cast As Boolean = False
                    If left.BoundType.GetType.Equals(GetType(String)) Then
                        can_cast = right.BoundType.IsCastableTo(GetType(String))
                    Else
                        can_cast = left.BoundType.IsCastableTo(GetType(String))
                    End If
                    If Not can_cast Then
                        Dim method As MethodInfo = _ResolveCustomOperation(syntax.Kind, left.BoundType, right.BoundType)
                        If method Is Nothing Then Return Nothing
                        returntype = method.ReturnType
                        internal = Function(a As Object, b As Object) method.Invoke(Nothing, {a, b})
                    Else
                        returntype = GetType(String)
                        internal = Function(a As Object, b As Object) CStr(a) & CStr(b)
                    End If
                Else
                    Dim method As MethodInfo = _ResolveCustomOperation(syntax.Kind, left.BoundType, right.BoundType)
                    If method Is Nothing Then Return Nothing
                    returntype = method.ReturnType
                    internal = Function(a As Object, b As Object) method.Invoke(Nothing, {a, b})
                End If
            End If
            Return New BoundBinaryOperator(syntax, internal, returntype)
        End Function

        Private Shared Function _ResolveCustomOperation(op As SyntaxKind, left As Type, right As Type) As MethodInfo
            Dim op_name As String = ""
            Select Case op
                Case SyntaxKind.XorKeywordToken
                    op_name = "op_ExclusiveOr"
                Case SyntaxKind.AndKeywordToken
                    op_name = "op_BitwiseAnd"
                Case SyntaxKind.AndAlsoKeywordToken
                    Return Nothing
                Case SyntaxKind.OrKeywordToken
                    op_name = "op_BitwiseOr"
                Case SyntaxKind.OrElseKeywordToken
                    Return Nothing
                Case SyntaxKind.LikeKeywordToken
                    op_name = "op_Like"
                Case SyntaxKind.GreaterEqualsToken
                    op_name = "op_GreaterThanOrEqual"
                Case SyntaxKind.GreaterToken
                    op_name = "op_GreaterThan"
                Case SyntaxKind.LowerEqualsToken
                    op_name = "op_LessThanOrEqual"
                Case SyntaxKind.LowerToken
                    op_name = "op_LessThan"
                Case SyntaxKind.EqualsToken
                    op_name = "op_Equality"
                Case SyntaxKind.LowerGreaterToken
                    op_name = "op_Inequality"
                Case SyntaxKind.GreaterGreaterToken
                    op_name = "op_RightShift"
                Case SyntaxKind.LowerLowerToken
                    op_name = "op_LeftShift"
                Case SyntaxKind.PlusToken
                    op_name = "op_Addition"
                Case SyntaxKind.MinusToken
                    op_name = "op_Subtraction"
                Case SyntaxKind.ModKeywordToken
                    op_name = "op_Modulus"
                Case SyntaxKind.StarToken
                    op_name = "op_Multiplication"
                Case SyntaxKind.SlashToken
                    op_name = "op_Division"
                Case SyntaxKind.BackslashToken
                    op_name = "op_IntegerDivision"
                Case SyntaxKind.AmpersandToken
                    op_name = "op_Concatenate"
                Case SyntaxKind.CircumflexToken
                    op_name = "op_Exponent"
                Case Else
                    Throw New InvalidOperationException($"Invalid syntax kind for binary operation {op.ToString}.")
            End Select
            Dim binding As BindingFlags = BindingFlags.Public Or BindingFlags.Static
            Dim methods As MethodInfo() = left.GetMethods(binding).Concat(right.GetMethods(binding)).Where(
                Function(m) m.Name = op_name And m.GetParameters.Count = 2
            ).ToArray
            Return _MatchTypes(methods, left, right)
        End Function

        Private Shared Function _MatchTypes(methods As MethodInfo(), left As Type, right As Type) As MethodInfo
            Dim retval As MethodInfo = methods.FirstOrDefault(
                Function(m) m.GetParameters.First.ParameterType = left And m.GetParameters.Last.ParameterType = right
            )
            If retval IsNot Nothing Then
                Return retval
            Else
                retval = methods.FirstOrDefault(
                    Function(m) m.GetParameters.First.ParameterType.IsAssignableFrom(left) And m.GetParameters.Last.ParameterType = right Or
                                m.GetParameters.First.ParameterType = left And m.GetParameters.Last.ParameterType.IsAssignableFrom(right)
                )
                If retval IsNot Nothing Then
                    Return retval
                Else
                    Return methods.FirstOrDefault(
                        Function(m) m.GetParameters.First.ParameterType.IsAssignableFrom(left) And m.GetParameters.Last.ParameterType.IsAssignableFrom(right)
                    )
                End If
            End If
        End Function

        Private Shared Function _ResolveOperation(op As SyntaxKind, left As Type, right As Type) As Func(Of Object, Object, Object)
            Dim retval As Func(Of Object, Object, Object) = Nothing
            Select Case op
                Case SyntaxKind.XorKeywordToken
                    retval = Function(a As Object, b As Object) a Xor b
                Case SyntaxKind.AndKeywordToken
                    retval = Function(a As Object, b As Object) a And b
                Case SyntaxKind.AndAlsoKeywordToken
                    retval = Function(a As Object, b As Object) a AndAlso b
                Case SyntaxKind.OrKeywordToken
                    retval = Function(a As Object, b As Object) a Or b
                Case SyntaxKind.OrElseKeywordToken
                    retval = Function(a As Object, b As Object) a OrElse b
                Case SyntaxKind.LikeKeywordToken
                    retval = Function(a As Object, b As Object) a Like b
                Case SyntaxKind.IsKeywordToken
                    retval = Function(a As Object, b As Object) a Is b
                Case SyntaxKind.IsNotKeywordToken
                    retval = Function(a As Object, b As Object) a IsNot b
                Case SyntaxKind.GreaterEqualsToken
                    retval = Function(a As Object, b As Object) a >= b
                Case SyntaxKind.GreaterToken
                    retval = Function(a As Object, b As Object) a > b
                Case SyntaxKind.LowerEqualsToken
                    retval = Function(a As Object, b As Object) a <= b
                Case SyntaxKind.LowerToken
                    retval = Function(a As Object, b As Object) a < b
                Case SyntaxKind.EqualsToken
                    retval = Function(a As Object, b As Object) a = b
                Case SyntaxKind.LowerGreaterToken
                    retval = Function(a As Object, b As Object) a <> b
                Case SyntaxKind.GreaterGreaterToken
                    retval = Function(a As Object, b As Object) a >> b
                Case SyntaxKind.LowerLowerToken
                    retval = Function(a As Object, b As Object) a << b
                Case SyntaxKind.PlusToken
                    retval = Function(a As Object, b As Object) a + b
                Case SyntaxKind.MinusToken
                    retval = Function(a As Object, b As Object) a - b
                Case SyntaxKind.ModKeywordToken
                    retval = Function(a As Object, b As Object) a Mod b
                Case SyntaxKind.StarToken
                    retval = Function(a As Object, b As Object) a * b
                Case SyntaxKind.SlashToken
                    retval = Function(a As Object, b As Object) a / b
                Case SyntaxKind.BackslashToken
                    retval = Function(a As Object, b As Object) a \ b
                Case SyntaxKind.AmpersandToken
                    retval = Function(a As Object, b As Object) a & b
                Case SyntaxKind.CircumflexToken
                    retval = Function(a As Object, b As Object) a ^ b
                Case Else
                    Throw New InvalidOperationException($"Invalid syntax kind for binary operation {op.ToString}.")
            End Select
            Return retval
        End Function

        Private Shared Function _ResolveForPrimitive(left As Type, right As Type, op As Func(Of Object, Object, Object)) As Type
            Return op(CTypeDynamic(Nothing, left), CTypeDynamic(1, right)).GetType
        End Function

        Public Function Invoke(left As Object, right As Object) As Object
            Return Me._internal.Invoke(left, right)
        End Function

    End Class

End Namespace
