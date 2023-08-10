// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cdrcs.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Cdrcs.Protocols;
    using Cdrcs.Internal.Reflection;

    /// <summary>
    /// Abstracts calling protocol writer methods via Expressions
    /// </summary>
    /// <typeparam name="W"></typeparam>
    /// <remarks>The protocol should be a value type for the calls to be inlined.</remarks>
    internal class ProtocolWriter<W>
    {
        readonly ParameterExpression writer = Expression.Parameter(typeof(W), "writer");

        static readonly MethodInfo marshalCdrcsed =   Reflection.MethodInfoOf(() => Marshaler.Marshal(default(ICdrcsed)));
        static readonly MethodInfo serializeCdrcsed = Reflection.MethodInfoOf((ICdrcsed bonded) => bonded.Serialize(default(W)));
        static readonly MethodInfo writeBytes =      GetMethod(Reflection.MethodInfoOf((IProtocolWriter writer) => writer.WriteBytes(default(ArraySegment<byte>))));
        static readonly MethodInfo structBegin =     GetMethod(Reflection.MethodInfoOf((IProtocolWriter writer) => writer.WriteStructBegin(default(Metadata))));
        static readonly MethodInfo baseBegin =       GetMethod(Reflection.MethodInfoOf((IProtocolWriter writer) => writer.WriteBaseBegin(default(Metadata))));
        static readonly MethodInfo structEnd =       GetMethod(Reflection.MethodInfoOf((IProtocolWriter writer) => writer.WriteStructEnd()));
        static readonly MethodInfo baseEnd =         GetMethod(Reflection.MethodInfoOf((IProtocolWriter writer) => writer.WriteBaseEnd()));
        static readonly MethodInfo fieldBegin =      GetMethod(Reflection.MethodInfoOf((IProtocolWriter writer) => writer.WriteFieldBegin(default(CdrcsDataType), default(UInt16), default(Metadata))));
        static readonly MethodInfo fieldEnd =        GetMethod(Reflection.MethodInfoOf((IProtocolWriter writer) => writer.WriteFieldEnd()));
        static readonly MethodInfo fieldOmitted =    GetMethod(Reflection.MethodInfoOf((IProtocolWriter writer) => writer.WriteFieldOmitted(default(CdrcsDataType), default(UInt16), default(Metadata))));
        static readonly MethodInfo containerBegin =  GetMethod(Reflection.MethodInfoOf((IProtocolWriter writer) => writer.WriteContainerBegin(default(int), default(CdrcsDataType))));
        static readonly MethodInfo containerBegin2 = GetMethod(Reflection.MethodInfoOf((IProtocolWriter writer) => writer.WriteContainerBegin(default(int), default(CdrcsDataType), default(CdrcsDataType))));
        static readonly MethodInfo containerEnd =    GetMethod(Reflection.MethodInfoOf((IProtocolWriter writer) => writer.WriteContainerEnd()));
        static readonly MethodInfo itemBegin =       GetMethod(Reflection.MethodInfoOf((ITextProtocolWriter writer) => writer.WriteItemBegin()));
        static readonly MethodInfo itemEnd =         GetMethod(Reflection.MethodInfoOf((ITextProtocolWriter writer) => writer.WriteItemEnd()));
        
/*        static readonly bool untaggedProtocol =
            typeof(IUntaggedProtocolReader).IsAssignableFrom(typeof(W).GetAttribute<ReaderAttribute>().ReaderType);
*/
        static readonly Dictionary<CdrcsDataType, MethodInfo> write = new Dictionary<CdrcsDataType, MethodInfo>
            {
                { CdrcsDataType.BT_BOOL,    GetMethod(Reflection.MethodInfoOf((IProtocolWriter writer) => writer.WriteBool(default(bool)))) },
                { CdrcsDataType.BT_UINT8,   GetMethod(Reflection.MethodInfoOf((IProtocolWriter writer) => writer.WriteUInt8(default(byte)))) },
                { CdrcsDataType.BT_UINT16,  GetMethod(Reflection.MethodInfoOf((IProtocolWriter writer) => writer.WriteUInt16(default(UInt16)))) },
                { CdrcsDataType.BT_UINT32,  GetMethod(Reflection.MethodInfoOf((IProtocolWriter writer) => writer.WriteUInt32(default(UInt32)))) },
                { CdrcsDataType.BT_UINT64,  GetMethod(Reflection.MethodInfoOf((IProtocolWriter writer) => writer.WriteUInt64(default(UInt64)))) },
                { CdrcsDataType.BT_FLOAT,   GetMethod(Reflection.MethodInfoOf((IProtocolWriter writer) => writer.WriteFloat(default(float)))) },
                { CdrcsDataType.BT_DOUBLE,  GetMethod(Reflection.MethodInfoOf((IProtocolWriter writer) => writer.WriteDouble(default(double)))) },
                { CdrcsDataType.BT_STRING,  GetMethod(Reflection.MethodInfoOf((IProtocolWriter writer) => writer.WriteString(default(string)))) },
                { CdrcsDataType.BT_INT8,    GetMethod(Reflection.MethodInfoOf((IProtocolWriter writer) => writer.WriteInt8(default(sbyte)))) },
                { CdrcsDataType.BT_INT16,   GetMethod(Reflection.MethodInfoOf((IProtocolWriter writer) => writer.WriteInt16(default(Int16)))) },
                { CdrcsDataType.BT_INT32,   GetMethod(Reflection.MethodInfoOf((IProtocolWriter writer) => writer.WriteInt32(default(Int32)))) },
                { CdrcsDataType.BT_INT64,   GetMethod(Reflection.MethodInfoOf((IProtocolWriter writer) => writer.WriteInt64(default(Int64)))) },
                { CdrcsDataType.BT_WSTRING, GetMethod(Reflection.MethodInfoOf((IProtocolWriter writer) => writer.WriteWString(default(string)))) },
                { CdrcsDataType.BT_ENUM,    GetMethod(Reflection.MethodInfoOf((IProtocolWriter writer) => writer.WriteEnum(default(UInt32)))) },
            };

        static MethodInfo GetMethod(MethodInfo method)
        {
            // There is a method (sic!) to this madness. We need to get a method of type W, not method of the 
            // interface. Only this way the calls to methods of protocols that are implemented as a value types 
            // will be inlined by JIT. Inlining makes a big difference for performance.
            return typeof(W).FindMethod(method.Name, method.GetParameters().Select(p => p.ParameterType).ToArray());
        }

        public ParameterExpression Param { get { return writer; } }

        public Expression WriteStructBegin(Expression metadata)
        {
            return Expression.Call(writer, structBegin, metadata);
        }

        public Expression WriteBaseBegin(Expression metadata)
        {
            return Expression.Call(writer, baseBegin, metadata);
        }

        public Expression WriteStructEnd()
        {
            return Expression.Call(writer, structEnd);
        }

        public Expression WriteBaseEnd()
        {
            return Expression.Call(writer, baseEnd);
        }

        public Expression WriteFieldBegin(Expression type, ushort id, Metadata metadata)
        {
            return WriteFieldBegin(type, Expression.Constant(id), Expression.Constant(metadata));
        }

        public Expression WriteFieldBegin(Expression type, Expression id, Expression metadata)
        {
            return Expression.Call(writer, fieldBegin, type, id, metadata);
        }

        public Expression WriteFieldOmitted(CdrcsDataType type, ushort id, Metadata metadata)
        {
            return Expression.Call(writer, fieldOmitted, Expression.Constant(type), Expression.Constant(id), Expression.Constant(metadata));
        }
        
        public Expression WriteFieldEnd()
        {
            return Expression.Call(writer, fieldEnd);
        }

        public Expression WriteContainerBegin(Expression count, Expression valueType)
        {
            return Expression.Call(writer, containerBegin, count, valueType);
        }

        public Expression WriteContainerBegin(Expression count, Expression keyType, Expression valueType)
        {
            return Expression.Call(writer, containerBegin2, count, keyType, valueType);
        }

        public Expression WriteContainerEnd()
        {
            return Expression.Call(writer, containerEnd);
        }

        public Expression WriteItemBegin()
        {
            if (itemBegin != null)
                return Expression.Call(writer, itemBegin);
            else
                return Expression.Empty();
        }

        public Expression WriteItemEnd()
        {
            if (itemEnd != null)
                return Expression.Call(writer, itemEnd);
            else
                return Expression.Empty();
        }

        public Expression Write(Expression value, CdrcsDataType type)
        {
            var writeMethod = write[type];
            Debug.Assert(writeMethod != null);
            var writeType = writeMethod.GetParameters()[0].ParameterType;

            if (value.Type == writeType)
            {
                return Expression.Call(writer, writeMethod, value);
            }
            else
            {
                return Expression.Call(writer, writeMethod, Expression.Convert(value, writeType));
            }
        }

        public Expression WriteBytes(Expression data)
        {
            return Expression.Call(writer, writeBytes, data);
        }

        public Expression WriteCdrcsed(Expression value)
        {
/*            if (!untaggedProtocol) 
                return Expression.Call(value, serializeCdrcsed, writer);
*/            
            var data = Expression.Variable(typeof (ArraySegment<byte>), "data");
            return Expression.Block(
                new [] { data },
                Expression.Assign(data, Expression.Call(marshalCdrcsed, value)),
                Write(Expression.Property(data, "Count"), CdrcsDataType.BT_UINT32),
                WriteBytes(data));
        }
    }
}
