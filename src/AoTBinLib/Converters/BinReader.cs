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
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    /// <summary>
    /// Attack on Titan BIN reader.
    /// </summary>
    public class BinReader : IConverter<BinaryFormat, NodeContainerFormat>, IInitializer<IList<string>>
    {
        private IList<string> _params = Array.Empty<string>();

        /// <summary>
        /// Converter initializer.
        /// </summary>
        /// <param name="parameters">Reader configuration.</param>
        public void Initialize(IList<string> parameters) => _params = parameters;

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

            source.Stream.Seek(0);
            var reader = new DataReader(source.Stream)
            {
                Endianness = EndiannessMode.BigEndian,
            };

            uint magic = reader.ReadUInt32();

            return magic switch
            {
                0x00077DF9 => (NodeContainerFormat)ConvertFormat.With<StandardBinReader, ReaderParameters>(
                    new ReaderParameters
                    {
                        Endianness = EndiannessMode.BigEndian,
                        FileNames = _params,
                    },
                    source),
                0xF97D0700 => (NodeContainerFormat)ConvertFormat.With<StandardBinReader, ReaderParameters>(
                    new ReaderParameters
                    {
                        Endianness = EndiannessMode.LittleEndian,
                        FileNames = _params,
                    },
                    source),
                _ => throw new FormatException($"Unrecognized file magic number: {magic:X8}"),
            };
        }
    }
}
