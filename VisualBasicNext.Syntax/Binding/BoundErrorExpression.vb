﻿Imports VisualBasicNext.Syntax.Parsing

Namespace Binding
    Public Class BoundErrorExpression : Inherits BoundExpression

        Public Sub New(syntax As SyntaxNode)
            MyBase.New(syntax, BoundNodeKinds.BoundErrorExpression, GetType(Object))
        End Sub

    End Class

End Namespace