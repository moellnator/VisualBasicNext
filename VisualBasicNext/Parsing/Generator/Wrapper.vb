Imports VisualBasicNext.Parsing.CST

Namespace Parsing.Generator
    Public MustInherit Class Wrapper : Inherits Parser

        Protected ReadOnly _element As Parser
        Protected ReadOnly _node_type As NodeTypes

        Private Shared ReadOnly _instances As New Dictionary(Of Type, Wrapper)

        Protected Shared Function _GetInstance(Of T As {Wrapper})() As Wrapper
            If Not _instances.ContainsKey(GetType(T)) Then
                Activator.CreateInstance(GetType(T), True)
            End If
            Return _instances(GetType(T))
        End Function

        Protected Sub New(type As NodeTypes)
            Me._node_type = type
            _instances.Add(Me.GetType, Me)
            Me._element = Me._MakeParser
        End Sub

        Protected MustOverride Function _MakeParser() As Parser

        Protected Overrides Function _Parse(ByRef state As State) As Node
            Dim retval As Result = Me._element.Parse(state)
            Return If(retval.IsValid, New Node(Me._node_type, {retval.Node}), Nothing)
        End Function

    End Class

End Namespace