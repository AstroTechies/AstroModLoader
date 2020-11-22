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
        public ModProfile SelectedProfile = null; 

        public ProfileSelector()
        {
            InitializeComponent();
            this.statusLabel.Text = "";
            this.listBox1.DrawItem += ListBox1_DrawItem;
        }

        private int GetMaxListBoxEntryHeight(Font font)
        {
            int maxHeight = -1;
            foreach (KeyValuePair<string, ModProfile> entry in listBox1.Items)
            {
                int testingHeight = TextRenderer.MeasureText(entry.Key, font).Height;
                if (testingHeight > maxHeight) maxHeight = testingHeight;
            }
            return maxHeight;
        }

        private void ListBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            string decidedText = ((KeyValuePair<string, ModProfile>)listBox1.Items[e.Index]).Key;
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                e = new DrawItemEventArgs(e.Graphics, e.Font, e.Bounds, e.Index, e.State ^ DrawItemState.Selected, AMLPalette.ForeColor, AMLPalette.HighlightColor);
            }

            e.DrawBackground();
            e.Graphics.DrawString(decidedText, e.Font, new SolidBrush(AMLPalette.ForeColor), e.Bounds, StringFormat.GenericDefault);
            e.DrawFocusRectangle();
        }

        private void RefreshTheme()
        {
            this.listBoxPanel.BackColor = AMLPalette.AccentColor;
            this.listBox1.Select();
        }

        private void RefreshBox()
        {
            string currentlySelected = listBox1.SelectedValue as string;
            if (OurParentForm == null) return;
            if (OurParentForm.ModManager.ProfileList == null) OurParentForm.ModManager.ProfileList = new Dictionary<string, ModProfile>();
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

            double desiredItemHeight = GetMaxListBoxEntryHeight(listBox1.Font);
            if (desiredItemHeight > 5)
            {
                listBox1.Height = (int)(Math.Ceiling(listBox1.Height / desiredItemHeight) * desiredItemHeight);
                listBox1.ItemHeight = (int)desiredItemHeight;
                listBoxPanel.Height = listBox1.Height + 2;
                statusLabel.Location = new Point(statusLabel.Location.X, listBoxPanel.Location.Y + listBoxPanel.Height + 12);
                this.Height = statusLabel.Location.Y + 100;
            }

            this.RefreshTheme();
            this.listBox1.Refresh();

            for (int i = 0; i < this.listBox1.Items.Count; i++)
            {
                object thisItem = this.listBox1.Items[i];
                if (thisItem is KeyValuePair<string, ModProfile> thisKVP)
                {
                    if (thisKVP.Key == currentlySelected)
                    {
                        this.listBox1.SelectedIndex = i;
                        break;
                    }
                }
            }
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
            this.AdjustFormPosition();
        }

        private void ForceRefreshSelectedProfile()
        {
            string kosherKey = listBox1.SelectedValue as string;
            if (string.IsNullOrEmpty(kosherKey) || !OurParentForm.ModManager.ProfileList.ContainsKey(kosherKey))
            {
                SelectedProfile = null;
                return;
            }
            SelectedProfile = OurParentForm.ModManager.ProfileList[kosherKey];
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            statusLabel.Text = "";
            ForceRefreshSelectedProfile();
        }

        private void ForceLoadSelectedProfile()
        {
            if (SelectedProfile == null)
            {
                this.ShowBasicButton("Please select a profile to load it.", "OK", null, null);
                return;
            }
            OurParentForm.ModManager.ApplyProfile(SelectedProfile);
            OurParentForm.ModManager.FullUpdate();
            OurParentForm.FullRefresh();
            statusLabel.Text = "Successfully loaded from profile.";
            ForceRefreshSelectedProfile();
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            ForceLoadSelectedProfile();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            ForceLoadSelectedProfile();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            SelectedProfile = null;
            this.DialogResult = DialogResult.OK;
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
                    int dialogResult = this.ShowBasicButton("Do you want to overwrite this profile?", "Yes", "No", null);
                    if (dialogResult == 0)
                    {
                        OurParentForm.ModManager.ProfileList[profileNamePrompt.OutputText] = OurParentForm.ModManager.GenerateProfile();
                        OurParentForm.ModManager.SyncConfigToDisk();
                        RefreshBox();
                        statusLabel.Text = "Successfully overwrote profile.";
                    }
                    else
                    {
                        statusLabel.Text = "";
                    }
                    return;
                }
                OurParentForm.ModManager.ProfileList.Add(profileNamePrompt.OutputText, OurParentForm.ModManager.GenerateProfile());
                OurParentForm.ModManager.SyncConfigToDisk();
                RefreshBox();
                statusLabel.Text = "Successfully created profile.";
            }
        }

        private void deleteProfileButton_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedValue == null) return;
            int dialogResult = this.ShowBasicButton("Are you sure you want to delete this profile?", "Yes", "No", null);
            if (dialogResult == 0)
            {
                OurParentForm.ModManager.ProfileList.Remove(listBox1.SelectedValue as string);
                OurParentForm.ModManager.SyncConfigToDisk();
                RefreshBox();
                statusLabel.Text = "Successfully deleted profile.";
            }
            ForceRefreshSelectedProfile();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (SelectedProfile == null)
            {
                this.ShowBasicButton("Please select a profile to save to it.", "OK", null, null);
                return;
            }
            OurParentForm.ModManager.ProfileList[listBox1.SelectedValue as string] = OurParentForm.ModManager.GenerateProfile();
            OurParentForm.ModManager.SyncConfigToDisk();
            RefreshBox();
            statusLabel.Text = "Successfully saved to profile.";
            ForceRefreshSelectedProfile();
        }
    }
}
