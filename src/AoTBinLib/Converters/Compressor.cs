// Copyright (c) 2021 Kaplas
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

namespace AoTBinLib.Converters
{
    using System;
    using System.IO;
    using AoTBinLib.Exceptions;
    using Ionic.Zlib;
    using Yarhl.FileFormat;
    using Yarhl.IO;

    /// <summary>
    /// ZLib compressor.
    /// </summary>
    public class Compressor : IConverter<BinaryFormat, BinaryFormat>, IInitializer<EndiannessMode>
    {
        private EndiannessMode _endianness = EndiannessMode.LittleEndian;

        /// <summary>
        /// Converter initializer.
        /// </summary>
        /// <param name="parameters">Endianness.</param>
        public void Initialize(EndiannessMode parameters) => _endianness = parameters;

        /// <summary>
        /// Compress (deflate) a BinaryFormat.
        /// </summary>
        /// <param name="source">Source BinaryFormat.</param>
        /// <returns>Deflated BinaryFormat.</returns>
        public BinaryFormat Convert(BinaryFormat source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            DataStream result = Deflate(source.Stream, _endianness);
            return new BinaryFormat(result);
        }

        private static DataStream Deflate(DataStream source, EndiannessMode endianness)
        {
            DataStream dest = DataStreamFactory.FromMemory();

            var writer = new DataWriter(dest)
            {
                Endianness = endianness,
            };

            source.Seek(0);
            writer.Write((int)source.Length);

            int remaining = (int)source.Length;
            while (remaining > 0)
            {
                int chunkSize = Math.Min(remaining, 0x8000);

                long lengthPos = dest.Position;
                writer.Write(0); // size placeholder

                long startDataPos = dest.Position;
                using var zlibStream = new ZlibStream(dest, CompressionMode.Compress, CompressionLevel.BestCompression, true);
                source.WriteSegmentTo(source.Position, chunkSize, zlibStream);
                zlibStream.Close();
                long compressedChunkSize = dest.Position - startDataPos;
                dest.PushToPosition(lengthPos);
                writer.Write((int)compressedChunkSize);
                dest.PopPosition();

                source.Seek(chunkSize, SeekOrigin.Current);
                remaining -= chunkSize;
            }

            writer.Write(0);

            return dest;
        }
    }
}
