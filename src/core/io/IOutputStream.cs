// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cdrcs.IO
{
    using System;
    using System.Text;

    /// <summary>
    /// Writes primitive data types as binary values in a specific encoding
    /// </summary>
    public interface IOutputStream
    {
        /// <summary>
        /// Gets or sets the position within the stream
        /// </summary>
        /// <exception cref="NotSupportedException"/>
        long Position { get; set; }

        /// <summary>
        /// Align stream to given size
        /// </summary>
        void Align(ushort alignment);

        /// <summary>
        /// Write Encapsulation Sequence
        /// </summary>
        void WriteEncapsulation();

        /// <summary>
        /// Write 8-bit unsigned integer
        /// </summary>
        void WriteUInt8(byte value);

        /// <summary>
        /// Write little-endian encoded 16-bit unsigned integer
        /// </summary>
        void WriteUInt16(ushort value);

        /// <summary>
        /// Write little-endian encoded 16-bit integer
        /// </summary>
        void WriteInt16(short value);

        /// <summary>
        /// Write little-endian encoded 32-bit unsigned integer
        /// </summary>
        void WriteUInt32(uint value);

        /// <summary>
        /// Write little-endian encoded 32-bit integer
        /// </summary>
        void WriteInt32(int value);

        /// <summary>
        /// Write little-endian encoded 64-bit unsigned integer
        /// </summary>
        void WriteUInt64(ulong value);

        /// <summary>
        /// Write little-endian encoded 64-bit integer
        /// </summary>
        void WriteInt64(long value);

        /// <summary>
        /// Write little-endian encoded single precision IEEE 754 float
        /// </summary>
        void WriteFloat(float value);

        /// <summary>
        /// Write little-endian encoded double precision IEEE 754 float
        /// </summary>
        void WriteDouble(double value);

        /// <summary>
        /// Write boolean value
        /// </summary>
        void WriteBoolean(bool value);

        /// <summary>
        /// Write enum valueb as uint32
        /// </summary>
        void WriteEnum(uint value);

        /// <summary>
        /// Write an array of bytes verbatim
        /// </summary>
        /// <param name="data">Array segment specifying bytes to write</param>
        void WriteBytes(ArraySegment<byte> data);

        /// <summary>
        /// Write UTF-8 or UTF-16 encoded string
        /// </summary>
        /// <param name="encoding">String encoding</param>
        /// <param name="value">String value</param>
        /// <param name="size">Size in bytes of encoded string</param>
        void WriteString(Encoding encoding, string value, int size);
    }
}
