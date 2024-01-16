

using System;
using System.ComponentModel;
using System.Reflection;

namespace BindingPoc
{
    /// <summary>
    /// This is a simple binding demo. The Container class has 2 controls, TestControl's Name property, that are the same that are bound to 2 different instances of the same data elements, 
    /// Person class's Name property.
    /// The Binding class, TestBinding, is a generic class that takes the data element and the control as parameters. It uses reflection to get the property values from the data element.
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

        Person a;
        Person b;
        private void SetupBinding()
        {
            a = new Person();
            b = new Person();
            Container container = new Container(a, b);
        }

        private void ChangePropertiesOfDataElements()
        {
            a.Name = "A1";
            b.Name = "B1";
        }
    }
}
