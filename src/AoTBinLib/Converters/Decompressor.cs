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
    /// ZLib decompressor.
    /// </summary>
    public class Decompressor : IConverter<BinaryFormat, BinaryFormat>, IInitializer<EndiannessMode>
    {
        private EndiannessMode _endianness = EndiannessMode.LittleEndian;

        /// <summary>
        /// Converter initializer.
        /// </summary>
        /// <param name="parameters">Endianness.</param>
        public void Initialize(EndiannessMode parameters) => _endianness = parameters;

        /// <summary>
        /// Decompress (inflate) a BinaryFormat.
        /// </summary>
        /// <param name="source">Source BinaryFormat.</param>
        /// <returns>Inflated BinaryFormat.</returns>
        public BinaryFormat Convert(BinaryFormat source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            DataStream result = Inflate(source.Stream, _endianness);
            return new BinaryFormat(result);
        }

        private static DataStream Inflate(DataStream source, EndiannessMode endianness)
        {
            DataStream dest = DataStreamFactory.FromMemory();

            source.Seek(0);
            var reader = new DataReader(source)
            {
                Endianness = endianness,
            };

            int size = reader.ReadInt32();
            int chunkSize = reader.ReadInt32();

            while (chunkSize != 0)
            {
                using var zlibStream = new ZlibStream(dest, CompressionMode.Decompress, true);
                source.WriteSegmentTo(source.Position, chunkSize, zlibStream);
                zlibStream.Close();

                source.Seek(chunkSize, SeekOrigin.Current);
                chunkSize = reader.ReadInt32();
            }

            if (dest.Length != size)
            {
                throw new ExtractionException("Result size doesn't match with expected size.");
            }

            return dest;
        }
    }
}
