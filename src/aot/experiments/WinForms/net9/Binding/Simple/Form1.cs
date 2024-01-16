using System.ComponentModel;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            person.LaksName = "Alice";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            person.LaksName = "Bob";
        }

    }

    // Define a custom class with a property
    public class Person : INotifyPropertyChanged // Implement the INotifyPropertyChanged interface
    {
        private string laks_name; // Use a private field to store the name value

        public string LaksName
        {
            get { return laks_name; }
            set
            {
                laks_name = value;
                OnPropertyChanged("LaksName"); // Raise the PropertyChanged event with the property name
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


    public class MyBindableComponent : BindableComponent
    {
        private object dataSource;
        private string dataMember;

        // Use the new keyword to hide the base class implementation
        public new object DataSource
        {
            get
            {
                return dataSource;
            }
            set
            {
                dataSource = value;
                // Add your own logic for data binding here
            }
        }

        // Use the new keyword to hide the base class implementation
        public new string DataMember
        {
            get
            {
                return dataMember;
            }
            set
            {
                dataMember = value;
                // Add your own logic for data binding here
            }
        }

        //private BindingContext bindingContext;
        //private ControlBindingsCollection dataBindings;

        //public new BindingContext BindingContext
        //{
        //    get
        //    {
        //        if (bindingContext == null)
        //            bindingContext = new BindingContext();
        //        return bindingContext;
        //    }
        //    set
        //    {
        //        bindingContext = value;
        //    }
        //}

        //public new ControlBindingsCollection DataBindings
        //{
        //    get
        //    {
        //        if (dataBindings == null)
        //            dataBindings = new ControlBindingsCollection(this);
        //        return dataBindings;
        //    }
        //}
    }
}
