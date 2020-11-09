Imports System.Collections.Immutable
Imports VisualBasicNext.CodeAnalysis
Imports VisualBasicNext.CodeAnalysis.Diagnostics
Imports VisualBasicNext.CodeAnalysis.Evaluating
Imports VisualBasicNext.CodeAnalysis.Symbols

Public Class ScriptEngine

    Private _state As New VMState
    Private _compilation As Compilation = Nothing
    Private _diagnostics As ErrorList
    Private _value As Object

    Public Function Evaluate(text As String) As Boolean
        Me._diagnostics = Nothing
        Me._value = Nothing
        Dim compilation As Compilation = Compilation.CreateFromText(Me._compilation, text)
        Dim diagnostics As New ErrorList(compilation.Diagnostics)
        Dim result As EvaluationResult = Nothing
        If Not diagnostics.HasErrors Then
            result = compilation.Evaluate(Me._state)
            diagnostics &= result.Diagnostics
            If Not diagnostics.HasErrors Then
                Me._compilation = compilation
                Me._value = result.Value
                Return True
            End If
        End If
        Me._diagnostics = diagnostics
        Return False
    End Function

    Public Function Import(name As String) As Boolean
        Dim old_diagnostics As ErrorList = Me._diagnostics
        Dim old_Value As Object = Me._value
        Dim retval As Boolean = Me.Evaluate("Imports " & name)
        Me._value = old_Value
        Me._diagnostics = old_diagnostics
        Return retval
    End Function

    Public ReadOnly Property Diagnostics As ErrorList
        Get
            Return If(Me._diagnostics, New ErrorList)
        End Get
    End Property

    Public ReadOnly Property Result As Object
        Get
            Return Me._value
        End Get
    End Property

    Public Sub Reset()
        Me._compilation = Nothing
        Me._state = New VMState
        Me._diagnostics = Nothing
    End Sub

    Public Property GlobalVariable(name As String) As Object
        Get
            Dim symbol As VariableSymbol = Me._state.IsDefined(name)
            If symbol Is Nothing Then Throw New ArgumentException($"No such symbol '{name}' in global variables.", NameOf(name))
            Return Me._state.Variable(symbol)
        End Get
        Set(value As Object)
            Dim symbol As VariableSymbol = Me._state.IsDefined(name)
            If symbol Is Nothing Then Throw New ArgumentException($"No such symbol '{name}' in global variables.", NameOf(name))
            Me._state.Variable(symbol) = CTypeDynamic(value, symbol.Type)
        End Set
    End Property

    Public Sub DeclareGlobalVariable(name As String, type As Type, Optional value As Object = Nothing)
        If Me._state.IsDefined(name) IsNot Nothing Then Throw New ArgumentException($"A variable symbol with name '{name}' is already defined.", NameOf(name))
        value = CTypeDynamic(value, type)
        Me._state.Variable(New GlobalVariableSymbol(name, type)) = value
    End Sub

    Public Function GetGlobalVariables() As ImmutableArray(Of String)
        Return Me._state.GetGlobalVariableSymbols.Select(Function(sym) sym.Name).ToImmutableArray
    End Function

End Class
