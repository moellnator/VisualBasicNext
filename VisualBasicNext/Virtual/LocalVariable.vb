Namespace Virtual
    Public Class LocalVariable

        Private _internal As Object
        Private ReadOnly _type As Type
        Private ReadOnly _name As String

        Public Property Value As Object
            Get
                Return Me._internal
            End Get
            Set(value As Object)
                Me._internal = CTypeDynamic(value, Me._type)
            End Set
        End Property

        Public Sub New(name As String, type As Type, Optional value As Object = Nothing)
            Me._name = name
            Me._type = type
            Me.Value = value
        End Sub

    End Class

End Namespace
