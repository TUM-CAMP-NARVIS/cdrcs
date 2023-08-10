// by Ulrich Eck

#region DDS CDR format
/*
see OMG Specification ..
*/
#endregion

namespace Cdrcs.Protocols
{
    using System;
    using System.Buffers.Binary;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Cdrcs.IO;


    public enum RepresentationId
    {
        CDR_BE = 0x0000,
        CDR_LE = 0x0001,
        PL_CDR_LE = 0x0002,
        PL_CDR_BE = 0x0003,
        CDR2_BE = 0x0010,
        CDR2_LE = 0x0011,
        PL_CDR2_BE = 0x0012,
        PL_CDR2_LE = 0x0013,
        D_CDR_BE = 0x0014,
        D_CDR_LE = 0x0015,
        XML = 0x0004,
    }

    public class UnsupportedCDRRepresentation : Exception
    {
        private readonly ushort _rid;

        public UnsupportedCDRRepresentation(ushort rid)
        {
            _rid = rid;
        }

        public override string ToString()
        {
            string ridName = Enum.IsDefined(typeof(RepresentationId), (int)_rid) ?
                String.Format(" ({0})", ((RepresentationId)_rid).ToString()) : "";
            return String.Format("Unsupported CDR representation: 0x{0:X4}{1}", _rid, ridName);
        }
    }

    /// <summary>
    /// Writer for the DDS CDR protocol
    /// </summary>
    /// <typeparam name="O">Implementation of IOutputStream interface</typeparam>
    [Reader(typeof(DdsCdrReader<>))]
    public struct DdsCdrWriter<O> : IProtocolWriter
        where O : IOutputStream
    {
        const ushort Magic = (ushort)ProtocolType.DDS_CDR_PROTOCOL;
        readonly O output;

        /// <summary>
        /// Create an instance of DdsCdrWriter
        /// </summary>
        /// <param name="output">Serialized payload output</param>
        public DdsCdrWriter(O output)
        {
            this.output = output;
        }

        /// <summary>
        /// Write protocol magic number and version
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteEncapsulation()
        {
            output.WriteEncapsulation();
        }

        #region Complex types
        /// <summary>
        /// Start writing a struct
        /// </summary>
        /// <param name="metadata">Schema metadata</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteStructBegin(Metadata metadata)
        {}

        /// <summary>
        /// Start writing a base struct
        /// </summary>
        /// <param name="metadata">Base schema metadata</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBaseBegin(Metadata metadata)
        {}

        /// <summary>
        /// End writing a struct
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteStructEnd()
        {
        }

        /// <summary>
        /// End writing a base struct
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBaseEnd()
        {
        }

        /// <summary>
        /// Start writing a field
        /// </summary>
        /// <param name="type">Type of the field</param>
        /// <param name="id">Identifier of the field</param>
        /// <param name="metadata">Metadata of the field</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteFieldBegin(CdrcsDataType type, ushort id, Metadata metadata)
        {
            // here we could also manage the alignment instead of each individual handler function..
        }


        /// <summary>
        /// Indicate that field was omitted because it was set to its default value
        /// </summary>
        /// <param name="dataType">Type of the field</param>
        /// <param name="id">Identifier of the field</param>
        /// <param name="metadata">Metadata of the field</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteFieldOmitted(CdrcsDataType dataType, ushort id, Metadata metadata)
        {
            Console.Write("FieldOmitted should not be called!!");
        }


        /// <summary>
        /// End writing a field
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteFieldEnd()
        {}

        /// <summary>
        /// Start writing a list or set container
        /// missing: add_size_header feature
        /// </summary>
        /// <param name="count">Number of elements in the container</param>
        /// <param name="elementType">Type of the elements</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteContainerBegin(int count, CdrcsDataType elementType)
        {
            output.Align(4);
            output.WriteUInt32((uint)count);
        }

        /// <summary>
        /// Start writing a fixed size list or set container
        /// missing: add_size_header feature
        /// </summary>
        /// <param name="count">Number of elements in the container</param>
        /// <param name="elementType">Type of the elements</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteFixedContainerBegin(int count, CdrcsDataType elementType)
        {
        }

        /// <summary>
        /// Start writing a map container
        /// </summary>
        /// <param name="count">Number of elements in the container</param>
        /// <param name="keyType">Type of the keys</param>
        /// /// <param name="valueType">Type of the values</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteContainerBegin(int count, CdrcsDataType keyType, CdrcsDataType valueType)
        {
            output.Align(4);
            output.WriteUInt32((uint)count);
        }

        /// <summary>
        /// End writing a container
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteContainerEnd()
        {}

        /// <summary>
        /// Write array of bytes verbatim
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBytes(ArraySegment<byte> data)
        {
            output.Align(4);
            output.WriteUInt32((uint)data.Count);
            output.WriteBytes(data);
        }

        /// <summary>
        /// Write array of bytes verbatim
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByteArray(ArraySegment<byte> data)
        {
            //Align(1)
            output.WriteBytes(data);
        }
        #endregion

        #region Primitive types
        /// <summary>
        /// Write an UInt8
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt8(Byte value)
        {
            output.Align(1);
            output.WriteUInt8(value);
        }

        /// <summary>
        /// Write an UInt16
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt16(UInt16 value)
        {
            output.Align(2);
            output.WriteUInt16(value);
        }

        /// <summary>
        /// Write an UInt16
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt32(UInt32 value)
        {
            output.Align(4);
            output.WriteUInt32(value);
        }

        /// <summary>
        /// Write an UInt64
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt64(UInt64 value)
        {
            output.Align(8);
            output.WriteUInt64(value);
        }

        /// <summary>
        /// Write an Int8
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt8(SByte value)
        {
            output.Align(1);
            output.WriteUInt8((Byte)value);
        }

        /// <summary>
        /// Write an Int16
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt16(Int16 value)
        {
            output.Align(2);
            output.WriteUInt16((ushort)value);
        }

        /// <summary>
        /// Write an Int32
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt32(Int32 value)
        {
            output.Align(4);
            output.WriteUInt32((uint)value);
        }

        /// <summary>
        /// Write an Int64
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt64(Int64 value)
        {
            output.Align(4);
            output.WriteUInt64((ulong)value);
        }

        /// <summary>
        /// Write a float
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteFloat(float value)
        {
            output.Align(4);
            output.WriteFloat(value);
        }

        /// <summary>
        /// Write a double
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteDouble(double value)
        {
            output.Align(8);
            output.WriteDouble(value);
        }

        /// <summary>
        /// Write a bool
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBool(bool value)
        {
            output.Align(1);
            output.WriteBoolean(value);
        }

        /// <summary>
        /// Write a enum
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteEnum(uint value)
        {
            output.Align(4);
            output.WriteUInt32(value);
        }

        /// <summary>
        /// Write a UTF-8 string
        /// missing features: bounded string with predefined max-size
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteString(string value)
        {
            output.Align(4);
            if (value.Length == 0)
            {
                output.WriteUInt32(0);
            }
            else
            {
                var size = Encoding.UTF8.GetByteCount(value);
                output.WriteUInt32((UInt32)size + 1);
                output.WriteString(Encoding.UTF8, value, size);
                output.WriteUInt8((byte)0x00);
            }
        }

        /// <summary>
        /// Write a UTF-16 string
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteWString(string value)
        {
            output.Align(4);
            if (value.Length == 0)
            {
                output.WriteUInt32(0);
            }
            else
            {
                int byteSize = checked(value.Length * 2);
                output.WriteUInt32((UInt32)value.Length + 1);
                output.Align(2);
                output.WriteString(Encoding.Unicode, value, byteSize);
                output.WriteUInt8((byte)0x00);
            }
        }

        #endregion
    }

    /// <summary>
    /// Reader for the DDS CDR protocol
    /// </summary>
    /// <typeparam name="I">Implementation of IInputStream interface</typeparam>
    public struct DdsCdrReader<I> : IClonableProtocolReader, ICloneable<DdsCdrReader<I>>
        where I : IInputStream, ICloneable<I>
    {
        readonly I input;

        /// <summary>
        /// Create an instance of DdsCdrReader
        /// </summary>
        /// <param name="input">Input payload</param>
        public DdsCdrReader(I input)
        {
            this.input = input;
        }

        /// <summary>
        /// Clone the reader
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        DdsCdrReader<I> ICloneable<DdsCdrReader<I>>.Clone()
        {
            return new DdsCdrReader<I>(input.Clone());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IClonableProtocolReader ICloneable<IClonableProtocolReader>.Clone()
        {
            return (this as ICloneable<DdsCdrReader<I>>).Clone();
        }

        #region Complex types

        /// <summary>
        /// Start reading a struct
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadStructBegin()
        { }

        /// <summary>
        /// Start reading a base of a struct
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadBaseBegin()
        { }

        /// <summary>
        /// End reading a struct
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadStructEnd()
        { }

        /// <summary>
        /// End reading a base of a struct
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadBaseEnd()
        { }

        /// <summary>
        /// Start reading a field
        /// </summary>
        /// <param name="type">An out parameter set to the field type
        /// or BT_STOP/BT_STOP_BASE if there is no more fields in current struct/base</param>
        /// <param name="id">Out parameter set to the field identifier</param>
        /// <exception cref="EndOfStreamException"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadFieldBegin(out CdrcsDataType type, out ushort id)
        {
            // CDR does not support reading the types unless we're using x-types extensions
            type = CdrcsDataType.BT_UNAVAILABLE;
            id = 0;
        }

        /// <summary>
        /// End reading a field
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadFieldEnd()
        { }

        /// <summary>
        /// Start reading a list or set container
        /// </summary>
        /// <param name="count">An out parameter set to number of items in the container</param>
        /// <param name="elementType">An out parameter set to type of container elements</param>
        /// <exception cref="EndOfStreamException"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadContainerBegin(out int count, out CdrcsDataType elementType)
        {
            // CDR does not support reading the types unless we're using x-types extensions
            elementType = CdrcsDataType.BT_UNAVAILABLE;
            input.Align(4);
            count = checked((int)input.ReadUInt32());
        }

        /// <summary>
        /// Start reading a fixed-size list or set container
        /// </summary>
        /// <param name="count">An out parameter set to number of items in the container</param>
        /// <param name="elementType">An out parameter set to type of container elements</param>
        /// <exception cref="EndOfStreamException"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadFixedContainerBegin(out int count, out CdrcsDataType elementType)
        {
            // CDR does not support reading the types unless we're using x-types extensions
            elementType = CdrcsDataType.BT_UNAVAILABLE;
            count = 0;
        }

        /// <summary>
        /// Start reading a map container
        /// </summary>
        /// <param name="count">An out parameter set to number of items in the container</param>
        /// <param name="keyType">An out parameter set to the type of map keys</param>
        /// <param name="valueType">An out parameter set to the type of map values</param>
        /// <exception cref="EndOfStreamException"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadContainerBegin(out int count, out CdrcsDataType keyType, out CdrcsDataType valueType)
        {
            // CDR does not support reading the types unless we're using x-types extensions
            keyType = CdrcsDataType.BT_UNAVAILABLE;
            valueType = CdrcsDataType.BT_UNAVAILABLE;
            input.Align(4);
            count = checked((int)input.ReadUInt32());
        }

        /// <summary>
        /// End reading a container
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadContainerEnd()
        { }

        #endregion

        #region Primitive types

        /// <summary>
        /// Read an UInt8
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadUInt8()
        {
            input.Align(1);
            return input.ReadUInt8();
        }

        /// <summary>
        /// Read an UInt16
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadUInt16()
        {
            input.Align(2);
            return input.ReadUInt16();
        }

        /// <summary>
        /// Read an UInt32
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUInt32()
        {
            input.Align(4);
            return input.ReadUInt32();
        }

        /// <summary>
        /// Read an UInt64
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UInt64 ReadUInt64()
        {
            input.Align(8);
            return input.ReadUInt64();
        }

        /// <summary>
        /// Read an Int8
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte ReadInt8()
        {
            input.Align(1);
            return (sbyte)input.ReadUInt8();
        }

        /// <summary>
        /// Read an Int16
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadInt16()
        {
            input.Align(2);
            return (short)input.ReadUInt16();
        }

        /// <summary>
        /// Read an Int32
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt32()
        {
            input.Align(4);
            return (int)input.ReadUInt32();
        }

        /// <summary>
        /// Read an Int64
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadInt64()
        {
            input.Align(8);
            return (long)input.ReadUInt64();
        }

        /// <summary>
        /// Read a bool
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadBool()
        {
            return input.ReadUInt8() != 0;
        }

        /// <summary>
        /// Read an enum
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadEnum()
        {
            input.Align(4);
            return input.ReadUInt32();
        }

        /// <summary>
        /// Read a float
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadFloat()
        {
            input.Align(4);
            return input.ReadFloat();
        }

        /// <summary>
        /// Read a double
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ReadDouble()
        {
            input.Align(8);
            return input.ReadDouble();
        }

        /// <summary>
        /// Read a UTF-8 string
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public String ReadString()
        {
            input.Align(4);
            var length = checked((int)input.ReadUInt32());
            return length == 0 ? string.Empty : input.ReadString(Encoding.UTF8, length);
        }

        /// <summary>
        /// Read a UTF-16 string
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadWString()
        {
            input.Align(4);
            var length = checked((int)(input.ReadUInt32() * 2));
            input.Align(2);
            return length == 0 ? string.Empty : input.ReadString(Encoding.Unicode, length);
        }

        /// <summary>
        /// Read an array of bytes verbatim
        /// </summary>
        /// <param name="count">Number of bytes to read</param>
        /// <exception cref="EndOfStreamException"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArraySegment<byte> ReadBytes(int count)
        {
            return input.ReadBytes(count);
        }

        /// <summary>
        /// Read an array of bytes verbatim
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArraySegment<byte> ReadBytes()
        {
            input.Align(4);
            int count = (int)ReadUInt32();
            return input.ReadBytes(count);
        }
        #endregion

        #region Skip
        /// <summary>
        /// Skip a value of specified type
        /// </summary>
        /// <param name="type">Type of the value to skip</param>
        /// <exception cref="EndOfStreamException"/>
        public void Skip(CdrcsDataType type)
        {
            switch (type)
            {
                case (CdrcsDataType.BT_BOOL):
                case (CdrcsDataType.BT_UINT8):
                case (CdrcsDataType.BT_INT8):
                    input.SkipBytes(sizeof(byte));
                    break;
                case (CdrcsDataType.BT_UINT16):
                case (CdrcsDataType.BT_INT16):
                    input.SkipBytes(sizeof(ushort));
                    break;
                case (CdrcsDataType.BT_UINT32):
                case (CdrcsDataType.BT_INT32):
                    input.SkipBytes(sizeof(uint));
                    break;
                case (CdrcsDataType.BT_FLOAT):
                    input.SkipBytes(sizeof(float));
                    break;
                case (CdrcsDataType.BT_DOUBLE):
                    input.SkipBytes(sizeof(double));
                    break;
                case (CdrcsDataType.BT_UINT64):
                case (CdrcsDataType.BT_INT64):
                    input.SkipBytes(sizeof(ulong));
                    break;
                case (CdrcsDataType.BT_STRING):
                    input.SkipBytes(checked(((int)input.ReadUInt32())));
                    break;
                case (CdrcsDataType.BT_WSTRING):
                    input.SkipBytes(checked((int)(input.ReadUInt32()) * 2));
                    break;
                case CdrcsDataType.BT_LIST:
                case CdrcsDataType.BT_SET:
                    SkipContainer();
                    break;
                case CdrcsDataType.BT_MAP:
                    SkipMap();
                    break;
                case CdrcsDataType.BT_STRUCT:
                    SkipStruct();
                    break;
                default:
                    Throw.InvalidCdrcsDataType(type);
                    break;
            }
        }


        void SkipContainer()
        {
            CdrcsDataType elementType;
            int count;

            ReadContainerBegin(out count, out elementType);

            switch (elementType)
            {
                case CdrcsDataType.BT_BOOL:
                case CdrcsDataType.BT_INT8:
                case CdrcsDataType.BT_UINT8:
                    input.SkipBytes(count);
                    break;
                case (CdrcsDataType.BT_UINT16):
                case (CdrcsDataType.BT_INT16):
                    input.SkipBytes(checked(count * sizeof(ushort)));
                    break;
                case (CdrcsDataType.BT_UINT32):
                case (CdrcsDataType.BT_INT32):
                    input.SkipBytes(checked(count * sizeof(uint)));
                    break;
                case (CdrcsDataType.BT_UINT64):
                case (CdrcsDataType.BT_INT64):
                    input.SkipBytes(checked(count * sizeof(ulong)));
                    break;
                case CdrcsDataType.BT_FLOAT:
                    input.SkipBytes(checked(count * sizeof(float)));
                    break;
                case CdrcsDataType.BT_DOUBLE:
                    input.SkipBytes(checked(count * sizeof(double)));
                    break;
                default:
                    while (0 <= --count)
                    {
                        Skip(elementType);
                    }
                    break;
            }
        }

        void SkipMap()
        {
            CdrcsDataType keyType;
            CdrcsDataType valueType;
            int count;

            ReadContainerBegin(out count, out keyType, out valueType);
            while (0 <= --count)
            {
                Skip(keyType);
                Skip(valueType);
            }
        }

        void SkipStruct()
        {
            while (true)
            {
                CdrcsDataType type;
                ushort id;

                ReadFieldBegin(out type, out id);

                if (type == CdrcsDataType.BT_STOP_BASE) continue;
                if (type == CdrcsDataType.BT_STOP) break;

                Skip(type);
            }
        }
        #endregion
    }
}
