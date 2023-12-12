// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;

namespace ScratchProject;

// This project is meant for temporary testing and experimenting and should be kept as simple as possible.

internal static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

        // Create a Windows Form with several controls that demonstrate simple data binding.
        DataSet dataSet = new DataSet();
        DataTable customersTable = new DataTable("Customers");
        customersTable.Columns.Add("CustomerID", typeof(int));
        customersTable.Columns.Add("CustomerName", typeof(string));
        dataSet.Tables.Add(customersTable);

        DataTable ordersTable = new DataTable("Orders");
        ordersTable.Columns.Add("OrderID", typeof(int));
        ordersTable.Columns.Add("CustomerID", typeof(int));
        ordersTable.Columns.Add("OrderDate", typeof(DateTime));
        dataSet.Tables.Add(ordersTable);

        DataRelation relation = new DataRelation("custToOrders", customersTable.Columns["CustomerID"], ordersTable.Columns["CustomerID"]);
        dataSet.Relations.Add(relation);

        // Add sample data rows to the Customers table.
        customersTable.Rows.Add(1, "John Doe");
        customersTable.Rows.Add(2, "Jane Smith");

        // Add sample data rows to the Orders table.
        ordersTable.Rows.Add(1, 1, DateTime.Now);
        ordersTable.Rows.Add(2, 2, DateTime.Now.AddDays(-1));

        // Bind the DateTimePicker control to the OrderDate column of the Orders table.
        DateTimePicker dateTimePicker = new DateTimePicker();
        dateTimePicker.DataBindings.Add(new Binding("Value", ordersTable, "OrderDate"));
        dateTimePicker.Location = new Point(10, 10);

        // Bind the TextBox control to the CustomerName column of the Customers table.
        TextBox textBox = new TextBox();
        textBox.DataBindings.Add(new Binding("Text", customersTable, "CustomerName"));
        textBox.Location = new Point(10, 40);

        // Bind the TextBox control to the OrderID column of the Orders table.
        TextBox textBox2 = new TextBox();
        textBox2.DataBindings.Add(new Binding("Text", ordersTable, "OrderID"));
        textBox2.Location = new Point(10, 70);

        // Bind the TextBox control to the OrderDate column of the Orders table.
        TextBox textBox3 = new TextBox();
        textBox3.DataBindings.Add(new Binding("Text", ordersTable, "OrderDate"));
        textBox3.Location = new Point(10, 100);

        Form form = new Form();
        form.Controls.Add(dateTimePicker);
        form.Controls.Add(textBox);
        form.Controls.Add(textBox2);
        form.Controls.Add(textBox3);

        Application.Run(form);


        // /**
        //     Below code will iterate through the column values via a "Next" and "Previous" value
        // **/
        // DataTable table = new DataTable();
        // table.Columns.Add("Column1");

        // for (int i = 0; i < 3; i++)
        // {
        //     table.Rows.Add($"Value {i}");
        // }

        // int index = 0;

        // TextBox textBox1 = new TextBox();
        // textBox1.DataBindings.Add("Text", table, "Column1");
        // textBox1.Location = new Point(10, 10);

        // Button nextButton = new Button();
        // nextButton.Text = "Next";
        // nextButton.Click += (sender, e) =>
        // {
        //     index = (index + 1) % table.Rows.Count;
        //     textBox1.Text = table.Rows[index]["Column1"].ToString();
        // };

        // Button prevButton = new Button();
        // prevButton.Text = "Previous";
        // prevButton.Click += (sender, e) =>
        // {
        //     index = (index - 1 + table.Rows.Count) % table.Rows.Count;
        //     textBox1.Text = table.Rows[index]["Column1"].ToString();
        // };

        // FlowLayoutPanel panel = new FlowLayoutPanel();
        // panel.Controls.Add(prevButton);
        // panel.Controls.Add(nextButton);
        // panel.Location = new Point(10, 70);

        // Form form = new Form();
        // form.Controls.Add(panel);
        // form.Controls.Add(textBox1);

        // Application.Run(form);

    }
}