Namespace Virtual
    Public Class DynamicVariable : Inherits LocalVariable

        Private _wrapped As LocalVariable

        Public Overrides Property Value As Object
            Get
                Return Me._wrapped.Value
            End Get
            Set(value As Object)
                If Me._wrapped Is Nothing Then Exit Property
                Me._wrapped.Value = value
            End Set
        End Property

        Public Sub New(name As String, type As Type)
            MyBase.New(name, type)
        End Sub

        Public Sub SetLocal(var As LocalVariable)
            Me._wrapped = var
        End Sub

        Public Sub Reset()
            Me._wrapped = Nothing
        End Sub

    End Class

End Namespace
