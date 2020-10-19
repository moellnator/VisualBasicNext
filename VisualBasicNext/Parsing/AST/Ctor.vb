Imports System.Reflection
Imports VisualBasicNext.Virtual

Namespace Parsing.AST
    Public Class Ctor : Inherits Expression

        Private ReadOnly _type As TypeName
        Private ReadOnly _arguments As Expression() = {}
        Private ReadOnly _array_ranks As Integer() = Nothing
        Private ReadOnly _array_init As ExprArray = Nothing
        Private ReadOnly _array_bounds As Expression() = Nothing

        Public Sub New(cst As CST.Node)
            Dim type_node As CST.Node = cst(0)(1)
            type_node = New CST.Node(Parsing.CST.NodeTypes.Generic, type_node.Children.Append(New CST.Node(Parsing.CST.NodeTypes.Generic, New CST.Node() {})))
            Me._type = FromCST(New CST.Node(Parsing.CST.NodeTypes.TypeName, {type_node}))
            Dim p_node As CST.Node = cst(0)(2)
            If p_node.Count <> 0 Then
                If p_node.Content.EndsWith("}") Then
                    If Not p_node(1)(0)(1).Count = 0 Then Me._array_init = FromCST(p_node(1))
                    p_node = p_node(0)
                    Dim aargs As CST.Node() = {p_node(1)}.Concat(p_node(3).Select(Function(c) c(1))).ToArray
                    If aargs.First.Count = 1 AndAlso aargs(0)(0).Content <> ","c Then
                        p_node = aargs(0)(0)
                        Dim bounds As CST.Node() = {p_node(0)}.Concat(p_node(1).Select(Function(c) c(1))).ToArray
                        Me._array_bounds = bounds.Select(Function(c) DirectCast(FromCST(c), Expression)).ToArray
                        Me._array_ranks = {Me._array_bounds.Count}.Concat(aargs.Skip(1).Select(Function(c) c.Count + 1)).ToArray
                    Else
                        Me._array_ranks = aargs.Select(Function(c) c.Count + 1).ToArray
                    End If
                Else
                    p_node = p_node(0)(1)(0)
                    Dim arg_nodes As CST.Node() = {p_node(0)}.Concat(p_node(1).Select(Function(c) c(1))).ToArray
                    Me._arguments = arg_nodes.Select(Function(c) DirectCast(FromCST(c), Expression)).ToArray
                End If
            End If
        End Sub

        Public Overrides Function Evaluate(target As State) As Object
            Dim retval As Object = Nothing
            Dim type As Type = Me._type.Evaluate(target)
            If Me._array_ranks IsNot Nothing Then
                'TODO Crate array and check bounds vs. init value if present
                Stop
            Else
                Dim args As Object() = Me._arguments.Select(Function(a) a.Evaluate(target)).ToArray
                Dim sign As Type() = args.Select(Function(a) a.GetType).ToArray
                Dim tctor As ConstructorInfo = If(sign.Count = 0, type.GetConstructor(Type.EmptyTypes), type.GetConstructor(sign))
                If tctor Is Nothing Then Throw New Exception($"No constructor matches the given arguments in {type.Name}.")
                retval = tctor.Invoke(args)
            End If
            Return retval
        End Function

    End Class

End Namespace
