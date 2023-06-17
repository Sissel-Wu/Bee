using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LibBee.Model;

namespace LibBee.Annotations
{
    public class DataParser<T>
    {
        readonly List<MethodInfo> fieldMethods;
        readonly List<MethodInfo> arrMethods;
        readonly List<BeeColumn> columnsInfo;

        public string EntityName
        {
            get;
            private set;
        }

        public DataParser()
        {
            fieldMethods = new();
            columnsInfo = new();
            arrMethods = new();

            var type = typeof(T);
            EntityName = type.Name;
            foreach (var method in type.GetMethods())
            {
                var temp = method.GetCustomAttributes(typeof(FieldAttribute), true);
                if (temp.Length > 0)
                {
                    fieldMethods.Add(method);
                    Console.WriteLine(method.Name + " is a field.");
                }
                else
                {
                    temp = method.GetCustomAttributes(typeof(FixedSizeFieldsAttribute), true);
                    if (temp.Length > 0)
                    {
                        arrMethods.Add(method);
                        Console.WriteLine(method.Name + " is a fixed-size list of fields.");
                    }
                }
            }

            foreach (var method in fieldMethods)
            {
                columnsInfo.Add(new BeeColumn(method.ReturnType, method.Name));
            }
        }

        public BeeTable ParseAsTable(IEnumerable<T> entities)
        {
            var rows = new List<BeeRow>();
            var arrSizes = new Dictionary<string, int>();

            foreach (var entity in entities)
            {
                var values = new List<object>();
                foreach (var fieldMethod in fieldMethods)
                {
                    values.Add(fieldMethod.Invoke(entity, null));
                }

                foreach (var arrMethod in arrMethods)
                {
                    var colName = arrMethod.Name;
                    var arrValue = arrMethod.Invoke(entity, null);
                    int fixedSize = -1;
                    if (arrSizes.ContainsKey(colName))
                    {
                        fixedSize = arrSizes[colName];
                    }
                    else
                    {
                        if (arrValue.GetType() == typeof(string[]))
                        {
                            var strs = arrValue as string[];
                            for (int i = 0; i < strs.Length; ++i)
                            {
                                columnsInfo.Add(new BeeColumn(typeof(string), colName + (i + 1)));
                            }
                            arrSizes[colName] = strs.Length;
                            fixedSize = strs.Length;
                        }
                    }

                    if (arrValue.GetType() == typeof(string[]))
                    {
                        var strs = arrValue as string[];
                        if (strs.Length != fixedSize)
                        {
                            throw new InvalidOperationException("the sizes of " + colName + " are different.");
                        }
                        foreach (var str in strs)
                        {
                            values.Add(str);
                        }
                    }
                }

                rows.Add(new BeeRow(values));
            }

            return new BeeTable(columnsInfo, rows);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(EntityName).Append('(');
            foreach (var columnInfo in columnsInfo)
            {
                sb.Append(columnInfo.ColumnType.Name)
                  .Append(' ')
                  .Append(columnInfo.Name)
                  .Append(','); 
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append(')');

            return sb.ToString();
        }
    }
}
