using System;
using System.Collections.Generic;
using System.Linq;

namespace LibProse.hades
{
    public class IdMap<T, TP>
        where T: HDTNode
        where TP: HDTPath<T, TP>
    {
        public int MaxInLength { get; private set; }
        private List<Tuple<object, int>> list;

        // the dictionary splits the mapped range into blocks of size (maxInputLength * 2)
        // [0 - maxInputLength): original index
        // [maxInputLength * 2 - infinite): either mapper or constant value
        private void CalculateDictionary(IEnumerable<Tuple<TP, TP>> examples, IEnumerable<IMapper<T>> mappers)
        {
            list = new List<Tuple<object, int>>();
            var exampleList = examples.ToList();
            MaxInLength = exampleList.Select(x => x.Item1.Length()).Max();
            var newId = MaxInLength * 2;

            foreach (var mapper in mappers)
            {
                list.Add(new Tuple<object, int>(mapper, newId));
                newId += MaxInLength * 2;
            }

            newId = -MaxInLength * 2;
            foreach (var (_, p2) in exampleList)
            {
                foreach (var node in p2.GetIterator())
                {
                    if (list.Any(x => x.Item1.Equals(node.Key))) continue;
                    list.Add(new Tuple<object, int>(node.Key, newId));
                    newId -= MaxInLength * 2;
                }
            }
        }

        public IdMap(IEnumerable<Tuple<TP, TP>> examples, IEnumerable<IMapper<T>> mappers)
        {
            CalculateDictionary(examples, mappers);
        }

        public IEnumerable<Tuple<object, int>> GetIterator()
        {
            return list;
        }
    }
}