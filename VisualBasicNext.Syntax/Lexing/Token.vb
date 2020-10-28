Namespace Lexing

    Public Enum TokenTypes
        BadToken = -1
        WhiteSpace = 0
        EndOfSource
        Newline
        PlusEqualsToken
        Comment
        StringValue
        NumberValue
        DateValue
        QuestionmarkToken
        DotToken
        QuestionmarkDotToken
        CommaToken
        OpenBracketToken
        CloseBracketToken
        OpenBraceToken
        CloseBraceToken
        Questionmark
        EqualsToken
        PlusToken
        MinusEqualsToken
        MinusToken
        StarToken
        StarEqualsToken
        SlashEqualsToken
        SlashToken
        BackslashEqualsToken
        AmpersandToken
        AmpersandEqualsToken
        BackslashToken
        CircumflexEqualsToken
        CircumflexToken
        GreatethanToken
        CircuamflexToken
        GreaterToken
        GreaterGreaterToken
        GreaterGreaterEqualsToken
        GreaterEqualsToken
        LowerLowerEqualsToken
        LowerLowerToken
        LowerEqualsToken
        LowerGreaterToken
        LowerToken
        Identifier
        BoolValue
        NothingValue
        NotKeyword
        OrKeyword
        XorKeyword
        ModKeyword
        AndAlsoKeyword
        OrElseKeyword
        IsKeyword
        IsNotKeyword
        LikeKeyword
        IfKeyword
        TypeOfKeyword
        NewKeyword
        CTypeKeyword
        AsKeyword
        OfKeyword
        FunctionKeyword
        SubKeyword
    End Enum

    Public Class Token

        Public ReadOnly Property TokenType As TokenTypes
        Public ReadOnly Property Source As Text.Span
        Public ReadOnly Property Value As Object = Nothing

        Public Sub New(tokenType As TokenTypes, source As Text.Span, Optional value As Object = Nothing)
            Me.TokenType = tokenType
            Me.Source = source
            Me.Value = value
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Me.TokenType.ToString} ({If(Value IsNot Nothing, "<" & Value.GetType.ToString & "> " & Value.ToString, "")})"
        End Function

    End Class

End Namespace
