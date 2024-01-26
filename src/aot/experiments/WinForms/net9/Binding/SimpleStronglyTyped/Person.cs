using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BindingPoc
{
    // Define a custom class with a property
    public class Person : INotifyPropertyChanged
    {
        private NameFoo? _name;
        private AddressFoo? _address;

        [TypeConverter(typeof(NameConverter))]
        public NameFoo? Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged("Name"); // Raise the PropertyChanged event with the property name
            }
        }

        [TypeConverter(typeof(AddressConverter))]
        public AddressFoo? Address 
        {
            get => _address;
            set
            {
                _address = value;
                OnPropertyChanged("Address"); // Raise the PropertyChanged event with the property name
            }
        }

        // Declare the PropertyChanged event
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class NameFoo
    {
        public NameFoo(string s) { NameBar=s; }
        public string? NameBar { get; set; }
    }

    public class NameConverter : TypeConverter
    {
        // Override the CanConvertFrom method to indicate that we can convert from string
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            if (sourceType == typeof(NameFoo))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        // Override the ConvertFrom method to perform the conversion from string to uppercase
        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is NameFoo foo)
            {
                return foo.NameBar;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        {
            if (destinationType == typeof(NameFoo))
            {
                return true;
            }
            return base.CanConvertTo(context, destinationType);
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (destinationType == typeof(NameFoo) && value is Person person)
            {
                // Get the Color object
                return person.Name?.NameBar;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class AddressFoo
    {
        public AddressFoo(string s) { AddressBar=s; }
        public string? AddressBar { get; set; }
    }

    public class AddressConverter : TypeConverter
    {
        // Override the CanConvertFrom method to indicate that we can convert from string
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            if (sourceType == typeof(AddressFoo))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        // Override the ConvertFrom method to perform the conversion from string to uppercase
        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is AddressFoo foo)
            {
                return foo.AddressBar;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        {
            if (destinationType == typeof(AddressFoo))
            {
                return true;
            }
            return base.CanConvertTo(context, destinationType);
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (destinationType == typeof(AddressFoo) && value is Person person)
            {
                // Get the Color object
                return person.Address?.AddressBar;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }


}
