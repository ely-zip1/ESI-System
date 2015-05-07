using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;
using System.IO;
using System.Drawing.Printing;
using System.Threading;

namespace InventoryTransactionEntry
{
    public partial class frmInventoryTransactionEntry : Form
    {

        Global db = new Global();
        Hashtable arrPriceType = new Hashtable();

        double unitPrice = 0;
        double piecePrice = 0;
        double piecePerUnit = 0;
        double lineAmount = 0;
        string transaction_no = "";
        int screenHeight;
        int screenWidth;
        int posted = 0;

        //string priceCategory = "";
        //string priceEffectivity = "";
        //string priceType = "";
        string pricetype_date = "";


        double linePrice = 0;

        int currentRow = 0;
        int pageNumber = 1;
        int lines = 0;
        int totalCases = 0;
        int totalPieces = 0;
        int rows = 0;
        double totalValue = 0;

        private bool _dragging = false;
        private Point _offset;
        private Point _start_point = new Point(0, 0);

        public frmInventoryTransactionEntry()
        {
            InitializeComponent();
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;

            screenHeight = Screen.PrimaryScreen.Bounds.Height;
            screenWidth = Screen.PrimaryScreen.Bounds.Width;
        }

        private void frmInventoryTransactionEntry_Load(object sender, EventArgs e)
        {
            //this.Left = Top = 0;
            //this.Width = Screen.PrimaryScreen.WorkingArea.Width;
            //this.Height = Screen.PrimaryScreen.WorkingArea.Height;

            lblWelcome.Visible = false;
            lbluser.Visible = false;

            panelMain.Location = new Point(
                    this.ClientSize.Width / 2 - panelMain.Size.Width / 2,
                    this.ClientSize.Height / 2 - panelMain.Size.Height / 2);
            panelMain.Anchor = AnchorStyles.None;
            panelMain.Enabled = false;

            panel4.Width = this.ClientSize.Width + 1;

            pnlControlbox.Location = new Point(screenWidth - pnlControlbox.Bounds.Width + 4, 0);

            frmLogin login = new frmLogin();
            login.Owner = this;
            login.ShowDialog();

            panelMain.Enabled = true;

            cmbtransno.Focus();


            loader();
            this.AcceptButton = btnlineItems;

        }

        private void loader()
        {
            txtItemCode.AutoSize = false;
            this.txtItemCode.Size = new System.Drawing.Size(73, 20);

            txtexpiry.AutoSize = false;
            this.txtexpiry.Size = new System.Drawing.Size(94, 19);

            //Transaction Number
            cmbtransno.Items.Clear();
            db.openConnection();
            db.fetch("Select trans_no from transaction_entry order by entry_id desc limit 1");
            if (db.dr.Read())
            {
                transaction_no = (Convert.ToInt32(db.dr[0]) + 1).ToString();
            }
            else
            {
                transaction_no = "1";
            }
            db.dr.Close();
            db.closeConnection();

            int transNo_length = 0;
            transNo_length = transaction_no.Length;
            if (transNo_length == 1)
            {
                cmbtransno.Items.Add("00000" + transaction_no);
            }
            else if (transNo_length == 2)
            {
                cmbtransno.Items.Add("0000" + transaction_no);
            }
            else if (transNo_length == 3)
            {
                cmbtransno.Items.Add("000" + transaction_no);
            }
            else if (transNo_length == 4)
            {
                cmbtransno.Items.Add("00" + transaction_no);
            }
            else if (transNo_length == 5)
            {
                cmbtransno.Items.Add("0" + transaction_no);
            }
            else if (transNo_length == 6)
            {
                cmbtransno.Items.Add(transaction_no);
            }

            db.openConnection();
            db.fetch("Select trans_no from transaction_entry");
            while (db.dr.Read())
            {
                cmbtransno.Items.Add(db.dr[0].ToString());
            }
            db.dr.Close();
            db.closeConnection();


            //Transaction Date
            DateTime today = DateTime.Today;
            txttransDate.Text = today.ToString("MM/dd/yyyy");

            //Transaction Type
            cmbtranstype.Items.Clear();
            cmbtranstype.Items.Add("");

            db.openConnection();
            db.fetch("Select transaction_code,transaction_type from transaction_type");
            while (db.dr.Read())
            {
                cmbtranstype.Items.Add(db.dr["transaction_code"] + " - " + db.dr["transaction_type"]);
            }
            db.dr.Close();
            db.closeConnection();

            //Source Warehouse
            cmbsourceWH.Items.Clear();
            cmbsourceWH.Items.Add("");
            db.openConnection();
            db.fetch("Select concat_ws(' - ', code, name) from warehouse");
            while (db.dr.Read())
            {
                cmbsourceWH.Items.Add(db.dr[0].ToString());
            }
            db.dr.Close();
            db.closeConnection();

            //Source Location
            cmbsourceLocation.Items.Clear();
            cmbsourceLocation.Items.Add("");
            db.openConnection();
            db.fetch("Select concat_ws(' - ',code, location) from location");
            while (db.dr.Read())
            {
                cmbsourceLocation.Items.Add(db.dr[0].ToString());
            }
            db.dr.Close();
            db.closeConnection();

            //Price Category
            cmbpricecategory.Items.Clear();
            cmbpricecategory.Items.Add("");
            cmbpricecategory.Items.Add("Purchase Price");
            cmbpricecategory.Items.Add("Selling Price");

            //Select Price
            cmbselectprice.Items.Clear();
            cmbselectprice.Items.Add("");
            cmbselectprice.Items.Add("Current");
            cmbselectprice.Items.Add("3 Months Ago");
            cmbselectprice.Items.Add("6 Months Ago");

            cmbtransno.SelectedIndex = 0;
            cmbtranstype.SelectedIndex = 0;
            cmbsourceWH.SelectedIndex = 0;
            cmbsourceLocation.SelectedIndex = 0;
            cmbpricecategory.SelectedIndex = 0;
            cmbselectprice.SelectedIndex = 0;

            lblsourceSalesman.Text = "";
            lbldestSalesman.Text = "";
            cmbreasoncode.Items.Add("");
            cmbreasoncode.SelectedIndex = 0;
        }

        private void cmbtranstype_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbtranstype.BackColor = Color.White;

            db.dr.Close();
            db.closeConnection();

            if (cmbtranstype.Text == "AD - ADJUSTMENTS")
            {
                //Reason Code
                cmbreasoncode.Items.Clear();
                cmbreasoncode.Items.Add("");
                db.openConnection();
                db.fetch("Select concat_ws(' - ',reason_code,reason_description) from reason_code where reason_code like 'AD%'");
                while (db.dr.Read())
                {
                    cmbreasoncode.Items.Add(db.dr[0].ToString());
                    cmbreasoncode.SelectedIndex = 0;
                }
                db.dr.Close();
                db.closeConnection();
            }
            else if (cmbtranstype.Text == "PR - PURCHASE RETURN")
            {
                //Reason Code
                cmbreasoncode.Items.Clear();
                cmbreasoncode.Items.Add("");
                db.openConnection();
                db.fetch("Select concat_ws(' - ',reason_code,reason_description) from reason_code where reason_code like 'PR%'");
                while (db.dr.Read())
                {
                    cmbreasoncode.Items.Add(db.dr[0].ToString());
                    cmbreasoncode.SelectedIndex = 0;
                }
                db.dr.Close();
                db.closeConnection();
            }
            else if (cmbtranstype.Text == "DS - DIRECT SALES")
            {
                //Reason Code
                cmbreasoncode.Items.Clear();
                cmbreasoncode.Items.Add("");
                db.openConnection();
                db.fetch("Select concat_ws(' - ',reason_code,reason_description) from reason_code where reason_code like 'DS%'");
                while (db.dr.Read())
                {
                    cmbreasoncode.Items.Add(db.dr[0].ToString());
                    cmbreasoncode.SelectedIndex = 0;
                }
                db.dr.Close();
                db.closeConnection();
            }
            else if (cmbtranstype.Text == "LL - LOCATION TO LOCATION TRANSFER")
            {
                //Reason Code
                cmbreasoncode.Items.Clear();
                cmbreasoncode.Items.Add("");
                db.openConnection();
                db.fetch("Select concat_ws(' - ',reason_code,reason_description) from reason_code where reason_code like 'LL%'");
                while (db.dr.Read())
                {
                    cmbreasoncode.Items.Add(db.dr[0].ToString());
                    cmbreasoncode.SelectedIndex = 0;
                }
                db.dr.Close();
                db.closeConnection();
            }
            else if (cmbtranstype.Text == "SD - SAMPLE AND DONATION")
            {
                //Reason Code
                cmbreasoncode.Items.Clear();
                cmbreasoncode.Items.Add("");
                db.openConnection();
                db.fetch("Select concat_ws(' - ',reason_code,reason_description) from reason_code where reason_code like 'SD%'");
                while (db.dr.Read())
                {
                    cmbreasoncode.Items.Add(db.dr[0].ToString());
                    cmbreasoncode.SelectedIndex = 0;
                }
                db.dr.Close();
                db.closeConnection();
            }
            else if (cmbtranstype.Text == "SL - STOCK LOCATION ENTRY")
            {
                //Reason Code
                cmbreasoncode.Items.Clear();
                cmbreasoncode.Items.Add("");
                db.openConnection();
                db.fetch("Select concat_ws(' - ',reason_code,reason_description) from reason_code where reason_code like 'SL%'");
                while (db.dr.Read())
                {
                    cmbreasoncode.Items.Add(db.dr[0].ToString());
                    cmbreasoncode.SelectedIndex = 0;
                }
                db.dr.Close();
                db.closeConnection();

                cmbDestWH.Enabled = false;
                cmbDestLocation.Enabled = false;
            }
            else if (cmbtranstype.Text == "WW - WAREHOUSE TO WAREHOUSE TRANSFER")
            {
                //Reason Code
                cmbreasoncode.Items.Clear();
                cmbreasoncode.Items.Add("");
                db.openConnection();
                db.fetch("Select concat_ws(' - ',reason_code,reason_description) from reason_code where reason_code like 'WW%'");
                while (db.dr.Read())
                {
                    cmbreasoncode.Items.Add(db.dr[0].ToString());
                    cmbreasoncode.SelectedIndex = 0;
                }
                db.dr.Close();
                db.closeConnection();
            }

        }

        private void cmbsourceLocation_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbsourceLocation.BackColor = Color.White;

            string Location = "";

            db.dr.Close();
            db.closeConnection();

            db.openConnection();
            db.fetch("Select location from location where code = '" + cmbsourceLocation.Text + "'");
            if (db.dr.Read())
            {
                Location = db.dr[0].ToString();
            }
            db.dr.Close();
            db.closeConnection();

            //lblsourceSalesman.Text = Location;
        }

        private void cmbpricecategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbpricecategory.BackColor = Color.White;
            //if (cmbpricecategory.Text == "Selling price")
            //{
            //    global.openConnection();
            //    global.fetch("Select code from c_location");
            //    while (global.dr.Read())
            //    {
            //        cmbsourceLocation.Items.Add(global.dr[0].ToString());
            //    }
            //    global.dr.Close();
            //    global.closeConnection();
            //}
        }

        private void btnlineItems_Click(object sender, EventArgs e)
        {
            Label err = new Label();
            int x;
            int y;
            linePrice = 0;
            foreach (Control c in this.groupBoxTransactionDetails.Controls)
            {
                if ((c is ComboBox || c is TextBox) && c.Name != "txtcomment")
                {
                    if (c.Text == "")
                    {
                        x = c.Location.X + c.Width - 5;
                        y = c.Location.Y - 12;

                        err = new Label();
                        err.Name = "lbl" + c.Name;
                        err.Font = new System.Drawing.Font("Segoe UI", 15, FontStyle.Bold);
                        err.Parent = this;
                        err.Location = new Point(x, y);
                        err.Text = "*";//"•";
                        err.ForeColor = Color.IndianRed;
                        err.AutoSize = true;
                        err.Margin = new System.Windows.Forms.Padding(0);
                        err.Visible = true;
                        groupBoxTransactionDetails.Controls.Add(err);
                    }
                    else
                    {
                        foreach (Control lbl in this.groupBoxTransactionDetails.Controls)
                        {
                            if (lbl.Name == ("lbl" + c.Name))
                            {
                                lbl.Dispose();
                            }
                        }
                    }
                }
            }

            if (cmbtranstype.Text == "")
            {
                cmbtranstype.BackColor = Color.IndianRed;
                cmbtranstype.Focus();
            }
            else if (txtdocumentNo.Text == "")
            {
                txtdocumentNo.BackColor = Color.IndianRed;
                txtdocumentNo.Focus();
            }
            else if (cmbsourceWH.Text == "")
            {
                cmbsourceWH.BackColor = Color.IndianRed;
                cmbsourceWH.Focus();
            }
            else if (cmbsourceLocation.Text == "")
            {
                cmbsourceLocation.BackColor = Color.IndianRed;
                cmbsourceLocation.Focus();
            }
            else if (cmbpricecategory.Text == "")
            {
                cmbpricecategory.BackColor = Color.IndianRed;
                cmbpricecategory.Focus();
            }
            else if (cmbselectprice.Text == "")
            {
                cmbselectprice.BackColor = Color.IndianRed;
                cmbselectprice.Focus();
            }
            else if (cmbreasoncode.Text == "")
            {
                cmbreasoncode.BackColor = Color.IndianRed;
                cmbreasoncode.Focus();
            }
            else
            {

                int hasDuplicate = 0;
                db.openConnection();
                db.fetch("select trans_no from transaction_entry where trans_no = '" + cmbtransno.Text + "'");
                if (db.dr.Read())
                {
                    hasDuplicate++;
                }
                db.dr.Close();
                db.closeConnection();

                if (hasDuplicate > 0)
                {
                    db.openConnection();
                    db.InUpDel("update transaction_entry set " +
                            "trans_type_link = (select id from transaction_type where transaction_code = '" + cmbtranstype.Text.Substring(0, 2) + "'), " +
                            "doc_no = '" + txtdocumentNo.Text + "', trans_date = (select str_to_date('" + txttransDate.Text + "','%m/%d/%Y')) , " +
                            "source_WH_link = (select warehouse_id from warehouse where code = '" + cmbsourceWH.Text.Substring(0, 2) + "'), " +
                            "source_location_link = (select location_id from location where code = '" + cmbsourceLocation.Text.Substring(0, 2) + "')," +
                            "source_salesman_link = null, destination_WH_link = null, destination_location_link = null, destination_salesman_link = null, " +
                            "price_category = '" + cmbpricecategory.Text + "', " +
                            "price_type = '" + cmbselectprice.Text + "', " +
                            "reason_code_link = (select reasoncode_id from reason_code where reason_code = '" + cmbreasoncode.Text.Substring(0, 3) + "'), " +
                            "comment = '" + txtcomment.Text + "', " +
                            "status = '" + posted + "' where trans_no = '" + cmbtransno.Text + "'");
                    db.closeConnection();
                    //MessageBox.Show("Update Done");
                }

                else
                {
                    db.openConnection();
                    db.InUpDel("insert into transaction_entry values(null, '" + cmbtransno.Text + "'," +
                        "(select id from transaction_type where transaction_code = '" + cmbtranstype.Text.Substring(0, 2) + "'), " +
                        "'" + txtdocumentNo.Text + "', (select str_to_date('" + txttransDate.Text + "','%m/%d/%Y')) , " +
                        "(select warehouse_id from warehouse where code = '" + cmbsourceWH.Text.Substring(0, 2) + "'), " +
                        "(select location_id from location where code = '" + cmbsourceLocation.Text.Substring(0, 2) + "')," +
                        "null, null, null, null, " +
                        "'" + cmbpricecategory.Text + "', " +
                        "'" + cmbselectprice.Text + "', " +
                        "(select reasoncode_id from reason_code where reason_code = '" + cmbreasoncode.Text.Substring(0, 3) + "'), " +
                        "'" + txtcomment.Text + "', " +
                        "'" + posted + "')");
                    db.closeConnection();

                }

                groupBoxTransactionDetails.Enabled = false;
                pnlSL.Enabled = true;
                lineItemLoader();
            }


        }

        private void btndelete_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete this Transaction?", "Delete Transaction Entry", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                db.openConnection();
                db.InUpDel("delete from transaction_entry where trans_no = '" + cmbtransno.Text + "'");
                db.closeConnection();

                foreach (Control c in groupBoxTransactionDetails.Controls)
                {
                    if (c is TextBox)
                    {
                        if (c.Name != "txttransDate")
                        {
                            c.Text = "";
                        }
                    }
                    if (c is ComboBox)
                    {
                        c.Text = "";
                    }
                }

                loader();
            }
        }

        private void lineItemLoader()
        {
            string transType = cmbtranstype.Text;
            string reason = cmbreasoncode.Text;

            txttransactionNo.Text = cmbtransno.Text;
            txtdocumentNo2.Text = txtdocumentNo.Text;
            txttransDate2.Text = txttransDate.Text;
            txtComment2.Text = txtcomment.Text;

            txtSourceWH.Text = cmbsourceWH.Text;
            txtSourceLocation.Text = cmbsourceLocation.Text;
            txtLC.Text = txtSourceLocation.Text.Substring(0, 2);
            txtSourceSalesman.Text = lblsourceSalesman.Text;
            txtCases.Text = "0";
            txtPieces.Text = "0";

            if (cmbtranstype.Text == "SL - STOCK LOCATION ENTRY")
            {
                txtDestWH.Enabled = false;
                txtDestLocation.Enabled = false;
                txtDestSalesman.Enabled = false;
            }
            //Transaction Code
            txtTransCode.Text = cmbtranstype.Text.Substring(0, 2);

            //Transaction Description
            txtTransdesc.Text = cmbtranstype.Text.Substring(5, cmbtranstype.Text.Length - 5);

            //Reason Code
            txtReasonCode.Text = cmbreasoncode.Text.Substring(0, 3);

            //Reason Description
            txtReasonDesc.Text = cmbreasoncode.Text.Substring(6, cmbreasoncode.Text.Length - 6);

            //Warehouse Code
            txtWHCode.Text = txtSourceWH.Text.Substring(0, 2);

            txtOrderAmount.Text = " 0.00";

            Global global2 = new Global();
            global2.openConnection();
            global2.fetch("select count(*) from view_inventory_dummy where transaction = '" + cmbtransno.Text + "'");
            while (global2.dr.Read())
            {
                rows = Convert.ToInt32(global2.dr[0]);
                progressBar1.Value = 0;
            }
            progressBar1.Maximum = rows;
            global2.dr.Close();
            global2.closeConnection();

            backgroundWorker1.RunWorkerAsync(cmbtransno.Text);

            txtItemCode.Focus();
            //AUTO COMPLETE 
            string[] item_code = null;
            int counter = 0;
            // Create the list to use as the custom source.
            var source = new AutoCompleteStringCollection();

            db.openConnection();
            db.fetch("select count(item_code) from item_master");
            if (db.dr.Read())
            {
                item_code = new string[Convert.ToInt32(db.dr[0])];
            }
            db.dr.Close();
            db.closeConnection();

            db.openConnection();
            db.fetch("select item_code from item_master");
            while (db.dr.Read())
            {
                item_code.SetValue(db.dr[0].ToString(), counter);
                counter++;
            }
            db.dr.Close();
            db.closeConnection();

            source.AddRange(item_code);

            txtItemCode.AutoCompleteCustomSource = source;
            txtItemCode.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            txtItemCode.AutoCompleteSource = AutoCompleteSource.CustomSource;
        }

        private void txtItemCode_TextChanged(object sender, EventArgs e)
        {
            if (txtItemCode.Text == "")
            {
                txtItemDesc.Text = "";
                txtexpiry.Clear();
            }

            db.openConnection();
            db.fetch("select item_description,pieces_per_unit from item_master where item_code = '" + txtItemCode.Text + "'");
            if (db.dr.Read())
            {
                // If item_code exists, display item_description and other item details
                txtItemDesc.Text = db.dr["item_description"].ToString();
                piecePerUnit = Convert.ToDouble(db.dr["pieces_per_unit"]);
            }
            else
            {
                txtItemDesc.Text = "";
            }
            db.dr.Close();
            db.closeConnection();

            if (txtItemDesc.Text != "")
            {

                getUnitPrice();
                //piecePrice = unitPrice / piecePerUnit;

                cmbPT.Items.Clear();

                foreach (DictionaryEntry de in arrPriceType)
                {
                    cmbPT.Items.Add(de.Key);
                }

                int index = cmbPT.Items.IndexOf("PL1");
                if (index != null)
                {
                    cmbPT.SelectedIndex = index;
                }
                else
                {
                    cmbPT.SelectedIndex = 0;
                }

                db.openConnection();
                db.fetch("select tax_rate from item_master where item_code = '" + txtItemCode.Text + "'");
                if (db.dr.Read())
                {
                    txtTax.Text = db.dr[0].ToString() + " %";
                }
                db.dr.Close();
                db.closeConnection();

                if (cmbselectprice.Text == "Current Price")
                {
                    db.openConnection();
                    db.fetch("select tax_rate from item_master where item_code = '" + txtItemCode.Text + "'");
                    if (db.dr.Read())
                    {
                        txtTax.Text = db.dr[0].ToString() + " %";
                    }
                    db.dr.Close();
                    db.closeConnection();
                }
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            foreach (Control c in pnlSL.Controls)
            {
                if (c is TextBox)
                {
                    c.Text = "";
                }
            }
            txtOrderAmount.Text = "";


            if (lvlListItem.Items.Count > 0)
            {
                foreach (ListViewItem itemRow in this.lvlListItem.Items)
                {
                    //for (int i = 0; i < itemRow.SubItems.Count; i++)
                    //{
                    //    // Do something useful here !
                    //    // e.g 'itemRow.SubItems[count]' <-- Should give you direct access to
                    //    // the item located at co-ordinates(0,0). Once you got it, do something 
                    //    // with it.
                    //}
                    db.openConnection();

                    db.InUpDel("delete from inventory_dummy where item_link = (select item_id from item_master where item_code = '" + itemRow.SubItems[0].Text + "')");

                    db.closeConnection();

                }
            }

            orderAmountSum();
            txtOrderAmount.Text = "";

            lvlListItem.Items.Clear();
            pnlSL.Enabled = false;
            groupBoxTransactionDetails.Enabled = true;
            cmbtransno.Focus();


        }

        private void btnAddItem_Click(object sender, EventArgs e)
        {
            txtItemCode.Focus();

            if (txtItemCode.Text == "")
            {
                MessageBox.Show("No Item Entered!");
                txtItemCode.Focus();
            }
            else if (txtItemDesc.Text == "")
            {
                MessageBox.Show("Item does not Exist!");
                txtItemCode.Focus();
            }
            else if ((txtCases.Text == "0" || txtCases.Text == "") && (txtPieces.Text == "0" || txtPieces.Text == ""))
            {
                MessageBox.Show("No Quantity Entered!");
                txtCases.Focus();
            }
            else if (txtexpiry.MaskCompleted == false)
            {
                MessageBox.Show("No Expiration Date Entered!");
                txtexpiry.Focus();
            }
            else
            {
                if (txtCases.Text == "")
                {
                    txtCases.Text = "0";
                }
                if (txtPieces.Text == "")
                {
                    txtPieces.Text = "0";
                }

                int dataExists = 0;
                db.openConnection();
                db.fetch("select id from view_inventory_dummy where item_code = '" + txtItemCode.Text + "' and transaction = '" + txttransactionNo.Text + "'");
                while (db.dr.Read())
                {
                    dataExists = 1;
                }
                db.dr.Close();
                db.closeConnection();

                if (dataExists == 0)
                {
                    lineAmount = Math.Round(((Convert.ToDouble(txtCases.Text) * piecePerUnit) + Convert.ToDouble(txtPieces.Text)) * piecePrice, 2);
                    MessageBox.Show("" + piecePrice);

                    ListViewItem lvi = new ListViewItem(txtItemCode.Text);
                    lvi.SubItems.Add(cmbPT.Text);
                    lvi.SubItems.Add(txtItemDesc.Text);
                    lvi.SubItems.Add(txtLC.Text);
                    lvi.SubItems.Add(txtCases.Text);
                    lvi.SubItems.Add(txtPieces.Text);
                    lvi.SubItems.Add(String.Format("{0:0.00}", piecePrice));
                    lvi.SubItems.Add(String.Format("{0:0.00}", lineAmount));
                    lvlListItem.Items.Add(lvi);

                    //MessageBox.Show("" + pricetype_date);
                    string insert = "insert into inventory_dummy values(null, " +
                        "(select warehouse_id from warehouse where code = '" + txtWHCode.Text + "' ), " +
                        "(select item_id from item_master where item_code = '" + txtItemCode.Text + "'), " +
                        "(select location_id from location where code = '" + txtLC.Text + "'), " +
                        "'" + txtCases.Text + "', " +
                        "'" + txtPieces.Text + "'," +
                        "'" + txtexpiry.Text + "', " +
                        "(select entry_id from transaction_entry where trans_no = '" + cmbtransno.Text + "')," +
                        "'" + lvi.SubItems[6].Text + "'," +
                        " '" + lvi.SubItems[7].Text + "',";

                    if (cmbpricecategory.Text == "Selling Price")
                    {
                        insert += "(select s_id from price_selling where (code = '" + txtItemCode.Text + "' and price_type = '" + cmbPT.Text + "') and effective_from = '" + pricetype_date + "'),";
                        insert += "0)";
                    }
                    else if (cmbpricecategory.Text == "Purchase Price")
                    {
                        insert += "0,";
                        insert += "'PL1')";
                    }

                    db.openConnection();
                    db.InUpDel(insert);
                    db.closeConnection();
                }
                else if (dataExists == 1)
                {
                    lineAmount = ((Convert.ToDouble(txtCases.Text) * piecePerUnit) + Convert.ToDouble(txtPieces.Text)) * piecePrice;

                    MessageBox.Show("" + piecePrice);

                    DialogResult dialogResult = MessageBox.Show("Same item aleady exists. \nDo you want to replace it?", "Duplicate Entry", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        string update = "update inventory_dummy set" +
                            " warehouse_link = (select warehouse_id from warehouse where code = '" + txtWHCode.Text + "' )," +
                            "item_link = (select item_id from item_master where item_code = '" + txtItemCode.Text + "'), " +
                            "location_link = (select location_id from location where code = '" + txtLC.Text + "'), " +
                            "cases = " + txtCases.Text + ", " +
                            "pieces = " + txtPieces.Text + "," +
                            "expiration_date = '" + txtexpiry.Text + "', " +
                            "transaction_link = (select entry_id from transaction_entry where trans_no = '" + cmbtransno.Text + "')," +
                            "priceperpiece = '" + piecePrice + "'," +
                            "linevalue = '" + lineAmount + "',";


                        if (cmbpricecategory.Text == "Selling Price")
                        {
                            update += "price_selling_link = (select s_id from price_selling where (code = '" + txtItemCode.Text + "' and " +
                                "price_type = '" + cmbPT.Text + "') and effective_from = '" + pricetype_date + "'), ";
                            update += "price_purchase_link = 0 ";
                            update += "where item_link = (select item_id from item_master where item_code = '" + txtItemCode.Text + "') and " +
                            "transaction_link = (select entry_id from transaction_entry where trans_no = '" + cmbtransno.Text + "')";
                        }
                        else if (cmbpricecategory.Text == "Purchase Price")
                        {
                            update += "price_selling_link = 0,";
                            update += "price_purchase_link = 'PL1' ";
                            update += "where item_link = (select item_id from item_master where item_code = '" + txtItemCode.Text + "') and " +
                            "transaction_link = (select entry_id from transaction_entry where trans_no = '" + cmbtransno.Text + "')";
                        }

                        //MessageBox.Show(""+piecePrice);

                        db.openConnection();
                        db.InUpDel(update);
                        db.closeConnection();


                        foreach (ListViewItem lvi in this.lvlListItem.Items)
                        {
                            if (lvi.SubItems[0].Text == txtItemCode.Text)
                            {
                                lvi.SubItems[0].Text = txtItemCode.Text;
                                lvi.SubItems[1].Text = cmbPT.Text;
                                lvi.SubItems[2].Text = txtItemDesc.Text;
                                lvi.SubItems[3].Text = txtLC.Text;
                                lvi.SubItems[4].Text = txtCases.Text;
                                lvi.SubItems[5].Text = txtPieces.Text;
                                lvi.SubItems[6].Text = String.Format("{0:0.00}", piecePrice);
                                lvi.SubItems[7].Text = String.Format("{0:0.00}", lineAmount);
                                break;
                            }
                        }
                    }
                    else if (dialogResult == DialogResult.No)
                    {
                        //do something else
                    }
                }

                orderAmountSum();

                foreach (Control c in pnlItemDetails.Controls)
                {
                    if (c is TextBox)
                    {
                        if (c.Name != "txtOrderAmount" && c.Name != "txtWHCode" && c.Name != "txtLC")
                        {
                            c.Text = "";
                        }
                        if (c.Name == "txtCases" || c.Name == "txtPieces")
                        {
                            c.Text = "0";
                        }
                    }
                }

                cmbPT.Items.Clear();

                txtexpiry.Text = "";
                txtItemCode.Focus();
            }
        }

        private void btnDeleteLine_Click(object sender, EventArgs e)
        {
            int isSelected = 0;

            DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete this item?", "Delete Item", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {

                foreach (ListViewItem itemRow in this.lvlListItem.Items)
                {
                    if (itemRow.Selected == true)
                    {
                        isSelected++;
                    }
                    else
                    {

                    }
                }
                if (isSelected > 0)
                {

                    if (lvlListItem.Items.Count > 0)
                    {

                        ListViewItem item = lvlListItem.SelectedItems[0];
                        string code = item.SubItems[0].Text;
                        string i_id = "";

                        for (int i = 0; i < lvlListItem.Items.Count; i++)
                        {
                            if (lvlListItem.Items[i].Selected)
                            {
                                lvlListItem.Items[i].Remove();
                                i--;
                            }
                        }



                        db.openConnection();
                        db.fetch("select id from view_inventory_dummy where item_code = '" + code + "' and transaction = '" + cmbtransno.Text + "'");
                        if (db.dr.Read())
                        {
                            i_id = db.dr[0].ToString();
                        }
                        db.dr.Close();
                        db.closeConnection();

                        db.openConnection();
                        db.InUpDel("delete from inventory_dummy where id = '" + i_id + "'");
                        db.closeConnection();

                        foreach (Control c in pnlItemDetails.Controls)
                        {
                            if (c is TextBox)
                            {
                                if (c.Name != "txtOrderAmount" && c.Name != "txtWHCode" && c.Name != "txtLC")
                                {
                                    c.Text = "";
                                }
                                if (c.Name == "txtCases" || c.Name == "txtPieces")
                                {
                                    c.Text = "0";
                                }
                            }
                        }
                        txtexpiry.Text = "";

                        orderAmountSum();

                        txtItemCode.Focus();
                    }
                }
            }
        }

        private void orderAmountSum()
        {
            double iSum = 0;

            if (lvlListItem.Items.Count == 0)
            {
                txtOrderAmount.Text = "";
            }
            else
            {
                foreach (ListViewItem o in this.lvlListItem.Items)
                {
                    iSum = iSum + Convert.ToDouble(o.SubItems[7].Text);
                }

                txtOrderAmount.Text = " " + String.Format("{0:n}", iSum);
            }
        }

        private void lvlListItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvlListItem.SelectedItems.Count > 0)
            {
                ListViewItem item = lvlListItem.SelectedItems[0];
                txtItemCode.Text = item.SubItems[0].Text;
                //txtPT.Text = item.SubItems[1].Text;
                txtLC.Text = item.SubItems[3].Text;
                txtCases.Text = item.SubItems[4].Text;
                txtPieces.Text = item.SubItems[5].Text;
                txtItemDesc.Text = item.SubItems[2].Text;

                getUnitPrice();
                //txtPrice.Text = String.Format("{0:0.00}", unitPrice);

                db.openConnection();
                db.fetch("select expiration_date from view_inventory_dummy where item_code = '" + txtItemCode.Text + "'");
                while (db.dr.Read())
                {
                    txtexpiry.Text = db.dr[0].ToString();
                }
                db.dr.Close();
                db.closeConnection();

                db.openConnection();
                db.fetch("select tax_rate from item_master where item_code = '" + txtItemCode.Text + "'");
                if (db.dr.Read())
                {
                    txtTax.Text = db.dr[0].ToString() + " %";
                }
                db.dr.Close();
                db.closeConnection();
            }
            else
            {
                foreach (Control c in pnlItemDetails.Controls)
                {
                    if (c is TextBox)
                    {
                        if (c.Name != "txtOrderAmount" && c.Name != "txtWHCode" && c.Name != "txtLC")
                        {
                            c.Text = "";
                        }
                        if (c.Name == "txtCases" || c.Name == "txtPieces")
                        {
                            c.Text = "0";
                        }
                    }
                }
            }
        }

        private void getCurrentSellingPrice()
        {
            db.openConnection();
            db.fetch("select effective_from, price_type, price_per_piece from price_selling where code = '" + txtItemCode.Text + "' and " +
                "str_to_date(effective_from, '%m/%d/%Y') = (select max(str_to_date(effective_from, '%m/%d/%Y')) from price_selling where code = '" + txtItemCode.Text + "')");

            arrPriceType.Clear();

            while (db.dr.Read())
            {
                pricetype_date = db.dr["effective_from"].ToString();
                arrPriceType.Add(db.dr["price_type"].ToString(), db.dr["price_per_piece"].ToString());
            }
            db.dr.Close();
            db.closeConnection();
        }

        private void getCurrentPurchasePrice()
        {
            db.openConnection();
            db.fetch("select price_per_piece, effective_date from price_purchase where pcode = '" + txtItemCode.Text + "' and " +
                "str_to_date(effective_date, '%m/%d/%Y') = (select max(str_to_date(effective_date, '%m/%d/%Y')) from price_purchase where pcode = '" + txtItemCode.Text + "') limit 1");

            arrPriceType.Clear();

            while (db.dr.Read())
            {
                pricetype_date = db.dr["effective_date"].ToString();
                arrPriceType.Add("PL1", db.dr["price_per_piece"].ToString());
            }
            db.dr.Close();
            db.closeConnection();
        }

        private void getPreviousPrice(string priceCategory, string month)
        {
            string datePrevious = null;
            string dateCurrent = null;
            //string date = null;
            string code = txtItemCode.Text;
            string columns = null;
            string table = null;
            string where = null;
            string dateColumn = null;

            if (priceCategory == "Selling")
            {
                columns = "price_type, price_per_piece";
                table = "price_selling";
            }
            else if (priceCategory == "Purchase")
            {
                columns = "price_per_piece";
                table = "price_purchase";
            }

            // Fetches latest date & date for the preceding 3 months   
            db.openConnection();
            db.fetch("select str_to_date(max('" + dateColumn + "'), '%m/%d/%Y') as dateCurrent, " +
                "subdate(str_to_date(max('" + dateColumn + "'), '%m/%d/%Y'), interval " + month + " month) as datePrevious" +
                " from '" + table + "' where code = '" + code + "'");
            if (db.dr.Read())
            {
                dateCurrent = db.dr["dateCurrent"].ToString();
                datePrevious = db.dr["datePrevious"].ToString();
            }
            db.dr.Close();
            db.closeConnection();

            // fetches existing latest existing date between the dateCurrent and datePrevious which is
            // also the date for the price
            if (dateCurrent != null && datePrevious != null)
            {
                db.openConnection();
                db.fetch("select max(str_to_date('" + dateColumn + "','%m/%d/%Y')) as date from '" + table + "' where" +
                    "str_to_date('" + dateColumn + "','%m/%d/%Y') between '" + datePrevious + "' and subdate('" + dateCurrent + "', interval 1 day)");
                if (db.dr.Read())
                {
                    pricetype_date = db.dr["date"].ToString();
                }
                db.dr.Close();
                db.closeConnection();
            }

            if (pricetype_date != "" || pricetype_date != null)
            {
                string query = "select " + columns + " from " + table + " where " +
                        "code = " + code + " and " + dateColumn + " = " + pricetype_date;

                db.openConnection();
                db.fetch(query);

                if (db.dr.Read())
                {
                    db.openConnection();
                    db.fetch(query);

                    arrPriceType.Clear();

                    while (db.dr.Read())
                    {
                        if (priceCategory == "Selling")
                        {
                            arrPriceType.Add(db.dr["price_type"].ToString(), db.dr["price_per_piece"].ToString());
                        }
                        else if (priceCategory == "Purchase")
                        {
                            arrPriceType.Add("PL1", db.dr["price_per_piece"].ToString());
                            break;
                        }
                    }
                    db.dr.Close();
                    db.closeConnection();
                }
                db.dr.Close();
                db.closeConnection();
            }
            else
            {
                if (table == "price_selling")
                {
                    getCurrentSellingPrice();
                }
                else
                {
                    getCurrentPurchasePrice();
                }
            }

        }

        private void getUnitPrice()
        {
            if (cmbpricecategory.Text == "Selling Price")
            {
                if (cmbselectprice.Text == "Current")
                {
                    getCurrentSellingPrice();
                }
                else if (cmbselectprice.Text == "3 Months Ago")
                {
                    getPreviousPrice("Selling", "3");
                }
                else if (cmbselectprice.Text == "6 Months Ago")
                {
                    getPreviousPrice("Selling", "6");
                }
                //piecePrice = unitPrice / piecePerUnit;
                //MessageBox.Show("" + piecePrice);
            }
            //Purchase price
            else if (cmbpricecategory.Text == "Purchase Price")
            {
                if (cmbselectprice.Text == "Current")
                {
                    getCurrentPurchasePrice();
                }
                else if (cmbselectprice.Text == "3 Months Ago")
                {
                    getPreviousPrice("Purchase", "3");
                }
                else if (cmbselectprice.Text == "6 Months Ago")
                {
                    getPreviousPrice("Purchase", "6");
                }
                //piecePrice = unitPrice / piecePerUnit;
                //MessageBox.Show("" + piecePrice);
            }
        }

        private void groupBoxTransactionDetails_EnabledChanged(object sender, EventArgs e)
        {
            if (groupBoxTransactionDetails.Enabled == true)
            {
                this.AcceptButton = btnlineItems;
            }
            else
            {
                this.AcceptButton = btnAddItem;
            }
        }

        private void frmInventoryTransactionEntry_Activated(object sender, EventArgs e)
        {
            loader();
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnMaxRes_Click(object sender, EventArgs e)
        {
            formResize();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void panel1_DoubleClick(object sender, EventArgs e)
        {
            formResize();
        }

        private void formResize()
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                this.WindowState = FormWindowState.Maximized;
                btnMaxRes.Image = Properties.Resources.restore;
                btnMaxRes.FlatAppearance.BorderSize = 0;

                screenHeight = this.Height;
                screenWidth = this.Width;
                pnlControlbox.Location = new Point(screenWidth - pnlControlbox.Bounds.Width + 1, 0);

                panel1.Width = this.Width;
                panel1.Location = new Point(0, 0);
                lblWelcome.Top = (panel1.Height + 8);
                lbluser.Top = (panel1.Height + 5); ;
            }
            else if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
                this.CenterToScreen();
                btnMaxRes.Image = Properties.Resources.maximize;
                btnMaxRes.FlatAppearance.BorderSize = 0;

                screenHeight = this.Height;
                screenWidth = this.Width;

                panelMain.Location = new Point(
                    this.ClientSize.Width / 2 - panelMain.Size.Width / 2,
                    this.ClientSize.Height / 2 - panelMain.Size.Height / 2);
                panelMain.Anchor = AnchorStyles.None;

                this.Paint += new PaintEventHandler(pnl2_Paint);

                panel1.Width = this.Width - 10;
                panel1.Location = new Point(5, 5);
                lblWelcome.Top = (panel1.Height + 8);
                lbluser.Top = (panel1.Height + 5); ;
                pnlControlbox.Location = new Point(this.Width - (pnlControlbox.Width + 10), 0);
            }
        }

        private void pnl2_Paint(object sender, PaintEventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                ControlPaint.DrawBorder(e.Graphics, ClientRectangle,
                                         Color.Gray, 5, ButtonBorderStyle.Solid,
                                         Color.Gray, 5, ButtonBorderStyle.Solid,
                                         Color.Gray, 5, ButtonBorderStyle.Solid,
                                         Color.Gray, 5, ButtonBorderStyle.Solid);
            }
            else
            {
                ControlPaint.DrawBorder(e.Graphics, ClientRectangle,
                                         Color.Gray, 0, ButtonBorderStyle.Solid,
                                         Color.Gray, 0, ButtonBorderStyle.Solid,
                                         Color.Gray, 0, ButtonBorderStyle.Solid,
                                         Color.Gray, 0, ButtonBorderStyle.Solid);
            }
        }

        private void cmbtransno_SelectedIndexChanged(object sender, EventArgs e)
        {
            string transType = "";
            string sWH = "";
            string sLoc = "";
            string priceCat = "";
            string priceType = "";
            string reason = "";

            //trans_no
            db.openConnection();
            db.fetch("select concat_ws(' - ', transaction_code, transaction_type) as trans_no from view_transaction_entry where  trans_no = '" + cmbtransno.Text + "'");
            if (db.dr.Read())
            {
                transType = db.dr["trans_no"].ToString();
                switch (transType.Substring(0, 2))
                {
                    case "AD": cmbtranstype.SelectedIndex = 1;
                        break;
                    case "PR": cmbtranstype.SelectedIndex = 2;
                        break;
                    case "DS": cmbtranstype.SelectedIndex = 3;
                        break;
                    case "LL": cmbtranstype.SelectedIndex = 4;
                        break;
                    case "SD": cmbtranstype.SelectedIndex = 5;
                        break;
                    case "SL": cmbtranstype.SelectedIndex = 6;
                        break;
                    case "WW": cmbtranstype.SelectedIndex = 7;
                        break;
                    default: cmbtranstype.SelectedIndex = 0;
                        break;
                }
            }
            else
            {
                cmbtranstype.SelectedIndex = 0;
            }
            db.dr.Close();
            db.closeConnection();

            //document_No & trans_date
            string[] date;

            db.openConnection();
            db.fetch("select doc_no, trans_date from view_transaction_entry where  trans_no = '" + cmbtransno.Text + "'");
            if (db.dr.Read())
            {

                txtdocumentNo.Text = db.dr["doc_no"].ToString();
                //txttransDate.Text = db.dr["trans_date"].ToString();
                date = db.dr["trans_date"].ToString().Split(' ');
                txttransDate.Text = date[0];
            }
            else
            {
                txtdocumentNo.Text = "";

                DateTime today = DateTime.Today;
                txttransDate.Text = today.ToString("MM/dd/yyyy");
            }
            db.dr.Close();
            db.closeConnection();

            //source warehouse
            db.openConnection();
            db.fetch("select source_warehouse from view_transaction_entry where  trans_no = '" + cmbtransno.Text + "'");
            if (db.dr.Read())
            {

                sWH = db.dr["source_warehouse"].ToString();
                switch (sWH)
                {
                    case "ESI_MAIN": cmbsourceWH.SelectedIndex = 1;
                        break;
                    case "ESI_OZAMIZ": cmbsourceWH.SelectedIndex = 2;
                        break;
                    case "ESI_BUTUAN": cmbsourceWH.SelectedIndex = 3;
                        break;
                    case "ESI_BUKIDNON": cmbsourceWH.SelectedIndex = 4;
                        break;
                    case "ESI_ILIGAN": cmbsourceWH.SelectedIndex = 5;
                        break;
                    default: cmbsourceWH.SelectedIndex = 0;
                        break;
                }
            }
            else
            {
                cmbsourceWH.SelectedIndex = 0;
            }
            db.dr.Close();
            db.closeConnection();

            //source location
            db.openConnection();
            db.fetch("select source_location from view_transaction_entry where  trans_no = '" + cmbtransno.Text + "'");
            if (db.dr.Read())
            {
                sLoc = db.dr["source_location"].ToString();
                switch (sLoc)
                {
                    case "GD": cmbsourceLocation.SelectedIndex = 1;
                        break;
                    case "PR": cmbsourceLocation.SelectedIndex = 2;
                        break;
                    case "FB": cmbsourceLocation.SelectedIndex = 3;
                        break;
                    case "FR": cmbsourceLocation.SelectedIndex = 4;
                        break;
                    case "SP": cmbsourceLocation.SelectedIndex = 5;
                        break;
                    case "BD": cmbsourceLocation.SelectedIndex = 6;
                        break;
                    case "CA": cmbsourceLocation.SelectedIndex = 7;
                        break;
                    case "CB": cmbsourceLocation.SelectedIndex = 8;
                        break;
                    case "CC": cmbsourceLocation.SelectedIndex = 9;
                        break;
                    case "CD": cmbsourceLocation.SelectedIndex = 10;
                        break;
                    case "CE": cmbsourceLocation.SelectedIndex = 11;
                        break;
                    case "DA": cmbsourceLocation.SelectedIndex = 12;
                        break;
                    case "DB": cmbsourceLocation.SelectedIndex = 13;
                        break;
                    case "DC": cmbsourceLocation.SelectedIndex = 14;
                        break;
                    case "EA": cmbsourceLocation.SelectedIndex = 15;
                        break;
                    case "EB": cmbsourceLocation.SelectedIndex = 16;
                        break;
                    case "FG": cmbsourceLocation.SelectedIndex = 17;
                        break;
                    default: cmbsourceLocation.SelectedIndex = 0;
                        break;
                }
            }
            else
            {
                cmbsourceLocation.SelectedIndex = 0;
            }
            db.dr.Close();
            db.closeConnection();

            //price cat & type
            db.openConnection();
            db.fetch("select price_category, price_type from view_transaction_entry where  trans_no = '" + cmbtransno.Text + "'");
            if (db.dr.Read())
            {
                priceCat = db.dr["price_category"].ToString();
                switch (priceCat)
                {
                    case "Purchase Price": cmbpricecategory.SelectedIndex = 1;
                        break;
                    case "Selling Price": cmbpricecategory.SelectedIndex = 2;
                        break;
                    default: cmbpricecategory.SelectedIndex = 0;
                        break;
                }

                priceType = db.dr["price_type"].ToString();
                switch (priceType)
                {
                    case "Current": cmbselectprice.SelectedIndex = 1;
                        break;
                    case "3 Months Ago": cmbselectprice.SelectedIndex = 2;
                        break;
                    case "6 Months Ago": cmbselectprice.SelectedIndex = 3;
                        break;
                    default: cmbselectprice.SelectedIndex = 0;
                        break;
                }
            }
            else
            {
                cmbpricecategory.SelectedIndex = 0;
                cmbselectprice.SelectedIndex = 0;
            }
            db.dr.Close();
            db.closeConnection();

            //reason
            db.openConnection();
            db.fetch("select reason_code from view_transaction_entry where  trans_no = '" + cmbtransno.Text + "'");
            if (db.dr.Read())
            {
                reason = db.dr["reason_code"].ToString();
                switch (reason)
                {
                    case "AD2": cmbreasoncode.SelectedIndex = 1;
                        break;
                    case "AD3": cmbreasoncode.SelectedIndex = 2;
                        break;
                    case "AD4": cmbreasoncode.SelectedIndex = 3;
                        break;
                    case "AD5": cmbreasoncode.SelectedIndex = 4;
                        break;
                    case "AD1": cmbreasoncode.SelectedIndex = 5;
                        break;
                    case "DS1": cmbreasoncode.SelectedIndex = 1;
                        break;
                    case "DS2": cmbreasoncode.SelectedIndex = 2;
                        break;
                    case "DS3": cmbreasoncode.SelectedIndex = 3;
                        break;
                    case "DS4": cmbreasoncode.SelectedIndex = 4;
                        break;
                    case "DS5": cmbreasoncode.SelectedIndex = 5;
                        break;
                    case "DS6": cmbreasoncode.SelectedIndex = 6;
                        break;
                    case "DS7": cmbreasoncode.SelectedIndex = 7;
                        break;
                    case "LL1": cmbreasoncode.SelectedIndex = 1;
                        break;
                    case "LL2": cmbreasoncode.SelectedIndex = 2;
                        break;
                    case "LL3": cmbreasoncode.SelectedIndex = 3;
                        break;
                    case "LL4": cmbreasoncode.SelectedIndex = 4;
                        break;
                    case "LL5": cmbreasoncode.SelectedIndex = 5;
                        break;
                    case "LL6": cmbreasoncode.SelectedIndex = 6;
                        break;
                    case "LL7": cmbreasoncode.SelectedIndex = 7;
                        break;
                    case "PR1": cmbreasoncode.SelectedIndex = 1;
                        break;
                    case "PR2": cmbreasoncode.SelectedIndex = 2;
                        break;
                    case "PR3": cmbreasoncode.SelectedIndex = 3;
                        break;
                    case "PR4": cmbreasoncode.SelectedIndex = 4;
                        break;
                    case "PR5": cmbreasoncode.SelectedIndex = 5;
                        break;
                    case "PR6": cmbreasoncode.SelectedIndex = 6;
                        break;
                    case "PR7": cmbreasoncode.SelectedIndex = 7;
                        break;
                    case "SD1": cmbreasoncode.SelectedIndex = 1;
                        break;
                    case "WW1": cmbreasoncode.SelectedIndex = 1;
                        break;
                    case "SL1": cmbreasoncode.SelectedIndex = 1;
                        break;
                    case "SL2": cmbreasoncode.SelectedIndex = 2;
                        break;
                    case "SL3": cmbreasoncode.SelectedIndex = 3;
                        break;
                    case "SL4": cmbreasoncode.SelectedIndex = 4;
                        break;
                    case "SL5": cmbreasoncode.SelectedIndex = 5;
                        break;
                    case "SL6": cmbreasoncode.SelectedIndex = 6;
                        break;
                    case "SL7": cmbreasoncode.SelectedIndex = 7;
                        break;
                    case "SL8": cmbreasoncode.SelectedIndex = 8;
                        break;
                    default: cmbreasoncode.SelectedIndex = 0;
                        break;
                }
            }
            else
            {
                cmbreasoncode.Items.Add("");
                cmbreasoncode.SelectedIndex = 0;
            }
            db.dr.Close();
            db.closeConnection();

            //Comment
            db.openConnection();
            db.fetch("select comment from view_transaction_entry where  trans_no = '" + cmbtransno.Text + "'");
            if (db.dr.Read())
            {
                txtcomment.Text = db.dr["comment"].ToString();
            }
            else
            {
                txtcomment.Text = "";
            }
            db.dr.Close();
            db.closeConnection();


        }

        private void txtdocumentNo_TextChanged(object sender, EventArgs e)
        {
            txtdocumentNo.BackColor = Color.White;
        }

        private void cmbsourceWH_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbsourceWH.BackColor = Color.White;
        }

        private void cmbselectprice_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbselectprice.BackColor = Color.White;
        }

        private void cmbDestWH_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbDestWH.BackColor = Color.White;
        }

        private void cmbDestLocation_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbDestLocation.BackColor = Color.White;
        }

        private void cmbreasoncode_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbreasoncode.BackColor = Color.White;
        }

        private void txtcomment_TextChanged(object sender, EventArgs e)
        {
            txtcomment.BackColor = Color.White;
        }

        private void frmInventoryTransactionEntry_Activated_1(object sender, EventArgs e)
        {
            lbluser.Text = Variables.userLogged;
            lblWelcome.Visible = true;
            lbluser.Visible = true;
            foreach (Control c in pnlControlbox.Controls)
            {
                c.BackColor = Color.DodgerBlue;
            }
        }

        #region drag
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            _dragging = true;  // _dragging is your variable flag
            _start_point = new Point(e.X, e.Y);
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            _dragging = false;
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragging)
            {
                Point p = PointToScreen(e.Location);
                Location = new Point(p.X - this._start_point.X, p.Y - this._start_point.Y);
            }
        }
        #endregion

        private void roundButton1_Click(object sender, EventArgs e)
        {
            foreach (Control c in this.pnlSL.Controls)
            {
                if (c is TextBox)
                {
                    c.Text = "";
                }
            }

            foreach (Control c in this.pnlItemDetails.Controls)
            {
                if (c is TextBox)
                {
                    c.Text = "";
                }
            }

            lvlListItem.Items.Clear();

            pnlSL.Enabled = false;
            groupBoxTransactionDetails.Enabled = true;
            cmbtransno.Focus();
        }

        #region PRINTING

        private void btnprint_Click(object sender, EventArgs e)
        {
            frmPrint print = new frmPrint();
            print.Owner = this;
            print.ShowDialog();
        }

        #endregion

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            //progressBar1.Value = 0;
            string[][][] items;

            items = new string[rows][][];
            for (int i = 0; i < rows; i++)
            {
                items[i] = new string[7][];
                for (int j = 0; j < 7; j++)
                {
                    items[i][j] = new string[1];
                }
            }
            Global globals = new Global();
            globals.openConnection();
            int x = 0;
            globals.fetch("select item_code, item_description, location_code, cases, pieces, pieces_per_unit, priceperpiece, linevalue from view_inventory_dummy where transaction = '" + e.Argument.ToString() + "'");
            while (globals.dr.Read())
            {
                items[x][0][0] = globals.dr["item_code"].ToString();
                items[x][1][0] = globals.dr["item_description"].ToString();
                items[x][2][0] = globals.dr["location_code"].ToString();
                items[x][3][0] = globals.dr["cases"].ToString();
                items[x][4][0] = globals.dr["pieces"].ToString();
                items[x][5][0] = globals.dr["priceperpiece"].ToString();
                items[x][6][0] = globals.dr["linevalue"].ToString();
                linePrice += Convert.ToDouble(globals.dr["linevalue"]);
                x++;
                backgroundWorker1.ReportProgress(x);
            }
            globals.dr.Close();
            globals.closeConnection();

            e.Result = items;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            string[][][] items = (e.Result as string[][][]);

            lvlListItem.Items.Clear();

            for (int i = 0; i < rows; i++)
            {
                ListViewItem lvi = new ListViewItem(items[i][0][0]);
                lvi.SubItems.Add("");
                lvi.SubItems.Add(items[i][1][0]);
                lvi.SubItems.Add(items[i][2][0]);
                lvi.SubItems.Add(items[i][3][0]);
                lvi.SubItems.Add(items[i][4][0]);
                lvi.SubItems.Add(items[i][5][0]);
                lvi.SubItems.Add(items[i][6][0]);
                lvlListItem.Items.Add(lvi);
            }

            txtOrderAmount.Text = " " + String.Format("{0:n}", linePrice);
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            panel3.Visible = true;
            foreach (Control c in this.panel3.Controls)
            {
                c.Visible = true;
            }
            progressBar1.Value += 1;

            Thread.Sleep(10);
            if (progressBar1.Value >= rows)
            {
                panel3.Visible = false;
            }
        }

        private void frmInventoryTransactionEntry_Deactivate(object sender, EventArgs e)
        {

            foreach (Control c in pnlControlbox.Controls)
            {
                c.BackColor = Color.Gainsboro;
            }
        }

        private void frmInventoryTransactionEntry_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    roundButton1.PerformClick();
                    break;
                case Keys.Delete:
                    btndelete.PerformClick();
                    btnDeleteLine.PerformClick();
                    break;
                case Keys.P:
                    btnprint.PerformClick();
                    break;
                default:
                    break;
            }
        }

        private void panelMain_Paint(object sender, PaintEventArgs e)
        {

        }

        private void cmbPT_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPT.Text == "")
            {
                txtPrice.Text = "0.00";
                unitPrice = 0;
            }
            else
            {
                txtPrice.Text = String.Format("{0:0.00}", arrPriceType[cmbPT.Text]);
                unitPrice = Convert.ToDouble(txtPrice.Text);
                piecePrice = unitPrice;

            }
        }

        private void btnPost_Click(object sender, EventArgs e)
        {
            posting post = new posting();
            post.Owner = this;
            post.ShowDialog();

            panelMain.Enabled = true;
        }
    }
}
