using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using LibProse.hades;
using LibProse.synthesis;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.Learning;
using Microsoft.ProgramSynthesis.Learning.Strategies.Deductive.RuleLearners;
using Microsoft.ProgramSynthesis.Rules;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis.Utils;
using Microsoft.ProgramSynthesis.VersionSpace;

namespace SprProse.synthesis
{
    using Cell = Tuple<Coord, string>;
    
    public class RelateWitnesses: DomainLearningLogic
    {
        public RelateWitnesses(Grammar grammar) : base(grammar) { }

        [WitnessFunction(nameof(RelateSemantics.FilterWith), 1)]
        ExampleSpec WitnessFilterWith_pred(GrammarRule rule, ExampleSpec spec)
        {
            Debug.WriteLine("Entering FilterWith_pred");
            var inputState = spec.ProvidedInputs[0];
            var output = spec.Examples[inputState];
            if (output is not FilterProgram filterProgram)
                return null;
            var rst = new Dictionary<State, object> {{inputState, filterProgram.MapCond}};
            return new ExampleSpec(rst);
        }
        
        [WitnessFunction(nameof(RelateSemantics.FilterWith), 2)]
        ExampleSpec WitnessFilterWith_seq(GrammarRule rule, ExampleSpec spec)
        {
            Debug.WriteLine("Entering FilterWith_seq");
            var inputState = spec.ProvidedInputs[0];
            var output = spec.Examples[inputState];
            if (output is not FilterProgram filterProgram)
                return null;
            var rst = new Dictionary<State, object> {{inputState, filterProgram.Seq}};
            return new ExampleSpec(rst);
        }
        
        [WitnessFunction(nameof(RelateSemantics.Associate), 0)]
        ExampleSpec WitnessAssociate_baseProgram(GrammarRule rule, ExampleSpec spec)
        {
            Debug.WriteLine("Entering Associate_baseProgram");
            var inputState = spec.ProvidedInputs[0];
            var output = spec.Examples[inputState];
            if (output is not AssocProgram assocProgram)
                return null;
            var rst = new Dictionary<State, object> {{inputState, assocProgram.Parent}};
            return new ExampleSpec(rst);
        }
        
        [WitnessFunction(nameof(RelateSemantics.Associate), 1)]
        ExampleSpec WitnessAssociate_relFunc1(GrammarRule rule, ExampleSpec spec)
        {
            Debug.WriteLine("Entering Associate_relFunc1");
            var inputState = spec.ProvidedInputs[0];
            var output = spec.Examples[inputState];
            if (output is not AssocProgram assocProgram)
                return null;
            var rst = new Dictionary<State, object> {{inputState, assocProgram.RelFuncIn}};
            return new ExampleSpec(rst);
        }
        
        [WitnessFunction(nameof(RelateSemantics.Associate), 2)]
        ExampleSpec WitnessAssociate_relFunc2(GrammarRule rule, ExampleSpec spec)
        {
            Debug.WriteLine("Entering Associate_relFunc2");
            var inputState = spec.ProvidedInputs[0];
            var output = spec.Examples[inputState];
            if (output is not AssocProgram assocProgram)
                return null;
            var rst = new Dictionary<State, object> {{inputState, assocProgram.RelFuncOut}};
            return new ExampleSpec(rst);
        }
        
        [RuleLearner(nameof(RelateSemantics.Cons))]
        internal Optional<ProgramSet> LearnPartition(SynthesisEngine engine, GrammarRule rule,
            LearningTask<ExampleSpec> task, CancellationToken cancelToken)
        {
            Debug.WriteLine("Entering Cons");
            var examples = task.Spec.Examples;
            var inputState = task.Spec.ProvidedInputs[0];
            if (inputState[rule.Grammar.InputSymbol] is not SprSheet inputSpr)
                return Optional<ProgramSet>.Nothing;
            if (examples[inputState] is not SprSheet outputSpr)
                return Optional<ProgramSet>.Nothing;
            if (inputSpr.NumRows == 0 && outputSpr.NumRows == 0)
                return Optional<ProgramSet>.Nothing;

            // bottom up search in the hades paper
            var componentPrograms = BottomUpSynthesis.Synthesize(inputSpr, outputSpr)?.ToList();
            if (componentPrograms == null || !componentPrograms.Any())
                return Optional<ProgramSet>.Nothing;
            
            var singleProgramSetList = new List<ProgramSet>();
            foreach (var componentProgram in componentPrograms)
            {
                var singleSpec = new Dictionary<State, object> {{inputState, componentProgram}};
                var singleTask = task.Clone(rule.Grammar.Symbols["componentProgram"], new ExampleSpec(singleSpec));
                var singleCandidates = engine.Learn(singleTask, cancelToken);
                if (!singleCandidates.IsEmpty)
                    singleProgramSetList.Add(singleCandidates);
            }
            
            var emptySpec = new Dictionary<State, object> {{inputState, new SprSheet(0, 0)}};
            var emptyTask = task.Clone(rule.Head, new ExampleSpec(emptySpec));
            var currentMulti = engine.Learn(emptyTask, cancelToken);
            for (var i = singleProgramSetList.Count - 1; i >= 0; --i)
            {
                var consRule = Grammar.Rule(nameof(RelateSemantics.Cons)) as NonterminalRule;
                //var nestedMulti = new DirectProgramSet(rule.LetBody, currentMulti.RealizedPrograms);
                currentMulti = ProgramSet.Join(consRule, singleProgramSetList[i], currentMulti);
            }
            
            return currentMulti.Some();
        }
    }
}