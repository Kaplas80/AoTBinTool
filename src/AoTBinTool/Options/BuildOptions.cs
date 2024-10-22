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

namespace AoTBinTool.Options
{
    using CommandLine;

    /// <summary>
    /// Build BIN archive.
    /// </summary>
    [Verb("build", HelpText = "Build BIN archive.")]
    public class BuildOptions
    {
        /// <summary>
        /// Gets or sets the input directory.
        /// </summary>
        [Option("input", Required = true, HelpText = "Input directory.")]
        public string Input { get; set; }

        /// <summary>
        /// Gets or sets the output BIN file.
        /// </summary>
        [Option("output", Required = true, HelpText = "Output BIN file.")]
        public string Output { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the output file will use big endian.
        /// </summary>
        [Option("big-endian", Required = false, HelpText = "Create using Big Endian (for PS3).")]
        public bool BigEndian { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the output file will be in DLC format.
        /// </summary>
        [Option("dlc", Required = false, HelpText = "Create as DLC.")]
        public bool Dlc { get; set; }
    }
}
