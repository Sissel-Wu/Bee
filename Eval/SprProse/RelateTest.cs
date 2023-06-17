using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using LibProse.utils;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis.Utils;
using SprProse.synthesis;
using SprProse.utils;

namespace SprProse
{
    [TestClass]
    public class RelateTest
    {
        private const string GrammarPath = @"../../../synthesis/grammar/relate.grammar";
        private const string BenchmarkPath = @"../../../../Benchmarks/Spreadsheet/";
        private const string LogPath = @"../../../logs/";
        private static readonly List<string> TestResults = new();
        
        [ClassCleanup]
        public static void LogResults()
        {
            TestUtils.LogResults(LogPath + "prose_relateSpr_", TestResults);
        }

        private static void ReadIO(string benchPath, out SprSheet inputSpr, out SprSheet outputSpr)
        {
            inputSpr = SprBenchmarks.Read(benchPath + "/in.xlsx");
            outputSpr = SprBenchmarks.Read(benchPath + "/out.xlsx");
        }
        
        private static void RunBenchmark(string benchPath, string testName = null)
        {
            var config = Utils.ReadJson(benchPath + "/config.json");
            var constStrs = Utils.ReadConstStrings(config);
            var constInts = Utils.ReadConstInts(config);
            var constDates = Utils.ReadConstDates(config);
            
            var grammar = TestUtils.CompileGrammar(GrammarPath, typeof(RelateSemantics).GetTypeInfo());
            ReadIO(benchPath, out var inputSpr, out var outputSpr);
            var examples = new Dictionary<State, object> {{State.CreateForLearning(grammar.InputSymbol, inputSpr), outputSpr}};
            var spec = new ExampleSpec(examples);
            TestUtils.TrySynthesize(grammar, spec, g => new RelateWitnesses(g),
                out var synthesizedProgram, out var timeElapsed);
            var pass = false;
            if (synthesizedProgram != null)
            {
                var input = State.CreateForExecution(grammar.InputSymbol, inputSpr);
                var output = (SprSheet) synthesizedProgram.Invoke(input);
                pass = outputSpr.Equals(output);
            }
            else
            {
                Console.WriteLine("no program found");
            }
            if (testName != null)
            {
                TestResults.Add(testName + "," + pass + "," + timeElapsed);
            }
            Assert.IsTrue(pass);
        }

        [TestMethod] // warm up
        public void Test00()
        {
            RunBenchmark(BenchmarkPath + "flashrelate-01");
        }

        [TestMethod]
        public void Test01()
        {
            RunBenchmark(BenchmarkPath + "flashrelate-01", "flashrelate-1");
        }

        [TestMethod]
        public void Test02()
        {
            RunBenchmark(BenchmarkPath + "pldi11-01", "pldi11-1");
        }

        [TestMethod]
        public void Test03()
        {
            RunBenchmark(BenchmarkPath + "pldi11-02", "pldi11-2");
        }

        [TestMethod]
        public void Test04()
        {
            RunBenchmark(BenchmarkPath + "pldi11-03", "pldi11-3");
        }

        [TestMethod]
        public void Test05()
        {
            RunBenchmark(BenchmarkPath + "pldi11-04", "pldi11-4");
        }

        [TestMethod]
        public void Test06()
        {
            RunBenchmark(BenchmarkPath + "pldi11-05", "pldi11-5");
        }
        
        [TestMethod]
        public void Test22()
        {
            RunBenchmark(BenchmarkPath + "pldi11-06", "pldi11-6");
        }

        [TestMethod]
        public void Test07()
        {
            RunBenchmark(BenchmarkPath + "forum-01", "forum-1");
        }

        [TestMethod]
        public void Test08()
        {
            RunBenchmark(BenchmarkPath + "forum-02", "forum-2");
        }

        [TestMethod]
        public void Test09()
        {
            RunBenchmark(BenchmarkPath + "forum-03", "forum-3");
        }

        [TestMethod]
        public void Test10()
        {
            RunBenchmark(BenchmarkPath + "forum-04", "forum-4");
        }

        [TestMethod]
        public void Test11()
        {
            RunBenchmark(BenchmarkPath + "forum-05", "forum-5");
        }

        [TestMethod]
        public void Test12()
        {
            RunBenchmark(BenchmarkPath + "forum-06", "forum-6");
        }

        [TestMethod]
        public void Test13()
        {
            RunBenchmark(BenchmarkPath + "forum-07", "forum-7");
        }

        [TestMethod]
        public void Test14()
        {
            RunBenchmark(BenchmarkPath + "forum-08", "forum-8");
        }

        [TestMethod]
        public void Test15()
        {
            RunBenchmark(BenchmarkPath + "forum-09", "forum-9");
        }

        [TestMethod]
        public void Test16()
        {
            RunBenchmark(BenchmarkPath + "forum-10", "forum-10");
        }

        [TestMethod]
        public void Test17()
        {
            RunBenchmark(BenchmarkPath + "forum-11", "forum-11");
        }

        [TestMethod]
        public void Test18()
        {
            RunBenchmark(BenchmarkPath + "forum-12", "forum-12");
        }

        [TestMethod]
        public void Test19()
        {
            RunBenchmark(BenchmarkPath + "forum-13", "forum-13");
        }

        [TestMethod]
        public void Test20()
        {
            RunBenchmark(BenchmarkPath + "forum-14", "forum-14");
        }

        [TestMethod]
        public void Test21()
        {
            RunBenchmark(BenchmarkPath + "forum-15", "forum-15");
        }
    }
}