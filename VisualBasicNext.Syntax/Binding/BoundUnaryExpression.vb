﻿Imports VisualBasicNext.CodeAnalysis.Parsing

Namespace Binding
    Friend Class BoundUnaryExpression : Inherits BoundExpression

        Public Sub New(syntax As SyntaxNode, op As BoundUnaryOperator, right As BoundExpression)
            MyBase.New(syntax, BoundNodeKind.BoundUnaryExpression, op.ReturnType)
            Me.Op = op
            Me.Right = right
        End Sub

        Public ReadOnly Property Op As BoundUnaryOperator
        Public ReadOnly Property Right As BoundExpression

    End Class

End Namespace
