Imports VisualBasicNext.Parsing.Generator

Namespace Parsing.Scripting
    Public Class Script : Inherits Wrapper

        Public Shared ReadOnly Property Instance As Parser = New Script

        Public Sub New()
            MyBase.New(CST.NodeTypes.Script)
        End Sub

        Protected Overrides Function _MakeParser() As Parser
            Return OneOrNone(
                OneOrNone(_GetInstance(Of Statement)()) And
                Many(
                    Match(Tokenizing.TokenTypes.EndOfLine) And
                    OneOrNone(_GetInstance(Of Statement)())
                )
            )
        End Function

    End Class

End Namespace
