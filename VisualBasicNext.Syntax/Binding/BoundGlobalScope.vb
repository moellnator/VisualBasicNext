Imports System.Collections.Immutable
Imports VisualBasicNext.Syntax.Diagnostics
Imports VisualBasicNext.Syntax.Parsing
Imports VisualBasicNext.Syntax.Symbols

Namespace Binding
    Public Class BoundGlobalScope
        Public ReadOnly Property Syntax As SyntaxNode
        Public ReadOnly Property Previous As BoundGlobalScope
        Public ReadOnly Property Diagnostics As ErrorList
        Public ReadOnly Property [Imports] As IEnumerable(Of String)
        Public ReadOnly Property Variables As ImmutableArray(Of VariableSymbol)
        Public ReadOnly Property Statements As ImmutableArray(Of BoundStatement)

        Public Sub New(syntax As SyntaxNode, previous As BoundGlobalScope, diagnostics As ErrorList, variables As IEnumerable(Of VariableSymbol), statements As IEnumerable(Of BoundStatement), [imports] As IEnumerable(Of String))
            Me.Syntax = syntax
            Me.Previous = previous
            Me.Diagnostics = diagnostics
            Me.Imports = [imports]
            Me.Variables = variables.ToImmutableArray
            Me.Statements = statements.ToImmutableArray
        End Sub

    End Class

End Namespace
