// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cdrcs.Expressions
{
    using System;
    using System.Globalization;
    using System.Linq.Expressions;
    using System.Reflection;
    using Cdrcs.Protocols;
    using Cdrcs.Internal.Reflection;

    /// <summary>
    /// Creates expression of type <see cref="ICdrcsed"/> given a reader and runtime schema.
    /// </summary>
    /// <param name="reader">Expression representing reader.</param>
    /// <param name="schema">Expression representing RuntimeSchema.</param>
    /// <returns>Expression representing creation of <see cref="ICdrcsed"/> with the specified reader and runtime schema.</returns>
    public delegate Expression PayloadCdrcsedFactory(Expression reader, Expression schema);

    public static class ParserFactory<R>
    {
        public static IParser Create<S>(S schema)
        {
            return Create(schema, null);
        }

        public static IParser Create<S>(S schema, PayloadCdrcsedFactory bondedFactory)
        {
            return Cache<S>.Create(schema, bondedFactory);
        }

        static class Cache<S>
        {
            public static readonly Func<S, PayloadCdrcsedFactory, IParser> Create;

            [System.Diagnostics.CodeAnalysis.SuppressMessage(
                "Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
            static Cache()
            {
                Type parserType;

                var attribute = typeof(R).GetAttribute<ParserAttribute>();
                if (attribute == null)
                {
                    if (typeof(IProtocolReader).IsAssignableFrom(typeof(R)))
                    {
                        parserType = typeof(TaggedParser<R>);
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "Can't determine parser type for reader type {0}, specify using ParserAttribute.",
                                typeof(R)));
                    }
                }
                else
                {
                    var genericParserType = attribute.ParserType;
                    if (!genericParserType.IsGenericType() || genericParserType.GetTypeInfo().GenericTypeParameters.Length != 1)
                    {
                        throw new InvalidOperationException(
                            "Parser type is expected to be a generic type with one type param for Reader.");
                    }

                    parserType = genericParserType.MakeGenericType(typeof(R));
                    if (!typeof(IParser).IsAssignableFrom(parserType))
                    {
                        throw new InvalidOperationException(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "Parser type {0} specified in attribute for Reader type {1} is not an IParser.",
                                parserType,
                                typeof(R)));
                    }
                }

                var ctor = parserType.GetConstructor(typeof(S), typeof(PayloadCdrcsedFactory)) ??
                           parserType.GetConstructor(typeof(S));

                if (ctor == null)
                {
                    throw new InvalidOperationException(
                        string.Format(CultureInfo.InvariantCulture,
                                      "Can't find constructor for type '{0}' with either ({1}) or ({1}, {2}) signature.",
                                      parserType, typeof(S), typeof(PayloadCdrcsedFactory)));
                }

                var schema = Expression.Parameter(typeof(S));
                var bondedFactory = Expression.Parameter(typeof(PayloadCdrcsedFactory));
                var newExpression = ctor.GetParameters().Length == 2
                                        ? Expression.New(ctor, schema, bondedFactory)
                                        : Expression.New(ctor, schema);

                Create = Expression.Lambda<Func<S, PayloadCdrcsedFactory, IParser>>(newExpression, schema, bondedFactory).Compile();
            }
        }
    }
}
