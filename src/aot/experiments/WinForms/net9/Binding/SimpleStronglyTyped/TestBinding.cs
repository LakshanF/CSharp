using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BindingPoc
{
    public class TestBinding
    {
        PropertyDescriptorCollection? _properties;
        string? _name;
        object? _t;
        public TestBinding(PropertyDescriptorCollection properties, String name, object t)
        {
            _t = t;
            _name = name;
            _properties = properties;
        }

        public object GetPropertyValue()
        {
            PropertyDescriptor? descriptor = _properties!.Find(_name!, false);
            Type type = descriptor!.PropertyType;
            return descriptor!.Converter.ConvertTo(_t, type)!;
        }

    }
}
