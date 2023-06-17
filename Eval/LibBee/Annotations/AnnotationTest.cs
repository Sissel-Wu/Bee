using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace LibBee.Annotations
{
    [Entity]
    class Foo
    {
        string _name;
        int _count;

        public Foo(string name, int count)
        {
            this._name = name;
            this._count = count;
        }

        [Field]
        public string Name()
        {
            return _name;
        }

        [Field]
        public int Count()
        {
            return _count;
        }

        [Action]
        public void CallName(string called)
        {
            Console.WriteLine("calling name: " + called);
        }
    }

    //[TestClass]
    public class AnnotationTest
    {
        [TestMethod]
        public void TestField()
        {
            var foo = new Foo("Leo", 42);
            Console.WriteLine("new foo");
            var typeInfo = foo.GetType();

            var fields = new List<object>();
            var methods = new List<MethodInfo>();
            foreach (var method in typeInfo.GetMethods())
            {
                var temp = method.GetCustomAttributes(typeof(FieldAttribute), true);
                if (temp.Length > 0)
                {
                    fields.Add(temp[0]);
                    methods.Add(method);
                    Console.WriteLine(method.Name + " is a field.");
                }
            }

            foreach (var method in methods)
            {
                Console.WriteLine(method.ReturnType + " " + method.Name + ": " + method.Invoke(foo, null));
            }
        }

        [TestMethod]
        public void TestOperation()
        {
            var foo = new Foo("Leo", 42);
            Console.WriteLine("new foo");

            foo.CallName("Peter");
        }

        [TestMethod]
        public void TestParsingTable()
        {
            var foo1 = new Foo("Leo", 42);
            var foo2 = new Foo("Defy", 112);
            var foo3 = new Foo("Lucy", 212);

            var foos = new List<Foo> { foo1, foo2, foo3 };
            var parser = new DataParser<Foo>();
            Console.WriteLine("parser: " + parser);
            Console.WriteLine(parser.ParseAsTable(foos));
        }
    }
}
