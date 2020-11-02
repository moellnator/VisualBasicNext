Imports VisualBasicNext.Syntax.Binding
Imports VisualBasicNext.Syntax.Symbols

Public Class Evaluator

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
            Case Else
                Throw New Exception($"Unknown statement in evaluator: '{statement.Kind.ToString}'.")
        End Select
        Return retval
    End Function

    Private Sub EvaluateVariableDeclarationStatement(statment As BoundVariableDeclarationStatement)
        Dim initial As Object = Nothing
        If statment.Initializer IsNot Nothing Then initial = Me.EvaluteExpression(statment.Initializer)
        Me.Assign(statment.Symbol, initial)
    End Sub

    Private Sub Assign(symbol As VariableSymbol, value As Object)
        Dim converted As Object = CTypeDynamic(value, symbol.Type)
        If symbol.Kind = SymbolKinds.GlobalVariable Then
            Me.State.Variable(symbol) = converted
        Else
            Dim local As Dictionary(Of Symbol, Object) = Me._local_variables.Peek
            If local.ContainsKey(symbol) Then local.Remove(symbol)
            local.Add(symbol, converted)
        End If
    End Sub

    Private Function EvaluateExpressionStatement(statement As BoundExpressionStatement) As Object
        Return Me.EvaluteExpression(statement.Expression)
    End Function

    Private Function EvaluteExpression(expression As BoundExpression) As Object
        If expression.Constant IsNot Nothing Then Return expression.Constant.Value
        Select Case expression.Kind
            Case BoundNodeKinds.BoundLiteral
                Return Me.EvaluateLiteralExpression(expression)
            Case BoundNodeKinds.BoundVariableExpression
                Return Me.EvaluateVariableExpression(expression)
            Case Else
                Throw New Exception($"Unknown expression in evaluator: '{expression.Kind.ToString}'.")
        End Select
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
