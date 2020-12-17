Imports System.Collections.Immutable
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports VisualBasicNext.CodeAnalysis.Diagnostics
Imports VisualBasicNext.CodeAnalysis.Parsing

Namespace Binding
    Public Class TypeResolver

        Private Shared ReadOnly ImplicitNumericConversions As New Dictionary(Of Type, List(Of Type))() From {
           {GetType(SByte), New List(Of Type) From {
                GetType(Short),
                GetType(Integer),
                GetType(Long),
                GetType(Single),
                GetType(Double),
                GetType(Decimal)
            }},
            {GetType(Byte), New List(Of Type) From {
                GetType(Short),
                GetType(UShort),
                GetType(Integer),
                GetType(UInteger),
                GetType(Long),
                GetType(ULong),
                GetType(Single),
                GetType(Double),
                GetType(Decimal)
            }},
            {GetType(Short), New List(Of Type) From {
                GetType(Integer),
                GetType(Long),
                GetType(Single),
                GetType(Double),
                GetType(Decimal)
            }},
            {GetType(UShort), New List(Of Type) From {
                GetType(Integer),
                GetType(UInteger),
                GetType(Long),
                GetType(ULong),
                GetType(Single),
                GetType(Double),
                GetType(Decimal)
            }},
            {GetType(Integer), New List(Of Type) From {
                GetType(Long),
                GetType(Single),
                GetType(Double),
                GetType(Decimal)
            }},
            {GetType(UInteger), New List(Of Type) From {
                GetType(Long),
                GetType(ULong),
                GetType(Single),
                GetType(Double),
                GetType(Decimal)
            }},
            {GetType(Long), New List(Of Type) From {
                GetType(Single),
                GetType(Double),
                GetType(Decimal)
            }},
            {GetType(Char), New List(Of Type) From {
                GetType(UShort),
                GetType(Integer),
                GetType(UInteger),
                GetType(Long),
                GetType(ULong),
                GetType(Single),
                GetType(Double),
                GetType(Decimal)
            }},
            {GetType(Single), New List(Of Type) From {
                GetType(Double)
            }},
            {GetType(ULong), New List(Of Type) From {
                GetType(Single),
                GetType(Double),
                GetType(Decimal)
            }}
        }

        Private Shared ReadOnly _DeclaredTypes As ImmutableDictionary(Of String, Type)
        Private Shared ReadOnly _Namespaces As ImmutableArray(Of String)
        Private Shared ReadOnly _Extensions As MethodInfo()

        Shared Sub New()
            Dim declared As New Dictionary(Of String, Type)
            Dim namespaces As New List(Of String)
            Dim assemblies As Assembly() = AppDomain.CurrentDomain.GetAssemblies
            For Each assembly As Assembly In assemblies
                For Each t As Type In assembly.GetTypes()
                    If t.Attributes.HasFlag(TypeAttributes.Public) And Not t.FullName.Contains("<") And Not t.IsNested Then
                        _add_partial_namespaces(t.Namespace, namespaces)
                        If declared.ContainsKey(t.FullName.ToLower) Then Stop
                        declared.Add(t.FullName.ToLower, t)
                        _add_nested_types(t, t.FullName.ToLower, declared)
                    End If
                Next
            Next
            _DeclaredTypes = declared.ToImmutableDictionary
            _Namespaces = namespaces.ToImmutableArray
            _Extensions = _GetAllExtensions()
        End Sub

        Private Shared Sub _add_partial_namespaces(namespaceName As String, ByRef output As List(Of String))
            If Not output.Contains(namespaceName) Then
                Dim parts As String() = If(namespaceName, "").Split("."c)
                For i = 1 To parts.Count
                    Dim partial_namespace As String = String.Join("."c, parts.Take(i).ToArray).ToLower
                    If Not output.Contains(partial_namespace) Then output.Add(partial_namespace)
                Next
            End If
        End Sub

        Private Shared Sub _add_nested_types(base As Type, key As String, ByRef output As Dictionary(Of String, Type))
            For Each nest As Type In base.GetNestedTypes(BindingFlags.Public)
                Dim nest_key As String = key & "." & nest.Name.ToLower
                output.Add(nest_key, nest)
                _add_nested_types(nest, nest_key, output)
            Next
        End Sub

        Private Shared Function _GetAllExtensions() As MethodInfo()
            Dim binding As BindingFlags = BindingFlags.Public Or BindingFlags.Static Or BindingFlags.NonPublic
            Dim assm As Assembly() = AppDomain.CurrentDomain.GetAssemblies
            Dim types As Type() = assm.SelectMany(Function(a) a.GetTypes.Where(Function(t) t.IsSealed And Not t.IsGenericType And Not t.IsNested)).ToArray
            Dim methods As MethodInfo() = types.SelectMany(Function(t) t.GetMethods(binding).Where(Function(m) m.IsDefined(GetType(ExtensionAttribute), False))).ToArray
            Return methods
        End Function

        Public Shared Function GetExtensions(t As Type) As MethodInfo()
            Return _Extensions.Where(Function(m) IsAssignableToGenericType(t, m.GetParameters()(0).ParameterType)).ToArray
        End Function

        Public Shared Function IsAssignableToGenericType(ByVal givenType As Type, ByVal genericType As Type) As Boolean
            If givenType = genericType Then Return True
            For Each it In givenType.GetInterfaces()
                If it.IsGenericType AndAlso it.GetGenericTypeDefinition().GUID = genericType.GUID Then Return True
            Next
            If givenType.IsGenericType AndAlso givenType.GetGenericTypeDefinition().GUID = genericType.GUID Then Return True
            Dim baseType As Type = givenType.BaseType
            If baseType Is Nothing Then Return False
            Return IsAssignableToGenericType(baseType, genericType)
        End Function

        Private Shared Function TryResolveInternal(name As String, ByRef returnType As Type) As Boolean
            Dim retval As Boolean = True
            Select Case name.ToLower
                Case "integer"
                    returnType = GetType(Integer)
                Case "uinteger"
                    returnType = GetType(UInteger)
                Case "short"
                    returnType = GetType(Short)
                Case "ushort"
                    returnType = GetType(UShort)
                Case "long"
                    returnType = GetType(Long)
                Case "ulong"
                    returnType = GetType(ULong)
                Case "byte"
                    returnType = GetType(Byte)
                Case "sbyte"
                    returnType = GetType(SByte)
                Case "decimal"
                    returnType = GetType(Decimal)
                Case "string"
                    returnType = GetType(String)
                Case "date"
                    returnType = GetType(Date)
                Case "single"
                    returnType = GetType(Single)
                Case "double"
                    returnType = GetType(Double)
                Case "object"
                    returnType = GetType(Object)
                Case "boolean"
                    returnType = GetType(Boolean)
                Case "char"
                    returnType = GetType(Char)
                Case Else
                    returnType = Nothing
                    retval = False
            End Select
            Return retval
        End Function

        Public ReadOnly Property Diagnostics As New ErrorList
        Private ReadOnly _syntax As TypeNameNode
        Private ReadOnly _imports As ImmutableArray(Of String)

        Public Sub New(syntax As SyntaxNode, Optional importsList? As ImmutableArray(Of String) = Nothing)
            Me._syntax = syntax
            Me._Diagnostics = New ErrorList
            Me._imports = If(importsList, ImmutableArray(Of String).Empty)
        End Sub

        Public Shared Function IsValidNamespace(name As String) As Boolean
            Return _Namespaces.Contains(name.ToLower)
        End Function

        Public Function ResolveType() As Type
            Dim retval As Type = GetType(Object)
            If _TryResolveBaseType(Me._syntax, Me._imports, retval) Then
                If Me._syntax.Items.Any(Function(node) node.HasGenerics) Then retval = _MakeGenericType(retval)
                If Me._syntax.NullableToken IsNot Nothing Then
                    If retval.IsValueType Then
                        retval = GetType(Nullable(Of)).MakeGenericType({retval})
                    Else
                        Diagnostics.ReportReferenceTypeCannotBeNullable(retval, Me._syntax)
                        retval = GetType(Object)
                    End If
                End If
                If Me._syntax.HasArrayDimensions Then retval = Me._MakeArrayType(retval)
            Else
                Diagnostics.ReportUndefinedType(Me._syntax)
            End If
            Return retval
        End Function

        Private Shared Function _TryResolveBaseType(syntaxNode As TypeNameNode, importsList As ImmutableArray(Of String), ByRef returnType As Type) As Boolean
            If syntaxNode.Items.Count = 1 AndAlso TryResolveInternal(_GetItemName(syntaxNode.Items.First), returnType) Then Return True
            Dim full_name As String = _ConvertTypeName(syntaxNode)
            If _DeclaredTypes.TryGetValue(full_name, returnType) Then Return True
            For Each n As String In importsList
                If _DeclaredTypes.TryGetValue(n & "." & full_name, returnType) Then Return True
            Next
            Return False
        End Function

        Private Shared Function _ConvertTypeName(syntaxNode As TypeNameNode) As String
            Return String.Join("."c, syntaxNode.Items.Select(Function(item) _GetItemName(item)).ToArray)
        End Function

        Private Shared Function _GetItemName(item As TypeNameItemNode) As String
            Return item.Identifier.Span.ToString.ToLower & If(item.HasGenerics, "`" & item.Generics.ListItems.Count, "")
        End Function

        Private Function _MakeArrayType(type As Type) As Type
            Dim retval As Type = type
            For Each t As ArrayDimensionsListItemNode In Me._syntax.ArrayDimensions.ListItems
                Dim rank As Integer = t.Delimeters.Count + 1
                retval = If(rank = 1, retval.MakeArrayType(), retval.MakeArrayType(rank))
            Next
            Return retval
        End Function

        Private Function _MakeGenericType(type As Type) As Type
            If Not type.IsGenericTypeDefinition Then
                Me.Diagnostics.ReportTypeNotGeneric(type, Me._syntax)
                Return type
            End If
            Dim generics As New List(Of Type)
            For Each item As TypeNameItemNode In Me._syntax.Items
                If item.HasGenerics Then
                    For Each genericType As GenericsListItemNode In item.Generics.ListItems
                        Dim resolver As New TypeResolver(genericType.TypeName, Me._imports)
                        Dim gtype As Type = resolver.ResolveType
                        If resolver.Diagnostics.Any Then
                            Me.Diagnostics.Append(resolver.Diagnostics)
                            Return type
                        Else
                            generics.Add(gtype)
                        End If
                    Next
                End If
            Next
            Try
                Return type.MakeGenericType(generics.ToArray)
            Catch ex As ArgumentException
                Me.Diagnostics.ReportGenericArgumentMissmatch(type, generics, Me._syntax)
                Return type
            End Try
        End Function

        Public Shared Function TryResolveTypeByName(name As String, ByRef value As Type, Optional importsList? As ImmutableArray(Of String) = Nothing) As Boolean
            value = Nothing
            If _DeclaredTypes.ContainsKey(name.ToLower) Then
                value = _DeclaredTypes.Item(name.ToLower)
                Return True
            End If
            Dim import As String = importsList.Value.FirstOrDefault(Function(imp) _DeclaredTypes.ContainsKey((imp & "." & name).ToLower))
            If import Is Nothing Then Return False
            value = _DeclaredTypes.Item((import & "." & name).ToLower)
            Return True
        End Function


    End Class

End Namespace
