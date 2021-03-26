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

namespace AoTBinTool.Options
{
    using CommandLine;

    /// <summary>
    /// Update BIN archive.
    /// </summary>
    [Verb("update", HelpText = "Update BIN archive.")]
    public class UpdateOptions
    {
        /// <summary>
        /// Gets or sets the input bin archive.
        /// </summary>
        [Option("input-bin", Required = true, HelpText = "Input BIN archive.")]
        public string InputBin { get; set; }

        /// <summary>
        /// Gets or sets the input directory.
        /// </summary>
        [Option("input-dir", Required = true, HelpText = "Input directory.")]
        public string InputDir { get; set; }

        /// <summary>
        /// Gets or sets the output BIN file.
        /// </summary>
        [Option("output", Required = true, HelpText = "Output BIN file.")]
        public string Output { get; set; }

        /// <summary>
        /// Gets or sets the file names list.
        /// </summary>
        [Option("file-list", Required = false, HelpText = "File names file.")]
        public string FileList { get; set; }
    }
}
