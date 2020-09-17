Imports VisualBasicNext.Parsing.Generator

Namespace Parsing.Scripting
    Public Class Expression : Inherits Wrapper

        Public Shared ReadOnly Property Instance As Parser = New Expression

        Private Sub New()
            MyBase.New(CST.NodeTypes.Expression)
        End Sub

        Protected Overrides Function _MakeParser() As Parser
            Return _GetInstance(Of Member)()
        End Function

        'expression = bin | new | lambda | ternary | addressof | gettype

        'new = 'new' typename '(' (expression (',' expression)*)? ')' array?
        'lambda = 'function' '(' (Identifier 'as' typename) (',' Identifier 'as' typename)* ')' expression
        'ternary = 'if' '(' expression ',' expression ',' expression ')'
        'addressof = 'addressof' atom
        'gettype = 'gettype' '(' typename ')'
        'bin = or ('xor' or)*
        '    or = and (('or'|'orelse') and)*
        '        and = not (('and'|'andalso') not)*
        '            not = 'not'* compare
        '                compare = (shift (('='|'<>'|'>'|'>='|'<'|'<='|'like'|'is'|'isnot') shift)*) | ('typeof' atom ('is'|'isnot') typename)
        '                    shift = add(('<<'|'>>') add)*
        '                        add = mod ([+-] mod) *
        '                            mod = intmul ('mod' intmul) *
        '                                intmul = mul('\' mul)*
        '                                    mul = unary (([*/]) unary) *
        '                                        unary = [+-]* power
        '                                            power = access ('^' access)*

        Public Class Member : Inherits Wrapper

            Private Sub New()
                MyBase.New(CST.NodeTypes.Member)
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
                Return _GetInstance(Of Literal)() Or _GetInstance(Of Identifier)() Or _GetInstance(Of Block)() Or _GetInstance(Of Array)()
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

    End Class

End Namespace
