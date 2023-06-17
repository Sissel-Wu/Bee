using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LibProse.hades;

namespace XMLProse
{
    public class XmlNode: HDTNode
    {
        public enum NodeType
        {
            Element,
            Comment,
            Text
        }

        // main id is Tag+Idx
        public string Tag { get; }
        public int Idx { get; }
        public string Text { get; }

        public NodeType Type { get; }

        public Dictionary<string, string> Attributes { get; }

        public XmlNode(string tag, int idx, NodeType type, Dictionary<string, string> attributes, string text=null): base(new Tuple<string, int>(tag, idx))
        {
            Tag = tag;
            Idx = idx;
            Type = type;
            Attributes = attributes;
            Text = text;
        }
        
        public XmlNode(): base() {}

        public XmlNode(string tag, int idx, NodeType type=NodeType.Element) : base(new Tuple<string, int>(tag, idx))
        {
            Tag = tag;
            Idx = idx;
            Type = type;
            Attributes = new();
        }

        public XmlNode(XmlNode toCopy) : base(new Tuple<string, int>(toCopy.Tag, toCopy.Idx))
        {
            Tag = toCopy.Tag;
            Idx = toCopy.Idx;
            Type = toCopy.Type;
            Attributes = new Dictionary<string, string>();
            foreach (var (k, v) in toCopy.Attributes)
            {
                Attributes[k] = v;
            }
        }

        public bool HasAttr(string key)
        {
            return Attributes.ContainsKey(key);
        }

        public string GetAttr(string key)
        {
            Attributes.TryGetValue(key, out var val);
            return val;
        }

        public void SetAttrValue(string key, string newValue)
        {
            Attributes[key] = newValue;
        }

        public override bool AllEquals(object obj)
        {
            if (obj is not XmlNode to) return false;
            return to.Tag == Tag 
                   && to.Idx == Idx 
                   && to.Type == Type
                   && to.Text == Text
                   && (to.Attributes == null && null == Attributes ||
                       to.Attributes != null && Attributes != null
                       && to.Attributes.Count == Attributes.Count 
                       && to.Attributes.All(x => x.Value == Attributes[x.Key]));
        }

        public override string ToString()
        {
            return $"<{Tag}|{Idx}>";
        }
    }
    
    public class DeletedNode: XmlNode
    {
        private DeletedNode(): base("#deleted", 0) { }
        private static DeletedNode instance;

        public static DeletedNode Get()
        {
            return instance ??= new DeletedNode();
        }
    }

    public class XmlPath: HDTPath<XmlNode, XmlPath>
    {
        public XmlPath(): base(new List<XmlNode>()) { }

        public XmlPath(IEnumerable<XmlNode> path): base(path) { }

        public override XmlPath Tail()
        {
            return new XmlPath(Path.TakeLast(Path.Count - 1));
        }

        public override XmlPath Add(XmlNode node)
        {
            var rst = new XmlPath(Path);
            rst.Path.Add(node);
            return rst;
        }

        public override XmlPath Concat(XmlPath follower)
        {
            return new XmlPath(Path.Concat(follower.Path));
        }

        public bool HasPrefix(string prefix)
        {
            return Path.Any(node => node.Tag == prefix);
        }
    }

    public class XmlHDT: HDTTree<XmlNode, XmlPath>
    {
        public XmlHDT(XmlNode node, IEnumerable<XmlHDT> children, bool isRoot): base(node, children, isRoot) { }
            
        public XmlHDT(IEnumerable<XmlPath> paths, bool isRoot=true): base(paths, isRoot)
        {
            // check if all the paths share the same head
            var head = Paths.First().Head();
            
            foreach (var path in Paths)
            {
                if (!path.Head().Equals(head))
                {
                    throw new InvalidOperationException("error in constructing HDT; different heads");
                }
            }
            ThisNode = new XmlNode(head);
            Children = new ();
            Dictionary<XmlNode, List<XmlPath>> tempDict = new();
            foreach (var path in Paths)
            {
                var tail = path.Tail();
                if (tail.Empty()) // this is a leaf node
                {
                    Debug.Assert(Paths.Count() == 1);
                    return;
                }
                
                var subHead = tail.Head();
                if (!tempDict.ContainsKey(subHead))
                {
                    tempDict.Add(subHead, new List<XmlPath>());
                }
                tempDict[subHead].Add(tail);
            }
            
            foreach (var kv in tempDict)
            {
                Children.Add(new XmlHDT(kv.Value, false));
            }
        }

        public XmlHDT(XMLTree xmlTree, bool isRoot=true, XmlPath prefix=null, int idx=0): base(isRoot)
        {
            var type = xmlTree.Tag switch
            {
                "#comment" => XmlNode.NodeType.Comment,
                "#text" => XmlNode.NodeType.Text,
                _ => XmlNode.NodeType.Element
            };
            ThisNode = new XmlNode(xmlTree.Tag, idx, type, xmlTree.Attrs, xmlTree.Text);

            prefix ??= new XmlPath(new List<XmlNode>());
            prefix = prefix.Add(ThisNode);
            var tempDict = new Dictionary<string, List<XMLTree>>();
            foreach (var child in xmlTree.Children)
            {
                if (!tempDict.ContainsKey(child.Tag))
                    tempDict[child.Tag] = new List<XMLTree>();
                tempDict[child.Tag].Add(child);
            }
            
            Children = new List<HDTTree<XmlNode, XmlPath>>();
            var thisPath = new XmlPath().Add(ThisNode);
            
            if (!tempDict.Any()) // ThisNode is leaf 
            {
                Paths = new List<XmlPath> { thisPath };
                return;
            }
            // This is internal node
            Paths = new List<XmlPath>();
            foreach (var (_, list) in tempDict)
            {
                for (var i = 0; i < list.Count; i++)
                {
                    var child = new XmlHDT(list[i], false, prefix, i);
                    Children.Add(child);
                    foreach (var subPath in child.Paths)
                    {
                        Paths.Add(thisPath.Concat(subPath));
                    }
                }
            }
        }
    }
}