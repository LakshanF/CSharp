using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBindingViaVS
{
    public class Contact:INotifyPropertyChanged
    {
        private string? _lastName;
        private string? _firstName;

        public string? LastName
        {
            get { return _lastName; }
            set
            {
                _lastName = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("LastName");
            }
        }
        public string? FirstName
        {
            get { return _firstName; }
            set
            {
                _firstName = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("FirstName");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
