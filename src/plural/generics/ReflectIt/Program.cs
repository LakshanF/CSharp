using System;
using System.Reflection;
using System.Collections.Generic;

namespace ReflectIt
{
    class Program
    {
        static void Main(string[] args)
        {
            var openGenType = typeof(Dictionary<,>);
            var closeGenType = openGenType.MakeGenericType(typeof(Employee), typeof(int));
            var o = Activator.CreateInstance(closeGenType);
            Console.WriteLine(o.GetType().Name);
            Console.WriteLine(o.GetType().FullName);

            var t2 = typeof(Employee);
            var o2 = Activator.CreateInstance(t2);
            var m1 = t2.GetMethod("Speak");
            var m2 = m1.MakeGenericMethod(typeof(string));
            m2.Invoke(o2, null);
        }
    }

    public class Employee
    {
        public string Name { get; set; }

        public void Speak<T>()
        {
            Console.WriteLine(typeof(T).Name);
        }
    }
}
