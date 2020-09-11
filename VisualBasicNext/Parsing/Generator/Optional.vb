Imports VisualBasicNext.Parsing.CST

Namespace Parsing.Generator
    Public Class [Optional] : Inherits Parser

        Private ReadOnly _element As Parser

        Public Sub New(element As Parser)
            Me._element = element
        End Sub

        Protected Overrides Function _Parse(ByRef state As State) As Node
            Dim retval As Node() = {}
            Dim result As Result = Me._element.Parse(state)
            If result.IsValid Then retval = {result.Node}
            Return New Node(NodeTypes.Generic, retval)
        End Function

        Public Overrides Function ToString() As String
            Return Me._element.ToString & "?"
        End Function

    End Class

End Namespace
