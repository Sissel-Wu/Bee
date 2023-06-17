using LibBee;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SprBee.utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LibBee.Bottom;
using LibBee.Model;

namespace SprBee
{
    [TestClass]
    public class SprTest
    {
        // switch between bidirectional or forward-only
        private const bool Bidirectional = true;
        //private const bool Bidirectional = false;
        
        private const int ExpTimeout = 2 * 60 * 1000;
        
        private const string BenchmarkPath = "../../../../Benchmarks/Spreadsheet/";
        private const string LogPath = @"../../../logs/";
        private static readonly List<string> TestResults = new();

        private static void RunBenchmark(string benchPath, string testName = null)
        {
            var config = Utils.ReadJson(benchPath + "/config.json");
            var constStrs = (string[]) Utils.ReadConstStrings(config).ToArray();
            var constInts = (int[]) Utils.ReadConstInts(config).ToArray();
            var constDates = (DateTime[]) Utils.ReadConstDates(config).ToArray();
            Utils.GetBoolProperty(config, "is_int_content", out bool isIntContent);
            Utils.GetBoolProperty(config, "is_double_content", out bool isDoubleContent);
            Utils.GetBoolProperty(config, "is_int_row_head", out bool isIntRowHead);
            Utils.GetBoolProperty(config, "is_int_col_head", out bool isIntColHead);
            
            var inputCsvPath = benchPath + "/input.csv";
            var tablePair = (isIntRowHead, isIntColHead, isIntContent, isDoubleContent) switch
            {
                (false, false, false, false) => Benchmarks.ReadData<string, string, string>(inputCsvPath),
                (false, false, true, false) => Benchmarks.ReadData<string, string, int>(inputCsvPath),
                (false, false, false, true) => Benchmarks.ReadData<string, string, double>(inputCsvPath),
                (false, true, false, false) => Benchmarks.ReadData<string, int, string>(inputCsvPath),
                (false, true, true, false) => Benchmarks.ReadData<string, int, int>(inputCsvPath),
                (false, true, false, true) => Benchmarks.ReadData<string, int, double>(inputCsvPath),
                (true, false, false, false) => Benchmarks.ReadData<int, string, string>(inputCsvPath),
                (true, false, true, false) => Benchmarks.ReadData<int, string, int>(inputCsvPath),
                (true, false, false, true) => Benchmarks.ReadData<int, string, double>(inputCsvPath),
                (true, true, false, false) => Benchmarks.ReadData<int, int, string>(inputCsvPath),
                (true, true, true, false) => Benchmarks.ReadData<int, int, int>(inputCsvPath),
                (true, true, false, true) => Benchmarks.ReadData<int, int, double>(inputCsvPath),
                _ => throw new InvalidDataException("wrong type spec in " + benchPath + "/input.csv")
            };
            
            Console.WriteLine(tablePair.Item1);
            Console.WriteLine(tablePair.Item2);

            var actions = Benchmarks.ReadOperations(benchPath + "/actions");
            var actionRecorder = ActionRecorder.GetInstance();
            actionRecorder.Start();
            foreach (var action in actions)
            {
                if (action[0].ToLower() != "fill")
                    throw new InvalidDataException("invalid actions in " + benchPath);
                SprOp.Fill(action[1], int.Parse(action[2]), int.Parse(action[3]));
            }
            actionRecorder.End();
            var commands = actionRecorder.RecordedActions;
            foreach (var command in commands)
            {
                Console.WriteLine(command);
            }

            Interfaces.ConfigLinearExpr(true);
            Interfaces.ConfigSumExpr(true);
            Interfaces.ConfigDivExpr(true);
            Interfaces.ConfigModExpr(true);
            
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            
            // var tables = Interfaces.ReadSpreadsheetExampleTables(benchPath + "/input.csv", isIntContent, isDoubleContent, isIntRowHead, isIntColHead);
            // Console.WriteLine(Interfaces.TablesToStr(tables));
            // var actions = Interfaces.ReadSpreadsheetExampleOperations(benchPath + "/actions");
            // Console.WriteLine(Interfaces.ActionsToStr(actions));
            
            var synthesizer = new Synthesizer(new List<BeeTable> {tablePair.Item1, tablePair.Item2}, commands);
            var programs = synthesizer.Synthesize(120000, constStrs, constDates, constInts, false, Bidirectional);
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
            Utils.LogResults(LogPath + "bee_spreadsheet", Bidirectional, TestResults);
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void A00()
        {
            RunBenchmark(BenchmarkPath + "flashrelate-01/");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void FlashRelate1()
        {
            RunBenchmark(BenchmarkPath + "flashrelate-01/", "flashrelate-1");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Pldi1()
        {
            RunBenchmark(BenchmarkPath + "pldi11-01/", "pldi11-1");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Pldi2()
        {
            RunBenchmark(BenchmarkPath + "pldi11-02/", "pldi11-2");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Pldi3()
        {
            RunBenchmark(BenchmarkPath + "pldi11-03/", "pldi11-3");
            //Assert.Fail();
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Pldi4()
        {
            RunBenchmark(BenchmarkPath + "pldi11-04/", "pldi11-4");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Pldi5()
        {
            RunBenchmark(BenchmarkPath + "pldi11-05/", "pldi11-5");
        }
        
        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Pldi6()
        {
            RunBenchmark(BenchmarkPath + "pldi11-06/", "pldi11-6");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Forum01()
        {
            RunBenchmark(BenchmarkPath + "forum-01/", "forum-1");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Forum02()
        {
            RunBenchmark(BenchmarkPath + "forum-02/", "forum-2");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Forum03()
        {
            RunBenchmark(BenchmarkPath + "forum-03/", "forum-3");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Forum04()
        {
            RunBenchmark(BenchmarkPath + "forum-04/", "forum-4");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Forum05()
        {
            RunBenchmark(BenchmarkPath + "forum-05/", "forum-5");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Forum06()
        {
            RunBenchmark(BenchmarkPath + "forum-06/", "forum-6");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Forum07()
        {
            RunBenchmark(BenchmarkPath + "forum-07/", "forum-7");
        }
        
        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Forum07_1()
        {
            RunBenchmark(BenchmarkPath + "forum-07-1/", "forum-7-1");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Forum08()
        {
            RunBenchmark(BenchmarkPath + "forum-08/", "forum-8");
            //Assert.Fail();
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Forum09()
        {
            RunBenchmark(BenchmarkPath + "forum-09/", "forum-9");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Forum10()
        {
            RunBenchmark(BenchmarkPath + "forum-10/", "forum-10");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Forum11()
        {
            RunBenchmark(BenchmarkPath + "forum-11-1/", "forum-11");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Forum12()
        {
            RunBenchmark(BenchmarkPath + "forum-12/", "forum-12");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Forum13()
        {
            RunBenchmark(BenchmarkPath + "forum-13/", "forum-13");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Forum14()
        {
            RunBenchmark(BenchmarkPath + "forum-14/", "forum-14");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Forum15()
        {
            RunBenchmark(BenchmarkPath + "forum-15/", "forum-15");
        }
    }
}
