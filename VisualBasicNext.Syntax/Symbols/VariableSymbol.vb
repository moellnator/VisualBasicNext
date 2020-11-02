Namespace Symbols
    Public MustInherit Class VariableSymbol : Inherits Symbol

        Protected Sub New(name As String, kind As SymbolKinds, type As Type)
            MyBase.New(name, kind)
            Me.Type = type
        End Sub

        Public ReadOnly Property Type As Type
    End Class

End Namespace
