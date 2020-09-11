Imports System.Reflection.Emit

Namespace Parsing.AST
    Public Class Literal : Inherits Expression

        Private Shared ReadOnly _Culture As Globalization.CultureInfo = Globalization.CultureInfo.InvariantCulture
        Private ReadOnly _object As Object
        Private Shared ReadOnly _NUM_MAP As New Dictionary(Of String, Func(Of String, Object)) From {
            {"ui", Function(s) UInteger.Parse(s, _Culture)},
            {"i", Function(s) Integer.Parse(s, _Culture)},
            {"ul", Function(s) ULong.Parse(s, _Culture)},
            {"l", Function(s) Long.Parse(s, _Culture)},
            {"us", Function(s) UShort.Parse(s, _Culture)},
            {"s", Function(s) Short.Parse(s, _Culture)},
            {"r", Function(s) Double.Parse(s, _Culture)},
            {"f", Function(s) Single.Parse(s, _Culture)},
            {"d", Function(s) Decimal.Parse(s, _Culture)}
        }
        Private Shared ReadOnly _OBJ_MAP As New Dictionary(Of Type, OpCode) From {
            {GetType(Single), OpCodes.Ldc_R4},
            {GetType(Double), OpCodes.Ldc_R8},
            {GetType(Integer), OpCodes.Ldc_I4},
            {GetType(Long), OpCodes.Ldc_I8}
        }

        Public Sub New(cst As CST.Node)
            Dim obj As Object = Nothing
            Try
                Select Case cst.Tokens.First.TokenType
                    Case Tokenizing.TokenTypes.Number
                        Dim content As String = cst.Content.ToLower
                        Dim is_unsigned As Boolean = content.Contains("u")
                        Dim suffix As String = If(is_unsigned, "u", "") & content.Last
                        content = content.Trim("d", "r", "f", "i", "l", "s", "u")
                        If content.StartsWith("&") Then content = Val(content).ToString
                        If _NUM_MAP.ContainsKey(suffix) Then
                            obj = _NUM_MAP(suffix).Invoke(content)
                        Else
                            Select Case True
                                Case "eE.".Any(Function(ch) content.Contains(ch)), Val(content) > Integer.MaxValue
                                    obj = Double.Parse(content, _Culture)
                                Case Else
                                    obj = Integer.Parse(content, _Culture)
                            End Select
                        End If
                    Case Tokenizing.TokenTypes.Character
                        obj = Char.Parse(cst.Content.Substring(1, 1))
                    Case Tokenizing.TokenTypes.String
                        obj = cst.Content.Substring(1, cst.Content.Length - 2)
                    Case Tokenizing.TokenTypes.Keyword
                        Select Case cst.Content.ToLower
                            Case "true", "false"
                                obj = Boolean.Parse(cst.Content)
                            Case "nothing"
                                obj = Nothing
                        End Select
                    Case Else
                        Throw New ParserException($"Unable to parse literal expression '{cst.Content}' at {cst.Origin.ToString}", cst.Origin)
                End Select
            Catch ex As Exception When TypeOf ex IsNot ParserException
                Throw New ParserException($"Unable to parse literal expression '{cst.Content}' at {cst.Origin.ToString}: " & ex.Message, ex, cst.Origin)
            Finally
                Me._object = obj
            End Try
        End Sub

        Public Overrides Function Evaluate(target As Virtual.State) As Object
            Return Me._object
        End Function

    End Class

End Namespace
