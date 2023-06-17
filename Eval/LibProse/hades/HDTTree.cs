using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace LibProse.hades
{
    public class HDTTree<T, TP> 
        where T: HDTNode
        where TP: HDTPath<T, TP>
    {
        protected T ThisNode { get; set; }
        protected List<HDTTree<T, TP>> Children { get; set; }
        protected List<TP> Paths { get; set; }
        protected bool IsRoot { get; }

        protected HDTTree(bool isRoot)
        {
            IsRoot = isRoot;
        }

        protected HDTTree(T node, IEnumerable<HDTTree<T, TP>> children, bool isRoot)
        {
            ThisNode = node;
            Children = children.ToList();
            IsRoot = isRoot;
        }

        protected HDTTree(IEnumerable<TP> paths, bool isRoot)
        {
            // check if all the paths share the same head
            IsRoot = isRoot;
            var hdtPaths = paths.ToList();
            Paths = hdtPaths;
            
            Debug.Assert(hdtPaths.Any());
            var head = hdtPaths.First().Head();
            Debug.Assert(hdtPaths.All(x => x.Head().Equals(head)));
        }

        private string ToString(int numIndent)
        {
            StringBuilder sb = new();
            for (int i = 0; i < numIndent; ++i)
            {
                sb.Append(' ');
            }

            sb.Append(ThisNode.Key).Append('\n');
            foreach (var child in Children)
            {
                sb.Append(child.ToString(numIndent + 2));
            }

            return sb.ToString();
        }

        public override string ToString()
        {
            return ToString(0);
        }

        public IEnumerable<TP> SplitIntoPaths()
        {
            if (Paths != null)
                return Paths;
            else
                throw new NotImplementedException();
        }
    }
}