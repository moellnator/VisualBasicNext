Imports VisualBasicNext.Virtual

Namespace Parsing.AST
    Public Class Identifier : Inherits Expression

        Private ReadOnly _items As String()

        Public Sub New(cst As CST.Node)
            Me._items = cst.Content.Split(".")
        End Sub

        Public Overrides Function Evaluate(target As State) As Object
            Dim obj As Object = Nothing
            Dim type As Type = GetType(Object)
            If target.IsDeclared(_items.First) Then
                obj = target.Variable(_items.First)
                type = obj.GetType
            Else

            End If
            Stop
        End Function

    End Class

End Namespace
