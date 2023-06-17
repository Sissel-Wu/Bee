using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SprProse
{
    public class Entry
    {
        public List<string> Tuples { get; set; }

        public Entry()
        {
            Tuples = new();
        }

        public Entry(params string[] strs)
        {
            Tuples = new();
            foreach (var str in strs)
                Tuples.Add(str);
        }

        public void Add(string str)
        {
            Tuples.Add(str);
        }

        public override bool Equals(object obj)
        {
            if (typeof(Entry) != obj.GetType())
                return false;
            var oth = (Entry)obj;

            return Tuples.SequenceEqual(oth.Tuples);
        }

        public override int GetHashCode()
        {
            return 233 * Tuples.Count + Tuples[0].GetHashCode();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append('<');
            foreach (var str in Tuples)
                sb.Append(str);
            sb.Append('>');
            return sb.ToString();
        }
    }
}
