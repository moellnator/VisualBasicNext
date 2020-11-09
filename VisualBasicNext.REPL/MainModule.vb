Imports VisualBasicNext.CodeAnalysis.Diagnostics
Imports VisualBasicNext.CodeAnalysis.Text

Module MainModule

    Private Engine As New ScriptingEngine

    Sub Main()
        Engine.Import("System")
        Dim is_exit As Boolean = False
        Dim multiline As String = ""
        While Not is_exit
            _Print(If(multiline = "", "» ", "  "), ConsoleColor.Cyan)
            Dim input As String = Console.ReadLine
            Select Case input
                Case "exit"
                    is_exit = True
                Case "clear"
                    Console.Clear()
                Case "reset"
                    Engine.Reset()
                Case Else
                    If input.Trim.EndsWith(" _") Then
                        multiline &= input.Substring(0, input.LastIndexOf(" _"))
                    Else
                        multiline &= input
                        _Submit(multiline)
                        multiline = ""
                    End If
            End Select
        End While
    End Sub

    Private Sub _Submit(text As String)
        If Engine.Evaluate(text) Then
            _PrintValue(Engine.Result)
        Else
            For Each e As ErrorObject In Engine.Diagnostics
                _PrintErrorObject(e)
            Next
        End If
    End Sub

    Private Sub _PrintErrorObject(err As ErrorObject)
        _Print("  " & err.Message & vbNewLine, ConsoleColor.Red)
        Dim position As Position = err.Content.GetStartPosition
        Dim line As Line = err.Content.Source(position.LineNumber)
        _Print("  > " & line.Content.ToString.Substring(0, position.Offset), ConsoleColor.White)
        _Print(err.Content.ToString, ConsoleColor.Red)
        Dim suffix As String = If(
                position.Offset + err.Content.Length < line.Content.Length,
                line.Content.ToString.Substring(position.Offset + err.Content.Length),
                ""
            )
        _Print(suffix & vbNewLine, ConsoleColor.White)
    End Sub

    Private Sub _PrintValue(value As Object, Optional indent As String = "")
        If value IsNot Nothing Then
            If Not value.GetType.Equals(GetType(String)) AndAlso GetType(IEnumerable).IsAssignableFrom(value.GetType) Then
                Dim en As IEnumerable = value
                _Print($"  {indent}Enumerating <{value.GetType.Name}>: " & vbNewLine, ConsoleColor.White)
                For Each v As Object In en
                    _PrintValue(v, indent & "  ")
                Next
            Else
                _Print($"  {indent}<{value.GetType.Name}> ", ConsoleColor.Green)
                _Print(value.ToString & vbNewLine, ConsoleColor.DarkGreen)
            End If
        End If
    End Sub

    Private Sub _Print(text As String, Optional color As ConsoleColor = ConsoleColor.Gray)
        Dim c As ConsoleColor = Console.ForegroundColor
        Console.ForegroundColor = color
        Console.Write(text)
        Console.ForegroundColor = c
    End Sub

End Module
