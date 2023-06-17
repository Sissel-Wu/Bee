using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ProgramSynthesis.Utils.Interactive;

namespace SprProse.synthesis
{
    using Cell = Tuple<Coord, string>;
    using Mapping = IEnumerable<Tuple<Tuple<Coord, string>, Tuple<Coord, string>>>;
    
    public static class BottomUpSynthesis
    {
        private static CompProgram PickBestProgram(List<CompProgram> workList)
        {
            return workList.First();
        }

        private static HashSet<Coord> MergeOutCoordinates(IEnumerable<Mapping> mappings)
        {
            var rst = new HashSet<Coord>();
            foreach (var mapping in mappings)
            {
                foreach (var (_, (coord, _)) in mapping)
                {
                    rst.Add(coord);
                }
            }
            return rst;
        }

        private static bool IsNewCoverage(CompProgram newProgram, List<CompProgram> programs)
        {
            var newCoords = newProgram.Mapping.Select(x => x.Item2.Item1).ToList();
            foreach (var oldProgram in programs)
            {
                var oldCoords = oldProgram.Mapping.Select(x => x.Item2.Item1).ToList();
                
                if (newCoords.Count != oldCoords.Count) continue;
                if (oldCoords.SequenceEqual(newCoords)) return false;
            }

            return true;
        }
        
        private static bool IsComplete(ISet<Coord> foundCoords, SprSheet outputSpr)
        {
            foreach (var (coord, val) in outputSpr.EnumerateCells())
            {
                if (val == null || val.IsEmpty()) continue;
                if (!foundCoords.Contains(coord)) return false;
            }

            return true;
        }

        private static bool IsNewCoordCovered(CompProgram newProgram, ISet<Coord> foundCoordinates)
        {
            foreach (var (_, (coord, _)) in newProgram.Mapping)
            {
                if (!foundCoordinates.Contains(coord))
                    return true;
            }
            return false;
        }

        // Assemble the components of a program in a greedy manner
        private static IEnumerable<CompProgram> Assemble(ISet<Cell> uncovered, List<CompProgram> comps)
        {
            var minRemainedCount = int.MaxValue;
            var minRemainedSet = uncovered;
            CompProgram bestComp = null;

            if (!uncovered.Any() || uncovered.All(x => x.Item2 == null || x.Item2.IsEmpty()))
                return Array.Empty<CompProgram>();
                
            foreach (var comp in comps)
            {
                var newGenCells = comp.Mapping.Select(x => x.Item2).ToList();
                var remainedSet = new HashSet<Cell>();
                foreach (var cell in uncovered)
                {
                    var isCovered = newGenCells.Any(x => Equals(x.Item1, cell.Item1));
                    if (!isCovered)
                        remainedSet.Add(cell);
                }
                
                if (remainedSet.Count < minRemainedCount)
                {
                    minRemainedCount = remainedSet.Count;
                    minRemainedSet = remainedSet;
                    bestComp = comp;
                }
            }
            return new[] {bestComp}.Concat(Assemble(minRemainedSet, comps));
        }
        
        public static IEnumerable<CompProgram> Synthesize(SprSheet inputSpr, SprSheet outputSpr)
        {
            var workList = new List<CompProgram>();
            var comps = new List<CompProgram>();
            
            // Step1: Generate all possible filter programs
            var mapRule = new MapRule(inputSpr, outputSpr);
            var candidateMappings = mapRule.EnumerateCandidateMap().ToList();
            Console.WriteLine("mappings found: " + candidateMappings.Count);
            foreach (var (mapping, seq) in candidateMappings)
            {
                //Console.WriteLine(seq);
                var mappingL = mapping.ToList();
                var condGen = new CondGen(inputSpr, seq);
                var cond = condGen.SynthesizeMapCond(mappingL.Select(x=>x.Item1));
                if (cond != null)
                {
                    var filterProgram = new FilterProgram(mappingL, cond, seq);
                    workList.Add(filterProgram);
                    comps.Add(filterProgram);
                }
                //Console.WriteLine(mappingL.Count);
                //PrintMapping(mappingList);
            }
            Console.WriteLine("valid filter programs: " + comps.Count);
            
            // Step2: Enumerate associate programs until the whole mapping is covered
            var coveredCoord = MergeOutCoordinates(comps.Select(x=>x.Mapping));
            while (workList.Any())
            {
                var baseComp = PickBestProgram(workList);
                workList.Remove(baseComp);
                var assocGen = new AssocGen(inputSpr, outputSpr);
                foreach (var (relFunc1, relFunc2, mapping) in assocGen.EnumerateRelFunc(baseComp.Mapping))
                {
                    var assocProgram = new AssocProgram(baseComp, mapping, relFunc1, relFunc2);
                    if (IsNewCoverage(assocProgram, comps))
                    //if (IsNewCoordCovered(assocProgram, coveredCoord))
                    {
                        workList.Add(assocProgram);
                        comps.Add(assocProgram);
                        coveredCoord = MergeOutCoordinates(comps.Select(x=>x.Mapping));
                    }
                }
            }
            Console.WriteLine("total compPrograms: " + comps.Count);
            var complete = IsComplete(coveredCoord, outputSpr);
            if (complete)
            {
                Console.WriteLine("complete.");
                return Assemble(outputSpr.EnumerateCells().ToHashSet(), comps);
            }
            Console.WriteLine("no program.");
            return null;
        }
    }
}