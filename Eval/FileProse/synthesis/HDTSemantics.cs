using System;
using System.Collections.Generic;
using System.Linq;
using LibProse.hades;
using Microsoft.ProgramSynthesis.Utils;

namespace FileProse.synthesis
{
    using IOPair = Tuple<FilePath, FilePath>;
    public static class HDTSemantics
    {
        public static ISet<IOPair> Cons(IEnumerable<IOPair> singleProgram, ISet<IOPair> multiProgram)
        {
            var rst = new HashSet<IOPair>();
            rst.AddRange(singleProgram);
            rst.AddRange(multiProgram);
            return rst;
        }

        public static ISet<IOPair> EmptyProgram()
        {
            return new HashSet<IOPair>();
        }

        public static IOPair ApplyTransform(PathTerm<FileNode, FilePath> pathTerm, FilePath x)
        {
            return new Tuple<FilePath, FilePath>(x, pathTerm.Evaluate(x));
        }

        public static IEnumerable<FilePath> FilterPaths(IPred<FileNode, FilePath> pred, IEnumerable<FilePath> inputs)
        {
            return inputs.Where(pred.EvaluatePred);
        }
    }
}