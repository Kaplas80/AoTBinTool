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
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    /// <summary>
    /// Decompresses all compressed nodes.
    /// </summary>
    public class StandardBinDecompressor : IConverter<NodeContainerFormat, NodeContainerFormat>, IInitializer<EndiannessMode>
    {
        private EndiannessMode _endianness = EndiannessMode.LittleEndian;

        /// <summary>
        /// Converter initializer.
        /// </summary>
        /// <param name="parameters">Endianness.</param>
        public void Initialize(EndiannessMode parameters) => _endianness = parameters;

        /// <summary>
        /// Decompresses all compressed nodes.
        /// </summary>
        /// <param name="source">Original node container.</param>
        /// <returns>Decompressed node container.</returns>
        public NodeContainerFormat Convert(NodeContainerFormat source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            foreach (Node node in Navigator.IterateNodes(source.Root))
            {
                if (node.Format is not BinaryFormat)
                {
                    continue;
                }

                if (node.Tags["Type"] == FileType.Compressed)
                {
                    node.TransformWith<Decompressor, EndiannessMode>(_endianness);
                }
                else if (node.Tags["Type"] == FileType.CompressedAlternateEndian)
                {
                    var alternateEndianness = _endianness == EndiannessMode.BigEndian ? EndiannessMode.LittleEndian : EndiannessMode.BigEndian;
                    node.TransformWith<Decompressor, EndiannessMode>(alternateEndianness);
                }
            }

            return source;
        }
    }
}
