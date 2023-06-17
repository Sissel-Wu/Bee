using LumenWorks.Framework.IO.Csv;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using LibProse.hades;

namespace FileProse.utils
{
    using ExamplePairs = ISet<Tuple<FilePath, FilePath>>;

    public static class HadesBenchmarks
    {
        private static FilePath PathFromCSVItem(string id, string filepath, string basename, string extension, 
            string group, int size, DateTime modTime, bool executable, bool readable, bool writable, bool deleted)
        {
            if (filepath.EndsWith('/'))
                filepath = filepath[..^1];
            if (filepath == "")
                filepath = "<root>";
            if (filepath.StartsWith('/'))
                filepath = "<root>" + filepath;
            if (filepath.StartsWith('.'))
                filepath = "<root>" + filepath[1..];

            var splits = filepath.Split('/');
            var nodes = splits.Select(dirName => new FileNode(dirName, "", true, "staff", 0, modTime, executable, readable, writable)).ToList();
            nodes.Add(new FileNode(basename, extension, false, group, size, modTime, executable, readable, writable, deleted));
            
            return new FilePath(id, nodes);
        }

        private static FileHDT ReadFromCSV(string csvPath)
        {
            var csvTable = new DataTable();
            using (var csvReader = new CsvReader(new StreamReader(File.OpenRead(csvPath)), false))
            {
                csvTable.Load(csvReader);
            }

            List<FilePath> hdtPaths = new();
            for (var i = 0; i < csvTable.Rows.Count; ++i)
            {
                var isExample = csvTable.Rows[i][10] as string;
                if (isExample.ToLower() != "true") continue;
                var id = csvTable.Rows[i][11] as string;
                var basename = csvTable.Rows[i][0] as string;
                var extension = csvTable.Rows[i][1] as string;
                extension ??= "";
                var path = csvTable.Rows[i][2] as string;
                path ??= "";
                var group = "";
                if (csvTable.Rows[i].ItemArray.Length > 12)
                    @group = csvTable.Rows[i][12] as string;
                @group ??= "";
                var size = int.Parse(csvTable.Rows[i][3] as string);

                var datetimeS = csvTable.Rows[i][4] as string;
                var datetime = DateTime.Parse(datetimeS);

                var executable = (csvTable.Rows[i][6] as string).ToLower() == "true";
                var readable = (csvTable.Rows[i][7] as string).ToLower() == "true";
                var writable = (csvTable.Rows[i][8] as string).ToLower() == "true";
                var deleted = (csvTable.Rows[i][9] as string).ToLower() == "true";

                hdtPaths.Add(PathFromCSVItem(id, path, basename, extension, @group, size, datetime, executable, readable, writable, deleted));
            }

            return new FileHDT(hdtPaths);
        }

        private static IEnumerable<KeyValuePair<string, string>> ReadPathMapping(string filePath)
        {
            var result = new List<KeyValuePair<string, string>>();
            try
            {
                using var fileStream = File.OpenText(filePath);
                var line = fileStream.ReadLine();
                while (line != null)
                {
                    var pIn = line.Split(',')[0];
                    var pOut = line.Split(',')[1];
                    result.Add(new KeyValuePair<string, string>(pIn, pOut));
                
                    line = fileStream.ReadLine();
                }
            }
            catch (FileNotFoundException)
            {
                return result;
            }

            return result;
        }

        private static FilePath GetPath(string desc, FileHDT tree)
        {
            if (desc == "<deleted>")
            {
                return new FilePath().Add(DeletedNode.Get());
            }
            foreach (var path in tree.SplitIntoPaths())
            {
                if (path.Id == desc[(desc.IndexOf('<')+1)..desc.IndexOf('>')])
                {
                    return path;
                }
            }
            throw new KeyNotFoundException("GetPath: no matching key of " + desc);
        }

        private static ExamplePairs FurcateMapping(FileHDT hdtIn, FileHDT hdtOut, 
            IEnumerable<KeyValuePair<string, string>> specifiedMapping)
        {
            var rst = new HashSet<Tuple<FilePath, FilePath>>();
            var keyValuePairs = specifiedMapping.ToList();
            if (keyValuePairs.Any())
            {
                foreach (var kv in keyValuePairs)
                {
                    var pIn = GetPath(kv.Key, hdtIn);
                    var pOut = GetPath(kv.Value, hdtOut);
                    rst.Add(new Tuple<FilePath, FilePath>(pIn, pOut));
                }
            }
            else
            {
                foreach (var pOut in hdtOut.SplitIntoPaths())
                {
                    var pIn = hdtIn.GetPathById(pOut.Id);
                    if (pIn == null)
                        throw new KeyNotFoundException("FurcateMapping: No id in hdtIn that matches " + pOut.Id);
                    rst.Add(new Tuple<FilePath, FilePath>(pIn, pOut));
                }
            }

            return rst;
        }
        
        public static void GetCorrespondence(string benchPath, out IEnumerable<FilePath> inputs, out ExamplePairs correspondence)
        {
            var hdtIn = ReadFromCSV(benchPath + "/input.csv");
            Console.WriteLine(hdtIn);
            var hdtOut = ReadFromCSV(benchPath + "/output.csv");
            Console.WriteLine(hdtOut);
            var specifiedMapping = ReadPathMapping(benchPath + "/path_mapping");
            correspondence = FurcateMapping(hdtIn, hdtOut, specifiedMapping);
            foreach (var (key, value) in correspondence)
            {
                Console.WriteLine(key + " -> " + value);
            }
            Console.WriteLine("=========");
            Console.WriteLine();
            inputs = hdtIn.SplitIntoPaths();
        }

        public static ISet<FilePath> GetInputSet(ExamplePairs examplePairs)
        {
            var rst = new HashSet<FilePath>();
            foreach (var (key, _) in examplePairs)
            {
                rst.Add(key);
            }

            return rst;
        }

        public static void PrepareCustomizedComponents(IEnumerable<string> constStrs, IEnumerable<int> constInts, IEnumerable<DateTime> constDates,
                out IEnumerable<IMapper<FileNode>> mappers,
                out IEnumerable<IFeature<FileNode, FilePath>> features)
        {
            var strList = constStrs.ToList();
            var dateTimes = constDates.ToList();
            var ints = constInts.ToList();

            var tempFs = new List<IFeature<FileNode, FilePath>>();
            var pathUnderTerms = strList.Select(str => new PathUnder(str)).ToList();
            tempFs.AddRange(pathUnderTerms);
            foreach (var cp in Enum.GetValues(typeof(Comparator)).Cast<Comparator>())
            {
                var dtCompares = dateTimes.Select(dt => new ModTimeCompare(cp, dt)).ToList();
                tempFs.AddRange(dtCompares);
                var intCompares = ints.Select(i => new SizeCompare(cp, i)).ToList();
                tempFs.AddRange(intCompares);
            }
            
            var tempM = new List<IMapper<FileNode>>
            {
                new NoChange(),
                new Delete(),
                FieldMapper.Of(x => x.Basename, "basename"),
                FieldMapper.Of(x => x.Extension, "extension"),
                FieldMapper.Of(x => x.Group, "group"),
                FieldMapper.Of(x => x.ModTime.Year.ToString(), "year"),
                FieldMapper.Of(x => x.ModTime.Month.ToString(), "month"),
                FieldMapper.Of(x => x.ModTime.Day.ToString(), "day"),
            };
            tempM.Add(new ChangeExecutable(true));
            tempM.Add(new ChangeExecutable(false));
            tempM.Add(new ChangeReadable(true));
            tempM.Add(new ChangeReadable(false));
            tempM.Add(new ChangeWritable(true));
            tempM.Add(new ChangeWritable(false));
            tempM.AddRange(strList.Select(str => new ChangeExt(str)));
            tempM.AddRange(strList.Select(str => new ChangeGroup(str)));
            tempM.Add(new ChangeExt("unzip"));
            //tempM.Add(FieldMapper.Of(x => x.Key as string, "key"));
            tempM.AddRange(strList.Select(str => new ConstStr(str)));
            tempM.Add(new ConstStr(".tar"));
            tempM.Add(new ConstStr("-"));
            tempM.Add(new ConstStr("."));
            tempFs.AddRange(new []
            {
                FieldFeature<FileNode, FilePath>.Of(x => x.Last().Extension, "extension"),
                //FieldFeature.Of(x => x.Last().Basename, "basename"),
                FieldFeature<FileNode, FilePath>.Of(x => x.Last().Group, "group"),
                //FieldFeature.Of(x => x.Last().ModTime, "modTime"),
                FieldFeature<FileNode, FilePath>.Of(x => x.Last().ModTime.Year, "year"),
                FieldFeature<FileNode, FilePath>.Of(x => x.Last().ModTime.Month, "month"),
                FieldFeature<FileNode, FilePath>.Of(x => x.Last().ModTime.Day, "day"),
            });

            mappers = tempM;
            features = tempFs;
        }
    }
}
