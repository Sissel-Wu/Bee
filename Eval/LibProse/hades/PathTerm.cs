using System.Collections.Generic;
using System.Linq;

namespace LibProse.hades
{
    public class PathTerm<T, TP> 
        where T: HDTNode
        where TP: HDTPath<T, TP>, new()
    {
        private readonly List<ISegmentTerm<T, TP>> segments;

        public PathTerm(List<ISegmentTerm<T, TP>> segments)
        {
            this.segments = segments;
        }

        public PathTerm(params ISegmentTerm<T, TP>[] segments)
        {
            this.segments = segments.ToList();
        }

        public TP Evaluate(TP path)
        {
            var rst = new TP();

            return segments.Aggregate(rst, (current, segment) => current.Concat(segment.Evaluate(path)));
        }
        
        public override string ToString()
        {
            return string.Join("++", segments);
        }
    }
}