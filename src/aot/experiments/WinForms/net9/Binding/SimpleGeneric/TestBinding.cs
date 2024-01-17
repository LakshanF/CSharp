using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BindingPoc
{
    public class TestBinding<T>
    {
        PropertyInfo? _dataElement;
        T _t;
        public TestBinding(T t, PropertyInfo dataElement)
        {
            _t = t;
            _dataElement = dataElement;
        }

        public TProperty GetPropertyValue<TObject, TProperty>()
        {
            // Get the value of the property as an object
            object? value = _dataElement?.GetValue(_t, null);

            // Convert the value to the desired property type using Convert.ChangeType
            return (TProperty)Convert.ChangeType(value!, typeof(TProperty));
        }

    }
}
