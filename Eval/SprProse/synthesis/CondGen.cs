using System;
using System.Collections.Generic;
using System.Linq;

namespace SprProse.synthesis
{
    using Cell = Tuple<Coord, string>;
    
    public class CondGen
    {
        private readonly List<IPred> atomicPredSymbols;
        private readonly SprSheet inputSpr;
        private readonly SequenceMaker seq;

        private void InitPredSymbols()
        {
            for (var row = 0; row < inputSpr.NumRows; ++row)
            {
                atomicPredSymbols.Add(new RowEq(row));
                atomicPredSymbols.Add(new NotPred(new RowEq(row)));
            }
            for (var col = 0; col < inputSpr.NumCols; ++col)
            {
                atomicPredSymbols.Add(new ColEq(col));
                atomicPredSymbols.Add(new NotPred(new ColEq(col)));
            }
            var stringSet = new HashSet<string>();
            foreach (var (_, val) in inputSpr.EnumerateCells())
            {
                if (stringSet.Add(val))
                {
                    atomicPredSymbols.Add(new DataEq(val));
                    atomicPredSymbols.Add(new NotPred(new DataEq(val)));
                }
            }
            
            atomicPredSymbols.Add(new FirstReadValue());
            atomicPredSymbols.Add(new NotPred(new FirstReadValue()));
            atomicPredSymbols.Add(new ValueBeenSelected());
            atomicPredSymbols.Add(new NotPred(new ValueBeenSelected()));
        }

        public CondGen(SprSheet inputSpr, SequenceMaker seq)
        {
            this.inputSpr = inputSpr;
            this.seq = seq;
            atomicPredSymbols = new List<IPred>();
            InitPredSymbols();
        }

        private static int SearchState(IterState state, List<Cell> cellList, int begin = 0)
        {
            for (var i = begin; i < cellList.Count; ++i)
            {
                if (state.CurrentIn.Item1.Equals(cellList[i].Item1))
                {
                    return i;
                }
            }
            return -1;
        }

        private bool ValidCond(IPred pred, List<Cell> cellList)
        {
            var filterRst = RelateSemantics.FilterWith(inputSpr, pred, seq).ToList();
            if (filterRst.Count != cellList.Count) return false;
            for (var i = 0; i < cellList.Count; ++i)
            {
                if (!filterRst[i].Item1.Item1.Equals(cellList[i].Item1))
                {
                    return false;
                }
            }
            return true;
        }

        public IPred SynthesizeMapCond(IEnumerable<Cell> cells)
        {
            var cellList = cells.ToList();
            // gen the strongest conjunction
            var iterState = new IterState(inputSpr, seq);
            var currentIdx = 0;
            var terms = new List<IPred>();
            terms.AddRange(atomicPredSymbols);
            while (true)
            {
                var searchIdx = SearchState(iterState, cellList, currentIdx);
                if (searchIdx != -1) // found
                {
                    currentIdx = searchIdx + 1;
                    // remove the predicates that are not satisfied
                    terms = terms.Where(term => term.Evaluate(iterState)).ToList();
                }

                if (!iterState.HasNext())
                    break;
                iterState.SetSelectCurrent();
                iterState.UpdateCurrentIn();
                iterState.UpdateCurrentOut();
            }

            if (!terms.Any()) return null;
            var conj = new AndPred(terms);

            return ValidCond(conj, cellList) ? conj : null;
        }
    }
}