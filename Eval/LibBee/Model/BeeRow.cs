using System.Collections.Generic;

namespace LibBee.Model
{
    public class BeeRow
    {
        private readonly List<object> values;

        internal BeeRow(List<object> values)
        {
            this.values = values;
        }

        public int Length => values.Count;

        public object this[int index]
        {
            get => values[index];
            set => values[index] = value;
        }
    }
}
