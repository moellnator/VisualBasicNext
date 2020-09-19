Imports VisualBasicNext.Virtual

Namespace Parsing.AST
    Public Class Operators : Inherits Expression

        Private Shared ReadOnly _OP_MAP As New Dictionary(Of String, Func(Of Object, Object, Object)) From {
            {"xor", Function(a, b) a Xor b},
            {"or", Function(a, b) a Or b},
            {"and", Function(a, b) a And b},
            {"orelse", Function(a, b) a OrElse b},
            {"andalso", Function(a, b) a AndAlso b},
            {"=", Function(a, b) a = b},
            {"<>", Function(a, b) a <> b},
            {">", Function(a, b) a > b},
            {">=", Function(a, b) a >= b},
            {"<", Function(a, b) a < b},
            {"<=", Function(a, b) a <= b},
            {"like", Function(a, b) a Like b},
            {"is", Function(a, b) a Is b},
            {"<<", Function(a, b) a << b},
            {">>", Function(a, b) a >> b},
            {"+", Function(a, b) a + b},
            {"-", Function(a, b) a - b},
            {"mod", Function(a, b) a Mod b},
            {"\", Function(a, b) a \ b},
            {"*", Function(a, b) a * b},
            {"/", Function(a, b) a / b},
            {"^", Function(a, b) a ^ b},
            {"&", Function(a, b) a & b}
        }

        Private Shared ReadOnly _UN_MAP As New Dictionary(Of String, Func(Of Object, Object, Object)) From {
             {"not", Function(a, b) Not a},
             {"+", Function(a, b) +a},
             {"-", Function(a, b) -a}
        }

        Private ReadOnly _operand_a As Expression
        Private ReadOnly _operator As Func(Of Object, Object, Object)
        Private ReadOnly _operand_b As Expression = Nothing

        Private Sub New(cst As CST.Node, unary As Boolean)
            If unary Then
                Dim op_list As CST.Node() = cst.First.First.ToArray
                If op_list.Count = 1 Then
                    Me._operand_a = FromCST(cst.First.Last)
                    Me._operator = _UN_MAP(op_list.First.Content)
                Else
                    Dim first_node As CST.Node = op_list.First
                    Dim rest_node As New CST.Node(
                        Parsing.CST.NodeTypes.Operators,
                        {New CST.Node(Parsing.CST.NodeTypes.Generic,
                            {
                                New CST.Node(Parsing.CST.NodeTypes.Generic, op_list.Skip(1).ToArray),
                                cst.First.Last
                            }
                        )}
                    )
                    Me._operator = _UN_MAP(first_node.Content)
                    Me._operand_a = FromCST(rest_node)
                End If
            Else
                Dim op_list As CST.Node() = cst.First.Last.ToArray
                Me._operand_a = FromCST(cst.First.First)
                If op_list.Count = 1 Then
                    Dim first_node As CST.Node = op_list.First
                    Me._operand_b = FromCST(first_node.Last)
                    Me._operator = _OP_MAP(first_node.First.Content)
                Else
                    Dim first_node As CST.Node = op_list.First
                    Me._operator = _OP_MAP(first_node.First.Content)
                    Dim rest_node As New CST.Node(
                        Parsing.CST.NodeTypes.Operators,
                        {New CST.Node(Parsing.CST.NodeTypes.Generic,
                            {
                                first_node.Last,
                                New CST.Node(Parsing.CST.NodeTypes.Generic, op_list.Skip(1).ToArray)
                            }
                        )}
                    )
                    Me._operand_b = FromCST(rest_node)
                End If
            End If
        End Sub

        Public Shared Function BuildNode(cst As CST.Node) As Expression
            Dim retval As Expression = Nothing
            Dim root As CST.Node = cst.First
            Dim isleaf As Boolean = If(root.Count <> 1, False, root.NodeType <> Parsing.CST.NodeTypes.Operators)
            If isleaf Then
                retval = FromCST(root)
            Else
                If root.First.Count = 0 OrElse root.First.NodeType <> Parsing.CST.NodeTypes.Operators Then
                    If root.First.Count = 0 Then
                        retval = FromCST(root.Last)
                    Else
                        retval = New Operators(cst, True)
                    End If
                Else
                    If root.Last.Count = 0 Then
                        retval = FromCST(root.First)
                    Else
                        retval = New Operators(cst, False)
                    End If
                End If
            End If
            Return retval
        End Function

        Public Overrides Function Evaluate(target As State) As Object
            Dim op_a As Object = Me._operand_a.Evaluate(target)
            Dim op_b As Object = Me._operand_b?.Evaluate(target)
            Return Me._operator(op_a, op_b)
        End Function

    End Class

End Namespace
