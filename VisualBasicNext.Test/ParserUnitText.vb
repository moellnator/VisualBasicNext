Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports VisualBasicNext.Parsing
Imports VisualBasicNext.Parsing.Generator
Imports VisualBasicNext.Parsing.Scripting
Imports VisualBasicNext.Parsing.Tokenizing

<TestClass()> Public Class ParserUnitText

    <TestMethod> Public Sub TestParser()
        Dim vm As New Virtual.Machine
        Dim retval As Object = vm.Evaluate("123f")
        Debug.Print($"({retval.GetType.FullName}) {retval.ToString}")
    End Sub

    <TestMethod> Public Sub TestQualifier()
        Dim vm As New Virtual.Machine
        vm.CurrentState.DeclareLocal("test", 15)
        Dim retval As Object = vm.Evaluate("test")
        Stop
    End Sub

End Class