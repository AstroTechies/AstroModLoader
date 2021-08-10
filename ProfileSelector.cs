using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
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

            Rectangle decidedNewBounds = new Rectangle(e.Bounds.Location.X + (AMLPalette.BorderPenWidth / 2), e.Bounds.Location.Y + (AMLPalette.BorderPenWidth / 2), e.Bounds.Width - AMLPalette.BorderPenWidth, e.Bounds.Height);

            e.Graphics.FillRectangle(new SolidBrush(e.BackColor), decidedNewBounds);
            e.Graphics.DrawString(decidedText, e.Font, new SolidBrush(AMLPalette.ForeColor), decidedNewBounds, StringFormat.GenericDefault);
            if ((e.State & DrawItemState.Focus) == DrawItemState.Focus && (e.State & DrawItemState.NoFocusRect) != DrawItemState.NoFocusRect)
            {
                ControlPaint.DrawFocusRectangle(e.Graphics, decidedNewBounds, e.ForeColor, e.BackColor);
            }
        }

        private void RefreshTheme()
        {
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
                int desiredFullHeight = (int)(Math.Ceiling(listBox1.Height / desiredItemHeight) * desiredItemHeight);

                int maxEntries = listBox1.Height / (int)desiredItemHeight;
                if (maxEntries <= OurParentForm.ModManager.ProfileList.Count)
                {
                    desiredFullHeight = (int)desiredItemHeight * (OurParentForm.ModManager.ProfileList.Count + 1);
                }

                listBox1.Height = desiredFullHeight;
                listBox1.ItemHeight = (int)desiredItemHeight;
                statusLabel.Location = new Point(statusLabel.Location.X, listBox1.Location.Y + listBox1.Height + (int)desiredItemHeight);
                this.Height = statusLabel.Location.Y + ((int)desiredItemHeight * 7);
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

        private async Task ForceLoadSelectedProfile()
        {
            if (SelectedProfile == null)
            {
                this.ShowBasicButton("Please select a profile to load it.", "OK", null, null);
                return;
            }
            OurParentForm.ModManager.ApplyProfile(SelectedProfile);
            await OurParentForm.ModManager.FullUpdate();
            OurParentForm.FullRefresh();
            statusLabel.Text = "Successfully loaded from profile.";
            ForceRefreshSelectedProfile();
        }

        private async void listBox1_DoubleClick(object sender, EventArgs e)
        {
            await ForceLoadSelectedProfile();
        }

        private async void okButton_Click(object sender, EventArgs e)
        {
            await ForceLoadSelectedProfile();
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

        private void ForceDeleteProfilePrompt()
        {
            if (listBox1.SelectedValue == null)
            {
                this.ShowBasicButton("Please select a profile to delete it.", "OK", null, null);
                return;
            }
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

        private void ForceExportProfile()
        {
            if (listBox1.SelectedValue == null || SelectedProfile == null)
            {
                this.ShowBasicButton("Please select a profile to export it as a .zip file.", "OK", null, null);
                return;
            }

            var dialog = new SaveFileDialog();
            dialog.Filter = "ZIP files (*.zip)|*.zip|All files (*.*)|*.*";
            dialog.Title = "Export a profile";
            dialog.RestoreDirectory = true;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string targetFolderPath = Path.Combine(Path.GetTempPath(), "AstroModLoader", "export");
                Directory.CreateDirectory(targetFolderPath);

                ModProfile creatingProfile = new ModProfile();
                creatingProfile.ProfileData = new Dictionary<string, Mod>();
                creatingProfile.Name = listBox1.SelectedValue as string;
                creatingProfile.Info = "Exported by " + AMLUtils.UserAgent + " at " + DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffK");

                List<KeyValuePair<string, Mod>> plannedOrdering = new List<KeyValuePair<string, Mod>>();
                foreach (KeyValuePair<string, Mod> entry in SelectedProfile.ProfileData)
                {
                    if (entry.Value.Enabled) plannedOrdering.Add(entry);
                }
                plannedOrdering = new List<KeyValuePair<string, Mod>>(plannedOrdering.OrderBy(o => o.Value.Priority).ToList());

                for (int i = 0; i < plannedOrdering.Count; i++)
                {
                    plannedOrdering[i].Value.Priority = i + 1;
                    creatingProfile.ProfileData[plannedOrdering[i].Key] = plannedOrdering[i].Value;

                    // Copy mod pak to the zip as well
                    string onePathOnDisk = OurParentForm.ModManager.GetPathOnDisk(plannedOrdering[i].Value, plannedOrdering[i].Key);
                    if (!string.IsNullOrEmpty(onePathOnDisk)) File.Copy(onePathOnDisk, Path.Combine(targetFolderPath, Path.GetFileName(onePathOnDisk)));
                }

                File.WriteAllBytes(Path.Combine(targetFolderPath, "profile1.json"), Encoding.UTF8.GetBytes(AMLUtils.SerializeObject(creatingProfile)));

                ZipFile.CreateFromDirectory(targetFolderPath, dialog.FileName);
                Directory.Delete(targetFolderPath, true);

                RefreshBox();
                statusLabel.Text = "Successfully exported profile.";
            }
        }

        private async void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete) ForceDeleteProfilePrompt();
            if (e.KeyCode == Keys.Enter) await ForceLoadSelectedProfile();
            if (e.KeyCode == Keys.X) ForceExportProfile();
        }

        private void deleteProfileButton_Click(object sender, EventArgs e)
        {
            ForceDeleteProfilePrompt();
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
