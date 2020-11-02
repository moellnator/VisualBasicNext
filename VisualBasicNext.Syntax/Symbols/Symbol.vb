Namespace Symbols

    Public Enum SymbolKinds
        GlobalVariable
    End Enum

    Public MustInherit Class Symbol : Implements IEquatable(Of Symbol)

        Protected Sub New(name As String, kind As SymbolKinds)
            Me.Name = name
            Me.Kind = kind
        End Sub

        Public ReadOnly Property Name As String
        Public ReadOnly Property Kind As SymbolKinds

        Public Overrides Function Equals(obj As Object) As Boolean
            Return If(GetType(Symbol).IsAssignableFrom(obj.GetType), Me.IEquatable_Equals(obj), False)
        End Function

        Private Function IEquatable_Equals(other As Symbol) As Boolean Implements IEquatable(Of Symbol).Equals
            Return Me.Name = other.Name
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return Me.Name.GetHashCode
        End Function

    End Class

End Namespace