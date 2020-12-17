Imports VisualBasicNext.CodeAnalysis.Binding
Imports VisualBasicNext.CodeAnalysis.Diagnostics
Imports VisualBasicNext.CodeAnalysis.Symbols

Namespace Evaluating
    Friend Class Evaluator

        Public ReadOnly Property Diagnostics As New ErrorList

        Private ReadOnly _local_variables As New Stack(Of Dictionary(Of Symbol, Object))

        Public Sub New(script As BoundScript, state As VMState)
            Me.Script = script
            Me.State = state
            Me._local_variables.Push(New Dictionary(Of Symbol, Object))
        End Sub

        Public ReadOnly Property Script As BoundScript
        Public ReadOnly Property State As VMState

        Public Function Evaluate() As Object
            Dim retval As Object = Nothing
            Try
                For Each statement As BoundStatement In Me.Script.Statements
                    retval = Me._EvaluateStatement(statement)
                Next
            Catch ex As EvaluationException
                Me.Diagnostics.ReportRuntimeException(ex.InnerException, ex.Syntax)
            End Try
            Return retval
        End Function

        Private Function _EvaluateStatement(statement As BoundStatement) As Object
            Dim retval As Object = Nothing

            Select Case statement.Kind
                Case BoundNodeKind.BoundVariableDeclarationStatement
                    Me._EvaluateVariableDeclarationStatement(statement)
                Case BoundNodeKind.BoundExpressionStatement
                    retval = Me._EvaluateExpressionStatement(statement)
                Case BoundNodeKind.BoundImportStatement
                Case Else
                    Throw New Exception($"Unknown statement in evaluator: '{statement.Kind.ToString}'.")
            End Select

            Return retval
        End Function

        Private Sub _EvaluateVariableDeclarationStatement(statement As BoundVariableDeclarationStatement)
            Dim initial As Object = Nothing
            If statement.Initializer IsNot Nothing Then
                initial = Me._EvaluateExpression(statement.Initializer)
                Me._Assign(statement.Symbol, initial, statement.Initializer.Syntax.Span)
            Else
                initial = CTypeDynamic(Nothing, DirectCast(statement.Symbol, VariableSymbol).Type)
                Me._Assign(statement.Symbol, initial, statement.Syntax.Span)
            End If
        End Sub

        Private Sub _Assign(symbol As VariableSymbol, value As Object, span As Text.Span)
            Try
                Dim converted As Object = CTypeDynamic(value, symbol.Type)
                If symbol.Kind = SymbolKinds.GlobalVariable Then
                    Me.State.Variable(symbol) = converted
                Else
                    Dim local As Dictionary(Of Symbol, Object) = Me._local_variables.Peek
                    If local.ContainsKey(symbol) Then local.Remove(symbol)
                    local.Add(symbol, converted)
                End If
            Catch icex As InvalidCastException
                Me.Diagnostics.ReportInvalidConversion(value.GetType, symbol.Type, span)
            Catch ex As Exception
                Throw New EvaluationException(ex, span)
            End Try
        End Sub

        Private Function _EvaluateExpressionStatement(statement As BoundExpressionStatement) As Object
            Return Me._EvaluateExpression(statement.Expression)
        End Function

        Private Function _EvaluateExpression(expression As BoundExpression) As Object
            If expression.Constant IsNot Nothing Then Return expression.Constant.Value
            Select Case expression.Kind
                Case BoundNodeKind.BoundLiteral
                    Return Me._EvaluateLiteralExpression(expression)
                Case BoundNodeKind.BoundVariableExpression
                    Return Me._EvaluateVariableExpression(expression)
                Case BoundNodeKind.BoundCastExpression
                    Return Me._EvaluateCastExpression(expression)
                Case BoundNodeKind.BoundCastDynamicExpression
                    Return Me._EvaluateCastDynamicExpression(expression)
                Case BoundNodeKind.BoundGetTypeExpression
                    Return Me._EvaluateGetTypeExpression(expression)
                Case BoundNodeKind.BoundTernaryExpression
                    Return Me._EvaluateTernaryExpression(expression)
                Case BoundNodeKind.BoundNullCheckExpression
                    Return Me._EvaluateNullCheckExpression(expression)
                Case BoundNodeKind.BoundArrayExpression
                    Return Me._EvaluateArrayExpression(expression)
                Case BoundNodeKind.BoundUnaryExpression
                    Return Me._EvaluateUnaryExpression(expression)
                Case BoundNodeKind.BoundBinaryExpression
                    Return Me._EvaluateBinaryExpression(expression)
                Case BoundNodeKind.BoundExtrapolatedStringExpression
                    Return Me._EvaluateExtrapolatedStringExpression(expression)
                Case BoundNodeKind.BoundTryCastExpression
                    Return Me._EvaluateTryCastExpression(expression)
                Case BoundNodeKind.BoundArrayAccessExpression
                    Return Me._EvaluateArrayAccessExpression(expression)
                Case BoundNodeKind.BoundInstanceFieldGetExpression
                    Return Me._EvaluateInstanceFieldGetExpression(expression)
                Case BoundNodeKind.BoundInstanceMethodInvokationExpression
                    Return Me._EvaluateInstanceMethodInvokationExpression(expression)
                Case BoundNodeKind.BoundInstancePropertyGetExpression
                    Return Me._EvaluateInstancePropertyGetExpression(expression)
                Case BoundNodeKind.BoundClassMethodInvokationExpression
                    Return Me._EvaluateClassMethodInvokationExpression(expression)
                Case BoundNodeKind.BoundEnumerableItemAccessExpression
                    Return Me._EvaluateEnumerableItemAccessExpression(expression)
                Case Else
                    Throw New Exception($"Unknown expression in evaluator: '{expression.Kind.ToString}'.")
            End Select
        End Function

        Private Function _EvaluateEnumerableItemAccessExpression(expression As BoundEnumerableItemAccessExpression) As Object
            Try
                Dim index As Integer = Me._EvaluateExpression(expression.Index)
                Dim source As IEnumerable = Me._EvaluateExpression(expression.Source)
                Return expression.AccessMethod(source, index)
            Catch ex As Exception
                Throw New EvaluationException(ex, expression.Syntax.Span)
            End Try
        End Function

        Private Function _EvaluateClassMethodInvokationExpression(expression As BoundClassMethodInvokationExpression) As Object
            Try
                Dim arguments As Object() = expression.Arguments.Select(Function(arg) Me._EvaluateExpression(arg)).ToArray
                Return expression.Member.Invoke(Nothing, arguments)
            Catch ex As Exception
                Throw New EvaluationException(ex, expression.Syntax.Span)
            End Try
        End Function

        Private Function _EvaluateInstanceFieldGetExpression(expression As BoundInstanceFieldGetExpression) As Object
            Try
                Dim obj As Object = Me._EvaluateExpression(expression.Source)
                Return expression.Member.GetValue(obj)
            Catch ex As Exception
                Throw New EvaluationException(ex, expression.Syntax.Span)
            End Try
        End Function

        Private Function _EvaluateInstanceMethodInvokationExpression(expression As BoundInstanceMethodInvokationExpression) As Object
            Try
                Dim obj As Object = Me._EvaluateExpression(expression.Source)
                Dim arguments As Object() = expression.Arguments.Select(Function(arg) Me._EvaluateExpression(arg)).ToArray
                Return expression.Member.Invoke(obj, arguments)
            Catch ex As Exception
                Throw New EvaluationException(ex, expression.Syntax.Span)
            End Try
        End Function

        Private Function _EvaluateInstancePropertyGetExpression(expression As BoundInstancePropertyGetExpression) As Object
            Try
                Dim obj As Object = Me._EvaluateExpression(expression.Source)
                Dim arguments As Object() = expression.Arguments.Select(Function(arg) Me._EvaluateExpression(arg)).ToArray
                Return expression.Member.GetValue(obj, arguments)
            Catch ex As Exception
                Throw New EvaluationException(ex, expression.Syntax.Span)
            End Try
        End Function

        Private Function _EvaluateArrayAccessExpression(expression As BoundArrayAccessExpression) As Object
            Try
                Dim index As Integer() = expression.Index.Select(Function(a) Me._EvaluateExpression(a)).Cast(Of Integer).ToArray
                Dim source As Array = Me._EvaluateExpression(expression.Source)
                Return source.GetValue(index)
            Catch ex As Exception
                Throw New EvaluationException(ex, expression.Syntax.Span)
            End Try
        End Function

        Private Function _EvaluateTryCastExpression(expression As BoundTryCastExpression) As Object
            Dim value As Object = Me._EvaluateExpression(expression.Expression)
            Try
                Return CTypeDynamic(value, expression.Target)
            Catch ex As Exception
                Return Nothing
            End Try
        End Function

        Private Function _EvaluateExtrapolatedStringExpression(expression As BoundExtrapolatedStringExpression) As Object
            Dim retval As New System.Text.StringBuilder
            Dim expr As BoundExpression = Nothing
            retval.Append(expression.Start)
            For index As Integer = 0 To expression.Expressions.Length - 1
                Try
                    expr = expression.Expressions(index)
                    Dim value As String = Me._EvaluateExpression(expr)
                    retval.Append(value)
                    retval.Append(expression.Remainders(index))
                Catch ex As InvalidCastException
                    Me.Diagnostics.ReportInvalidConversion(expr.BoundType, GetType(String), expr.Syntax.Span)
                    Return Nothing
                Catch ex As Exception
                    Throw New EvaluationException(ex, expression.Syntax.Span)
                End Try
            Next
            Return retval.ToString
        End Function

        Private Function _EvaluateBinaryExpression(expression As BoundBinaryExpression) As Object
            Dim left As Object = Me._EvaluateExpression(expression.Left)
            Dim right As Object = Me._EvaluateExpression(expression.Right)
            Try
                Return expression.Op.Invoke(left, right)
            Catch ex As InvalidCastException
                Me.Diagnostics.ReportOperatorNotDefined(expression.Op.Syntax.Kind, left.GetType, right.GetType, expression.Op.Syntax.Span)
                Return Nothing
            Catch ex As Exception
                Throw New EvaluationException(ex, expression.Syntax.Span)
            End Try
        End Function

        Private Function _EvaluateUnaryExpression(expression As BoundUnaryExpression) As Object
            Dim value As Object = Me._EvaluateExpression(expression.Right)
            Try
                Return expression.Op.Invoke(value)
            Catch ex As InvalidCastException
                Me.Diagnostics.ReportOperatorNotDefined(expression.Op.Syntax.Kind, value.GetType, expression.Op.Syntax.Span)
                Return Nothing
            End Try
        End Function

        Private Function _EvaluateArrayExpression(expression As BoundArrayExpression) As Object
            Try
                Dim obj_array As Object() = expression.Items.Select(Function(item) Me._EvaluateExpression(item)).ToArray
                If expression.Rank = 1 Then
                    If Not expression.BoundType.Equals(GetType(Object).MakeArrayType) Then
                        Dim retval As Array = Array.CreateInstance(expression.BoundType.GetElementType, obj_array.Count)
                        If obj_array.Count > 0 Then Array.Copy(obj_array, retval, obj_array.Count)
                        Return retval
                    Else
                        Return obj_array
                    End If
                Else
                    Dim lenghts As Integer() = {obj_array.Length}.Concat(Enumerable.Range(0, expression.Rank - 1).Select(Function(level) DirectCast(obj_array.First, Array).GetUpperBound(level) + 1)).ToArray
                    Dim retval As Array = Array.CreateInstance(expression.BoundType.GetElementType, lenghts)
                    _CopyArrayMultidimensional(obj_array, retval)
                    Return retval
                End If
            Catch ex As Exception
                Throw New EvaluationException(ex, expression.Syntax.Span)
            End Try
        End Function

        Private Shared Sub _CopyArrayMultidimensional(source As Object(), target As Array)
            For index As Integer = 0 To source.Count - 1
                _IterCopyArrayMultiDimensional(source, target, {index})
            Next
        End Sub

        Private Shared Sub _IterCopyArrayMultiDimensional(source As Object(), target As Array, level As Integer())
            If level.Count = target.Rank Then
                target.SetValue(DirectCast(source(level.First), Array).GetValue(level.Skip(1).ToArray), level)
            Else
                For index As Integer = 0 To target.GetUpperBound(level.Count)
                    _IterCopyArrayMultiDimensional(source, target, level.Append(index).ToArray)
                Next
            End If
        End Sub

        Private Function _EvaluateTernaryExpression(expression As BoundTernaryExpression) As Object
            Try
                Dim isTrue As Boolean = Me._EvaluateExpression(expression.Condition)
                If isTrue Then
                    Return Me._EvaluateExpression(expression.TrueExpression)
                Else
                    Return Me._EvaluateExpression(expression.FalseExpression)
                End If
            Catch ex As Exception
                Throw New EvaluationException(ex, expression.Syntax.Span)
            End Try
        End Function

        Private Function _EvaluateNullCheckExpression(expression As BoundNullCheckExpression) As Object
            Try
                Dim value As Object = Me._EvaluateExpression(expression.Expression)
                If value IsNot Nothing Then
                    Return value
                Else
                    Try
                        Return CTypeDynamic(Me._EvaluateExpression(expression.FallbackExpression), expression.BoundType)
                    Catch ex As InvalidCastException
                        Me.Diagnostics.ReportInvalidConversion(expression.FallbackExpression.BoundType, expression.BoundType, expression.FallbackExpression.Syntax.Span)
                        Return Nothing
                    End Try
                End If
            Catch ex As Exception
                Throw New EvaluationException(ex, expression.Syntax.Span)
            End Try
        End Function

        Private Function _EvaluateGetTypeExpression(expression As BoundGetTypeExpression) As Object
            Return expression.Type
        End Function

        Private Function _EvaluateCastDynamicExpression(expression As BoundCastDynamicExpression) As Object
            Try
                Return CTypeDynamic(Me._EvaluateExpression(expression.Expression), Me._EvaluateExpression(expression.Type))
            Catch ex As InvalidCastException
                Me.Diagnostics.ReportInvalidConversion(expression.Expression.BoundType, expression.BoundType, expression.Syntax.Span)
                Return Nothing
            Catch ex As Exception
                Throw New EvaluationException(ex, expression.Syntax.Span)
            End Try
        End Function

        Private Function _EvaluateCastExpression(expression As BoundCastExpression) As Object
            Try
                Return CTypeDynamic(Me._EvaluateExpression(expression.Expression), expression.Type)
            Catch ex As InvalidCastException
                Me.Diagnostics.ReportInvalidConversion(expression.Expression.BoundType, expression.BoundType, expression.Syntax.Span)
                Return Nothing
            Catch ex As Exception
                Throw New EvaluationException(ex, expression.Syntax.Span)
            End Try
        End Function

        Private Function _EvaluateVariableExpression(expression As BoundVariableExpression) As Object
            Try
                If expression.Variable.Kind = SymbolKinds.GlobalVariable Then
                    Return Me.State.Variable(expression.Variable)
                Else
                    Return Me._local_variables.Peek().Item(expression.Variable)
                End If
            Catch ex As Exception
                Throw New EvaluationException(ex, expression.Syntax.Span)
            End Try
        End Function

        Private Function _EvaluateLiteralExpression(expression As BoundLiteralExpression) As Object
            Return expression.Value
        End Function

    End Class

End Namespace
