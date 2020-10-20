using AstroModIntegrator;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace AstroModLoader
{
    public partial class Form1 : Form
    {
        public ModHandler ModManager;
        public TableHandler TableManager;

        public DataGridView dataGridView1;
        public Panel footer;
        public Panel panel1;

        public Form1()
        {
            InitializeComponent();
            this.Text = "AstroModLoader v" + Application.ProductVersion;

            // Enable double buffering to look nicer
            if (!SystemInformation.TerminalServerSession)
            {
                Type ourGridType = dataGridView1.GetType();
                PropertyInfo pi = ourGridType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
                pi.SetValue(dataGridView1, true, null);
                this.DoubleBuffered = true;
            }
            dataGridView1.Select();

            ModManager = new ModHandler(this);
            TableManager = new TableHandler(dataGridView1, ModManager);

            //SizeChanged += frm_sizeChanged;
            //dataGridView1.DataError += DataGridView1_DataError;
            dataGridView1.CellValueChanged += DataGridView1_CellValueChanged;
            dataGridView1.CellContentClick += DataGridView1_CellContentClick;
            dataGridView1.DataBindingComplete += DataGridView1_DataBindingComplete;
            dataGridView1.CellEndEdit += DataGridView1_CellEndEdit;
            dataGridView1.SelectionChanged += new EventHandler(DataGridView1_SelectionChanged);
            footer.Paint += Footer_Paint;
            AMLPalette.RefreshTheme(this);
        }

        private void DataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;

            Type t = dataGridView1.GetType();
            FieldInfo viewSetter = t.GetField("latestEditingControl", BindingFlags.Default | BindingFlags.NonPublic | BindingFlags.Instance);
            viewSetter.SetValue(dataGridView1, null);
        }

        private void DataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs anError)
        {
            MessageBox.Show("Error happened " + anError.Context.ToString());
        }

        private void Footer_Paint(object sender, PaintEventArgs e)
        {
            using (Pen p = new Pen(Color.FromArgb(60, 60, 60), 1))
            {
                e.Graphics.DrawLine(p, new Point(0, 0), new Point(footer.ClientSize.Width, 0));
            }
        }

        private void DataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            foreach (DataGridViewRow row in this.dataGridView1.Rows)
            {
                if (row.Tag is Mod taggedMod)
                {
                    taggedMod.Enabled = (bool)row.Cells[0].Value;
                    if (row.Cells[2].Value is string strVal)
                    {
                        taggedMod.InstalledVersion = new Version((string)row.Cells[2].Value);
                    }
                }
            }
            ModManager.FullUpdate();
        }

        private void DataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                if (column is DataGridViewCheckBoxColumn)
                {
                    column.ReadOnly = false;
                    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
                }
            }

            ForceResize();
        }

        private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            dataGridView1.EndEdit();
            dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void DataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            Mod selectedMod = TableManager.GetCurrentlySelectedMod();
            if (selectedMod == null)
            {
                this.modInfo.Text = "";
                return;
            }
            string kosherDescription = string.IsNullOrEmpty(selectedMod.ModData.Description) ? "N/A" : selectedMod.ModData.Description;
            if (kosherDescription.Length > 80) kosherDescription = kosherDescription.Substring(0, 80) + "...";
            string kosherSync = "N/A";
            switch(selectedMod.ModData.Sync)
            {
                case SyncMode.ClientOnly:
                    kosherSync = "Client only";
                    break;
                case SyncMode.ServerOnly:
                    kosherSync = "Server only";
                    break;
                case SyncMode.ServerAndClient:
                    kosherSync = "Server and client";
                    break;
            }

            this.modInfo.Text = "Name: " + selectedMod.Name + "\nDescription: " + kosherDescription + "\nSync: " + kosherSync;
        }

        public void ForceResize()
        {
            //dataGridView1.AutoResizeColumns();
            footer.Width = this.Width;
        }

        private void frm_sizeChanged(object sender, EventArgs e)
        {
            ForceResize();
        }

        private void refresh_Click(object sender, EventArgs e)
        {
            ModManager.SyncModsFromDisk();
            ModManager.SyncConfigFromDisk();
            TableManager.Refresh();
            AMLPalette.RefreshTheme(this);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
        }

        private void playButton_Click(object sender, EventArgs e)
        {
            ModManager.FullUpdate();
            if (ModManager.BinaryFilePath != null) Process.Start(ModManager.BinaryFilePath);
        }

        private void settingsButton_Click(object sender, EventArgs e)
        {
            SettingsForm settingsForm = new SettingsForm();
            settingsForm.StartPosition = FormStartPosition.Manual;
            settingsForm.Location = new Point((this.Location.X + this.Width / 2) - (settingsForm.Width / 2), (this.Location.Y + this.Height / 2) - (settingsForm.Height / 2));
            settingsForm.Show(this);
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            TextPrompt profileNamePrompt = new TextPrompt();
            profileNamePrompt.StartPosition = FormStartPosition.Manual;
            profileNamePrompt.Location = new Point((this.Location.X + this.Width / 2) - (profileNamePrompt.Width / 2), (this.Location.Y + this.Height / 2) - (profileNamePrompt.Height / 2));
            profileNamePrompt.AllowBrowse = false;
            profileNamePrompt.Width -= 100;
            profileNamePrompt.DisplayText = "Name this profile:";
            if (profileNamePrompt.ShowDialog(this) == DialogResult.OK)
            {
                if (ModManager.ProfileList.ContainsKey(profileNamePrompt.OutputText))
                {
                    DialogResult dialogResult = MessageBox.Show("A profile with this name already exists! Do you want to overwrite it?", "Uh oh!", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        ModManager.ProfileList[profileNamePrompt.OutputText] = ModManager.GenerateProfile();
                        ModManager.SyncConfigToDisk();
                    }
                    return;
                }
                ModManager.ProfileList.Add(profileNamePrompt.OutputText, ModManager.GenerateProfile());
                ModManager.SyncConfigToDisk();
            }
        }

        private void loadButton_Click(object sender, EventArgs e)
        {
            ProfileSelector selectorForm = new ProfileSelector();
            selectorForm.StartPosition = FormStartPosition.Manual;
            selectorForm.Location = new Point((this.Location.X + this.Width / 2) - (selectorForm.Width / 2), (this.Location.Y + this.Height / 2) - (selectorForm.Height / 2));
            if (selectorForm.ShowDialog(this) == DialogResult.OK)
            {
                ModManager.ApplyProfile(selectorForm.SelectedProfile);
                ModManager.FullUpdate();
                TableManager.Refresh();
            }
        }
    }
}
