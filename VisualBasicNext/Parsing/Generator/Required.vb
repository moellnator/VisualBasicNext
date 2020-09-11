Imports VisualBasicNext.Parsing.CST

Namespace Parsing.Generator
    Public Class Required : Inherits Parser


        Private ReadOnly _element As Parser

        Public Sub New(element As Parser)
            Me._element = element
        End Sub

        Protected Overrides Function _Parse(ByRef state As State) As Node
            Dim retval As Result = Me._element.Parse(state)
            If Not retval.IsValid Then Throw New ParserException($"Required '{_element.ToString}' missing after {state.Current.Location.ToString}.", state.Current.Location)
            Return retval.Node
        End Function

        Public Overrides Function ToString() As String
            Return Me._element.ToString & "!"
        End Function

    End Class

End Namespace
