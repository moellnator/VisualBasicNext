Imports System.Reflection
Imports VisualBasicNext.CodeAnalysis.Parsing

Namespace Binding
    Friend Class BoundConstructorExpression : Inherits BoundExpression

        Public Sub New(syntax As SyntaxNode, type As Type, ctor As ConstructorInfo, arguments As BoundExpression())
            MyBase.New(syntax, BoundNodeKind.BoundConstructorExpression, type)
            Me.Type = type
            Me.Ctor = ctor
            Me.Arguments = arguments
        End Sub

        Public ReadOnly Property Type As Type
        Public ReadOnly Property Ctor As ConstructorInfo
        Public ReadOnly Property Arguments As BoundExpression()

    End Class

End Namespace
