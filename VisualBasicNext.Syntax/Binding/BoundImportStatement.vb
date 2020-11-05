Imports VisualBasicNext.Syntax.Parsing

Namespace Binding
    Public Class BoundImportStatement : Inherits BoundStatement

        Public Sub New(syntax As ImportsStatementNode, name As String)
            MyBase.New(syntax, BoundNodeKinds.BoundImportStatement)
            Me.Name = name
        End Sub

        Public ReadOnly Property Name As String

    End Class

End Namespace
