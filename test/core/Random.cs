namespace UnitTest
{
    using System;
    using System.Text;
    using Cdrcs;
    using Cdrcs.Protocols;
    using Cdrcs.IO;

    internal static class Random
    {
        static readonly System.Random random;

        static Random()
        {
            var seed = (int)DateTime.Now.ToBinary();
            random = new System.Random(seed);
            System.Diagnostics.Debug.WriteLine("Random seed {0}", seed);
        }

        public static T Init<T>()
        {
            return Deserialize<T>.From(new RandomReader(random));
        }

        public static T Init<T>(IFactory factory)
        {
            return new Deserializer<RandomReader>(typeof(T), factory)
                .Deserialize<T>(new RandomReader(random));
        }

        public static T Init<T>(Factory factory)
        {
            return new Deserializer<RandomReader>(typeof(T), factory)
                .Deserialize<T>(new RandomReader(random));
        }
    }

    internal class RandomReader : IClonableProtocolReader, ICloneable<RandomReader>
    {
        readonly System.Random random;

        const int MaxStringLength = 50;
        const int MaxContainerLength = 5;
        const int MaxContainerDepth = 5;

        int level;

        public RandomReader(System.Random random)
        {
            this.random = random;
        }

        RandomReader ICloneable<RandomReader>.Clone()
        {
            return this;
        }

        IClonableProtocolReader ICloneable<IClonableProtocolReader>.Clone()
        {
            return this;
        }

        public bool ReadFieldOmitted()
        {
            return false;
        }

        public int ReadContainerBegin()
        {
            if (++level == MaxContainerDepth)
                return 0;

            return random.Next(MaxContainerLength);
        }

        public void ReadContainerEnd()
        {
            --level;
        }

        public sbyte ReadInt8()
        {
            return (sbyte)random.Next(sbyte.MinValue, sbyte.MaxValue);
        }

        public void SkipInt8()
        { }

        public short ReadInt16()
        {
            return (short)random.Next(short.MinValue, short.MaxValue);
        }

        public void SkipInt16()
        { }

        public int ReadInt32()
        {
            return random.Next(int.MinValue, int.MaxValue);
        }

        public void SkipInt32()
        { }

        public long ReadInt64()
        {
            return BitConverter.ToInt64(GetRandomBytes(sizeof(long)), 0);
        }

        public void SkipInt64()
        { }

        public byte ReadUInt8()
        {
            return (byte)random.Next(byte.MinValue, byte.MaxValue);
        }

        public void SkipUInt8()
        { }

        public ushort ReadUInt16()
        {
            return (ushort)random.Next(ushort.MinValue, ushort.MaxValue);
        }

        public void SkipUInt16()
        { }

        public uint ReadUInt32()
        {
            return (uint)random.Next();
        }

        public void SkipUInt32()
        { }

        public ulong ReadUInt64()
        {
            return (ulong)random.Next();
        }

        public void SkipUInt64()
        { }

        public float ReadFloat()
        {
            return 3.14159F * random.Next(-100, 100);
        }

        public void SkipFloat()
        { }

        public double ReadDouble()
        {
            return ReadFloat();
        }

        public void SkipDouble()
        { }

        public ArraySegment<byte> ReadBytes(int count)
        {
            return new ArraySegment<byte>(GetRandomBytes(count));
        }

        public void SkipBytes(int count)
        { }

        public bool ReadBool()
        {
            return random.Next(0, 2) == 1;
        }

        public void SkipBool()
        { }

        public string ReadString()
        {
            var length = random.Next(MaxStringLength);
            var builder = new StringBuilder(length);

            for (var i = 0; i < length; i++)
            {
                builder.Append((char)(random.Next(32, 126)));
            }

            return builder.ToString();
        }

        public void SkipString()
        { }

        public string ReadWString()
        {
            return ReadString();
        }

        public void SkipWString()
        { }

        byte[] GetRandomBytes(int length)
        {                   
            var array = new byte[length];
            random.NextBytes(array);
            return array;
        }


        // missing interface methods ..

        void IProtocolReader.ReadStructBegin()
        {
            
        }

        void IProtocolReader.ReadBaseBegin()
        {
            
        }

        void IProtocolReader.ReadStructEnd()
        {
            
        }

        void IProtocolReader.ReadBaseEnd()
        {
            
        }

        void IProtocolReader.ReadFieldBegin(out CdrcsDataType type, out ushort id)
        {
            type = CdrcsDataType.BT_UNAVAILABLE;
            id = 0;
        }

        void IProtocolReader.ReadFieldEnd()
        {
            
        }

        void IProtocolReader.ReadContainerBegin(out int count, out CdrcsDataType elementType)
        {
            elementType = CdrcsDataType.BT_UNAVAILABLE;
            count = 0;
        }

        void IProtocolReader.ReadFixedContainerBegin(out int count, out CdrcsDataType elementType)
        {
            elementType = CdrcsDataType.BT_UNAVAILABLE;
            count = 0;
        }

        void IProtocolReader.ReadContainerBegin(out int count, out CdrcsDataType keyType, out CdrcsDataType valueType)
        {
            keyType = CdrcsDataType.BT_UNAVAILABLE;
            valueType = CdrcsDataType.BT_UNAVAILABLE;
            count = 0;
        }

        void IProtocolReader.Skip(CdrcsDataType type)
        {
        }

        ArraySegment<byte> IProtocolReader.ReadBytes()
        {
            var array = new byte[] { 5, 8, 9, 20, 70, 44, 2, 4 };
            return new ArraySegment<byte>(array, 2, 3);
        }

        uint IProtocolReader.ReadEnum()
        {
            return 1;
        }
    }
}
