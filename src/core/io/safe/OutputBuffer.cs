// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cdrcs.IO.Safe
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Text;

    /// <summary>
    /// Implements IOutputStream on top of memory buffer
    /// </summary>
    public class OutputBuffer : IOutputStream
    {
        const int BlockCopyMin = 32;
        protected internal byte[] buffer;
        protected internal int position;
        protected internal int length;
        protected internal int prefix_len = 0;

        /// <summary>
        /// Gets data inside the buffer
        /// </summary>
        public ArraySegment<byte> Data
        {
            get { return new ArraySegment<byte>(buffer, 0, position); }
        }

        /// <summary>
        /// Gets or sets the current position within the buffer
        /// </summary>
        public virtual long Position
        {
            get { return position; }
            set { position = checked ((int)value); }
        }

        public OutputBuffer(int length = 64 * 1024)
            : this()
        {
            buffer = ResizeBuffer(null, length);
            this.length = buffer.Length;
        }

        public OutputBuffer(byte[] buffer)
            : this()
        {
            this.buffer = buffer;
            length = buffer.Length;
        }

        private OutputBuffer()
        {
            Debug.Assert(BitConverter.IsLittleEndian);
            position = 0;
        }

        #region IOutputStream

        /// <summary>
        /// Align stream to given size
        /// </summary>
        public void Align(ushort alignment)
        {
            // Note: The 4 starting bytes (for Representation Id and Options) are not considered for alignment
            var modulo = (Position + prefix_len) % alignment;
            if (modulo > 0)
            {
                Console.WriteLine("***** align to {0}  pos={1} => modulo={2} => add {3}", alignment, Position, modulo, alignment - modulo);
                for (int i = 0; i < alignment - modulo; i++)
                {
                    WriteUInt8(0x00);
                }
            }
        }

        /// <summary>
        /// Write Encapsulation Sequence
        /// </summary>
        public void WriteEncapsulation()
        {
            // Note: currently only CDR_LE and CDR_BE without representation options are supported for now
            // see pycdr2 for more details on what's missing
            if (BitConverter.IsLittleEndian)
            {
                WriteUInt8((byte)0x00);
                WriteUInt8((byte)0x01);
                WriteUInt8((byte)0x00);
                WriteUInt8((byte)0x00);
            }
            else
            {
                WriteUInt8((byte)0x00);
                WriteUInt8((byte)0x00);
                WriteUInt8((byte)0x00);
                WriteUInt8((byte)0x00);
            }
            prefix_len = 4;

        }



        /// <summary>
        /// Write boolean
        /// </summary>
        public void WriteBoolean(bool value)
        {
            if (position >= length)
            {
                Grow(sizeof(byte));
            }

            buffer[position++] = value ? (byte)0x01 : (byte)0x00;
        }

        /// <summary>
        /// Write enum as uint32 - maybe templated?
        /// </summary>
        public void WriteEnum(uint value)
        {
            WriteUInt32(value);
        }

        /// <summary>
        /// Write 8-bit unsigned integer
        /// </summary>
        public void WriteUInt8(byte value)
        {
            if (position >= length)
            {
                Grow(sizeof(byte));
            }

            buffer[position++] = value;
        }

        /// <summary>
        /// Write 8-bit integer
        /// </summary>
        public void WriteInt8(sbyte value)
        {
            if (position >= length)
            {
                Grow(sizeof(sbyte));
            }

            buffer[position++] = (byte)value;
        }

        /// <summary>
        /// Write little-endian encoded 16-bit unsigned integer
        /// </summary>
        public void WriteUInt16(ushort value)
        {
            if (position + sizeof(ushort) > length)
            {
                Grow(sizeof(ushort));
            }

            var i = position;
            var b = buffer;
            b[i++] = (byte)value;
            b[i++] = (byte)(value >> 8);
            position = i;
        }

        /// <summary>
        /// Write little-endian encoded 16-bit integer
        /// </summary>
        public void WriteInt16(short value)
        {
            if (position + sizeof(short) > length)
            {
                Grow(sizeof(short));
            }

            var i = position;
            var b = buffer;
            b[i++] = (byte)value;
            b[i++] = (byte)(value >> 8);
            position = i;
        }

        /// <summary>
        /// Write little-endian encoded 32-bit unsigned integer
        /// </summary>
        public virtual void WriteUInt32(uint value)
        {
            if (position + sizeof(uint) > length)
            {
                Grow(sizeof(uint));
            }

            var i = position;
            var b = buffer;
            b[i++] = (byte)value;
            b[i++] = (byte)(value >> 8);
            b[i++] = (byte)(value >> 16);
            b[i++] = (byte)(value >> 24);
            position = i;
        }

        /// <summary>
        /// Write little-endian encoded 32-bit integer
        /// </summary>
        public virtual void WriteInt32(int value)
        {
            if (position + sizeof(int) > length)
            {
                Grow(sizeof(int));
            }

            var i = position;
            var b = buffer;
            b[i++] = (byte)value;
            b[i++] = (byte)(value >> 8);
            b[i++] = (byte)(value >> 16);
            b[i++] = (byte)(value >> 24);
            position = i;
        }

        /// <summary>
        /// Write little-endian encoded 64-bit unsigned integer
        /// </summary>
        public virtual void WriteUInt64(ulong value)
        {
            if (position + sizeof(ulong) > length)
            {
                Grow(sizeof(ulong));
            }

            var i = position;
            var b = buffer;
            b[i++] = (byte)value;
            b[i++] = (byte)(value >> 8);
            b[i++] = (byte)(value >> 16);
            b[i++] = (byte)(value >> 24);
            b[i++] = (byte)(value >> 32);
            b[i++] = (byte)(value >> 40);
            b[i++] = (byte)(value >> 48);
            b[i++] = (byte)(value >> 56);
            position = i;
        }

        /// <summary>
        /// Write little-endian encoded 64-bit unsigned integer
        /// </summary>
        public virtual void WriteInt64(long value)
        {
            if (position + sizeof(long) > length)
            {
                Grow(sizeof(long));
            }

            var i = position;
            var b = buffer;
            b[i++] = (byte)value;
            b[i++] = (byte)(value >> 8);
            b[i++] = (byte)(value >> 16);
            b[i++] = (byte)(value >> 24);
            b[i++] = (byte)(value >> 32);
            b[i++] = (byte)(value >> 40);
            b[i++] = (byte)(value >> 48);
            b[i++] = (byte)(value >> 56);
            position = i;
        }
        
        /// <summary>
                 /// Write little-endian encoded single precision IEEE 754 float
                 /// </summary>
        public virtual void WriteFloat(float value)
        {
            WriteUInt32(new FloatLayout { value = value }.bytes);
        }

        /// <summary>
        /// Write little-endian encoded double precision IEEE 754 float
        /// </summary>
        public virtual void WriteDouble(double value)
        {
            WriteUInt64(new DoubleLayout { value = value }.bytes);
        }

        /// <summary>
        /// Write an array of bytes verbatim
        /// </summary>
        /// <param name="data">Array segment specifying bytes to write</param>
        public virtual void WriteBytes(ArraySegment<byte> data)
        {
            var newOffset = position + data.Count;
            if (newOffset > length)
            {
                Grow(data.Count);
            }

            if (data.Count < BlockCopyMin)
            {
                for (int i = position, j = data.Offset; i < newOffset; ++i, ++j)
                {
                    buffer[i] = data.Array[j];
                }
            }
            else
            {
                Buffer.BlockCopy(data.Array, data.Offset, buffer, position, data.Count);
            }
            position = newOffset;
        }

        /// <summary>
        /// Write UTF-8 or UTF-16 encoded string
        /// </summary>
        /// <param name="encoding">String encoding</param>
        /// <param name="value">String value</param>
        /// <param name="size">Size in bytes of encoded string</param>
        public virtual void WriteString(Encoding encoding, string value, int size)
        {
            if (position + size > length)
            {
                Grow(size);
            }
            position += encoding.GetBytes(value, 0, value.Length, buffer, position);
        }

        #endregion

        // Grow the buffer so that there is enough space to write 'count' bytes
        internal virtual void Grow(int count)
        {
            int minLength = checked(position + count);
            length = checked(length + (length >> 1));

            const int ArrayIndexMaxValue = 0x7FFFFFC7;
            if ((uint)length > ArrayIndexMaxValue)
            {
                length = ArrayIndexMaxValue;
            }
            else if (length < minLength)
            {
                length = minLength;
            }

            buffer = ResizeBuffer(buffer, length);
            length = buffer.Length;
        }

        /// <summary>
        /// Resize the internal buffer.
        /// </summary>
        /// <param name="buffer">Existing buffer.</param>
        /// <param name="newSize">New buffer size.</param>
        /// <returns>The new buffer.</returns>
        /// <remarks>
        /// <para>
        /// Implementations are responsible for ensuring that the new buffer
        /// is at least <paramref name="newSize"/> bytes large and that the
        /// bytes in <paramref name="buffer"/> are copied to the new buffer.
        /// </para>
        /// </remarks>
        protected virtual byte[] ResizeBuffer(byte[] buffer, int newSize)
        {
            Array.Resize(ref buffer, newSize);
            return buffer;
        }

        #region layouts

        [StructLayout(LayoutKind.Explicit)]
        struct DoubleLayout
        {
            [FieldOffset(0)]
            public readonly ulong bytes;

            [FieldOffset(0)]
            public double value;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct FloatLayout
        {
            [FieldOffset(0)]
            public readonly uint bytes;

            [FieldOffset(0)]
            public float value;
        }

        #endregion
    }
}
