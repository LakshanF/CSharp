using System.ComponentModel;
using System.Windows.Forms;

namespace WinFormsApp2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            people.Clear();
            people.Add(new Person() { Name = "Alice" });
            people.Add(new Person() { Name = "Bob" });
            //people.Add(new Person() { Name = "Charlie" });
            bindingSource.ResetBindings(false);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            people.Clear();
            people.Add(new Person() { Name = "Sup" });
            people.Add(new Person() { Name = "Shyam" });
            people.Add(new Person() { Name = "Raj" });
            bindingSource.ResetBindings(false);
        }
    }

    public class Person : INotifyPropertyChanged // Implement the INotifyPropertyChanged interface
    {
        private string name; // Use a private field to store the name value
        private int age;

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged("Name"); // Raise the PropertyChanged event with the property name
            }
        }

        public int Age
        {
            get { return age; }
            set
            {
                age = value;
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

}
