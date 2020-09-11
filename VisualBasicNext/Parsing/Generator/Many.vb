Imports VisualBasicNext.Parsing.CST

Namespace Parsing.Generator
    Public Class Many : Inherits Parser

        Private ReadOnly _element As Parser

        Public Sub New(element As Parser)
            Me._element = element
        End Sub

        Protected Overrides Function _Parse(ByRef state As State) As Node
            Dim subresults As New List(Of Node)
            Dim current As Result = Nothing
            Do
                current = Me._element.Parse(state)
                If current.IsValid Then subresults.Add(current.Node)
            Loop While current.IsValid
            Return New Node(NodeTypes.Generic, subresults)
        End Function

        Public Overrides Function ToString() As String
            Return Me._element.ToString & "*"
        End Function

    End Class

End Namespace
