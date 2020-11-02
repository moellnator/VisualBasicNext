Namespace Symbols
    Public Class GlobalVariableSymbol : Inherits VariableSymbol

        Public Sub New(name As String, type As Type)
            MyBase.New(name, SymbolKinds.GlobalVariable, type)
        End Sub

    End Class

End Namespace
