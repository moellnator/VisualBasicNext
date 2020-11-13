Imports VisualBasicNext.CodeAnalysis.Diagnostics

Public Class OutputElement

    'TODO -> add more object formatting, i.e. arrays, enumerables, sturcts, etc...

    Public Sub New()
        InitializeComponent()
        Me._hide_linenumber = True
        Me._line_overhead = 0
    End Sub

    Public Sub SetValue(value As Object, diagnostics As ErrorList)
        If diagnostics.HasErrors Then
            Me.SetHightlight(ColorPalette.ColorSyntaxError)
            Me._Text = _FormatDiagnostics(diagnostics)
        Else
            Me.SetHightlight(ColorPalette.ColorStructure)
            Me._Text = _FormatValue(value)
        End If
        Me.AutoSizeElement()
        Me.Invalidate(True)
    End Sub

    Private Shared Function _FormatValue(value As Object) As FormattedText
        Dim builder As New FormattedTextBuilder
        builder.Append("<", ColorPalette.ColorOperator)
        Dim value_type As Type = If(value IsNot Nothing, value.GetType, GetType(Object))
        Dim type_color As Color = If(value_type.IsValueType, ColorPalette.ColorStructure, ColorPalette.ColorTypeName)
        builder.Append(value_type.Name, type_color)
        builder.Append("> ", ColorPalette.ColorOperator)
        builder.Append(If(value IsNot Nothing, value.ToString, "{Nothing}"), ColorPalette.ColorIdentifier)
        Return builder.ToFormattedText
    End Function

    Private Shared Function _FormatDiagnostics(value As ErrorList) As FormattedText
        Dim builder As New FormattedTextBuilder
        For Each errorObj As ErrorObject In value
            builder.Append(errorObj.Message.ToString & vbNewLine, ColorPalette.ColorSyntaxError)
            Dim pos As CodeAnalysis.Text.Position = errorObj.Content.GetStartPosition
            Dim line As String = errorObj.Content.Source.Item(pos.LineNumber).Content.ToString
            Dim prefix As String = "> " & line.Substring(0, errorObj.Content.Start)
            Dim content As String = errorObj.Content.ToString
            Dim suffix As String = line.Substring(errorObj.Content.End)
            builder.Append(prefix, ColorPalette.ColorOperator)
            builder.Append(content, ColorPalette.ColorSyntaxError)
            builder.Append(suffix & vbNewLine, ColorPalette.ColorOperator)
        Next
        Return builder.ToFormattedText
    End Function

End Class

