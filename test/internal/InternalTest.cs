namespace InternalTest
{
    using System.Collections.Generic;
    using System;
    using Cdrcs.Tag;

    enum EnumType1
    {
        EnumValue1 = 5,
        EnumValue2 = 10,
        EnumValue3 = -10,
        EnumValue4 = 0x2A,
        EnumValue5 = 9, //-10 in two's complement (cannot be expressed in csharp)
    };

    [Cdrcs.Schema]
    public class BasicTypes
    {

        [Cdrcs.Id(0)]
        [Cdrcs.Attribute("name", "Boolean")]
        bool _bool;

        [Cdrcs.Id(2)]
        string _str;

        [Cdrcs.Id(3), Cdrcs.Type(typeof(wstring))]
        string _wstr;

        [Cdrcs.Id(14)]
        Int8 _int8;

        [Cdrcs.Id(15)]
        Int16 _int16;

        [Cdrcs.Id(16)]
        Int32 _int32;

        [Cdrcs.Id(17)]
        Int64 _int64;

        [Cdrcs.Id(13)]
        UInt8 _uint8;

        [Cdrcs.Id(11)]
        UInt16 _uint16;

        [Cdrcs.Id(12)]
        UInt32 _uint32;

        [Cdrcs.Id(10)]
        UInt64 _uint64;

        [Cdrcs.Id(18)]
        double _double;

        [Cdrcs.Id(20)]
        float _float;

        [Cdrcs.Id(21), Default(EnumValue1)]
        EnumType1 _enum1;
    };


    [Cdrcs.Schema]
    public struct Required
    {
        [Cdrcs.Id(0)]
        required uint32 x;
    };

    [Cdrcs.Schema]
    public struct public structWithDefaults
    {
    [Cdrcs.Id(0), Default(true)]
    bool m_bool_1;
    [Cdrcs.Id(1), Default(false)]
    bool m_bool_2;
    [Cdrcs.Id(2)]
    bool m_bool_3;

    [Cdrcs.Id(3), Default("default string value")]
    string m_str_1;
    [Cdrcs.Id(4)]
    string m_str_2;

    [Cdrcs.Id(5), Default(-127)]
    Int8 m_int8_4;
    [Cdrcs.Id(6)]
    Int8 m_int8_5;

    [Cdrcs.Id(7), Default(-32767)]
    Int16 m_int16_4;
    [Cdrcs.Id(8)]
    Int16 m_int16_5;

    [Cdrcs.Id(9)]
    Int32 m_int32_4;
    [Cdrcs.Id(10), Default(2147483647)]
    Int32 m_int32_max;

    [Cdrcs.Id(11)]
    Int64 m_int64_4;
    [Cdrcs.Id(12), Default(9223372036854775807)]
    Int64 m_int64_max;

    [Cdrcs.Id(13), Default(255)]
    UInt8 m_uint8_2;
    [Cdrcs.Id(14)]
    UInt8 m_uint8_3;

    [Cdrcs.Id(15), Default(65535)]
    UInt16 m_uint16_2;
    [Cdrcs.Id(16)]
    UInt16 m_uint16_3;

    [Cdrcs.Id(17)]
    UInt32 m_uint32_3;
    [Cdrcs.Id(18), Default(4294967295)]
    UInt32 m_uint32_max;

    [Cdrcs.Id(19)]
    UInt64 m_uint64_3;
    [Cdrcs.Id(20), Default(0xFFFFFFFFFFFFFFFF)]
    UInt64 m_uint64_max;

    [Cdrcs.Id(21)]
    double m_double_3;
    [Cdrcs.Id(22), Default(-123.4567890)]
    double m_double_4;
    [Cdrcs.Id(23), Default(-0.0)]
    double m_double_5;

    [Cdrcs.Id(24)]
    float m_float_3;
    [Cdrcs.Id(25), Default(2.71828183)]
    float m_float_4;
    [Cdrcs.Id(26), Default(+0.0)]
    float m_float_7;

    [Cdrcs.Id(27), Default(EnumValue1)]
    EnumType1 m_enum1;
    [Cdrcs.Id(28), Default(EnumValue3)]
    EnumType1 m_enum2;

    [Cdrcs.Id(29), Cdrcs.Type(typeof(wstring)), Default(L"default wstring value")]
    string m_wstr_1;
    [Cdrcs.Id(30), Cdrcs.Type(typeof(wstring))]
    string m_wstr_2;
};
/*
[Cdrcs.Schema]
public struct Generic<T>
{
}

[Cdrcs.Schema]
public struct Names
{
    [Cdrcs.Id(0)]
    Generic<blob> gb;
    [Cdrcs.Id(1)]
    Generic<wstring> gws;
    [Cdrcs.Id(2)]
    Generic<nullable<BasicTypes>> gnb;
    [Cdrcs.Id(3)]
    Generic<map<Int8, EnumType1>> gmie;
}

[Cdrcs.Schema]
public struct ListVsNullable
{
    [Cdrcs.Id(0)]
    nullable<Int64> nullableInt;
    [Cdrcs.Id(1)]
    vector<Int64> vectorInt;
    [Cdrcs.Id(2)]
    list<int64> listInt;
    [Cdrcs.Id(3)]
    blob blobData;
}
*/
}

