#pragma warning disable
namespace UnitTestSamples
{
    using System.Collections.Generic;
    using System;
    using Cdrcs.Tag;
    using Cdrcs;

    public enum EnumType1
    {
        EnumValue1 = 5,
        EnumValue2 = 10,
        EnumValue3 = -10,
        EnumValue4 = 0x2A,
        Low = 1,
        EnumValue5 = 99, //-10 in two's complement (cannot be expressed here)
    };

    [Schema]
    [Attribute("Foo", "foo")]
    [Attribute("Bar", "bar")]
    public class BasicTypes
    {
        [global::Cdrcs.Id(0)]
        [Attribute("Name", "Boolean")]
        public bool _bool { get; set; }

        [global::Cdrcs.Id(2)]
        public string _str { get; set; } = "";

        [global::Cdrcs.Id(3), global::Cdrcs.Type(typeof(wstring))]
        public string _wstr { get; set; } = "";

        [global::Cdrcs.Id(14)]
        public sbyte _int8 { get; set; }

        [global::Cdrcs.Id(15)]
        public Int16 _int16 { get; set; }

        [global::Cdrcs.Id(16)]
        public Int32 _int32 { get; set; }

        [global::Cdrcs.Id(17)]
        public Int64 _int64 { get; set; }

        [global::Cdrcs.Id(13)]
        public byte _uint8 { get; set; }

        [global::Cdrcs.Id(11)]
        public UInt16 _uint16 { get; set; }

        [global::Cdrcs.Id(12)]
        public UInt32 _uint32 { get; set; }

        [global::Cdrcs.Id(10)]
        public UInt64 _uint64 { get; set; }

        [global::Cdrcs.Id(18)]
        public double _double { get; set; }

        [global::Cdrcs.Id(20)]
        public float _float { get; set; }

        [global::Cdrcs.Id(21), Default(EnumType1.EnumValue1)]
        public EnumType1 _enum1 { get; set; }

        /*[global::Cdrcs.Id(22), global::Cdrcs.Type(typeof(UInt64))]
        public DateTime dt { get; set; }*/
    };

    [Schema]
    public class Integers
    {
        [global::Cdrcs.Id(14)]
        public sbyte _int8 { get; set; }

        [global::Cdrcs.Id(15)]
        public Int16 _int16 { get; set; }

        [global::Cdrcs.Id(16)]
        public Int32 _int32 { get; set; }

        [global::Cdrcs.Id(17)]
        public Int64 _int64 { get; set; }

        [global::Cdrcs.Id(13)]
        public byte _uint8 { get; set; }

        [global::Cdrcs.Id(11)]
        public UInt16 _uint16 { get; set; }

        [global::Cdrcs.Id(12)]
        public UInt32 _uint32 { get; set; }

        [global::Cdrcs.Id(10)]
        public UInt64 _uint64 { get; set; }

    };

    [Schema]
    public class MaxUInt64
    {
        [global::Cdrcs.Id(10)]
        public UInt64 _uint64 { get; set; }
    };

/*    [Schema]
    public class FieldOfStructWithAliases
    {
        [global::Cdrcs.Id(0)]
        UnitTestSamples.Aliases.BlobAlias b { get; set; }

        [global::Cdrcs.Id(1)]
        DateTime dt { get; set; }
    }

    [Schema]
    public class ContainerOfStructWithAliases
    {
        [global::Cdrcs.Id(0)]
        Dictionary<string, LinkedList<nullable<UnitTestSamples.Aliases.BlobAlias>>> m { get; set; }
    }

    [Schema]
    public class BaseWithAliases : UnitTestSamples.Aliases.BlobAlias
    {
    }

    [Schema]
    public class NestedWithAliases
    {
        [global::Cdrcs.Id(0)]
        FieldOfStructWithAliases f { get; set; }
    }
*/
    [Schema]
    public class RequiredOptional
    {
        [Attribute("JsonName", "OptionalX")]
        [global::Cdrcs.Id(7), RequiredOptional]
        public UInt32 x { get; set; }
    };

    [Schema]
    public class Required
    {
        [Attribute("JsonName", "RequiredX")]
        [global::Cdrcs.Id(7), global::Cdrcs.RequiredAttribute]
        public UInt32 x { get; set; }

        [Attribute("JsonName", "RequiredY")]
        [global::Cdrcs.Id(9), global::Cdrcs.RequiredAttribute]
        public BasicTypes y { get; set; } = new BasicTypes();
    };

    [Schema]
    public class DerivedRequired : Required
    {
        [global::Cdrcs.Id(1), global::Cdrcs.RequiredAttribute]
        public UInt32 foo { get; set; }

        [global::Cdrcs.Id(2), global::Cdrcs.RequiredAttribute]
        public string bar { get; set; } = "";

        [global::Cdrcs.Id(3)]
        public bool flag { get; set; }

        [global::Cdrcs.Id(4), global::Cdrcs.RequiredAttribute]
        public double bla { get; set; }
    };

    [Schema]
    public class Optional
    {
        [Attribute("JsonName", "OptionalX")]
        [global::Cdrcs.Id(7), global::Cdrcs.RequiredOptional]
        public UInt32 x { get; set; }

        [Attribute("JsonName", "OptionalY")]
        [global::Cdrcs.Id(9), global::Cdrcs.RequiredOptional]
        public BasicTypes y { get; set; } = new BasicTypes();
};

    [Schema]
    public class RequiredInDerived : Optional
    {
        [global::Cdrcs.Id(7), global::Cdrcs.RequiredAttribute]
        public UInt32 x { get; set; }

        [global::Cdrcs.Id(9), global::Cdrcs.RequiredAttribute]
        public BasicTypes y { get; set; } = new BasicTypes();
    }

    [Schema]
    public class RequiredInBase : Required
    {
        [global::Cdrcs.Id(7), global::Cdrcs.RequiredOptional]
        public UInt32 x { get; set; }

        [global::Cdrcs.Id(9), global::Cdrcs.RequiredOptional]
        public BasicTypes y { get; set; } = new BasicTypes();
    }

    [Schema]
    public class RequiredInBaseAndDerived : Required
    {
        [global::Cdrcs.Id(7), global::Cdrcs.RequiredAttribute]
        public UInt32 x { get; set; }

        [global::Cdrcs.Id(9), global::Cdrcs.RequiredAttribute]
        public BasicTypes y { get; set; } = new BasicTypes();
    }

/*    [Schema]
    [Attribute("xmlns", "urn:UnitTest.BasicTypes")]
    public class BasicTypesView view_of BasicTypes
    {
        _bool, _int8, _float;
    };
*/
    [Schema]
    public class Nested1
    {
        [global::Cdrcs.Id(0)]
        public BasicTypes basic1 { get; set; } = new BasicTypes();

        [global::Cdrcs.Id(1)]
        public BasicTypes basic2 { get; set; } = new BasicTypes();

        [global::Cdrcs.Id(2)]
        public Cdrcs.GUID guid { get; set; } = new Cdrcs.GUID();
    };

    [Schema]
    public class Nested
    {
        [global::Cdrcs.Id(0)]
        public BasicTypes basic { get; set; } = new BasicTypes();

        [global::Cdrcs.Id(1)]
        public Nested1 nested { get; set; } = new Nested1();
    };

    [Schema]
    public class EmptyBase : Nested
    {
    };

    [Schema]
    public class Derived : EmptyBase
    {
        [global::Cdrcs.Id(0)]
        public string derived { get; set; }

        [Attribute("JsonName", "nestedDerived")]
        [global::Cdrcs.Id(1)]
        public Nested1 nested { get; set; } = new Nested1();
    };

    [Schema]
    public class B
    {
        [global::Cdrcs.Id(0)]
        public Int32 b { get; set; }
    }

    [Schema]
    public class B1 : B
    {
        [Attribute("JsonName", "b1")]
        [global::Cdrcs.Id(0)]
        public Int32 b { get; set; }
    }

    [Schema]
    public class B2 : B1
    {
        [Attribute("JsonName", "b2")]
        [global::Cdrcs.Id(0)]
        public Int32 b { get; set; }
    }

    [Schema]
    public class B3 : B2
    {
        [Attribute("JsonName", "b3")]
        [global::Cdrcs.Id(0)]
        public Int32 b { get; set; }
    }

    [Schema]
    public class B4 : B3
    {
        [Attribute("JsonName", "b4")]
        [global::Cdrcs.Id(0)]
        public Int32 b { get; set; }
    }

    [Schema]
    public class B5 : B4
    {
        [Attribute("JsonName", "b5")]
        [global::Cdrcs.Id(0)]
        public Int32 b { get; set; }
    }

    [Schema]
    public class Deep : B5 { }


    [Schema]
    public class NestedContainers
    {
    [global::Cdrcs.Id(0)]
    public List<List<UInt64>> vvb;
    [global::Cdrcs.Id(1), global::Cdrcs.Type(typeof(LinkedList<nullable<Dictionary<string, bool>>>))]
    public LinkedList<Dictionary<string, bool>> vnc;
    [global::Cdrcs.Id(2)]
    public List<List<BasicTypes>> vvbt;
    [global::Cdrcs.Id(3)]
    public List<LinkedList<Nested>> vln;
    [global::Cdrcs.Id(4)]
    public LinkedList<List<string>> lvs;
    [global::Cdrcs.Id(5)]
    public LinkedList<HashSet<float>> lsf;
    [global::Cdrcs.Id(6)]
    public List<HashSet<bool>> vsb;
    [global::Cdrcs.Id(7)]
    public Dictionary<string, LinkedList<bool>> mslb;
/*    [global::Cdrcs.Id(8)]
    public Dictionary<Int32, Dictionary<Int64, BasicTypesView>> mimbn;
*/    
    }


    /*    [Schema]
        [Attribute("xmlns", "urn:UnitTest.Derived")]
        public class DerivedView view_of Derived
        {
            derived;
        };
    */
    /*
        [Schema]
        public class Nothing
        {
            [global::Cdrcs.Id(0), Default(nothing)]
            bool _bool;
            [global::Cdrcs.Id(2), Default(nothing)]
            string _str;
            [global::Cdrcs.Id(3), Default(nothing)]
            wstring _wstr;
            [global::Cdrcs.Id(14), Default(nothing)]
            sbyte _int8;
            [global::Cdrcs.Id(15), Default(nothing)]
            Int16 _int16;
            [global::Cdrcs.Id(16), Default(nothing)]
            Int32 _int32;
            [global::Cdrcs.Id(17), Default(nothing)]
            Int64 _int64;
            [global::Cdrcs.Id(13), Default(nothing)]
            byte _uint8;
            [global::Cdrcs.Id(11), Default(nothing)]
            UInt16 _uint16;
            [global::Cdrcs.Id(12), Default(nothing)]
            UInt32 _uint32;
            [global::Cdrcs.Id(10), Default(nothing)]
            UInt64 _uint64;
            [global::Cdrcs.Id(18), Default(nothing)]
            double _double;
            [global::Cdrcs.Id(20), Default(nothing)]
            float _float;
            [global::Cdrcs.Id(21), Default(nothing)]
            EnumType1 _enum1;
            [global::Cdrcs.Id(30), Default(nothing)]
            LinkedList<string> l;
            [global::Cdrcs.Id(31), Default(nothing)]
            HashSet<double> s;
            [global::Cdrcs.Id(32), Default(nothing)]
            List<LinkedList<Int32>> vl;
            [global::Cdrcs.Id(33), Default(nothing)]
            System.ArraySegment<byte> b;
            [global::Cdrcs.Id(34), Default(nothing)]
            Dictionary<string, double> m;
            [global::Cdrcs.Id(35), Default(nothing)]
            DateTime dt;
        };

        [Schema]
        public class NotNothingView
        {
            [global::Cdrcs.Id(20)]
            required float _float;
            [global::Cdrcs.Id(21), Default(EnumValue4)]
            required EnumType1 _enum1;
            [global::Cdrcs.Id(30)]
            required LinkedList<string> l;
            [global::Cdrcs.Id(31)]
            required HashSet<double> s;
            [global::Cdrcs.Id(32)]
            required List<LinkedList<Int32>> vl;
        };
 */

        [Schema]
        public class NullableBasicTypes
        {
            [global::Cdrcs.Id(0), global::Cdrcs.Type(typeof(nullable<bool>))]
            public bool? _bool;
            [global::Cdrcs.Id(2), global::Cdrcs.Type(typeof(nullable<string>))]
            public string _str;
            [global::Cdrcs.Id(3), global::Cdrcs.Type(typeof(nullable<wstring>))]
            public wstring _wstr;
            [global::Cdrcs.Id(14), global::Cdrcs.Type(typeof(nullable<sbyte>))]
            public sbyte? _int8;
            [global::Cdrcs.Id(15), global::Cdrcs.Type(typeof(nullable<Int16>))]
            public Int16? _int16;
            [global::Cdrcs.Id(16), global::Cdrcs.Type(typeof(nullable<Int32>))]
            public Int32? _int32;
            [global::Cdrcs.Id(17), global::Cdrcs.Type(typeof(nullable<Int64>))]
            public Int64? _int64;
            [global::Cdrcs.Id(13), global::Cdrcs.Type(typeof(nullable<byte>))]
            public byte? _uint8;
            [global::Cdrcs.Id(11), global::Cdrcs.Type(typeof(nullable<UInt16>))]
            public UInt16? _uint16;
            [global::Cdrcs.Id(12), global::Cdrcs.Type(typeof(nullable<UInt32>))]
            public UInt32? _uint32;
            [global::Cdrcs.Id(10), global::Cdrcs.Type(typeof(nullable<UInt64>))]
            public UInt64? _uint64;
            [global::Cdrcs.Id(18), global::Cdrcs.Type(typeof(nullable<double>))]
            public double? _double;
            [global::Cdrcs.Id(20), global::Cdrcs.Type(typeof(nullable<float>))]
            public float? _float;
            [global::Cdrcs.Id(21), global::Cdrcs.Type(typeof(nullable<EnumType1>))]
            public EnumType1? _enum1;
            [global::Cdrcs.Id(22), global::Cdrcs.Type(typeof(nullable<DateTime>))]
            public DateTime? dt;
        };

        [Schema]
        public class NullableStruct
        {
            [global::Cdrcs.Id(0), global::Cdrcs.Type(typeof(nullable<BasicTypes>))]
            public BasicTypes basic;
            [global::Cdrcs.Id(1), global::Cdrcs.Type(typeof(nullable<Nested1>))]
            public Nested1 nested;
        };

    [Schema]
    public class SimpleContainers
    {
        [global::Cdrcs.Id(0)]
        public LinkedList<string> strings { get; set; }

        [global::Cdrcs.Id(1)]
        public LinkedList<BasicTypes> basics { get; set; }

        [global::Cdrcs.Id(2)]
        public Dictionary<Int32, string> numbers { get; set; }
    };

    [Schema]
    public class Lists
    {
        [global::Cdrcs.Id(0)]
        public LinkedList<bool> _bool { get; set; }

        [global::Cdrcs.Id(2)]
        public LinkedList<string> _str { get; set; }

        [global::Cdrcs.Id(3), global::Cdrcs.Type(typeof(LinkedList<wstring>))]
        public LinkedList<string> _wstr { get; set; }

        [global::Cdrcs.Id(14)]
        public LinkedList<sbyte> _int8 { get; set; }

        [global::Cdrcs.Id(15)]
        public LinkedList<Int16> _int16 { get; set; }

        [global::Cdrcs.Id(16)]
        public LinkedList<Int32> _int32 { get; set; }

        [global::Cdrcs.Id(17)]
        public LinkedList<Int64> _int64 { get; set; }

        [global::Cdrcs.Id(13)]
        public LinkedList<byte> _uint8 { get; set; }

        [global::Cdrcs.Id(11)]
        public LinkedList<UInt16> _uint16 { get; set; }

        [global::Cdrcs.Id(12)]
        public LinkedList<UInt32> _uint32 { get; set; }

        [global::Cdrcs.Id(10)]
        public LinkedList<UInt64> _uint64 { get; set; }

        [global::Cdrcs.Id(18)]
        public LinkedList<double> _double { get; set; }

        [global::Cdrcs.Id(20)]
        public LinkedList<float> _float { get; set; }

        [global::Cdrcs.Id(21)]
        public LinkedList<EnumType1> _enum1 { get; set; }

        [global::Cdrcs.Id(30)]
        public LinkedList<BasicTypes> basic { get; set; }

        [global::Cdrcs.Id(31)]
        public LinkedList<Nested1> nested { get; set; }
    };

    [Schema]
    public class Vectors
    {
        [global::Cdrcs.Id(0)]
        public List<bool> _bool { get; set; }

        [global::Cdrcs.Id(2)]
        public List<string> _str { get; set; }

        [global::Cdrcs.Id(3), global::Cdrcs.Type(typeof(List<wstring>))]
        public List<string> _wstr { get; set; }

        [global::Cdrcs.Id(14)]
        public List<sbyte> _int8 { get; set; }

        [global::Cdrcs.Id(15)]
        public List<Int16> _int16 { get; set; }

        [global::Cdrcs.Id(16)]
        public List<Int32> _int32 { get; set; }

        [global::Cdrcs.Id(17)]
        public List<Int64> _int64 { get; set; }

        [global::Cdrcs.Id(13)]
        public List<byte> _uint8 { get; set; }

        [global::Cdrcs.Id(11)]
        public List<UInt16> _uint16 { get; set; }

        [global::Cdrcs.Id(12)]
        public List<UInt32> _uint32 { get; set; }

        [global::Cdrcs.Id(10)]
        public List<UInt64> _uint64 { get; set; }

        [global::Cdrcs.Id(18)]
        public List<double> _double { get; set; }

        [global::Cdrcs.Id(20)]
        public List<float> _float { get; set; }

        [global::Cdrcs.Id(21)]
        public List<EnumType1> _enum1 { get; set; }

        [global::Cdrcs.Id(30)]
        public List<BasicTypes> basic { get; set; }

        [global::Cdrcs.Id(31)]
        public List<Nested1> nested { get; set; }
    };

    [Schema]
    public class Sets
    {
        [global::Cdrcs.Id(0)]
        public HashSet<bool> _bool { get; set; }

        [global::Cdrcs.Id(2)]
        public HashSet<string> _str { get; set; }

        [global::Cdrcs.Id(3), global::Cdrcs.Type(typeof(HashSet<wstring>))]
        public HashSet<string> _wstr { get; set; }

        [global::Cdrcs.Id(14)]
        public HashSet<sbyte> _int8 { get; set; }

        [global::Cdrcs.Id(15)]
        public HashSet<Int16> _int16 { get; set; }

        [global::Cdrcs.Id(16)]
        public HashSet<Int32> _int32 { get; set; }

        [global::Cdrcs.Id(17)]
        public HashSet<Int64> _int64 { get; set; }

        [global::Cdrcs.Id(13)]
        public HashSet<byte> _uint8 { get; set; }

        [global::Cdrcs.Id(11)]
        public HashSet<UInt16> _uint16 { get; set; }

        [global::Cdrcs.Id(12)]
        public HashSet<UInt32> _uint32 { get; set; }

        [global::Cdrcs.Id(10)]
        public HashSet<UInt64> _uint64 { get; set; }

        [global::Cdrcs.Id(18)]
        public HashSet<double> _double { get; set; }

        [global::Cdrcs.Id(20)]
        public HashSet<float> _float { get; set; }

        [global::Cdrcs.Id(21)]
        public HashSet<EnumType1> _enum1 { get; set; }

    };

    [Schema]
    public class Maps
    {
        [global::Cdrcs.Id(0)]
        public Dictionary<string, bool> _bool { get; set; }

        [global::Cdrcs.Id(2)]
        public Dictionary<string, string> _str { get; set; }

        [global::Cdrcs.Id(3), global::Cdrcs.Type(typeof(Dictionary<string, wstring>))]
        public Dictionary<string, string> _wstr { get; set; }

        [global::Cdrcs.Id(14)]
        public Dictionary<string, sbyte> _int8 { get; set; }

        [global::Cdrcs.Id(15)]
        public Dictionary<string, Int16> _int16 { get; set; }

        [global::Cdrcs.Id(16)]
        public Dictionary<string, Int32> _int32 { get; set; }

        [global::Cdrcs.Id(17)]
        public Dictionary<string, Int64> _int64 { get; set; }

        [global::Cdrcs.Id(13)]
        public Dictionary<string, byte> _uint8 { get; set; }

        [global::Cdrcs.Id(11)]
        public Dictionary<string, UInt16> _uint16 { get; set; }

        [global::Cdrcs.Id(12)]
        public Dictionary<string, UInt32> _uint32 { get; set; }

        [global::Cdrcs.Id(10)]
        public Dictionary<string, UInt64> _uint64 { get; set; }

        [global::Cdrcs.Id(18)]
        public Dictionary<string, double> _double { get; set; }

        [global::Cdrcs.Id(20)]
        public Dictionary<string, float> _float { get; set; }

        [global::Cdrcs.Id(21)]
        public Dictionary<string, EnumType1> _enum1 { get; set; }

        [global::Cdrcs.Id(22)]
        public Dictionary<string, BasicTypes> _basic { get; set; }

    };

    [Schema]
    public class Containers
    {
        [global::Cdrcs.Id(0)]
        public Lists l { get; set; }

        [global::Cdrcs.Id(1)]
        public Sets s { get; set; }

        [global::Cdrcs.Id(2)]
        public Vectors v { get; set; }

        [global::Cdrcs.Id(3)]
        public Maps m { get; set; }

    };

    [Schema]
    public class VectorsOfNullable
    {
        [global::Cdrcs.Id(0), global::Cdrcs.Type(typeof(List<nullable<bool>>))]
        public List<bool?> _bool { get; set; }

        [global::Cdrcs.Id(2), global::Cdrcs.Type(typeof(List<nullable<string>>))]
        public List<string> _str { get; set; }

        [global::Cdrcs.Id(3), global::Cdrcs.Type(typeof(List<nullable<wstring>>))]
        public List<string> _wstr { get; set; }

        [global::Cdrcs.Id(14), global::Cdrcs.Type(typeof(List<nullable<sbyte>>))]
        public List<sbyte?> _int8 { get; set; }

        [global::Cdrcs.Id(15), global::Cdrcs.Type(typeof(List<nullable<Int16>>))]
        public List<Int16?> _int16 { get; set; }

        [global::Cdrcs.Id(16), global::Cdrcs.Type(typeof(List<nullable<Int32>>))]
        public List<Int32?> _int32 { get; set; }

        [global::Cdrcs.Id(17), global::Cdrcs.Type(typeof(List<nullable<Int64>>))]
        public List<Int64?> _int64 { get; set; }

        [global::Cdrcs.Id(13), global::Cdrcs.Type(typeof(List<nullable<byte>>))]
        public List<byte?> _uint8 { get; set; }

        [global::Cdrcs.Id(11), global::Cdrcs.Type(typeof(List<nullable<UInt16>>))]
        public List<UInt16?> _uint16 { get; set; }

        [global::Cdrcs.Id(12), global::Cdrcs.Type(typeof(List<nullable<UInt32>>))]
        public List<UInt32?> _uint32 { get; set; }

        [global::Cdrcs.Id(10), global::Cdrcs.Type(typeof(List<nullable<UInt64>>))]
        public List<UInt64?> _uint64 { get; set; }

        [global::Cdrcs.Id(18), global::Cdrcs.Type(typeof(List<nullable<double>>))]
        public List<double?> _double { get; set; }

        [global::Cdrcs.Id(20), global::Cdrcs.Type(typeof(List<nullable<float>>))]
        public List<float?> _float { get; set; }

        [global::Cdrcs.Id(21), global::Cdrcs.Type(typeof(List<nullable<EnumType1>>))]
        public List<EnumType1?> _enum1 { get; set; }

        [global::Cdrcs.Id(30), global::Cdrcs.Type(typeof(List<nullable<BasicTypes>>))]
        public List<BasicTypes> basic { get; set; }

        [global::Cdrcs.Id(31), global::Cdrcs.Type(typeof(List<nullable<Nested1>>))]
        public List<Nested1> nested { get; set; }

        [global::Cdrcs.Id(32), global::Cdrcs.Type(typeof(List<nullable<System.ArraySegment<byte>>>))]
        public List<System.ArraySegment<byte>?> _blob { get; set; }

    };

    [Schema]
    public class MapsOfNullable
    {
        [global::Cdrcs.Id(0), global::Cdrcs.Type(typeof(Dictionary<string, nullable<bool>>))]
        public Dictionary<string, bool?> _bool { get; set; }

        [global::Cdrcs.Id(2), global::Cdrcs.Type(typeof(Dictionary<string, nullable<string>>))]
        public Dictionary<string, string> _str { get; set; }

        [global::Cdrcs.Id(3), global::Cdrcs.Type(typeof(Dictionary<string, nullable<wstring>>))]
        public Dictionary<string, string> _wstr { get; set; }

        [global::Cdrcs.Id(14), global::Cdrcs.Type(typeof(Dictionary<string, nullable<sbyte>>))]
        public Dictionary<string, sbyte?> _int8 { get; set; }

        [global::Cdrcs.Id(15), global::Cdrcs.Type(typeof(Dictionary<string, nullable<Int16>>))]
        public Dictionary<string, Int16?> _int16 { get; set; }

        [global::Cdrcs.Id(16), global::Cdrcs.Type(typeof(Dictionary<string, nullable<Int32>>))]
        public Dictionary<string, Int32?> _int32 { get; set; }

        [global::Cdrcs.Id(17), global::Cdrcs.Type(typeof(Dictionary<string, nullable<Int64>>))]
        public Dictionary<string, Int64?> _int64 { get; set; }

        [global::Cdrcs.Id(13), global::Cdrcs.Type(typeof(Dictionary<string, nullable<byte>>))]
        public Dictionary<string, byte?> _uint8 { get; set; }

        [global::Cdrcs.Id(11), global::Cdrcs.Type(typeof(Dictionary<string, nullable<UInt16>>))]
        public Dictionary<string, UInt16?> _uint16 { get; set; }

        [global::Cdrcs.Id(12), global::Cdrcs.Type(typeof(Dictionary<string, nullable<UInt32>>))]
        public Dictionary<string, UInt32?> _uint32 { get; set; }

        [global::Cdrcs.Id(10), global::Cdrcs.Type(typeof(Dictionary<string, nullable<UInt64>>))]
        public Dictionary<string, UInt64?> _uint64 { get; set; }

        [global::Cdrcs.Id(18), global::Cdrcs.Type(typeof(Dictionary<string, nullable<double>>))]
        public Dictionary<string, double?> _double { get; set; }

        [global::Cdrcs.Id(20), global::Cdrcs.Type(typeof(Dictionary<string, nullable<float>>))]
        public Dictionary<string, float?> _float { get; set; }

        [global::Cdrcs.Id(21), global::Cdrcs.Type(typeof(Dictionary<string, nullable<EnumType1>>))]
        public Dictionary<string, EnumType1?> _enum1 { get; set; }

        [global::Cdrcs.Id(22), global::Cdrcs.Type(typeof(Dictionary<string, nullable<BasicTypes>>))]
        public Dictionary<string, BasicTypes> _basic { get; set; }

        [global::Cdrcs.Id(23), global::Cdrcs.Type(typeof(Dictionary<string, nullable<System.ArraySegment<byte>>>))]
        public Dictionary<string, System.ArraySegment<byte>> _blob { get; set; }

    };

    [Schema]
    public class ContainersOfNullable
    {
        [global::Cdrcs.Id(0)]
        public VectorsOfNullable vn { get; set; }

        [global::Cdrcs.Id(1)]
        public MapsOfNullable mn { get; set; }

    };

    [Schema]
    public class NullableVectors
    {
        [global::Cdrcs.Id(0)]
        public nullable<List<bool>> _bool { get; set; }

        [global::Cdrcs.Id(2)]
        public nullable<List<string>> _str { get; set; }

        [global::Cdrcs.Id(3)]
        public nullable<List<wstring>> _wstr { get; set; }

        [global::Cdrcs.Id(14)]
        public nullable<List<sbyte>> _int8 { get; set; }

        [global::Cdrcs.Id(15)]
        public nullable<List<Int16>> _int16 { get; set; }

        [global::Cdrcs.Id(16)]
        public nullable<List<Int32>> _int32 { get; set; }

        [global::Cdrcs.Id(17)]
        public nullable<List<Int64>> _int64 { get; set; }

        [global::Cdrcs.Id(13)]
        public nullable<List<byte>> _uint8 { get; set; }

        [global::Cdrcs.Id(11)]
        public nullable<List<UInt16>> _uint16 { get; set; }

        [global::Cdrcs.Id(12)]
        public nullable<List<UInt32>> _uint32 { get; set; }

        [global::Cdrcs.Id(10)]
        public nullable<List<UInt64>> _uint64 { get; set; }

        [global::Cdrcs.Id(18)]
        public nullable<List<double>> _double { get; set; }

        [global::Cdrcs.Id(20)]
        public nullable<List<float>> _float { get; set; }

        [global::Cdrcs.Id(21)]
        public nullable<List<EnumType1>> _enum1 { get; set; }

        [global::Cdrcs.Id(30)]
        public nullable<List<BasicTypes>> basic { get; set; }

        [global::Cdrcs.Id(31)]
        public nullable<List<Nested1>> nested { get; set; }
    };

    [Schema]
    public class NullableLists
    {
        [global::Cdrcs.Id(0)]
        public nullable<LinkedList<bool>> _bool { get; set; }

        [global::Cdrcs.Id(2)]
        public nullable<LinkedList<string>> _str { get; set; }

        [global::Cdrcs.Id(3)]
        public nullable<LinkedList<wstring>> _wstr { get; set; }

        [global::Cdrcs.Id(14)]
        public nullable<LinkedList<sbyte>> _int8 { get; set; }

        [global::Cdrcs.Id(15)]
        public nullable<LinkedList<Int16>> _int16 { get; set; }

        [global::Cdrcs.Id(16)]
        public nullable<LinkedList<Int32>> _int32 { get; set; }

        [global::Cdrcs.Id(17)]
        public nullable<LinkedList<Int64>> _int64 { get; set; }

        [global::Cdrcs.Id(13)]
        public nullable<LinkedList<byte>> _uint8 { get; set; }

        [global::Cdrcs.Id(11)]
        public nullable<LinkedList<UInt16>> _uint16 { get; set; }

        [global::Cdrcs.Id(12)]
        public nullable<LinkedList<UInt32>> _uint32 { get; set; }

        [global::Cdrcs.Id(10)]
        public nullable<LinkedList<UInt64>> _uint64 { get; set; }

        [global::Cdrcs.Id(18)]
        public nullable<LinkedList<double>> _double { get; set; }

        [global::Cdrcs.Id(20)]
        public nullable<LinkedList<float>> _float { get; set; }

        [global::Cdrcs.Id(21)]
        public nullable<LinkedList<EnumType1>> _enum1 { get; set; }

        [global::Cdrcs.Id(30)]
        public nullable<LinkedList<BasicTypes>> basic { get; set; }

        [global::Cdrcs.Id(31)]
        public nullable<LinkedList<Nested1>> nested { get; set; }

    };

    [Schema]
    public class NullableSets
    {
        [global::Cdrcs.Id(0)]
        public nullable<HashSet<bool>> _bool { get; set; }

        [global::Cdrcs.Id(2)]
        public nullable<HashSet<string>> _str { get; set; }

        [global::Cdrcs.Id(3)]
        public nullable<HashSet<wstring>> _wstr { get; set; }

        [global::Cdrcs.Id(14)]
        public nullable<HashSet<sbyte>> _int8 { get; set; }
        [global::Cdrcs.Id(15)]
        public nullable<HashSet<Int16>> _int16 { get; set; }
        [global::Cdrcs.Id(16)]
        public nullable<HashSet<Int32>> _int32 { get; set; }
        [global::Cdrcs.Id(17)]
        public nullable<HashSet<Int64>> _int64 { get; set; }
        [global::Cdrcs.Id(13)]
        public nullable<HashSet<byte>> _uint8 { get; set; }
        [global::Cdrcs.Id(11)]
        public nullable<HashSet<UInt16>> _uint16 { get; set; }
        [global::Cdrcs.Id(12)]
        public nullable<HashSet<UInt32>> _uint32 { get; set; }
        [global::Cdrcs.Id(10)]
        public nullable<HashSet<UInt64>> _uint64 { get; set; }
        [global::Cdrcs.Id(18)]
        public nullable<HashSet<double>> _double { get; set; }
        [global::Cdrcs.Id(20)]
        public nullable<HashSet<float>> _float { get; set; }
        [global::Cdrcs.Id(21)]
        public nullable<HashSet<EnumType1>> _enum1 { get; set; }
    };

    [Schema]
    public class NullableMaps
    {
        [global::Cdrcs.Id(0)]
        public nullable<Dictionary<float, bool>> _bool { get; set; }
        [global::Cdrcs.Id(2)]
        public nullable<Dictionary<float, string>> _str { get; set; }
        [global::Cdrcs.Id(3)]
        public nullable<Dictionary<float, wstring>> _wstr { get; set; }
        [global::Cdrcs.Id(14)]
        public nullable<Dictionary<float, sbyte>> _int8 { get; set; }
        [global::Cdrcs.Id(15)]
        public nullable<Dictionary<float, Int16>> _int16 { get; set; }
        [global::Cdrcs.Id(16)]
        public nullable<Dictionary<float, Int32>> _int32 { get; set; }
        [global::Cdrcs.Id(17)]
        public nullable<Dictionary<float, Int64>> _int64 { get; set; }
        [global::Cdrcs.Id(13)]
        public nullable<Dictionary<float, byte>> _uint8 { get; set; }
        [global::Cdrcs.Id(11)]
        public nullable<Dictionary<float, UInt16>> _uint16 { get; set; }
        [global::Cdrcs.Id(12)]
        public nullable<Dictionary<float, UInt32>> _uint32 { get; set; }
        [global::Cdrcs.Id(10)]
        public nullable<Dictionary<float, UInt64>> _uint64 { get; set; }
        [global::Cdrcs.Id(18)]
        public nullable<Dictionary<float, double>> _double { get; set; }
        [global::Cdrcs.Id(20)]
        public nullable<Dictionary<float, float>> _float { get; set; }
        [global::Cdrcs.Id(21)]
        public nullable<Dictionary<float, EnumType1>> _enum1 { get; set; }
        [global::Cdrcs.Id(22)]
        public nullable<Dictionary<float, BasicTypes>> _basic { get; set; }
    };

    [Schema]
    public class NullableContainers
    {
        [global::Cdrcs.Id(0)]
        public NullableVectors nv { get; set; }
        [global::Cdrcs.Id(1)]
        public NullableLists nl { get; set; }
        [global::Cdrcs.Id(2)]
        public NullableSets ns { get; set; }
        [global::Cdrcs.Id(3)]
        public NullableMaps nm { get; set; }
    };

    public partial class TreeNode
    {

    }

    [Schema]
    public class Tree
    {
        [global::Cdrcs.Id(0)]
        public nullable<TreeNode> root { get; set; }
    };

    [Schema]
    public partial class TreeNode
    {
        [global::Cdrcs.Id(0)]
        public nullable<TreeNode> left { get; set; }
        [global::Cdrcs.Id(1)]
        public nullable<TreeNode> right { get; set; }
        [global::Cdrcs.Id(2)]
        public BasicTypes value { get; set; }
        [global::Cdrcs.Id(3)]
        public LinkedList<Tree> trees { get; set; }
    };

    [Schema]
    public class StructWithDefaults
    {
        [global::Cdrcs.Id(0), Default(true)]
        public bool m_bool_1 { get; set; }
        [global::Cdrcs.Id(1), Default(false)]
        public bool m_bool_2 { get; set; }
        [global::Cdrcs.Id(2)]
        public bool m_bool_3 { get; set; }

        [global::Cdrcs.Id(3), Default("default string value")]
        public string m_str_1 { get; set; }
        [global::Cdrcs.Id(4)]
        public string m_str_2 { get; set; }

        [global::Cdrcs.Id(5), Default(-127)]
        public sbyte m_int8_4 { get; set; }
        [global::Cdrcs.Id(6)]
        public sbyte m_int8_5 { get; set; }

        [global::Cdrcs.Id(7), Default(-32767)]
        public Int16 m_int16_4 { get; set; }
        [global::Cdrcs.Id(8)]
        public Int16 m_int16_5 { get; set; }

        [global::Cdrcs.Id(9)]
        public Int32 m_int32_4 { get; set; }
        [global::Cdrcs.Id(10), Default(2147483647)]
        public Int32 m_int32_max { get; set; }

        [global::Cdrcs.Id(11)]
        public Int64 m_int64_4 { get; set; }
        [global::Cdrcs.Id(12), Default(9223372036854775807)]
        public Int64 m_int64_max { get; set; }

        [global::Cdrcs.Id(13), Default(255)]
        public byte m_uint8_2 { get; set; }
        [global::Cdrcs.Id(14)]
        public byte m_uint8_3 { get; set; }

        [global::Cdrcs.Id(15), Default(65535)]
        public UInt16 m_uint16_2 { get; set; }
        [global::Cdrcs.Id(16)]
        public UInt16 m_uint16_3 { get; set; }

        [global::Cdrcs.Id(17)]
        public UInt32 m_uint32_3 { get; set; }
        [global::Cdrcs.Id(18), Default(4294967295)]
        public UInt32 m_uint32_max { get; set; }

        [global::Cdrcs.Id(19)]
        public UInt64 m_uint64_3 { get; set; }
        [global::Cdrcs.Id(20), Default(0xFFFFFFFFFFFFFFFF)]
        public UInt64 m_uint64_max { get; set; }

        [global::Cdrcs.Id(21)]
        public double m_double_3 { get; set; }
        [global::Cdrcs.Id(22), Default(-123.4567890)]
        public double m_double_4 { get; set; }
        [global::Cdrcs.Id(23), Default(-0.0)]
        public double m_double_5 { get; set; }

        [global::Cdrcs.Id(24)]
        public float m_float_3 { get; set; }
        [global::Cdrcs.Id(25), Default(2.71828183)]
        public float m_float_4 { get; set; }
        [global::Cdrcs.Id(26), Default(+0.0)]
        public float m_float_7 { get; set; }

        [global::Cdrcs.Id(27), Default(EnumType1.EnumValue1)]
        public EnumType1 m_enum1 { get; set; }
        [global::Cdrcs.Id(28), Default(EnumType1.EnumValue3)]
        public EnumType1 m_enum2 { get; set; }

        [global::Cdrcs.Id(29), Default("default wstring value")]
        public wstring m_wstr_1 { get; set; }
        [global::Cdrcs.Id(30)]
        public wstring m_wstr_2 { get; set; }
    };

    [Schema]
    public class StructWithBonded
    {
        [global::Cdrcs.Id(0)]
        public bonded<Nested> field { get; set; }
        [global::Cdrcs.Id(1)]
        public List<bonded<Nested>> poly { get; set; }
    };

    [Schema]
    public class StructWithBlobs
    {
        [global::Cdrcs.Id(0)]
        public System.ArraySegment<byte> b { get; set; }
        [global::Cdrcs.Id(1)]
        public List<System.ArraySegment<byte>> lb { get; set; }
        [global::Cdrcs.Id(2)]
        public nullable<System.ArraySegment<byte>> nb { get; set; }
    };

    [Schema]
    public class StructWithByteLists
    {
        [global::Cdrcs.Id(0)]
        public List<sbyte> b { get; set; }
        [global::Cdrcs.Id(1)]
        public List<List<sbyte>> lb { get; set; }
        [global::Cdrcs.Id(2)]
        public nullable<List<sbyte>> nb { get; set; }
    };

/*    [Schema]
    public class GenericNothingClass<T>
    {
        [global::Cdrcs.Id(4), Default(nothing)]
        T nothingField { get; set; }
    };

    [Schema]
    public class GenericNothingScalar<T : value>
{
        [global::Cdrcs.Id(4), Default(nothing)]
        T nothingField { get; set; }
    };
*/

    [Schema]
    public class GenericClass<T>
    {
        [global::Cdrcs.Id(0)]
        public T field { get; set; }
        [global::Cdrcs.Id(1)]
        public List<T> vectorField { get; set; }
        [global::Cdrcs.Id(2)]
        public LinkedList<GenericClass<T>> listGeneric { get; set; }
        [global::Cdrcs.Id(3)]
        public nullable<T> nullableField { get; set; }
        [global::Cdrcs.Id(4)]
        public Dictionary<string, T> mapField { get; set; }
    };

    [Schema]
    public class GenericScalar<T>
{
        [global::Cdrcs.Id(0)]
        public T field { get; set; }
        [global::Cdrcs.Id(1)]
        public List<T> vectorField { get; set; }
        [global::Cdrcs.Id(2)]
        public LinkedList<GenericScalar<T>> listGeneric { get; set; }
        [global::Cdrcs.Id(3)]
        public nullable<T> nullableField { get; set; }
        [global::Cdrcs.Id(4)]
        public Dictionary<T, T> mapField { get; set; }
    };

    [Schema]
    public class GenericBonded<T>
    {
        [global::Cdrcs.Id(0)]
        public bonded<T> field { get; set; }
        [global::Cdrcs.Id(1)]
        public List<bonded<T>> poly { get; set; }
    };

    [Schema]
    public class BondedGeneric
    {
        [global::Cdrcs.Id(0)]
        public GenericClass<bonded<Nested>> cbt;
        [global::Cdrcs.Id(1)]
        public GenericClass<bonded<GenericClass<wstring>>> cbgws { get; set; }
    };

    [Schema]
    public class GenericDerived<T> : GenericClass<T>
    {
    };

    [Schema]
    public class Generics
    {
        [global::Cdrcs.Id(10)]
        public GenericScalar<bool> sb { get; set; }
        [global::Cdrcs.Id(11)]
        public GenericScalar<float> sf { get; set; }
        [global::Cdrcs.Id(12)]
        public GenericScalar<UInt64> sui64 { get; set; }
        [global::Cdrcs.Id(13)]
        public GenericScalar<EnumType1> se { get; set; }
        [global::Cdrcs.Id(14)]
        public GenericScalar<DateTime> sdt { get; set; }

        [global::Cdrcs.Id(20)]
        public GenericClass<HashSet<Int32>> ci32 { get; set; }
        [global::Cdrcs.Id(21)]
        public GenericClass<System.ArraySegment<byte>> cblob { get; set; }
        [global::Cdrcs.Id(22)]
        public GenericClass<string> cs { get; set; }
        [global::Cdrcs.Id(23)]
        public GenericClass<List<System.ArraySegment<byte>>> cvblob { get; set; }
        [global::Cdrcs.Id(24)]
        public GenericClass<BasicTypes> cbt { get; set; }
        [global::Cdrcs.Id(25)]
        public GenericClass<wstring> cws { get; set; }
        [global::Cdrcs.Id(26), global::Cdrcs.Type(typeof(GenericClass<nullable<wstring>>))]
        public GenericClass<wstring> cnws { get; set; }
        [global::Cdrcs.Id(27), global::Cdrcs.Type(typeof(GenericClass<nullable<UInt64>>))]
        public GenericClass<UInt64?> snui64 { get; set; }
        [global::Cdrcs.Id(28)]
        public GenericClass<LinkedList<wstring>> slws { get; set; }
        [global::Cdrcs.Id(29)]
        public GenericClass<Dictionary<wstring, nullable<Int16>>> smwsni { get; set; }
    };

    [Schema]
    public class GenericInheritance
    {
        [global::Cdrcs.Id(0)]
        public GenericDerived<BasicTypes> bbt { get; set; }
        [global::Cdrcs.Id(1)]
        public GenericDerived<GenericClass<wstring>> bgws { get; set; }
    };
/*
    [Schema]
    public class GenericsWithNothing
    {
        [global::Cdrcs.Id(10)]
        GenericNothingScalar<bool> sb { get; set; }
        [global::Cdrcs.Id(11)]
        GenericNothingScalar<float> sf { get; set; }
        [global::Cdrcs.Id(12)]
        GenericNothingScalar<UInt64> sui64 { get; set; }
        [global::Cdrcs.Id(13)]
        GenericNothingScalar<EnumType1> se { get; set; }
        [global::Cdrcs.Id(14)]
        GenericNothingScalar<DateTime> sdt { get; set; }

        [global::Cdrcs.Id(20)]
        GenericNothingClass<HashSet<Int32>> ci32 { get; set; }
        [global::Cdrcs.Id(21)]
        GenericNothingClass<System.ArraySegment<byte>> cblob { get; set; }
        [global::Cdrcs.Id(22)]
        GenericNothingClass<string> cs { get; set; }
        [global::Cdrcs.Id(23)]
        GenericNothingClass<List<System.ArraySegment<byte>>> cvblob { get; set; }
        [global::Cdrcs.Id(24)]
        GenericNothingClass<wstring> cws { get; set; }
        [global::Cdrcs.Id(25)]
        GenericNothingClass<List<wstring>> cvws { get; set; }
    };
*/
    [Schema]
    public class BoxWstring
    {
        [global::Cdrcs.Id(0)]
        public wstring value { get; set; }
    };

    [Schema]
    public class Box<T>
    {
        [global::Cdrcs.Id(0)]
        public T value { get; set; }
    };

    [Schema]
    public class GenericWString
    {
        [global::Cdrcs.Id(0)]
        public Box<wstring> wstr { get; set; }
    };

    [Schema]
    public class NonGenericWString
    {
        [global::Cdrcs.Id(0)]
        public BoxWstring wstr { get; set; }
    };

    // Name conflicts
    public enum TypeAttribute { DefaultAttribute }

    [Schema]
    public class IdAttribute
    {
        [global::Cdrcs.Id(0), Default(TypeAttribute.DefaultAttribute)]
        public TypeAttribute field { get; set; }
    }

    [Schema]
    public class classT { }

    [Schema]
    public class structT { }

/*
    [Schema]
    public class GenericConflict<structT : value>
{
        [global::Cdrcs.Id(0)]
        [Schema]
        structT field { get; set; }
    }

    [Schema]
    public class Foo<classT>
    {
        [global::Cdrcs.Id(0)]
        classT field { get; set; }
    }

    [polymorphic("")]
    public class WithPolymorphic
    {
        [global::Cdrcs.Id(0)]
        bond_meta::full_name _bond_meta { get; set; }
        [global::Cdrcs.Id(1), Default("a")]
        string a { get; set; }
    }

    [Schema]
    public class DerivedPolymorphic : WithPolymorphic
    {
        [global::Cdrcs.Id(2), Default("b")]
        string b { get; set; }
    }

    [Schema]
    public class WithMeta
    {
        [global::Cdrcs.Id(1)]
        bond_meta::name theName { get; set; }
        [global::Cdrcs.Id(2)]
        bond_meta::full_name theFullName { get; set; }
        [global::Cdrcs.Id(3), Default("a")]
        string a { get; set; }
    }

    [Schema]
    public class DerivedWithMeta : WithMeta
    {
        [global::Cdrcs.Id(1)]
        bond_meta::full_name anotherName { get; set; }
        [global::Cdrcs.Id(2), Default("b")]
        string b { get; set; }
    }

    [Schema]
    public class WithConflictingMeta
    {
        [global::Cdrcs.Id(1), Default("Foo")]
        string name { get; set; }
        [global::Cdrcs.Id(2), Default("Bar")]
        string fullName { get; set; }
        [global::Cdrcs.Id(3)]
        bond_meta::full_name meta { get; set; }
    }*/

    [Schema]
    public class DateAsString
    {
        [global::Cdrcs.Id(0)]
        public string timestamp { get; set; }
    }

}
#pragma warning enable
