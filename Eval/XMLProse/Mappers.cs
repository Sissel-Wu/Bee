using System;
using System.Collections.Generic;
using LibProse.hades;

namespace XMLProse
{
    public class TextNode : IMapper<XmlNode>
    {
        private readonly string value;

        public TextNode(string value)
        {
            this.value = value;
        }

        public XmlNode Evaluate(XmlNode xmlNode)
        {
            return new XmlNode("#text", xmlNode.Idx, XmlNode.NodeType.Text, new Dictionary<string, string>(), value);
        }

        public bool CanMap(XmlNode from, XmlNode to)
        {
            return to.Type == XmlNode.NodeType.Text && to.Text == value;
        }

        public override string ToString()
        {
            return "Text(" + value + ")";
        }
    }
    
    public delegate string StrField(XmlNode node);
    public static class FieldMapper
    {
        private class GetField : IMapper<XmlNode>
        {
            private readonly StrField strField;
            private readonly string name;
            private readonly bool toText;

            public GetField(StrField strField, string name, bool toText=false)
            {
                this.strField = strField;
                this.name = name;
                this.toText = toText;
            }
            
            public XmlNode Evaluate(XmlNode node)
            {
                return toText ? new XmlNode("#text", node.Idx, XmlNode.NodeType.Text) 
                    : new XmlNode(strField.Invoke(node), node.Idx);
            }

            public bool CanMap(XmlNode from, XmlNode to)
            {
                return !toText && to.Key.Equals(strField.Invoke(from)) 
                       || toText && to.Type == XmlNode.NodeType.Text && to.Text == strField.Invoke(from);
            }

            public override string ToString()
            {
                return name;
            }
        }

        public static IMapper<XmlNode> Of(StrField strField, string name, bool toText=false)
        {
            return new GetField(strField, name, toText);
        }
    }
    
    public class NoChange : IMapper<XmlNode>
    {
        public XmlNode Evaluate(XmlNode xmlNode)
        {
            return xmlNode;
        }

        public bool CanMap(XmlNode from, XmlNode to)
        {
            return from.AllEquals(to);
        }

        public override string ToString()
        {
            return "none";
        }
    }

    public class Delete : IMapper<XmlNode>
    {
        public XmlNode Evaluate(XmlNode node)
        {
            return DeletedNode.Get();
        }

        public bool CanMap(XmlNode from, XmlNode to)
        {
            return Equals(to, DeletedNode.Get());
        }
        
        public override string ToString()
        {
            return "Delete";
        }
    }

    public class TagShift : IMapper<XmlNode>
    {
        private readonly string oldTag;
        private readonly string newTag;
        private readonly int shift;

        public TagShift(string oldTag, string newTag, int shift)
        {
            this.oldTag = oldTag;
            this.newTag = newTag;
            this.shift = shift;
        }

        public XmlNode Evaluate(XmlNode node)
        {
            if (node.Tag != oldTag) return DeletedNode.Get();
            var rst = new XmlNode(newTag, node.Idx + shift, node.Type);
            foreach (var (k, v) in node.Attributes)
            {
                rst.SetAttrValue(k, v);
            }
            return rst;
        }

        public bool CanMap(XmlNode from, XmlNode to)
        {
            return Evaluate(from).AllEquals(to);
        }
        
        public override string ToString()
        {
            return $"TagShift({oldTag},{newTag},{shift})";
        }
    }

    public class ReplaceAttrStr : IMapper<XmlNode>
    {
        private readonly string attrName;
        private readonly string origin;
        private readonly string replacement;

        public ReplaceAttrStr(string attrName, string origin, string replacement)
        {
            this.attrName = attrName;
            this.origin = origin;
            this.replacement = replacement;
        }

        public XmlNode Evaluate(XmlNode node)
        {
            if (!node.HasAttr(attrName)) return node; // no change
            var copied = new XmlNode(node);
            var oldValue = node.GetAttr(attrName);
            copied.SetAttrValue(attrName, oldValue.Replace(origin, replacement));
            return copied;
        }

        public bool CanMap(XmlNode from, XmlNode to)
        {
            return from.HasAttr(attrName) && to.HasAttr(attrName) && from.GetAttr(attrName) != to.GetAttr(attrName) && Evaluate(from).AllEquals(to);
        }
        
        public override string ToString()
        {
            return $"ReplaceAttrStr({attrName},{origin},{replacement})";
        }
    }

    public class AppendAttrStr : IMapper<XmlNode>
    {
        private readonly string attrName;
        private readonly string suffix;

        public AppendAttrStr(string attrName, string suffix)
        {
            this.attrName = attrName;
            this.suffix = suffix;
        }

        public XmlNode Evaluate(XmlNode node)
        {
            if (!node.HasAttr(attrName)) return node; // no change
            var copied = new XmlNode(node);
            var oldValue = node.GetAttr(attrName);
            copied.SetAttrValue(attrName, oldValue + suffix);
            return copied;
        }

        public bool CanMap(XmlNode @from, XmlNode to)
        {
            return from.HasAttr(attrName) && to.HasAttr(attrName) && from.GetAttr(attrName) != to.GetAttr(attrName) && Evaluate(from).AllEquals(to);
        }
        
        public override string ToString()
        {
            return $"AppendAttrStr({attrName},{suffix})";
        }
    }
}