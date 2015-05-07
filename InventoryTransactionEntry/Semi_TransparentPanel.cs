using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace InventoryTransactionEntry
{
    class Semi_TransparentPanel : Panel
    {

        Label lb;

        public Label DesLabel
        {

            get { return lb; }

        }



        public Semi_TransparentPanel()

            : base()
        {

            //lb = new Label();

            //lb.Text = "Add text here!";

            //lb.AutoSize = false;

            //lb.Dock = DockStyle.Top;

            //lb.Height = 50;

            //bar = new ProgressBar();

            //bar.Dock = DockStyle.Top;

            //bar.Value = 50;

            //this.Controls.Add(bar);

            //this.Controls.Add(lb);

        }



        const int WS_EX_TRANSPARENT = 0x00000020;



        protected override CreateParams CreateParams
        {

            get
            {

                CreateParams cp = base.CreateParams;

                cp.ExStyle |= WS_EX_TRANSPARENT;

                return cp;

            }

        }



        protected override void OnPaintBackground(PaintEventArgs e)
        {

            //do not paint the background

        }



        protected override void OnPaint(PaintEventArgs e)
        {

            SolidBrush brush = new SolidBrush(Color.FromArgb(128, this.BackColor));//semi-transparent color.

            e.Graphics.FillRectangle(brush, new Rectangle(0, 0, this.Width, this.Height));

        }

    }
}
