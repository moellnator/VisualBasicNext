Imports VisualBasicNext.Parsing.Generator

Namespace Parsing.Scripting
    Public Class Statement : Inherits Wrapper

        Private Shared ReadOnly EndOfStatement As Parser = Match(Tokenizing.TokenTypes.EndOfLine)

        Public Sub New()
            MyBase.New(CST.NodeTypes.Statement)
        End Sub

        Protected Overrides Function _MakeParser() As Parser
            Return _GetInstance(Of Import)() Or _GetInstance(Of Declaration)()
        End Function

        Public Class Import : Inherits Wrapper

            Public Sub New()
                MyBase.New(CST.NodeTypes.ImportStatement)
            End Sub

            Protected Overrides Function _MakeParser() As Parser
                Return Match(Tokenizing.TokenTypes.Keyword, "[Ii]mports") And
                       Require(_GetInstance(Of TypeName)())
            End Function

        End Class

        Public Class Declaration : Inherits Wrapper

            Public Sub New()
                MyBase.New(CST.NodeTypes.DeclarationStatement)
            End Sub

            Protected Overrides Function _MakeParser() As Parser
                Return Match(Tokenizing.TokenTypes.Keyword, "[Dd]im") And
                    Require(Match(Tokenizing.TokenTypes.Identifier)) And
                    Require(Match(Tokenizing.TokenTypes.Keyword, "[Aa]s")) And
                    Require(_GetInstance(Of TypeName)) And
                    OneOrNone(
                        Match(Tokenizing.TokenTypes.Operator, "\=") And
                        Require(Expression.Instance)
                    )
            End Function

        End Class

        'Assignment
        'Call

    End Class

End Namespace
