using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace InventoryTransactionEntry
{
    public partial class posting : Form
    {
        Global db = new Global();
        string[] transactionEntry;
        string[] unprintedTransactions;
        Boolean post;

        int casesDummy = 0;
        int casesMaster = 0;
        int piecesDummy = 0;
        int piecesMaster = 0;
        int mergedCases = 0;
        int totalCases = 0;
        int mergedPieces = 0;
        int totalPieces = 0;
        int piecePerUnit = 0;
        int totalRows = 0;
        int percentage = 0;

        public posting()
        {
            InitializeComponent();
        }

        private void posting_Load(object sender, EventArgs e)
        {
            this.Paint += new PaintEventHandler(pnl2_Paint);

            db.openConnection();
            db.fetch("select trans_no, doc_no from transaction_entry");
            while (db.dr.Read())
            {
                ListViewItem lvi = new ListViewItem();
                lvi.SubItems.Add(db.dr["trans_no"].ToString());
                lvi.SubItems.Add(db.dr["doc_no"].ToString());
                listView1.Items.Add(lvi);
            }
            db.dr.Close();
            db.closeConnection();

            panel1.Width = this.Width - 10;
        }

        private void pnl2_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, ClientRectangle,
                                         Color.Gray, 5, ButtonBorderStyle.Solid,
                                         Color.Gray, 5, ButtonBorderStyle.Solid,
                                         Color.Gray, 5, ButtonBorderStyle.Solid,
                                         Color.Gray, 5, ButtonBorderStyle.Solid);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                ListViewItem lvi = listView1.SelectedItems[0];

                if (lvi.Checked == false)
                {
                    lvi.Checked = true;
                }
                else
                {
                    lvi.Checked = false;
                }
            }
        }

        private void checkBox1_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                foreach (ListViewItem lvi in this.listView1.Items)
                {
                    lvi.Checked = true;
                }
            }
            else
            {
                foreach (ListViewItem lvi in this.listView1.Items)
                {
                    lvi.Checked = false;
                }
            }
        }

        private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            int totalItem = listView1.Items.Count;
            int checkedItem = listView1.CheckedItems.Count;
            if (totalItem == checkedItem)
            {
                checkBox1.Checked = true;
            }
            else if (checkedItem < totalItem)
            {
                checkBox1.Checked = false;
            }
        }

        private void checkUnprintedTransactions()
        {
            int unprintedCounter = 0;
            int x = 0;
            unprintedTransactions = new string[listView1.CheckedItems.Count];

            DialogResult result = MessageBox.Show("Verifying the data is required before posting.\n Ensure that all transactions have been printed.\n Do you wish to continue?", "Confirm", MessageBoxButtons.YesNo);

            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                foreach (ListViewItem lvi in this.listView1.Items)
                {
                    if (lvi.Checked == true)
                    {
                        db.openConnection();
                        db.fetch("Select trans_no from transaction_entry where trans_no = '" + lvi.SubItems[1].Text + "' and status ='0'");
                        if (db.dr.Read())
                        {
                            MessageBox.Show(db.dr[0].ToString());
                            unprintedTransactions[x] = lvi.SubItems[1].Text;
                            lvi.Checked = false;
                            checkBox1.Checked = false;
                            x++;
                        }
                        db.dr.Close();
                        db.closeConnection();
                    }
                }
            }

            foreach (string s in unprintedTransactions)
            {
                if (s != null)
                {
                    unprintedCounter++;
                }
            }

            if (listView1.CheckedItems.Count == 0)
            {
                DialogResult dialogResult = MessageBox.Show("All selected transactions still needs to be printed before posting!");
                post = false;
            }
            else if (unprintedCounter > 0 && listView1.CheckedItems.Count > 0)
            {
                DialogResult dialogResult = MessageBox.Show("Unprinted transactions are still detected. Do you wish to continue posting the printed transactions?", "Confirm", MessageBoxButtons.YesNo);

                if (dialogResult == System.Windows.Forms.DialogResult.Yes)
                {
                    post = true;
                }
                else
                {
                    post = false;
                }
            }

        }

        private void btnPost_Click(object sender, EventArgs e)
        {
            int x = 0;

            if (listView1.CheckedItems.Count == 0)
            {
                MessageBox.Show("No transaction has been selected!");
            }
            else
            {
                checkUnprintedTransactions();
                if (post)
                {
                    transactionEntry = new string[listView1.CheckedItems.Count];
                    foreach (ListViewItem lvi in this.listView1.Items)
                    {
                        if (lvi.Checked)
                        {
                            transactionEntry[x] = lvi.SubItems[1].Text;

                            db.openConnection();
                            db.fetch("select count(*) as rows from inventory_dummy where transaction_link = " +
                                "(select entry_id from transaction_entry where trans_no = '" + transactionEntry[x] + "')");
                            if (db.dr.Read())
                            {
                                totalRows += Convert.ToInt32(db.dr["rows"]);
                            }
                            db.dr.Close();
                            db.closeConnection();
                            percentage = 100 / totalRows;
                            x++;
                        }
                    }

                    if (backgroundWorker1.IsBusy != true)
                    {
                        // Start the asynchronous operation.
                        backgroundWorker1.RunWorkerAsync();
                    }
                }
                else
                {
                    this.Close();
                }
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            int counter = 0;

            for (int x = 0; x < transactionEntry.GetLength(0); x++)
            {
                db.openConnection();
                db.fetch("select * from inventory_dummy where transaction_link = " +
                    "(select entry_id from transaction_entry where trans_no = '" + transactionEntry[x] + "')");
                while (db.dr.Read())
                {
                    casesDummy = Convert.ToInt32(db.dr["cases"]);
                    piecesDummy = Convert.ToInt32(db.dr["pieces"]);

                    Global dbIM = new Global();
                    dbIM.openConnection();
                    dbIM.fetch("select * from inventory_master where " +
                        "warehouse_code = '" + db.dr["warehouse_link"].ToString() + "' and " +
                        "item_id_link = '" + db.dr["item_link"].ToString() + "' and " +
                        "location_link = '" + db.dr["location_link"].ToString() + "' and " +
                        "expiration_date = '" + db.dr["expiration_date"].ToString() + "'");
                    if (dbIM.dr.Read())
                    {
                        //Update Item from Inventory

                        Global dbItemMaster = new Global();
                        dbItemMaster.openConnection();
                        dbItemMaster.fetch("select pieces_per_unit from item_master where item_id = '" + db.dr["item_link"].ToString() + "'");
                        if (dbItemMaster.dr.Read())
                        {
                            piecePerUnit = Convert.ToInt32(dbItemMaster.dr["pieces_per_unit"]);
                        }
                        dbItemMaster.dr.Close();
                        dbItemMaster.closeConnection();

                        casesMaster = Convert.ToInt32(dbIM.dr["i_cases"]);
                        piecesMaster = Convert.ToInt32(dbIM.dr["i_pieces"]);

                        mergedPieces = piecesDummy + piecesMaster;
                        mergedCases = casesDummy + casesMaster;

                        if (mergedPieces > piecePerUnit)
                        {
                            totalPieces = mergedPieces % piecePerUnit;
                            totalCases = mergedCases + (mergedPieces / piecePerUnit);
                        }
                        else
                        {
                            totalPieces = mergedPieces;
                            totalCases = mergedCases;
                        }

                        string updateQuery = "update inventory_master set " +
                            "i_cases = '" + totalCases + "', " +
                            "i_pieces = '" + totalPieces + "' " +
                            "where i_id = '" + dbIM.dr["i_id"].ToString() + "'";

                        Global dbUpdate = new Global();
                        dbUpdate.openConnection();
                        dbUpdate.InUpDel(updateQuery);
                        dbUpdate.closeConnection();

                        reportPostingProgress(++counter);
                    }
                    else
                    {
                        //Insert Item into Inventory

                        string insertQuery = "insert into inventory_master values (null, " +
                            "'" + db.dr["warehouse_link"] + "', " +
                            "'" + db.dr["item_link"] + "', " +
                            "'" + db.dr["location_link"] + "', " +
                            "'" + db.dr["cases"] + "', " +
                            "'" + db.dr["pieces"] + "', " +
                            "'" + db.dr["expiration_date"] + "' " +
                            ")";

                        Global dbInsert = new Global();
                        dbInsert.openConnection();
                        dbInsert.InUpDel(insertQuery);
                        dbInsert.closeConnection();

                        reportPostingProgress(++counter);
                    }
                }
                db.dr.Close();
                db.closeConnection();
            }
        }

        private void reportPostingProgress(int counter)
        {
            if (counter == (totalRows))
            {
                backgroundWorker1.ReportProgress((percentage * counter) + (100 - (percentage * totalRows)));
            }
            else
            {
                backgroundWorker1.ReportProgress(percentage * counter);
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            lblposting.Visible = true;
            Thread.Sleep(1);
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("humana'g post!");
            lblposting.Visible = false;
            progressBar1.Value = 0;
        }
    }
}
