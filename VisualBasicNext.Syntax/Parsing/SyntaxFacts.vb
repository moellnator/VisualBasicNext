Imports System.Runtime.CompilerServices
Imports VisualBasicNext.CodeAnalysis.Lexing

Namespace Parsing
    Friend Module SyntaxFacts

        <Extension> Public Function GetUnaryOperatorPrecedence(kind As SyntaxKind) As Integer
            Dim retval As Integer = 0
            Select Case kind
                Case SyntaxKind.PlusToken, SyntaxKind.MinusToken
                    retval = 12
                Case SyntaxKind.NotKeywordToken
                    retval = 4
            End Select
            Return retval
        End Function

        <Extension> Public Function GetBinaryOperatorPrecedence(kind As SyntaxKind) As Integer
            Dim retval As Integer = 0
            Select Case kind
                Case SyntaxKind.CircumflexToken
                    retval = 13
                Case SyntaxKind.StarToken, SyntaxKind.SlashToken
                    retval = 11
                Case SyntaxKind.BackslashToken
                    retval = 10
                Case SyntaxKind.ModKeywordToken
                    retval = 9
                Case SyntaxKind.PlusToken, SyntaxKind.MinusToken
                    retval = 8
                Case SyntaxKind.AmpersandToken
                    retval = 7
                Case SyntaxKind.GreaterGreaterToken, SyntaxKind.LowerLowerToken
                    retval = 6
                Case SyntaxKind.GreaterToken, SyntaxKind.GreaterToken,
                     SyntaxKind.GreaterEqualsToken,
                     SyntaxKind.LowerToken,
                     SyntaxKind.LowerEqualsToken,
                     SyntaxKind.EqualsToken,
                     SyntaxKind.LowerGreaterToken,
                     SyntaxKind.IsKeywordToken,
                     SyntaxKind.IsNotKeywordToken,
                     SyntaxKind.LikeKeywordToken
                    retval = 5
                Case SyntaxKind.AndKeywordToken, SyntaxKind.AndAlsoKeywordToken
                    retval = 3
                Case SyntaxKind.OrKeywordToken, SyntaxKind.OrElseKeywordToken
                    retval = 2
                Case SyntaxKind.XorKeywordToken
                    retval = 1
            End Select
            Return retval
        End Function

    End Module

End Namespace
