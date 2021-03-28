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
    using AoTBinLib.Types;
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    /// <summary>
    /// Attack on Titan DLC BIN writer.
    /// </summary>
    public class DlcBinWriter : IConverter<NodeContainerFormat, BinaryFormat>, IInitializer<WriterParameters>
    {
        private WriterParameters _params = new ()
        {
            Endianness = EndiannessMode.LittleEndian,
            BlockSize = 0x00,
            Stream = DataStreamFactory.FromMemory(),
        };

        /// <summary>
        /// Converter initializer.
        /// </summary>
        /// <param name="parameters">Writer configuration.</param>
        public void Initialize(WriterParameters parameters) => _params = parameters;

        /// <summary>
        /// Serializes a DLC BIN archive.
        /// </summary>
        /// <param name="source">Collection of files as NodeContainerFormat.</param>
        /// <returns>The BinaryFormat.</returns>
        public BinaryFormat Convert(NodeContainerFormat source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (source.Root.Children.Count == 0)
            {
                throw new FormatException("No files selected.");
            }

            _params.Stream ??= DataStreamFactory.FromMemory();

            _params.Stream.Seek(0);
            var writer = new DataWriter(_params.Stream)
            {
                Endianness = _params.Endianness,
            };

            BinFileHeader header = new ()
            {
                MagicNumber = 0x00000064,
                FileCount = source.Root.Children.Count,
                BlockSize = 0,
                Padding = 0,
            };

            writer.WriteOfType(header);
            const int headerSize = 0x10 + 0x80 + 0x80;
            writer.Stream.SetLength(headerSize);

            for (int i = 0; i < source.Root.Children.Count; i++)
            {
                Node node = source.Root.Children[i];

                writer.Stream.Seek(0, SeekOrigin.End);
                long offset = writer.Stream.Position;

                writer.Stream.Seek(0x10 + (0x04 * i), SeekOrigin.Begin);
                writer.Write((int)offset);

                writer.Stream.Seek(0x90 + (0x04 * i), SeekOrigin.Begin);
                writer.Write((int)node.Stream.Length); // size

                writer.Stream.Seek(0, SeekOrigin.End);
                node.Stream.WriteTo(writer.Stream);
                writer.WritePadding(0x00, 0x10);
            }

            return new BinaryFormat(_params.Stream);
        }
    }
}
