using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using LumenWorks.Framework.IO.Csv;

namespace FileBee.utils
{
    public static class Benchmarks
    {
        public static Dictionary<string, MyFile> ReadFiles(string csvPath)
        {
            var result = new Dictionary<string, MyFile>();

            var csvTable = new DataTable();
            using (var csvReader = new CsvReader(new StreamReader(File.OpenRead(csvPath)), false))
            {
                csvTable.Load(csvReader);
            }
            for (var i = 0; i < csvTable.Rows.Count; ++i)
            {
                var isExample = csvTable.Rows[i][10] as string;
                if (isExample!.ToLower() == "true")
                {
                    var id = (string) csvTable.Rows[i][11];
                    var basename = csvTable.Rows[i][0] as string;
                    var extension = csvTable.Rows[i][1] as string;
                    extension ??= "";
                    var path = csvTable.Rows[i][2] as string;
                    path ??= "";
                    var group = "";
                    if (csvTable.Rows[i].ItemArray.Length > 12)
                        group = csvTable.Rows[i][12] as string;
                    group ??= "";
                    var size = int.Parse((string)csvTable.Rows[i][3]);

                    var datetime = DateTime.Parse((string)csvTable.Rows[i][4]);

                    bool executable = ((string) csvTable.Rows[i][6]).ToLower() == "true";
                    bool readable = ((string) csvTable.Rows[i][7]).ToLower() == "true";
                    bool writable = ((string) csvTable.Rows[i][8]).ToLower() == "true";

                    result[id] = new MyFile(id, basename, extension, path, size, datetime, group, readable, writable, executable);
                }
            }

            return result;
        }

        public static List<string[]> ReadOperations(string path)
        {
            var lines = File.ReadAllLines(path);
            var rst = new List<string[]>();
            foreach (var line in lines)
            {
                rst.Add(line.Split(' '));
            }

            return rst;
        }
    }
}
