// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cdrcs.IO.Safe
{
    using System;
    using System.Buffers.Binary;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using Cdrcs.Protocols;

    /// <summary>
    /// Implements IInputStream on top of memory buffer
    /// </summary>
    public class InputBuffer : IInputStream, ICloneable<InputBuffer>
    {
        readonly int offset;
        protected internal byte[] buffer;
        protected internal int end;
        protected internal int position;
        protected internal int prefix_len = 0;

        protected internal bool is_little_endian = true;
        protected internal RepresentationId representation_id;
        protected internal ushort representation_options;


        public virtual long Length
        {
            get { return end - offset; }
        }

        public virtual long Position
        {
            get { return position - offset; }
            set { position = offset + checked ((int)value); }
        }

        public InputBuffer(byte[] data)
            : this(data, 0, data.Length)
        {}

        public InputBuffer(byte[] data, int length)
            : this(data, 0, length)
        {}

        public InputBuffer(ArraySegment<byte> seg)
            : this(seg.Array, seg.Offset, seg.Count)
        {}

        public InputBuffer(byte[] data, int offset, int length)
        {
            is_little_endian = BitConverter.IsLittleEndian;
            buffer = data;
            this.offset = offset;
            end = offset + length;
            position = offset;

        }

        internal InputBuffer(InputBuffer that)
            : this(that.buffer, that.position, that.end - that.position)
        {}


        /// <summary>
        /// Create a clone of the current state of the buffer
        /// </summary>
        public InputBuffer Clone()
        {
            return new InputBuffer(this);
        }


        /// <summary>
        /// Align stream to given size
        /// </summary>
        public void Align(ushort alignment)
        {
            // Note: The 4 starting bytes (for Representation Id and Options) are not considered for alignment
            var modulo = (Position + prefix_len) % alignment;
            if (modulo > 0)
            {
                // use skip-bytes here ??
                Console.WriteLine("***** align to {0}  pos={1} => modulo={2} => add {3}", alignment, Position, modulo, alignment - modulo);
                for (int i = 0; i < alignment - modulo; i++)
                {
                    ReadUInt8();
                }
            }
        }

        /// <summary>
        /// Skip forward specified number ot bytes
        /// </summary>
        /// <param name="count">Number of bytes to skip</param>
        /// <exception cref="EndOfStreamException"/>
        public void SkipBytes(int count)
        {
            position = checked(position + count);
        }

        /// <summary>
        /// Write Encapsulation Sequence
        /// </summary>
        public void ReadEncapsulation()
        {
            ushort rid = ReadUInt16();
            if (!Enum.IsDefined(typeof(RepresentationId), (int)rid))
            {
                throw new UnsupportedCDRRepresentation(rid);
            }
            this.representation_id = (RepresentationId)rid;

            // NOTE: we currently only support CDR_BE and CDR_LE
            if (this.representation_id != RepresentationId.CDR_BE && this.representation_id != RepresentationId.CDR_LE)
            {
                throw new UnsupportedCDRRepresentation(rid);
            }
            this.representation_options = ReadUInt16();
            this.is_little_endian= (rid & 0x0001) == 0x0001;
            this.prefix_len = 4;
        }

        /// <summary>
        /// Read 8-bit unsigned integer
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        public byte ReadUInt8()
        {
            if (position >= end)
            {
                EndOfStream(1);
            }
            return buffer[position++];
        }

        /// <summary>
        /// Read 8-bit integer
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        public sbyte ReadInt8()
        {
            if (position >= end)
            {
                EndOfStream(1);
            }
            return (sbyte)buffer[position++];
        }

        /// <summary>
        /// Read little-endian encoded 16-bit unsigned integer
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        public virtual ushort ReadUInt16()
        {
            if (position > end - sizeof(ushort))
            {
                EndOfStream(sizeof(ushort));
            }
            uint result = buffer[position++];
            result |= ((uint)buffer[position++]) << 8;
            return (ushort)result;
        }

        /// <summary>
        /// Read little-endian encoded 16-bit integer
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        public virtual short ReadInt16()
        {
            if (position > end - sizeof(short))
            {
                EndOfStream(sizeof(short));
            }
            int result = buffer[position++];
            result |= ((int)buffer[position++]) << 8;
            return (short)result;
        }

        /// <summary>
        /// Read little-endian encoded 32-bit unsigned integer
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        public virtual uint ReadUInt32()
        {
            if (position > end - sizeof(uint))
            {
                EndOfStream(sizeof(uint));
            }
            uint result = buffer[position++];
            result |= ((uint)buffer[position++]) << 8;
            result |= ((uint)buffer[position++]) << 16;
            result |= ((uint)buffer[position++]) << 24;
            return result;
        }

        /// <summary>
        /// Read little-endian encoded 32-bit integer
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        public virtual int ReadInt32()
        {
            if (position > end - sizeof(int))
            {
                EndOfStream(sizeof(int));
            }
            int result = buffer[position++];
            result |= ((int)buffer[position++]) << 8;
            result |= ((int)buffer[position++]) << 16;
            result |= ((int)buffer[position++]) << 24;
            return result;
        }

        /// <summary>
        /// Read little-endian encoded 64-bit unsigned integer
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        public virtual ulong ReadUInt64()
        {
            if (position > end - sizeof(ulong))
            {
                EndOfStream(sizeof(ulong));
            }
            var result = BitConverter.ToUInt64(buffer, position);
            position += sizeof(ulong);
            return result;
        }

        /// <summary>
        /// Read little-endian encoded 64-bit integer
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        public virtual long ReadInt64()
        {
            if (position > end - sizeof(long))
            {
                EndOfStream(sizeof(long));
            }
            var result = BitConverter.ToInt64(buffer, position);
            position += sizeof(long);
            return result;
        }

        /// <summary>
        /// Read little-endian encoded single precision IEEE 754 float
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        public virtual float ReadFloat()
        {
            if (position > end - sizeof(float))
            {
                EndOfStream(sizeof(float));
            }
            var result = BitConverter.ToSingle(buffer, position);
            position += sizeof(float);
            return result;
        }

        /// <summary>
        /// Read little-endian encoded double precision IEEE 754 float
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        public virtual double ReadDouble()
        {
            if (position > end - sizeof(double))
            {
                EndOfStream(sizeof(double));
            }
            var result = BitConverter.ToDouble(buffer, position);
            position += sizeof(double);
            return result;
        }

        /// <summary>
        /// Read boolean
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        public virtual bool ReadBoolean()
        {
            if (position > end - sizeof(byte))
            {
                EndOfStream(sizeof(byte));
            }
            byte result = buffer[position++];
            return result != 0x00;
        }

        /// <summary>
        /// Read enum
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        public virtual uint ReadEnum()
        {
            return ReadUInt32();
        }

        /// <summary>
        /// Read an array of bytes verbatim
        /// </summary>
        /// <param name="count">Number of bytes to read</param>
        /// <exception cref="EndOfStreamException"/>
        public virtual ArraySegment<byte> ReadBytes(int count)
        {
            if (position > end - count)
            {
                EndOfStream(count);
            }
            var result = new ArraySegment<byte>(buffer, position, count);
            position += count;
            return result;
        }

        /// <summary>
        /// Read UTF-8 or UTF-16 encoded string
        /// </summary>
        /// <param name="encoding">String encoding</param>
        /// <param name="size">Size of payload in bytes</param>
        public virtual string ReadString(Encoding encoding, int size)
        {
            if (position > end - size)
            {
                EndOfStream(size);
            }
            var result = encoding.GetString(buffer, position, size);
            position += size;
            return result;
        }

        protected internal virtual void EndOfStream(int count)
        {
            Throw.EndOfStreamException();
        }
    }
}
