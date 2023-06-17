using System;
using System.Collections.Generic;

namespace SprProse.synthesis
{
    // only used to 
    
    using Cell = Tuple<Coord, string>;
    using Mapping = IEnumerable<Tuple<Tuple<Coord, string>, Tuple<Coord, string>>>;

    public class CompProgram
    {
        public Mapping Mapping { get; }

        protected CompProgram(Mapping mapping)
        {
            Mapping = mapping;
        }
    }

    public class FilterProgram : CompProgram
    {
        public IPred MapCond { get; }
        public SequenceMaker Seq { get; }
        
        public FilterProgram(Mapping mapping, IPred mapCond, SequenceMaker seq): base(mapping)
        {
            MapCond = mapCond;
            Seq = seq;
        }

        public override string ToString()
        {
            return $"Filter({MapCond});Seq={Seq}";
        }
    }

    public class AssocProgram : CompProgram
    {
        public CompProgram Parent { get; }
        public IRelFunc RelFuncIn { get; }
        public IRelFunc RelFuncOut { get; }

        public AssocProgram(CompProgram parent, Mapping mapping, IRelFunc relFuncIn, IRelFunc relFuncOut): base(mapping)
        {
            Parent = parent;
            RelFuncIn = relFuncIn;
            RelFuncOut = relFuncOut;
        }

        public override string ToString()
        {
            return $"Assoc({RelFuncIn}, {RelFuncOut}, {Parent})";
        }
    }
}