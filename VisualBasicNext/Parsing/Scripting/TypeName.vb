Imports VisualBasicNext.Parsing.Generator

Namespace Parsing.Scripting

    Public Class TypeName : Inherits Wrapper

        Private Sub New()
            MyBase.New(CST.NodeTypes.TypeName)
        End Sub

        Protected Overrides Function _MakeParser() As Parser
            Dim atom As Parser =
                    Match(Tokenizing.TokenTypes.Identifier) And
                    OneOrNone(
                        Match(Tokenizing.TokenTypes.BlockOpen, "\(") And
                        Match(Tokenizing.TokenTypes.Keyword, "[Oo]f") And
                        Require(Me) And
                        Require(Match(Tokenizing.TokenTypes.BlockClose, "\)"))
                    )
            Dim retval As Parser =
                    atom And
                    Many(
                        Match(Tokenizing.TokenTypes.Operator, "\.") And
                        Require(atom)
                    ) And
                    OneOrNone(
                        Match(Tokenizing.TokenTypes.BlockOpen, "\(") And
                        Require(Match(Tokenizing.TokenTypes.BlockClose, "\)"))
                    )
            Return retval
        End Function

    End Class

End Namespace