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
    using System.Linq;
    using AoTBinLib.Converters;
    using AoTBinLib.Enums;
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
            Parser.Default.ParseArguments<ExtractOptions, BuildOptions>(args)
                .WithParsed<ExtractOptions>(Extract)
                .WithParsed<BuildOptions>(Build);
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

            Console.Write("Reading BIN file... ");
            Node binFile = NodeFactory.FromFile(opts.Input);
            binFile.TransformWith<BinReader, IList<string>>(fileList);
            Console.WriteLine("DONE");

            Console.Write("Extracting files... ");
            List<FileInfo> filesInfo = new ();
            foreach (Node node in Navigator.IterateNodes(binFile))
            {
                string outputPath = Path.GetFullPath(string.Concat(opts.Output, node.Path.Substring(binFile.Path.Length)));
                if (node.IsContainer)
                {
                    Directory.CreateDirectory(outputPath);
                }
                else
                {
                    node.Stream.WriteTo(outputPath);

                    var fileInfo = new FileInfo
                    {
                        Name = node.Path.Substring(binFile.Path.Length + 1),
                        Type = node.Tags["Type"],
                        Index = node.Tags["Index"],
                    };
                    filesInfo.Add(fileInfo);
                }
            }

            Console.WriteLine("DONE");

            string info = Path.Combine(opts.Output, "fileInfo.yaml");
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            string yaml = serializer.Serialize(filesInfo);

            File.WriteAllText(info, yaml);
        }

        private static void Build(BuildOptions opts)
        {
            if (!Directory.Exists(opts.Input))
            {
                Console.WriteLine("INPUT DIRECTORY NOT FOUND!!");
                return;
            }

            if (File.Exists(opts.Output))
            {
                Console.WriteLine("OUTPUT FILE ALREADY EXISTS. IT WILL BE DELETED");
                Console.Write("Continue (y/N)?");
                string continueValue = Console.ReadLine();
                if (string.IsNullOrEmpty(continueValue) || !string.Equals(continueValue, "y", StringComparison.InvariantCultureIgnoreCase))
                {
                    Console.WriteLine("Cancelled by user.");
                    return;
                }

                File.Delete(opts.Output);
            }

            List<FileInfo> filesInfo = new ();
            if (!File.Exists(Path.Combine(opts.Input, "fileInfo.yaml")))
            {
                Console.WriteLine("fileInfo.yaml not found. Using default values");
                string[] files = Directory.GetFiles(opts.Input, "*");
                filesInfo.AddRange(files.Select(file => new FileInfo
                {
                    Name = Path.GetRelativePath(opts.Input, file),
                    Type = FileType.Normal,
                }));
            }
            else
            {
                string info = Path.Combine(opts.Input, "fileInfo.yaml");
                string yaml = File.ReadAllText(info);
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();
                filesInfo = deserializer.Deserialize<List<FileInfo>>(yaml);
            }

            Console.Write($"Reading files in {opts.Input}... ");
            var container = NodeFactory.CreateContainer("root");
            for (int i = 0; i < filesInfo.Count; i++)
            {
                var fileInfo = filesInfo[i];
                DataStream s = DataStreamFactory.FromFile(Path.Combine(opts.Input, fileInfo.Name), FileOpenMode.Read);
                Node node = NodeFactory.FromSubstream(i.ToString(), s, 0, s.Length);
                node.Tags["Type"] = fileInfo.Type;
                node.Tags["Index"] = fileInfo.Index;
                container.Add(node);
            }

            container.SortChildren((x, y) => ((int)x.Tags["Index"]).CompareTo((int)y.Tags["Index"]));
            Console.WriteLine("DONE");

            DataStream stream = DataStreamFactory.FromFile(opts.Output, FileOpenMode.Write);
            var parameters = new WriterParameters
            {
                Endianness = opts.BigEndian ? EndiannessMode.BigEndian : EndiannessMode.LittleEndian,
                Stream = stream,
            };

            Console.Write("Building BIN archive... ");
            container.TransformWith<StandardBinWriter, WriterParameters>(parameters);
            stream.Flush();
            stream.Dispose();
            Console.WriteLine("DONE");
        }
    }
}
