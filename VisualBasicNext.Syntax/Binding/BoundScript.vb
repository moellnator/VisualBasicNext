Imports System.Collections.Immutable
Imports VisualBasicNext.CodeAnalysis.Diagnostics
Imports VisualBasicNext.CodeAnalysis.Parsing

Namespace Binding
    Friend Class BoundScript : Inherits BoundNode

        Public Sub New(previous As BoundScript, syntax As SyntaxNode, statements As IEnumerable(Of BoundStatement), diagnostics As ErrorList)
            MyBase.New(syntax, BoundNodeKind.BoundScript)
            Me.Statements = statements.ToImmutableArray
            Me.Previous = previous
            Me.Diagnostics = diagnostics
        End Sub

        Public ReadOnly Property Statements As ImmutableArray(Of BoundStatement)
        Public ReadOnly Property Previous As BoundScript
        Public ReadOnly Property Diagnostics As ErrorList
    End Class

End Namespace
