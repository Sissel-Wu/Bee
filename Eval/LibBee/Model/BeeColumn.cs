using System;

namespace LibBee.Model
{
    public class BeeColumn
    {
        public Type ColumnType { get; }

        public string Name { get; }

        public string GetTypeNameInC()
        {
            return Utils.ConvertTypeName(ColumnType);
        }

        internal BeeColumn(Type type, string name)
        {
            ColumnType = type;
            Name = name;
        }
    }
}
