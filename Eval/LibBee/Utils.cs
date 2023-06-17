using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace LibBee
{
    internal static class Utils
    {
        public static string ConvertTypeName(Type type)
        {
            if (type == typeof(int))
            {
                return "Int";
            }
            else if (type == typeof(bool))
            {
                return "Bool";
            }
            else if (type == typeof(string))
            {
                return "String";
            }
            else if (type == typeof(double))
            {
                return "Double";
            }
            else if (type == typeof(DateTime))
            {
                return "Time";
            }
            else if (type.IsArray)
            {
                return "RefSet";
            }
            else 
            {
                return "Ref";
            }
        }
        
        public static dynamic ReadJson(string jsonPath)
        {
            using var r = new StreamReader(jsonPath);
            var json = r.ReadToEnd();
            dynamic items = JsonConvert.DeserializeObject(json);
            return items;
        }
        
        public static IEnumerable<string> ReadConstStrings(dynamic json)
        {
            var jArr = json["const strings"];
            var rst = new List<string>();
            if (jArr == null) return rst;
            foreach (var obj in jArr)
            {
                rst.Add(obj.ToString());
            }

            return rst;
        }
        
        public static IEnumerable<int> ReadConstInts(dynamic json)
        {
            var jArr = json["const ints"];
            var rst = new List<int>();
            if (jArr == null) return rst;
            foreach (var obj in jArr)
            {
                rst.Add((int)obj);
            }

            return rst;
        }
        
        public static IEnumerable<DateTime> ReadConstDates(dynamic json)
        {
            var jArr = json["const datetimes"];
            var rst = new List<DateTime>();
            if (jArr == null) return rst;
            foreach (var obj in jArr)
            {
                rst.Add(DateTime.Parse(obj.ToString()));
            }

            return rst;
        }

        public static bool GetBoolProperty(dynamic json, string key, out bool value)
        {
            var val = json[key];
            if (val == null)
            {
                value = false;
                return false;
            }

            value = val;
            return true;
        }

        public static void LogResults(string prefix, bool idBidirectional, IEnumerable<string> lines)
        {
            var now = DateTime.Now;
            var bi = idBidirectional ? "bidirectional" : "forward";
            var path = prefix + "_" + bi + "_" + now.ToString("yyyy-MM-dd--HH-mm-ss") + ".txt";
            File.WriteAllLines(path, lines);
        }
    }
}
