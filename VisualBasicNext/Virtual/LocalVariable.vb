Namespace Virtual
    Public Class LocalVariable

        Private _internal As Object
        Public ReadOnly Property Type As Type
        Public ReadOnly Property Name As String

        Public Overridable Property Value As Object
            Get
                Return Me._internal
            End Get
            Set(value As Object)
                Me._internal = CTypeDynamic(value, Me.Type)
            End Set
        End Property

        Public Sub New(name As String, type As Type, Optional value As Object = Nothing)
            Me.Name = name
            Me.Type = type
            Me.Value = value
        End Sub

    End Class

End Namespace
