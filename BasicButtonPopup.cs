using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AstroModLoader
{
    public partial class BasicButtonPopup : Form
    {
        public string DisplayText;
        public int ResultButton;

        public BasicButtonPopup()
        {
            InitializeComponent();
        }

        private void BasicButtonPopup_Load(object sender, EventArgs e)
        {
            mainLabel.Text = DisplayText;
            if (this.Owner is Form1 parentForm)
            {
                this.Text = parentForm.Text;
                AMLPalette.RefreshTheme(parentForm);
            }
            AMLPalette.RefreshTheme(this);

            using (Graphics g = CreateGraphics())
            {
                SizeF sz = g.MeasureString(mainLabel.Text, mainLabel.Font);
                this.Width = (int)Math.Ceiling(sz.Width) + 100;
            }

            mainLabel.Select();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ResultButton = 0;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ResultButton = 1;
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ResultButton = 2;
            this.Close();
        }
    }
}
