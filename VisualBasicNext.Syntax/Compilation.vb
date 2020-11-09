Imports VisualBasicNext.Syntax.Binding
Imports VisualBasicNext.Syntax.Diagnostics
Imports VisualBasicNext.Syntax.Evaluating
Imports VisualBasicNext.Syntax.Parsing
Imports VisualBasicNext.Syntax.Text

Public Class Compilation

    Public ReadOnly Property Source As Source
    Public ReadOnly Property Diagnostics As ErrorList
    Public ReadOnly Property SyntaxTree As SyntaxNode

    Private ReadOnly _previous As Compilation
    Private _global_scope As BoundGlobalScope

    Private Sub New(previous As Compilation, source As Source)
        Me.Source = source
        Dim parser As New Parser(Me.Source)
        Me.SyntaxTree = parser.Parse
        Me.Diagnostics = New ErrorList(parser.Diagnostics)
        Me._previous = previous
    End Sub

    Public Shared Function CreateFromText(previous As Compilation, text As String) As Compilation
        Return New Compilation(previous, Source.FromText(text))
    End Function

    Private Function _GetGlobalScope() As BoundGlobalScope
        If Me._global_scope Is Nothing Then
            Me._global_scope = Binder.BindGlobalScope(Me.SyntaxTree, Me._previous?._GetGlobalScope)
        End If
        Return Me._global_scope
    End Function

    Private Function _GetScript() As BoundScript
        Dim previous As BoundScript = Me._previous?._GetScript
        Return Binder.BindScript(previous, Me._GetGlobalScope)
    End Function

    Public Function Evaluate(state As VMState) As EvaluationResult
        Dim program As BoundScript = Me._GetScript()
        If Me._GetGlobalScope.Diagnostics.HasErrors Then Return New EvaluationResult(Nothing, Me._GetGlobalScope.Diagnostics)
        If program.Diagnostics.HasErrors Then Return New EvaluationResult(Nothing, program.Diagnostics)
        Dim evaluator As New Evaluator(program, state)
        Dim result As Object = evaluator.Evaluate
        Return New EvaluationResult(result, program.Diagnostics & evaluator.Diagnostics)
    End Function

End Class
