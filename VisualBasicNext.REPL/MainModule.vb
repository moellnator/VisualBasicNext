Imports VisualBasicNext.Syntax
Imports VisualBasicNext.Syntax.Binding
Imports VisualBasicNext.Syntax.Diagnostics
Imports VisualBasicNext.Syntax.Evaluating
Imports VisualBasicNext.Syntax.Parsing
Imports VisualBasicNext.Syntax.Text

Module MainModule

    Sub Main()
        Dim is_exit As Boolean = False
        Dim multiline As String = ""
        Dim previous As Compilation = Nothing
        Dim vmstate As New VMState
        While Not is_exit
            _Print(If(multiline = "", "» ", "  "), ConsoleColor.Cyan)
            Dim input As String = Console.ReadLine
            Select Case input
                Case "exit"
                    is_exit = True
                Case "cls"
                    Console.Clear()
                Case Else
                    If input.Trim.EndsWith(" _") Then
                        multiline &= input.Substring(0, input.LastIndexOf(" _"))
                    Else
                        multiline &= input
                        Dim compilation As Compilation = Compilation.CreateFromText(previous, multiline)
                        Dim diagnostics As New ErrorList(compilation.Diagnostics)
                        Dim result As EvaluationResult = Nothing
                        If Not diagnostics.HasErrors Then
                            result = compilation.Evaluate(vmstate)
                            diagnostics &= result.Diagnostics
                        End If
                        If diagnostics.Any() Then
                            For Each e As ErrorObject In diagnostics
                                _Print("  " & e.Message & vbNewLine, ConsoleColor.Red)
                                Dim position As Position = e.Content.GetStartPosition
                                Dim line As Line = compilation.Source(position.LineNumber)
                                _Print("  > " & line.Content.ToString.Substring(0, position.Offset), ConsoleColor.White)
                                _Print(e.Content.ToString, ConsoleColor.Red)
                                Dim suffix As String = If(
                                        position.Offset + e.Content.Length < line.Content.Length,
                                        line.Content.ToString.Substring(position.Offset + e.Content.Length),
                                        ""
                                    )
                                _Print(suffix & vbNewLine, ConsoleColor.White)
                            Next
                        Else
                            'compilation.SyntaxTree.WriteTo(Console.Out)
                            _PrintValue(result.Value)
                            previous = compilation
                        End If
                        multiline = ""
                    End If
            End Select
        End While
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
