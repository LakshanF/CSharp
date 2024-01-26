

using System;
using System.ComponentModel;
using System.Reflection;

namespace BindingPoc
{
    /// <summary>
    /// This is a simple binding demo. The Container class has 2 controls, TestControl's Name property, that are the same that are bound to 2 different instances of the same data elements, 
    /// Person class's Name property.
    /// The Binding class, TestBinding, takes the data element PropertyDescriptor and an instance of it as parameters.
    /// </summary>
    internal class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Simple Binding Demo!");
            
            Program p = new Program();
            p.SetupBinding();
            p.ChangePropertiesOfDataElements();
            
            Console.WriteLine("Done!");
        }

        Person? a;
        Person? b;
        private void SetupBinding()
        {
            a = new Person();
            b = new Person();
            Container container = new Container(a, b);
        }

        private void ChangePropertiesOfDataElements()
        {
            a!.Name = new NameFoo("A1");
            b!.Address = new AddressFoo("B1");
        }
    }
}
