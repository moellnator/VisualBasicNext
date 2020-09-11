Imports VisualBasicNext.Parsing.CST

Namespace Parsing.Generator
    Public Class Sequence : Inherits Parser

        Private _parsers As Parser()

        Public Sub New(elements As IEnumerable(Of Parser))
            Dim parsers As New List(Of Parser)
            For Each p As Parser In elements
                If TypeOf p Is Sequence Then
                    parsers.AddRange(DirectCast(p, Sequence)._parsers)
                Else
                    parsers.Add(p)
                End If
            Next
            Me._parsers = parsers.ToArray
        End Sub

        Protected Overrides Function _Parse(ByRef state As State) As Node
            Dim subresults As New List(Of Node)
            For Each p As Parser In Me._parsers
                Dim retval As Result = p.Parse(state)
                If retval.IsValid Then
                    subresults.Add(retval.Node)
                Else
                    Return Nothing
                End If
            Next
            Return New Node(NodeTypes.Generic, subresults)
        End Function


        Public Overrides Function ToString() As String
            Return "(" & String.Join(" ", Me._parsers.Select(Function(p) p.ToString)) & ")"
        End Function


    End Class

End Namespace
