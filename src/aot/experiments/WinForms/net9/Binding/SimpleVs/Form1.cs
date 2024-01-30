using System.Diagnostics.CodeAnalysis;
namespace SimpleBindingViaVS
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // @HACK - we need to preserve the data element binding which is not visible to the trimmer
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Contact))]
        private void button1_Click(object sender, EventArgs e)
        {
            contact = new Contact() { FirstName = "Lakshan", LastName = "Fernando" };
            contactBindingSource.DataSource = contact;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            contact.FirstName = "Casimir";
        }
    }
}
