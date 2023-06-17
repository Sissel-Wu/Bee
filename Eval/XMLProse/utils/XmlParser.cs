using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace XMLProse.utils
{
    public static class XmlParser
    {
        private static XMLTree Parse(System.Xml.XmlNode node, XMLTree parent)
        {
            if (node.Name == "#text")
            {
                return new XMLTree(false, "#text", parent, null, null, node.Value);
            }
            else if (node.Name == "#comment")
            {
                return new XMLTree(false, "#comment", parent, null, null, node.Value);
            }
            else
            {
                var current = new XMLTree(false, node.Name, parent);
                if (node.Attributes != null)
                {
                    for (int i = 0; i < node.Attributes.Count; ++i)
                    {
                        var attr = node.Attributes[i];
                        current.Attrs[attr.Name] = attr.Value;
                    }
                }

                var children = new List<XMLTree>();
                if (node.HasChildNodes)
                {
                    for (int i = 0; i < node.ChildNodes.Count; ++i)
                    {
                        var child = Parse(node.ChildNodes[i], current);
                        children.Add(child);
                    }
                }
                current.Children = children;
                return current;
            }
        }

        public static XMLTree Read(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            var root = doc.FirstChild;
            return Parse(root, null);
        }
    }
}
