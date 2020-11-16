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
            
            // Debugging why InvoiceService cannot be Resolved - IoCtests->Can_Resolve_Concrete_Type()
            // InvoiceService ctor requires implementations of IRepository<Customer> repository, ILogger logger
            // The first parm has a path via the container map, IRepository<>=>SQLRepository<>
            // SQLRepository<Customer> isn't in the map but can be done
            // BUT SQLRepository doesnt have a default ctor - it has a ctor that takes a ILogger which needs to be managed
            var openGenType2 = typeof(SQLRepositry<>);
            var closeGenType2 = openGenType2.MakeGenericType(typeof(Customer));
            var openGenType2ctor = closeGenType2.GetConstructors();
            // Creating a concrete implementation of ILogger ctor for SQLRepository
            var closed1 = Activator.CreateInstance(typeof(SQLServiceLogger));
            var o2 = Activator.CreateInstance(closeGenType2, closed1);
            Console.WriteLine(o2.GetType().Name);
            Console.WriteLine(o2.GetType().FullName);            

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
