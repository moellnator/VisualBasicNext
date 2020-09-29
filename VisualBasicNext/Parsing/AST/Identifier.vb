Imports System.Reflection
Imports VisualBasicNext.Virtual

Namespace Parsing.AST
    Public Class Identifier : Inherits Expression

        Private Class _Element
            Public ReadOnly Property Item As Expression = Nothing
            Public ReadOnly Property Name As String = Nothing
            Public ReadOnly Property Generic As TypeName() = {}
            Public ReadOnly Property Access As Expression()() = {}

            Public ReadOnly Property IsIdentifier As Boolean
                Get
                    Return Me.Item Is Nothing
                End Get
            End Property

            Public Sub New(cst As CST.Node)
                If cst.First.NodeType = Parsing.CST.NodeTypes.Atom Then
                    Me.Item = FromCST(cst.First)
                Else
                    Me.Name = cst.First.Content
                End If
                If IsIdentifier Then
                    Me.Generic = _GetGeneric(cst(1))
                    Me.Access = _GetAccess(cst(2))
                Else
                    Me.Access = _GetAccess(cst(1))
                End If
            End Sub

            Private Shared Function _GetGeneric(cst As CST.Node) As TypeName()
                Dim retval As TypeName() = {}
                If cst.Count <> 0 Then
                    Dim generic_nodes As CST.Node() = {cst(0)(2)}.Concat(cst(0)(3).Select(Function(m) m.Last)).ToArray
                    retval = generic_nodes.Select(Function(g) DirectCast(FromCST(g), TypeName)).ToArray
                End If
                Return retval
            End Function

            Private Shared Function _GetAccess(cst As CST.Node) As Expression()()
                Dim access As New List(Of Expression())
                If cst.Count <> 0 Then
                    For Each n As CST.Node In cst
                        If n(1).Count = 0 Then
                            access.Add({})
                        Else
                            Dim a_nodes As CST.Node() = {n(1)(0)(0)}.Concat(n(1)(0)(1).Select(Function(m) m(1))).ToArray
                            access.Add(a_nodes.Select(Function(a) DirectCast(FromCST(a), Expression)).ToArray)
                        End If
                    Next
                End If
                Return access.ToArray
            End Function

        End Class

        Private _elements As _Element()

        Public Sub New(cst As CST.Node)
            Dim element_nodes As CST.Node() = {cst.First.First}.Concat(cst.First.Last.Select(Function(m) m.Last)).ToArray
            Me._elements = element_nodes.Select(Function(e) New _Element(e)).ToArray
        End Sub

        Public Overrides Function Evaluate(target As State) As Object
            Dim retval As Object = Nothing
            Dim type As Type = GetType(Object)
            Dim binding As BindingFlags = BindingFlags.Public Or BindingFlags.Static
            Dim generics As Type() = Me._elements.SelectMany(Function(e) e.Generic.Select(Function(c) DirectCast(c.Evaluate(target), Type))).ToArray
            Dim generic_offset As Integer = 0
            If Me._elements.First.IsIdentifier Then
                If target.IsDeclared(Me._elements.First.Name) Then
                    retval = target.Variable(Me._elements.First.Name)
                    type = retval.GetType
                    binding = BindingFlags.Public Or BindingFlags.Instance
                    If Me._elements.First.Generic.Count <> 0 Then Throw New Exception("Local variable can not have generic parameters.")
                Else
                    'Namespace or type... iterate elements until "type" is defined in target... no access or generic allowed in namespaces
                End If
            Else
                retval = Me._elements.First.Item.Evaluate(target)
                type = retval.GetType
                binding = BindingFlags.Public Or BindingFlags.Instance
            End If
            Stop
        End Function

    End Class

End Namespace
