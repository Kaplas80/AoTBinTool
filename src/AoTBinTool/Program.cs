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
namespace AoTBinTool
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using AoTBinLib.Converters;
    using Yarhl.FileSystem;

    /// <summary>
    /// Main program class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main entry-point.
        /// </summary>
        /// <param name="args">Application arguments.</param>
        public static void Main(string[] args)
        {
            string input = @"G:\Games\Attack on Titan Wings of Freedom\LINKDATA\LINKDATA_EU_A.BIN";
            string filelist = @"G:\Games\Attack on Titan Wings of Freedom\LINKDATA\FILELIST_A.txt";
            string output = @"G:\Games\Attack on Titan Wings of Freedom\LINKDATA\test";

            string[] p = File.ReadAllLines(filelist);

            Node n = NodeFactory.FromFile(input);
            n.TransformWith<BinReader, IList<string>>(p);
            foreach (Node n1 in Navigator.IterateNodes(n))
            {
                string outputPath = Path.GetFullPath(string.Concat(output, n1.Path.Substring(n.Path.Length)));
                if (n1.IsContainer)
                {
                    Directory.CreateDirectory(outputPath);
                }
                else
                {
                    n1.Stream.WriteTo(outputPath);
                }
            }
        }
    }
}
