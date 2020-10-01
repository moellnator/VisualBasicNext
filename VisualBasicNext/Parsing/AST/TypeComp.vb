Imports VisualBasicNext.Virtual

Namespace Parsing.AST
    Public Class TypeComp : Inherits Expression

        Private ReadOnly _sub_expr As Expression
        Private ReadOnly _type As TypeName

        Public Sub New(cst As CST.Node)
            Me._sub_expr = FromCST(cst.First()(1))
            Me._type = FromCST(cst.First()(3))
        End Sub

        Public Overrides Function Evaluate(target As State) As Object
            Dim obj As Object = Me._sub_expr.Evaluate(target)
            Dim type As Type = Me._type.Evaluate(target)
            Return obj.GetType.Equals(type)
        End Function

    End Class

End Namespace
