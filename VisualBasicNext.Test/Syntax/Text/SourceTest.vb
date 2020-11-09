Imports VisualBasicNext.CodeAnalysis.Text

<TestClass> Public Class SourceTest

    <TestMethod> Public Sub TestSourceCreation()
        Dim teststring As String = "Line0" & vbNewLine & "Line1" & vbCr & "Line2" & vbLf & "Line3"
        Dim source As Source = Source.FromText(teststring)
        Assert.AreEqual(4, source.Count)
    End Sub

    <TestMethod> Public Sub TestSourceLineIndex()
        Dim lines As String() = Enumerable.Range(0, 4).Select(Function(index) $"line{index.ToString}").ToArray
        Dim test As String = String.Join(vbNewLine, lines)
        Dim source As Source = Source.FromText(test)
        Dim offset As Integer = 2
        For i As Integer = 0 To 3
            Dim line As Line = source(i)
            Dim pos As Position = Position.FromSourceLocation(source, line.Start + offset)
            Assert.AreEqual(i, pos.LineNumber)
            Assert.AreEqual(offset, pos.Offset)
        Next
    End Sub

End Class
