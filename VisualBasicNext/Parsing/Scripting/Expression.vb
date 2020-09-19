Imports VisualBasicNext.Parsing.CST
Imports VisualBasicNext.Parsing.Generator

Namespace Parsing.Scripting
    Public Class Expression : Inherits Wrapper

        Public Shared ReadOnly Property Instance As Parser = New Expression

        Private Sub New()
            MyBase.New(CST.NodeTypes.Expression)
        End Sub

        Protected Overrides Function _MakeParser() As Parser
            Return _GetInstance(Of Operators)()
        End Function

        'expression = new | lambda | addressof | typeof | operators

        'new = 'new' typename '(' (expression (',' expression)*)? ')' array?
        'lambda = 'function' '(' (Identifier 'as' typename) (',' Identifier 'as' typename)* ')' expression
        'addressof = 'addressof' atom
        'typeof = TypeOf atom Is typename

        'Atomic -> Ctype(expression, typename)
        'Atomic Keywords Integer, Short, Long, U..., String, Boolean, Date, Single, Double, Decimal

        'Remove: Identifier, AtomAccess, Member -> (atom|identifier)access? and many("." identifier access? )

        Public Class Operators : Inherits Wrapper

            Private Sub New()
                MyBase.New(NodeTypes.Operators)
            End Sub

            Public Class OperatorNode : Inherits Parser

                Private ReadOnly _internal As Parser

                Public Sub New(internal As Parser)
                    Me._internal = internal
                End Sub

                Protected Overrides Function _Parse(ByRef state As State) As Node
                    Dim retval As Result = Me._internal.Parse(state)
                    Return If(retval.IsValid, New Node(NodeTypes.Operators, {retval.Node}), Nothing)
                End Function

            End Class

            Protected Overrides Function _MakeParser() As Parser
                Dim p As Parser = _MakeBinary(_GetInstance(Of Member)(), "\^")
                p = Many(Match(Tokenizing.TokenTypes.Operator, "[\+\-]")) And New OperatorNode(p)
                p = _MakeBinary(p, "[\*\/]")
                p = _MakeBinary(p, "\\")
                p = _MakeBinary(p, "[Mm]od")
                p = _MakeBinary(p, "[\+\-\&]")
                p = _MakeBinary(p, "(\<\<)|(\>\>)")
                p = _MakeBinary(p, "(\=)|(\<\>)|(\>)|(\>\=)|(\<)|(\<\=)|([Ll]ike)|([Ii]s)|([Ii]s[Nn]ot)")
                p = Many(Match(Tokenizing.TokenTypes.Operator, "[Nn]ot")) And New OperatorNode(p)
                p = _MakeBinary(p, "([Aa]nd)|([Aa]nd[Aa]lso)")
                p = _MakeBinary(p, "([Oo]r)|([Oo]r[Ee]lse)")
                p = _MakeBinary(p, "[Xx]or")
                Return p
            End Function

            Private Shared Function _MakeBinary(base As Parser, token As String) As Parser
                Dim b As Parser = New OperatorNode(base)
                Return b And Many(Match(Tokenizing.TokenTypes.Operator, token) And Require(b))
            End Function

        End Class

            Public Class Member : Inherits Wrapper

            Private Sub New()
                MyBase.New(NodeTypes.Member)
            End Sub

            Protected Overrides Function _MakeParser() As Parser
                Return _GetInstance(Of AtomAccess)() And
                       Many(
                          Match(Tokenizing.TokenTypes.Operator, "\.") And
                          Require(Match(Tokenizing.TokenTypes.Identifier)) And
                          OneOrNone(
                            Match(Tokenizing.TokenTypes.BlockOpen, "\(") And
                            Match(Tokenizing.TokenTypes.Keyword, "[Oo]f") And
                            Require(_GetInstance(Of TypeName)) And
                            Many(Match(Tokenizing.TokenTypes.Separator) And Require(_GetInstance(Of TypeName))) And
                            Require(Match(Tokenizing.TokenTypes.BlockClose, "\)"))
                          ) And
                          Many(AtomAccess.Access)
                       )
            End Function

        End Class

        Public Class AtomAccess : Inherits Wrapper

            Public Shared ReadOnly Property Access As Parser =
                Match(Tokenizing.TokenTypes.BlockOpen, "\(") And
                OneOrNone(
                    _GetInstance(Of Expression)() And
                    Many(
                        Match(Tokenizing.TokenTypes.Separator) And
                        Require(_GetInstance(Of Expression)())
                    )
                ) And
                Require(Match(Tokenizing.TokenTypes.BlockClose, "\)"))

            Private Sub New()
                MyBase.New(CST.NodeTypes.AtomAccess)
            End Sub

            Protected Overrides Function _MakeParser() As Parser
                Return _GetInstance(Of Atom)() And Many(Access)
            End Function

        End Class

        Public Class Atom : Inherits Wrapper

            Private Sub New()
                MyBase.New(CST.NodeTypes.Atom)
            End Sub

            Protected Overrides Function _MakeParser() As Parser
                Return _GetInstance(Of Literal)() Or
                    _GetInstance(Of Identifier)() Or
                    _GetInstance(Of TypeIdentifier)() Or
                    _GetInstance(Of Ternary)() Or
                    _GetInstance(Of Block)() Or
                    _GetInstance(Of Array)()
            End Function

        End Class

        Public Class Literal : Inherits Wrapper

            Private Sub New()
                MyBase.New(CST.NodeTypes.Literal)
            End Sub

            Protected Overrides Function _MakeParser() As Parser
                Return Match(Tokenizing.TokenTypes.Number) Or
                       Match(Tokenizing.TokenTypes.Character) Or
                       Match(Tokenizing.TokenTypes.String) Or
                       (Match(Tokenizing.TokenTypes.Keyword, "[Tt]rue") Or Match(Tokenizing.TokenTypes.Keyword, "[Ff]alse")) Or
                       Match(Tokenizing.TokenTypes.Keyword, "[Nn]othing")
            End Function

        End Class

        Public Class Block : Inherits Wrapper

            Private Sub New()
                MyBase.New(CST.NodeTypes.Block)
            End Sub

            Protected Overrides Function _MakeParser() As Parser
                Return Match(Tokenizing.TokenTypes.BlockOpen, "\(") And
                       Require(_GetInstance(Of Expression)) And
                       Require(Match(Tokenizing.TokenTypes.BlockClose, "\)"))
            End Function

        End Class

        Public Class Identifier : Inherits Wrapper

            Private Sub New()
                MyBase.New(CST.NodeTypes.Identifier)
            End Sub

            Protected Overrides Function _MakeParser() As Parser
                Dim atom As Parser =
                    Match(Tokenizing.TokenTypes.Identifier) And
                    OneOrNone(
                        Match(Tokenizing.TokenTypes.BlockOpen, "\(") And
                        Match(Tokenizing.TokenTypes.Keyword, "[Oo]f") And
                        Require(_GetInstance(Of TypeName)) And
                        Many(Match(Tokenizing.TokenTypes.Separator) And Require(_GetInstance(Of TypeName))) And
                        Require(Match(Tokenizing.TokenTypes.BlockClose, "\)"))
                    )
                Return atom And Many(Match(Tokenizing.TokenTypes.Operator, "\.") And Require(atom))
            End Function

        End Class

        Public Class [Array] : Inherits Wrapper

            Private Sub New()
                MyBase.New(CST.NodeTypes.Array)
            End Sub

            Protected Overrides Function _MakeParser() As Parser
                Return Match(Tokenizing.TokenTypes.BlockOpen, "\{") And
                       OneOrNone(
                            _GetInstance(Of Expression)() And
                            Many(
                                Match(Tokenizing.TokenTypes.Separator) And
                                Require(_GetInstance(Of Expression))
                            )
                       ) And
                       Require(Match(Tokenizing.TokenTypes.BlockClose, "\}"))
            End Function

        End Class

        Public Class TypeIdentifier : Inherits Wrapper

            Private Sub New()
                MyBase.New(CST.NodeTypes.TypeIdentifier)
            End Sub

            Protected Overrides Function _MakeParser() As Parser
                Return Match(Tokenizing.TokenTypes.Keyword, "[Gg]et[Tt]ype") And
                       Require(Match(Tokenizing.TokenTypes.BlockOpen, "\(")) And
                       Require(_GetInstance(Of TypeName)) And
                       Require(Match(Tokenizing.TokenTypes.BlockClose, "\)"))
            End Function

        End Class

        Public Class Ternary : Inherits Wrapper

            Private Sub New()
                MyBase.New(CST.NodeTypes.Ternary)
            End Sub

            Protected Overrides Function _MakeParser() As Parser
                Return Match(Tokenizing.TokenTypes.Keyword, "[Ii]f") And
                       Require(Match(Tokenizing.TokenTypes.BlockOpen, "\(")) And
                       Require(_GetInstance(Of Expression)()) And
                       Require(Match(Tokenizing.TokenTypes.Separator)) And
                       Require(_GetInstance(Of Expression)()) And
                       OneOrNone(
                            Match(Tokenizing.TokenTypes.Separator) And
                            Require(_GetInstance(Of Expression)())
                       ) And
                       Require(Match(Tokenizing.TokenTypes.BlockClose, "\)"))
            End Function

        End Class

    End Class

End Namespace
