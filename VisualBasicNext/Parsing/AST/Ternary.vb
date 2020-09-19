Imports VisualBasicNext.Virtual

Namespace Parsing.AST
    Public Class Ternary : Inherits Expression

        Private ReadOnly _condition As Expression
        Private ReadOnly _option_true As Expression
        Private ReadOnly _option_false As Expression = Nothing

        Public Sub New(cst As CST.Node)
            Me._condition = FromCST(cst.First()(2))
            Me._option_true = FromCST(cst.First()(4))
            If cst.First()(5).Count <> 0 Then Me._option_false = FromCST(cst.First()(5).First()(1))
        End Sub

        Public Overrides Function Evaluate(target As State) As Object
            Dim cond As Object = Me._condition.Evaluate(target)
            Dim retval As Object
            If Me._option_false IsNot Nothing Then
                If cond = True Then
                    retval = Me._option_true.Evaluate(target)
                Else
                    retval = Me._option_false.Evaluate(target)
                End If
            Else
                If cond IsNot Nothing Then
                    retval = cond
                Else
                    retval = Me._option_true.Evaluate(target)
                End If
            End If
            Return retval
        End Function

    End Class

End Namespace
