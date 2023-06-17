using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibBee.Model;
using PostSharp.Aspects;
using PostSharp.Extensibility;

namespace LibBee.Annotations
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Method)]
    public class ActionAttribute : MethodInterceptionAspect
    {
        public override void OnInvoke(MethodInterceptionArgs methodArgs)
        {
            var recorder = ActionRecorder.GetInstance();
            if (recorder.IsRecording)
            {
                // get signature
                var method = methodArgs.Method;
                Console.WriteLine("intercepting " + method.Name);
                var parameters = method.GetParameters();
                Console.WriteLine("size of parameters: " + parameters.Count());
                foreach (var param in parameters)
                {
                    Console.WriteLine(param.ParameterType + " " + param.Name);
                }

                // get args
                var instance = methodArgs.Instance;
                if (methodArgs.Method.IsStatic)
                {
                    Console.WriteLine(methodArgs.Method.DeclaringType + " static");
                }
                else
                {
                    Console.WriteLine(instance.GetType().Name + " " + instance);
                }

                var arguments = new List<object>();
                foreach (var arg in methodArgs.Arguments)
                {
                    Type type = arg.GetType();
                    Console.WriteLine(type.Name + " " + arg);
                    arguments.Add(arg);
                }
                
                var command = new ActionCommand(instance, method, arguments);
                recorder.AddCommand(command);
            }
            else
            {
                methodArgs.Proceed();
            }
        }
    }

}
