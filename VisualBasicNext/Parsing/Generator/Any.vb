Imports VisualBasicNext.Parsing.CST

Namespace Parsing.Generator
    Public Class Any : Inherits Parser

        Protected Overrides Function _Parse(ByRef state As State) As Node
            Return If(state.MoveNext, New Node(NodeTypes.Generic, {state.Current}), Nothing)
        End Function

        Public Overrides Function ToString() As String
            Return "."
        End Function

    End Class

End Namespace
