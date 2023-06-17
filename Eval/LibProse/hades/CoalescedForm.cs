using System;
using System.Collections.Generic;
using System.Linq;

namespace LibProse.hades
{
    public class CoalescedForm<T, TP>
        where T: HDTNode, new()
        where TP: HDTPath<T, TP>, new()
    {
        private readonly SummarizedForm<T, TP> summarizedForm;
        private readonly List<int> mergeableNodeLengths;
        private readonly Dictionary<int, IEnumerable<Tuple< IEnumerable<Tuple<int, int, int, List<T>>>, List<List<int>> >>> lengthDict;

        public CoalescedForm(SummarizedForm<T, TP> summarizedForm)
        {
            this.summarizedForm = summarizedForm;
            mergeableNodeLengths = new List<int>();
            for (var set = 0; ; ++set)
            {
                var num = summarizedForm.EncodedToNode.Count(x => x == set);
                mergeableNodeLengths.Add(num);
                if (num == 0) break;
            }
            lengthDict = new Dictionary<int, IEnumerable<Tuple< IEnumerable<Tuple<int, int, int, List<T>>>, List<List<int>> >>>();
        }

        private static bool CanShrink(Tuple<int, int, int, List<T>> node1, Tuple<int, int, int, List<T>> node2)
        {
            // if (node1.Item4.SequenceEqual(node2.Item4)) return true;
            return node1.Item1 == node2.Item1 && node1.Item3 == node2.Item2-1;
        }

        // merge node_i and node_i+1
        private static IEnumerable<Tuple<int, int, int, List<T>>> Shrink(IEnumerable<Tuple<int, int, int, List<T>>> origin, int i)
        {
            var rst = new List<Tuple<int, int, int, List<T>>>();
            var list = origin.ToList();

            var sameItem4 = list[i].Item4.SequenceEqual(list[i+1].Item4);
            var newItem4 = sameItem4 ? list[i].Item4 : list[i].Item4.Concat(list[i+1].Item4).ToList();
            
            rst.AddRange(list.Take(i));
            rst.Add(new (list[i].Item1, list[i].Item2, list[i+1].Item3, newItem4));
            rst.AddRange(list.TakeLast(list.Count-i-2));
            return rst;
        }

        // generate list of paths that has length k-1, where k is the length of inputForm
        private static IEnumerable<Tuple<int, int, int, List<T>>> ShrinkAll(IEnumerable<Tuple<int, int, int, List<T>>> inputForm, IEnumerable<int> shrinkPoints)
        {
            var list = inputForm.ToList();
            var ps = shrinkPoints as int[] ?? shrinkPoints.ToArray();
            if (ps.Any(t => !CanShrink(list[t], list[t+1])))
            {
                return null;
            }
            IEnumerable<Tuple<int, int, int, List<T>>> temp = list;
            for (var i = ps.Count() - 1; i >= 0; --i)
            {
                temp = Shrink(temp, ps[i]);
            }
            return temp;
        }

        // return non-duplicate point-sets (arity is k), each element e in each set should satisfy begin <= e < n-1
        public static IEnumerable<List<int>> GenShrinkPoints(int k, int n, int begin=0)
        {
            if (k > n-1 - begin)
            {
                yield break;
            }
            if (k == 0)
            {
                yield return new List<int>();
            }
            else if (k == 1)
            {
                for (var i = begin; i < n-1; ++i)
                {
                    yield return new List<int> { i };
                }
            }
            else
            {
                // pick begin
                foreach (var e in GenShrinkPoints(k-1, n, begin+1))
                {
                    e.Insert(0, begin);
                    yield return e;
                }
                    
                // do not pick begin
                foreach (var e in GenShrinkPoints(k, n, begin+1))
                {
                    yield return e;
                }
            }
        }

        private static List<List<int>> CalMergePartition(List<int> shrinkPoints, int length)
        {
            var rst = new List<List<int>>();
            var curr = 0;
            var temp = new List<int>();
            for (var i = 0; i < length; ++i)
            {
                temp.Add(i);
                if (curr < shrinkPoints.Count && shrinkPoints[curr] == i)
                {
                    ++curr;
                }
                else
                {
                    rst.Add(temp); 
                    temp = new List<int>();
                }
            }
            return rst;
        }

        private IEnumerable<Tuple< IEnumerable<Tuple<int, int, int, List<T>>>, List<List<int>> >> GetCoalescedFormsOfLength(int k)
        {
            if (k <= 0 || k > summarizedForm.EncodedPath.Count)
                yield break;
            
            var wrapped = summarizedForm.EncodedPath.Select(node => 
                new Tuple<int, int, int, List<T>>(node.Item1, node.Item2, node.Item2, new List<T> {node.Item3})).ToList();
            if (k == wrapped.Count)
            {
                var mergePartition = CalMergePartition(new List<int>(), wrapped.Count);
                yield return new Tuple< IEnumerable<Tuple<int, int, int, List<T>>>, List<List<int>> > (wrapped, mergePartition);
            }
            else
            {
                foreach (var shrinkPoints in GenShrinkPoints(wrapped.Count-k, wrapped.Count))
                {
                    var shrinked = ShrinkAll(wrapped, shrinkPoints);
                    var mergePartition = CalMergePartition(shrinkPoints, wrapped.Count);
                    if (shrinked != null)
                    {
                        yield return new Tuple<IEnumerable<Tuple<int, int, int, List<T>>>, List<List<int>>> (shrinked, mergePartition);
                    }
                }
            }
        }

        public IEnumerable<Tuple< IEnumerable<Tuple<int, int, int, List<T>>>, List<List<int>> >> OfLength(int k)
        {
            if (!lengthDict.ContainsKey(k))
            {
                lengthDict[k] = GetCoalescedFormsOfLength(k);
            }
            return lengthDict[k];
        }
    }
}