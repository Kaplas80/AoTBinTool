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
    using AoTBinTool.Options;
    using CommandLine;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;
    using Yarhl.FileSystem;
    using Yarhl.IO;
    using FileInfo = AoTBinLib.Types.FileInfo;

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
            Parser.Default.ParseArguments<Options.ExtractOptions, Options.BuildOptions>(args)
                .WithParsed<Options.ExtractOptions>(Extract)
                .WithParsed<Options.BuildOptions>(Build);
        }

        private static void Extract(ExtractOptions opts)
        {
            if (!File.Exists(opts.Input))
            {
                Console.WriteLine("INPUT FILE NOT FOUND!!");
                return;
            }

            if (Directory.Exists(opts.Output))
            {
                Console.WriteLine("OUTPUT DIRECTORY ALREADY EXISTS. IT WILL BE DELETED");
                Console.Write("Continue (y/N)?");
                string continueValue = Console.ReadLine();
                if (string.IsNullOrEmpty(continueValue) || !string.Equals(continueValue, "y", StringComparison.InvariantCultureIgnoreCase))
                {
                    Console.WriteLine("Cancelled by user.");
                    return;
                }

                Directory.Delete(opts.Output, true);
            }

            string[] fileList;

            if (!string.IsNullOrEmpty(opts.FileList) && File.Exists(opts.FileList))
            {
                Console.WriteLine($"Using \"{opts.FileList}\" as file list...");
                fileList = File.ReadAllLines(opts.FileList);
            }
            else
            {
                fileList = Array.Empty<string>();
            }

            Console.WriteLine("Reading BIN file...");
            Node binFile = NodeFactory.FromFile(opts.Input);
            binFile.TransformWith<BinReader, IList<string>>(fileList);

            List<FileInfo> filesInfo = new List<FileInfo>();
            foreach (Node node in Navigator.IterateNodes(binFile))
            {
                string outputPath = Path.GetFullPath(string.Concat(opts.Output, node.Path.Substring(binFile.Path.Length)));
                if (node.IsContainer)
                {
                    Directory.CreateDirectory(outputPath);
                }
                else
                {
                    var fileInfo = new FileInfo();
                    fileInfo.Name = node.Path.Substring(binFile.Path.Length);
                    fileInfo.Type = node.Tags["Type"];
                    Console.WriteLine($"Writing: {outputPath}");
                    node.Stream.WriteTo(outputPath);

                    filesInfo.Add(fileInfo);
                }
            }

            var info = NodeFactory.FromFile(Path.Combine(opts.Output, "fileInfo.yaml"));
            DataWriter dw = new DataWriter(info.Stream);

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            string yaml = serializer.Serialize(filesInfo);

            dw.Write(yaml);
        }

        private static void Build(BuildOptions opts)
        {
            throw new NotImplementedException();
        }
    }
}
