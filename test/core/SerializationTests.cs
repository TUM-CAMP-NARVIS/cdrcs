﻿namespace UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Cdrcs;
    using NUnit.Framework;
    using UnitTestSamples;

    // Name conflicts with Cdrcs.Tag
    public abstract class wstring { }
    public abstract class blob { }
    public abstract class nullable<T> { }
    public abstract class bonded<T> { }

    public static class CdrcsTypeAliasConverter
    {
        public static long Convert(DateTime value, long unused)
        {
            return value.Ticks;
        }

        public static DateTime Convert(long value, DateTime unused)
        {
            if (value >= DateTime.MinValue.Ticks && value <= DateTime.MaxValue.Ticks)
                return new DateTime(value);

            return default(DateTime);
        }
    }

    [TestFixture]
    public class SerializationTests
    {
        [Test]
        public void BasicTypes()
        {
            TestSerialization<BasicTypes>();
        }

        [Test]
        public void IntegerLimits()
        {
            Util.AllSerializeDeserialize<Integers, Integers>(new Integers 
            {
                _int8 = sbyte.MaxValue, 
                _int16 = short.MaxValue,
                _int32 = int.MaxValue,
                _int64 = long.MaxValue,
                _uint8 = byte.MaxValue,
                _uint16 = ushort.MaxValue,
                _uint32 = uint.MaxValue,
                // Note: not ulong.MaxValue because NewtonSoft JSON doesn't support it in portable profile
                _uint64 = long.MaxValue
            });

            Util.AllSerializeDeserialize<Integers, Integers>(new Integers 
            {
                _int8 = sbyte.MinValue,
                _int16 = short.MinValue,
                _int32 = int.MinValue,
                _int64 = long.MinValue,
                _uint8 = byte.MinValue,
                _uint16 = ushort.MinValue,
                _uint32 = uint.MinValue,
                _uint64 = ulong.MinValue
            });

            Util.AllSerializeDeserialize<MaxUInt64, MaxUInt64>(new MaxUInt64
            {
                _uint64 = ulong.MaxValue
            });
        }

/*        [Test]
        public void ReadonlyBasicTypes()
        {
            TestSerialization<Readonly.BasicTypes>();
        }

        [Test]
        public void ReadonlySimpleContainers()
        {
            TestSerialization<Readonly.SimpleContainers>();
        }
*/
        [Test]
        public void Nested()
        {
            TestSerialization<Nested>();
        }

        [Test]
        public void Void()
        {
            TestSerialization<Cdrcs.Void>();
        }

        [Test]
        public void Lists()
        {
            TestSerialization<Lists>();
        }

        [Test]
        public void Vectors()
        {
            TestSerialization<Vectors>();
        }

        [Test]
        public void Sets()
        {
            TestSerialization<Sets>();
        }

        [Test]
        public void Maps()
        {
            TestSerialization<Maps>();
        }

        [Test]
        public void Containers()
        {
            TestSerialization<CdrcsClass<SortedSet<string>>>();
            TestSerialization<Containers>();
        }

        [Test]
        public void NestedContainers()
        {
            TestSerialization<NestedContainers>();
        }

        [Test]
        public void Inheritance()
        {
            TestSerialization<Deep>();
            TestSerialization<Derived>();
        }

        [Test]
        public void Inheritance_SliceToBase()
        {
            TestSerialization<Derived, Nested>();
            TestSerialization<Derived, EmptyBase>();
        }

        [Test]
        public void Inheritance_SliceFieldToBase()
        {
            TestFieldSerialization<EmptyBase, Nested>();
            /*TestFieldSerialization<DerivedView, EmptyBase>();*/
        }

/*        [Test]
        public void Nothing()
        {
            TestSerialization<Nothing>();
            TestSerialization<NotNothingView, Nothing>();
        }
*/        
        [Test]
        public void NullableBasicTypes()
        {
            TestSerialization<NullableBasicTypes>();
        }

        [Test]
        public void NullableStruct()
        {
            TestSerialization<NullableStruct>();
        }

        public void TypeMismatch<From, To>()
        {
            Util.RoundtripStream<CdrcsClass<From>, CdrcsClass<To>> test = (serialize, deserialize) =>
            {
                try
                {
                    byte[] stream = { };
                    serialize(new CdrcsClass<From>(), stream);
                    /*stream.Position = 0;*/
                    deserialize(stream);
                    Assert.Fail("Deserialization of mismatched type didn't throw exception.");
                }
                catch (InvalidDataException)
                {}
            };

            // TODO: for untagged protocol mismatch schema will be detected in schema validation
            test(Util.SerializeCDR, Util.DeserializeCDR<CdrcsClass<To>>);
        }

        [Test]
        public void TypeMismatch()
        {
            TypeMismatch<List<int>, List<HashSet<int>>>();
            TypeMismatch<List<int>, HashSet<int>>();
            TypeMismatch<List<int>, int>();
            TypeMismatch<List<int>, BasicTypes>();
            TypeMismatch<BasicTypes, List<int>>();
            TypeMismatch<BasicTypes, double>();
            TypeMismatch<BasicTypes, HashSet<int>>();
            TypeMismatch<HashSet<int>, HashSet<string>>();
            TypeMismatch<HashSet<int>, List<float>>();
            TypeMismatch<HashSet<int>, BasicTypes>();
            TypeMismatch<Dictionary<int, int>, Dictionary<int, string>>();
            TypeMismatch<Dictionary<int, int>, Dictionary<string, int>>();
            TypeMismatch<Dictionary<int, int>, HashSet<string>>();
        }

        [Test]
        public void ContainersOfNullable()
        {
            TestSerialization<ContainersOfNullable>();

            var from = new ContainersOfNullable
            {
                vn = new VectorsOfNullable
                {
                    _bool = {false, null, true},
                    _double = {null, null},
                    _str = {"foo", null},
                    _wstr = {null, "bar"},
                    basic = {null},
                    _uint16 = {0, null, null, 1},
                    _blob =
                    {
                        default(ArraySegment<byte>), 
                        new ArraySegment<byte>(new byte[] { 0, 1 }), 
                        default(ArraySegment<byte>)
                    }
                },
                mn = new MapsOfNullable
                {
                    _basic = {{"foo", null}},
                    _double = {{"bar", 3.14}, {"foo", null}},
                    _float = {{"bar", null}, {"foo", 3.14f}},
                    _blob =
                    {
                        {"bar", default(ArraySegment<byte>)}, 
                        {"foo", new ArraySegment<byte>(new byte[] { 0, 1 })},
                        {"bux", default(ArraySegment<byte>) }
                    }
                }
            };

            Util.AllSerializeDeserialize<ContainersOfNullable, ContainersOfNullable>(from);
        }
        
        [Test]
        public void NullableContainers()
        {
            TestSerialization<NullableContainers>();
        }

        [Test]
        public void Blobs()
        {
            TestSerialization<StructWithBlobs>();
            TestSerialization<StructWithBlobs, StructWithByteLists>();
            TestSerialization<StructWithByteLists, StructWithBlobs>();
        }

        [Test]
        public void Recursive()
        {
            TestSerialization<Tree>();
        }

        [Test]
        public void TypePromotion()
        {
            TestTypePromotion<byte, UInt16>();
            TestTypePromotion<byte, UInt32>();
            TestTypePromotion<byte, UInt64>();
            TestTypePromotion<UInt16, UInt32>();
            TestTypePromotion<UInt16, UInt64>();
            TestTypePromotion<UInt32, UInt64>();
            TestTypePromotion<sbyte, Int16>();
            TestTypePromotion<sbyte, Int32>();
            TestTypePromotion<sbyte, Int64>();
            TestTypePromotion<Int16, Int32>();
            TestTypePromotion<Int16, Int64>();
            TestTypePromotion<Int32, Int64>();
            TestTypePromotion<float, double>();

            TestTypePromotion<byte, byte?>();
            TestTypePromotion<Int16, Int16?>();
            TestTypePromotion<Int32, Int32?>();
        }

/*        [Test]
        public void Views()
        {
            TestSerialization<BasicTypes, BasicTypesView>();
            TestFieldSerialization<BasicTypes, BasicTypesView>();
        }
*/

        [Test]
        public void Omit()
        {
            // Omit optional fields set to default value
            TestPayloadSize(Util.SerializeCDR, Util.TranscodeCDRCDR, new StructWithDefaults(), 1);
            TestPayloadSize(Util.SerializeCDR, Util.TranscodeCDRCDR, new ContainersOfNullable(), 5);
            TestPayloadSize(Util.SerializeCDR, Util.TranscodeCDRCDR, new NullableVectors(), 1);
            TestPayloadSize(Util.SerializeCDR, Util.TranscodeCDRCDR, new NullableLists(), 1);
            TestPayloadSize(Util.SerializeCDR, Util.TranscodeCDRCDR, new NullableBasicTypes(), 1);
            TestPayloadSize(Util.SerializeCDR, Util.TranscodeCDRCDR, new Lists(), 1);
            TestPayloadSize(Util.SerializeCDR, Util.TranscodeCDRCDR, new Vectors(), 1);
            TestPayloadSize(Util.SerializeCDR, Util.TranscodeCDRCDR, new Sets(), 1);
            TestPayloadSize(Util.SerializeCDR, Util.TranscodeCDRCDR, new Maps(), 1);
            TestPayloadSize(Util.SerializeCDR, Util.TranscodeCDRCDR, new StructWithBlobs(), 1);
            /*TestPayloadSize(Util.SerializeCDR, Util.TranscodeCDRCDR, new Nothing(), 1);*/
            
            // Don't skip empty container for field with default nothing
/*            TestPayloadSize(Util.SerializeCDR, Util.TranscodeCDRCDR, new Nothing { b = new ArraySegment<byte>(new byte[0]) }, 5);
            TestPayloadSize(Util.SerializeCDR, Util.TranscodeCDRCDR, new Nothing { l = new LinkedList<string>() }, 5);
            TestPayloadSize(Util.SerializeCDR, Util.TranscodeCDRCDR, new Nothing { s = new HashSet<double>() }, 5);
            TestPayloadSize(Util.SerializeCDR, Util.TranscodeCDRCDR, new Nothing { m = new Dictionary<string, double>() }, 6);
*/            
            // Don't omit required fields
            TestPayloadSize(Util.SerializeCDR, Util.TranscodeCDRCDR, new CdrcsClass<Int32>(), 3);

            // Don't omit required_optional fields
            TestPayloadSize(Util.SerializeCDR, Util.TranscodeCDRCDR, new RequiredOptional(), 4);

        }

        [Test]
        public void Skip()
        {
            TestFieldSkip<bool>();
            TestFieldSkip<byte>();
            TestFieldSkip<UInt16>();
            TestFieldSkip<UInt32>();
            TestFieldSkip<UInt64>();
            TestFieldSkip<sbyte>();
            TestFieldSkip<Int16>();
            TestFieldSkip<Int32>();
            TestFieldSkip<Int64>();
            TestFieldSkip<float>();
            TestFieldSkip<double>();
            TestFieldSkip<string>();

            TestFieldSkip<BasicTypes>();
            TestFieldSkip<Nested>();
            TestFieldSkip<Containers>();
            TestFieldSkip<NestedContainers>();
            TestFieldSkip<Derived>();
            TestFieldSkip<StructWithBlobs>();
            TestFieldSkip<Tree>();
        }

        void TestRequired<From, To>()
        {
            var from = Random.Init<From>();
            Util.RoundtripStream<From, To> testRequired = (serialize, deserialize) =>
            {
                byte[] stream = { };

                serialize(from, stream);
                /*stream.Position = 0;*/
                
                try
                {
                    deserialize(stream);
                    Assert.Fail("Missing required field not detected.");
                }
                catch (InvalidDataException e)
                {
                    Assert.IsTrue(e.Message.Contains("Required field"), e.Message);
                }
            };

            testRequired(Util.SerializeCDR, Util.DeserializeCDR<To>);
            // TODO: tagged protocol determine missing required fields during schema valiadation
            //testRequired(Util.SerializeSP, Util.DeserializeSP<From, To>);
        }

        [Test]
        public void Required()
        {
            TestSerialization<Required>();
            TestSerialization<RequiredInDerived>();
            TestSerialization<RequiredInBase>();
            TestSerialization<RequiredInBaseAndDerived>();
            TestSerialization<Optional, RequiredOptional>();
            TestRequired<CdrcsClass<double>, CdrcsClass<int, double>>();
            TestRequired<CdrcsClass<double>, CdrcsClass<BasicTypes, double>>();
        }

        [Test]
        public void Box()
        {
            TestSerialization<Cdrcs.Box<bool>>();
            TestSerialization<Cdrcs.Box<sbyte>>();
            TestSerialization<Cdrcs.Box<short>>();
            TestSerialization<Cdrcs.Box<int>>();
            TestSerialization<Cdrcs.Box<long>>();
            TestSerialization<Cdrcs.Box<byte>>();
            TestSerialization<Cdrcs.Box<ushort>>();
            TestSerialization<Cdrcs.Box<uint>>();
            TestSerialization<Cdrcs.Box<ulong>>();
            TestSerialization<Cdrcs.Box<float>>();
            TestSerialization<Cdrcs.Box<double>>();
        }

/*        [Test]
        public void TypeFromFileWithSpaces()
        {
            Assert.IsNotNull(new EnsureSpacesInPathsWork());
        }
*/
  /*      [Test]
        public void ImmutableCollections()
        {
            TestSerialization<ImmutableCollections.ImmutableCollectionsHolder>();
        }
*/
        void TestTypePromotion<From, To>()
        {
            TestFieldSerialization<From, To>();
            TestFieldSerialization<List<From>, List<To>>();
            TestFieldSerialization<HashSet<From>, HashSet<To>>();
            TestFieldSerialization<Dictionary<From, string>, Dictionary<To, string>>();
            TestFieldSerialization<Dictionary<string, From>, Dictionary<string, To>>();
        }

        void TestFieldSkip<T>()
        {
            TestSerialization<CdrcsClass<T, double>, CdrcsClass<double>>(true);
            TestSerialization<CdrcsClass<List<T>, double>, CdrcsClass<double>>(true);
            TestSerialization<CdrcsClass<HashSet<T>, double>, CdrcsClass<double>>(true);
            TestSerialization<CdrcsClass<Dictionary<string, T>, double>, CdrcsClass<double>>(true);
        }

        void TestSerialization<From, To>(bool noTranscoding = false)
            where From : class, new()
            where To : class
        {
            Util.AllSerializeDeserialize<From, To>(new From(), noTranscoding);
            Util.AllSerializeDeserialize<From, To>(Random.Init<From>(), noTranscoding);
        }

        void TestSerialization<T>(bool noTranscoding = false)
            where T : class, new()
        {
            Assert.IsTrue(Comparer.Equal(new T(), new T()));
            TestSerialization<T, T>(noTranscoding);
        }

        void TestPayloadSize<T>(Action<T, byte[]> serialize, Action<byte[], byte[]> transcode, T obj, int size)
        {
            byte[] stream = { };
            
            serialize(obj, stream);
            Assert.AreEqual(size, stream.Length);

            byte[] stream2 = { };
            {
                /*stream.Position = 0;*/
                transcode(stream, stream2);
                Assert.AreEqual(size, stream2.Length);
            }
        }

        void TestFieldSerialization<From, To>()
        {
            TestSerialization<CdrcsClass<From>, CdrcsClass<To>>();

            if (CdrcsClass<From>.TypeId != CdrcsClass<To>.TypeId)
            {
                try
                {
                    TestSerialization<CdrcsClass<To>, CdrcsClass<From>>();
                }
                catch (InvalidDataException e)
                {
                    var expected = string.Format("Invalid type {0}, expected {1}",
                        CdrcsClass<To>.TypeId, CdrcsClass<From>.TypeId);
                    Assert.AreEqual(expected, e.Message);
                    return;
                }

                Assert.IsTrue(false);
            }
        }
    }
}
