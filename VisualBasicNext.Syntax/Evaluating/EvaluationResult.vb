Imports VisualBasicNext.CodeAnalysis.Diagnostics

Namespace Evaluating
    Public Class EvaluationResult

        Public Sub New(value As Object, diagnostics As ErrorList)
            Me.Value = value
            Me.Diagnostics = diagnostics
        End Sub

        Public ReadOnly Property Value As Object
        Public ReadOnly Property Diagnostics As ErrorList

    End Class

End Namespace