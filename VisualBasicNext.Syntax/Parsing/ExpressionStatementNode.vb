﻿Imports VisualBasicNext.Syntax.Lexing

Namespace Parsing
    Public Class ExpressionStatementNode : Inherits StatementNode

        Public ReadOnly Property Expression As ExpressionNode

        Public Overrides ReadOnly Iterator Property Children As IEnumerable(Of SyntaxNode)
            Get
                Yield Me.Expression
                Yield Me.EndOfStatementToken
            End Get
        End Property

        Public Sub New(expression As ExpressionNode, endOfStatementToken As SyntaxToken)
            MyBase.New(SyntaxKind.ExpressionStatementNode, endOfStatementToken)
            Me.Expression = expression
        End Sub

    End Class

End Namespace
