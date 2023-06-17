using System;

namespace SprProse
{
    using Cell = Tuple<Coord, string>;
    
    public interface IRelFunc
    {
        public Cell Evaluate(Cell cell, SprSheet sheet=null);
    }
    
    public class RelCol : IRelFunc
    {
        private readonly int col;

        public RelCol(int col)
        {
            this.col = col;
        }

        public Cell Evaluate(Cell cell, SprSheet sheet)
        {
            var (coord, oldVal) = cell;
            var oldRow = coord.X;
            var value = sheet == null ? oldVal : sheet.GetValue(oldRow, col);
            
            return new Cell(new Coord(oldRow, col), value);
        }

        public override string ToString()
        {
            return $"RelCol{col}";
        }
    }
    
    public class RelRow : IRelFunc
    {
        private readonly int row;

        public RelRow(int row)
        {
            this.row = row;
        }

        public Cell Evaluate(Cell cell, SprSheet sheet)
        {
            var (coord, oldVal) = cell;
            var oldCol = coord.Y;
            var value = sheet == null ? oldVal : sheet.GetValue(row, oldCol);
            
            return new Cell(new Coord(row, oldCol), value);
        }

        public override string ToString()
        {
            return $"RowCol{row}";
        }
    }
}