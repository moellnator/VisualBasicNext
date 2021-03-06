﻿Imports VisualBasicNext.CodeAnalysis.Parsing

Namespace Binding
    Friend Class BoundCastDynamicExpression : Inherits BoundExpression

        Public Sub New(syntax As SyntaxNode, expression As BoundExpression, type As BoundExpression)
            MyBase.New(syntax, BoundNodeKind.BoundCastDynamicExpression, GetType(Object))
            Me.Expression = expression
            Me.Type = type
        End Sub

        Public ReadOnly Property Expression As BoundExpression
        Public ReadOnly Property Type As BoundExpression

    End Class

End Namespace
