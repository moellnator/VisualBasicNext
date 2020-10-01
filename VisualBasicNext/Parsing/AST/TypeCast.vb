Imports VisualBasicNext.Virtual

Namespace Parsing.AST
    Public Class TypeCast : Inherits Expression

        Private ReadOnly _subexpr As Expression
        Private ReadOnly _type As TypeName

        Public Sub New(cst As CST.Node)
            Me._subexpr = FromCST(cst.First()(2))
            Me._type = FromCST(cst.First()(4))
        End Sub

        Public Overrides Function Evaluate(target As State) As Object
            Return CTypeDynamic(Me._subexpr.Evaluate(target), Me._type.Evaluate(target))
        End Function

    End Class

End Namespace
