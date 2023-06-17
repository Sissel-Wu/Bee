using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibProse.hades;

namespace XMLProse.utils
{
    using ExamplePairs = ISet<Tuple<XmlPath, XmlPath>>;
    
    public static class HadesBenchmarks
    {
        public static XmlHDT ReadFromXML(string path)
        {
            var xmlTree = XmlParser.Read(path);
            return new XmlHDT(xmlTree);
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

        private static XmlPath GetPath(string desc, XmlHDT tree)
        {
            if (desc == "<deleted>")
            {
                return new XmlPath().Add(DeletedNode.Get());
            }
            foreach (var path in tree.SplitIntoPaths())
            {
                if (desc == path.ToString())
                    return path;
            }
            throw new KeyNotFoundException("GetPath: no matching key of " + desc);
        }
        
        private static ExamplePairs FurcateMapping(XmlHDT hdtIn, XmlHDT hdtOut, 
            IEnumerable<KeyValuePair<string, string>> specifiedMapping)
        {
            var rst = new HashSet<Tuple<XmlPath, XmlPath>>();
            var keyValuePairs = specifiedMapping.ToList();
            if (keyValuePairs.Any())
            {
                foreach (var (key, value) in keyValuePairs)
                {
                    var pIn = GetPath(key, hdtIn);
                    var pOut = GetPath(value, hdtOut);
                    rst.Add(new Tuple<XmlPath, XmlPath>(pIn, pOut));
                }
            }
            // else
            // {
            //     foreach (var pOut in hdtOut.SplitIntoPaths())
            //     {
            //         var pIn = hdtIn.GetPathById(pOut.Id);
            //         if (pIn == null)
            //             throw new KeyNotFoundException("FurcateMapping: No id in hdtIn that matches " + pOut.Id);
            //         rst.Add(new Tuple<FilePath, FilePath>(pIn, pOut));
            //     }
            // }
            return rst;
        }

        public static void GetCorrespondence(string benchPath, out IEnumerable<XmlPath> inputs, out ExamplePairs correspondence)
        {
            var hdtIn = ReadFromXML(benchPath + "/input.xml");
            Console.WriteLine(hdtIn);
            var hdtOut = ReadFromXML(benchPath + "/output.xml");
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

        public static void PrepareCustomizedComponents(IEnumerable<string> constStrs, IEnumerable<int> constInts, IEnumerable<DateTime> constDates,
                out IEnumerable<IMapper<XmlNode>> mappers,
                out IEnumerable<IFeature<XmlNode, XmlPath>> features)
        {
            var strList = constStrs.ToList();
            var dateTimes = constDates.ToList();
            var ints = constInts.ToList();

            var tempFs = new List<IFeature<XmlNode, XmlPath>>();
            var tempM = new List<IMapper<XmlNode>>
            {
                new NoChange(),
                new Delete(),
                FieldMapper.Of(x => x.Tag, "tag"),
                FieldMapper.Of(x => x.Tag, "tag", true),
                //FieldMapper.Of(x => x.Key as string, "key"),
            };
            tempM.AddRange(strList.Select(str => new TextNode(str)));
            var tps = from p1 in strList
                from p2 in strList
                from p3 in strList
                select new ReplaceAttrStr(p1, p2, p3);
            tempM.AddRange(tps);
            var dps = (from p1 in strList from p2 in strList select new {p1, p2}).ToList();
            tempM.AddRange(dps.Select(p => new AppendAttrStr(p.p1, p.p2)));
            tempM.AddRange(strList.Select(str => FieldMapper.Of(x => x.GetAttr(str), "attr:" + str)));
            tempM.AddRange(strList.Select(str => FieldMapper.Of(x => x.GetAttr(str), "attr:" + str, true)));
            tempM.AddRange(strList.Select(str => FieldMapper.Of(x => x.GetAttr(str)?.Substring(0, 1), "FirstChar", true)));
            tempM.AddRange(strList.Select(str => new ConstMapper<XmlNode>(str)));
            tempM.AddRange(dps.Select(p => new TagShift(p.p1, p.p2, -1)));
            tempM.AddRange(dps.Select(p => new TagShift(p.p1, p.p2, -2)));
            tempM.AddRange(dps.Select(p => new TagShift(p.p1, p.p2, 1)));
            tempM.AddRange(dps.Select(p => new TagShift(p.p1, p.p2, 2)));
            tempFs.AddRange(new []
            {
                FieldFeature<XmlNode, XmlPath>.Of(x => x.Last().Tag, "tag"),
                //FieldFeature.Of(x => x.Last().Basename, "basename"),
                FieldFeature<XmlNode, XmlPath>.Of(x => x.Last().Type, "type"),
                //FieldFeature.Of(x => x.Last().ModTime, "modTime"),
            });
            tempFs.AddRange(strList.Select(str => FieldFeature<XmlNode, XmlPath>.Of(x => x.ToString().Contains(str), "path_contains:" + str)));
            tempFs.AddRange(strList.Select(str => FieldFeature<XmlNode, XmlPath>.Of(x => x.Last().HasAttr(str), "has_attr:" + str)));
            //tempFs.AddRange(strList.Select(str => FieldFeature<XmlNode, XmlPath>.Of(x => x.Last().GetAttr(str), "attr:" + str)));
            tempFs.AddRange(dps.Select(p => FieldFeature<XmlNode, XmlPath>.Of(x => x.Last().GetAttr(p.p1)?.Contains(p.p2), "attr_contains:" + p.p1 + ":" + p.p2)));

            mappers = tempM;
            features = tempFs;
        }
    }
}