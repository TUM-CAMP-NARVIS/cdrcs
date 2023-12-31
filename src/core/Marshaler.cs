// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cdrcs
{
    using System;
    using System.IO;
    using Cdrcs.IO;
    using Cdrcs.IO.Safe;
    using Cdrcs.Protocols;

    /// <summary>
    /// Unmarshal payload of unknown schema
    /// </summary>
    public static class Unmarshal
    {
        /// <summary>
        /// Unmarshal payload into an instance of ICdrcsed
        /// </summary>
        /// <typeparam name="I">Implementation of IInputStream</typeparam>
        /// <param name="input">Input stream with the payload</param>
        /// <returns>ICdrcsed wrapping the input</returns>
        public static ICdrcsed From<I>(I input)
            where I : IInputStream, ICloneable<I>
        {
            return From(input, RuntimeSchema.Empty);
        }

        /// <summary>
        /// Unmarshal payload with specified schema into an instance of ICdrcsed
        /// </summary>
        /// <typeparam name="I">Implementation of IInputStream</typeparam>
        /// <param name="input">Input stream with the payload</param>
        /// <param name="schema">Runtime schema of the payload</param>
        /// <returns>ICdrcsed wrapping the input</returns>
        public static ICdrcsed From<I>(I input, RuntimeSchema schema)
            where I : IInputStream, ICloneable<I>
        {
            if (input.Length > 0)
            {
                input.ReadEncapsulation();
            }
            // we only support one protocol for now
            var protocol = ProtocolType.DDS_CDR_PROTOCOL;

            switch (protocol)
            {
                case ProtocolType.DDS_CDR_PROTOCOL:
                    return new CdrcsedVoid<DdsCdrReader<I>>(
                        new DdsCdrReader<I>(input), schema);

                default:
                    throw new InvalidDataException(string.Format("Unknown ProtocolType {0}", protocol));
            }
        }

        /// <summary>
        /// Unmarshal payload into an instance of ICdrcsed
        /// </summary>
        /// <param name="data">Byte array containing the payload</param>
        public static ICdrcsed From(ArraySegment<byte> data)
        {
            return From(new InputBuffer(data));
        }

        /// <summary>
        /// Unmarshal payload with specified schema into an instance of ICdrcsed
        /// </summary>
        /// <param name="data">Byte array segment containing the payload</param>
        /// <param name="schema">Runtime schema of the payload</param>
        public static ICdrcsed From(ArraySegment<byte> data, RuntimeSchema schema)
        {
            return From(new InputBuffer(data), schema);
        }
    }

    /// <summary>
    /// Unmarshal an object of type T
    /// </summary>
    /// <typeparam name="T">Type representing a Cdrcs schema</typeparam>
    public static class Unmarshal<T>
    {
        /// <summary>
        /// Unmarshal object of type T from payload
        /// </summary>
        /// <typeparam name="I">Implementation of IInputStream</typeparam>
        /// <param name="input">Input stream</param>
        /// <returns>Unmarshaled object</returns>
        public static T From<I>(I input)
            where I : IInputStream, ICloneable<I>
        {
            if (input.Length > 0) {
                input.ReadEncapsulation();
            }
            // we only support one protocol here for now ..
            var protocol = ProtocolType.DDS_CDR_PROTOCOL;

            switch (protocol)
            {
                case ProtocolType.DDS_CDR_PROTOCOL:
                    return Deserialize<T>.From(new DdsCdrReader<I>(input));
                
                default:
                    throw new InvalidDataException(string.Format("Unknown ProtocolType {0}", protocol));
            }
        }

        /// <summary>
        /// Unmarshal object of type T from payload
        /// </summary>
        /// <param name="data">Byte array segment containing the payload</param>
        /// <returns></returns>
        public static T From(ArraySegment<byte> data)
        {
            return From(new InputBuffer(data));
        }
    }

    /// <summary>
    /// Marshal objects
    /// </summary>
    public static class Marshal
    {
        /// <summary>
        /// Marshal object of type T using protocol writer W
        /// </summary>
        /// <typeparam name="W">Protocol writer</typeparam>
        /// <typeparam name="T">Type representing a Cdrcs schema</typeparam>
        /// <param name="writer">Writer instance</param>
        /// <param name="obj">Object to be marshaled</param>
        public static void To<W, T>(W writer, T obj)
            where W : IProtocolWriter
        {
            writer.WriteEncapsulation();
            Serialize.To(writer, obj);
        }
    }

    public static class Marshaler
    {
        /// <summary>
        /// Marshal object of type T using protocol writer W and a specific Serializer instance.
        /// </summary>
        /// <typeparam name="W">Protocol writer</typeparam>
        /// <param name="serializer">Serializer instance</param>
        /// <param name="writer">Writer instance</param>
        /// <param name="obj">Object to be marshaled</param>
        public static void Marshal<W>(this Serializer<W> serializer, object obj, W writer)
            where W : IProtocolWriter
        {
            writer.WriteEncapsulation();
            serializer.Serialize(obj, writer);
        }

        internal static ArraySegment<byte> Marshal(ICdrcsed bonded)
        {
            var output = new OutputBuffer(4096);
            var writer = new DdsCdrWriter<OutputBuffer>(output);

            writer.WriteEncapsulation();
            bonded.Serialize(writer);
            return output.Data;
        }
    }
}
