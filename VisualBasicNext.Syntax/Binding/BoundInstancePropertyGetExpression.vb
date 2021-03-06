﻿Imports System.Collections.Immutable
Imports System.Reflection
Imports VisualBasicNext.CodeAnalysis.Parsing

Namespace Binding
    Friend Class BoundInstancePropertyGetExpression : Inherits BoundExpression

        Public Sub New(syntax As SyntaxNode, source As BoundExpression, member As PropertyInfo, arguments As IEnumerable(Of BoundExpression))
            MyBase.New(syntax, BoundNodeKind.BoundInstancePropertyGetExpression, member.PropertyType)
            Me.Source = source
            Me.Member = member
            Me.Arguments = arguments.ToImmutableArray
        End Sub

        Public ReadOnly Property Source As BoundExpression
        Public ReadOnly Property Member As PropertyInfo
        Public ReadOnly Property Arguments As ImmutableArray(Of BoundExpression)

    End Class

End Namespace
