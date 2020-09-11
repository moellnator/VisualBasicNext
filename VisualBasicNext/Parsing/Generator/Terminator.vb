Imports VisualBasicNext.Parsing.CST
Imports VisualBasicNext.Parsing.Tokenizing

Namespace Parsing.Generator
    Public Class Terminator : Inherits Parser

        Protected Overrides Function _Parse(ByRef state As State) As Node
            Return If(state.MoveNext, Nothing, New Node(NodeTypes.Generic, New Token() {}))
        End Function

        Public Overrides Function ToString() As String
            Return "$"
        End Function

    End Class

End Namespace