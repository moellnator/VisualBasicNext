Namespace Parsing.Generator
    Public Class Result

        Public ReadOnly Property IsValid As Boolean
            Get
                Return Me.Node IsNot Nothing
            End Get
        End Property

        Public ReadOnly Property Node As CST.Node

        Public Sub New(node As CST.Node)
            Me.Node = node
        End Sub

    End Class

End Namespace
