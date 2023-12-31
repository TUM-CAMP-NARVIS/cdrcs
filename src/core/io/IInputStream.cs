// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cdrcs.IO
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Reads primitive data types as binary values in a specific encoding
    /// </summary>
    public interface IInputStream
    {
        /// <summary>
        /// Gets the length in bytes of the stream
        /// </summary>
        /// <exception cref="NotSupportedException"/>
        long Length { get; }

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
        void ReadEncapsulation();

        /// <summary>
        /// Read 8-bit unsigned integer
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        byte ReadUInt8();

        /// <summary>
        /// Read 8-bit integer
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        sbyte ReadInt8();

        /// <summary>
        /// Read little-endian encoded 16-bit unsigned integer
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        ushort ReadUInt16();

        /// <summary>
        /// Read little-endian 16-bit integer
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        short ReadInt16();

        /// <summary>
        /// Read little-endian encoded 32-bit unsigned integer
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        uint ReadUInt32();

        /// <summary>
        /// Read little-endian encoded 32-bit integer
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        int ReadInt32();

        /// <summary>
        /// Read little-endian encoded 64-bit unsigned integer
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        ulong ReadUInt64();

        /// <summary>
        /// Read little-endian encoded 64-bit integer
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        long ReadInt64();

        /// <summary>
        /// Read little-endian encoded single precision IEEE 754 float
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        float ReadFloat();

        /// <summary>
        /// Read little-endian encoded double precision IEEE 754 float
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        double ReadDouble();

        /// <summary>
        /// Read boolean
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        bool ReadBoolean();

        /// <summary>
        /// Read enum value
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        uint ReadEnum();

        /// <summary>
        /// Read an array of bytes verbatim
        /// </summary>
        /// <param name="count">Number of bytes to read</param>
        /// <exception cref="EndOfStreamException"/>
        ArraySegment<byte> ReadBytes(int count);

        /// <summary>
        /// Skip forward specified number ot bytes
        /// </summary>
        /// <param name="count">Number of bytes to skip</param>
        /// <exception cref="EndOfStreamException"/>
        void SkipBytes(int count);

        /// <summary>
        /// Read UTF-8 or UTF-16 encoded string
        /// </summary>
        /// <param name="encoding">String encoding</param>
        /// <param name="size">Size of payload in bytes</param>
        string ReadString(Encoding encoding, int size);
    }
}
