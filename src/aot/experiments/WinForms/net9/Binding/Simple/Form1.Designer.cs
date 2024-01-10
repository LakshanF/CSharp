namespace WinFormsApp1
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            button1 = new Button();
            button2 = new Button();

            textBox1 = new TextBox();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(310, 306);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 0;
            button1.Text = "ClickMe";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Location = new Point(410, 306);
            button2.Name = "button2";
            button2.Size = new Size(75, 23);
            button2.TabIndex = 0;
            button2.Text = "Next";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;

            // 
            // textBox1
            // 
            textBox1.Location = new Point(360, 177);
            textBox1.Name = "textBox1";
            textBox1.Text = "HelloWorld";
            textBox1.Size = new Size(100, 23);
            textBox1.TabIndex = 1;


            // Bind the Text property of the TextBox to the Name property of the person object
            // Specify the DataSourceUpdateMode option as OnPropertyChanged
            person = new Person();
            // Since the Name property is null for a default Person, textBox1's original Text property (HelloWorld) is going to be overwritten by the new binding
            textBox1.DataBindings.Add("Text", person, "Name", false, DataSourceUpdateMode.OnPropertyChanged);
            // The above adds an implicit Binding object with the parameters
            // textBox1.DataBindings.Add(new Binding("Text", person, "Name", false, DataSourceUpdateMode.OnPropertyChanged));


            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(textBox1);
            Controls.Add(button1);
            Controls.Add(button2);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private Button button2;
        private TextBox textBox1;

        private Person person;
    }
}
