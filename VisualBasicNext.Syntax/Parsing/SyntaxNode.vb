Imports VisualBasicNext.CodeAnalysis.Lexing
Imports VisualBasicNext.CodeAnalysis.Text

Namespace Parsing

    Public MustInherit Class SyntaxNode

        Public MustOverride ReadOnly Property Children As IEnumerable(Of SyntaxNode)

        Public Overridable ReadOnly Property Span As Span
            Get
                Return Span.FromBounds(Me.Children.First.Span, Me.Children.Last.Span)
            End Get
        End Property

        Public ReadOnly Property TextPosition As Position
            Get
                Return Me.Span.GetStartPosition
            End Get
        End Property

        Public ReadOnly Property Kind As SyntaxKind

        Protected Sub New(kind As SyntaxKind)
            Me.Kind = kind
        End Sub

        Public ReadOnly Property IsToken() As Boolean
            Get
                Return Me.Kind.ToString.EndsWith("Token")
            End Get
        End Property

        Public ReadOnly Property IsKeywordToken() As Boolean
            Get
                Return Me.Kind.ToString.EndsWith("KeywordToken")
            End Get
        End Property

        Public Sub WriteTo(writer As IO.TextWriter)
            WriteTo(writer, Me, "")
        End Sub

        Private Shared Sub WriteTo(writer As IO.TextWriter, node As SyntaxNode, indent As String, Optional islast As Boolean = True)
            Dim is_console As Boolean = writer.Equals(Console.Out)
            Dim color As ConsoleColor = Console.ForegroundColor
            If Not node Is Nothing Then
                Dim tokenmarker As String = If(islast, "└──", "├──")
                If is_console Then Console.ForegroundColor = ConsoleColor.Gray
                writer.Write(indent & tokenmarker)
                If is_console Then Console.ForegroundColor = ConsoleColor.Magenta
                writer.Write(node.Kind.ToString)
                If node.IsToken Then
                    writer.Write(" (")
                    If Not DirectCast(node, SyntaxToken).Value Is Nothing Then
                        If is_console Then Console.ForegroundColor = ConsoleColor.White
                        writer.Write($"<{DirectCast(node, SyntaxToken).Value.GetType.Name}> {DirectCast(node, SyntaxToken).Value.ToString}")
                    End If
                    If is_console Then Console.ForegroundColor = ConsoleColor.Magenta
                    writer.Write($")")
                End If
                writer.WriteLine()
                Dim lastchild As SyntaxNode = node.Children.LastOrDefault
                indent &= If(islast, "   ", "│  ")
                For Each child As SyntaxNode In node.Children
                    WriteTo(writer, child, indent, child.Equals(lastchild))
                Next
            End If
            If is_console Then Console.ForegroundColor = color
        End Sub

        Public Overrides Function ToString() As String
            Using writer As New IO.StringWriter
                Me.WriteTo(writer)
                Return writer.ToString
            End Using
        End Function

    End Class

End Namespace
