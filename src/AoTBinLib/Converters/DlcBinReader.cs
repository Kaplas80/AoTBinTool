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
    using AoTBinLib.Enums;
    using AoTBinLib.Types;
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    /// <summary>
    /// Attack on Titan DLC BIN reader.
    /// </summary>
    public class DlcBinReader : IConverter<BinaryFormat, NodeContainerFormat>, IInitializer<ReaderParameters>
    {
        private ReaderParameters _params = new ()
        {
            Endianness = EndiannessMode.LittleEndian,
            FileNames = Array.Empty<string>(),
        };

        /// <summary>
        /// Converter initializer.
        /// </summary>
        /// <param name="parameters">Reader configuration.</param>
        public void Initialize(ReaderParameters parameters) => _params = parameters;

        /// <summary>
        /// Deserializes a DLC BIN archive.
        /// </summary>
        /// <param name="source">DLC BIN archive as BinaryFormat.</param>
        /// <returns>The NodeContainerFormat.</returns>
        public NodeContainerFormat Convert(BinaryFormat source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (source.Stream.Length < 0x10)
            {
                throw new FormatException("Not a valid BIN file.");
            }

            source.Stream.Seek(0);
            var reader = new DataReader(source.Stream)
            {
                Endianness = _params.Endianness,
            };

            BinFileHeader header = reader.Read<BinFileHeader>();

            if (header.MagicNumber != 0x00000064)
            {
                throw new FormatException($"Unrecognized file magic number: {header.MagicNumber:X8}");
            }

            NodeContainerFormat result = new ();
            for (int i = 0; i < header.FileCount; i++)
            {
                string fileSubPath = string.Empty;
                string fileName = $"{i + 1:0000}";
                if (_params.FileNames.Count > 0 && i < _params.FileNames.Count)
                {
                    string[] path = _params.FileNames[i].Split('/');
                    fileSubPath = string.Join('/', path[0..^1]);
                    fileName = path[^1];
                }

                source.Stream.Seek(0x10 + (0x04 * i));
                int offset = reader.ReadInt32();
                source.Stream.Seek(0x90 + (0x04 * i));
                int size = reader.ReadInt32();

                Node node = NodeFactory.FromSubstream(fileName, source.Stream, offset, size);
                node.Tags["Type"] = FileType.Normal;
                node.Tags["Index"] = i + 1;

                if (!string.IsNullOrEmpty(fileSubPath))
                {
                    NodeFactory.CreateContainersForChild(result.Root, fileSubPath, node);
                }
                else
                {
                    result.Root.Add(node);
                }
            }

            return result;
        }
    }
}
