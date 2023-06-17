using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using FileBee.utils;
using LibBee;
using LibBee.Annotations;
using LibBee.Model;
using System.Diagnostics;

namespace FileBee
{
    [TestClass]
    public class FileTest
    {
        // switch between bidirectional or forward-only
        private const bool Bidirectional = true;
        //private const bool Bidirectional = false;
        
        private const int ExpTimeout = 2 * 60 * 1000;
        
        private const string BenchmarkPath = @"../../../../Benchmarks/File/";
        private const string LogPath = @"../../../logs/";
        private static readonly List<string> TestResults = new();

        private static void RunBenchmark(string benchPath, string testName = null)
        {
            var config = Utils.ReadJson(benchPath + "/config.json");
            var constStrs = (string[]) Utils.ReadConstStrings(config).ToArray();
            var constInts = (int[]) Utils.ReadConstInts(config).ToArray();
            var constDates = (DateTime[]) Utils.ReadConstDates(config).ToArray();

            var fileDict = Benchmarks.ReadFiles(benchPath + "/input.csv");
            var actions = Benchmarks.ReadOperations(benchPath + "/actions");
            var actionRecorder = ActionRecorder.GetInstance();
            actionRecorder.Start();
            foreach (var action in actions)
            {
                switch (action[0])
                {
                    case "chmod":
                        FileOp.Chmod(fileDict[action[1]], action[2]);
                        break;
                    case "move":
                        FileOp.Move(fileDict[action[1]], action[2]);
                        break;
                    case "copy":
                        FileOp.Copy(fileDict[action[1]], action[2]);
                        break;
                    case "rename":
                        FileOp.Rename(fileDict[action[1]], action[2]);
                        break;
                    case "chext":
                        FileOp.Chext(fileDict[action[1]], action[2]);
                        break;
                    case "chgrp":
                        FileOp.Chgrp(fileDict[action[1]], action[2]);
                        break;
                    case "delete":
                        FileOp.Delete(fileDict[action[1]]);
                        break;
                    case "unzip":
                        FileOp.Unzip(fileDict[action[1]], action[2]);
                        break;
                    case "tar":
                    {
                        var splits = action[1].Split(',');
                        FileOp.Tar(splits.Select(x => fileDict[x]).ToArray() , action[2]);
                        break;
                    }
                }
            }
            actionRecorder.End();
            var fileEntities = fileDict.Values.Select(x => new FileEntity(x)).ToList();
            var dataParser = new DataParser<FileEntity>();
            var fileTable = dataParser.ParseAsTable(fileEntities);
            Console.WriteLine(fileTable);
            var commands = actionRecorder.RecordedActions;
            foreach (var command in commands)
            {
                Console.WriteLine(command);
            }
            
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var synthesizer = new Synthesizer(new List<BeeTable> { fileTable }, commands);
            var programs = synthesizer.Synthesize(120000, constStrs, constDates, constInts, true, Bidirectional);
            bool success = programs.Any();
            stopWatch.Stop();
            if (testName != null)
            {
                TestResults.Add(testName + "," + success + "," + stopWatch.ElapsedMilliseconds);
            }

            Assert.IsTrue(success);
        }

        [ClassCleanup]
        public static void LogResults()
        {
            Utils.LogResults(LogPath + "bee_files", Bidirectional, TestResults);
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void A00()
        {
            RunBenchmark(BenchmarkPath + "hades-f04/");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Hades01()
        {
            RunBenchmark(BenchmarkPath + "hades-f01/", "hades01");
        }

        [TestMethod]
        [Timeout(ExpTimeout)]
        public void Hades02()
        {
            RunBenchmark(BenchmarkPath + "hades-f02/", "hades02");
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
        public void Hades18()
        {
            RunBenchmark(BenchmarkPath + "hades-f18", "hades18");
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
