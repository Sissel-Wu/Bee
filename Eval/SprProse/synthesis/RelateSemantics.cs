using System;
using System.Collections.Generic;

namespace SprProse.synthesis
{
    using Cell = Tuple<Coord, string>;
    
    public static class RelateSemantics
    {
        public static SprSheet Cons(IEnumerable<Tuple<Cell, Cell>> ioCellPairs, SprSheet table)
        {
            var toBePlotted = new List<Cell>();
            foreach (var (from, to) in ioCellPairs)
            {
                toBePlotted.Add(new Cell(to.Item1, from.Item2));
            }
            
            return table.PlotNewCells(toBePlotted);
        }

        public static IEnumerable<Tuple<Cell, Cell>> FilterWith(SprSheet inputSpr, IPred mapPred, SequenceMaker seq)
        {
            var iterState = new IterState(inputSpr, seq);
            while (true)
            {
                if (mapPred.Evaluate(iterState))
                {
                    iterState.SetSelectCurrent();
                    yield return new Tuple<Cell, Cell>(iterState.CurrentIn, iterState.CurrentOut);
                    iterState.UpdateCurrentOut();
                }
                if (!iterState.HasNext()) break;
                iterState.UpdateCurrentIn();
            }
        }

        public static IEnumerable<Tuple<Cell, Cell>> Associate(IEnumerable<Tuple<Cell, Cell>> cellsMapping, 
            IRelFunc relFuncIn, IRelFunc relFuncOut, SprSheet sheet)
        {
            foreach (var (inCell, outCell) in cellsMapping)
            {
                var newInCell = relFuncIn.Evaluate(inCell, sheet);
                var newOutCell = relFuncOut.Evaluate(outCell);
                yield return new Tuple<Cell, Cell>(newInCell, newOutCell);
            }
        }

        public static SprSheet EmptyProgram()
        {
            return new SprSheet(0, 0);
        }

        public static IEnumerable<Tuple<Cell, Cell>> Associate(IEnumerable<Tuple<Cell, Cell>> cellsMapping,
            IRelFunc relFuncIn, IRelFunc relFuncOut, ICellPred pred, SprSheet sheet)
        {
            foreach (var (inCell, outCell) in cellsMapping)
            {
                if (pred != null && !pred.Evaluate(inCell)) continue;
                var newInCell = relFuncIn.Evaluate(inCell, sheet);
                var newOutCell = relFuncOut.Evaluate(outCell);
                yield return new Tuple<Cell, Cell>(newInCell, newOutCell);
            }
        }
    }
}