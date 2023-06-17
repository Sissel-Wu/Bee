using System;
using System.Collections.Generic;
using SprProse.synthesis;

namespace SprProse
{
    using Cell = Tuple<Coord, string>;
    
    public class IterState
    {
        public IterState(SprSheet inputSpr, SequenceMaker sequenceMaker)
        {
            this.inputSpr = inputSpr;
            this.sequenceMaker = sequenceMaker;
            CurrentIn = inputSpr.GetInitCell();
            CurrentOut = sequenceMaker.GetInitCell();
            SelectedData = new HashSet<string>();
            VisitedData = new HashSet<string>();
        }

        private readonly SprSheet inputSpr;
        private readonly SequenceMaker sequenceMaker;
        
        public Cell CurrentIn { get; private set; }
        public Cell CurrentOut { get; private set; }
        public HashSet<string> SelectedData { get; private set; }
        public HashSet<string> VisitedData { get; private set; }

        public void SetSelectCurrent()
        {
            SelectedData.Add(CurrentIn.Item2);
        }

        public void UpdateCurrentIn()
        {
            int row = CurrentIn.Item1.X, col = CurrentIn.Item1.Y;
            var cell = inputSpr.GetNextCell(row, col);
            if (cell != null)
            {
                VisitedData.Add(CurrentIn.Item2);
                CurrentIn = cell;
            }
        }
        
        public void UpdateCurrentOut()
        {
            var cell = sequenceMaker.GetNextCell(CurrentOut.Item1.X, CurrentOut.Item1.Y);
            if (cell != null)
            {
                CurrentOut = cell;
            }
        }

        public bool HasNext()
        {
            int row = CurrentIn.Item1.X, col = CurrentIn.Item1.Y;
            return row < inputSpr.NumRows 
                   && col < inputSpr.NumCols 
                   && !(row == inputSpr.NumRows-1 && col == inputSpr.NumCols-1);
        }
    }
}