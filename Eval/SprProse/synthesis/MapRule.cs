using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SprProse.synthesis
{
    using Cell = Tuple<Coord, string>;
    using Mapping = IEnumerable<Tuple<Tuple<Coord, string>, Tuple<Coord, string>>>;
    
    public class MapRule
    {
        private readonly SprSheet inputSheet;
        private readonly SprSheet outputSheet;

        public MapRule(SprSheet inputSheet, SprSheet outputSheet)
        {
            this.inputSheet = inputSheet;
            this.outputSheet = outputSheet;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool HasEnoughRemainingCells(List<Cell> candidateBlock, int outPos, Coord inPos, int inRows, int inCols)
        {
            return inRows * inCols - inPos.X * inCols - inPos.Y >= candidateBlock.Count - outPos;
        }

        private IEnumerable<List<Tuple<Cell, Cell>>> MaintainsOrder(List<Cell> candidateBlock, int outPos, Cell inCell)
        {
            var rst = new List<List<Tuple<Cell, Cell>>>();
            var inRows = inputSheet.NumRows;
            var inCols = inputSheet.NumCols;
            if (outPos == candidateBlock.Count) // base case
            {
                rst.Add(new List<Tuple<Cell, Cell>>());
                return rst;
                //yield break;
            }

            // find the next cell in the input sheet whose data matches candidateBlock[outPos]
            var outVal = candidateBlock[outPos].Item2;
            while (true)
            {
                if (inCell == null || !HasEnoughRemainingCells(candidateBlock, outPos, inCell.Item1, inRows, inCols))
                {
                    return rst;
                    //yield break;
                }
                if (inCell.Item2 == outVal) break;
                inCell = inputSheet.GetNextCell(inCell.Item1.X, inCell.Item1.Y);
            }

            // now inCell matches candidateBlock[outPos]
            // recursively call: (1) include inCell (2) not include inCell
            var nextStart = inputSheet.GetNextCell(inCell.Item1.X, inCell.Item1.Y);
            foreach (var subResult in MaintainsOrder(candidateBlock, outPos + 1, nextStart)) // case (1)
            {
                subResult.Insert(0,new Tuple<Cell, Cell>(inCell, candidateBlock[outPos]));
                rst.Add(subResult);
                //yield return result;
            }

            foreach (var subResult in MaintainsOrder(candidateBlock, outPos, nextStart)) // case (2)
            {
                rst.Add(subResult);
            }

            return rst;
        }

        public IEnumerable<Tuple<Mapping, SequenceMaker>> EnumerateCandidateMap()
        {
            var outRows = outputSheet.NumRows;
            var outCols = outputSheet.NumCols;
            for (var i = 0; i < outRows; i++)
            {
                for (var j = 0; j < outCols; ++j)
                {
                    for (var k = outCols-1; k >= j; --k)
                    {
                        var outCells = outputSheet.GetCells(i, j, k);
                        foreach (var map in MaintainsOrder(outCells, 0, inputSheet.GetInitCell()))
                        {
                            yield return new Tuple<Mapping, SequenceMaker>(map, new SequenceMaker(i, j, k));
                        }
                    }
                }
            }
        }
    }
}