Imports VisualBasicNext.Syntax.Diagnostics
Imports VisualBasicNext.Syntax.Lexing
Imports VisualBasicNext.Syntax.Text

Module MainModule

    Sub Main()
        Dim is_exit As Boolean = False
        While Not is_exit
            _Print("»", ConsoleColor.Cyan)
            Dim input As String = Console.ReadLine
            Dim source As Source = Source.FromText(input)
            Dim lexer As New Lexer(source)
            Dim tokens As Token() = lexer.Where(Function(t) t.TokenType <> TokenTypes.WhiteSpace).ToArray
            If lexer.Diagnostics.Any() Then
                For Each e As ErrorObject In lexer.Diagnostics
                    _Print("  " & e.Message & vbNewLine, ConsoleColor.Red)
                    Dim position As Position = e.Content.GetStartPosition
                    Dim line As Line = source(position.LineNumber)
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
                For Each t As Token In tokens
                    _Print("  " & t.ToString & vbNewLine, ConsoleColor.Magenta)
                Next
            End If
        End While
    End Sub

    Private Sub _Print(text As String, Optional color As ConsoleColor = ConsoleColor.Gray)
        Dim c As ConsoleColor = Console.ForegroundColor
        Console.ForegroundColor = color
        Console.Write(text)
        Console.ForegroundColor = c
    End Sub

End Module
