using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using LibProse.hades;
using LibProse.synthesis;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.Learning;
using Microsoft.ProgramSynthesis.Learning.Strategies.Deductive.RuleLearners;
using Microsoft.ProgramSynthesis.Rules;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis.Utils;
using Microsoft.ProgramSynthesis.VersionSpace;

namespace FileProse.synthesis
{
    using ExamplePair = Tuple<FilePath, FilePath>;
    
    public class HDTWitnessFunctions: DomainLearningLogic
    {
        public static IEnumerable<IMapper<FileNode>> Mappers;
        public static IEnumerable<IFeature<FileNode, FilePath>> Features;
        public static IdMap<FileNode, FilePath> IdMap;

        public static int MaxInputLength = 0;

        public HDTWitnessFunctions(Grammar grammar) : base(grammar) { }

        [RuleLearner(nameof(HDTSemantics.Cons))]
        internal Optional<ProgramSet> LearnPartition(SynthesisEngine engine, GrammarRule rule, 
            LearningTask<ExampleSpec> task, CancellationToken cancelToken)
        {
            Debug.WriteLine("Entering Cons");
            var examples = task.Spec.Examples;
            var inputState = task.Spec.ProvidedInputs[0];
            var examplePairs = examples[inputState] as ISet<ExamplePair>;
            Debug.Assert(examplePairs != null, nameof(examplePairs) + " != null");
            if (!examplePairs.Any()) // base case, should be synthesized by "EmptyProgram()"
                return Optional<ProgramSet>.Nothing;
            
            MaxInputLength = examplePairs.Select(x => x.Item1.Length()).Max();
            // split the search, first find the singleProgram, then delegate to a new learning task
            var singleProgramSetList = new List<ProgramSet>();
            foreach (var partition in CommonDeductions<FileNode, FilePath>.EnumeratePartitions(examplePairs, IdMap))
            {
                var complete = true;
                foreach (var set in partition)
                {
                    Debug.WriteLine("Learning single program:");
                    foreach (var tp in set) Debug.WriteLine(tp);
                    Debug.WriteLine("---");
                    
                    var singleSpec = new Dictionary<State, object> {{inputState, set}};
                    var singleTask = task.Clone(rule.Grammar.Symbols["singleProgram"], new ExampleSpec(singleSpec));
                    var singleCandidates = engine.Learn(singleTask, cancelToken);
                    if (singleCandidates.IsEmpty)
                    {
                        complete = false;
                        break;
                    }
                    var singleProgram = singleCandidates.RealizedPrograms.First();
                    Debug.WriteLine("learned single program: ");
                    Debug.WriteLine(singleProgram);
                    singleProgramSetList.Add(singleCandidates);
                }
                Debug.WriteLine("=========");
                if (complete) break;
                singleProgramSetList.Clear();
            }
            if (singleProgramSetList.Count == 0)
                return Optional<ProgramSet>.Nothing;
            
            var emptySpec = new Dictionary<State, object> {{inputState, new HashSet<ExamplePair>()}};
            var emptyTask = task.Clone(rule.Head, new ExampleSpec(emptySpec));
            var currentMulti = engine.Learn(emptyTask, cancelToken);
            for (var i = singleProgramSetList.Count - 1; i >= 0; --i)
            {
                var consRule = Grammar.Rule(nameof(HDTSemantics.Cons)) as NonterminalRule;
                //var nestedMulti = new DirectProgramSet(rule.LetBody, currentMulti.RealizedPrograms);
                currentMulti = ProgramSet.Join(consRule, singleProgramSetList[i], currentMulti);
            }

            return currentMulti.Some();
        }

        [WitnessFunction("LetTerm", 0)]
        public ExampleSpec WitnessLetTerm(GrammarRule rule, ExampleSpec spec)
        {
            Debug.WriteLine("Entering LetTerm");
            var inputState = spec.ProvidedInputs[0];
            var outputs = spec.Examples[inputState].ToEnumerable().Cast<ExamplePair>().ToList();
            foreach (var output in outputs)
            {
                Debug.WriteLine(output);
            }
            var term = CommonDeductions<FileNode, FilePath>.Unify(outputs.ToHashSet(), IdMap);
            if (term == null)
                return null;
            var rst = new Dictionary<State, object> {{inputState, term}};

            return new ExampleSpec(rst);
        }
        
        [WitnessFunction("MapTransform", 1)]
        public ExampleSpec WitnessMapTransform_FilteredPaths(GrammarRule rule, ExampleSpec spec)
        {
            Debug.WriteLine("Entering MapTransform->FilteredPaths");
            var inputState = spec.ProvidedInputs[0];
            var allInPaths = inputState[rule.Grammar.InputSymbol].ToEnumerable().Cast<FilePath>();
            var outputs = spec.Examples[inputState].ToEnumerable().Cast<ExamplePair>().ToList();
            // find all the in_paths that are in the outputs
            var filteredPaths = allInPaths
                .Where(inPath => outputs.Any(pair => pair.Item1.Equals(inPath))).ToList();
            var rst = new Dictionary<State, object> {{inputState, filteredPaths}};

            return new ExampleSpec(rst);
        }
        
        [WitnessFunction(nameof(HDTSemantics.FilterPaths), 0)]
        public ExampleSpec WitnessFilterPaths_Pred(GrammarRule rule, ExampleSpec spec)
        {
            Debug.WriteLine("Entering Predicate");
            var inputState = spec.ProvidedInputs[0];
            var allInPaths = inputState[rule.Grammar.InputSymbol].ToEnumerable().Cast<FilePath>().ToList();
            var outputs = spec.Examples[inputState].ToEnumerable().Cast<FilePath>().ToList();
            var labels = allInPaths.Select(path => outputs.Contains(path)).ToList();
            var decisionTree = new DecisionTree<FileNode, FilePath>(allInPaths, Features.ToList(), labels);
            decisionTree.Learn();
            if (decisionTree.Success)
            {
                var truePred = decisionTree.TruePredicate;
                // var falsePred = decisionTree.FalsePredicate;
                Debug.WriteLine(truePred);
                var rst = new Dictionary<State, object> {{inputState, truePred}};
                return new ExampleSpec(rst);
            }
            else
            {
                Debug.WriteLine("Learn pred fail");
                return null;
            }
        }
    }
}