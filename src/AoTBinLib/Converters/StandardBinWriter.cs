﻿// Copyright (c) 2021 Kaplas
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
    using AoTBinLib.Enums;
    using AoTBinLib.Types;
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    /// <summary>
    /// Attack on Titan standard BIN writer.
    /// </summary>
    public class StandardBinWriter : IConverter<NodeContainerFormat, BinaryFormat>, IInitializer<WriterParameters>
    {
        private static readonly byte[] _dummy =
        {
            0x83, 0x5F, 0x83, 0x7E, 0x81, 0x5B, 0x82, 0xC5, 0x82, 0xB7, 0x2E, 0x0D, 0x0A, 0x90, 0xB3, 0x8E,
            0xAE, 0x82, 0xC8, 0x83, 0x66, 0x81, 0x5B, 0x83, 0x5E, 0x82, 0xAA, 0x93, 0xFC, 0x82, 0xE9, 0x82,
            0xDC, 0x82, 0xC5, 0x81, 0x41, 0x82, 0xD0, 0x82, 0xC6, 0x82, 0xDC, 0x82, 0xB8, 0x83, 0x8A, 0x83,
            0x93, 0x83, 0x4E, 0x83, 0x66, 0x81, 0x5B, 0x83, 0x5E, 0x82, 0xF0, 0x8D, 0xEC, 0x90, 0xAC, 0x82,
            0xB7, 0x82, 0xE9, 0x82, 0xBD, 0x82, 0xDF, 0x82, 0xCC, 0x83, 0x5F, 0x83, 0x7E, 0x81, 0x5B, 0x83,
            0x74, 0x83, 0x40, 0x83, 0x43, 0x83, 0x8B, 0x82, 0xC6, 0x82, 0xB5, 0x82, 0xC4, 0x8D, 0xEC, 0x90,
            0xAC, 0x82, 0xB3, 0x82, 0xEA, 0x82, 0xC4, 0x82, 0xA2, 0x82, 0xDC, 0x82, 0xB7, 0x2E, 0x0D, 0x0A,
        };

        private WriterParameters _params = new ()
        {
            Endianness = EndiannessMode.LittleEndian,
            BlockSize = 0x800,
            Stream = DataStreamFactory.FromMemory(),
        };

        /// <summary>
        /// Converter initializer.
        /// </summary>
        /// <param name="parameters">Writer configuration.</param>
        public void Initialize(WriterParameters parameters) => _params = parameters;

        /// <summary>
        /// Serializes a BIN archive.
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

            if (_params.BlockSize == 0)
            {
                _params.BlockSize = 0x800;
            }

            BinFileHeader header = new BinFileHeader
            {
                MagicNumber = 0x00077DF9,
                BlockSize = _params.BlockSize,
                FileCount = source.Root.Children.Count,
                Padding = 0,
            };

            writer.WriteOfType(header);
            int headerSize = 0x10 + (header.FileCount * 0x10);
            writer.Stream.SetLength(headerSize);

            for (int i = 0; i < source.Root.Children.Count; i++)
            {
                Node node = source.Root.Children[i];

                writer.Stream.Seek(0, SeekOrigin.End);
                writer.WritePadding(0x00, header.BlockSize);
                long startBlock = writer.Stream.Position / header.BlockSize;

                writer.Stream.Seek(0x10 + (i * 0x10), SeekOrigin.Begin);
                writer.Write(startBlock);

                FileType type = node.Tags["Type"];
                switch (type)
                {
                    case FileType.Empty:
                        writer.Write(0); // size
                        writer.Write(0); // inflated size
                        break;
                    case FileType.Dummy:
                        writer.Write(0x70); // size
                        writer.Write(0x835F837E); // inflated size
                        writer.Stream.Seek(0, SeekOrigin.End);
                        writer.Write(_dummy);
                        break;
                    case FileType.Normal:
                        writer.Write((int)node.Stream.Length); // size
                        writer.Write(0); // inflated size
                        writer.Stream.Seek(0, SeekOrigin.End);
                        node.Stream.WriteTo(writer.Stream);
                        break;
                    case FileType.Compressed:
                        writer.Write((int)node.Stream.Length); // size
                        writer.Write((int)node.Tags["InflatedSize"]); // inflated size
                        writer.Stream.Seek(0, SeekOrigin.End);
                        node.Stream.WriteTo(writer.Stream);
                        break;
                    case FileType.CompressedAlternateEndian:
                        var alternateEndianness = _params.Endianness == EndiannessMode.BigEndian ? EndiannessMode.LittleEndian : EndiannessMode.BigEndian;
                        writer.Write((int)node.Stream.Length); // size
                        writer.Endianness = alternateEndianness;
                        writer.Write((int)node.Tags["InflatedSize"]); // inflated size
                        writer.Endianness = _params.Endianness;
                        writer.Stream.Seek(0, SeekOrigin.End);
                        node.Stream.WriteTo(writer.Stream);
                        break;
                    default:
                        throw new FormatException($"Unsupported file type: {type}");
                }
            }

            return new BinaryFormat(_params.Stream);
        }
    }
}
