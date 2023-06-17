using System;
using System.Collections.Generic;
using System.Linq;
using LibProse.hades;
using Microsoft.ProgramSynthesis.Utils;

namespace XMLProse.synthesis
{
    using IOPair = Tuple<XmlPath, XmlPath>;
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

        public static IOPair ApplyTransform(PathTerm<XmlNode, XmlPath> pathTerm, XmlPath x)
        {
            return new Tuple<XmlPath, XmlPath>(x, pathTerm.Evaluate(x));
        }

        public static IEnumerable<XmlPath> FilterPaths(IPred<XmlNode, XmlPath> pred, IEnumerable<XmlPath> inputs)
        {
            return inputs.Where(pred.EvaluatePred);
        }
    }
}