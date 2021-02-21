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
namespace AoTBinLib.Types
{
    /// <summary>
    /// Default BIN file header.
    /// </summary>
    [Yarhl.IO.Serialization.Attributes.Serializable]
    public class BinFileHeader
    {
        /// <summary>
        /// Gets or sets the file magic number.
        /// </summary>
        public uint MagicNumber { get; set; }

        /// <summary>
        /// Gets or sets the number of files inside the archive.
        /// </summary>
        public int FileCount { get; set; }

        /// <summary>
        /// Gets or sets the block size.
        /// </summary>
        public int BlockSize { get; set; }

        /// <summary>
        /// Gets or sets the padding bytes.
        /// </summary>
        public int Padding { get; set; }
    }
}
