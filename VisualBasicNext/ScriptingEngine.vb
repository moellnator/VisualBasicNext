Imports System.Collections.Immutable
Imports VisualBasicNext.CodeAnalysis
Imports VisualBasicNext.CodeAnalysis.Diagnostics
Imports VisualBasicNext.CodeAnalysis.Evaluating
Imports VisualBasicNext.CodeAnalysis.Symbols

''' <summary>
''' This class represents an encapsulated API entity for VB.NeXt scripting.
''' </summary>
Public Class ScriptingEngine

    Private _state As New VMState
    Private _compilation As Compilation = Nothing
    Private _diagnostics As ErrorList
    Private _value As Object

    ''' <summary>
    ''' Evaluates the given text within the context of the current script engine state.
    ''' </summary>
    ''' <param name="text">The text to evalute</param>
    ''' <returns>Returns whether or not the evaluation succeeded.</returns>
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

    ''' <summary>
    ''' Imports the given namespace into the current scripting context.
    ''' </summary>
    ''' <param name="name">The name of the namespace to import</param>
    ''' <returns>Returns whether or not the import succeeded.</returns>
    Public Function Import(name As String) As Boolean
        Dim old_diagnostics As ErrorList = Me._diagnostics
        Dim old_Value As Object = Me._value
        Dim retval As Boolean = Me.Evaluate("Imports " & name)
        Me._value = old_Value
        Me._diagnostics = old_diagnostics
        Return retval
    End Function

    ''' <summary>
    ''' The diagnostic information produced by the latest script evaluation.
    ''' </summary>
    ''' <returns>Returns a list of diagnostic items that occured during the latest script evaluation.</returns>
    Public ReadOnly Property Diagnostics As ErrorList
        Get
            Return If(Me._diagnostics, New ErrorList)
        End Get
    End Property

    ''' <summary>
    ''' The resulting value of the latest successful script evaluation.
    ''' </summary>
    ''' <returns>Returns the resulting value of the latest script evaluation if it was successful or nothing otherwise.</returns>
    Public ReadOnly Property Result As Object
        Get
            Return Me._value
        End Get
    End Property

    ''' <summary>
    ''' Reset the internal state of the scripting engine.
    ''' </summary>
    Public Sub Reset()
        Me._compilation = Nothing
        Me._state = New VMState
        Me._diagnostics = Nothing
    End Sub

    ''' <summary>
    ''' Gets or sets the value for the named global variable in the current scripting context.
    ''' </summary>
    ''' <param name="name">The name of the global variable</param>
    ''' <returns>Returns the value of the named global variable if it exists within the current scripting context.</returns>
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

    ''' <summary>
    ''' Declares a global variable symbol within the current scripting context.
    ''' </summary>
    ''' <param name="name">The name of the new global variable</param>
    ''' <param name="type">The runtime type of the new global variable</param>
    ''' <param name="value">The initial value assigned to the blobal variable</param>
    Public Sub DeclareGlobalVariable(name As String, type As Type, Optional value As Object = Nothing)
        If Me._state.IsDefined(name) IsNot Nothing Then Throw New ArgumentException($"A variable symbol with name '{name}' is already defined.", NameOf(name))
        value = CTypeDynamic(value, type)
        Me._state.Variable(New GlobalVariableSymbol(name, type)) = value
    End Sub

    ''' <summary>
    ''' Lists the names of all currently defined global variables withing the current scripting context.
    ''' </summary>
    ''' <returns>Return a list of global variable names.</returns>
    Public Function GetGlobalVariables() As ImmutableArray(Of String)
        Return Me._state.GetGlobalVariableSymbols.Select(Function(sym) sym.Name).ToImmutableArray
    End Function

End Class
