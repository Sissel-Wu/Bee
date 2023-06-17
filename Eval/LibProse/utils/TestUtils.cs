using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.AST;
using Microsoft.ProgramSynthesis.Compiler;
using Microsoft.ProgramSynthesis.Diagnostics;
using Microsoft.ProgramSynthesis.Learning;
using Microsoft.ProgramSynthesis.Learning.Strategies;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis.VersionSpace;

namespace LibProse.utils
{
    public static class TestUtils
    {
        public static void LogResults(string logPrefix, IEnumerable<string> testResults)
        {
            var now = DateTime.Now;
            var path = logPrefix + now.ToString("yyyy-MM-dd--HH-mm-ss") + ".txt";
            File.WriteAllLines(path, testResults);
            Debug.WriteLine("test results written to file");
        }

        public delegate DomainLearningLogic InitLearningLogic(Grammar grammar);
        
        private static SynthesisEngine ConfigureSynthesis(Grammar grammar, InitLearningLogic init)
        {
            var witnessFunctions = init(grammar);
            var deductiveSynthesis = new DeductiveSynthesis(witnessFunctions);
            var synthesisStrategies = new ISynthesisStrategy[] { deductiveSynthesis };
            var synthesisConfig = new SynthesisEngine.Config { Strategies = synthesisStrategies };
            var prose = new SynthesisEngine(grammar, synthesisConfig);
            return prose;
        }

        public static Grammar CompileGrammar(string grammarPath, TypeInfo semanticTypeInfo)
        {
            Result<Grammar> compileResult = DSLCompiler.Compile(new CompilerOptions
            {
                InputGrammarText = File.ReadAllText(grammarPath),
                References = CompilerReference.FromAssemblyFiles(semanticTypeInfo.Assembly)
            });

            if (compileResult.HasErrors)
            {
                Console.Error.WriteLine(compileResult.Diagnostics);
            }

            return compileResult.Value;
        }

        public static void TrySynthesize(Grammar grammar, 
            Spec spec, InitLearningLogic init,
            out ProgramNode synthesized, out long timeElapsed)
        {
            var proseEngine = ConfigureSynthesis(grammar, init);

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            ProgramSet learnedSet = proseEngine.LearnGrammar(spec);
            IEnumerable<ProgramNode> programs = learnedSet.RealizedPrograms;
            stopWatch.Stop();

            var programNodes = programs.ToList();
            if (programNodes.Count != 0)
            {
                synthesized = programNodes.First();
                Debug.WriteLine(synthesized.ToString());
            }
            else
            {
                synthesized = null;    
            }
            timeElapsed = stopWatch.ElapsedMilliseconds;
        }
    }
}
