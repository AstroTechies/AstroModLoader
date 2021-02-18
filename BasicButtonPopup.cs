using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace AstroModLoader
{
    public partial class BasicButtonPopup : Form
    {
        public string DisplayText;
        public int ResultButton;
        public string PageToVisit = "";

        public BasicButtonPopup()
        {
            InitializeComponent();
        }

        private void BasicButtonPopup_Load(object sender, EventArgs e)
        {
            mainLabel.Text = DisplayText;

            using (Graphics g = CreateGraphics())
            {
                SizeF sz = g.MeasureString(mainLabel.Text, mainLabel.Font);
                this.Width = (int)(Math.Ceiling(sz.Width) * 1.4f);
                mainLabel.Height = (int)Math.Ceiling(sz.Height) * 2;
                this.Height = (int)Math.Ceiling(sz.Height) * 7;
            }

            if (this.Owner is Form1 parentForm) this.Text = parentForm.Text;
            this.AdjustFormPosition();
            AMLPalette.RefreshTheme(this);

            mainLabel.Select();
        }

        private void BasicButtonPopup_KeyDown(object sender, KeyEventArgs e)
        {
            if (!string.IsNullOrEmpty(PageToVisit)) return;
            if (button3.Visible) return;

            if (e.KeyCode == Keys.Return)
            {
                ResultButton = 0;
                this.Close();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                ResultButton = 1;
                this.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ResultButton = 0;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(PageToVisit))
            {
                ResultButton = 1;
                this.Close();
            }
            else
            {
                Process.Start(PageToVisit);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ResultButton = 2;
            this.Close();
        }
    }
}
