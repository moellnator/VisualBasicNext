Imports System.Collections.Immutable
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports VisualBasicNext.CodeAnalysis.Diagnostics
Imports VisualBasicNext.CodeAnalysis.Parsing
Imports VisualBasicNext.CodeAnalysis.Symbols
Imports VisualBasicNext.TypeExtensions

Namespace Binding
    Friend Class Binder

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

        Private Function _BindImportStatement(statement As ImportsStatementNode) As BoundStatement
            Dim name As String = statement.Identifier.Span.ToString
            If Not TypeResolver.IsValidNamespace(name) Then
                Me.Diagnostics.ReportBadNamespace(name, statement.Identifier.Span)
                Return Me._BindErrorStatement(statement)
            End If
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
                    If Not CanCast(init_type, type) Then
                        Me.Diagnostics.ReportInvalidConversion(init_type, type, statement.Expression.Span)
                        Return Me._BindErrorStatement(statement)
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
            'TODO [implementation] -> check if in lambda -> bind local variable
            If varname <> String.Empty And type IsNot Nothing Then
                Dim symbol As New GlobalVariableSymbol(varname, type)
                If Not Me._scope.TryDeclareVariable(symbol) Then
                    Me.Diagnostics.ReportVariableAlreadyDefined(varname, name)
                    Return Nothing
                End If
                Return symbol
            End If
            Return Nothing
        End Function

        Private Function _BindExpressionStatement(statement As ExpressionStatementNode) As BoundExpressionStatement
            Return New BoundExpressionStatement(statement, Me._BindExpression(statement.Expression))
        End Function

        Private Function _BindExpression(expression As ExpressionNode) As BoundExpression
            Select Case expression.Kind
                Case Lexing.SyntaxKind.MemberAccessListNode
                    Return Me._BindMemberAccessExpression(expression)
                Case Lexing.SyntaxKind.MemberAccessItemNode
                    Return Me._BindMemberAccessItemExpression(expression)
                Case Lexing.SyntaxKind.BlockExpressionNode
                    Return Me._BindBlockExpression(expression)
                Case Lexing.SyntaxKind.LiteralNode
                    Return Me._BindLiteralExpression(expression)
                Case Lexing.SyntaxKind.CastExpressionNode
                    Return Me._BindCastExpression(expression)
                Case Lexing.SyntaxKind.CastDynamicExpressionNode
                    Return Me._BindCastDynamicExpression(expression)
                Case Lexing.SyntaxKind.GetTypeExpressionNode
                    Return Me._BindGetTypeExpression(expression)
                Case Lexing.SyntaxKind.TernaryExpressionNode
                    Return Me._BindTernaryExpression(expression)
                Case Lexing.SyntaxKind.NullCheckExpression
                    Return Me._BindNullCheckExpression(expression)
                Case Lexing.SyntaxKind.ArrayExpressionNode
                    Return Me._BindArrayExpression(expression)
                Case Lexing.SyntaxKind.UnaryExpressionNode
                    Return Me._BindUnaryExpression(expression)
                Case Lexing.SyntaxKind.BinaryExpressionNode
                    Return Me._BindBinaryExpression(expression)
                Case Lexing.SyntaxKind.ExtrapolatedStringExpressionNode
                    Return Me._BindExtrapolatedStringExpression(expression)
                Case Lexing.SyntaxKind.TryCastExpressionNode
                    Return Me._BindTryCastExpression(expression)
                Case Else
                    Throw New Exception($"Syntax node <{expression.Kind.ToString}> is not an expression.")
            End Select
        End Function

        Friend Shared Function CanCast(fromType As Type, toType As Type) As Boolean
            If fromType.IsCastableTo(toType) Then Return True
            Return False
        End Function

        Private Function _BindExpression(Of T)(expression As ExpressionNode) As BoundExpression
            Dim retval As BoundExpression = Me._BindExpression(expression)
            If Not CanCast(retval.BoundType, GetType(T)) Then
                Me.Diagnostics.ReportInvalidConversion(retval.BoundType, GetType(T), expression.Span)
                Return New BoundErrorExpression(expression)
            End If
            Return retval
        End Function

        Private Function _BindMemberAccessExpression(expression As MemberAccessListNode) As BoundExpression
            Dim source As BoundExpression = Nothing
            Dim source_type As Type = Nothing
            If expression.Source.Kind = Lexing.SyntaxKind.MemberAccessItemNode Then
                If Me._scope.TryLookupSymbol(DirectCast(expression.Source, MemberAccessItemNode).Identifier.Span.ToString) IsNot Nothing Then
                    source = Me._BindMemberAccessItemExpression(expression.Source)
                Else
                    'TODO [implementation] -> member items until is type + while nested type (only generics, no access allowed) -> bind first member static -> source
                End If
            Else
                source = Me._BindExpression(expression.Source)
                source_type = source.BoundType
            End If
            For Each memberaccess As MemberAccessItemNode In expression.Items
                If memberaccess.Identifier.Span.ToString.Equals(String.Empty) Then Return New BoundErrorExpression(expression)
                source = Me._BindMemberAccessItemExpression(memberaccess, source)
            Next
            Return source
        End Function

        Private Function _BindMemberAccessItemExpression(expression As MemberAccessItemNode, Optional source As BoundExpression = Nothing) As BoundExpression
            If expression.Delimeter Is Nothing Then
                If source IsNot Nothing Then Throw New ArgumentException("First member item cannot have a source type.")
                Return Me._BindVariableAccess(expression)
            Else
                If source Is Nothing Then Throw New ArgumentException("Source type cannot be nothing for member access.")
                Dim retval As BoundExpression = Nothing
                Dim access_offset As Integer = 0
                Dim access_count As Integer = If(expression.Access IsNot Nothing, expression.Access.Items.Count, 0)
                If source.BoundType.Equals(GetType(Object)) Then
                    'TODO [implementation] -> Object late binding via 'Microsoft.VisualBasic.CompilerServices.NewLateBinding::LateCall'/'LateGet'/'LateIndexGet'/...
                Else
                    Dim name As String = expression.Identifier.Span.ToString
                    Dim members As MemberInfo() = source.BoundType.GetMembers(
                        BindingFlags.Public Or BindingFlags.Instance
                    ).Concat(TypeResolver.GetExtensions(source.BoundType)).ToArray
                    members = members.Where(Function(m) m.Name.ToLower.Equals(name.ToLower)).ToArray
                    If members.Count = 0 Then
                        Me.Diagnostics.ReportMemberNotFound(name, source.BoundType, expression.Identifier.Span)
                        Return New BoundErrorExpression(expression)
                    End If
                    Select Case members.First.MemberType
                        Case MemberTypes.Field
                            retval = New BoundInstanceFieldGetExpression(expression, source, members.First)
                        Case MemberTypes.Property, MemberTypes.Method
                            'TODO [implementation] -> Make member generic with arguments or inferred types...
                            Dim member As MemberInfo = Nothing
                            Dim arguments As BoundExpression() = Array.Empty(Of BoundExpression)
                            If access_count = 0 Then
                                member = members.FirstOrDefault(Function(m) _GetParameters(m).Count = 0)
                                If member Is Nothing Then member = _FindMatchingExtension(members, source, arguments)
                            Else
                                arguments = expression.Access.Items(access_offset).Items.Select(Function(arg) Me._BindExpression(arg.Argument)).ToArray
                                member = _FindMatchingMember(members, arguments)
                                If member Is Nothing Then member = _FindMatchingExtension(members, source, arguments)
                                access_offset += 1
                            End If
                            If member Is Nothing Then

                                Me.Diagnostics.ReportInvalidArguments(name, source.BoundType, expression.Span)
                                Return New BoundErrorExpression(expression)
                            End If
                            If member.MemberType = MemberTypes.Property Then
                                retval = New BoundInstancePropertyGetExpression(expression, source, member, arguments)
                            ElseIf DirectCast(member, MethodInfo).IsStatic Then
                                Stop
                            Else
                                retval = New BoundInstanceMethodInvokationExpression(expression, source, member, arguments)
                            End If
                        Case Else
                            Me.Diagnostics.ReportMemberTypeNotValid(members.First.MemberType, expression.Identifier.Span)
                            Return New BoundErrorExpression(expression)
                    End Select
                End If
                For index As Integer = access_offset To access_count - 1
                    retval = _ResolveAccess(expression, retval, index)
                    If retval.Kind.Equals(BoundNodeKind.BoundErrorExpression) Then Return retval
                Next
                Return retval
            End If
        End Function

        Private Function _BindVariableAccess(expression As MemberAccessItemNode) As BoundExpression
            Dim name As String = expression.Identifier.Span.ToString
            Dim symbol As VariableSymbol = Me._scope.TryLookupSymbol(name)
            If symbol IsNot Nothing Then
                If expression.Generics IsNot Nothing Then Me.Diagnostics.ReportVariableCannotBeGeneric(symbol.Name, expression.Generics.Span)
                Dim retval As BoundExpression = New BoundVariableExpression(expression, symbol)
                If expression.Access IsNot Nothing Then
                    For index As Integer = 0 To expression.Access.Items.Count - 1
                        retval = _ResolveAccess(expression, retval, index)
                        If retval.Kind.Equals(BoundNodeKind.BoundErrorExpression) Then Return retval
                    Next
                End If
                Return retval
            Else
                Me.Diagnostics.ReportVariableNotDeclared(expression.Identifier)
                Return New BoundErrorExpression(expression)
            End If
        End Function

        Private Function _ResolveAccess(expression As MemberAccessItemNode, source As BoundExpression, index As Integer) As BoundExpression
            If source.BoundType.Equals(GetType(Object)) Then
                'TODO [implementation] -> Return late bound array indexing
            ElseIf source.BoundType.IsArray Then
                Dim arguments As BoundExpression() = expression.Access.Items(index).Items.Select(Function(arg) Me._BindExpression(Of Integer)(arg.Argument)).ToArray
                Dim elementType As Type = source.BoundType.GetElementType
                Return New BoundArrayAccessExpression(expression.Access.Items(index), source, arguments, elementType)
            ElseIf GetType(MulticastDelegate).IsAssignableFrom(source.BoundType) Then
                Dim members As MemberInfo() = source.BoundType.GetMembers(BindingFlags.Public And BindingFlags.Instance).Where(Function(m) m.Name = "Invoke")
                Dim arguments As BoundExpression() = expression.Access.Items(index).Items.Select(Function(arg) Me._BindExpression(arg.Argument)).ToArray
                Dim member As MemberInfo = _FindMatchingMember(members, arguments)
                If member Is Nothing Then
                    Me.Diagnostics.ReportInvalidArguments("Invoke", source.BoundType, expression.Access.Items(index).Span)
                    Return New BoundErrorExpression(expression)
                End If
                Return New BoundInstanceMethodInvokationExpression(expression.Access.Items(index), source, member, arguments)
            ElseIf source.BoundType.GetDefaultMembers.Any Then
                Dim arguments As BoundExpression() = expression.Access.Items(index).Items.Select(Function(arg) Me._BindExpression(arg.Argument)).ToArray
                Dim members As MemberInfo() = source.BoundType.GetDefaultMembers
                Dim member As MemberInfo = _FindMatchingMember(members, arguments)
                If member IsNot Nothing Then
                    Select Case member.MemberType
                        Case MemberTypes.Property
                            Return New BoundInstancePropertyGetExpression(expression.Access.Items(index), source, member, arguments)
                        Case MemberTypes.Method
                            If DirectCast(member, MethodInfo).ReturnType.Equals(GetType(Void)) Then _
                                Me.Diagnostics.ReportDoesNotProduceAValue(member, expression.Access.Items(index).Span)
                            Return New BoundInstanceMethodInvokationExpression(expression.Access.Items(index), source, member, arguments)
                    End Select
                End If
            End If
            Me.Diagnostics.ReportDoesNotAcceptArguments(expression.Access.Span)
            Return New BoundErrorExpression(expression)
        End Function

        Private Function _FindMatchingExtension(members As IEnumerable(Of MemberInfo), obj As BoundExpression, arguments As IEnumerable(Of BoundExpression)) As MemberInfo
            Dim extensions As MethodInfo() = members.Where(
                Function(m) m.MemberType = MemberTypes.Method AndAlso m.IsDefined(GetType(ExtensionAttribute), False)
            ).Cast(Of MethodInfo).ToArray
            Return _FindMatchingMember(extensions, {obj}.Concat(arguments))
        End Function

        Private Function _FindMatchingMember(members As IEnumerable(Of MemberInfo), arguments As IEnumerable(Of BoundExpression)) As MemberInfo
            Dim retval As MemberInfo = Nothing
            If Not members.Any Then Return Nothing
            retval = members.FirstOrDefault(Function(m) _MatchesArgumentsList(m, arguments))
            If retval Is Nothing Then retval = members.FirstOrDefault(Function(m) _CanMatchArgumentsList(m, arguments))
            Return retval
        End Function

        Private Shared Function _MatchesArgumentsList(member As MemberInfo, arguments As IEnumerable(Of BoundExpression)) As Boolean
            Dim parameters As ParameterInfo() = _GetParameters(member)
            If parameters Is Nothing OrElse parameters.Count <> arguments.Count Then Return False
            Return parameters.Select(Function(p) p.ParameterType).SequenceEqual(arguments.Select(Function(a) a.BoundType))
        End Function

        Private Shared Function _CanMatchArgumentsList(member As MemberInfo, arguments As IEnumerable(Of BoundExpression)) As Boolean
            Dim parameters As ParameterInfo() = _GetParameters(member)
            If parameters Is Nothing OrElse parameters.Count <> arguments.Count Then Return False
            Return parameters.Select(Function(p) p.ParameterType).Zip(
                arguments.Select(Function(a) a.BoundType),
                Function(p, a) a.IsCastableTo(p)
            ).All(Function(b) b)
        End Function

        Private Shared Function _GetParameters(member As MemberInfo) As ParameterInfo()
            Select Case member.MemberType
                Case MemberTypes.Method
                    Return DirectCast(member, MethodInfo).GetParameters
                Case MemberTypes.Property
                    Return DirectCast(member, PropertyInfo).GetIndexParameters
                Case Else
                    Return Nothing
            End Select
        End Function

        Private Function _BindTryCastExpression(expression As TryCastExpressionNode) As BoundTryCastExpression
            Dim expr As BoundExpression = Me._BindExpression(expression.Expression)
            Dim target As Type = Me._BindTypeClause(expression.Target)
            Return New BoundTryCastExpression(expression, expr, target)
        End Function

        Private Function _BindExtrapolatedStringExpression(expression As ExtrapolatedStringExpressionNode) As BoundExtrapolatedStringExpression
            Dim start As String = expression.PartialStringStart.Value
            Dim expressions As BoundExpression() = expression.Subnodes.Select(Function(e) Me._BindExpression(Of String)(e.Expression)).ToArray
            Dim remainders As String() = expression.Subnodes.Select(Function(e) CStr(e.TerminatorSyntax.Value)).ToArray
            Return New BoundExtrapolatedStringExpression(expression, start, expressions, remainders)
        End Function

        Private Function _BindArrayExpression(expression As ArrayExpressionNode) As BoundArrayExpression
            Dim items As ImmutableArray(Of BoundExpression) = expression.Items.Select(Function(e) Me._BindExpression(e.Expression)).ToImmutableArray
            Dim all_array As Boolean = items.Count > 0 AndAlso expression.Items.All(Function(e) e.Expression.Kind = Lexing.SyntaxKind.ArrayExpressionNode)
            Dim common_type As Type = GetType(Object)
            If items.Count > 0 AndAlso items.All(Function(c) c.BoundType.Equals(items.First.BoundType)) Then common_type = items.First.BoundType
            If all_array Then
                Dim common_count As Integer = DirectCast(expression.Items.First.Expression, ArrayExpressionNode).Items.Count
                If expression.Items.All(Function(e) DirectCast(e.Expression, ArrayExpressionNode).Items.Count = common_count) And common_type.IsArray Then
                    Dim rank As Integer = common_type.GetArrayRank + 1
                    Return New BoundArrayExpression(expression, items, common_type.GetElementType.MakeArrayType(rank), rank)
                Else
                    Return New BoundArrayExpression(expression, items, common_type.MakeArrayType)
                End If
            Else
                Return New BoundArrayExpression(expression, items, common_type.MakeArrayType)
            End If
        End Function

        Private Function _BindBinaryExpression(expression As BinaryExpressionNode) As BoundExpression
            Dim left As BoundExpression = Me._BindExpression(expression.Left)
            Dim right As BoundExpression = Me._BindExpression(expression.Right)
            Dim op As BoundBinaryOperator = BoundBinaryOperator.Bind(expression.OperatorToken, left, right)
            If op IsNot Nothing Then
                Return New BoundBinaryExpression(expression, left, op, right)
            Else
                Me.Diagnostics.ReportOperatorNotDefined(expression.OperatorToken.Kind, left.BoundType, right.BoundType, expression.OperatorToken.Span)
                Return New BoundErrorExpression(expression)
            End If
        End Function

        Private Function _BindUnaryExpression(expression As UnaryExpressionNode) As BoundExpression
            Dim operand As BoundExpression = Me._BindExpression(expression.Right)
            Dim op As BoundUnaryOperator = BoundUnaryOperator.Bind(expression.OperatorToken, operand)
            If op IsNot Nothing Then
                Return New BoundUnaryExpression(expression, op, operand)
            Else
                Me.Diagnostics.ReportOperatorNotDefined(expression.OperatorToken.Kind, operand.BoundType, expression.OperatorToken.Span)
                Return New BoundErrorExpression(expression)
            End If
        End Function

        Private Function _BindTernaryExpression(expression As TernaryExpressionNode) As BoundTernaryExpression
            Dim condition As BoundExpression = Me._BindExpression(Of Boolean)(expression.Condition)
            Dim trueExpression As BoundExpression = Me._BindExpression(expression.TrueExpression)
            Dim falseExpression As BoundExpression = Me._BindExpression(expression.FalseExpression)
            Dim boundType As Type = GetType(Object)
            If trueExpression.BoundType.Equals(falseExpression.BoundType) Then boundType = trueExpression.BoundType
            Return New BoundTernaryExpression(expression, condition, trueExpression, falseExpression, boundType)
        End Function

        Private Function _BindNullCheckExpression(expression As NullCheckExpressionNode) As BoundExpression
            Dim expressionFirst As BoundExpression = Me._BindExpression(expression.Expression)
            Dim expressionFallBack As BoundExpression = Me._BindExpression(expression.FallbackExpression)
            If Not CanCast(expressionFallBack.BoundType, expressionFirst.BoundType) Then
                Me.Diagnostics.ReportInvalidConversion(expressionFallBack.BoundType, expressionFirst.BoundType, expression.FallbackExpression.Span)
                Return New BoundErrorExpression(expression)
            End If
            Return New BoundNullCheckExpression(expression, expressionFirst, expressionFallBack, expressionFirst.BoundType)
        End Function

        Private Function _BindGetTypeExpression(expression As GetTypeExpressionNode) As BoundExpression
            Dim type As Type = Me._BindTypeClause(expression.TypeName)
            Return New BoundGetTypeExpression(expression, type)
        End Function

        Private Function _BindCastExpression(expression As CastExpressionNode) As BoundExpression
            Dim type As Type = Me._BindTypeClause(expression.Typename)
            If type Is Nothing Then Return New BoundErrorExpression(expression)
            Dim value As BoundExpression = Me._BindExpression(expression.Expression)
            If Not CanCast(value.BoundType, type) Then
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
