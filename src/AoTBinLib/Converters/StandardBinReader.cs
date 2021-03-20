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
    using AoTBinLib.Enums;
    using AoTBinLib.Exceptions;
    using AoTBinLib.Types;
    using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    /// <summary>
    /// Attack on Titan standard BIN reader.
    /// </summary>
    public class StandardBinReader : IConverter<BinaryFormat, NodeContainerFormat>, IInitializer<ReaderParameters>
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
        /// Deserializes a BIN archive.
        /// </summary>
        /// <param name="source">BIN archive as BinaryFormat.</param>
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

            if (header.MagicNumber != 0x00077DF9)
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

                long startBlock = reader.ReadInt64();
                long offset = startBlock * header.BlockSize;
                int size = reader.ReadInt32();
                uint inflatedSize = reader.ReadUInt32();

                Node node;
                switch (size)
                {
                    case 0:
                        // There can be empty files
                        node = NodeFactory.FromMemory(fileName);
                        node.Tags["Type"] = FileTypes.Empty;
                        break;
                    case 0x70 when inflatedSize == 0x835F837E:
                        // PS3 dummy file
                        node = NodeFactory.FromSubstream(fileName, source.Stream, offset, size);
                        node.Tags["Type"] = FileTypes.Dummy;
                        break;
                    default:
                    {
                        if (inflatedSize == 0)
                        {
                            // File is uncompressed.
                            node = NodeFactory.FromSubstream(fileName, source.Stream, offset, size);
                            node.Tags["Type"] = FileTypes.Normal;
                        }
                        else
                        {
                            DataStream stream = DataStreamFactory.FromStream(source.Stream, offset, size);
                            DataStream inflatedStream;
                            FileTypes type;
                            if (size == 0x00001A48 && inflatedSize == 0x10290000)
                            {
                                // This files appear in PS3. They are stored as LittleEndian.
                                var alternateEndianness = _params.Endianness == EndiannessMode.BigEndian ? EndiannessMode.LittleEndian : EndiannessMode.BigEndian;
                                inflatedStream = Inflate(stream, alternateEndianness);
                                type = FileTypes.CompressedAlternateEndian;
                            }
                            else
                            {
                                inflatedStream = Inflate(stream, _params.Endianness);
                                type = FileTypes.Compressed;
                            }

                            node = NodeFactory.FromSubstream(fileName, inflatedStream, 0, inflatedStream.Length);
                            node.Tags["Type"] = type;
                        }

                        break;
                    }
                }

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

        private static DataStream Inflate(Stream source, EndiannessMode endianness)
        {
            DataStream result = DataStreamFactory.FromMemory();

            source.Seek(0, SeekOrigin.Begin);
            var reader = new DataReader(source)
            {
                Endianness = endianness,
            };

            int size = reader.ReadInt32();
            int chunkSize = reader.ReadInt32();

            while (chunkSize != 0)
            {
                byte[] compressedData = reader.ReadBytes(chunkSize);
                byte[] outputData = new byte[32768];

                DataStream compressedChunk = DataStreamFactory.FromArray(compressedData, 0, chunkSize);
                using var inflaterStream = new InflaterInputStream(compressedChunk);
                int read = inflaterStream.Read(outputData, 0, 32768);
                result.Write(outputData, 0, read);

                chunkSize = reader.ReadInt32();
            }

            if (result.Length != size)
            {
                throw new ExtractionException("Result size doesn't match with expected size.");
            }

            return result;
        }
    }
}
