using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NPOI.SS.UserModel;

namespace SprProse
{
    using Cell = Tuple<Coord, string>;
    
    public class SprSheet
    {
        public int NumRows { get; }
        public int NumCols { get; }
        private readonly string[,] valuesStrings;
        private readonly Cell[,] relateCells; // should check null when accessing

        public SprSheet(List<List<ICell>> rows)
        {
            // find boundaries
            //char left = 'Z', right = 'A';
            int left = int.MaxValue, right = -1;
            int top = int.MaxValue, bottom = -1;

            foreach (var row in rows)
            {
                foreach (var cell in row)
                {
                    int rowIdx = cell.Address.Row, colIdx = cell.Address.Column;
                    if (colIdx > right)
                        right = colIdx;
                    if (colIdx < left)
                        left = colIdx;
                    if (rowIdx > bottom)
                        bottom = rowIdx;
                    if (rowIdx < top)
                        top = rowIdx;
                }
            }
            
            NumRows = bottom - top + 1;
            NumCols = right - left + 1;
            valuesStrings = new string[NumRows, NumCols];
            relateCells = new Cell[NumRows, NumCols];
            for (int i = 0; i < NumRows; ++i)
            {
                var row = rows[i];
                int deviation = row[0].Address.Column - left;
                for (int j = 0; j < row.Count; ++j)
                {
                    valuesStrings[i, j + deviation] = row[j].ToString();
                    relateCells[i, j + deviation] = new Cell(new Coord(i, j + deviation), row[j].ToString());
                }
                // empty cells initialized as ""
                for (int j = 0; j < deviation; ++j)
                {
                    valuesStrings[i, j] = "";
                    relateCells[i, j] = new Cell(new Coord(i, j), "");
                }
                for (int j = row.Count + deviation; j < NumCols; ++j)
                {
                    valuesStrings[i, j] = "";
                    relateCells[i, j] = new Cell(new Coord(i, j), "");
                }
            }
        }

        public SprSheet(IEnumerable<Cell> cells)
        {
            // find boundaries
            int left = int.MaxValue, right = -1;
            int top = int.MaxValue, bottom = -1;

            var cellList = cells.ToList();
            foreach (var (coord, _) in cellList)
            {
                int rowIdx = coord.X, colIdx = coord.Y;
                if (colIdx > right)
                    right = colIdx;
                if (colIdx < left)
                    left = colIdx;
                if (rowIdx > bottom)
                    bottom = rowIdx;
                if (rowIdx < top)
                    top = rowIdx;
            }
            
            NumRows = bottom + 1;
            NumCols = right + 1;
            valuesStrings = new string[NumRows, NumCols];
            relateCells = new Cell[NumRows, NumCols];
            foreach (var (coord, value) in cellList)
            {
                int rowIdx = coord.X, colIdx = coord.Y;
                valuesStrings[rowIdx, colIdx] = value;
                relateCells[rowIdx, colIdx] = new Cell(coord, value);
            }
            // empty cells initialized as ""
            for (int i = 0; i < NumRows; ++i)
            {
                for (int j = 0; j < NumCols; ++j)
                {
                    if (valuesStrings[i, j] != null) continue;
                    valuesStrings[i, j] = "";
                    relateCells[i, j] = new Cell(new Coord(i, j), "");
                }
            }
        }

        public SprSheet(int numRows, int numCols)
        {
            NumRows = numRows;
            NumCols = numCols;
            valuesStrings = new string[numRows, numCols];
            relateCells = new Cell[numRows, numCols];
            for (int i = 0; i < numRows; ++i)
            {
                for (int j = 0; j < numCols; ++j)
                {
                    valuesStrings[i, j] = "";
                    relateCells[i, j] = new Cell(new Coord(i, j), "");
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(SprSheet))
                return false;

            var oth = (SprSheet)obj;
            if (NumCols != oth.NumCols || NumRows != oth.NumRows)
                return false;

            for (int i = 0; i < NumRows; ++i)
                for (int j = 0; j < NumCols; ++j)
                    if (valuesStrings[i, j] != oth.valuesStrings[i, j])
                        return false;

            return true;
        }

        public List<Cell> GetCells(int i, int j, int k)
        {
            var rst = new List<Cell>();
            for (var row = i; row < NumRows; ++row)
            {
                for (var col = j; col <= k; ++col)
                {
                    rst.Add(relateCells[row, col]);
                }
            }
            return rst;
        }
        
        private List<Cell> MergeCells(List<Cell> newCells)
        {
            var rst = new List<Cell>();
            for (var i = 0; i < NumRows; ++i)
                for (var j = 0; j < NumCols; ++j)
                    rst.Add(relateCells[i,j]);
            rst.AddRange(newCells);
            return rst;
        }

        // return a new sheet added with new cells 
        // if overlap, ignore the new cells
        public SprSheet PlotNewCells(IEnumerable<Cell> newCells)
        {
            // check cells overlap
            var newCellList = newCells.ToList();
            var cellsToAdd = new List<Cell>();
            foreach (var (coord, val) in newCellList)
            {
                if (coord.X < NumRows && coord.Y < NumCols && !string.IsNullOrEmpty(valuesStrings[coord.X, coord.Y]))
                    //throw new Exception("PlotNewCells: cell coord overlaps with existing cells");
                    continue;
                cellsToAdd.Add(new Cell(coord, val));
            }
            
            return new SprSheet(MergeCells(cellsToAdd));
        }

        public string GetValue(int i, int j)
        {
            return valuesStrings[i, j];
        }

        public Cell GetInitCell()
        {
            return relateCells[0, 0];
        }

        public IEnumerable<Cell> EnumerateCells()
        {
            return relateCells.Cast<Cell>();
        }

        public Cell GetNextCell(int row, int col)
        {
            if (row >= NumRows || col >= NumCols || (row == NumRows-1 && col == NumCols-1)) // reach the end
                return null;
            if (col == NumCols-1) // reach the end of the row
                return relateCells[row+1, 0];
            return relateCells[row, col+1];
        }

        public override int GetHashCode()
        {
            int hash = NumRows;
            hash = hash * 233 + NumCols;
            if (NumRows != 0 && NumCols != 0)
            {
                hash = hash * 233 + valuesStrings[0, 0].GetHashCode();
                hash = hash * 233 + valuesStrings[NumRows - 1, NumCols - 1].GetHashCode();
            }

            return hash;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(NumRows).Append(" X ").Append(NumCols).AppendLine();
            for (var i = 0; i < NumRows; ++i)
            {
                for (var j = 0; j < NumCols; ++j)
                {
                    sb.Append(valuesStrings[i, j]);
                    if (j != NumCols - 1)
                        sb.Append(", ");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
