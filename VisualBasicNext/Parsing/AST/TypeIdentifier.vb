Imports VisualBasicNext.Virtual

Namespace Parsing.AST
    Public Class TypeIdentifier : Inherits Expression

        Private ReadOnly _type As TypeName

        Public Sub New(cst As CST.Node)
            Me._type = FromCST(cst.First()(2))
        End Sub

        Public Overrides Function Evaluate(target As State) As Object
            Return Me._type.Evaluate(target)
        End Function

    End Class

End Namespace
