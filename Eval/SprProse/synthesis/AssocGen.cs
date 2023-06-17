using System;
using System.Collections.Generic;
using System.Linq;

namespace SprProse.synthesis
{
    using Cell = Tuple<Coord, string>;
    using Mapping = IEnumerable<Tuple<Tuple<Coord, string>, Tuple<Coord, string>>>;

    public class AssocGen
    {
        private readonly SprSheet inputSpr;
        private readonly SprSheet outputSpr;
        private readonly List<IRelFunc> relFunctionsIn;
        private readonly List<IRelFunc> relFunctionsOut;

        private void InitRelFunctions()
        {
            for (var i = 0; i < inputSpr.NumRows; ++i)
                relFunctionsIn.Add(new RelRow(i));
            for (var i = 0; i < inputSpr.NumCols; ++i)
                relFunctionsIn.Add(new RelCol(i));
            for (var i = 0; i < outputSpr.NumRows; ++i)
                relFunctionsOut.Add(new RelRow(i));
            for (var i = 0; i < outputSpr.NumCols; ++i)
                relFunctionsOut.Add(new RelCol(i));
        }

        public AssocGen(SprSheet inputSpr, SprSheet outputSpr)
        {
            this.inputSpr = inputSpr;
            this.outputSpr = outputSpr;
            relFunctionsIn = new List<IRelFunc>();
            relFunctionsOut = new List<IRelFunc>();
            InitRelFunctions();
        }
        
        public IEnumerable<Tuple<IRelFunc, IRelFunc, Mapping>> EnumerateRelFunc(Mapping mapping)
        {
            var mappingL = mapping.ToList();
            foreach (var relFuncIn in relFunctionsIn)
            {
                foreach (var relFuncOut in relFunctionsOut)
                {
                    var newMapping = RelateSemantics.Associate(mappingL, relFuncIn, relFuncOut, null, inputSpr).ToList();
                    if (Consistent(newMapping))
                        yield return new Tuple<IRelFunc, IRelFunc, Mapping>(relFuncIn, relFuncOut, newMapping);
                }
            }
        }

        private bool Consistent(List<Tuple<Cell, Cell>> newMapping)
        {
            foreach (var (inCell, outCell) in newMapping)
            {
                var actualVal = inCell.Item2;
                var expectedVal = outputSpr.GetValue(outCell.Item1.X, outCell.Item1.Y);
                if (actualVal != expectedVal)
                {
                    return false;
                }
            }

            return true;
        }
    }
}