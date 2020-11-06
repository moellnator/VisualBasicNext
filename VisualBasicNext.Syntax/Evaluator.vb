Imports VisualBasicNext.Syntax.Binding
Imports VisualBasicNext.Syntax.Diagnostics
Imports VisualBasicNext.Syntax.Symbols

Public Class Evaluator

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
        For Each statement As BoundStatement In Me.Script.Statements
            retval = Me.EvaluateStatement(statement)
        Next
        Return retval
    End Function

    Private Function EvaluateStatement(statement As BoundStatement) As Object
        Dim retval As Object = Nothing
        Select Case statement.Kind
            Case BoundNodeKinds.BoundVariableDeclarationStatement
                Me.EvaluateVariableDeclarationStatement(statement)
            Case BoundNodeKinds.BoundExpressionStatement
                retval = Me.EvaluateExpressionStatement(statement)
            Case BoundNodeKinds.BoundImportStatement
            Case Else
                Throw New Exception($"Unknown statement in evaluator: '{statement.Kind.ToString}'.")
        End Select
        Return retval
    End Function

    Private Sub EvaluateVariableDeclarationStatement(statement As BoundVariableDeclarationStatement)
        Dim initial As Object = Nothing
        If statement.Initializer IsNot Nothing Then
            initial = Me.EvaluateExpression(statement.Initializer)
            Me.Assign(statement.Symbol, initial, statement.Initializer.Syntax.Span)
        Else
            initial = CTypeDynamic(Nothing, DirectCast(statement.Symbol, VariableSymbol).Type)
            Me.Assign(statement.Symbol, initial, statement.Syntax.Span)
        End If
    End Sub

    Private Sub Assign(symbol As VariableSymbol, value As Object, span As Text.Span)
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
            Stop
        End Try
    End Sub

    Private Function EvaluateExpressionStatement(statement As BoundExpressionStatement) As Object
        Return Me.EvaluateExpression(statement.Expression)
    End Function

    Private Function EvaluateExpression(expression As BoundExpression) As Object
        If expression.Constant IsNot Nothing Then Return expression.Constant.Value
        Select Case expression.Kind
            Case BoundNodeKinds.BoundLiteral
                Return Me.EvaluateLiteralExpression(expression)
            Case BoundNodeKinds.BoundVariableExpression
                Return Me.EvaluateVariableExpression(expression)
            Case BoundNodeKinds.BoundCastExpression
                Return Me.EvaluateCastExpression(expression)
            Case BoundNodeKinds.BoundCastDynamicExpression
                Return Me.EvaluateCastDynamicExpression(expression)
            Case BoundNodeKinds.BoundGetTypeExpression
                Return Me.EvaluateGetTypeExpression(expression)
            Case BoundNodeKinds.BoundTernaryExpression
                Return Me.EvaluateTernaryExpression(expression)
            Case BoundNodeKinds.BoundNullCheckExpression
                Return Me.EvaluateNullCheckExpression(expression)
            Case BoundNodeKinds.BoundArrayExpression
                Return Me.EvaluateArrayExpression(expression)
            Case Else
                Throw New Exception($"Unknown expression in evaluator: '{expression.Kind.ToString}'.")
        End Select
    End Function

    Private Function EvaluateArrayExpression(expression As BoundArrayExpression) As Object
        Dim obj_array As Object() = expression.Items.Select(Function(item) Me.EvaluateExpression(item)).ToArray
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
            CopyArrayMultidimensional(obj_array, retval)
            Return retval
        End If
    End Function

    Private Shared Sub CopyArrayMultidimensional(source As Object(), target As Array)
        For index As Integer = 0 To source.Count - 1
            IterCopyArrayMultiDimensional(source, target, {index})
        Next
    End Sub

    Private Shared Sub IterCopyArrayMultiDimensional(source As Object(), target As Array, level As Integer())
        If level.Count = target.Rank Then
            target.SetValue(DirectCast(source(level.First), Array).GetValue(level.Skip(1).ToArray), level)
        Else
            For index As Integer = 0 To target.GetUpperBound(level.Count)
                IterCopyArrayMultiDimensional(source, target, level.Append(index).ToArray)
            Next
        End If
    End Sub

    Private Function EvaluateTernaryExpression(expression As BoundTernaryExpression) As Object
        Dim isTrue As Boolean = Me.EvaluateExpression(expression.Condition)
        If isTrue Then
            Return Me.EvaluateExpression(expression.TrueExpression)
        Else
            Return Me.EvaluateExpression(expression.FalseExpression)
        End If
    End Function

    Private Function EvaluateNullCheckExpression(expression As BoundNullCheckExpression) As Object
        Dim value As Object = Me.EvaluateExpression(expression.Expression)
        If value IsNot Nothing Then
            Return value
        Else
            Try
                Return CTypeDynamic(Me.EvaluateExpression(expression.FallbackExpression), expression.BoundType)
            Catch ex As InvalidCastException
                Me.Diagnostics.ReportInvalidConversion(expression.FallbackExpression.BoundType, expression.BoundType, expression.FallbackExpression.Syntax.Span)
                Return Nothing
            End Try
        End If
    End Function

    Private Function EvaluateGetTypeExpression(expression As BoundGetTypeExpression) As Object
        Return expression.Type
    End Function

    Private Function EvaluateCastDynamicExpression(expression As BoundCastDynamicExpression) As Object
        Try
            Return CTypeDynamic(Me.EvaluateExpression(expression.Expression), Me.EvaluateExpression(expression.Type))
        Catch ex As InvalidCastException
            Me.Diagnostics.ReportInvalidConversion(expression.Expression.BoundType, expression.BoundType, expression.Syntax.Span)
            Return Nothing
        End Try
    End Function

    Private Function EvaluateCastExpression(expression As BoundCastExpression) As Object
        Try
            Return CTypeDynamic(Me.EvaluateExpression(expression.Expression), expression.Type)
        Catch ex As InvalidCastException
            Me.Diagnostics.ReportInvalidConversion(expression.Expression.BoundType, expression.BoundType, expression.Syntax.Span)
            Return Nothing
        End Try
    End Function

    Private Function EvaluateVariableExpression(expression As BoundVariableExpression) As Object
        If expression.Variable.Kind = SymbolKinds.GlobalVariable Then
            Return Me.State.Variable(expression.Variable)
        Else
            Return Me._local_variables.Peek().Item(expression.Variable)
        End If
    End Function

    Private Function EvaluateLiteralExpression(expression As BoundLiteralExpression) As Object
        Return expression.Value
    End Function

End Class
