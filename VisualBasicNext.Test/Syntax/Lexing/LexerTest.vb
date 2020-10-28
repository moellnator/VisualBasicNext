Imports VisualBasicNext.Syntax.Lexing
Imports VisualBasicNext.Syntax.Text

<TestClass> Public Class LexerTest

    <TestMethod> Public Sub TestEmptySource()
        Dim source As Source = Source.FromText(String.Empty)
        Dim lexer As New Lexer(source)
        Assert.IsFalse(lexer.Diagnostics.Any)
        Dim token As Token = AssertIsSingle(lexer.ToArray)
        Assert.AreEqual(TokenTypes.EndOfSource, token.TokenType)
    End Sub

    Private Shared Function AssertIsSingle(Of T)(enumeration As IEnumerable(Of T)) As T
        Assert.IsTrue(enumeration.Count = 1)
        Return enumeration.First
    End Function

    <TestMethod> Public Sub TestSingleTokens()
        For Each token_test As KeyValuePair(Of String, TokenTypes) In TestToken
            Dim source As Source = Source.FromText(token_test.Key)
            Dim lexer As New Lexer(source)
            Dim tokens As Token() = lexer.ToArray
            Assert.IsFalse(lexer.Diagnostics.Any)
            Assert.IsTrue(tokens.Count = 2)
            Assert.AreEqual(token_test.Value, tokens.First.TokenType)
            Assert.AreEqual(token_test.Key, tokens.First.Source.ToString)
            Assert.AreEqual(TokenTypes.EndOfSource, tokens.Last.TokenType)
            If TestTokenValue.ContainsKey(token_test.Key) Then
                Assert.AreEqual(TestTokenValue(token_test.Key), tokens.First.Value)
            End If
            Debug.Print(tokens.First.ToString)
        Next
    End Sub

    Private Shared ReadOnly TestToken As New Dictionary(Of String, TokenTypes) From {
        {" ", TokenTypes.WhiteSpace},
        {"   ", TokenTypes.WhiteSpace},
        {vbNewLine, TokenTypes.PlusEqualsToken},
        {"'comment", TokenTypes.Comment},
        {"""Test""""String""", TokenTypes.StringValue},
        {"""a""c", TokenTypes.StringValue},
        {"&HAA", TokenTypes.NumberValue},
        {"&O17", TokenTypes.NumberValue},
        {"&B1111", TokenTypes.NumberValue},
        {"&HAASB", TokenTypes.NumberValue},
        {"&HAAUB", TokenTypes.NumberValue},
        {"&O17S", TokenTypes.NumberValue},
        {"12", TokenTypes.NumberValue},
        {"1.2", TokenTypes.NumberValue},
        {"1.2e3", TokenTypes.NumberValue},
        {"1.2e-3", TokenTypes.NumberValue},
        {"12UI", TokenTypes.NumberValue},
        {"1.2D", TokenTypes.NumberValue},
        {"1.2e-3F", TokenTypes.NumberValue},
        {"12SB", TokenTypes.NumberValue},
        {"#13.8.2002 12:14 PM#", TokenTypes.DateValue},
        {".", TokenTypes.DotToken},
        {"?.", TokenTypes.QuestionmarkDotToken},
        {"?", TokenTypes.QuestionmarkToken},
        {",", TokenTypes.CommaToken},
        {"(", TokenTypes.OpenBracketToken},
        {")", TokenTypes.CloseBracketToken},
        {"{", TokenTypes.OpenBraceToken},
        {"}", TokenTypes.CloseBraceToken},
        {"+", TokenTypes.PlusToken},
        {"=", TokenTypes.EqualsToken},
        {"+=", TokenTypes.PlusEqualsToken},
        {"-", TokenTypes.MinusToken},
        {"-=", TokenTypes.MinusEqualsToken},
        {"*", TokenTypes.StarToken},
        {"*=", TokenTypes.StarEqualsToken},
        {"/", TokenTypes.SlashToken},
        {"/=", TokenTypes.SlashEqualsToken},
        {"\", TokenTypes.BackslashToken},
        {"\=", TokenTypes.BackslashEqualsToken},
        {"&", TokenTypes.AmpersandToken},
        {"&=", TokenTypes.AmpersandEqualsToken},
        {"^", TokenTypes.CircumflexToken},
        {"^=", TokenTypes.CircumflexEqualsToken},
        {">", TokenTypes.GreaterToken},
        {">>", TokenTypes.GreaterGreaterToken},
        {">>=", TokenTypes.GreaterGreaterEqualsToken},
        {">=", TokenTypes.GreaterEqualsToken},
        {"<>", TokenTypes.LowerGreaterToken},
        {"<", TokenTypes.LowerToken},
        {"<<", TokenTypes.LowerLowerToken},
        {"<<=", TokenTypes.LowerLowerEqualsToken},
        {"<=", TokenTypes.LowerEqualsToken},
        {"Nothing", TokenTypes.NothingValue},
        {"True", TokenTypes.BoolValue},
        {"False", TokenTypes.BoolValue},
        {"abs", TokenTypes.Identifier},
        {"New", TokenTypes.NewKeyword}
    }

    Private Shared ReadOnly TestTokenValue As New Dictionary(Of String, Object) From {
        {"""Test""""String""", "Test""String"},
        {"""a""c", "a"c},
        {"&HAA", &HAA},
        {"&O17", &O17},
        {"&B1111", &B1111},
        {"&HAASB", Convert.ToSByte("AA", 16)},
        {"&HAAUB", Convert.ToByte(&HAA)},
        {"&O17S", &O17S},
        {"12", 12},
        {"1.2", 1.2},
        {"1.2e3", 1200.0},
        {"1.2e-3", 0.0012},
        {"12UI", 12UI},
        {"1.2D", 1.2D},
        {"1.2e-3F", 0.0012F},
        {"12SB", Convert.ToSByte(12)},
        {"#13.8.2002 12:14 PM#", #8/13/2002 12:14 PM#},
        {"Nothing", Nothing},
        {"True", True},
        {"False", False}
    }

End Class
