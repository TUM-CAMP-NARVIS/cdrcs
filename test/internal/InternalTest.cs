#pragma warning disable
namespace InternalTest
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
        EnumValue5 = 9, //-10 in two's complement (cannot be expressed in csharp)
    };

    [Schema]
    [Attribute("Foo", "foo")]
    [Attribute("Bar", "bar")]
    public interface BasicTypes
    {

        [Id(0)]
        [Attribute("Name", "Boolean")]
        bool _bool { get; set; }

        [Id(2)]
        string _str { get; set; }

        [Id(3), Type(typeof(wstring))]
        string _wstr { get; set; }

        [Id(14)]
        sbyte _int8 { get; set; }

        [Id(15)]
        Int16 _int16 { get; set; }

        [Id(16)]
        Int32 _int32 { get; set; }

        [Id(17)]
        Int64 _int64 { get; set; }

        [Id(13)]
        byte _uint8 { get; set; }

        [Id(11)]
        UInt16 _uint16 { get; set; }

        [Id(12)]
        UInt32 _uint32 { get; set; }

        [Id(10)]
        UInt64 _uint64 { get; set; }

        [Id(18)]
        double _double { get; set; }

        [Id(20)]
        float _float { get; set; }

        [Id(21), Default(EnumType1.EnumValue1)]
        EnumType1 _enum1 { get; set; }
    };


    [Schema]
    public interface Required
    {
        [Id(0), Required]
        UInt32 x { get; set; }
    };

    [Schema]
    public interface StructWithDefaults
    {
    [Id(0), Default(true)]
    bool m_bool_1 { get; set; }
    [Id(1), Default(false)]
    bool m_bool_2 { get; set; }
    [Id(2)]
    bool m_bool_3 { get; set; }

    [Id(3), Default("default string value")]
    string m_str_1 { get; set; }
    [Id(4)]
    string m_str_2 { get; set; }

    [Id(5), Default((sbyte)-127)]
    sbyte m_int8_4 { get; set; }
    [Id(6)]
    sbyte m_int8_5 { get; set; }

    [Id(7), Default((short)-32767)]
    Int16 m_int16_4 { get; set; }
    [Id(8)]
    Int16 m_int16_5 { get; set; }

    [Id(9)]
    Int32 m_int32_4 { get; set; }
    [Id(10), Default((int)2147483647)]
    Int32 m_int32_max { get; set; }

    [Id(11)]
    Int64 m_int64_4 { get; set; }
    [Id(12), Default((long)9223372036854775807)]
    Int64 m_int64_max { get; set; }

    [Id(13), Default((byte)255)]
    byte m_uint8_2 { get; set; }
    [Id(14)]
    byte m_uint8_3 { get; set; }

    [Id(15), Default((ushort)65535)]
    UInt16 m_uint16_2 { get; set; }
    [Id(16)]
    UInt16 m_uint16_3 { get; set; }

    [Id(17)]
    UInt32 m_uint32_3 { get; set; }
    [Id(18), Default((uint)4294967295)]
    UInt32 m_uint32_max { get; set; }

    [Id(19)]
    UInt64 m_uint64_3 { get; set; }
    [Id(20), Default((ulong)0xFFFFFFFFFFFFFFFF)]
    UInt64 m_uint64_max { get; set; }

    [Id(21)]
    double m_double_3 { get; set; }
    [Id(22), Default((double)-123.4567890)]
    double m_double_4 { get; set; }
    [Id(23), Default((double)-0.0)]
    double m_double_5 { get; set; }

    [Id(24)]
    float m_float_3 { get; set; }
    [Id(25), Default((float)2.71828183)]
    float m_float_4 { get; set; }
    [Id(26), Default((float)+0.0)]
    float m_float_7 { get; set; }

    [Id(27), Default(EnumType1.EnumValue1)]
    EnumType1 m_enum1 { get; set; }
    [Id(28), Default(EnumType1.EnumValue3)]
    EnumType1 m_enum2 { get; set; }

    [Id(29), Type(typeof(wstring)), Default("default wstring value")] // how to work with wstring values in csharp??
    string m_wstr_1 { get; set; }
    [Id(30), Type(typeof(wstring))]
    string m_wstr_2 { get; set; }
};
/*
[Schema]
public struct Generic<T>
{
}

[Schema]
public struct Names
{
    [Id(0)]
    Generic<blob> gb;
    [Id(1)]
    Generic<wstring> gws;
    [Id(2)]
    Generic<nullable<BasicTypes>> gnb;
    [Id(3)]
    Generic<map<Int8, EnumType1>> gmie;
}

[Schema]
public struct ListVsNullable
{
    [Id(0)]
    nullable<Int64> nullableInt;
    [Id(1)]
    vector<Int64> vectorInt;
    [Id(2)]
    list<int64> listInt;
    [Id(3)]
    blob blobData;
}
*/
}
#pragma warning restore

