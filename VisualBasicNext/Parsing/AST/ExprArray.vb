Imports VisualBasicNext.Virtual

Namespace Parsing.AST
    Public Class ExprArray : Inherits Expression

        Private ReadOnly _sub_expressions As Expression() = {}
        Private ReadOnly _rank As Integer = 1

        Public Sub New(cst As CST.Node)
            Dim inner As CST.Node = cst.Children.First.Children(1)
            If Not inner.Children.Count = 0 Then
                inner = inner.Children.First
                Dim sub_nodes As New List(Of CST.Node) From {inner.First}
                If Not inner.Children(1).Children.Count = 0 Then sub_nodes.AddRange(inner.Children(1).Children.Select(Function(c) c.Children.Last))
                Me._sub_expressions = sub_nodes.Select(Function(n) DirectCast(FromCST(n), Expression)).ToArray
                If Me._sub_expressions.All(Function(s) TypeOf s Is ExprArray) Then
                    Dim count As Integer = DirectCast(Me._sub_expressions.First, ExprArray)._sub_expressions.Count
                    Dim rank As Integer = DirectCast(Me._sub_expressions.First, ExprArray)._rank
                    If Me._sub_expressions.All(
                        Function(s) DirectCast(s, ExprArray)._sub_expressions.Count.Equals(count) AndAlso DirectCast(s, ExprArray)._rank.Equals(rank)
                       ) Then
                        Me._rank = 1 + rank
                    End If
                End If
            End If
        End Sub

        Public Overrides Function Evaluate(target As State) As Object
            Dim retval As Array = New Object() {}
            Dim type As Type = GetType(Object)
            Dim results As Object() = Me._sub_expressions.Select(Function(s) s.Evaluate(target)).ToArray
            If results.Count > 0 Then
                If results.All(Function(r) r.GetType.Equals(results.First.GetType)) Then type = results.First.GetType
                Dim destination As Array = Array.CreateInstance(type, results.Count)
                Array.Copy(results, destination, results.Count)
                retval = destination
            End If
            If Me._rank <> 1 Then
                Dim destination As Array = Array.CreateInstance(
                    type.GetElementType,
                    {retval.Length}.Concat(
                        Enumerable.Range(0, Me._rank - 1).Select(Function(r) DirectCast(retval(0), Array).GetUpperBound(r) + 1)
                    ).ToArray
                )
                _CopyMultiDim(destination, retval)
                retval = destination
            End If
            Return retval
        End Function

        Private Shared Sub _CopyMultiDim(target As Array, source As Array())
            For index As Integer = 0 To source.Count - 1
                _SubCopyMultiDim(target, source, {index})
            Next
        End Sub

        Private Shared Sub _SubCopyMultiDim(target As Array, source As Array(), level As Integer())
            If level.Count = target.Rank Then
                target.SetValue(source(level.First).GetValue(level.Skip(1).ToArray), level)
            Else
                For index As Integer = 0 To target.GetUpperBound(level.Count)
                    _SubCopyMultiDim(target, source, level.Append(index).ToArray)
                Next
            End If
        End Sub

        Private Shared Function _AreEqualDim(arrays As Array()) As Boolean
            Dim len As Integer = arrays.First.Length
            Dim rank As Integer = arrays.First.Rank
            Return Array.TrueForAll(arrays, Function(o) o.Length = len And o.Rank = rank)
        End Function

    End Class

End Namespace
