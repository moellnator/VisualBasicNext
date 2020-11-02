Imports VisualBasicNext.Syntax.Symbols

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

End Class
