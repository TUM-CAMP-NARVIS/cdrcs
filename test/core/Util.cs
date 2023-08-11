namespace UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;
    using RMarshal = System.Runtime.InteropServices.Marshal;
    using NUnit.Framework;
    using Cdrcs;
    using Cdrcs.Protocols;
    using Cdrcs.IO.Safe;
    using Marshal = Cdrcs.Marshal;

    public class BufferHolder
    {
        public byte[] buffer;
    }

    public static class Util
    {
        private const int UnsafeBufferSize = 4 * 1024 * 1024;

        public static IEnumerable<MethodInfo> GetDeclaredMethods(this Type type, string name)
        {
            return type.GetMethods().Where(m => m.Name == name);
        }

        public static MethodInfo GetMethod(this Type type, string name, params Type[] paramTypes)
        {
            var methods = type.GetDeclaredMethods(name);

            return (
                from method in methods
                let parameters = method.GetParameters()
                where parameters != null
                where parameters.Select(p => p.ParameterType).Where(t => !t.IsGenericParameter).SequenceEqual(paramTypes)
                select method).FirstOrDefault();
        }

        public static void TranscodeCDRCDR(BufferHolder from, BufferHolder to)
        {
            var input = new InputBuffer(from.buffer, 11);
            var reader = new DdsCdrReader<InputBuffer>(input);

            var output = new OutputBuffer();
            var writer = new DdsCdrWriter<OutputBuffer>(output);

            Transcode.FromTo(reader, writer);
            /*output.Flush();*/
            to.buffer = output.Data.ToArray<byte>();
        }


        // DdsCdr tests:

        public static void SerializeCDR<T>(T obj, BufferHolder stream)
        {
            var output = new OutputBuffer();
            var writer = new DdsCdrWriter<OutputBuffer>(output);

            Serialize.To(writer, obj);
            /*output.Flush();*/
            stream.buffer = output.Data.ToArray<byte>();
            Console.WriteLine("Buffer: " + stream.buffer.ToString());
            Console.Out.Flush();
        }

        public static void MarshalCDR<T>(T obj, BufferHolder stream)
        {
            var output = new OutputBuffer();
            var writer = new DdsCdrWriter<OutputBuffer>(output);

            Marshal.To(writer, obj);
            /*output.Flush();*/
            stream.buffer = output.Data.ToArray<byte>();

        }

        public static void SerializerMarshalCDR<T>(T obj, BufferHolder  stream)
        {
            var output = new OutputBuffer();
            var writer = new DdsCdrWriter<OutputBuffer>(output);
            var serializer = new Serializer<DdsCdrWriter<OutputBuffer>>(typeof(T));
            serializer.Marshal(obj, writer);
            /*output.Flush();*/
            stream.buffer = output.Data.ToArray<byte>();
        }

        public static ArraySegment<byte> MarshalCDR<T>(T obj)
        {
            var output = new OutputBuffer(new byte[11]);
            var writer = new DdsCdrWriter<OutputBuffer>(output);

            Marshal.To(writer, obj);
            return output.Data;
        }

        public static void SerializeCDR<T>(ICdrcsed<T> obj, BufferHolder stream)
        {
            var output = new OutputBuffer();
            var writer = new DdsCdrWriter<OutputBuffer>(output);

            Serialize.To(writer, obj);
            /*output.Flush();*/
            stream.buffer = output.Data.ToArray<byte>();
        }

        public static void SerializeCDR(ICdrcsed obj, BufferHolder stream)
        {
            var output = new OutputBuffer();
            var writer = new DdsCdrWriter<OutputBuffer>(output);

            Serialize.To(writer, obj);
            /*output.Flush();*/
            stream.buffer = output.Data.ToArray<byte>();
        }

        /*        public static ArraySegment<byte> SerializeUnsafeCDR<T>(T obj)
                {
                    var output = new OutputBuffer();
                    var writer = new CompactBinaryWriter<OutputBuffer>(output);

                    Serialize.To(writer, obj);
                    return output.Data;
                }

                public static IntPtr SerializePointerCDR<T>(T obj, IntPtr ptr, int length)
                {
                    var output = new OutputPointer(ptr, length);
                    var writer = new CompactBinaryWriter<OutputPointer>(output);

                    Serialize.To(writer, obj);
                    return output.Data;
                }
        */
        public static ArraySegment<byte> SerializeSafeCDR<T>(T obj)
        {
            var output = new Cdrcs.IO.Safe.OutputBuffer(new byte[11]);
            var writer = new DdsCdrWriter<Cdrcs.IO.Safe.OutputBuffer>(output);

            Serialize.To(writer, obj);
            return output.Data;
        }

        public static ArraySegment<byte> SerializeSafeCDRNoInlining<T>(T obj)
        {
            var output = new Cdrcs.IO.Safe.OutputBuffer(new byte[11]);
            var writer = new DdsCdrWriter<Cdrcs.IO.Safe.OutputBuffer>(output);

            var serializer = new Serializer<DdsCdrWriter<Cdrcs.IO.Safe.OutputBuffer>>(typeof(T), false);
            serializer.Serialize(obj, writer);
            return output.Data;
        }

        public static T DeserializeCDR<T>(BufferHolder stream)
        {
            var input = new InputBuffer(stream.buffer);
            var reader = new DdsCdrReader<InputBuffer>(input);

            return Deserialize<T>.From(reader);
        }

        public static T DeserializeSafeCDR<T>(ArraySegment<byte> data)
        {
            var input = new Cdrcs.IO.Safe.InputBuffer(data);
            var reader = new DdsCdrReader<Cdrcs.IO.Safe.InputBuffer>(input);

            return Deserialize<T>.From(reader);
        }
        /*
                public static T DeserializeUnsafeCDR<T>(ArraySegment<byte> data)
                {
                    var input = new InputBuffer(data);
                    var reader = new CompactBinaryReader<InputBuffer>(input);

                    return Deserialize<T>.From(reader);
                }

                public static T DeserializePointerCDR<T>(IntPtr data, int length)
                {
                    var input = new InputPointer(data, length);
                    var reader = new CompactBinaryReader<InputPointer>(input);

                    return Deserialize<T>.From(reader);
                }
        */

        public static string SerializeXmlString<T>(T obj)
        {
            var builder = new StringBuilder();
            var writer = new SimpleXmlWriter(XmlWriter.Create(builder, new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true }));
            Serialize.To(writer, obj);
            writer.Flush();
            return builder.ToString();
        }




        /*        public static ICdrcsed<T> MakeCdrcsedCDR<T>(T obj)
                {
                    byte[] stream = { };
                    SerializeCDR(obj, stream);
                    *//*stream.Position = 0;*//*
                    // Create new MemoryStream at non-zero offset in a buffer
                    var buffer = new byte[stream.Length + 2];
                    stream.Read(buffer, 1, buffer.Length - 2);
                    var input = new InputBuffer(new MemoryStream(buffer, 1, buffer.Length - 2, false, true));
                    var reader = new DdsCdrReader<InputStream>(input);
                    return new Cdrcsed<T, DdsCdrReader<InputStream>>(reader);
                }
        */

        public delegate void RoundtripStream<From, To>(Action<From, BufferHolder> serialize, Func<BufferHolder, To> deserialize);
        public delegate void MarshalStream<From>(Action<From, BufferHolder> serialize);
        public delegate void MarshalMemory<From>(Func<From, ArraySegment<byte>> serialize);
        public delegate void TranscodeStream<From, To>(Action<From, BufferHolder> serialize, Action<BufferHolder, BufferHolder> transcode, Func<BufferHolder, To> deserialize);
/*        public delegate void RoundtripMemory<From, To>(Func<From, ArraySegment<byte>> serialize, Func<ArraySegment<byte>, To> deserialize);
        public delegate void RoundtripPointer<From, To>(Func<From, IntPtr, int, IntPtr> serialize, Func<IntPtr, int, To> deserialize);
        public delegate void RoundtripMemoryPointer<From, To>(Func<From, ArraySegment<byte>> serialize, Func<IntPtr, int, To> deserialize);
*/
        public static void AllSerializeDeserialize<From, To>(From from, bool noTranscoding = false)
            where From : class
            where To : class
        {
/*            RoundtripMemory<From, To> memoryRoundtrip = (serialize, deserialize) =>
            {
                var data = serialize(from);
                var to = deserialize(data);
                Assert.IsTrue(from.IsEqual(to));
            };

            RoundtripPointer<From, To> pointerRoundtrip = (serialize, deserialize) =>
            {
                var ptr = RMarshal.AllocHGlobal(UnsafeBufferSize);
                var data = serialize(from, ptr, UnsafeBufferSize);
                var to = deserialize(data, UnsafeBufferSize);
                Assert.IsTrue(from.IsEqual(to));
                RMarshal.FreeHGlobal(data);
            };

            RoundtripMemoryPointer<From, To> memoryPointerRoundtrip = (serialize, deserialize) =>
            {
                var data = serialize(from);
                var pinned = GCHandle.Alloc(data.Array, GCHandleType.Pinned);
                var to = deserialize(RMarshal.UnsafeAddrOfPinnedArrayElement(data.Array, data.Offset), data.Count);
                Assert.IsTrue(from.IsEqual(to));
                pinned.Free();
            };*/

            RoundtripStream<From, To> streamRoundtrip = (serialize, deserialize) =>
            {
                BufferHolder stream = new BufferHolder {
                    buffer = new byte[11]
                };
                
                serialize(from, stream);
                /*stream.Position = 0;*/
                var to = deserialize(stream);

                Assert.IsTrue(from.IsEqual(to));
            };

            MarshalStream<From> streamMarshal = serialize => streamRoundtrip(serialize, stream =>
            {
                /*stream.Position = 0;*/
                return Unmarshal<To>.From(new InputBuffer(stream.buffer));
            });

            MarshalStream<From> streamMarshalSchema = serialize => streamRoundtrip(serialize, stream =>
            {
                /*stream.Position = 0;*/
                return Unmarshal.From(new InputBuffer(stream.buffer), Schema<From>.RuntimeSchema).Deserialize<To>();
            });

            MarshalStream<From> streamMarshalNoSchema = serialize => streamRoundtrip(serialize, stream =>
            {
                /*stream.Position = 0;*/
                return Unmarshal.From(new InputBuffer(stream.buffer)).Deserialize<To>();
            });

/*            MarshalMemory<From> memoryMarshal = serialize => memoryRoundtrip(serialize, Unmarshal<To>.From);
*/
            TranscodeStream<From, To> streamTranscode = (serialize, transcode, deserialize) => 
                streamRoundtrip((obj, stream) =>
                {
                    BufferHolder tmp = new BufferHolder
                    {
                        buffer = new byte[11]
                    };
                    {
                        serialize(obj, tmp);
                        /*tmp.Position = 0;*/
                        transcode(tmp, stream);
                    }
                }, deserialize);

            if (noTranscoding)
                streamTranscode = (serialize, transcode, deserialize) => { };

            // Compact Binary
            streamRoundtrip(SerializeCDR, DeserializeCDR<To>);
/*
            memoryRoundtrip(SerializeUnsafeCB, DeserializeSafeCB<To>);
            memoryRoundtrip(SerializeUnsafeCB, DeserializeUnsafeCB<To>);
            memoryPointerRoundtrip(SerializeUnsafeCB, DeserializePointerCB<To>);
            pointerRoundtrip(SerializePointerCB, DeserializePointerCB<To>);
            memoryRoundtrip(SerializeSafeCB, DeserializeSafeCB<To>);
            memoryRoundtrip(SerializeSafeCB, DeserializeUnsafeCB<To>);
            memoryPointerRoundtrip(SerializeSafeCB, DeserializePointerCB<To>);
            memoryRoundtrip(SerializeSafeCBNoInlining, DeserializeSafeCB<To>);
            streamMarshal(MarshalCDR);
            streamMarshal(SerializerMarshalCDR);
            streamMarshalSchema(MarshalCDR);
            streamMarshalNoSchema(MarshalCDR);
            memoryMarshal(MarshalCB);
*/


            /*streamTranscode(SerializeCDR, TranscodeCDRCDR, DeserializeCDR<To>);*/

        }

        delegate bool TypePredicate(Type field);

        static bool AnyField<T>(TypePredicate predicate)
        {
            var types = new HashSet<Type>();
            return AnyField(types, typeof(T), predicate);
        }

        static bool AnyField(HashSet<Type> types, Type type, TypePredicate predicate)
        {
            types.Add(type);
            return type.GetSchemaFields()
                .Select(field => field.MemberType)
                .Where(fieldType => !types.Contains(fieldType))
                .Any(fieldType => predicate(fieldType) || fieldType.IsCdrcsStruct() && AnyField(types, fieldType, predicate));
        }
    }
}
