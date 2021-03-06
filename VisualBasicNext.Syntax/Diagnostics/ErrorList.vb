﻿Imports System.Reflection
Imports VisualBasicNext.CodeAnalysis.Lexing
Imports VisualBasicNext.CodeAnalysis.Parsing
Imports VisualBasicNext.CodeAnalysis.Text

Namespace Diagnostics
    Public Class ErrorList : Implements IReadOnlyList(Of ErrorObject)

        Private ReadOnly _content As List(Of ErrorObject)

        Public Sub New()
            Me._content = New List(Of ErrorObject)
        End Sub

        Public Sub New(previous As ErrorList)
            Me.New()
            Me._content.AddRange(previous)
        End Sub

        Public ReadOnly Property HasErrors As Boolean
            Get
                Return Me.Any()
            End Get
        End Property

        Public Sub Append(errorList As ErrorList)
            Me._content.AddRange(errorList._content)
        End Sub

        Friend Sub ReportBadCharakter(input As Char, span As Text.Span)
            Me.ReportMessage($"Invalid character '{input}'", span)
        End Sub

        Friend Sub ReportBadConversion(fromType As Type, toType As Type, span As Text.Span)
            Me.ReportMessage($"Invalid conversion from type ({fromType.ToString}) to ({toType.ToString})", span)
        End Sub

        Friend Sub ReportRuntimeException(innerException As Exception, syntax As Span)
            Me.ReportMessage($"A runtime exception of type <{innerException.GetType.Name}> orruced: {innerException.Message}. The error occured", syntax)
        End Sub

        Friend Sub ReportBadLiteral(literal As String, target As Type, span As Text.Span)
            Me.ReportMessage($"Literal '{literal}' does not represent a value of type ({target})", span)
        End Sub

        Friend Sub ReportMissing(text As String, span As Text.Span)
            Me.ReportMessage($"Expected missing '{text}'", span)
        End Sub

        Friend Sub ReportUnexpectedToken(expected As SyntaxKind, actual As SyntaxKind, span As Text.Span)
            Me.ReportMessage($"Expected <{expected.ToString}> instead of <{actual.ToString}>", span)
        End Sub

        Friend Sub ReportUndefinedType(typename As TypeNameNode)
            Me.ReportMessage($"Undefined or inaccessible type <{typename.Span.ToString()}>", typename.Span)
        End Sub

        Friend Sub ReportUndefinedType(typename As String, span As Span)
            Me.ReportMessage($"Undefined or inaccessible type <{typename}>", span)
        End Sub

        Friend Sub ReportTypeNotGeneric(type As Type, syntax As SyntaxNode)
            Me.ReportMessage($"Type <type.Name> is not a generic type", syntax.Span)
        End Sub

        Friend Sub ReportGenericArgumentMissmatch(type As Type, generics As List(Of Type), syntax As SyntaxNode)
            Me.ReportMessage(
                $"No matching generic type definition found of <{type.Name}> with generic arguments ({_GenericsToString(generics)})",
                syntax.Span
            )
        End Sub

        Friend Sub ReportGenericArgumentMissmatch(method As String, generics As List(Of Type), span As Span)
            Me.ReportMessage(
                $"No matching generic method definition found of <{method}> with generic arguments ({_GenericsToString(generics)})",
                span
            )
        End Sub

        Private Shared Function _GenericsToString(generics As List(Of Type)) As String
            If Not generics Is Nothing Then Return String.Join(","c, generics.Where(Function(g) g IsNot Nothing).Select(Function(g) g.Name).ToArray)
            Return ""
        End Function

        Friend Sub ReportVariableAlreadyDefined(name As String, statement As SyntaxToken)
            Me.ReportMessage($"Variable '{name}' has already been defines in the current scope ", statement.Span)
        End Sub

        Friend Sub ReportInvalidConversion(fromType As Type, toType As Type, expression As Text.Span)
            Me.ReportMessage($"No valid conversion from <{fromType.ToString}> to type <{toType.ToString}> found ", expression)
        End Sub

        Friend Sub ReportVariableNotDeclared(syntax As SyntaxToken)
            Me.ReportMessage($"Variable '{syntax.Span.ToString} is not declared in current scope'", syntax.Span)
        End Sub

        Private Sub ReportMessage(message As String, span As Text.Span)
            Me._content.Add(New ErrorObject(message & $" in source at {span.GetStartPosition.ToString}.", span))
        End Sub

        Friend Sub ReportBadNamespace(name As String, span As Span)
            Me.ReportMessage($"Namespace '{name}' not found in current app domain", span)
        End Sub

        Friend Sub ReportOperatorNotDefined(kind As SyntaxKind, boundType As Type, span As Span)
            Me.ReportMessage($"Operator <{kind.ToString}> not defined on type <{boundType.Name}>", span)
        End Sub

        Friend Sub ReportOperatorNotDefined(kind As SyntaxKind, leftType As Type, rigthType As Type, span As Span)
            Me.ReportMessage($"Operator <{kind.ToString}> not defined between type <{leftType.Name}> and <{rigthType.Name}>", span)
        End Sub

        Friend Sub ReportReferenceTypeCannotBeNullable(target As Type, syntax As TypeNameNode)
            Me.ReportMessage($"Type <{target.Name}> must be value type in order to be defined nullable.", syntax.Span)
        End Sub

        Friend Sub ReportMismatchingDimensions(actual As Integer, expected As Integer, span As Span)
            Me.ReportMessage($"Mismatching array dimension {actual.ToString}, expected {If(expected = 0, "'any'", expected.ToString)}", span)
        End Sub

        Friend Sub ReportMemberCannotBeGeneric(name As String, span As Span)
            Me.ReportMessage($"Member '{name}' cannot have generic arguments", span)
        End Sub

        Friend Sub ReportDoesNotAcceptArguments(span As Span)
            Me.ReportMessage($"Expression does not accept any arguments", span)
        End Sub

        Friend Sub ReportInvalidArguments(name As String, type As Type, span As Span)
            Me.ReportMessage($"No member '{name}' found in <{type.Name}> with given arguments", span)
        End Sub

        Friend Sub ReportDoesNotProduceAValue(member As MemberInfo, span As Span)
            Me.ReportMessage($"Method call to member '{member.Name}' does not produce a value", span)
        End Sub

        Friend Sub ReportMemberNotFound(member As String, type As Type, span As Span)
            Me.ReportMessage($"Member '{member}'´not found in <{type.Name}>", span)
        End Sub

        Friend Sub ReportMemberTypeNotValid(memberType As MemberTypes, span As Span)
            Me.ReportMessage($"Member type '{memberType.ToString}' not valid in expression", span)
        End Sub

        Friend Sub ReportStaticMemberAccessExpected(expression As MemberAccessListNode)
            Me.ReportMessage($"Static member access expected after type name.", expression.Span)
        End Sub

        Friend Sub ReportTypeIsNotCollection(type As Type, span As Span)
            Me.ReportMessage($"Type <{type.Name}> is not a collection type.", span)
        End Sub

        Default Public ReadOnly Property Item(index As Integer) As ErrorObject Implements IReadOnlyList(Of ErrorObject).Item
            Get
                Return Me._content.Item(index)
            End Get
        End Property

        Public ReadOnly Property Count As Integer Implements IReadOnlyCollection(Of ErrorObject).Count
            Get
                Return Me._content.Count
            End Get
        End Property

        Public Function GetEnumerator() As IEnumerator(Of ErrorObject) Implements IEnumerable(Of ErrorObject).GetEnumerator
            Return Me._content.GetEnumerator
        End Function

        Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Return Me.GetEnumerator
        End Function

        Public Sub Clear()
            Me._content.Clear()
        End Sub

        Public Shared Operator &(a As ErrorList, b As ErrorList) As ErrorList
            Dim retval As New ErrorList()
            retval._content.AddRange(a)
            retval._content.AddRange(b)
            Return retval
        End Operator

    End Class

End Namespace
