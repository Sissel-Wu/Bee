using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibProse.hades
{
    public interface ISegmentTerm<T, TP> 
        where T: HDTNode
        where TP: HDTPath<T, TP>, new ()
    {
        public TP Evaluate(TP path);

        public bool CanMap(TP inPath, TP outPath);
    }
    
    public class MapSegmentTerm<T, TP> : ISegmentTerm<T, TP>
        where T: HDTNode
        where TP: HDTPath<T, TP>, new ()
    {
        private IMapper<T> Mapper { get; }
        private int T1 { get; }
        private int T2 { get; }

        public MapSegmentTerm(IMapper<T> mapper, int t1, int t2)
        {
            Mapper = mapper;
            T1 = t1;
            T2 = t2;
        }

        public TP Evaluate(TP path)
        {
            var rst = new TP();
            var list = path.GetIterator().ToList();
            var begin = T1 >= 0 ? T1 : list.Count + T1;
            var end = T2 >= 0 ? T2 : list.Count + T2;
            for (var i = begin; i <= end; ++i)
            {
                rst = rst.Add(Mapper.Evaluate(list[i]));
            }

            return rst;
        }

        public bool CanMap(TP inPath, TP outPath)
        {
            var inList = inPath.GetIterator().ToList();
            var outList = outPath.GetIterator().ToList();
            var begin = T1 >= 0 ? T1 : inList.Count + T1;
            var end = T2 >= 0 ? T2 : inList.Count + T2;
            if (outList.Count != end - begin + 1) return false;
            for (var i = begin; i <= end; ++i)
            {
                if (!Mapper.CanMap(inList[i], outList[i - begin])) return false;
            }

            return true;
        }

        public override string ToString()
        {
            return $"({T1},{T2},{Mapper})";
        }
    }

    public class MergeSegmentTerm<T, TP> : ISegmentTerm<T, TP>
        where T: HDTNode, new ()
        where TP: HDTPath<T, TP>, new ()
    {
        private List<Tuple<int, int, IMapper<T>>> Components { get; }
        
        public MergeSegmentTerm(List<Tuple<int, int, IMapper<T>>> components)
        {
            Components = components;
        }
        
        public MergeSegmentTerm(params Tuple<int, int, IMapper<T>>[] components)
        {
            Components = components.ToList();
        }

        public TP Evaluate(TP path)
        {
            var sb = new StringBuilder();
            var list = path.GetIterator().ToList();
            foreach (var (t1, t2, mapper) in Components)
            {
                var begin = t1 >= 0 ? t1 : list.Count + t1;
                var end = t2 >= 0 ? t2 : list.Count + t2;
                for (var i = begin; i <= end; ++i)
                {
                    var str = mapper.Evaluate(list[i]).Key.ToString();
                    sb.Append(str);
                }
            }
            var node = new T { Key = sb.ToString() };

            return new TP().Add(node);
        }

        public bool CanMap(TP inPath, TP outPath)
        {
            if (outPath.Length() != 1) return false;
            var sb = new StringBuilder();
            var list = inPath.GetIterator().ToList();
            foreach (var (t1, t2, mapper) in Components)
            {
                var begin = t1 >= 0 ? t1 : list.Count + t1;
                var end = t2 >= 0 ? t2 : list.Count + t2;
                for (var i = begin; i <= end; ++i)
                {
                    var str = mapper.Evaluate(list[i]).Key.ToString();
                    sb.Append(str);
                }
            }

            return outPath.GetIterator().First().Key.Equals(sb.ToString());
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("[");
            foreach (var (t1, t2, mapper) in Components)
            {
                sb.Append($"({t1},{t2},{mapper})+");
            }
            sb.Remove(sb.Length-1, 1);
            sb.Append(']');
            return sb.ToString();
        }
    }
}