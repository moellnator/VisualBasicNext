Namespace Virtual
    Public Class Context

        Public Class Frame

            Private ReadOnly _context As Context
            Private ReadOnly _variables As LocalVariable()

            Public Sub New(context As Context)
                Me._context = context
                Me._variables = Me._context._variables.Select(Function(v) New LocalVariable(v.Name, v.Type)).ToArray
            End Sub

            Public Sub Initialize(values As Object())
                For index As Integer = 0 To values.Count - 1
                    Me._variables(index).Value = values(index)
                Next
            End Sub

            Public Sub Activate()
                For index As Integer = 0 To Me._variables.Count - 1
                    Me._context._variables(index).SetLocal(Me._variables(index))
                Next
            End Sub

            Public Sub Deactivate()
                For index As Integer = 0 To Me._variables.Count - 1
                    Me._context._variables(index).Reset()
                Next
            End Sub

        End Class

        Private _parent As Context
        Private ReadOnly _variables As DynamicVariable()
        Private ReadOnly _frames As New Stack(Of Frame)

        Public ReadOnly Property Parent As Context
            Get
                Return Me._parent
            End Get
        End Property

        Public Sub New(arguments As String(), types As Type())
            Me._variables = Enumerable.Range(0, arguments.Count).Select(
                Function(index) New DynamicVariable(arguments(index), types(index))
            ).ToArray
        End Sub

        Public Sub SetParent(parent As Context)
            Me._parent = parent
        End Sub

        Public Function IsDeclared(name As String) As Boolean
            Dim retval As Boolean = False
            If Me._variables.Any(Function(v) v.Name = name) Then
                retval = True
            ElseIf Me._parent IsNot Nothing Then
                retval = Me._parent.IsDeclared(name)
            End If
            Return retval
        End Function

        Public Function GetVariable(name As String) As LocalVariable
            Dim retval As LocalVariable = Me._variables.FirstOrDefault(Function(v) v.Name = name)
            If retval Is Nothing AndAlso Me._parent IsNot Nothing Then retval = Me._parent.GetVariable(name)
            Return retval
        End Function

        Public Sub Enter(arguments As Object())
            Dim nextframe As New Frame(Me)
            nextframe.Initialize(arguments)
            Me._frames.Push(nextframe)
            nextframe.Activate()
        End Sub

        Public Sub Leave()
            If Me._frames.Count = 0 Then Throw New Exception("Frame stack underflow.")
            Dim current As Frame = Me._frames.Pop
            current.Deactivate()
            If Me._frames.Count <> 0 Then Me._frames.Peek.Activate()
        End Sub

    End Class

End Namespace
