using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using FileProse.synthesis;
using FileProse.utils;
using LibProse.hades;
using LibProse.utils;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis.Utils;

namespace FileProse
{
    [TestClass]
    public class HDTTest
    {
        private const int ExpTimeout = 2 * 60 * 1000;
        
        private const string GrammarPath = @"../../../synthesis/grammar/hdt.grammar";
        private const string BenchmarkPath = @"../../../../Benchmarks/File/";
        private const string LogPath = @"../../../logs/";
        private static readonly List<string> TestResults = new();

        private static void RunBenchmark(string benchPath, string testName = null)
        {
            var config = Utils.ReadJson(benchPath + "/config.json");
            var constStrs = Utils.ReadConstStrings(config);
            var constInts = Utils.ReadConstInts(config);
            var constDates = Utils.ReadConstDates(config);

            HadesBenchmarks.PrepareCustomizedComponents(constStrs, constInts, constDates,
                    out HDTWitnessFunctions.Mappers,
                    out HDTWitnessFunctions.Features);

            var grammar = TestUtils.CompileGrammar(GrammarPath, typeof(HDTSemantics).GetTypeInfo());
            HadesBenchmarks.GetCorrespondence(benchPath, out var inputs, out var correspondence);
            var examples = new Dictionary<State, object> {{State.CreateForLearning(grammar.InputSymbol, inputs), correspondence}};
            var spec = new ExampleSpec(examples);
            
            HDTWitnessFunctions.IdMap = new IdMap<FileNode, FilePath>(correspondence, HDTWitnessFunctions.Mappers);
            TestUtils.TrySynthesize(grammar, spec, g => new HDTWitnessFunctions(g),
                out var synthesizedProgram, out var timeElapsed);
            var pass = false;
            if (synthesizedProgram != null)
            {
                var input = State.CreateForExecution(grammar.InputSymbol, inputs.ToEnumerable());
                var output = synthesizedProgram.Invoke(input);
                pass = correspondence.SetEquals((ISet<Tuple<FilePath, FilePath>>)output);
            }
            else
            {
                Debug.WriteLine("no program found");
            }
            if (testName != null)
            {
                TestResults.Add(testName + "," + pass + "," + timeElapsed);
            }
            Assert.IsTrue(pass);
        }

        [ClassCleanup]
        public static void LogResults()
        {
            TestUtils.LogResults(LogPath + "prose_hdtFiles_", TestResults);
        }
        
        [TestMethod]
        public void TestSingle()
        {
            RunBenchmark( BenchmarkPath + "hades-f01");
        }
        
        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Hades01()
        {
            RunBenchmark(BenchmarkPath + "hades-f01/", "hades01");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Hades02() // cannot scale
        {
            Assert.Fail(); // for fast testing
            //RunBenchmark(BenchmarkPath + "hades-f02/", "hades02");
        }
        
        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Hades02Simplified()
        {
            RunBenchmark(BenchmarkPath + "hades-f02-simplified/", "hades02simplified");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Hades03()
        {
            RunBenchmark(BenchmarkPath + "hades-f03/", "hades03");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Hades04()
        {
            RunBenchmark(BenchmarkPath + "hades-f04/", "hades04");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Hades05()
        {
            RunBenchmark(BenchmarkPath + "hades-f05/", "hades05");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Hades06()
        {
            RunBenchmark(BenchmarkPath + "hades-f06/", "hades06");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Hades07()
        {
            RunBenchmark(BenchmarkPath + "hades-f07/", "hades07");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Hades08()
        {
            RunBenchmark(BenchmarkPath + "hades-f08/", "hades08");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Hades09()
        {
            RunBenchmark(BenchmarkPath + "hades-f09", "hades09");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Hades10()
        {
            RunBenchmark(BenchmarkPath + "hades-f10", "hades10");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Hades12()
        {
            RunBenchmark(BenchmarkPath + "hades-f12", "hades12");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Hades13()
        {
            RunBenchmark(BenchmarkPath + "hades-f13", "hades13");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Hades14()
        {
            RunBenchmark(BenchmarkPath + "hades-f14", "hades14");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Hades15()
        {
            RunBenchmark(BenchmarkPath + "hades-f15", "hades15");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Hades16()
        {
            RunBenchmark(BenchmarkPath + "hades-f16", "hades16");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Hades18() // cannot scale
        {
            Assert.Fail(); // for fast testing
            RunBenchmark(BenchmarkPath + "hades-f18", "hades18");
        }
        
        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Hades18Simplified()
        {
            RunBenchmark(BenchmarkPath + "hades-f18-simplified", "hades18simplified");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Hades19()
        {
            RunBenchmark(BenchmarkPath + "hades-f19", "hades19");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Hades20()
        {
            RunBenchmark(BenchmarkPath + "hades-f20", "hades20");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Hades21()
        {
            RunBenchmark(BenchmarkPath + "hades-f21", "hades21");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Hades22()
        {
            RunBenchmark(BenchmarkPath + "hades-f22", "hades22");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Hades23()
        {
            RunBenchmark(BenchmarkPath + "hades-f23", "hades23");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Hades24()
        {
            RunBenchmark(BenchmarkPath + "hades-f24", "hades24");
        }
    }
}
