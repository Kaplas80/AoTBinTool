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

namespace Tests
{
    using System;
    using AoTBinLib.Enums;
    using NUnit.Framework;
    using Yarhl.IO;

    public class BinReaderTests
    {
        [Test]
        public void NullSourceThrowsException()
        {
            var reader = new AoTBinLib.Converters.BinReader();
            Assert.Throws<ArgumentNullException>(() => reader.Convert(null));
        }

        [Test]
        public void BadMagicThrowsException()
        {
            var reader = new AoTBinLib.Converters.BinReader();
            byte[] data = { 0x00, 0x00, 0x00, 0x00 };
            using DataStream stream = DataStreamFactory.FromArray(data, 0, data.Length);
            var format = new BinaryFormat(stream);
            Assert.Throws<FormatException>(() => reader.Convert(format));
        }

        [Test]
        public void CanDetectEndianness()
        {
            var reader = new AoTBinLib.Converters.BinReader();
            byte[] littleEndianData =
            {
                0xF9, 0x7D, 0x07, 0x00, 0x01, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0xBA, 0xAD, 0xF0, 0x0D, 0xBA, 0xAD, 0xF0, 0x0D, 0xBA, 0xAD, 0xF0, 0x0D, 0xBA, 0xAD, 0xF0, 0x0D,
            };
            using DataStream littleEndianStream = DataStreamFactory.FromArray(littleEndianData, 0, littleEndianData.Length);
            var littleEndianFormat = new BinaryFormat(littleEndianStream);

            var result = reader.Convert(littleEndianFormat);
            Assert.AreEqual(1, result.Root.Children.Count);
            Assert.AreEqual(FileTypes.Normal, result.Root.Children[0].Tags["Type"]);

            byte[] bigEndianData =
            {
                0x00, 0x07, 0x7D, 0xF9, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00,
                0xBA, 0xAD, 0xF0, 0x0D, 0xBA, 0xAD, 0xF0, 0x0D, 0xBA, 0xAD, 0xF0, 0x0D, 0xBA, 0xAD, 0xF0, 0x0D,
            };
            using DataStream bigEndianStream = DataStreamFactory.FromArray(bigEndianData, 0, bigEndianData.Length);
            var bigEndianFormat = new BinaryFormat(bigEndianStream);

            result = reader.Convert(bigEndianFormat);
            Assert.AreEqual(1, result.Root.Children.Count);
            Assert.AreEqual(FileTypes.Normal, result.Root.Children[0].Tags["Type"]);
        }

        [Test]
        public void CanSetFileNames()
        {
            var reader = new AoTBinLib.Converters.BinReader();
            string[] parameters = { "test/file1" };

            reader.Initialize(parameters);

            byte[] data =
            {
                0xF9, 0x7D, 0x07, 0x00, 0x01, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0xBA, 0xAD, 0xF0, 0x0D, 0xBA, 0xAD, 0xF0, 0x0D, 0xBA, 0xAD, 0xF0, 0x0D, 0xBA, 0xAD, 0xF0, 0x0D,
            };
            using DataStream stream = DataStreamFactory.FromArray(data, 0, data.Length);
            var format = new BinaryFormat(stream);

            var result = reader.Convert(format);
            Assert.AreEqual(1, result.Root.Children.Count);
            Assert.AreEqual("test", result.Root.Children[0].Name);
            Assert.AreEqual(1, result.Root.Children[0].Children.Count);
            Assert.AreEqual(FileTypes.Normal, result.Root.Children[0].Children[0].Tags["Type"]);
            Assert.AreEqual("file1", result.Root.Children[0].Children[0].Name);
        }
    }
}
