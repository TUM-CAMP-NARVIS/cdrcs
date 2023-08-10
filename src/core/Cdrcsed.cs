// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cdrcs
{
    using Cdrcs.IO;

    /// <summary>
    /// Interface representing bonded payload of unknown type
    /// </summary>
    public interface ICdrcsed
    {
        /// <summary>
        /// Serialize content of ICdrcsed instance to protocol writer
        /// </summary>
        /// <typeparam name="W">Type of the protocol writer</typeparam>
        /// <param name="writer">Protocol writer instance</param>
        void Serialize<W>(W writer);

        /// <summary>
        /// Deserialize an object of type U from the ICdrcsed instance
        /// </summary>
        /// <typeparam name="U">Type of object to deserialize</typeparam>
        /// <returns>Deserialized object</returns>
        U Deserialize<U>();

        /// <summary>
        /// Convert to an instance of ICdrcsed&lt;U>
        /// </summary>
        /// <typeparam name="U">Type representing a Cdrcs schema</typeparam>
        /// <returns>An instance of ICdrcsed&lt;U></returns>
        ICdrcsed<U> Convert<U>();
    }

    /// <summary>
    /// Interface representing the schema type bonded&lt;T>
    /// </summary>
    /// <typeparam name="T">Type representing a Cdrcs schema</typeparam>
    public interface ICdrcsed<out T> : ICdrcsed
    {
        /// <summary>
        /// Deserialize an object of type T from the ICdrcsed&lt;T> instance
        /// </summary>
        /// <returns>Deserialized object</returns>
        T Deserialize();
    }

    /// <summary>
    /// Implementation of ICdrcsed&lt;T> holding an instance of T
    /// </summary>
    /// <typeparam name="T">Type representing a Cdrcs schema</typeparam>
    public sealed class Cdrcsed<T> : ICdrcsed<T>
    {
        /// <summary>
        /// A static, readonly field representing an empty instance of Cdrcsed&lt;T>
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Cdrcsed<T> Empty = new Cdrcsed<T>(GenericFactory.Create<T>());
        readonly T instance;

        /// <summary>
        /// Creater Cdrcsed&lt;T> from an instance of T
        /// </summary>
        /// <param name="instance">Object of type T</param>
        public Cdrcsed(T instance)
        {
            this.instance = instance;
        }
        
        T ICdrcsed<T>.Deserialize()
        {
            return Clone<T>.From(instance);
        }

        U ICdrcsed.Deserialize<U>()
        {
            return Clone<U>.From(instance);
        }

        void ICdrcsed.Serialize<W>(W writer)
        {
            Serialize.To(writer, instance);
        }

        ICdrcsed<U> ICdrcsed.Convert<U>()
        {
            return this as ICdrcsed<U>;
        }
    }
    
    /// <summary>
    /// Implementation of ICdrcsed&lt;T> holding data serialized using protocol R
    /// </summary>
    /// <typeparam name="T">Type representing a Cdrcs schema</typeparam>
    /// <typeparam name="R">Protocol reader</typeparam>
    public sealed class Cdrcsed<T, R> : ICdrcsed<T>
        where R : ICloneable<R>
    {
        internal readonly R reader;
        readonly RuntimeSchema schema;

        /// <summary>
        /// Create an instance of Cdrcsed&lt;T, R> from a protocol reader
        /// </summary>
        /// <param name="reader">Protocol reader instance</param>
        public Cdrcsed(R reader)
        {
            this.reader = reader.Clone();
        }

        /// <summary>
        /// Create an instance of Cdrcsed&lt;T, R> from a protocol reader and runtime schema
        /// </summary>
        /// <param name="reader">Protocol reader instance</param>
        /// <param name="schema">Runtime schema of the payload</param>
        public Cdrcsed(R reader, RuntimeSchema schema)
        {
            this.reader = reader.Clone();
            this.schema = schema;
        }

        T ICdrcsed<T>.Deserialize()
        {
            // TODO: This is very bad from performance perspective.
            // It affects only rare cases (untagged payload with a struct deserialized as a bonded<T>)
            // but we need a better solution. The same applies to Deserialize<U> and Serialize<W>.
            if (schema.HasValue)
                return new Deserializer<R>(typeof(T), schema).Deserialize<T>(reader.Clone());
            
            return Deserialize<T>.From(reader.Clone());
        }

        U ICdrcsed.Deserialize<U>()
        {
            if (schema.HasValue)
                return new Deserializer<R>(typeof(U), schema).Deserialize<U>(reader.Clone());
            
            return Deserialize<U>.From(reader.Clone());
        }

        void ICdrcsed.Serialize<W>(W writer)
        {
            if (schema.HasValue)
                new Transcoder<R, W>(schema).Transcode(reader.Clone(), writer);
            else
                Transcode<T>.FromTo(reader.Clone(), writer);
        }

        ICdrcsed<U> ICdrcsed.Convert<U>()
        {
            return new Cdrcsed<U, R>(reader, schema);
        }
    }

    internal class CdrcsedVoid<R> : ICdrcsed 
        where R : ICloneable<R>
    {
        readonly R reader;
        readonly RuntimeSchema schema;

        public CdrcsedVoid(R reader)
        {
            this.reader = reader;
        }

        public CdrcsedVoid(R reader, RuntimeSchema schema)
        {
            this.reader = reader;
            this.schema = schema;
        }

        U ICdrcsed.Deserialize<U>()
        {
            if (schema.HasValue)
                return new Deserializer<R>(typeof(U), schema).Deserialize<U>(reader.Clone());

            return Deserialize<U>.From(reader.Clone());
        }

        void ICdrcsed.Serialize<W>(W writer)
        {
            if (schema.HasValue)
                new Transcoder<R, W>(schema).Transcode(reader.Clone(), writer);
            else
                Transcode.FromTo(reader.Clone(), writer);
        }

        ICdrcsed<U> ICdrcsed.Convert<U>()
        {
            return new Cdrcsed<U, R>(reader, schema);
        }
    }
}
