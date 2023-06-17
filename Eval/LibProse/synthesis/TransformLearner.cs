using System;
using System.Collections.Generic;
using System.Linq;
using LibProse.hades;
using Microsoft.ProgramSynthesis.Utils.Interactive;

namespace LibProse.synthesis
{
    public static class TransformLearner<T, TP>
        where T: HDTNode, new()
        where TP: HDTPath<T, TP>, new()
    {
        public static List<Tuple<int, IMapper<T>, int>> LearnMergeString(string targetStr, TP originPath, IdMap<T, TP> idMap)
        {
            var originLength = originPath.Length();
            var rst = new List<Tuple<int, IMapper<T>, int>>();
            foreach (var (obj, id) in idMap.GetIterator())
            {
                if (obj is not IMapper<T> mapper) continue;
                for (var j = 0; j < originLength; ++j)
                {
                    var nodeJ = originPath.At(j);
                    var mappedStr = (string) mapper.Evaluate(nodeJ).Key;                    
                    if (mappedStr == targetStr) // matched whole remained string
                    {
                        rst.Add(new Tuple<int, IMapper<T>, int>(j, mapper, id));
                        return rst;
                    }

                    if (!targetStr.StartsWith(mappedStr) || mappedStr.IsEmpty()) continue;
                    var subResult = LearnMergeString(targetStr[mappedStr.Length..], originPath, idMap)?.ToList();
                    if (subResult is not {Count: > 0}) continue;
                    rst.Add(new Tuple<int, IMapper<T>, int>(j, mapper, id));
                    rst.AddRange(subResult);
                    return rst;                
                }
            }
            // not found
            return null;
        }

        private static bool CheckSegment(ISegmentTerm<T, TP> segmentTerm, 
            List<TP> fromPaths, List<IEnumerable<T>> toSegments)
        {
            for (var ex = 0; ex < fromPaths.Count; ++ex)
            {
                var expectedOut = toSegments[ex].Aggregate(new TP(), (current, node) => current.Add(node));
                var actualOut = segmentTerm.Evaluate(fromPaths[ex]);
                if (!actualOut.Equals(expectedOut)) return false;
                if (!segmentTerm.CanMap(fromPaths[ex], expectedOut)) return false;
            }

            return true;
        }

        public static ISegmentTerm<T, TP> LearnMapSegment(IEnumerable<TP> fromPaths, 
            IEnumerable<IEnumerable<T>> toSegments,
            IEnumerable<Tuple<bool, int, bool, int>> indexArgs, IdMap<T, TP> idMap)
        {
            var fromPathList = fromPaths.ToList();
            var toSegmentList = toSegments.ToList();
            var (bb, bc, eb, ec) = indexArgs.First();
            
            foreach (var (obj, id) in idMap.GetIterator())
            {
                if (obj is IMapper<T> mapper) // mapper
                {
                    var temptSegment = MakeSegment(mapper, bb, bc, eb, ec, idMap.MaxInLength);
                    if (CheckSegment(temptSegment, fromPathList, toSegmentList))
                    {
                        return temptSegment;
                    }                    
                }
                if (id < 0) // constNode
                {
                    var constSegment = MakeSegment(new ConstMapper<T>(obj), bb, bc, eb, ec, idMap.MaxInLength);
                    if (CheckSegment(constSegment, fromPathList, toSegmentList))
                    {
                        return constSegment;
                    }
                }
            }
            return null;
        }

        public static ISegmentTerm<T, TP> LearnMergeSegment(IEnumerable<TP> fromPaths,
            IEnumerable<IEnumerable<T>> toSegments,
            IEnumerable<Tuple<bool, int, bool, int>> indexArgs, IdMap<T, TP> idMap)
        {
            var fromPathList = fromPaths.ToList();
            var toSegmentList = toSegments.ToList();
            var indexArgList = indexArgs.ToList();
            var maxInLength = fromPathList.Select(x => x.Length()).Max();

            for (var ex = 0; ex < fromPathList.Count; ++ex)
            {
                var targetStr = (string) toSegmentList[ex].First().Key;
                var subResult = LearnMergeString(targetStr, fromPathList[ex], idMap);
                var mappers = subResult.Select(x => x.Item2).ToList();
                if (mappers.Count != indexArgList.Count) continue;
                var temp = new List<Tuple<int, int, IMapper<T>>>();
                for (var j = 0; j < mappers.Count; ++j)
                {
                    var (bb, bc, eb, ec) = indexArgList[j];
                    var start = GetIndex(bb, bc, maxInLength);
                    var end = GetIndex(eb, ec, maxInLength);
                    temp.Add(new Tuple<int, int, IMapper<T>>(start, end, mappers[j]));
                }
                var temptSegment = new MergeSegmentTerm<T, TP>(temp);
                var deduplicateTos = toSegmentList.Select(x => x.Distinct()).ToList();
                if (CheckSegment(temptSegment, fromPathList, deduplicateTos))
                {
                    return temptSegment;
                }
            }
            
            return null;
        }

        private static int GetIndex(bool b, int c, int maxLength)
        {
            if (c <= -maxLength * 2) // constant
            {
                return 0;
            }
            if (-maxLength <= c && c < maxLength) // index
            {
                return c;
            }
            
            // mapper
            var mod2L = c % (2 * maxLength);
            return mod2L >= maxLength ? mod2L - 2 * maxLength : mod2L;
        }

        public static MapSegmentTerm<T, TP> MakeSegment(IMapper<T> mapper, bool b1, int c1, bool b2, int c2, int maxLength)
        {
            // b * length + c is the final value
            var start = GetIndex(b1, c1, maxLength);
            var end = GetIndex(b2, c2, maxLength);
            return new MapSegmentTerm<T, TP>(mapper, start, end);
        }
    }
}