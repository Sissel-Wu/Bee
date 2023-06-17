using LibBee;
using LibBee.Annotations;
using LibBee.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using XMLBee.utils;

namespace XMLBee
{
    [TestClass]
    public class XmlTest
    {
        // switch between bidirectional or forward-only
        //private const bool Bidirectional = true;
        private const bool Bidirectional = false;
        
        private const int ExpTimeout = 2 * 60 * 1000;
        
        private const string BenchmarkPath = "../../../../Benchmarks/XML/";
        private const string LogPath = @"../../../logs/";
        private static readonly List<string> TestResults = new();

        private void RunBenchmark(string benchPath, string testName = null)
        {
            var config = Utils.ReadJson(benchPath + "/config.json");
            var constStrs = (string[]) Utils.ReadConstStrings(config).ToArray();
            var constInts = (int[]) Utils.ReadConstInts(config).ToArray();
            var constDates = (DateTime[]) Utils.ReadConstDates(config).ToArray();
            
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            //var (elems, attrs) = Benchmarks.ReadFromCsv(benchPath + "/elements.csv");
            var (elems, attrs) = Benchmarks.ReadFromXml(benchPath + "/input.xml");
            var actions = Benchmarks.ReadOperations(benchPath + "/actions");
            var actionRecorder = ActionRecorder.GetInstance();
            actionRecorder.Start();
            foreach (var action in actions)
            {
                switch (action[0])
                {
                    case "modifyText":
                        XmlActions.ModifyText(elems[action[1]], action[2]);
                        break;
                    case "deleteE":
                        XmlActions.DeleteElem(elems[action[1]]);
                        break;
                    case "modifyAttribute":
                        XmlActions.ModifyAttr(attrs[action[1]], action[2]);
                        break;
                    case "wrap":
                        XmlActions.Wrap(elems[action[1]], action[2]);
                        break;
                    case "modifyTag":
                        XmlActions.ModifyTag(elems[action[1]], action[2]);
                        break;
                    case "addElement":
                        XmlActions.AddElem(elems[action[1]], action[2], action[3]);
                        break;
                    case "addAttribute":
                        XmlActions.AddAttr(elems[action[1]], action[2], action[3]);
                        break;
                    case "addElementAbove":
                        XmlActions.AddElemAbove(elems[action[1]], action[2], action[3]);
                        break;
                    case "moveBelow":
                        XmlActions.MoveBelow(elems[action[1]], elems[action[2]]);
                        break;
                    case "appendChild":
                        XmlActions.AppendChild(elems[action[1]], elems[action[2]]);
                        break;
                }
            }
            actionRecorder.End();
            var elemEntities = elems.Values.ToList();
            var attrEntities = attrs.Values.ToList();

            var elemTable = new DataParser<ElementEntity>().ParseAsTable(elemEntities);
            var attrTable = new DataParser<AttributeEntity>().ParseAsTable(attrEntities);
            Console.WriteLine(elemTable);
            Console.WriteLine(attrTable);

            var commands = actionRecorder.RecordedActions;
            foreach (var command in commands)
            {
                Console.WriteLine(command);
            }
            var synthesizer = new Synthesizer(new List<BeeTable> { elemTable, attrTable }, commands);
            var programs = synthesizer.Synthesize(120000, constStrs, constDates, constInts, true, Bidirectional);
            bool success = programs.Any();
            
            stopWatch.Stop();
            if (testName != null)
            {
                TestResults.Add(testName + "," + success + "," + stopWatch.ElapsedMilliseconds);
            }
            
            Assert.IsTrue(success);
        }

        [AssemblyCleanup]
        public static void LogResults()
        {
            Utils.LogResults(LogPath + "bee_xml", Bidirectional, TestResults);
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void A00()
        {
            RunBenchmark(BenchmarkPath + "online-01/");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Online01()
        {
            RunBenchmark(BenchmarkPath + "online-01/", "online1");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Online02()
        {
            //RunBenchmark(BenchmarkPath + "online-02/", "online2");
            Assert.Fail();
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
        public void Online07()
        {
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
        public void Online09()
        {
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
        public void Online14()
        {
            //RunBenchmark(BenchmarkPath + "online-14/", "online14");
            Assert.Fail();
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Online15()
        {
            RunBenchmark(BenchmarkPath + "online-15-1/", "online15");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Online16()
        {
            RunBenchmark(BenchmarkPath + "online-16-1/", "online16");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Online17()
        {
            RunBenchmark(BenchmarkPath + "online-17-1/", "online17");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Online18()
        {
            RunBenchmark(BenchmarkPath + "online-18/", "online18");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Online19()
        {
            RunBenchmark(BenchmarkPath + "online-19/", "online19");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Online20()
        {
            RunBenchmark(BenchmarkPath + "online-20/", "online20");
        }
    }
}
