using System;
using System.Collections.Generic;
using System.Linq;
using LibProse.hades;

namespace LibProse.synthesis
{
    public static class CommonDeductions<T, TP>
        where T: HDTNode, new()
        where TP: HDTPath<T, TP>, new()
    {
        private static List<Tuple<bool, int>> Align(IReadOnlyList<SummarizedForm<T, TP>> summarizedFormsOfExample,
            IReadOnlyList<List<List<int>>> partitionsOfExample)
        {
            var rst = new List<Tuple<bool, int>> (); // int is the number of segments each big-segment(can be MergeSegment) takes
            
            // group the segments by node (identify the MergeSegments)
            var encodedToNode = summarizedFormsOfExample[0].EncodedToNode;
            for (var j = 0; j < partitionsOfExample[0].Count; )
            {
                var segmentToEncoded = partitionsOfExample[0][j];
                var nodeIndexes = segmentToEncoded.Select(e => encodedToNode[e]).ToList();
                var isMerged = nodeIndexes.Count > 1 && nodeIndexes.Distinct().Count() == 1;
                var k = j + 1;
                for (; k < partitionsOfExample[0].Count; ++k)
                {
                    var nodeIndexesNext = partitionsOfExample[0][k].Select(e => encodedToNode[e]).ToList();
                    if (nodeIndexes.First() == nodeIndexesNext.First())
                        isMerged = true;
                    else
                        break;
                }

                if (isMerged)
                {
                    rst.Add(new Tuple<bool, int>(true, k-j));
                    j = k;
                }
                else
                {
                    ++j;
                    rst.Add(new Tuple<bool, int>(false, 1));
                }
            }
            return rst;
        }

        private static PathTerm<T, TP> ComputeCandidatePartCombination(
            IEnumerable<Tuple< IEnumerable<Tuple<int, int, int, List<T>>>, List<List<int>> >> cartesianProduct,
            IReadOnlyList<SummarizedForm<T, TP>> summarizedForms,
            IReadOnlyList<TP> inputPaths, IdMap<T, TP> idMap)
        {
            var rst = new List<ISegmentTerm<T,TP>>();
            
            var candidate = cartesianProduct.ToList();
            foreach (var (form, _) in candidate)
            {
                foreach (var node in form) Console.Write(node + ", ");
                Console.WriteLine();
            }
            Console.WriteLine("---");
            
            // invoke SMT solver to check if the forms have a common path
            var theForms = candidate.Select(x => x.Item1).ToList();
            var thePartitions = candidate.Select(x => x.Item2).ToList();
            var model = Z3Solving.SolvePathConstraints(theForms);
            if (model == null) return null;
            // retrieve from z3 model
            var dict = new Dictionary<string, object>();
            foreach (var (funcDecl, expr) in model.Consts)
            {
                if (expr.IsBool)
                    dict[funcDecl.Name.ToString()] = bool.Parse(expr.ToString());
                else
                    dict[funcDecl.Name.ToString()] = int.Parse(expr.ToString());
            }
            
            var alignedSegments = Align(summarizedForms, thePartitions);
            var curr = 0;
            var allDone = true;
            foreach (var (isMerged, num) in alignedSegments)
            {
                var indexArgs = new List<Tuple<bool, int, bool, int>>();
                for (var i = curr; i < curr + num; ++i)
                {
                    string beginB = "bb_" + i, beginC = "bc_" + i, endB = "eb_" + i, endC = "ec_" + i;
                    bool bb = (bool)dict[beginB], eb = (bool)dict[endB];
                    int bc = (int)dict[beginC], ec = (int)dict[endC];
                    indexArgs.Add(new Tuple<bool, int, bool, int>(bb, bc, eb, ec));
                }
                var learnableForms = new List<List<T>>();
                foreach (var ex in theForms)
                {
                    var exList = ex.ToList();
                    var path = new List<T>();
                    for (var i = curr; i < curr + num; ++i)
                    {
                        path.AddRange(exList[i].Item4);
                    }
                    learnableForms.Add(path);
                }
                // call to learn the segment
                var learnedSegment = isMerged ?
                    TransformLearner<T, TP>.LearnMergeSegment(inputPaths, learnableForms, indexArgs, idMap) :
                    TransformLearner<T, TP>.LearnMapSegment(inputPaths, learnableForms, indexArgs, idMap);
                if (learnedSegment == null)
                {
                    allDone = false;
                    break;
                }
                rst.Add(learnedSegment);
                curr += num;
            }

            return allDone ? new PathTerm<T, TP>(rst) : null;
        }

        private static PathTerm<T, TP> ComputePathTerm(IEnumerable<Tuple<TP, TP>> examples, IdMap<T, TP> idMap)
        {
            var exampleList = examples.ToList(); 
            var summarizedForms = exampleList.Select(x => new SummarizedForm<T, TP>(x, idMap)).ToList();
            var coalescedForms = summarizedForms.Select(x => new CoalescedForm<T, TP>(x)).ToList();

            //var minOutLength = exampleList.Select(example => example.Item2.Length()).Min(); // should be min since we are computing intersection
            var minOutLength = summarizedForms.Select(x => x.EncodedPath.Count).Min();
            var exampleInputs = exampleList.Select(x => x.Item1).ToList();
            for (var l = 1; l <= minOutLength; ++l)
            {
                var l1 = l;
                var formsOfLengthL = coalescedForms.Select(x => x.OfLength(l1)).ToList();
                var cartesianProducts = EnumerateCartesianProducts(formsOfLengthL).ToList();
                foreach (var cartesianProduct in cartesianProducts)
                {
                    var temptRst = ComputeCandidatePartCombination(cartesianProduct, summarizedForms, exampleInputs, idMap);
                    if (temptRst != null) return temptRst;
                }
            }

            return null;
        }

        public static PathTerm<T, TP> Unify(ISet<Tuple<TP, TP>> examples, IdMap<T, TP> idMap)
        {
            if (!examples.Any()) return null;
            
            var cache = PathTermsCache<T, TP>.GetInstance();
            if (cache.ContainsKey(examples))
            {
                return cache.Get(examples);
            }

            var pathTerm = ComputePathTerm(examples, idMap);
            cache.Add(examples, pathTerm);
            return pathTerm;
        }

        public static IEnumerable<TA> NegateSubset<TA>(IEnumerable<TA> universe, IEnumerable<TA> subset)
        {
            var enumerable = subset.ToList();
            foreach (var elem in universe)
            {
                if (!enumerable.Contains(elem))
                    yield return elem;
            }
        }
        
        private static IEnumerable<IEnumerable<TA>> EnumerateSubsetsSmallToLarge<TA>(IEnumerable<TA> universeSet)
        {
            var list = universeSet.ToList();
            var length = list.Count;
            var max = (int)Math.Pow(2, list.Count);

            for (var count = 0; count < max; count++)
            {
                var subset = new List<TA>();
                uint rs = 0;
                while (rs < length)
                {
                    if ((count & (1u << (int)rs)) > 0)
                    {
                        subset.Add(list[(int)rs]);
                    }
                    rs++;
                }
                yield return subset;
            }
        }

        private static IEnumerable<IEnumerable<IEnumerable<Tuple<TP, TP>>>> EnumeratePartitions
            (ISet<Tuple<TP, TP>> currentSet, ISet<Tuple<TP, TP>> restSet, int k, IdMap<T, TP> idMap)
        {
            if (k == 1)
            {
                var uSet = currentSet.Union(restSet).ToHashSet();
                var unified = Unify(uSet, idMap);
                if (unified != null) yield return new []{uSet};
                yield break;
            }

            foreach (var e in restSet)
            {
                var newCurrent = currentSet.Concat(new[] {e}).ToHashSet();
                var unified = Unify(newCurrent, idMap);
                if (unified == null) continue;
                var newRest = restSet.Where(x => !Equals(x, e)).ToHashSet();
                var subPartitions = EnumeratePartitions(new HashSet<Tuple<TP, TP>>(), newRest, k - 1, idMap).ToList();
                if (subPartitions.Any())
                {
                    foreach (var partition in subPartitions)
                    {
                        var newPartition = partition.ToList();
                        newPartition.Insert(0, newCurrent);
                        yield return newPartition;
                    }
                    yield break;
                }

                subPartitions = EnumeratePartitions(newCurrent, newRest, k, idMap).ToList();
                if (!subPartitions.Any()) continue;
                foreach (var partition in subPartitions)
                {
                    yield return partition;
                }
            }
        }

        public static IEnumerable<IEnumerable<IEnumerable<Tuple<TP, TP>>>> EnumeratePartitions
            (ISet<Tuple<TP, TP>> universeSet, IdMap<T, TP> idMap)
        {
            for (var i = 1; i <= universeSet.Count; ++i)
            {
                foreach (var partition in EnumeratePartitions(new HashSet<Tuple<TP, TP>>(), universeSet, i, idMap))
                {
                    yield return partition;
                }
            }
        }

        // return subsets of universeSet 
        public static IEnumerable<IEnumerable<TA>> EnumerateSubsets<TA>(IEnumerable<TA> universeSet)
        {
            var universeList = universeSet.ToList();
            foreach (var subset in EnumerateSubsetsSmallToLarge(universeList))
            {
                // return the negated subset
                Console.WriteLine("yielded another");
                yield return NegateSubset(universeList, subset);
            }
        }
        
        public static IEnumerable<IEnumerable<TA>> EnumerateCartesianProducts<TA>(IEnumerable<IEnumerable<TA>> sets)
        {
            var list = sets.ToList();
            if (list.Count == 0)
            {
                yield return new List<TA>();
                yield break;
            }

            var first = list.First();
            var rest = list.Skip(1);
            var subCartesianProducts = EnumerateCartesianProducts(rest).ToList();
            foreach (var f in first)
            {
                foreach (var r in subCartesianProducts)
                {
                    yield return new List<TA>{f}.Concat(r);
                }
            }
        }
    }
}