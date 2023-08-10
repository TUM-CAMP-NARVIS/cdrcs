// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cdrcs.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Cdrcs.Internal.Reflection;

    public class TaggedParser<R> : IParser
    {
        delegate Expression TypeHandlerCompiletime(CdrcsDataType type);
        delegate Expression TypeHandlerRuntime(Expression type);

        readonly TaggedReader<R> reader = new TaggedReader<R>();
        readonly PayloadCdrcsedFactory bondedFactory;
        readonly TaggedParser<R> baseParser;
        readonly TaggedParser<R> fieldParser;
        readonly bool isBase;

        public TaggedParser(RuntimeSchema schema)
            : this(schema, null)
        { }

        public TaggedParser(RuntimeSchema schema, PayloadCdrcsedFactory bondedFactory)
            : this(bondedFactory)
        { }

        public TaggedParser(Type type)
            : this(type, null)
        { }

        public TaggedParser(Type type, PayloadCdrcsedFactory bondedFactory)
            : this(bondedFactory)
        { }

        private TaggedParser(PayloadCdrcsedFactory bondedFactory)
        {
            isBase = false;
            this.bondedFactory = bondedFactory ?? NewCdrcsed;
            baseParser = new TaggedParser<R>(this, isBase: true);
            fieldParser = this;
        }

        TaggedParser(TaggedParser<R> that, bool isBase)
        {
            this.isBase = isBase;
            bondedFactory = that.bondedFactory;
            reader = that.reader;
            baseParser = this;
            fieldParser = that;
        }

        public ParameterExpression ReaderParam { get { return reader.Param; } }
        public Expression ReaderValue { get { return reader.Param; } }
        public int HierarchyDepth { get { return 0; } }
        public bool IsCdrcsed { get { return false; } }

        public Expression Apply(ITransform transform)
        {
            var fieldId = Expression.Variable(typeof(UInt16), "fieldId");
            var fieldType = Expression.Variable(typeof(CdrcsDataType), "fieldType");
            var endLabel = Expression.Label("end");
            var breakLoop = Expression.Break(endLabel);

            // (int)fieldType > (int)BT_STOP_BASE
            var notEndOrEndBase = Expression.GreaterThan(
                Expression.Convert(fieldType, typeof(int)),
                Expression.Constant((int)CdrcsDataType.BT_STOP_BASE));

            var notEnd = isBase ? notEndOrEndBase : Expression.NotEqual(fieldType, Expression.Constant(CdrcsDataType.BT_STOP));
            var isEndBase = Expression.Equal(fieldType, Expression.Constant(CdrcsDataType.BT_STOP_BASE));

            var body = new List<Expression>
            {
                isBase ? reader.ReadBaseBegin() : reader.ReadStructBegin(),
                transform.Begin,
                transform.Base(baseParser),
                reader.ReadFieldBegin(fieldType, fieldId)
            };

            // known fields
            body.AddRange(
                from f in transform.Fields select
                    Expression.Loop(
                        Expression.IfThenElse(notEndOrEndBase,
                            Expression.Block(
                                Expression.IfThenElse(
                                    Expression.Equal(fieldId, Expression.Constant(f.Id)),
                                    Expression.Block(
                                        f.Value(fieldParser, fieldType),
                                        reader.ReadFieldEnd(),
                                        reader.ReadFieldBegin(fieldType, fieldId),
                                        breakLoop),
                                    Expression.IfThenElse(
                                        Expression.GreaterThan(fieldId, Expression.Constant(f.Id)),
                                        Expression.Block(
                                            f.Omitted,
                                            breakLoop),
                                        transform.UnknownField(fieldParser, fieldType, fieldId) ?? Skip(fieldType))),
                                reader.ReadFieldEnd(),
                                reader.ReadFieldBegin(fieldType, fieldId),
                                Expression.IfThen(
                                    Expression.GreaterThan(fieldId, Expression.Constant(f.Id)),
                                    breakLoop)),
                            Expression.Block(
                                f.Omitted,
                                breakLoop)),
                        endLabel));

            // unknown fields
            body.Add(
                ControlExpression.While(notEnd,
                    Expression.Block(
                        Expression.IfThenElse(
                            isEndBase,
                            transform.UnknownEnd,
                            Expression.Block(
                                transform.UnknownField(fieldParser, fieldType, fieldId) ?? Skip(fieldType),
                                reader.ReadFieldEnd())),
                        reader.ReadFieldBegin(fieldType, fieldId))));

            body.Add(isBase ? reader.ReadBaseEnd() : reader.ReadStructEnd());
            body.Add(transform.End);

            return Expression.Block(
                new[] { fieldType, fieldId },
                body);
        }

        public Expression Container(CdrcsDataType? expectedType, ContainerHandler handler)
        {
            var count = Expression.Variable(typeof(int), "count");
            var elementType = Expression.Variable(typeof(CdrcsDataType), "elementType");
            var next = Expression.GreaterThan(Expression.PostDecrementAssign(count), Expression.Constant(0));

            var loops = MatchOrCompatible(
                elementType,
                expectedType,
                type => handler(this, type, next, count, null));

            return Expression.Block(
                new[] { count, elementType },
                reader.ReadContainerBegin(count, elementType),
                loops,
                reader.ReadContainerEnd());
        }

        public Expression Map(CdrcsDataType? expectedKeyType, CdrcsDataType? expectedValueType, MapHandler handler)
        {
            var count = Expression.Variable(typeof(int), "count");
            var keyType = Expression.Variable(typeof(CdrcsDataType), "keyType");
            var valueType = Expression.Variable(typeof(CdrcsDataType), "valueType");
            var next = Expression.GreaterThan(Expression.PostDecrementAssign(count), Expression.Constant(0));

            var loops = MatchOrCompatible(keyType, expectedKeyType, constantKeyType =>
                MatchOrCompatible(valueType, expectedValueType, constantValueType =>
                    handler(this, this, constantKeyType, constantValueType, next, Expression.Empty(), count)));

            return Expression.Block(
                new[] { count, keyType, valueType },
                reader.ReadContainerBegin(count, keyType, valueType),
                loops,
                reader.ReadContainerEnd());
        }

        public Expression Blob(Expression count)
        {
            return reader.ReadBytes(count);
        }

        public Expression Scalar(Expression valueType, CdrcsDataType expectedType, ValueHandler handler)
        {
            return MatchOrCompatible(valueType, expectedType,
                type => handler(reader.Read(type)));
        }

        public Expression Cdrcsed(ValueHandler handler)
        {
            var newCdrcsed = bondedFactory(reader.Param, Expression.Constant(RuntimeSchema.Empty));

            return Expression.Block(
                handler(newCdrcsed),
                reader.Skip(Expression.Constant(CdrcsDataType.BT_STRUCT)));
        }

        public Expression Skip(Expression type)
        {
            return reader.Skip(type);
        }

        static Expression NewCdrcsed(Expression reader, Expression schema)
        {
            var ctor = typeof(CdrcsedVoid<>).MakeGenericType(reader.Type).GetConstructor(reader.Type);
            return Expression.New(ctor, reader);
        }

        static Expression MatchOrCompatible(Expression valueType, CdrcsDataType? expectedType, TypeHandlerRuntime handler)
        {
            return (expectedType == null) ?
                handler(valueType) :
                MatchOrCompatible(valueType, expectedType.Value, type => handler(Expression.Constant(type)));
        }

        // Generate expression to handle exact match or compatible type
        static Expression MatchOrCompatible(Expression valueType, CdrcsDataType expectedType, TypeHandlerCompiletime handler)
        {
            return MatchOrElse(valueType, expectedType, handler,
                   TryCompatible(valueType, expectedType, handler));
        }

        // valueType maybe a ConstantExpression and then Prune optimizes unreachable branches out
        static Expression MatchOrElse(Expression valueType, CdrcsDataType expectedType, TypeHandlerCompiletime handler, Expression orElse)
        {
            return PrunedExpression.IfThenElse(
                Expression.Equal(valueType, Expression.Constant(expectedType)),
                handler(expectedType),
                orElse);
        }

        // Generates expression to handle value of type that is different but compatible with expected type
        static Expression TryCompatible(Expression valueType, CdrcsDataType expectedType, TypeHandlerCompiletime handler)
        {
            if (expectedType == CdrcsDataType.BT_DOUBLE)
            {
                return MatchOrElse(valueType, CdrcsDataType.BT_FLOAT, handler,
                       InvalidType(expectedType, valueType));
            }

            if (expectedType == CdrcsDataType.BT_UINT64)
            {
                return MatchOrElse(valueType, CdrcsDataType.BT_UINT32, handler,
                       MatchOrElse(valueType, CdrcsDataType.BT_UINT16, handler,
                       MatchOrElse(valueType, CdrcsDataType.BT_UINT8, handler,
                       InvalidType(expectedType, valueType))));
            }

            if (expectedType == CdrcsDataType.BT_UINT32)
            {
                return MatchOrElse(valueType, CdrcsDataType.BT_UINT16, handler,
                       MatchOrElse(valueType, CdrcsDataType.BT_UINT8, handler,
                       InvalidType(expectedType, valueType)));
            }

            if (expectedType == CdrcsDataType.BT_UINT16)
            {
                return MatchOrElse(valueType, CdrcsDataType.BT_UINT8, handler,
                       InvalidType(expectedType, valueType));
            }

            if (expectedType == CdrcsDataType.BT_INT64)
            {
                return MatchOrElse(valueType, CdrcsDataType.BT_INT32, handler,
                       MatchOrElse(valueType, CdrcsDataType.BT_INT16, handler,
                       MatchOrElse(valueType, CdrcsDataType.BT_INT8, handler,
                       InvalidType(expectedType, valueType))));
            }

            if (expectedType == CdrcsDataType.BT_INT32)
            {
                return MatchOrElse(valueType, CdrcsDataType.BT_INT16, handler,
                       MatchOrElse(valueType, CdrcsDataType.BT_INT8, handler,
                       InvalidType(expectedType, valueType)));
            }

            if (expectedType == CdrcsDataType.BT_INT16)
            {
                return MatchOrElse(valueType, CdrcsDataType.BT_INT8, handler,
                       InvalidType(expectedType, valueType));
            }

            return InvalidType(expectedType, valueType);
        }

        static Expression InvalidType(CdrcsDataType expectedType, Expression valueType)
        {
            return ThrowExpression.InvalidTypeException(Expression.Constant(expectedType), valueType);
        }
    }
}
