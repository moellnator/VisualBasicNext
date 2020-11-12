Imports VisualBasicNext.CodeAnalysis.Lexing
Imports VisualBasicNext.CodeAnalysis.Text

Namespace Parsing

    Public MustInherit Class SyntaxNode : Implements IEquatable(Of SyntaxNode)

        Public MustOverride ReadOnly Property Children As IEnumerable(Of SyntaxNode)
        Private _parents As Dictionary(Of SyntaxNode, SyntaxNode)

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

        Public Function GetParent(node As SyntaxNode) As SyntaxNode
            Dim retval As SyntaxNode = Nothing
            If Me._parents Is Nothing Then
                Me._parents = _CreateParentDictionary(Me)
            End If
            Me._parents.TryGetValue(node, retval)
            Return retval
        End Function

        Private Shared Function _CreateParentDictionary(root As SyntaxNode) As Dictionary(Of SyntaxNode, SyntaxNode)
            Dim retval As New Dictionary(Of SyntaxNode, SyntaxNode) From {
                {root, Nothing}
            }
            _CreateParentDictionary(retval, root)
            Return retval
        End Function

        Private Shared Sub _CreateParentDictionary(result As Dictionary(Of SyntaxNode, SyntaxNode), root As SyntaxNode)
            For Each child As SyntaxNode In root.Children
                If Not result.ContainsKey(child) Then result.Add(child, root)
                _CreateParentDictionary(result, child)
            Next
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

        Public Overrides Function Equals(obj As Object) As Boolean
            Return If(TypeOf obj Is SyntaxNode, Me._IEquatable_Equals(obj), False)
        End Function

        Private Function _IEquatable_Equals(other As SyntaxNode) As Boolean Implements IEquatable(Of SyntaxNode).Equals
            Return Me.Span.Equals(other.Span) And Me.Kind.Equals(other.Kind)
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return New CombinedHashCode(Me.Span, Me.Kind).GetHashCode
        End Function

    End Class

End Namespace
