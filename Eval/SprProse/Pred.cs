using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SprProse
{
    using Cell = Tuple<Coord, string>;
    
    public interface IPred
    {
        public bool Evaluate(IterState o);
    }
    
    public interface ICellPred : IPred
    {
        public bool Evaluate(Cell o);
    }

    public class AndPred : IPred
    {
        private readonly IEnumerable<IPred> subTerms;

        public AndPred(IEnumerable<IPred> subTerms)
        {
            this.subTerms = subTerms;
        }

        public AndPred(params IPred[] subTerms)
        {
            this.subTerms = subTerms.ToList();
        }

        public bool Evaluate(IterState o)
        {
            return subTerms.All(subTerm => subTerm.Evaluate(o));
        }

        public override string ToString()
        {
            var sb = new StringBuilder("And(");
            foreach (var subTerm in subTerms)
            {
                sb.Append(subTerm).Append(',');
            }
            return sb.ToString();
        }
    }
    
    public class NotPred : IPred
    {
        private readonly IPred subTerm;

        public NotPred(IPred subTerm)
        {
            this.subTerm = subTerm;
        }

        public bool Evaluate(IterState o)
        {
            return !subTerm.Evaluate(o);
        }

        public override string ToString()
        {
            return $"!{subTerm}";
        }
    }
    
    public class RowEq : IPred, ICellPred
    {
        private readonly int rowValue;

        public RowEq(int rowValue)
        {
            this.rowValue = rowValue;
        }

        public bool Evaluate(IterState o)
        {
            return o.CurrentIn.Item1.X == rowValue;
        }

        public bool Evaluate(Cell o)
        {
            return o.Item1.X == rowValue;
        }
        
        public override string ToString()
        {
            return $"RowEq({rowValue})";
        }
    }
    
    public class ColEq : IPred, ICellPred
    {
        private readonly int colValue;

        public ColEq(int colValue)
        {
            this.colValue = colValue;
        }

        public bool Evaluate(IterState o)
        {
            return o.CurrentIn.Item1.Y == colValue;
        }

        public bool Evaluate(Cell o)
        {
            return o.Item1.Y == colValue;
        }
        
        public override string ToString()
        {
            return $"ColEq({colValue})";
        }
    }

    public class DataEq : IPred, ICellPred
    {
        private readonly string dataValue;

        public DataEq(string dataValue)
        {
            this.dataValue = dataValue;
        }

        public bool Evaluate(IterState o)
        {
            return o.CurrentIn.Item2 == dataValue;
        }

        public bool Evaluate(Cell o)
        {
            return o.Item2 == dataValue;
        }
        
        public override string ToString()
        {
            return $"DataEq({dataValue})";
        }
    }

    public class FirstReadValue : IPred
    {
        public bool Evaluate(IterState o)
        {
            var val = o.CurrentIn.Item2;
            return !o.VisitedData.Contains(val);
        }
        
        public override string ToString()
        {
            return "FirstRead";
        }
    }
    
    public class ValueBeenSelected : IPred
    {
        public bool Evaluate(IterState o)
        {
            var val = o.CurrentIn.Item2;
            return o.SelectedData.Contains(val);
        }
        
        public override string ToString()
        {
            return "ValueSelected";
        }
    }
}