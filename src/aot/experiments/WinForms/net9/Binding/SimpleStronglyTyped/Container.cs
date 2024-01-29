using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BindingPoc
{

    // We want to create a custom type converter for the Name property
    // that converts it to uppercase

    public class TestControl
    {
        public string? TestProperty { get; set; }
    }

    public class Container
    {
        TestBinding bindingA;
        TestBinding bindingB;
        public TestControl ControlA { get; set; }
        public TestControl ControlB { get; set; }
        public Container(Person dataElement1, Person dataElement2)
        {
            ControlA = new TestControl();
            ControlA.TestProperty = "ControlA";

            ControlB = new TestControl();
            ControlB.TestProperty = "ControlB";

            DisplayControlProperties("Initial Binding");

            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(Person));

            dataElement1.PropertyChanged += OnChanged;
            bindingA = new TestBinding(properties!, "Name", dataElement1);

            dataElement2.PropertyChanged += OnChanged;
            bindingB = new TestBinding(properties!, "Address", dataElement2);

        }

        private void OnChanged(object? sender, PropertyChangedEventArgs e)
        {
            DisplayControlProperties("Property Changes");
        }

        public void DisplayControlProperties(string stage)
        {
            Console.WriteLine(stage);
            Console.WriteLine("---------------------");
            Console.WriteLine("ControlA: " + (bindingA==null ? ControlA.TestProperty : bindingA.GetPropertyValue()));
            Console.WriteLine("ControlB: " + (bindingB==null ? ControlB.TestProperty : bindingB.GetPropertyValue()));
            Console.WriteLine("---------------------");
            Console.WriteLine();
        }

    }
}
