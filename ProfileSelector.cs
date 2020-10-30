using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AstroModLoader
{
    public partial class ProfileSelector : Form
    {
        private Form1 OurParentForm;
        public ModProfile SelectedProfile; 

        public ProfileSelector()
        {
            InitializeComponent();
            this.listBox1.DrawItem += ListBox1_DrawItem;
        }

        private void ListBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                e = new DrawItemEventArgs(e.Graphics, e.Font, e.Bounds, e.Index, e.State ^ DrawItemState.Selected, AMLPalette.ForeColor, AMLPalette.HighlightColor);
            }

            e.DrawBackground();
            e.Graphics.DrawString(((KeyValuePair<string, ModProfile>)listBox1.Items[e.Index]).Key, e.Font, new SolidBrush(AMLPalette.ForeColor), e.Bounds, StringFormat.GenericDefault);
            e.DrawFocusRectangle();
        }

        private void RefreshTheme()
        {
            this.listBoxPanel.BackColor = AMLPalette.AccentColor;
            this.listBox1.Select();
        }

        private void RefreshBox()
        {
            if (OurParentForm == null) return;
            if (OurParentForm.ModManager.ProfileList.Count > 0)
            {
                listBox1.DisplayMember = "Key";
                listBox1.ValueMember = "Key";
                listBox1.DataSource = new BindingSource(OurParentForm.ModManager.ProfileList, null);
            }
            else
            {
                listBox1.DataSource = null;
            }

            this.RefreshTheme();
            AMLPalette.RefreshTheme(OurParentForm);
            this.listBox1.Refresh();
        }

        private void ProfileSelector_Load(object sender, EventArgs e)
        {
            if (this.Owner is Form1 parentForm)
            {
                OurParentForm = this.Owner as Form1;
                this.Text = OurParentForm.Text;
                RefreshBox();
            }
            this.RefreshTheme();
            AMLPalette.RefreshTheme(this);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string kosherKey = listBox1.SelectedValue as string;
            if (string.IsNullOrEmpty(kosherKey)) return;
            SelectedProfile = OurParentForm.ModManager.ProfileList[kosherKey];
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

        private void newProfileButton_Click(object sender, EventArgs e)
        {
            if (OurParentForm == null) return;

            TextPrompt profileNamePrompt = new TextPrompt();
            profileNamePrompt.StartPosition = FormStartPosition.Manual;
            profileNamePrompt.Location = new Point((this.Location.X + this.Width / 2) - (profileNamePrompt.Width / 2), (this.Location.Y + this.Height / 2) - (profileNamePrompt.Height / 2));
            profileNamePrompt.Text = OurParentForm.Text;
            profileNamePrompt.AllowBrowse = false;
            profileNamePrompt.Width -= 100;
            profileNamePrompt.DisplayText = "Name this profile:";
            if (profileNamePrompt.ShowDialog(this) == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(profileNamePrompt.OutputText)) return;
                if (OurParentForm.ModManager.ProfileList.ContainsKey(profileNamePrompt.OutputText))
                {
                    int dialogResult = AMLUtils.ShowBasicButton(this, "Do you want to overwrite this profile?", "Yes", "No", null);
                    if (dialogResult == 0)
                    {
                        OurParentForm.ModManager.ProfileList[profileNamePrompt.OutputText] = OurParentForm.ModManager.GenerateProfile();
                        OurParentForm.ModManager.SyncConfigToDisk();
                        RefreshBox();
                    }
                    return;
                }
                OurParentForm.ModManager.ProfileList.Add(profileNamePrompt.OutputText, OurParentForm.ModManager.GenerateProfile());
                OurParentForm.ModManager.SyncConfigToDisk();
                RefreshBox();
            }
        }

        private void deleteProfileButton_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedValue == null) return;
            int dialogResult = AMLUtils.ShowBasicButton(this, "Are you sure you want to delete this profile?", "Yes", "No", null);
            if (dialogResult == 0)
            {
                OurParentForm.ModManager.ProfileList.Remove(listBox1.SelectedValue as string);
                OurParentForm.ModManager.SyncConfigToDisk();
                RefreshBox();
            }
        }
    }
}
