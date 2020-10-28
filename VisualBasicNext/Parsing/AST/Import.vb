Imports VisualBasicNext.Virtual

Namespace Parsing.AST
    Public Class Import : Inherits Statement

        Private ReadOnly _namespace As String

        Public Sub New(cst As CST.Node)
            Me._namespace = cst.Children(0)(1).Content
        End Sub

        Public Overrides Function Evaluate(target As State) As Object
            target.Import(Me._namespace)
            Return Nothing
        End Function

    End Class

End Namespace
