using System;
using System.Collections.Generic;
using System.Linq;
using LibProse.hades;

namespace LibProse.synthesis
{
    internal class TreeStruct<T, TP>
        where T : HDTNode
        where TP : HDTPath<T, TP>
    {
        public TreeStruct(List<TP> data)
        {
            Data = data;
        }

        public List<TP> Data { get; }
        public TreeStruct<T, TP> Parent { get; set; }
    }
    
    internal class Leaf<T, TP>: TreeStruct<T, TP>
        where T : HDTNode
        where TP : HDTPath<T, TP>
    {
        public Leaf(List<TP> data, bool label): base(data)
        {
            Label = label;
        }

        public bool Label { get; }
    }
    
    internal class InternalNode<T, TP>: TreeStruct<T, TP>
        where T : HDTNode
        where TP : HDTPath<T, TP>
    {
        public IFeature<T, TP> Feature { get; }
        public List<Tuple<object, TreeStruct<T, TP>>> Children { get; }

        public InternalNode(List<TP> data, IFeature<T, TP> feature, List<Tuple<object, TreeStruct<T, TP>>> children) : base(data)
        {
            Feature = feature;
            Children = children;
            foreach (var child in children)
            {
                child.Item2.Parent = this;
            }
        }
    }

    public class DecisionTree<T, TP>
        where T : HDTNode
        where TP : HDTPath<T, TP>
    {
        private readonly List<TP> inputSet;
        private readonly List<IFeature<T, TP>> features;
        private readonly List<bool> labelSet;
        
        public bool Learned { get; private set; }
        public IPred<T, TP> TruePredicate { get; private set; }
        public IPred<T, TP> FalsePredicate { get; private set; }
        public bool Success { get; private set; }

        internal TreeStruct<T, TP> Root { get; private set; }

        public DecisionTree(List<TP> inputSet, List<IFeature<T, TP>> features, List<bool> labels)
        {
            this.inputSet = inputSet;
            this.features = features;
            this.labelSet = labels;
            Learned = false;
            Success = false;
            TruePredicate = null;
            FalsePredicate = null;
            Root = null;
        }

        private static double PLogP(double p)
        {
            return p == 0 ? 0 : p * Math.Log(p, 2);
        }

        // H(S)
        private static double Entropy(ICollection<bool> labels)
        {
            var trueCount = labels.Count(x => x);
            var falseCount = labels.Count - trueCount;
            var total = trueCount + falseCount;
            var trueProb = trueCount * 1.0 / total;
            var falseProb = falseCount * 1.0 / total;
            var entropy = -PLogP(trueProb) - PLogP(falseProb);
            return entropy;
        }

        // H(S|partition)
        private static double PartitionEntropy(IEnumerable<ICollection<bool>> labelSets)
        {
            var rst = 0d;
            var totalCount = 0;
            foreach (var labelSet in labelSets)
            {
                var entropy = Entropy(labelSet);
                rst += entropy * labelSet.Count;
                totalCount += labelSet.Count;
            }

            return rst / totalCount;
        }
        
        private static void PartitionByFeature(List<TP> data, List<bool> labels, IFeature<T, TP> feature,
            out List<List<TP>> partitions, out List<List<bool>> partitionLabels)
        {
            var dict = new Dictionary<object, Tuple<List<TP>, List<bool>>>();
            var nullList = new List<TP>();
            var nullLabels = new List<bool>();
            for (var i = 0; i < data.Count; ++i)
            {
                var value = feature.EvaluateFeature(data[i]);
                if (value == null)
                {
                    nullList.Add(data[i]);
                    nullLabels.Add(labels[i]);
                }
                else
                {
                    if (!dict.ContainsKey(value))
                        dict[value] = new Tuple<List<TP>, List<bool>>(new List<TP>(), new List<bool>());
                    var tuple = dict[value];
                    tuple.Item1.Add(data[i]);
                    tuple.Item2.Add(labels[i]);                    
                }
            }

            partitions = dict.Select(kvp => kvp.Value.Item1).ToList();
            if (nullList.Any()) partitions.Add(nullList);
            partitionLabels = dict.Select(kvp => kvp.Value.Item2).ToList();
            if (nullLabels.Any()) partitionLabels.Add(nullLabels);
        }

        private TreeStruct<T, TP> LearnTreeRecursive(List<TP> data, List<bool> labels)
        {
            var label0 = labels[0];
            if (labels.TrueForAll(x => x == label0)) // base case => leaf
            {
                return new Leaf<T, TP>(data, label0);
            }
                
            // recursive case => internal node
            // find the best feature (with max information gain)
            var largestGain = double.MinValue;
            IFeature<T, TP> bestFeature = null;
            var originalEntropy = Entropy(labels);
            foreach (var feature in features)
            {
                PartitionByFeature(data, labels, feature, out var _, out var partitionLabels);
                var partitionEntropy = PartitionEntropy(partitionLabels);
                var gain = originalEntropy - partitionEntropy; // InfoGain(H, partition) = H(S) - H(S|partition)
                if (gain > largestGain)
                {
                    largestGain = gain;
                    bestFeature = feature;
                }
            }
                
            if (bestFeature == null || largestGain <= 0) return null;
            PartitionByFeature(data, labels, bestFeature, out var childrenData, out var childrenLabels);
            var children = new List<Tuple<object, TreeStruct<T, TP>>>();
            for (var i = 0; i < childrenData.Count; ++i)
            {
                var childTree = LearnTreeRecursive(childrenData[i], childrenLabels[i]);
                if (childTree == null) return null;
                var val = bestFeature.EvaluateFeature(childrenData[i][0]);
                children.Add(new Tuple<object, TreeStruct<T, TP>>(val, childTree));
            }
            
            return new InternalNode<T, TP>(data, bestFeature, children);
        }

        private static IEnumerable<List<Tuple<IFeature<T, TP>, object>>>
            GetLeafsLabeledWith(TreeStruct<T, TP> now, bool label, List<Tuple<IFeature<T,TP>, object>> conjunction)
        {
            if (now is Leaf<T, TP> leaf)
            {
                if (leaf.Label == label)
                {
                    yield return conjunction;
                }
            }
            else
            {
                var internalNode = (InternalNode<T, TP>) now;
                foreach (var (value, child) in internalNode.Children)
                {
                    var newConjunction = conjunction.Concat(new[]
                        {new Tuple<IFeature<T, TP>, object>(internalNode.Feature, value)}).ToList();
                    foreach (var result in GetLeafsLabeledWith(child, label, newConjunction))
                    {
                        yield return result;
                    }
                }
            }
        }

        private static IPred<T, TP> ConjunctAll(IEnumerable<Tuple<IFeature<T, TP>, object>> equations)
        {
            var list = equations.ToList();
            if (!list.Any()) return new TruePred<T, TP>();
            IPred<T, TP> tempConj = null;
            foreach (var (feature, value) in list)
            {
                var term = new FeatureEquals<T, TP>(feature, value);
                if (tempConj == null)
                    tempConj = term;
                else
                    tempConj = new AndPred<T, TP>(tempConj, term);
            }
            return tempConj;
        }

        private static IPred<T, TP> GeneratePred(TreeStruct<T, TP> root, bool label)
        {
            var paths = GetLeafsLabeledWith(root, label, new List<Tuple<IFeature<T, TP>, object>>()).ToList();
            if (!paths.Any()) return new TruePred<T, TP>();
            IPred<T, TP> rst = null;
            foreach (var equations in paths)
            {
                var conjunct = ConjunctAll(equations);
                if (rst == null)
                    rst = conjunct;
                else
                    rst = new OrPred<T, TP>(rst, conjunct);
            }
            return rst;
        }
        
        public void Learn()
        {
            Learned = true;
            Root = LearnTreeRecursive(inputSet, labelSet);
            if (Root != null)
            {
                Success = true;
                TruePredicate = GeneratePred(Root, true);
                FalsePredicate = GeneratePred(Root, false);
            }
        }
    }
}