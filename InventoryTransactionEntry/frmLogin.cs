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
    public partial class frmLogin : Form
    {

        Global db = new Global();

        int log = 0;

        public frmLogin()
        {
            InitializeComponent();
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {
            txtuser.CharacterCasing = CharacterCasing.Upper;
            this.ControlBox = false;

            this.Paint += new PaintEventHandler(pnl2_Paint);

            if (txtuser.Focused == false)
            {
                txtuser.Focus();
            }
        }

        private void pnl2_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, ClientRectangle,
                                         Color.Gray, 5, ButtonBorderStyle.Solid,
                                         Color.Gray, 5, ButtonBorderStyle.Solid,
                                         Color.Gray, 5, ButtonBorderStyle.Solid,
                                         Color.Gray, 5, ButtonBorderStyle.Solid);
        }

        private void btncancel_Click(object sender, EventArgs e)
        {
            Application.Exit();

        }

        private void btnlogin_Click(object sender, EventArgs e)
        {
            string user = txtuser.Text;
            string pass = txtpass.Text;

            lblPrompt.Text = "";
            lblPrompt2.Text = "";

            if (pass == "" && user == "")
            {
                txtuser.Focus();
            }
            else if (user == "")
            {
                lblPrompt.Text = "Username field Empty!";
                lblPrompt.Left = (this.ClientSize.Width - lblPrompt.Width) / 2;
                txtuser.Focus();
            }
            else if (pass == "")
            {
                lblPrompt.Text = "Password field Empty!";
                lblPrompt.Left = (this.ClientSize.Width - lblPrompt.Width) / 2;
                txtpass.Focus();
            } 
            else
            {
                db.openConnection();

                db.fetch("select username from users where username = '"+user+"'");
                if (db.dr.Read())
                {
                    db.dr.Close();
                    db.closeConnection();
                    checkPass(user, pass);
                }
                else
                {
                    lblPrompt.Text = "Access Denied!";
                    lblPrompt.Left = (this.ClientSize.Width - lblPrompt.Width) / 2;
                    lblPrompt2.Text = "Invalid Username or Password!";
                    lblPrompt2.Left = (this.ClientSize.Width - lblPrompt2.Width) / 2;
                    txtuser.Text = "";
                    txtpass.Text = "";
                    txtuser.Focus();
                }

                db.closeConnection();
            }
            //else 
            //{
            //    db.openConnection();
            //    db.fetch("select strcmp((select password from users where username = '"+user+"'), sha1('"+pass+"')) as result");
            //    if (db.dr.Read())
            //    {
            //        if (db.dr[0] != null)
            //        {
            //            if (Convert.ToInt32(db.dr["result"]) == 0)
            //            {
            //                //this.Owner.Activate();
            //                this.Close();
            //            }
            //            else
            //            {
            //                lblPrompt.Text = "Access Denied!";
            //                lblPrompt.Left = (this.ClientSize.Width - lblPrompt.Width) / 2;
            //                lblPrompt2.Text = "Invalid Username or Password!";
            //                lblPrompt2.Left = (this.ClientSize.Width - lblPrompt.Width) / 2;
            //            }
            //        }
            //    }
            //    db.dr.Close();
            //    db.closeConnection();

            //}

        }

        private void checkPass(string user, string pass)
        {
            db.openConnection();
            db.fetch("select strcmp((select password from users where username = '" + user + "'), sha1('" + pass + "')) as result");
            if (db.dr.Read())
            {
                    if (Convert.ToInt32(db.dr["result"]) == 0)
                    {
                        log = 1;
                    }
                    else
                    {
                        lblPrompt.Text = "Access Denied!";
                        lblPrompt.Left = (this.ClientSize.Width - lblPrompt.Width) / 2;
                        lblPrompt2.Text = "Invalid Username or Password!";
                        lblPrompt2.Left = (this.ClientSize.Width - lblPrompt2.Width) / 2;
                    }
            }
            db.dr.Close();
            db.closeConnection();

            if (log == 1)
            {
                db.openConnection();
                db.fetch("select username from users where username = '" + user + "' and password = sha1('" + pass + "')");
                if (db.dr.Read())
                {
                    Variables.userLogged = db.dr[0].ToString();
                    this.Close();
                    this.Owner.Activate();
                }
                db.dr.Close();
                db.closeConnection();
            }
        }

        private void frmLogin_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (log == 0)
            {
                Application.Exit();
            }
            else
            {

            }
        }

        private void label4_Click(object sender, EventArgs e)
        {
            
        }
    }
}
