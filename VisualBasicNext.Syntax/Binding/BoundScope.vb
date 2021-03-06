﻿Imports System.Collections.Immutable
Imports VisualBasicNext.CodeAnalysis.Symbols

Namespace Binding
    Friend Class BoundScope

        Private ReadOnly _parent As BoundScope
        Private ReadOnly _symbols As New Dictionary(Of String, Symbol)
        Private ReadOnly _imports As New List(Of String)

        Public Sub New(parent As BoundScope)
            Me._parent = parent
            If Me._parent IsNot Nothing Then Me._imports.AddRange(parent._imports)
        End Sub

        Public Sub Import(name As String)
            If Not Me._imports.Contains(name) Then Me._imports.Add(name)
        End Sub

        Public Function TryDeclareVariable(symbol As VariableSymbol) As Boolean
            Return Me.TryDeclareSymbol(symbol)
        End Function

        Private Function TryDeclareSymbol(symbol As Symbol) As Boolean
            If Me._symbols.ContainsKey(symbol.Name) Then Return False
            Me._symbols.Add(symbol.Name, symbol)
            Return True
        End Function

        Public Function TryLookupSymbol(name As String) As Symbol
            Dim retval As Symbol = Nothing
            If Me._symbols.TryGetValue(name, retval) Then
                Return retval
            Else
                Return Me._parent?.TryLookupSymbol(name)
            End If
        End Function

        Public Function GetDeclaredVariables() As ImmutableArray(Of VariableSymbol)
            Return Me.GetDeclaredSymbols(Of VariableSymbol)
        End Function

        Private Function GetDeclaredSymbols(Of T As Symbol)() As ImmutableArray(Of T)
            Return Me._symbols.Values.OfType(Of T).ToImmutableArray
        End Function

        Public Function GetImports() As ImmutableArray(Of String)
            Return Me._imports.ToImmutableArray
        End Function

    End Class

End Namespace
