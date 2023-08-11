// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cdrcs.Expressions
{
    using System.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using Cdrcs.Protocols;
    using Cdrcs.Internal.Reflection;

    internal class UntaggedReader<R>
    {
        static readonly MethodInfo unmarshalCdrcsed = Reflection.MethodInfoOf(() => Unmarshal.From(default(ArraySegment<byte>)));
        static readonly MethodInfo fieldOmitted    = GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.ReadFieldOmitted()));
        static readonly MethodInfo containerBegin =  GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.ReadContainerBegin()));
        static readonly MethodInfo containerEnd =    GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.ReadContainerEnd()));
        static readonly MethodInfo readBytes =       GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.ReadBytes(default(int))));
        static readonly MethodInfo skipBytes =       GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.SkipBytes(default(int))));

        static readonly Dictionary<CdrcsDataType, MethodInfo> read = new Dictionary<CdrcsDataType, MethodInfo>
            {
                { CdrcsDataType.BT_BOOL,    GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.ReadBool())) },
                { CdrcsDataType.BT_UINT8,   GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.ReadUInt8())) },
                { CdrcsDataType.BT_UINT16,  GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.ReadUInt16())) },
                { CdrcsDataType.BT_UINT32,  GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.ReadUInt32())) },
                { CdrcsDataType.BT_UINT64,  GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.ReadUInt64())) },
                { CdrcsDataType.BT_FLOAT,   GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.ReadFloat())) },
                { CdrcsDataType.BT_DOUBLE,  GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.ReadDouble())) },
                { CdrcsDataType.BT_STRING,  GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.ReadString())) },
                { CdrcsDataType.BT_INT8,    GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.ReadInt8())) },
                { CdrcsDataType.BT_INT16,   GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.ReadInt16())) },
                { CdrcsDataType.BT_INT32,   GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.ReadInt32())) },
                { CdrcsDataType.BT_INT64,   GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.ReadInt64())) },
                { CdrcsDataType.BT_WSTRING, GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.ReadWString())) },
                { CdrcsDataType.BT_ENUM,    GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.ReadEnum())) },
            };

        static readonly Dictionary<CdrcsDataType, MethodInfo> skip = new Dictionary<CdrcsDataType, MethodInfo>
            {
                { CdrcsDataType.BT_BOOL,    GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.SkipBool())) },
                { CdrcsDataType.BT_UINT8,   GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.SkipUInt8())) },
                { CdrcsDataType.BT_UINT16,  GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.SkipUInt16())) },
                { CdrcsDataType.BT_UINT32,  GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.SkipUInt32())) },
                { CdrcsDataType.BT_UINT64,  GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.SkipUInt64())) },
                { CdrcsDataType.BT_FLOAT,   GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.SkipFloat())) },
                { CdrcsDataType.BT_DOUBLE,  GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.SkipDouble())) },
                { CdrcsDataType.BT_STRING,  GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.SkipString())) },
                { CdrcsDataType.BT_INT8,    GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.SkipInt8())) },
                { CdrcsDataType.BT_INT16,   GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.SkipInt16())) },
                { CdrcsDataType.BT_INT32,   GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.SkipInt32())) },
                { CdrcsDataType.BT_INT64,   GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.SkipInt64())) },
                { CdrcsDataType.BT_WSTRING, GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.SkipWString())) },
                { CdrcsDataType.BT_ENUM,    GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.SkipEnum())) },
            };

        static MethodInfo GetMethod(MethodInfo method)
        {
            // There is a method (sic!) to this madness. We need to get a method of type R, not method of the 
            // interface. Only this way the calls to methods of protocols that are implemented as a value types 
            // will be inlined by JIT. Inlining makes a big difference for performance.
            return typeof(R).FindMethod(method.Name, method.GetParameters().Select(p => p.ParameterType).ToArray());
        }

        readonly ParameterExpression reader = Expression.Parameter(typeof(R), "reader");

        public ParameterExpression Param { get { return reader; } }


        public Expression ReadFieldOmitted()
        {
            return Expression.Call(reader, fieldOmitted);
        }

        public Expression ReadContainerBegin()
        {
            return Expression.Call(reader, containerBegin);
        }

        public Expression ReadContainerEnd()
        {
            return Expression.Call(reader, containerEnd);
        }

        public Expression Read(CdrcsDataType type)
        {
            return Expression.Call(reader, read[type]);
        }

        public Expression Skip(CdrcsDataType type)
        {
            return Expression.Call(reader, skip[type]);
        }

        public Expression ReadBytes(Expression count)
        {
            return Expression.Call(reader, readBytes, count);
        }

        public Expression SkipBytes(Expression count)
        {
            return Expression.Call(reader, skipBytes, count);
        }

        public Expression ReadMarshaledCdrcsed()
        {
            return Expression.Call(unmarshalCdrcsed, 
                ReadBytes(Expression.Convert(Read(CdrcsDataType.BT_UINT32), typeof(int))));
        }
    }
}
