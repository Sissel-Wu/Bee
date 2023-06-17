using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NPOI.SS.UserModel;

namespace SprProse.utils
{
    public static class SprBenchmarks
    {
        private static bool InRange(string address, string range)
        {
            var topLeft = range.Split(':')[0];
            var bottomRight = range.Split(':')[1];

            char c1 = topLeft.ElementAt(0);
            char c2 = bottomRight.ElementAt(0);
            char c = address.ElementAt(0);

            int i1 = int.Parse(topLeft.Substring(1));
            int i2 = int.Parse(bottomRight.Substring(1));
            int i = int.Parse(address.Substring(1));

            return c1 <= c && c <= c2 && i1 <= i && i <= i2;
        }

        private static string ToStr(ICell cell)
        {
            switch (cell.CellType)
            {
                case CellType.Numeric:
                    return cell.NumericCellValue.ToString();
                case CellType.String:
                    return cell.StringCellValue;
                case CellType.Blank:
                    return "NULL";
                default:
                    return "???";
            }
        }

        public static SprSheet Read(string path)
        {
            IWorkbook workbook;
            using (var file = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                workbook = WorkbookFactory.Create(file);
            }

            var sheet = workbook.GetSheetAt(workbook.ActiveSheetIndex);
            List<List<ICell>> rows = new ();
            for (int i = sheet.FirstRowNum; i < sheet.PhysicalNumberOfRows; ++i)
            {
                var row = sheet.GetRow(i);
                if (row != null) // empty row
                {
                    var cells = row.Cells;

                    rows.Add(cells);
                    foreach (var cell in cells)
                    {
                        Console.Out.Write(cell.Address + " : ");
                        Console.Out.Write(ToStr(cell) + "      ");
                    }
                    Console.Out.WriteLine();
                }
                else
                {
                    rows.Add(new List<ICell>());
                    Console.Out.WriteLine("Empty row");
                }
            }

            return new SprSheet(rows);
        }
    }
}
