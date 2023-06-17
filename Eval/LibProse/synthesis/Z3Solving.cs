using System;
using System.Collections.Generic;
using System.Linq;

namespace LibProse.synthesis
{
    using Microsoft.Z3;

    public static class Z3Solving
    {
        public static Model SolvePathConstraints(IEnumerable<IEnumerable<Tuple<int, int, int>>> tuples)
        {
            var list = tuples.ToList();
            if (!list.Any())
                return null;
            var arity = list[0].Count();
            var ctx = new Context();
            var solver = ctx.MkSolver();
            var heads = new List<BoolExpr> ();
            var biases = new List<IntExpr> ();
            var heads2 = new List<BoolExpr> ();
            var biases2 = new List<IntExpr> ();
            for (var i = 0; i < arity; ++i)
            {
                heads.Add(ctx.MkBoolConst("bb_" + i));
                biases.Add(ctx.MkIntConst("bc_" + i));
                heads2.Add(ctx.MkBoolConst("eb_" + i));
                biases2.Add(ctx.MkIntConst("ec_" + i));
            }
            foreach (var path in list)
            {
                var pathItems = path.ToList();
                for (var i = 0; i < arity; ++i)
                {
                    var pathLength = ctx.MkInt(pathItems[i].Item1);
                    var beginIdx = ctx.MkInt(pathItems[i].Item2);
                    var endIdx = ctx.MkInt(pathItems[i].Item3);
                    var beginConstraint = ctx.MkEq(ctx.MkITE(heads[i], pathLength + biases[i], biases[i]), beginIdx);
                    var endConstraint = ctx.MkEq(ctx.MkITE(heads2[i], pathLength + biases2[i], biases2[i]), endIdx);
                    solver.Add(beginConstraint);
                    solver.Add(endConstraint);
                }
            }
                        
            var checkRst = solver.Check();
            if (checkRst == Status.SATISFIABLE)
            {
                var model = solver.Model;
                Console.WriteLine(model.ToString());
                return model;
            }

            Console.WriteLine("unsat");
            return null;
        }

        public static Model SolvePathConstraints<T>(IEnumerable<IEnumerable<Tuple<int, int, int, T>>> examples)
        {
            var simplified = examples.Select(
                xs => xs.Select(
                    x => new Tuple<int, int, int>(x.Item1, x.Item2, x.Item3)));
            return SolvePathConstraints(simplified);
        }
    }
}