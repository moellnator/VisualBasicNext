Namespace Parsing.AST
    Public MustInherit Class Node

        Public Shared Function FromCST(node As CST.Node) As Node
            Dim retval As Node = Nothing
            Select Case node.NodeType
                Case CST.NodeTypes.Expression, CST.NodeTypes.Atom
                    retval = FromCST(node.Children.First)
                Case CST.NodeTypes.Literal
                    retval = New Literal(node)
                Case CST.NodeTypes.Block
                    retval = New Block(node)
                Case CST.NodeTypes.Identifier
                    retval = New Identifier(node)
                Case CST.NodeTypes.Array
                    retval = New ExprArray(node)
                Case CST.NodeTypes.TypeName
                    retval = New TypeName(node)
                Case CST.NodeTypes.TypeIdentifier
                    retval = New TypeIdentifier(node)
                Case CST.NodeTypes.Ternary
                    retval = New Ternary(node)
                Case CST.NodeTypes.Operators
                    retval = Operators.BuildNode(node)
                Case Else
                    Throw New ParserException($"Unknown cst node '{node.NodeType}' at {node.Origin}", node.Origin)
            End Select
            Return retval
        End Function

        Public MustOverride Function Evaluate(target As Virtual.State) As Object

    End Class

End Namespace
