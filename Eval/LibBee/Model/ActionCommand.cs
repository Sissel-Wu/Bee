using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LibBee.Model
{
    public class ActionCommand
    {
        public MethodBase Method { get; }

        public string OperationName { get; }

        public bool IsStatic { get; }

        public object Instance { get; }

        public List<object> Args { get; }

        public ActionCommand(object instance, MethodBase method, List<object> args)
        {
            IsStatic = method.IsStatic;
            Instance = instance;
            Method = method;
            OperationName = method.Name;
            Args = args;
        }

        public void Execute()
        {
            Method.Invoke(Instance, Args.ToArray());
        }

        public IList<KeyValuePair<string, Type>> GetParameters()
        {
            List<KeyValuePair<string, Type>> result = new ();
            if (!IsStatic)
            {
                result.Add(new KeyValuePair<string, Type>("this", Instance.GetType()));
            }
            foreach (var param in Method.GetParameters())
            {
                result.Add(new KeyValuePair<string, Type>(param.Name, param.ParameterType));
            }

            return result;
        }
        

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (Instance != null)
            {
                sb.Append(Instance).Append('.');
            }
            sb.Append(OperationName).Append('(');
            foreach (var arg in Args)
            {
                sb.Append(arg);
                sb.Append(',');
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append(')');

            return sb.ToString();
        }
    }
}
