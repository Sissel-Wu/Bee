using System;
using System.Collections.Generic;
using System.Linq;
using LibProse.synthesis;
using Microsoft.ProgramSynthesis.Wrangling.Exceptions;

namespace LibProse.hades
{
    public class SummarizedForm<T, TP>
        where T: HDTNode, new()
        where TP: HDTPath<T, TP>, new()
    {
        public Tuple<TP, TP> PathPair { get; }
        public List<Tuple<int, int, T>> EncodedPath { get; private set; }
        public List<int> EncodedToNode { get; private set; }

        public SummarizedForm(Tuple<TP, TP> pathPair, IdMap<T, TP> idMap)
        {
            PathPair = pathPair;
            EncodedPath = new List<Tuple<int, int, T>>();
            EncodedToNode = new List<int>();
            Compute(idMap);
        }

        private static IEnumerable<Tuple<int, int, T>> EncodeNode(T node, TP originPath, IdMap<T, TP> idMap)
        {
            var originLength = originPath.Length();
            // no change
            for (var j = 0; j < originLength; ++j)
            {
                //if (originPath.At(j).Key.Equals(node.Key))
                if (originPath.At(j).AllEquals(node))
                    return new List<Tuple<int, int, T>> {new (originLength, j, node)};
            }
            // mapSegment
            for (var j = 0; j < originLength; ++j)
            {
                var nodeJ = originPath.At(j);
                foreach (var (obj, id) in idMap.GetIterator())
                {
                    if (obj is not IMapper<T> mapper) continue;
                    if (mapper.CanMap(nodeJ, node))
                        return new List<Tuple<int, int, T>> {new (originLength, id + j, node)};
                }
            }
            // mergeSegment
            if (node.Key is string targetStr)
            {
                var mergeComponents = TransformLearner<T, TP>.LearnMergeString(targetStr, originPath, idMap);
                if (mergeComponents != null && mergeComponents.Any())
                    return mergeComponents.Select(x => new Tuple<int, int, T>(originLength, x.Item1+x.Item3, node));
                
                // if (targetStr.EndsWith(".tar")) targetStr = targetStr[..^4];
                // mergeComponents = TransformLearner<T, TP>.LearnMergeString(targetStr, originPath, idMap);
                // if (mergeComponents != null && mergeComponents.Any())
                // {
                //     mergeComponents.Add(new  Tuple<int, IMapper<T>, int>(0, new ConstMapper<T>(".tar"), idMap.MaxInLength * -2000));
                //     return mergeComponents.Select(x => new Tuple<int, int, T>(originLength, x.Item1+x.Item3, node));
                // }
            }

            foreach (var (key, constId) in idMap.GetIterator())
            {
                if (key.Equals(node.Key))
                    return new List<Tuple<int, int, T>> {new (originLength, constId, node)};    
            }
            
            throw new InvalidInputException("id map does not have value for node " + node);
        }

        private void Compute(IdMap<T, TP> idMap)
        {
            var nodeIdx = 0;
            foreach (var node in PathPair.Item2.GetIterator())
            {
                var encoded = EncodeNode(node, PathPair.Item1, idMap).ToList();
                foreach (var n in encoded)
                {
                    EncodedPath.Add(n);
                    EncodedToNode.Add(nodeIdx);
                }
                ++nodeIdx;
            }
        }
    }
}