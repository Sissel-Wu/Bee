using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using LibBee.Annotations;
using LibBee.Model;
using LumenWorks.Framework.IO.Csv;

namespace SprBee.utils
{
    public static class Benchmarks
    {
        public static Tuple<BeeTable, BeeTable> ReadData<TR, TC, TM>(string csvPath)
        {
            var cells = new List<CellEntity<TR, TC, TM>>();
            var csvTable = new DataTable();
            using (var csvReader = new CsvReader(new StreamReader(File.OpenRead(csvPath)), false))
            {
                csvTable.Load(csvReader);
            }
            for (var i = 0; i < csvTable.Rows.Count; ++i)
            {
                var isExample = (string) csvTable.Rows[i][9];
                if (!isExample.ToLower().Contains("true")) continue;
                var row = int.Parse((string)csvTable.Rows[i][1]);
                var col = int.Parse((string)csvTable.Rows[i][2]);
                var rowHeadStr = csvTable.Rows[i][3] as string; rowHeadStr ??= "";
                var colHeadStr = csvTable.Rows[i][4] as string; colHeadStr ??= "";
                var cellContentStr = csvTable.Rows[i][7] as string; cellContentStr ??= "";

                var rowHead = typeof(TR) == typeof(int) ? (TR) (object) int.Parse(rowHeadStr) : (TR) (object) rowHeadStr;
                var colHead = typeof(TC) == typeof(int) ? (TC) (object) int.Parse(colHeadStr) : (TC) (object) colHeadStr;
                var cellContent = typeof(TM) == typeof(int) ? (TM) (object) int.Parse(cellContentStr) 
                    : typeof(TM) == typeof(double) ? (TM) (object) double.Parse(cellContentStr) 
                    : (TM) (object) cellContentStr;
                                
                cells.Add(new CellEntity<TR, TC, TM>(row, col, rowHead, colHead, cellContent));
            }

            // row-major ordering
            var sorted = cells.OrderBy(x => x.Row()).ThenBy(x => x.Col()).ToList();
            for (var i = 0; i < sorted.Count; ++i)
            {
                sorted[i].SetReadOrder(i);
            }
            var cellsTable = new DataParser<CellEntity<TR, TC, TM>>().ParseAsTable(cells);

            var rows = new List<RowEntity<TM>>();
            var acc = 0;
            for (var row = 0; ; ++row)
            {
                var values = sorted.Where(x => x.Row() == row).Select(x => x.Content()).ToArray();
                if (values.Length != 0) 
                    rows.Add(new RowEntity<TM>(row, values));
                acc += values.Length;
                if (acc == sorted.Count)
                    break;
            }
            var rowsTable = new DataParser<RowEntity<TM>>().ParseAsTable(rows);

            return new Tuple<BeeTable, BeeTable>(cellsTable, rowsTable);
        }
        
        public static List<string[]> ReadOperations(string path)
        {
            var lines = File.ReadAllLines(path);
            var rst = new List<string[]>();
            foreach (var line in lines)
            {
                if (line.Trim() == "") continue;
                // split with empty symbol
                rst.Add(line.Split(new[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries));
            }

            return rst;
        }
    }
}
