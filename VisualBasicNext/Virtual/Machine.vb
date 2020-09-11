Imports VisualBasicNext.Parsing
Imports VisualBasicNext.Parsing.Generator
Imports VisualBasicNext.Parsing.Tokenizing

Namespace Virtual
    Public Class Machine

        Public ReadOnly Property CurrentState As New State

        Public Function Evaluate(expression As String) As Object
            Dim retval As Object = Nothing
            If Not expression.Equals(String.Empty) Then
                Dim tokens As Token() = Tokenizer.Tokenize(expression).ToArray
                Dim expr As Parser = Scripting.Expression.Instance And Parser.Require(Parser.Terminator)
                Dim vm As AST.Node = AST.Node.FromCST(expr.Parse(New Generator.State(tokens, -1)).Node.First)
                retval = vm.Evaluate(CurrentState)
            End If
            Return retval
        End Function

    End Class

End Namespace
