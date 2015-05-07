using System;
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

namespace InventoryTransactionEntry
{
    public partial class frmPrint : Form
    {

        Global global = new Global();

        int pageNumber = 0;
        string[] emptyTransactions;

        string user = Variables.userLogged;
        string date = DateTime.Today.ToString("dd/MM/yyyy");
        string time = DateTime.Now.ToString("HH:mm:ss");
        string transType = "";//"STOCKS LOCATION ENTRY";
        string SourceWarehouse = "";//"01(ESI_MAIN)";
        string transNumber = "";//"1234";
        string sourceLocation = "";//"GD";
        string salesman = "";
        string docNumber = "";//"32498";
        string comment = "";//"this is a comment";
        string reason = "";//"UNREGISTERED PO ITEM";
        string HR = "---------------------------------------------------------------------------------------------------------------\n";
        string columns = "ItemCode".PadRight(10) + "Description".PadRight(40) + "LC".PadRight(5) + "Cases".PadRight(8) + "Pieces".PadRight(8) +
                         "Expiry".PadRight(14) + "Price/Piece".PadRight(15) + "Value\n";
        string LC = "";
        int totalLines = 0;
        int piecePerUnit = 0;

        int print = 0;

        int totalCases = 0;
        int totalPieces = 0;
        double totalValue = 0;

        int m = 0;

        int numberOfRows = 0;

        public frmPrint()
        {
            InitializeComponent();
        }

        private void frmPrint_Load(object sender, EventArgs e)
        {
            this.Paint += new PaintEventHandler(pnl2_Paint);

            global.openConnection();
            global.fetch("select trans_no, doc_no from transaction_entry where status = 0");
            while (global.dr.Read())
            {
                ListViewItem lvi = new ListViewItem();
                lvi.SubItems.Add(global.dr["trans_no"].ToString());
                lvi.SubItems.Add(global.dr["doc_no"].ToString());
                listView1.Items.Add(lvi);
            }
            global.dr.Close();
            global.closeConnection();

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

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
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

        private void printDocument1_PrintPage(object sender, PrintPageEventArgs e)
        {
            Font font = new Font("Courier New", 8);
            Brush brush = new SolidBrush(Color.Black);

            float fontHeight = font.GetHeight();
            int index = 0;

            int startX = 40;
            int startY = 40;
            int offset = 0;

            totalLines = 0;

            string[] header = {user,
                               date.PadRight(43) + "Extract Sales. Inc.".PadRight(55) + "Page".PadRight(10) + pageNumber++,
                               time.PadRight(41) + "Inventory Transaction",
                               "Transaction Type:".PadRight(20) + transType.PadRight(40) + "Source Warehouse:".PadRight(20) + SourceWarehouse,
                               "Transaction No.:".PadRight(20) + transNumber.PadRight(40) + "Source Location:".PadRight(20) + sourceLocation,
                               "Transaction Date:".PadRight(20) + date.PadRight(40) + "Salesman:".PadRight(20) + salesman,
                               "Document No.:".PadRight(20) + docNumber.PadRight(40) + "Comment:".PadRight(20) + comment,
                               "Reason:".PadRight(20) + reason.PadRight(20) + "\n",
                               HR,
                               columns,
                               HR
                              };

            for (int i = 0; i < header.Length; i++)
            {
                if (i == 3)
                {
                    e.Graphics.DrawString(header[i], font, brush, startX, startY + (offset + 20));
                    offset += (int)fontHeight + 10;
                }
                else
                {
                    e.Graphics.DrawString(header[i], font, brush, startX, startY + offset);
                }

                offset += (int)fontHeight + 5;
            }

            string[][][] items = new string[numberOfRows][][];
            for (int i = 0; i < numberOfRows; i++)
            {
                items[i] = new string[8][];
                for (int x = 0; x < 8; x++)
                {
                    if (x == 1)
                    {
                        items[i][x] = new string[2];
                    }
                    else
                    {
                        items[i][x] = new string[1];
                    }
                }
            }

            int j = 0;
            char s = ' ';
            global.openConnection();
            global.fetch("select item_code, item_description, location_code, cases, pieces, expiration_date, priceperpiece, linevalue from " +
                         "view_inventory_dummy where transaction = '" + transNumber + "'");

            while (global.dr.Read())
            {
                items[j][0][0] = global.dr["item_code"].ToString();
                //MessageBox.Show(items[j][0][0]);
                if (global.dr["item_description"].ToString().Length > 40)
                {
                    Global global2 = new Global();
                    global2.openConnection();
                    global2.fetch("select pieces_per_unit from item_master where item_code = '" + global.dr["item_code"].ToString() + "'");
                    while (global2.dr.Read())
                    {
                        //MessageBox.Show("piecesperunit: " + global2.dr["pieces_per_unit"].ToString());
                        piecePerUnit = Convert.ToInt32(global2.dr["pieces_per_unit"].ToString());
                        index = global.dr["item_description"].ToString().IndexOf(global2.dr[0].ToString());
                        if (index <= 0)
                        {
                            for (int x = 0; x < global.dr["item_description"].ToString().Length; x++)
                            {
                                if (Char.IsNumber(global.dr["item_description"].ToString()[x]))
                                {
                                    index = x;
                                    break;
                                }
                            }
                            items[j][1][0] = global.dr["item_description"].ToString().Substring(0, index);
                            items[j][1][1] = global.dr["item_description"].ToString().Substring(index, global.dr["item_description"].ToString().Length - index);
                        }
                        else
                        {
                            items[j][1][0] = global.dr["item_description"].ToString().Substring(0, index);
                            items[j][1][1] = global.dr["item_description"].ToString().Substring(index, global.dr["item_description"].ToString().Length - index);
                        }
                    }
                    global2.dr.Close();
                    global2.closeConnection();
                    //MessageBox.Show("descSplit = 1: " + items[j][1][0] +" "+ items[j][1][1]);
                }
                else
                {
                    items[j][1][0] = global.dr["item_description"].ToString();
                    //MessageBox.Show("descSplit = 0: "+items[j][1][0]);
                }
                items[j][2][0] = global.dr["location_code"].ToString();
                items[j][3][0] = global.dr["cases"].ToString();
                items[j][4][0] = global.dr["pieces"].ToString();
                items[j][5][0] = global.dr["expiration_date"].ToString();
                items[j][6][0] = global.dr["priceperpiece"].ToString();
                items[j][7][0] = global.dr["linevalue"].ToString();
                j++;
            }
            global.closeConnection();

            //for (int l = 0; l < numberOfRows; l++)
            //{
            //    totalCases += Convert.ToInt32(items[l][3][0]);
            //    totalPieces += Convert.ToInt32(items[l][4][0]);
            //    totalValue += Convert.ToDouble(items[l][7][0]);
            //}

            for (; m < numberOfRows; m++)
            {

                if (items[m][1][1] != null)
                {
                    e.Graphics.DrawString(items[m][0][0].PadRight(10) +
                                          items[m][1][0].PadRight(40) +
                                          items[m][2][0].PadRight((8 + items[m][2][0].Length) - items[m][3][0].Length) +
                                          items[m][3][0].PadRight((9 + items[m][3][0].Length) - items[m][4][0].Length) +
                                          items[m][4][0].PadRight((13 + items[m][4][0].Length) - items[m][5][0].Length) +
                                          items[m][5][0].PadRight((14 + items[m][5][0].Length) - items[m][6][0].Length) +
                                          items[m][6][0].PadRight((15 + items[m][6][0].Length) - items[m][7][0].Length) +
                                          items[m][7][0],
                                          font, brush, startX, startY + offset);
                    offset += (int)fontHeight + 5;

                    e.Graphics.DrawString("".PadRight(10) + items[m][1][1], font, brush, startX, startY + offset);
                    offset += (int)fontHeight + 5;
                    totalLines += 2;
                    totalCases += Convert.ToInt32(items[m][3][0]);
                    totalPieces += Convert.ToInt32(items[m][4][0]);
                    totalValue += Convert.ToDouble(items[m][7][0]);
                }
                else
                {
                    e.Graphics.DrawString(items[m][0][0].PadRight(10) +
                                          items[m][1][0].PadRight(40) +
                                          items[m][2][0].PadRight((8 + items[m][2][0].Length) - items[m][3][0].Length) +
                                          items[m][3][0].PadRight((9 + items[m][3][0].Length) - items[m][4][0].Length) +
                                          items[m][4][0].PadRight((13 + items[m][4][0].Length) - items[m][5][0].Length) +
                                          items[m][5][0].PadRight((14 + items[m][5][0].Length) - items[m][6][0].Length) +
                                          items[m][6][0].PadRight((15 + items[m][6][0].Length) - items[m][7][0].Length) +
                                          items[m][7][0],
                                          font, brush, startX, startY + offset);
                    offset += (int)fontHeight + 5;
                    totalLines += 1;
                    totalCases += Convert.ToInt32(items[m][3][0]);
                    totalPieces += Convert.ToInt32(items[m][4][0]);
                    totalValue += Convert.ToDouble(items[m][7][0]);
                }
                if (m == (numberOfRows - 1))
                {
                    offset -= 10;
                }

                //print to next page
                if (totalLines % 48 == 0)
                {
                    e.HasMorePages = true;
                    totalLines = 0;
                    m++;
                    return;
                }
                else
                {
                    e.HasMorePages = false;
                }

            }

            e.Graphics.DrawString("".PadRight(60 - "________".Length) +
                "________".PadRight(("________".Length + 9) - "________".Length) +
                "________".PadRight((42 + "________".Length) - "______________".Length) +
                "______________",
                font, brush, startX, startY + offset);
            offset += (int)fontHeight + 5;

            if (totalValue > 999 && totalValue <= 999999.99)
            {
                e.Graphics.DrawString("".PadRight(60 - totalCases.ToString().Length) +
                    totalCases.ToString().PadRight((totalCases.ToString().Length + 9) - totalPieces.ToString().Length) +
                    totalPieces.ToString().PadRight((42 + totalPieces.ToString().Length) - (1 + totalValue.ToString().Length)) +
                    String.Format("{0:n}", totalValue),
                    font, brush, startX, startY + offset);
                offset += (int)fontHeight + 60;
            }
            else if (totalValue > 999999.99 && totalValue <= 999999999.99)
            {
                e.Graphics.DrawString("".PadRight(60 - totalCases.ToString().Length) +
                    totalCases.ToString().PadRight((totalCases.ToString().Length + 9) - totalPieces.ToString().Length) +
                    totalPieces.ToString().PadRight((42 + totalPieces.ToString().Length) - (2 + totalValue.ToString().Length)) +
                    String.Format("{0:n}", totalValue),
                    font, brush, startX, startY + offset);
                offset += (int)fontHeight + 60;
            }
            else if (totalValue > 999999999.99)
            {
                e.Graphics.DrawString("".PadRight(60 - totalCases.ToString().Length) +
                    totalCases.ToString().PadRight((totalCases.ToString().Length + 9) - totalPieces.ToString().Length) +
                    totalPieces.ToString().PadRight((42 + totalPieces.ToString().Length) - (3 + totalValue.ToString().Length)) +
                    String.Format("{0:n}", totalValue),
                    font, brush, startX, startY + offset);
                offset += (int)fontHeight + 60;
            }
            else
            {
                e.Graphics.DrawString("".PadRight(60 - totalCases.ToString().Length) +
                    totalCases.ToString().PadRight((totalCases.ToString().Length + 9) - totalPieces.ToString().Length) +
                    totalPieces.ToString().PadRight((42 + totalPieces.ToString().Length) - totalValue.ToString().Length) +
                    totalValue,
                    font, brush, startX, startY + offset);
                offset += (int)fontHeight + 60;
            }

            e.Graphics.DrawString("".PadRight(10) +
                "_________________________".PadRight(35) +
                "_________________________".PadRight(35) +
                "_________________________",
                font, brush, startX, startY + offset);
            offset += (int)fontHeight + 5;

            e.Graphics.DrawString("".PadRight(16) +
                "Prepared By:".PadRight(36) +
                "Checked By:".PadRight(34) +
                "Received By:",
                font, brush, startX, startY + offset);
            offset += (int)fontHeight + 5;
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

        private void btnprint_Click(object sender, EventArgs e)
        {
            print = 1;
            setupPrint();
        }

        private void btnprintpreview_Click(object sender, EventArgs e)
        {
            print = 0;
            setupPrint();
        }

        private void setupPrint()
        {
            foreach (ListViewItem lvi in this.listView1.Items)
            {
                PrintDialog printDialog1 = new PrintDialog();
                PrintDocument printDocument1 = new PrintDocument();
                PrintPreviewDialog printPreviewDialog1 = new PrintPreviewDialog();

                m = 0;
                pageNumber = 1;
                totalCases = 0;
                totalPieces = 0;
                totalValue = 0;

                if (lvi.Checked)
                {
                    transNumber = lvi.SubItems[1].Text;
                    docNumber = lvi.SubItems[2].Text;
                    date = DateTime.Today.ToString("dd/MM/yyyy");
                    time = DateTime.Now.ToString("HH:mm:ss");

                    global.openConnection();
                    global.fetch("select transaction_type, source_warehouse, source_location, source_salesman, reason, comment from " +
                                 "view_transaction_entry where trans_no = '" + transNumber + "' and doc_no = '" + docNumber + "'");
                    while (global.dr.Read())
                    {
                        transType = global.dr["transaction_type"].ToString();
                        SourceWarehouse = global.dr["source_warehouse"].ToString();
                        sourceLocation = global.dr["source_location"].ToString();
                        reason = global.dr["reason"].ToString();
                        comment = global.dr["comment"].ToString();
                    }
                    global.dr.Close();

                    global.fetch("select location_code from " +
                                 "view_inventory_dummy where transaction = '" + transNumber + "'");
                    while (global.dr.Read())
                    {
                        LC = global.dr["location_code"].ToString();
                    }
                    global.dr.Close();

                    global.fetch("select count(*) from view_inventory_dummy where transaction = '" + transNumber + "'");
                    if (global.dr.Read())
                    {
                        numberOfRows = Convert.ToInt32(global.dr[0]);
                    }
                    global.dr.Close();
                    global.closeConnection();

                    printDocument1.PrintPage += new PrintPageEventHandler(printDocument1_PrintPage);
                    printPreviewDialog1.Document = printDocument1;

                    //print preview dialog size and position
                    ((Form)printPreviewDialog1).FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                    ((Form)printPreviewDialog1).Width = frmInventoryTransactionEntry.ActiveForm.Owner.Width - 10;
                    ((Form)printPreviewDialog1).Height = frmInventoryTransactionEntry.ActiveForm.Owner.Height - 55;
                    ((Form)printPreviewDialog1).StartPosition = FormStartPosition.Manual;
                    ((Form)printPreviewDialog1).DesktopLocation = new Point((frmInventoryTransactionEntry.ActiveForm.Owner.DesktopLocation.X + 5), frmInventoryTransactionEntry.ActiveForm.Owner.DesktopLocation.Y + 50);
                    printPreviewDialog1.PrintPreviewControl.AutoZoom = false;
                    printPreviewDialog1.PrintPreviewControl.Zoom = 1.0;

                    if (print == 1)
                    {
                        printDialog1.Document = printDocument1;
                        printDocument1.Print();

                        global.openConnection();
                        global.InUpDel("update transaction_entry set status = 1 where trans_no = '" + transNumber + "' and doc_no = '" + docNumber + "'");
                        global.closeConnection();
                    }
                    else
                    {
                        ((ToolStripButton)((ToolStrip)printPreviewDialog1.Controls[1]).Items[0]).Enabled = false;
                        printPreviewDialog1.ShowDialog();
                    }
                }
            }
        }


    }
}
