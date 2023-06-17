using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibProse.hades
{
    public abstract class HDTPath <T, TP> 
        where T: HDTNode
        where TP: HDTPath<T, TP>
    {
        protected readonly List<T> Path;

        protected HDTPath()
        {
            Path = new List<T>();
        }

        protected HDTPath(IEnumerable<T> path)
        {
            Path = path.ToList();
        }
        
        public T Head()
        {
            return Path.First();
        }

        public abstract TP Tail();

        public T Last()
        {
            return Path.Any() ? Path[^1] : null;
        }

        public abstract TP Add(T node);

        public T At(int index)
        {
            if (index < 0 || index >= Path.Count)
                return null;
            return Path[index];
        }

        public bool Empty()
        {
            return !Path.Any();
        }

        public int Length()
        {
            return Path.Count;
        }
        
        public IEnumerable<T> GetIterator()
        {
            return Path;
        }

        public abstract TP Concat(TP follower);

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var node in Path)
            {
                sb.Append(node).Append('/');
            }
            sb.Remove(sb.Length-1, 1);

            return sb.ToString();
        }

        public override int GetHashCode()
        {
            return Path.Aggregate(233, (current, node) => current * 31 + node.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            return obj is HDTPath<T, TP> path && Path.SequenceEqual(path.Path);
        }
    }
}