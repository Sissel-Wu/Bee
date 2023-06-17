using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using LumenWorks.Framework.IO.Csv;

namespace XMLBee.utils
{
    internal class XmlWalker
    {
        private int elementCount;
        private int attributeCount;
        private Dictionary<XmlNode, string> idTable;
        public Dictionary<string, ElementEntity> ElementDict { get; }
        public Dictionary<string, AttributeEntity> AttributeDict { get; }
        private readonly ElementEntity nullElem;

        public XmlWalker(XmlNode node)
        {
            elementCount = 0;
            attributeCount = 0;
            idTable = new Dictionary<XmlNode, string>();
            ElementDict = new Dictionary<string, ElementEntity>();
            AttributeDict = new Dictionary<string, AttributeEntity>();
            nullElem = new ElementEntity("-1", "NULL", "");
            Walk(node);
        }
        
        private void Walk(XmlNode node)
        {
            if (node.NodeType == XmlNodeType.Text) return;
            var elemId = "e" + elementCount++;
            string text;
            if (node.NodeType == XmlNodeType.Comment)
            {
                text = node.Value;
            }
            else
            {
                text = "";
                if (node.HasChildNodes)
                {
                    for (var i = 0; i < node.ChildNodes.Count; ++i)
                    {
                        if (node.ChildNodes[i]!.NodeType != XmlNodeType.Text) continue;
                        text = node.ChildNodes[i].Value;
                        break;
                    }
                }
            }

            var parent = ElementDict.Any() ? ElementDict[idTable[node.ParentNode!]] : nullElem;
            var thisElem = new ElementEntity(elemId, node.Name, text);
            idTable.Add(node, elemId);
            ElementDict.Add(elemId, thisElem);
            thisElem.SetParent(parent);
            var prev = nullElem;
            if (node.PreviousSibling != null)
            {
                prev = ElementDict[idTable[node.PreviousSibling]];
                prev.SetNext(thisElem); // modify the next-sibling of prev to be this element
            }
            thisElem.SetPrev(prev);
            thisElem.SetNext(nullElem);
                
            // setup attributes
            if (node.NodeType != XmlNodeType.Element) return;
            for (var i = 0; i < node.Attributes!.Count; ++i)
            {
                var attr = node.Attributes[i];
                var attrId = "a" + attributeCount++;
                AttributeDict.Add(attrId, new AttributeEntity(attrId, ElementDict[elemId], attr.Name, attr.Value));
            }
            if (!node.HasChildNodes) return;
            for (var i = 0; i < node.ChildNodes.Count; ++i)
            {
                Walk(node.ChildNodes[i]);
            }
        }
    }
    
    public static class Benchmarks
    {
        public static Tuple<Dictionary<string, ElementEntity>, Dictionary<string, AttributeEntity>> ReadFromXml(string path)
        {
            var doc = new XmlDocument();
            doc.Load(path);

            var root = doc.FirstChild;
            var walker = new XmlWalker(root);
            return new Tuple<Dictionary<string, ElementEntity>, Dictionary<string, AttributeEntity>>(walker.ElementDict, walker.AttributeDict);
        }
        
        public static Tuple<Dictionary<string, ElementEntity>, Dictionary<string, AttributeEntity>> ReadFromCsv(string path)
        {
            var elems = new Dictionary<string, ElementEntity>();
            var attrs = new Dictionary<string, AttributeEntity>();

            var elemsTable = new DataTable();
            using (var csvReader = new CsvReader(new StreamReader(File.OpenRead(path)), false))
            {
                elemsTable.Load(csvReader);
            }
            var nullElem = new ElementEntity("-1", "NULL", "");
            for (var i = 0; i < elemsTable.Rows.Count; ++i)
            {
                var id = elemsTable.Rows[i][0] as string;
                var tag = elemsTable.Rows[i][4] as string;
                var text = elemsTable.Rows[i][5] as string;
                elems[id] = new ElementEntity(id, tag, text ?? "");
            }
            for (var i = 0; i < elemsTable.Rows.Count; ++i)
            {
                var id = elemsTable.Rows[i][0] as string;
                var parent_id = elemsTable.Rows[i][1] as string;
                var prev_id = elemsTable.Rows[i][2] as string;
                var next_id = elemsTable.Rows[i][3] as string;
                elems[id].SetParent(parent_id == "-1" ? nullElem : elems[parent_id]);
                elems[id].SetPrev(prev_id == "-1" ? nullElem : elems[prev_id]);
                elems[id].SetNext(next_id == "-1" ? nullElem : elems[next_id]);
            }

            var attrsTable = new DataTable();
            using (var csvReader = new CsvReader(new StreamReader(File.OpenRead(path + "/attributes.csv")), false))
            {
                attrsTable.Load(csvReader);
            }
            for (var i = 0; i < attrsTable.Rows.Count; ++i)
            {
                var id = attrsTable.Rows[i][0] as string;
                var master_id = attrsTable.Rows[i][1] as string;
                var key = attrsTable.Rows[i][2] as string;
                var val = attrsTable.Rows[i][3] as string;
                attrs[id] = new AttributeEntity(id, elems[master_id], key, val ?? "");
            }

            return new Tuple<Dictionary<string, ElementEntity>, Dictionary<string, AttributeEntity>>(elems, attrs);
        }

        public static List<string[]> ReadOperations(string path)
        {
            var lines = File.ReadAllLines(path);
            var rst = new List<string[]>();
            foreach (var line in lines)
            {
                rst.Add(line.Split(' '));
            }

            return rst;
        }
    }
}
