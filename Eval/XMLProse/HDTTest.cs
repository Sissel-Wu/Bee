using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using LibProse.hades;
using LibProse.utils;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XMLProse.synthesis;
using XMLProse.utils;

namespace XMLProse
{
    [TestClass]
    public class HadesTest
    {
        private const string GrammarPath = @"../../../synthesis/grammar/hdt.grammar";
        private const string LogPath = @"../../../logs/";
        private const string BenchmarkPath = @"../../../../Benchmarks/XML/";
        private static readonly List<string> TestResults = new ();
        
        private const int ExpTimeout = 2 * 60 * 1000;
        
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
            
            HDTWitnessFunctions.IdMap = new IdMap<XmlNode, XmlPath>(correspondence, HDTWitnessFunctions.Mappers);
            TestUtils.TrySynthesize(grammar, spec, g => new HDTWitnessFunctions(g),
                out var synthesizedProgram, out var timeElapsed);
            var pass = false;
            if (synthesizedProgram != null)
            {
                var input = State.CreateForExecution(grammar.InputSymbol, inputs.ToEnumerable());
                var output = synthesizedProgram.Invoke(input);
                pass = correspondence.SetEquals((ISet<Tuple<XmlPath, XmlPath>>)output);
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
            TestUtils.LogResults(LogPath + "prose_hdtXml_", TestResults);
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void A00()
        {
            RunBenchmark(BenchmarkPath + "online-01");
        }
        
        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Online01()
        {
            RunBenchmark(BenchmarkPath + "online-01", "online-1");
        }
        
        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Online02() // out of scope: many-to-one
        {
            Assert.Fail();
            RunBenchmark(BenchmarkPath + "online-02/", "online2");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Online03()
        {
            RunBenchmark(BenchmarkPath + "online-03/", "online3");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Online04()
        {
            RunBenchmark(BenchmarkPath + "online-04/", "online4");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Online05()
        {
            RunBenchmark(BenchmarkPath + "online-05/", "online5");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Online06()
        {
            RunBenchmark(BenchmarkPath + "online-06/", "online6");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Online07() // out of scope: against the mapper definition
        { 
            Assert.Fail();
            RunBenchmark(BenchmarkPath + "online-07/", "online7");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Online08()
        {
            RunBenchmark(BenchmarkPath + "online-08/", "online8");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Online09() // not in scope: path dependence
        {
            Assert.Fail();
            RunBenchmark(BenchmarkPath + "online-09/", "online9");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Online10()
        {
            RunBenchmark(BenchmarkPath + "online-10/", "online10");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Online11()
        {
            RunBenchmark(BenchmarkPath + "online-11/", "online11");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Online12()
        {
            RunBenchmark(BenchmarkPath + "online-12/", "online12");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Online13()
        {
            RunBenchmark(BenchmarkPath + "online-13/", "online13");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Online14() // not in scope: the output paths have no corresponding inputs
        {
            Assert.Fail();
            RunBenchmark(BenchmarkPath + "online-14/", "online14");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Online15() // not in scope: string transformation across elements, many to one mapping
        {
            Assert.Fail();
            RunBenchmark(BenchmarkPath + "online-15-1/", "online15");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Online16() // not in scope: has path dependence
        {
            Assert.Fail();
            RunBenchmark(BenchmarkPath + "online-16-1/", "online16");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Online17() // not in scope: many to one
        {
            Assert.Fail();
            RunBenchmark(BenchmarkPath + "online-17-1/", "test17");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Online18()
        {
            RunBenchmark(BenchmarkPath + "online-18/", "test18");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Online19()
        {
            RunBenchmark(BenchmarkPath + "online-19/", "test19");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Online20() // out of scope: paths have no corresponding inputs
        {
            Assert.Fail();
            RunBenchmark(BenchmarkPath + "online-20-1/", "test20");
        }
    }
}