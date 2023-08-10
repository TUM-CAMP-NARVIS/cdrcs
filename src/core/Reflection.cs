// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cdrcs
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;
    using Cdrcs.Internal.Reflection;

    public static class Reflection
    {
        static readonly object Empty = new object();
        static readonly Dictionary<CdrcsDataType, string> bondTypeName = new Dictionary<CdrcsDataType, string>
        {
            {CdrcsDataType.BT_BOOL, "bool"},
            {CdrcsDataType.BT_UINT8, "uint8"},
            {CdrcsDataType.BT_UINT16, "uint16"},
            {CdrcsDataType.BT_UINT32, "uint32"},
            {CdrcsDataType.BT_UINT64, "uint64"},
            {CdrcsDataType.BT_FLOAT, "float"},
            {CdrcsDataType.BT_DOUBLE, "double"},
            {CdrcsDataType.BT_STRING, "string"},
            {CdrcsDataType.BT_LIST, "list"},
            {CdrcsDataType.BT_SET, "set"},
            {CdrcsDataType.BT_MAP, "map"},
            {CdrcsDataType.BT_INT8, "int8"},
            {CdrcsDataType.BT_INT16, "int16"},
            {CdrcsDataType.BT_INT32, "int32"},
            {CdrcsDataType.BT_INT64, "int64"},
            {CdrcsDataType.BT_WSTRING, "wstring"}
        };

        #region Public APIs

        /// <summary>
        /// Get list of fields for a Cdrcs schema
        /// </summary>
        public static IEnumerable<ISchemaField> GetSchemaFields(this Type type)
        {
            var fields = from fieldInfo in type.GetTypeInfo().DeclaredFields.Where(f => f.IsPublic)
                         let idAttr = fieldInfo.GetAttribute<IdAttribute>()
                         where idAttr != null
                         select new Field(fieldInfo, idAttr.Value) as ISchemaField;

            var properties =
                from propertyInfo in type.GetTypeInfo().DeclaredProperties
                let idAttr = propertyInfo.GetAttribute<IdAttribute>()
                where idAttr != null
                select new Property(propertyInfo, idAttr.Value) as ISchemaField;

            var concatenated = fields.Concat(properties);
            return concatenated.OrderBy(m => m.Id);
        }

        /// <summary>
        /// Get the inner Type of composite/container types
        /// </summary>
        public static Type GetValueType(this Type type)
        {
            if (type.IsCdrcsNullable() || type.IsCdrcsed())
                return type.GetTypeInfo().GenericTypeArguments[0];

            if (type.IsArray)
                return type.GetElementType();

            if (type.IsCdrcsBlob())
                return typeof(sbyte);

            return type.GetMethod(typeof(IEnumerable<>), "GetEnumerator")
                .ReturnType
                .GetTypeInfo()
                .GetDeclaredProperty("Current")
                .PropertyType;
        }

        /// <summary>
        /// Get the key and value Type for a map
        /// </summary>
        public static KeyValuePair<Type, Type> GetKeyValueType(this Type type)
        {
            var types = GetValueType(type).GetTypeInfo().GenericTypeArguments;

            if (types.Length != 2)
            {
                throw new InvalidOperationException("Expected generic type with 2 type arguments.");
            }

            return new KeyValuePair<Type, Type>(types[0], types[1]);
        }

        /// <summary>
        /// Get a value indicating whether the Type is a Cdrcs schema
        /// </summary>
        public static bool IsCdrcsStruct(this Type type)
        {
            return null != type.GetAttribute<SchemaAttribute>();
        }

        /// <summary>
        /// Get a value indicating whether the Type is a bonded&lt;T>
        /// </summary>
        public static bool IsCdrcsed(this Type type)
        {
            if (type.IsGenericType())
            {
                var definition = type.GetGenericTypeDefinition();
                return definition == typeof(ICdrcsed<>)
                    || definition == typeof(Tag.bonded<>)
                    || definition.GetTypeInfo().ImplementedInterfaces.Contains(typeof(ICdrcsed));
            }

            return false;
        }

        /// <summary>
        /// Get a value indicating whether the Type is a Cdrcs nullable type
        /// </summary>
        public static bool IsCdrcsNullable(this Type type)
        {
            return type.IsGenericType() &&
                   (type.GetGenericTypeDefinition() == typeof(Tag.nullable<>));
        }

        /// <summary>
        /// Get a value indicating whether the Type is a Cdrcs string
        /// </summary>
        public static bool IsCdrcsString(this Type type)
        {
            return type == typeof(Tag.wstring) || type == typeof(string);
        }

        /// <summary>
        /// Get a value indicating whether the Type is a Cdrcs blob
        /// </summary>
        public static bool IsCdrcsBlob(this Type type)
        {
            return type == typeof(Tag.blob) || type == typeof(ArraySegment<byte>);
        }

        /// <summary>
        /// Get a value indicating whether the Type is a Cdrcs list
        /// or a Cdrcs vector
        /// </summary>
        public static bool IsCdrcsList(this Type type)
        {
            if (type.IsGenericType())
            {
                var genericType = type.GetGenericTypeDefinition();
                if (genericType == typeof(IList<>) || genericType == typeof(ICollection<>))
                    return true;
            }

            return typeof(IList).IsAssignableFrom(type) ||
                   typeof(ICollection).IsAssignableFrom(type);
        }

        /// <summary>
        /// Get a value indicating whether the Type is a Cdrcs map
        /// </summary>
        public static bool IsCdrcsMap(this Type type)
        {
            if (type.IsGenericType())
            {
                var genericType = type.GetGenericTypeDefinition();
                if (genericType == typeof(IDictionary<,>))
                    return true;
            }

            return typeof(IDictionary).IsAssignableFrom(type);
        }

        /// <summary>
        /// Get a value indicating whether the Type is a Cdrcs set
        /// </summary>
        public static bool IsCdrcsSet(this Type type)
        {
            if (!type.IsGenericType())
                return false;

            return typeof(ISet<>).MakeGenericType(type.GetTypeInfo().GenericTypeArguments[0]).IsAssignableFrom(type);
        }

        /// <summary>
        /// Get a value indicating whether the Type is a Cdrcs container
        /// </summary>
        public static bool IsCdrcsContainer(this Type type)
        {
            return type.IsCdrcsList() ||
                   type.IsCdrcsSet() ||
                   type.IsCdrcsMap() ||
                   type.IsCdrcsBlob();
        }

        /// <summary>
        /// Get the CdrcsDataType value for the Type
        /// </summary>
        public static CdrcsDataType GetCdrcsDataType(this Type type)
        {
            while (true)
            {
                if (type.IsCdrcsStruct() || type.IsCdrcsed())
                    return CdrcsDataType.BT_STRUCT;

                if (type.IsCdrcsNullable())
                    return CdrcsDataType.BT_LIST;

                if (type.IsCdrcsMap())
                    return CdrcsDataType.BT_MAP;

                if (type.IsCdrcsSet())
                    return CdrcsDataType.BT_SET;

                if (type.IsCdrcsList() || type.IsCdrcsBlob())
                    return CdrcsDataType.BT_LIST;

                if (type.IsEnum())
                    return CdrcsDataType.BT_INT32;

                if (type == typeof(string))
                    return CdrcsDataType.BT_STRING;

                if (type == typeof(Tag.wstring))
                    return CdrcsDataType.BT_WSTRING;

                if (type == typeof(bool))
                    return CdrcsDataType.BT_BOOL;

                if (type == typeof(byte))
                    return CdrcsDataType.BT_UINT8;

                if (type == typeof(UInt16))
                    return CdrcsDataType.BT_UINT16;

                if (type == typeof(UInt32))
                    return CdrcsDataType.BT_UINT32;

                if (type == typeof(UInt64))
                    return CdrcsDataType.BT_UINT64;

                if (type == typeof(float))
                    return CdrcsDataType.BT_FLOAT;

                if (type == typeof(double))
                    return CdrcsDataType.BT_DOUBLE;

                if (type == typeof(sbyte))
                    return CdrcsDataType.BT_INT8;

                if (type == typeof(Int16))
                    return CdrcsDataType.BT_INT16;

                if (type == typeof(Int32))
                    return CdrcsDataType.BT_INT32;

                if (type == typeof(Int64))
                    return CdrcsDataType.BT_INT64;

                if (type.IsGenericType() && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    type = type.GetValueType();
                    continue;
                }

                return CdrcsDataType.BT_UNAVAILABLE;
            }
        }

        /// <summary>
        /// Get the ListSubType value for the Type
        /// </summary>
        public static ListSubType GetCdrcsListDataType(this Type type)
        {
            while (true)
            {
                if (type.IsCdrcsNullable())
                    return ListSubType.NULLABLE_SUBTYPE;

                if (type.IsCdrcsBlob())
                    return ListSubType.BLOB_SUBTYPE;

                if (type.IsCdrcsList())
                    return ListSubType.NO_SUBTYPE;

                if (type.IsGenericType() && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    type = type.GetValueType();
                    continue;
                }

                return ListSubType.NO_SUBTYPE;
            }
        }

        /// <summary>
        /// Get the Type representing the base schema or null if the schema has no base
        /// </summary>
        public static Type GetBaseSchemaType(this Type type)
        {
            if (type.IsClass())
            {
                var baseType = type.GetBaseType();
                return baseType != null && baseType.GetAttribute<SchemaAttribute>() != null ? baseType : null;
            }

            if (type.IsInterface())
            {
                // Get all base interfaces. In case if an inheritance chain longer than 2, this returns all 
                // the base interfaces flattened in no particular order, so we have to find the direct parent.
                var baseInterfaces = type.GetTypeInfo().ImplementedInterfaces
                    .Where(t => t.GetAttribute<SchemaAttribute>() != null).ToArray();

                for (var i = 0; i < baseInterfaces.Length; i++)
                {
                    var baseInterface = baseInterfaces[i];
                    var indirectBaseInterfacesCount =
                        baseInterface.GetTypeInfo().ImplementedInterfaces
                        .Count(t => t.GetAttribute<SchemaAttribute>() != null);

                    if (indirectBaseInterfacesCount == baseInterfaces.Length - 1)
                    {
                        return baseInterface;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get the Type of the schema field, including any type annotations from TypeAttribute
        /// </summary>
        /// <remarks>
        /// In some cases this may not be the actual type of the property or field.
        /// If the property or field has a TypeAttribute, this will be the attribute's value
        /// and can provide schema information that is not available on the actual 
        /// property/field type.
        /// </remarks>
        public static Type GetSchemaType(this ISchemaField schemaField)
        {
            var type = schemaField.MemberType;

            var typeAttr = schemaField.GetAttribute<TypeAttribute>();
            if (typeAttr != null)
            {
                type = ResolveTypeArgumentTags(type, typeAttr.Value);
            }

            return type;
        }

        #endregion

        #region Internal

        /// <summary>
        /// Gets the reflection member information from the top-level node in the body of the given lambda expression.
        /// </summary>
        /// <typeparam name="T">Input type of the lambda.</typeparam>
        /// <typeparam name="TResult">Return type of the lambda.</typeparam>
        /// <param name="expression">Lambda expression to extract reflection information from</param>
        /// <returns>Member information of the top-level node in the body of the lambda expression. An exception occurs if this node does not contain member information.</returns>
        /// <example>
        /// To obtain the MethodInfo for the "int Math::Abs(int)" overload, you can write:
        /// <code>(MethodInfo)CdrcsReflection.InfoOf((int x) => Math.Abs(x))</code>
        /// </example>
        static MemberInfo InfoOf<T, TResult>(Expression<Func<T, TResult>> expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            return InfoOf(expression.Body);
        }

        /// <summary>
        /// Gets the reflection member information from the top-level node in the body of the given lambda expression.
        /// </summary>
        /// <typeparam name="T">Input type of the lambda.</typeparam>
        /// <typeparam name="TResult">Return type of the lambda.</typeparam>
        /// <param name="expression">Lambda expression to extract reflection information from</param>
        /// <returns>Member information of the top-level node in the body of the lambda expression. Return null if that member is not a method. An exception occurs if this node does not contain member information.</returns>
        /// <example>
        /// To obtain the MethodInfo for the "int Math::Abs(int)" overload, you can write:
        /// <code>CdrcsReflection.MethodInfoOf((int x) => Math.Abs(x))</code>
        /// </example>
        internal static MethodInfo MethodInfoOf<T, TResult>(Expression<Func<T, TResult>> expression)
        {
            return InfoOf(expression) as MethodInfo;
        }

        /// <summary>
        /// Gets the reflection member information from the top-level node in the body of the given lambda expression.
        /// </summary>
        /// <typeparam name="T">Input type of the lambda.</typeparam>
        /// <typeparam name="TResult">Return type of the lambda.</typeparam>
        /// <param name="expression">Lambda expression to extract reflection information from</param>
        /// <returns>Member information of the top-level node in the body of the lambda expression. Return null if the member is not a generic method definition. An exception occurs if this node does not contain member information.</returns>
        /// <example>
        /// To obtain the generic method definition for some "int Foo::Bar&lt;T>(T)" overload, you can write:
        /// <code>CdrcsReflection.GenericMethodInfoOf((int x) => Foo.Bar(x))</code>, which returns the definition Foo.Bar&lt;>
        /// </example>
        internal static MethodInfo GenericMethodInfoOf<T, TResult>(Expression<Func<T, TResult>> expression)
        {
            var methodInfo = MethodInfoOf(expression);
            return methodInfo == null ? null : methodInfo.GetGenericMethodDefinition();
        }

        /// <summary>
        /// Gets the reflection member information from the top-level node in the body of the given lambda expression.
        /// </summary>
        /// <typeparam name="T">Input type of the lambda.</typeparam>
        /// <typeparam name="TResult">Return type of the lambda.</typeparam>
        /// <param name="expression">Lambda expression to extract reflection information from</param>
        /// <returns>Member information of the top-level node in the body of the lambda expression. Return null if the member is not a PropertyInfo. An exception occurs if this node does not contain member information.</returns>
        /// <example>
        /// To obtain the PropertyInfo for the "int Foo::SomeProperty", you can write:
        /// <code>CdrcsReflection.PropertyInfoOf((Foo f) => f.SomeProperty)</code>
        /// </example>
        internal static PropertyInfo PropertyInfoOf<T, TResult>(Expression<Func<T, TResult>> expression)
        {
            return InfoOf(expression) as PropertyInfo;
        }

        /// <summary>
        /// Gets the reflection member information from the top-level node in the body of the given lambda expression.
        /// </summary>
        /// <typeparam name="T">Input type of the lambda.</typeparam>
        /// <typeparam name="TResult">Return type of the lambda.</typeparam>
        /// <param name="expression">Lambda expression to extract reflection information from</param>
        /// <returns>Member information of the top-level node in the body of the lambda expression. Return null if the member is not a FieldInfo. An exception occurs if this node does not contain member information.</returns>
        /// <example>
        /// To obtain the FieldInfo for the "int Foo::someField" field, you can write:
        /// <code>CdrcsReflection.FieldInfoOf((Foo f) => f.someField)</code>
        /// </example>
        internal static FieldInfo FieldInfoOf<T, TResult>(Expression<Func<T, TResult>> expression)
        {
            return InfoOf(expression) as FieldInfo;
        }

        /// <summary>
        /// Gets the reflection member information from the top-level node in the body of the given lambda expression.
        /// </summary>
        /// <typeparam name="TResult">Return type of the lambda.</typeparam>
        /// <param name="expression">Lambda expression to extract reflection information from</param>
        /// <returns>Member information of the top-level node in the body of the lambda expression. An exception occurs if this node does not contain member information.</returns>
        /// <example>
        /// To obtain the PropertyInfo of "DateTime DateTime::Now { get; }", you can write:
        /// <code>(PropertyInfo)CdrcsReflection.InfoOf(() => DateTime.Now)</code>
        /// </example>
        static MemberInfo InfoOf<TResult>(Expression<Func<TResult>> expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            return InfoOf(expression.Body);
        }

        /// <summary>
        /// Gets the reflection member information from the top-level node in the body of the given lambda expression.
        /// </summary>
        /// <typeparam name="TResult">Return type of the lambda.</typeparam>
        /// <param name="expression">Lambda expression to extract reflection information from</param>
        /// <returns>Member information of the top-level node in the body of the lambda expression. Return null if that member is not a method. An exception occurs if this node does not contain member information.</returns>
        /// <example>
        /// To obtain the MethodInfo for the "int Math::Abs(int)" overload, you can write:
        /// <code>CdrcsReflection.MethodInfoOf(() => Math.Abs(default(int)))</code>
        /// </example>
        internal static MethodInfo MethodInfoOf<TResult>(Expression<Func<TResult>> expression)
        {
            return InfoOf(expression) as MethodInfo;
        }

        /// <summary>
        /// Gets the reflection member information from the top-level node in the body of the given lambda expression.
        /// </summary>
        /// <typeparam name="TResult">Return type of the lambda.</typeparam>
        /// <param name="expression">Lambda expression to extract reflection information from</param>
        /// <returns>Member information of the top-level node in the body of the lambda expression. Return null if the member is not a generic method definition. An exception occurs if this node does not contain member information.</returns>
        /// <example>
        /// To obtain the generic method definition for some "int Foo::Bar&lt;T>(T)" overload, you can write:
        /// <code>CdrcsReflection.GenericMethodInfoOf(() => Foo.Bar(default(int)))</code>, which returns the definition Foo.Bar&lt;>
        /// </example>
        internal static MethodInfo GenericMethodInfoOf<TResult>(Expression<Func<TResult>> expression)
        {
            var methodInfo = MethodInfoOf(expression);
            return methodInfo == null ? null : methodInfo.GetGenericMethodDefinition();
        }

        /// <summary>
        /// Gets the reflection member information from the top-level node in the body of the given lambda expression.
        /// </summary>
        /// <typeparam name="T">Input type of the lambda.</typeparam>
        /// <param name="expression">Lambda expression to extract reflection information from</param>
        /// <returns>Member information of the top-level node in the body of the lambda expression. An exception occurs if this node does not contain member information.</returns>
        /// <example>
        /// To obtain the MethodInfo for the "void Console::WriteLine(string)" overload, you can write:
        /// <code>(MethodInfo)CdrcsReflection.InfoOf((string s) => Console.WriteLine(s))</code>
        /// </example>
        static MemberInfo InfoOf<T>(Expression<Action<T>> expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            return InfoOf(expression.Body);
        }

        /// <summary>
        /// Gets the reflection member information from the top-level node in the body of the given lambda expression.
        /// </summary>
        /// <typeparam name="T">Input type of the lambda.</typeparam>
        /// <param name="expression">Lambda expression to extract reflection information from</param>
        /// <returns>Member information of the top-level node in the body of the lambda expression. Return null if that member is not a method. An exception occurs if this node does not contain member information.</returns>
        /// <example>
        /// To obtain the MethodInfo for the "void Foo::DoThing(int)" overload, you can write:
        /// <code>CdrcsReflection.MethodInfoOf(() => Foo.DoThing(default(int)))</code>
        /// </example>
        internal static MethodInfo MethodInfoOf<T>(Expression<Action<T>> expression)
        {
            return InfoOf(expression) as MethodInfo;
        }

        /// <summary>
        /// Gets the reflection member information from the top-level node in the body of the given lambda expression.
        /// </summary>
        /// <typeparam name="T">Input type of the lambda.</typeparam>
        /// <param name="expression">Lambda expression to extract reflection information from</param>
        /// <returns>Member information of the top-level node in the body of the lambda expression. Return null if the member is not a generic method definition. An exception occurs if this node does not contain member information.</returns>
        /// <example>
        /// To obtain the generic method definition for some "void Foo::Bar&lt;T>(T)" overload, you can write:
        /// <code>CdrcsReflection.GenericMethodInfoOf(() => Foo.Bar(default(int)))</code>, which returns the definition Foo.Bar&lt;>
        /// </example>
        internal static MethodInfo GenericMethodInfoOf<T>(Expression<Action<T>> expression)
        {
            var methodInfo = MethodInfoOf(expression);
            return methodInfo == null ? null : methodInfo.GetGenericMethodDefinition();
        }

        /// <summary>
        /// Gets the reflection member information from the top-level node in the body of the given lambda expression.
        /// </summary>
        /// <param name="expression">Lambda expression to extract reflection information from</param>
        /// <returns>Member information of the top-level node in the body of the lambda expression. An exception occurs if this node does not contain member information.</returns>
        static MemberInfo InfoOf(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            MethodCallExpression mce;
            MemberExpression me;
            NewExpression ne;
            UnaryExpression ue;
            BinaryExpression be;

            if ((mce = expression as MethodCallExpression) != null)
            {
                return mce.Method;
            }
            else if ((me = expression as MemberExpression) != null)
            {
                return me.Member;
            }
            else if ((ne = expression as NewExpression) != null)
            {
                return ne.Constructor;
            }
            else if ((ue = expression as UnaryExpression) != null)
            {
                if (ue.Method != null)
                {
                    return ue.Method;
                }
            }
            else if ((be = expression as BinaryExpression) != null)
            {
                if (be.Method != null)
                {
                    return be.Method;
                }
            }

            throw new NotSupportedException("Expression tree type doesn't have an extractable MemberInfo object.");
        }

        internal static Modifier GetModifier(this ISchemaField schemaField)
        {
            return schemaField.GetAttribute<RequiredAttribute>() != null
                ? Modifier.Required
                : schemaField.GetAttribute<RequiredOptionalAttribute>() != null
                ? Modifier.RequiredOptional
                : Modifier.Optional;
        }

        internal static int GetHierarchyDepth(this RuntimeSchema schema)
        {
            if (!schema.IsStruct)
                return 0;

            var depth = 0;
            for (var type = schema.TypeDef; type != null; type = schema.SchemaDef.structs[type.struct_def].base_def)
                depth++;
            return depth;
        }

        internal static int GetHierarchyDepth(this Type type)
        {
            var depth = 0;
            for (; type != null; type = type.GetBaseSchemaType()) depth++;
            return depth;
        }

        internal static string GetSchemaName(this Type type)
        {
            string name;

            if (type.IsCdrcsStruct() || type.IsEnum())
            {
                name = type.Name;
                var n = name.IndexOf('`');
                if (n >= 0)
                    name = name.Remove(n);
            }
            else if (type.IsCdrcsBlob())
            {
                return "blob";
            }
            else if (type.IsCdrcsed())
            {
                name = "bonded";
            }
            else if (type.IsCdrcsNullable())
            {
                name = "nullable";
            }
            else
            {
                name = bondTypeName[type.GetCdrcsDataType()];
            }

            if (!type.IsGenericType())
                return name;

            var args = type.GetTypeInfo().GenericTypeArguments;
            var builder = new StringBuilder(name, args.Length * 64);

            builder.Append("<");
            for (var i = 0; i < args.Length; ++i)
            {
                if (i != 0)
                    builder.Append(", ");

                builder.Append(args[i].GetSchemaFullName());
            }
            builder.Append(">");
            return builder.ToString();
        }

        internal static string GetSchemaFullName(this Type type)
        {
            if (type.IsCdrcsStruct() || type.IsEnum())
                return type.GetSchemaNamespace() + "." + type.GetSchemaName();

            return type.GetSchemaName();
        }

        static string GetSchemaNamespace(this Type type)
        {
            var attr = type.GetAttribute<NamespaceAttribute>();
            if (attr != null)
                return attr.Value;

            return type.Namespace;
        }

        static T GetAttribute<T>(this MemberInfo type)
            where T : class
        {
            return type.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
        }

        internal static T GetAttribute<T>(this Type type)
            where T : class
        {
            // ReSharper disable once RedundantCast
            // This explicit cast is needed because when targeting non-portable runtime,
            // type.GetTypeInfo returns an object which is also a Type, causing wrong call.
            return GetAttribute<T>(type.GetTypeInfo() as MemberInfo);
        }

        static T GetAttribute<T>(this ISchemaField schemaField)
            where T : class
        {
            return schemaField.MemberInfo.GetAttribute<T>();
        }

        static Type ResolveTypeArgumentTags(Type memberType, Type schemaType)
        {
            if (schemaType.IsGenericType())
            {
                Type[] memberTypeArguments;

                var schemaGenericType = schemaType.GetGenericTypeDefinition();
                var memberGenericType = memberType.IsGenericType() ? memberType.GetGenericTypeDefinition() : null;

                if ((schemaGenericType == typeof(Tag.nullable<>) && memberGenericType != typeof(Nullable<>)) ||
                    (schemaGenericType == typeof(Tag.bonded<>) && memberGenericType != typeof(ICdrcsed<>)))
                {
                    memberTypeArguments = new[] { memberType };
                }
                else
                {
                    memberTypeArguments = memberType.GetTypeInfo().GenericTypeArguments;
                }

                return schemaGenericType.MakeGenericType(Enumerable.Zip(
                    memberTypeArguments,
                    schemaType.GetTypeInfo().GenericTypeArguments,
                    ResolveTypeArgumentTags).ToArray());
            }

            return (schemaType == typeof(Tag.structT) || schemaType == typeof(Tag.classT)) ?
                memberType : 
                schemaType;
        }

        internal static Type GetObjectType(this Type schemaType)
        {
            if (schemaType == typeof(Tag.wstring))
            {
                return typeof(string);
            }

            if (schemaType == typeof(Tag.blob))
            {
                return typeof(ArraySegment<byte>);
            }
            
            if (schemaType.IsGenericType())
            {
                return schemaType.GetGenericTypeDefinition().MakeGenericType(
                    schemaType.GetTypeInfo().GenericTypeArguments.Select(type =>
                    {
                        if (type.IsGenericType())
                        {
                            var genericType = type.GetGenericTypeDefinition();
                            if (genericType == typeof(Tag.nullable<>))
                            {
                                var nullableValue = type.GetTypeInfo().GenericTypeArguments[0];
                                return nullableValue.IsClass() || nullableValue.IsCdrcsBlob()
                                    ? nullableValue.GetObjectType()
                                    : typeof(Nullable<>).MakeGenericType(nullableValue.GetObjectType());
                            }

                            if (genericType == typeof(Tag.bonded<>))
                            {
                                return typeof(ICdrcsed<>).MakeGenericType(type.GetTypeInfo().GenericTypeArguments[0].GetObjectType());
                            }
                        }

                        return type.GetObjectType();
                    }).ToArray());
            }

            return schemaType;
        }

        internal static object GetDefaultValue(this ISchemaField schemaField)
        {
            var declaringType = schemaField.DeclaringType;
            var declaringTypeInfo = declaringType.GetTypeInfo();
            var defaultAttribute = schemaField.GetAttribute<DefaultAttribute>(); 

            // For interfaces determine member default value from the type and/or DefaultAttribute
            if (declaringTypeInfo.IsInterface)
            {
                var schemaType = schemaField.GetSchemaType();

                if (defaultAttribute != null)
                {
                    if (defaultAttribute.Value == null)
                    {
                        return null;
                    }

                    if (schemaType.IsCdrcsNullable() || schemaType.IsCdrcsStruct() || schemaType.IsCdrcsContainer())
                    {
                        InvalidDefaultAttribute(schemaField, defaultAttribute.Value);
                    }

                    return defaultAttribute.Value;
                }
                else
                {
                    if (schemaType.IsCdrcsNullable())
                    {
                        return null;
                    }

                    if (schemaType.IsCdrcsStruct() || schemaType.IsCdrcsed() || schemaType.IsCdrcsContainer() || schemaType.IsCdrcsBlob())
                    {
                        return Empty;
                    }

                    if (schemaType.IsCdrcsString())
                    {
                        return string.Empty;
                    }

                    return Activator.CreateInstance(schemaField.MemberType);
                }
            }

            if (defaultAttribute != null)
            {
                InvalidDefaultAttribute(schemaField, defaultAttribute.Value);
            }
            
            // For classes create a default instance and get the actual default value of the member
            var objectType = declaringType.GetObjectType();
            var objectMemeber = objectType.GetSchemaFields().Single(m => m.Id == schemaField.Id);
            var obj = Activator.CreateInstance(objectType);
            var defaultValue = objectMemeber.GetValue(obj);
            return defaultValue;
        }

        static void InvalidDefaultAttribute(ISchemaField schemaField, object value)
        {
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                "Invalid default value '{2}' specified by DefaultAttribute for {0}.{1}", 
                schemaField.DeclaringType, schemaField.Name, value == null ? "null" : value.ToString()));
        }

        #endregion
    }
}
