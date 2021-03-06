﻿Imports VisualBasicNext.CodeAnalysis.Parsing

Namespace Binding
    Friend Class BoundTernaryExpression : Inherits BoundExpression

        Public Sub New(syntaxNode As SyntaxNode, condition As BoundExpression, trueExpression As BoundExpression, falseExpression As BoundExpression, boundType As Type)
            MyBase.New(syntaxNode, BoundNodeKind.BoundTernaryExpression, boundType)
            Me.Condition = condition
            Me.TrueExpression = trueExpression
            Me.FalseExpression = falseExpression
        End Sub

        Public ReadOnly Property Condition As BoundExpression
        Public ReadOnly Property TrueExpression As BoundExpression
        Public ReadOnly Property FalseExpression As BoundExpression

    End Class

End Namespace
