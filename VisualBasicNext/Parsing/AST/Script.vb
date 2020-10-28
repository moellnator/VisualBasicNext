Imports VisualBasicNext.Virtual

Namespace Parsing.AST
    Public Class Script : Inherits Node

        Private ReadOnly _statements As Statement() = {}

        Public Sub New(cst As CST.Node)
            Dim node As CST.Node = cst(0)
            If node.Count <> 0 Then
                node = node(0)
                Dim item_nodes As CST.Node() = {node(0)}.Concat(node(1).Select(Function(n) n(1))).ToArray
                item_nodes = item_nodes.Where(Function(n) n.Count <> 0 AndAlso n.Content <> vbNewLine).Select(Function(n) n(0)).ToArray
                Me._statements = item_nodes.Select(Function(c) DirectCast(FromCST(c), Statement)).ToArray
            End If
        End Sub

        Public Overrides Function Evaluate(target As State) As Object
            For Each s As Statement In Me._statements
                s.Evaluate(target)
            Next
            Return Nothing
        End Function

    End Class

End Namespace
