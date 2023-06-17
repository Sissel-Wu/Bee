using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace LibProse.utils
{
    public static class Utils
    {
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
    }
}