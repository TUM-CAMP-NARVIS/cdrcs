namespace UnitTest
{
    using System.Collections.Generic;
    using System.IO;
    using Cdrcs;
    using NUnit.Framework;
    using UnitTestSamples;

    [TestFixture]
    public class StructTests
    {
        [Test]
        public void StructWithFields()
        {
            TestStruct<StructWithFields>();
        }

        [Test]
        public void StructWithProperties()
        {
            TestStruct<StructWithProperties>();
        }

        [Test]
        public void NestedStructs()
        {
            TestStruct<NestedStructs>();
        }

        [Test]
        public void ClassWithStructFields()
        {
            TestClass<ClassWithStructFields>();
        }

        [Test]
        public void ClassWithStructProperties()
        {
            TestClass<ClassWithStructProperties>();
        }

        [Test]
        public void CollectionsOfStructs()
        {
            TestClass<CollectionsOfStructs>();
        }

        void TestClass<T>() where T : class
        {
            Util.AllSerializeDeserialize<T, T>(Random.Init<T>());
            TestCloning<T>();
        }

        void TestStruct<T>() where T : struct
        {
            TestSerialization<T>();
            TestCloning<T>();
        }

        void TestSerialization<T>()
        {
            {
                var stream = new BufferHolder { buffer = new byte[11] };
                var from = Random.Init<T>();
                Util.SerializeCDR(from, stream);
                /*stream.Position = 0;*/
                var to = Util.DeserializeCDR<T>(stream);
                Assert.IsTrue(Comparer.Equal(from, to));
            }
        }

        void TestCloning<T>()
        {
            var source = Random.Init<T>();
            var target = Clone<T>.From(source);
            Assert.IsTrue(Comparer.Equal(source, target));
        }
    }

    [global::Cdrcs.Schema]
    public struct StructWithProperties
    {
        [global::Cdrcs.Id(0)]
        public bool _bool { get; set; }

        [global::Cdrcs.Id(2)]
        public string _str { get; set; }

        [global::Cdrcs.Id(3), global::Cdrcs.Type(typeof(global::Cdrcs.Tag.wstring))]
        public string _wstr { get; set; }

        [global::Cdrcs.Id(10)]
        public ulong _uint64 { get; set; }

        [global::Cdrcs.Id(11)]
        public ushort _uint16 { get; set; }

        [global::Cdrcs.Id(12)]
        public uint _uint32 { get; set; }

        [global::Cdrcs.Id(13)]
        public byte _uint8 { get; set; }

        [global::Cdrcs.Id(14)]
        public sbyte _int8 { get; set; }

        [global::Cdrcs.Id(15)]
        public short _int16 { get; set; }

        [global::Cdrcs.Id(16)]
        public int _int32 { get; set; }

        [global::Cdrcs.Id(17)]
        public long _int64 { get; set; }

        [global::Cdrcs.Id(18)]
        public double _double { get; set; }

        [global::Cdrcs.Id(20)]
        public float _float { get; set; }

        [global::Cdrcs.Id(21)]
        public EnumType1 _enum1 { get; set; }

        [global::Cdrcs.Id(22), global::Cdrcs.Type(typeof(long))]
        public System.DateTime dt { get; set; }
    }

    [global::Cdrcs.Schema]
    public struct StructWithFields
    {
        [global::Cdrcs.Id(0)]
        public bool _bool;

        [global::Cdrcs.Id(2)]
        public string _str;

        [global::Cdrcs.Id(3), global::Cdrcs.Type(typeof(global::Cdrcs.Tag.wstring))]
        public string _wstr;

        [global::Cdrcs.Id(10)]
        public ulong _uint64;

        [global::Cdrcs.Id(11)]
        public ushort _uint16;

        [global::Cdrcs.Id(12)]
        public uint _uint32;

        [global::Cdrcs.Id(13)]
        public byte _uint8;

        [global::Cdrcs.Id(14)]
        public sbyte _int8;

        [global::Cdrcs.Id(15)]
        public short _int16;

        [global::Cdrcs.Id(16)]
        public int _int32;

        [global::Cdrcs.Id(17)]
        public long _int64;

        [global::Cdrcs.Id(18)]
        public double _double;

        [global::Cdrcs.Id(20)]
        public float _float;

        [global::Cdrcs.Id(22), global::Cdrcs.Type(typeof(long))]
        public System.DateTime dt;
    }

    [global::Cdrcs.Schema]
    public struct NestedStructs
    {
        [global::Cdrcs.Id(0)]
        public StructWithFields s1;

        [global::Cdrcs.Id(1)]
        public StructWithProperties s2 { get; set; }
    }

    [global::Cdrcs.Schema]
    public class ClassWithStructFields
    {
        [global::Cdrcs.Id(0)]
        public StructWithFields s1;

        [global::Cdrcs.Id(1)]
        public StructWithProperties s2;
    }

    [global::Cdrcs.Schema]
    public class ClassWithStructProperties
    {
        [global::Cdrcs.Id(0)]
        public StructWithFields s1 { get; set; }

        [global::Cdrcs.Id(1)]
        public StructWithProperties s2 { get; set; }
    }

    [global::Cdrcs.Schema]
    public class CollectionsOfStructs
    {
        [global::Cdrcs.Id(0)]
        public List<StructWithFields> l1 { get; set; }

        [global::Cdrcs.Id(1)]
        public List<StructWithProperties> l2 { get; set; }

        [global::Cdrcs.Id(2)]
        public Dictionary<int, StructWithProperties> m1 { get; set; }

        [global::Cdrcs.Id(3)]
        public NestedStructs[] a1 { get; set; }
    }
}
