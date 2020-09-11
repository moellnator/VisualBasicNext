Imports VisualBasicNext.Parsing.CST

Namespace Parsing.Generator
    Public Class Match : Inherits Parser

        Private ReadOnly _token_type As Tokenizing.TokenTypes
        Private ReadOnly _content As String
        Private ReadOnly _regex As Text.RegularExpressions.Regex

        Public Sub New(type As Tokenizing.TokenTypes, Optional content As String = "")
            Me._token_type = type
            Me._content = content
            Me._regex = If(Me._content <> "", New Text.RegularExpressions.Regex("^" & content & "$"), Nothing)
        End Sub

        Protected Overrides Function _Parse(ByRef state As State) As Node
            Dim retval As Node = Nothing
            If state.MoveNext Then
                If state.Current.TokenType = Me._token_type Then
                    If Me._content = "" OrElse Me._regex.IsMatch(state.Current.Content) Then
                        retval = New Node(NodeTypes.Generic, {state.Current})
                    End If
                End If
            End If
            Return retval
        End Function

        Public Overrides Function ToString() As String
            Return Me._token_type.ToString & If(Me._content <> "", "(='" & Me._content & "')", "")
        End Function

    End Class

End Namespace
