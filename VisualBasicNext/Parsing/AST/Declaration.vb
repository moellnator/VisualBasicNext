Imports VisualBasicNext.Virtual

Namespace Parsing.AST
    Public Class Declaration : Inherits Statement

        Private ReadOnly _name As String
        Private ReadOnly _type As TypeName
        Private ReadOnly _value As Expression = Nothing

        Public Sub New(cst As CST.Node)
            Me._name = cst(0)(1).Content
            Me._type = FromCST(cst(0)(3))
            If cst(0)(4).Count <> 0 Then
                Me._value = FromCST(cst(0)(4)(0)(1))
            End If
        End Sub

        Public Overrides Function Evaluate(target As State) As Object
            Dim type As Type = Me._type.Evaluate(target)
            Dim value As Object = Me._value?.Evaluate(target)
            target.DeclareLocal(Me._name, type, value)
            Return Nothing
        End Function

    End Class

End Namespace
