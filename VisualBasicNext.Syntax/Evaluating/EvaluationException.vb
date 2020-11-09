Imports VisualBasicNext.CodeAnalysis.Parsing

Namespace Evaluating
    Friend Class EvaluationException : Inherits Exception

        Public Sub New(innerException As Exception, syntax As Text.Span)
            MyBase.New("A runtime exception occured.", innerException)
            Me.Syntax = syntax
        End Sub

        Public ReadOnly Property Syntax As Text.Span

    End Class

End Namespace
