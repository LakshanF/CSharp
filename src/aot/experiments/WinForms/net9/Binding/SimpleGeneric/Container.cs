using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BindingPoc
{
    public class TestControl
    {
        public string TestProperty { get; set; }
    }

    public class Container
    {
        TestBinding<Person> bindingA;
        TestBinding<Person> bindingB;
        public TestControl ControlA { get; set; }
        public TestControl ControlB { get; set; }
        public Container(Person dataElement1, Person dataElement2)
        {
            ControlA = new TestControl();
            ControlA.TestProperty = "ControlA";

            ControlB = new TestControl();
            ControlB.TestProperty = "ControlB";

            DisplayControlProperties("Initial Binding");

            dataElement1.PropertyChanged += OnChanged;

            PropertyInfo controlAProperty = typeof(Container).GetProperty("ControlA");
            PropertyInfo dataElementAProperty = typeof(Person).GetProperty("Name");
            bindingA = new TestBinding<Person>(dataElement1, dataElementAProperty);

            dataElement2.PropertyChanged += OnChanged;

            PropertyInfo controlBProperty = typeof(Container).GetProperty("ControlB");
            PropertyInfo dataElementBProperty = typeof(Person).GetProperty("Name");
            bindingB = new TestBinding<Person>(dataElement2, dataElementBProperty);

        }

        private void OnChanged(object sender, PropertyChangedEventArgs e)
        {
            DisplayControlProperties("Property Changes");
        }

        public void DisplayControlProperties(string stage)
        {
            Console.WriteLine(stage);
            Console.WriteLine("---------------------");
            Console.WriteLine("ControlA: " + (bindingA==null? ControlA.TestProperty: bindingA.GetPropertyValue<Person, string>()));
            Console.WriteLine("ControlB: " + (bindingB==null ? ControlB.TestProperty : bindingB.GetPropertyValue<Person, string>()));
            Console.WriteLine("---------------------");
            Console.WriteLine();
        }

    }
}
