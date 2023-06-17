using System;

namespace SprProse
{
    using Cell = Tuple<Coord, string>;
    
    public class SequenceMaker
    {
        public int I { get; } // inclusive
        public int J { get; } // inclusive
        public int K { get; } // inclusive
        
        public SequenceMaker(int i, int j, int k)
        {
            I = i;
            J = j;
            K = k;
        }

        public Cell GetInitCell()
        {
            return new Cell(new Coord(I, J), "");
        }

        public Cell GetNextCell(int row, int col)
        {
            if (row < I)
                return new Cell(new Coord(I, J), "");
            else if (col < K)
                return new Cell(new Coord(row, col + 1), "");
            else
                return new Cell(new Coord(row + 1, J), "");
        }

        public override string ToString()
        {
            return $"Seq({I},{J},{K})";
        }
    }
}