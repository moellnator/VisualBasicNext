Imports VisualBasicNext.Syntax
Imports VisualBasicNext.Syntax.Diagnostics
Imports VisualBasicNext.Syntax.Evaluating

<TestClass> Public Class EvaluationTest

    <TestMethod> Public Sub TestArray()
        Dim test As String = "{1,2,3}"
        Dim expected As Integer() = {1, 2, 3}
        Dim value As Object = AssertEvaluates(test)
        Assert.IsInstanceOfType(value, GetType(Integer()))
        Assert.AreEqual(expected.Length, value.Length)
        For index As Integer = 0 To expected.Count - 1
            Assert.AreEqual(expected(index), value(index))
        Next
    End Sub

    <TestMethod> Public Sub TestParserRounttrip()
        For Each test As KeyValuePair(Of String, Object) In _Tests
            Dim value As Object = AssertEvaluates(test.Key)
            Assert.AreEqual(test.Value, value)
        Next
    End Sub

    Private Shared Function AssertEvaluates(text As String) As Object
        Dim state As New VMState
        Dim compilation As Compilation = Compilation.CreateFromText(Nothing, text)
        Dim diagnostics As New ErrorList(compilation.Diagnostics)
        Dim result As EvaluationResult = Nothing
        Assert.IsFalse(diagnostics.HasErrors)
        result = compilation.Evaluate(state)
        diagnostics &= result.Diagnostics
        Assert.IsFalse(diagnostics.HasErrors)
        Return result.Value
    End Function

    Private Shared ReadOnly _Tests As New Dictionary(Of String, Object) From {
        {"", Nothing},
        {"1", 1},
        {"-1", -1},
        {"+1", 1},
        {"1.0f", 1.0F},
        {"1.0e-10", 0.0000000001},
        {"1e+3", 1000.0},
        {"true", True},
        {"false", False},
        {"not false", True},
        {"nothing", Nothing},
        {"""test""", "test"},
        {"""test""""""", "test"""},
        {"$""test{42}""", "test42"},
        {"$""A{$""B{""C""}""}""", "ABC"},
        {"""a"" & ""b""", "ab"},
        {"1+2", 3},
        {"1+-2", -1},
        {"not 1", -2},
        {"1-3", -2},
        {"2*3", 6},
        {"4\2", 2},
        {"4/2", 2.0},
        {"4 mod 3", 1},
        {"2^2", 4.0},
        {"2 << 2", 8},
        {"4 >> 2", 1},
        {"true and true", True},
        {"true and false", False},
        {"true or false", True},
        {"false or false", False},
        {"true xor true", False},
        {"true xor false", True},
        {"3 xor 1", 2},
        {"3 and 1", 1},
        {"3 or 1", 3},
        {"&HFF", 255},
        {"&o10", 8},
        {"&b11", 3},
        {"&b11US", CUShort(3)},
        {"3 > 2", True},
        {"3 < 2", False},
        {"2 <= 2", True},
        {"2 >= 2", True},
        {"2 = 2", True},
        {"2 <> 2", False},
        {"1+2*3", 7},
        {"(1+2)*3", 9},
        {"if(true,3,5)", 3},
        {"if(false,3,5)", 5},
        {"gettype(integer)", GetType(Integer)},
        {"dim a as object = nothing : a is nothing", True},
        {"dim a as string = 2 : a", "2"},
        {"dim a as integer = ""2"" : a", 2},
        {"CType(""true"", boolean)", True},
        {"CTypeDynamic(""2"", gettype(integer))", 2},
        {"TryCast(""2"", integer)", 2},
        {"TryCast(""a"", integer)", Nothing},
        {"if(nothing, ""a"")", "a"}
    }

End Class
