using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibBee.Annotations
{
    [AttributeUsage(AttributeTargets.Method)]
    public class FieldAttribute : Attribute
    {
        public FieldAttribute()
        {
            // Console.WriteLine("Field instantiated.");
        }
    }
}
