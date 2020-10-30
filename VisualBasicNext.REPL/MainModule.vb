Imports VisualBasicNext.Syntax.Diagnostics
Imports VisualBasicNext.Syntax.Parsing
Imports VisualBasicNext.Syntax.Text

Module MainModule

    Sub Main()
        Dim is_exit As Boolean = False
        Dim multiline As String = ""
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
                        Dim parser As New Parser(multiline)
                        Dim script As ScriptNode = parser.Parse
                        If parser.Diagnostics.Any() Then
                            For Each e As ErrorObject In parser.Diagnostics
                                _Print("  " & e.Message & vbNewLine, ConsoleColor.Red)
                                Dim position As Position = e.Content.GetStartPosition
                                Dim line As Line = parser.Text(position.LineNumber)
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
                            script.WriteTo(Console.Out)
                        End If
                        multiline = ""
                    End If
            End Select
        End While
    End Sub

    Private Sub _Print(text As String, Optional color As ConsoleColor = ConsoleColor.Gray)
        Dim c As ConsoleColor = Console.ForegroundColor
        Console.ForegroundColor = color
        Console.Write(text)
        Console.ForegroundColor = c
    End Sub

End Module
