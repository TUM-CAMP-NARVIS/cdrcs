// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cdrcs.Protocols
{
    using System.IO;
    using System.Runtime.CompilerServices;

    internal static class Throw
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void InvalidCdrcsDataType(CdrcsDataType type)
        {
            throw new InvalidDataException(string.Format("Invalid CdrcsDataType {0}", type));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void EndOfStreamException()
        {
            throw new EndOfStreamException("Unexpected end of stream reached.");
        }
    }
}
