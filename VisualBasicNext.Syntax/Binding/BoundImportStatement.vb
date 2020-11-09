Imports VisualBasicNext.CodeAnalysis.Parsing

Namespace Binding
    Friend Class BoundImportStatement : Inherits BoundStatement

        Public Sub New(syntax As ImportsStatementNode, name As String)
            MyBase.New(syntax, BoundNodeKind.BoundImportStatement)
            Me.Name = name
        End Sub

        Public ReadOnly Property Name As String

    End Class

End Namespace
