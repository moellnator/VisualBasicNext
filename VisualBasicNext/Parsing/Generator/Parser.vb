Namespace Parsing.Generator
    Public MustInherit Class Parser

        Public Function Parse(state As State) As Result
            Dim current As State = state.Clone
            Dim retval As CST.Node = Me._Parse(current)
            If retval IsNot Nothing Then state.Update(current)
            Return New Result(retval)
        End Function

        Protected MustOverride Function _Parse(ByRef state As State) As CST.Node

        Public Shared Operator And(a As Parser, b As Parser) As Parser
            Return New Sequence({a, b})
        End Operator

        Public Shared Operator Or(a As Parser, b As Parser) As Parser
            Return New [Option]({a, b})
        End Operator

        Public Shared Function Any() As Parser
            Return New Any
        End Function

        Public Shared Function Terminator() As Parser
            Return New Terminator
        End Function

        Public Shared Function Many(a As Parser) As Parser
            Return New Many(a)
        End Function

        Public Shared Function OneOrMore(a As Parser) As Parser
            Return a And Many(a)
        End Function

        Public Shared Function OneOrNone(a As Parser) As Parser
            Return New [Optional](a)
        End Function

        Public Shared Function Match(type As Tokenizing.TokenTypes, Optional regex As String = "") As Parser
            Return New Match(type, regex)
        End Function

        Public Shared Function Require(a As Parser) As Parser
            Return New Required(a)
        End Function

    End Class

End Namespace
