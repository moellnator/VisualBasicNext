Imports VisualBasicNext.Parsing.CST

Namespace Parsing.Generator
    Public Class [Option] : Inherits Parser

        Private _parsers As Parser()

        Public Sub New(elements As IEnumerable(Of Parser))
            Dim parsers As New List(Of Parser)
            For Each p As Parser In elements
                If TypeOf p Is [Option] Then
                    parsers.AddRange(DirectCast(p, [Option])._parsers)
                Else
                    parsers.Add(p)
                End If
            Next
            Me._parsers = parsers.ToArray
        End Sub

        Protected Overrides Function _Parse(ByRef state As State) As Node
            Dim retval As Node = Nothing
            For Each p In Me._parsers
                Dim current As State = state.Clone
                Dim result As Result = p.Parse(current)
                If result.IsValid Then
                    state = current
                    retval = result.Node
                    Exit For
                End If
            Next
            Return retval
        End Function

        Public Overrides Function ToString() As String
            Return "(" & String.Join("|", Me._parsers.Select(Function(p) p.ToString)) & ")"
        End Function

    End Class

End Namespace
