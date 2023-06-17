using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ProgramSynthesis.Transformation.Text;
using Microsoft.ProgramSynthesis.Transformation.Text.Semantics;

namespace XMLProse
{
    public class XMLTree
    {
        public XMLTree Parent { get; set; }
        public bool IsNull { get; }
        public string Text { get; }
        public string Tag { get; } // "#comment" for comment node; "#text" for text node
        public List<XMLTree> Children { get; set; }
        public Dictionary<string, string> Attrs { get; }

        public XMLTree(bool isNull, string tag = "null", XMLTree parent = null, List<XMLTree> children = null, Dictionary<string, string> attrs = null, string text = null)
        {
            IsNull = isNull;
            Tag = tag;
            Parent = parent;
            Children = children ?? new List<XMLTree>();
            Attrs = attrs ?? new Dictionary<string, string>();

            // remove white strings
            Text = text?.Trim(); // Regex.Replace(text, @"\s+", "");
        }

        public string ToString(int indent)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < indent; ++i)
                builder.Append('*');
            if (Text != null)
            {
                builder.Append(Text).Append('\n');
            }
            else
            {
                builder.Append('<').Append(Tag).Append('>').Append('\n'); ;
            }
            foreach (var child in Children)
            {
                builder.Append(child.ToString(indent + 4));
            }

            return builder.ToString();
        }

        public override string ToString()
        {
            return ToString(0);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(XMLTree))
                return false;
            var other = (XMLTree)obj;

            if (IsNull && other.IsNull)
                return true;

            return Tag == other.Tag && Text == other.Text && Attrs.SequenceEqual(other.Attrs) && Children.SequenceEqual(other.Children);
        }

        public override int GetHashCode()
        {
            int hash = Tag.GetHashCode() * 31;

            return hash;
        }
    }
}
