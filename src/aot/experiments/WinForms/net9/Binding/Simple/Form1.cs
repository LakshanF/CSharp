using System.ComponentModel;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        // Define a custom class with a property
        public class Person : INotifyPropertyChanged // Implement the INotifyPropertyChanged interface
        {
            private string name; // Use a private field to store the name value

            public string Name
            {
                get { return name; }
                set
                {
                    name = value;
                    OnPropertyChanged("Name"); // Raise the PropertyChanged event with the property name
                }
            }

            // Declare the PropertyChanged event
            public event PropertyChangedEventHandler PropertyChanged;

            // Define a method to raise the event
            protected void OnPropertyChanged(string propertyName)
            {
                // Check if the event handler is not null
                if (PropertyChanged != null)
                {
                    // Invoke the event handler with the current instance and the property name
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            person.Name = "Alice";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            person.Name = "Bob";
        }

    }
}
