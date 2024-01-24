namespace SimpleBindingViaVS
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            contact = new Contact() { Email = "Laks@home.net", Name = "Lakshan" };
            contactBindingSource.DataSource = contact;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            contact.Name = "Casimir";
        }
    }
}
