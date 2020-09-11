Imports VisualBasicNext.Virtual

Namespace Parsing.AST
    Public Class Block : Inherits Expression

        Private ReadOnly _sub_expression As Expression

        Public Sub New(cst As CST.Node)
            Me._sub_expression = FromCST(cst.Children.First.Children(1))
        End Sub

        Public Overrides Function Evaluate(target As State) As Object
            Return _sub_expression.Evaluate(target)
        End Function

    End Class

End Namespace
