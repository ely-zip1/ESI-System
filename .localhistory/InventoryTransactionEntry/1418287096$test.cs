using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InventoryTransactionEntry
{
    public partial class test : Form
    {
        Global db = new Global();
        public test()
        {
            InitializeComponent();
        }

        private void test_Load(object sender, EventArgs e)
        {
            int rowCount = 0;
            string arr;
        }
            //string ans = "";
            //string show = "e l i  s h   a. ";
            //string[] split = show.Split(' ');
            //foreach (string word in split)
            //{
            //    ans += word + "\n";
            //}

            //MessageBox.Show(show +"\n"+ show.Replace(" ", string.Empty));

            //db.openConnection();
            //db.fetch("select * from date_test");
            //if (db.dr.Read())
            //{
            //    MessageBox.Show(db.dr["id"].ToString());
            //    db.InUpDel("insert into date_test values (null, '12/15/2015')");
            //}
            //db.dr.Close();
            //db.closeConnection();

            //double price = 0;
            //string id = "";

            //db.openConnection();
            //db.fetch("select item_code, pieces_per_unit from item_master");
            //while (db.dr.Read())
            //{
            //    if (db.dr["pieces_per_unit"] != DBNull.Value)
            //    {
            //        Global db2 = new Global();
            //        db2.openConnection();
            //        db2.fetch("select p_id, purchase_price from price_purchase where pcode = '" + db.dr["item_code"] + "'");
            //        while (db2.dr.Read())
            //        {
            //            id = db2.dr["p_id"].ToString();
            //            price = Math.Round(Convert.ToDouble(db2.dr["purchase_price"]) / Convert.ToInt32(db.dr["pieces_per_unit"]), 2);
            //            Global db3 = new Global();
            //            db3.openConnection();
            //            db3.InUpDel("update price_purchase set price_per_piece = '" + price + "' where p_id = '" + id + "'");
            //            db3.closeConnection();
            //        }
            //        db2.dr.Close();
            //        db2.closeConnection();
            //    }
            //    //Console.WriteLine(db.dr[0] + " ," + db.dr[1] + " ," + db.dr[2] + " ," + db.dr[3]);
            //}
            //db.dr.Close();
            //db.closeConnection();

        //    string[] arr2 = new string[5];
        //    arr2[0] = "kiugljk";
        //    int x = 0;

        //    foreach (string s in arr2)
        //    {
        //        //if (s == null)
        //        //{
        //        //    Console.WriteLine("empty");
        //        //}
        //        //else
        //        //{
        //        //    Console.WriteLine("{0} ", s);
        //        //}
        //        if (s != null)
        //        {
        //            x++;
        //        }
        //    }
        //    Console.WriteLine("");
        //    Console.WriteLine("" + x);
        //    Console.WriteLine("");
        //}

        private void button1_Click(object sender, EventArgs e)
        {
            int x = textBox1.Location.X + textBox1.Width ;
            int y = textBox1.Location.Y - 10;

            if (textBox1.Text == "")
            {
                //MessageBox.Show(""+ x +" "+ y);
                Label err = new Label();
                err.Name = "err";
                err.Font = new System.Drawing.Font("Segoe UI", 20, FontStyle.Bold);
                err.Parent = this;
                err.Location = new Point(x, y);
                err.Text = "•";
                err.ForeColor = Color.Black;
                err.Visible = true;
                err.AutoSize = true;

                //textBox1.Text = err.Size.Width + " " + err.Size.Height;
                //textBox1.Text = err.Margin.Top.ToString() + " " + err.Margin.Right.ToString() + " " + err.Margin.Bottom.ToString() + " " + err.Margin.Left.ToString();
            }
        }
    }
}
