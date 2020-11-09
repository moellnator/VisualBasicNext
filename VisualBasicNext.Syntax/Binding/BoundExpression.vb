Imports VisualBasicNext.Syntax.Parsing

Namespace Binding
    Public MustInherit Class BoundExpression : Inherits BoundNode

        Protected Sub New(syntax As SyntaxNode, kind As BoundNodeKind, boundType As Type)
            MyBase.New(syntax, kind)
            Me.BoundType = boundType
        End Sub

        Public ReadOnly Property BoundType As Type
        Public Overridable ReadOnly Property Constant As BoundConstant

    End Class

End Namespace
