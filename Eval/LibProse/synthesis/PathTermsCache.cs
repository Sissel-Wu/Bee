using System;
using System.Collections.Generic;
using LibProse.hades;

namespace LibProse.synthesis
{
    public class PathTermsCache<T, TP>
        where T : HDTNode
        where TP: HDTPath<T, TP>, new() 
    {
        private static PathTermsCache<T, TP> singleton;
        private Dictionary<ISet<Tuple<TP, TP>>, PathTerm<T, TP>> cache;

        private PathTermsCache()
        {
            cache = new Dictionary<ISet<Tuple<TP, TP>>, PathTerm<T, TP>>();
        }
        
        public static PathTermsCache<T, TP> GetInstance()
        {
            return singleton ??= new PathTermsCache<T, TP>();
        }
        
        public bool ContainsKey(ISet<Tuple<TP, TP>> examples)
        {
            return cache.ContainsKey(examples);
        }
        
        public PathTerm<T, TP> Get(ISet<Tuple<TP, TP>> examples)
        {
            return cache[examples];
        }
        
        public void Add(ISet<Tuple<TP, TP>> key, PathTerm<T, TP> value)
        {
            cache.Add(key, value);
        }
    }
}