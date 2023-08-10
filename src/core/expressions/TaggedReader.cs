// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cdrcs.Expressions
{
    using System.Linq;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Reflection;
    using Cdrcs.Protocols;
    using Cdrcs.Internal.Reflection;

    internal class TaggedReader<R>
    {
        readonly ParameterExpression reader = Expression.Parameter(typeof(R), "reader");

        static readonly MethodInfo structBegin =     GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.ReadStructBegin()));
        static readonly MethodInfo baseBegin =       GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.ReadBaseBegin()));
        static readonly MethodInfo structEnd =       GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.ReadStructEnd()));
        static readonly MethodInfo baseEnd =         GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.ReadBaseEnd()));
        static readonly MethodInfo fieldBegin =      GetMethod("ReadFieldBegin", typeof(CdrcsDataType).MakeByRefType(), typeof(UInt16).MakeByRefType());
        static readonly MethodInfo fieldEnd =        GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.ReadFieldEnd()));
        static readonly MethodInfo containerBegin =  GetMethod("ReadContainerBegin", typeof(int).MakeByRefType(), typeof(CdrcsDataType).MakeByRefType());
        static readonly MethodInfo containerBegin2 = GetMethod("ReadContainerBegin", typeof(int).MakeByRefType(), typeof(CdrcsDataType).MakeByRefType(), typeof(CdrcsDataType).MakeByRefType());
        static readonly MethodInfo containerEnd =    GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.ReadContainerEnd()));
        static readonly MethodInfo readBytes =       GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.ReadBytes(default(int))));
        static readonly MethodInfo skip =            GetMethod(Reflection.MethodInfoOf((IProtocolReader reader) => reader.Skip(default(CdrcsDataType))));

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

        static MethodInfo GetMethod(MethodInfo method)
        {
            // There is a method (sic!) to this madness. We need to get a method of type R, not method of the 
            // interface. Only this way the calls to methods of protocols that are implemented as a value types 
            // will be inlined by JIT. Inlining makes a big difference for performance.
            return typeof(R).FindMethod(method.Name, method.GetParameters().Select(p => p.ParameterType).ToArray());
        }
        
        static MethodInfo GetMethod(string name, params Type[] paramTypes)
        {
            var result = typeof(R).FindMethod(name, paramTypes);
            Debug.Assert(result != null);
            return result;
        }

        public ParameterExpression Param { get { return reader; } }

        public Expression ReadStructBegin()
        {
            return Expression.Call(reader, structBegin);
        }

        public Expression ReadBaseBegin()
        {
            return Expression.Call(reader, baseBegin);
        }

        public Expression ReadStructEnd()
        {
            return Expression.Call(reader, structEnd);
        }

        public Expression ReadBaseEnd()
        {
            return Expression.Call(reader, baseEnd);
        }

        public Expression ReadFieldBegin(Expression type, Expression id)
        {
            return Expression.Call(reader, fieldBegin, type, id);
        }

        public Expression ReadFieldEnd()
        {
            return Expression.Call(reader, fieldEnd);
        }

        public Expression ReadContainerBegin(Expression count, Expression type)
        {
            return Expression.Call(reader, containerBegin, count, type);
        }

        public Expression ReadContainerBegin(Expression count, Expression keyType, Expression valueType)
        {
            return Expression.Call(reader, containerBegin2, count, keyType, valueType);
        }

        public Expression ReadContainerEnd()
        {
            return Expression.Call(reader, containerEnd);
        }

        public Expression Read(CdrcsDataType type)
        {
            return Expression.Call(reader, read[type]);
        }
                
        public Expression Skip(Expression type)
        {
            return Expression.Call(reader, skip, type);
        }

        public Expression ReadBytes(Expression count)
        {
            return Expression.Call(reader, readBytes, count);
        }
    }
}
