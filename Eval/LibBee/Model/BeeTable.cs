using System;
using System.Collections.Generic;
using System.Text;

namespace LibBee.Model
{
    public class BeeTable
    {
        public List<BeeColumn> Columns { get; }
        public List<BeeRow> Rows { get; }

        internal BeeTable(List<BeeColumn> columns, List<BeeRow> rows)
        {
            Columns = columns;
            Rows = rows;
        }

        private static void RepeatAppendChar(StringBuilder sb, char c, int times)
        {
            for (int i = 0; i < times; ++i)
            {
                sb.Append(c);
            }
        }

        public override string ToString()
        {
            var lengths = new List<int>();
            int sumLength = 1;
            for (int i = 0; i < Columns.Count; ++i)
            {
                int length = Math.Max(Columns[i].Name.Length, Columns[i].ColumnType.Name.Length);
                foreach (var row in Rows)
                {
                    int c = row[i].ToString()!.Length;
                    length = length > c ? length : c;
                }
                lengths.Add(length);
                sumLength += length + 1;
            }
            var sb = new StringBuilder();
            RepeatAppendChar(sb, '-', sumLength);
            sb.Append('\n');
            for (int i = 0; i < Columns.Count; ++i)
            {
                sb.Append('|');
                sb.Append(Columns[i].Name);
                RepeatAppendChar(sb, ' ', lengths[i] - Columns[i].Name.Length);
            }
            sb.Append("|\n");
            for (int i = 0; i < Columns.Count; ++i)
            {
                sb.Append('|');
                sb.Append(Columns[i].ColumnType.Name);
                RepeatAppendChar(sb, ' ', lengths[i] - Columns[i].ColumnType.Name.Length);
            }
            sb.Append("|\n");
            RepeatAppendChar(sb, '-', sumLength);
            sb.Append("\n");
            for (int i = 0; i < Rows.Count; ++i)
            {
                for (int j = 0; j < Columns.Count; ++j)
                {
                    sb.Append('|');
                    var str = Rows[i][j].ToString()??string.Empty;
                    sb.Append(str);
                    RepeatAppendChar(sb, ' ', lengths[j] - str.Length);
                }
                sb.Append("|\n");
            }
            RepeatAppendChar(sb, '-', sumLength);

            return sb.ToString();
        }
    }
}
