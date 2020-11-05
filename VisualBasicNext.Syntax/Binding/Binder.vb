Imports System.Collections.Immutable
Imports VisualBasicNext.Syntax.Diagnostics
Imports VisualBasicNext.Syntax.Parsing
Imports VisualBasicNext.Syntax.Symbols
Imports VisualBasicNext.TypeExtensions

Namespace Binding
    Public Class Binder

        Public ReadOnly Property Diagnostics As New ErrorList

        Private ReadOnly _scope As BoundScope

        Private Sub New(Optional parent As BoundScope = Nothing)
            Me._scope = New BoundScope(parent)
        End Sub

        Public Shared Function BindGlobalScope(script As ScriptNode, previous As BoundGlobalScope) As BoundGlobalScope
            Dim parent As BoundScope = CreateParentScope(previous)
            Dim binder As New Binder(parent)
            Dim statements As ImmutableArray(Of BoundStatement) = script.Statements.Select(
               Function(s) binder._BindStatement(s)
            ).Where(
                Function(s) s IsNot Nothing
            ).ToImmutableArray
            Return New BoundGlobalScope(
                script,
                previous,
                binder.Diagnostics,
                binder._scope.GetDeclaredVariables,
                statements,
                binder._scope.GetImports
            )
        End Function

        Private Shared Function CreateParentScope(previous As BoundGlobalScope) As BoundScope
            Dim root As BoundScope = New BoundScope(Nothing)
            Dim stack As New Stack(Of BoundGlobalScope)
            While previous IsNot Nothing
                stack.Push(previous)
                previous = previous.Previous
            End While
            While stack.Count > 0
                previous = stack.Pop
                For Each v As VariableSymbol In previous.Variables
                    root.TryDeclareVariable(v)
                Next
                For Each i As String In previous.Imports
                    root.Import(i)
                Next
            End While
            Return root
        End Function

        Public Shared Function BindScript(previous As BoundScript, scope As BoundGlobalScope) As BoundScript
            Dim diagnostics As New ErrorList
            Return New BoundScript(previous, scope.Syntax, scope.Statements, diagnostics)
        End Function

        Private Function _BindStatement(statement As StatementNode) As BoundStatement
            Select Case statement.Kind
                Case Lexing.SyntaxKind.EmptyStatementNode
                    Return Nothing
                Case Lexing.SyntaxKind.ExpressionStatementNode
                    Return Me._BindExpressionStatement(statement)
                Case Lexing.SyntaxKind.VariableDeclarationStatementNode
                    Return Me._BindVariableDeclarationStatement(statement)
                Case Lexing.SyntaxKind.ImportsStatementNode
                    Return Me._BindImportStatement(statement)
                Case Else
                    Throw New Exception($"Syntax node <{statement.Kind.ToString}> is not a statement.")
            End Select
        End Function

        Private Function _BindImportStatement(statement As ImportsStatementNode) As BoundImportStatement
            Dim name As String = statement.Identifier.Span.ToString
            If Not TypeResolver.IsValidNamespace(name) Then Me.Diagnostics.ReportBadNamespace(name, statement.Identifier.Span)
            Me._scope.Import(name)
            Return New BoundImportStatement(statement, name)
        End Function

        Private Function _BindVariableDeclarationStatement(statement As VariableDeclarationStatementNode) As BoundStatement
            Dim type As Type = Me._BindTypeClause(statement.Typename)
            Dim name As Lexing.SyntaxToken = statement.IdentifierToken
            If name.Span.Length = 0 Then Return Me._BindErrorStatement(statement)
            Dim initializer As BoundExpression = Nothing
            If statement.Expression IsNot Nothing Then
                initializer = Me._BindExpression(statement.Expression)
                Dim init_type As Type = initializer.BoundType
                If type Is Nothing Then
                    type = init_type
                Else
                    If Not init_type.IsImplicitlyCastableTo(type) Then
                        Me.Diagnostics.ReportInvalidConversion(init_type, type, statement.Expression.Span)
                    End If
                End If
            End If
            If type Is Nothing Then type = GetType(Object)
            Dim symbol As Symbol = Me._BindVariableDeclaration(name, type)
            Return New BoundVariableDeclarationStatement(statement, symbol, initializer)
        End Function

        Private Function _BindErrorStatement(statement As SyntaxNode) As BoundStatement
            Return New BoundExpressionStatement(statement, New BoundErrorExpression(statement))
        End Function

        Private Function _BindTypeClause(typename As TypeNameNode) As Type
            If typename Is Nothing Then Return Nothing
            Dim resolver As New TypeResolver(typename, Me._scope.GetImports)
            Dim retval As Type = resolver.ResolveType
            If Not resolver.Diagnostics.Any Then
                Return retval
            Else
                Me.Diagnostics.Append(resolver.Diagnostics)
                Return Nothing
            End If
        End Function

        Private Function _BindVariableDeclaration(name As Lexing.SyntaxToken, type As Type) As Symbol
            Dim varname As String = name.Span.ToString
            'TODO : check if in lambda -> bind local variable
            Dim symbol As New GlobalVariableSymbol(varname, type)
            If Not Me._scope.TryDeclareVariable(symbol) Then Me.Diagnostics.ReportVariableAlreadyDefined(varname, name)
            Return symbol
        End Function

        Private Function _BindExpressionStatement(statement As ExpressionStatementNode) As BoundExpressionStatement
            Return New BoundExpressionStatement(statement, Me._BindExpression(statement.Expression))
        End Function

        Private Function _BindExpression(expression As ExpressionNode) As BoundExpression
            Select Case expression.Kind
                Case Lexing.SyntaxKind.BlockExpressionNode
                    Return Me._BindBlockExpression(expression)
                Case Lexing.SyntaxKind.LiteralNode
                    Return Me._BindLiteralExpression(expression)
                Case Lexing.SyntaxKind.VariableExpressionNode
                    Return Me._BindVariableExpression(expression)
                Case Lexing.SyntaxKind.CastExpressionNode
                    Return Me._BindCastExpression(expression)
                Case Lexing.SyntaxKind.CastDynamicExpressionNode
                    Return Me._BindCastDynamicExpression(expression)
                Case Lexing.SyntaxKind.GetTypeExpressionNode
                    Return Me._BindGetTypeExpression(expression)
                Case Else
                    Throw New Exception($"Syntax node <{expression.Kind.ToString}> is not an expression.")
            End Select
        End Function

        Private Function _BindExpression(Of T)(expression As ExpressionNode) As BoundExpression
            Dim retval As BoundExpression = Me._BindExpression(expression)
            If Not retval.BoundType.IsCastableTo(GetType(T)) Then
                Me.Diagnostics.ReportInvalidConversion(retval.BoundType, GetType(T), expression.Span)
                Return New BoundErrorExpression(expression)
            End If
            Return retval
        End Function

        Private Function _BindGetTypeExpression(expression As GetTypeExpressionNode) As BoundExpression
            Dim type As Type = Me._BindTypeClause(expression.TypeName)
            Return New BoundGetTypeExpression(expression, type)
        End Function

        Private Function _BindCastExpression(expression As CastExpressionNode) As BoundExpression
            Dim type As Type = Me._BindTypeClause(expression.Typename)
            Dim value As BoundExpression = Me._BindExpression(expression.Expression)
            If Not value.BoundType.IsCastableTo(type) Then
                Me.Diagnostics.ReportInvalidConversion(value.BoundType, type, expression.Typename.Span)
                Return New BoundErrorExpression(expression)
            End If
            Return New BoundCastExpression(expression, value, type)
        End Function

        Private Function _BindCastDynamicExpression(expression As CastDynamicExpressionNode) As BoundExpression
            Dim type As BoundExpression = Me._BindExpression(Of Type)(expression.TypeNode)
            Dim value As BoundExpression = Me._BindExpression(expression.Expression)
            Return New BoundCastDynamicExpression(expression, value, type)
        End Function

        Private Function _BindVariableExpression(expression As VariableExpressionNode) As BoundExpression
            Dim variable As VariableSymbol = Me._BindVariableReference(expression.Identifier)
            If variable Is Nothing Then Return New BoundErrorExpression(expression)
            Return New BoundVariableExpression(expression, variable)
        End Function

        Private Function _BindVariableReference(syntax As Lexing.SyntaxToken) As VariableSymbol
            Dim name As String = syntax.Span.ToString
            Dim symbol As Symbol = Me._scope.TryLookupSymbol(name)
            If symbol Is Nothing Then
                Me.Diagnostics.ReportVariableNotDeclared(syntax)
                Return Nothing
            Else
                Return symbol
            End If
        End Function

        Private Function _BindBlockExpression(blockExpression As BlockExpressionNode) As BoundExpression
            Return Me._BindExpression(blockExpression.Expression)
        End Function

        Private Function _BindLiteralExpression(literalExpression As LiteralExpressionNode) As BoundLiteralExpression
            Return New BoundLiteralExpression(literalExpression, literalExpression.LiteralToken.Value)
        End Function

    End Class

End Namespace
