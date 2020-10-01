﻿Imports System.Text
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
        vm.CurrentState.Import("System")
        vm.CurrentState.Import("VisualBasicNext")
        vm.CurrentState.Import("VisualBasicNext.Test")
        Dim retval As Object = vm.Evaluate("Test.SubClass(Of System.Int32).NestedSubClass.Member.Generic(Of System.Int32())({1,2,3})(2).ToString")
        If Not retval Is Nothing Then Debug.Print(retval.ToString)
    End Sub

    <TestMethod> Public Sub TestCast()
        Dim vm As New Virtual.Machine
        vm.CurrentState.Import("System")
        Dim retval As Object = vm.Evaluate("CType(123, System.String)")
        If Not retval Is Nothing Then Debug.Print(retval.ToString)
    End Sub

    <TestMethod> Public Sub TestTypeComp()
        Dim vm As New Virtual.Machine
        vm.CurrentState.Import("System")
        Dim retval As Object = vm.Evaluate("TypeOf 123 Is Integer")
        If Not retval Is Nothing Then Debug.Print(retval.ToString)
    End Sub


    <TestMethod> Public Sub TestNew()
        Dim vm As New Virtual.Machine
        vm.CurrentState.Import("System")
        Dim retval As Object = vm.Evaluate("TypeOf (New String(""c""c, 1)) Is String")
        If Not retval Is Nothing Then Debug.Print(retval.ToString)
    End Sub



End Class


Public Class SubClass(Of T)

    Public Class NestedSubClass

        Public Shared Member As NestedSubClass = Test()

        Public Property Self As NestedSubClass = Me

        Public Function Generic(Of U)(argument As U) As U
            Return argument
        End Function

        Public Shared Function Test() As NestedSubClass
            Return New NestedSubClass
        End Function

    End Class

End Class