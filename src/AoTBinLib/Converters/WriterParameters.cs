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
    using Yarhl.IO;

    /// <summary>
    /// Parameters needed for BIN writing.
    /// </summary>
    public class WriterParameters
    {
        /// <summary>
        /// Gets or sets the endianness mode of the file.
        /// </summary>
        public EndiannessMode Endianness { get; set; }

        /// <summary>
        /// Gets or sets the BIN block size. Default is 0x800.
        /// </summary>
        public int BlockSize { get; set; }

        /// <summary>
        /// Gets or sets the stream to write to.
        /// </summary>
        public DataStream Stream { get; set; }
    }
}
