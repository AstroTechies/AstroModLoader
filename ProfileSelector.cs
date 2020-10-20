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
    public partial class ProfileSelector : Form
    {
        public ModProfile SelectedProfile; 

        public ProfileSelector()
        {
            InitializeComponent();
        }

        private void ProfileSelector_Load(object sender, EventArgs e)
        {
            if (this.Owner is Form1 parentForm)
            {
                listBox1.DisplayMember = "Key";
                listBox1.ValueMember = "Value";
                listBox1.DataSource = new BindingSource(parentForm.ModManager.ProfileList, null);
                this.Text = parentForm.Text;
                AMLPalette.RefreshTheme(parentForm);
            }
            AMLPalette.RefreshTheme(this);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedProfile = listBox1.SelectedValue as ModProfile;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            SelectedProfile = null;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
