using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibBee.Annotations
{
    class FixedSizeFieldsAttribute : Attribute
    {
        public int Size { get; }

        public FixedSizeFieldsAttribute()
        {
        }
    }
}
