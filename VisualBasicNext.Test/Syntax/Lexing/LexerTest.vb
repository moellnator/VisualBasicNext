Imports VisualBasicNext.Syntax.Lexing
Imports VisualBasicNext.Syntax.Text

<TestClass> Public Class LexerTest

    <TestMethod> Public Sub TestEmptySource()
        Dim source As Source = Source.FromText(String.Empty)
        Dim lexer As New Lexer(source)
        Assert.IsFalse(lexer.Diagnostics.Any)
        Dim token As SyntaxToken = AssertIsSingle(lexer.ToArray)
        Assert.AreEqual(SyntaxKind.EndOfSourceToken, token.Kind)
    End Sub

    Private Shared Function AssertIsSingle(Of T)(enumeration As IEnumerable(Of T)) As T
        Assert.IsTrue(enumeration.Count = 1)
        Return enumeration.First
    End Function

    <TestMethod> Public Sub TestSingleTokens()
        For Each token_test As KeyValuePair(Of String, SyntaxKind) In TestToken
            Dim source As Source = Source.FromText(token_test.Key)
            Dim lexer As New Lexer(source)
            Dim tokens As SyntaxToken() = lexer.ToArray
            Assert.IsFalse(lexer.Diagnostics.Any)
            Assert.IsTrue(tokens.Count = 2)
            Assert.AreEqual(token_test.Value, tokens.First.Kind)
            Assert.AreEqual(token_test.Key, tokens.First.Span.ToString)
            Assert.AreEqual(SyntaxKind.EndOfSourceToken, tokens.Last.Kind)
            If TestTokenValue.ContainsKey(token_test.Key) Then
                Assert.AreEqual(TestTokenValue(token_test.Key), tokens.First.Value)
            End If
            Debug.Print(tokens.First.ToString)
        Next
    End Sub

    Private Shared ReadOnly TestToken As New Dictionary(Of String, SyntaxKind) From {
        {" ", SyntaxKind.WhiteSpaceToken},
        {"   ", SyntaxKind.WhiteSpaceToken},
        {vbNewLine, SyntaxKind.EndOfLineToken},
        {"'comment", SyntaxKind.CommentToken},
        {"""Test""""String""", SyntaxKind.StringValueToken},
        {"""a""c", SyntaxKind.StringValueToken},
        {"&HAA", SyntaxKind.NumberValueToken},
        {"&O17", SyntaxKind.NumberValueToken},
        {"&B1111", SyntaxKind.NumberValueToken},
        {"&HAASB", SyntaxKind.NumberValueToken},
        {"&HAAUB", SyntaxKind.NumberValueToken},
        {"&O17S", SyntaxKind.NumberValueToken},
        {"12", SyntaxKind.NumberValueToken},
        {"1.2", SyntaxKind.NumberValueToken},
        {"1.2e3", SyntaxKind.NumberValueToken},
        {"1.2e-3", SyntaxKind.NumberValueToken},
        {"12UI", SyntaxKind.NumberValueToken},
        {"1.2D", SyntaxKind.NumberValueToken},
        {"1.2e-3F", SyntaxKind.NumberValueToken},
        {"12SB", SyntaxKind.NumberValueToken},
        {"#13.8.2002 12:14 PM#", SyntaxKind.DateValueToken},
        {".", SyntaxKind.DotToken},
        {":", SyntaxKind.EndOfLineToken},
        {"?.", SyntaxKind.QuestionmarkDotToken},
        {"?", SyntaxKind.QuestionmarkToken},
        {",", SyntaxKind.CommaToken},
        {"(", SyntaxKind.OpenBracketToken},
        {")", SyntaxKind.CloseBracketToken},
        {"{", SyntaxKind.OpenBraceToken},
        {"}", SyntaxKind.CloseBraceToken},
        {"+", SyntaxKind.PlusToken},
        {"=", SyntaxKind.EqualsToken},
        {"+=", SyntaxKind.PlusEqualsToken},
        {"-", SyntaxKind.MinusToken},
        {"-=", SyntaxKind.MinusEqualsToken},
        {"*", SyntaxKind.StarToken},
        {"*=", SyntaxKind.StarEqualsToken},
        {"/", SyntaxKind.SlashToken},
        {"/=", SyntaxKind.SlashEqualsToken},
        {"\", SyntaxKind.BackslashToken},
        {"\=", SyntaxKind.BackslashEqualsToken},
        {"&", SyntaxKind.AmpersandToken},
        {"&=", SyntaxKind.AmpersandEqualsToken},
        {"^", SyntaxKind.CircumflexToken},
        {"^=", SyntaxKind.CircumflexEqualsToken},
        {">", SyntaxKind.GreaterToken},
        {">>", SyntaxKind.GreaterGreaterToken},
        {">>=", SyntaxKind.GreaterGreaterEqualsToken},
        {">=", SyntaxKind.GreaterEqualsToken},
        {"<>", SyntaxKind.LowerGreaterToken},
        {"<", SyntaxKind.LowerToken},
        {"<<", SyntaxKind.LowerLowerToken},
        {"<<=", SyntaxKind.LowerLowerEqualsToken},
        {"<=", SyntaxKind.LowerEqualsToken},
        {"Nothing", SyntaxKind.NothingValueToken},
        {"True", SyntaxKind.BoolValueToken},
        {"False", SyntaxKind.BoolValueToken},
        {"abs", SyntaxKind.IdentifierToken},
        {"New", SyntaxKind.NewKeywordToken}
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
