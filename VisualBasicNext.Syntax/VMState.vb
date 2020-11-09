Imports System.Collections.Immutable
Imports VisualBasicNext.CodeAnalysis.Symbols

Public Class VMState

    Private ReadOnly _globals As New Dictionary(Of VariableSymbol, Object)

    Public Property Variable(symbol As VariableSymbol) As Object
        Get
            Return Me._globals(symbol)
        End Get
        Set(value As Object)
            If Me._globals.ContainsKey(symbol) Then
                Me._globals.Remove(symbol)
            End If
            Me._globals.Add(symbol, value)
        End Set
    End Property

    Public Function IsDefined(name As String) As VariableSymbol
        Return Me._globals.FirstOrDefault(Function(g) g.Key.Name = name).Key
    End Function

    Public Function GetGlobalVariableSymbols() As ImmutableArray(Of VariableSymbol)
        Return Me._globals.Keys.ToImmutableArray
    End Function

End Class
