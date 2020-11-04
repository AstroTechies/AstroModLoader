using AstroModIntegrator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
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

            if (Program.CommandLineOptions.ServerMode) syncButton.Hide();

            ModManager = new ModHandler(this);
            TableManager = new TableHandler(dataGridView1, ModManager);

            dataGridView1.CellValueChanged += DataGridView1_CellValueChanged;
            dataGridView1.CellContentClick += DataGridView1_CellContentClick;
            dataGridView1.DataBindingComplete += DataGridView1_DataBindingComplete;
            dataGridView1.CellEndEdit += DataGridView1_CellEndEdit;
            dataGridView1.SelectionChanged += new EventHandler(DataGridView1_SelectionChanged);
            footer.Paint += Footer_Paint;
            AMLPalette.RefreshTheme(this);

            AllowDrop = true;
            DragEnter += new DragEventHandler(Form1_DragEnter);
            DragDrop += new DragEventHandler(Form1_DragDrop);
            dataGridView1.DragEnter += new DragEventHandler(Form1_DragEnter);
            dataGridView1.DragDrop += new DragEventHandler(Form1_DragDrop);

            PeriodicCheckTimer.Enabled = true;

            autoUpdater = new BackgroundWorker();
            autoUpdater.DoWork += new DoWorkEventHandler(AutoUpdater_DoWork);
            autoUpdater.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Simple_Refresh_RunWorkerCompleted);
            autoUpdater.RunWorkerAsync();
        }

        // Async operations
        private BackgroundWorker autoUpdater;

        private void AutoUpdater_DoWork(object sender, DoWorkEventArgs e)
        {
            if (ModManager != null) ModManager.AggregateIndexFiles();
        }

        private void Simple_Refresh_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            TableManager.Refresh();
        }

        public bool DownloadVersionSync(Mod thisMod, Version newVersion)
        {
            try
            {
                if (!ModManager.GlobalIndexFile.ContainsKey(thisMod.CurrentModData.ModID)) throw new IndexFileException("Can't find index file entry for mod");
                Dictionary<Version, IndexVersionData> allVerData = ModManager.GlobalIndexFile[thisMod.CurrentModData.ModID].AllVersions;
                if (!allVerData.ContainsKey(newVersion)) throw new IndexFileException("Failed to find the requested version in the mod's index file");

                using (var wb = new WebClient())
                {
                    wb.Headers[HttpRequestHeader.UserAgent] = "AstroModLoader " + Application.ProductVersion;

                    string kosherFileName = AMLUtils.SanitizeFilename(allVerData[newVersion].Filename);
                    if (kosherFileName.Substring(kosherFileName.Length - 6, 6) != "_P.pak") kosherFileName += "_P.pak";
                    wb.DownloadFile(allVerData[newVersion].URL, Path.Combine(ModManager.DownloadPath, kosherFileName));
                    ModManager.SyncSingleModFromDisk(Path.Combine(ModManager.DownloadPath, kosherFileName));
                }
            }
            catch (Exception ex)
            {
                if (ex is WebException || ex is IndexFileException)
                {
                    Debug.WriteLine(ex.ToString());
                    return false;
                }
                throw;
            }
            return true;
        }

        public void SwitchVersionSync(Mod thisMod, Version newVersion)
        {
            if (!thisMod.AllModData.ContainsKey(newVersion))
            {
                thisMod.CannotCurrentlyUpdate = true;
                bool outcome = DownloadVersionSync(thisMod, newVersion);
                thisMod.CannotCurrentlyUpdate = false;

                if (!outcome) return;
            }

            thisMod.InstalledVersion = newVersion;
            thisMod.Dirty = true;
        }

        // The rest
        private void DataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            Task.Run(() =>
            {
                if (ModManager.IsReadOnly) return;
                foreach (DataGridViewRow row in this.dataGridView1.Rows)
                {
                    if (row.Tag is Mod taggedMod)
                    {
                        if (taggedMod.CannotCurrentlyUpdate) continue;
                        taggedMod.Enabled = (bool)row.Cells[0].Value;
                        if (row.Cells[2].Value is string strVal)
                        {
                            Version changingVer = null;
                            if (strVal.Contains("Latest"))
                            {
                                taggedMod.ForceLatest = true;
                                changingVer = taggedMod.AvailableVersions[0];
                            }
                            else
                            {
                                taggedMod.ForceLatest = false;
                                changingVer = new Version(strVal);
                            }

                            SwitchVersionSync(taggedMod, changingVer);
                        }
                    }
                }
                ModManager.FullUpdate();
            });
        }

        public void AdjustModInfoText(string txt)
        {
            this.modInfo.Text = txt;
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] installingModPaths = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (installingModPaths.Length > 0)
            {
                foreach (string newInstallingMod in installingModPaths)
                {
                    try
                    {
                        File.Copy(newInstallingMod, Path.Combine(ModManager.DownloadPath, Path.GetFileName(newInstallingMod)));
                        //File.Copy(newInstallingMod, Path.Combine(ModManager.InstallPath, Path.GetFileName(newInstallingMod)));
                    }
                    catch (IOException) { }
                }

                FullRefresh();

                foreach (Mod mod in ModManager.Mods)
                {
                    mod.Dirty = true;
                    mod.Enabled = true;
                    if ((ModManager.InstalledAstroBuild != null && mod.CurrentModData.AstroBuild != null && !ModManager.InstalledAstroBuild.AcceptablySimilar(mod.CurrentModData.AstroBuild)) || (Program.CommandLineOptions.ServerMode && mod.CurrentModData.Sync == SyncMode.ClientOnly))
                    {
                        mod.Enabled = false;
                    }
                }

                ModManager.FullUpdate();
                TableManager.Refresh();
            }
        }

        private void DataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return;
            if (AMLUtils.IsLinux) return;

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
            ModManager.RefreshAllPriorites();
        }

        private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            dataGridView1.EndEdit();
            dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private Mod previouslySelectedMod;
        private bool canAdjustOrder = true;
        private void DataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            Mod selectedMod = TableManager.GetCurrentlySelectedMod();

            if (dataGridView1.SelectedRows.Count == 1)
            {
                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];

                // If shift is held, that means we are changing the order
                if (canAdjustOrder && ModifierKeys == Keys.Shift && selectedMod != null && previouslySelectedMod != null && previouslySelectedMod != selectedMod)
                {
                    int newModIndex = selectedRow.Index;
                    ModManager.SwapMod(previouslySelectedMod, newModIndex);

                    previouslySelectedMod = null;
                    canAdjustOrder = false;
                    TableManager.Refresh();
                    canAdjustOrder = true;

                    dataGridView1.ClearSelection();
                    dataGridView1.Rows[newModIndex].Selected = true;
                    dataGridView1.CurrentCell = dataGridView1.Rows[newModIndex].Cells[0];
                    selectedMod = ModManager.Mods[newModIndex];
                }
            }

            previouslySelectedMod = selectedMod;

            if (selectedMod == null)
            {
                AdjustModInfoText("");
                return;
            }
            string kosherDescription = string.IsNullOrEmpty(selectedMod.CurrentModData.Description) ? "N/A" : selectedMod.CurrentModData.Description;
            if (kosherDescription.Length > 80) kosherDescription = kosherDescription.Substring(0, 80) + "...";
            string kosherSync = "N/A";
            switch (selectedMod.CurrentModData.Sync)
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

            AdjustModInfoText("Name: " + selectedMod.CurrentModData.Name + "\nDescription: " + kosherDescription + "\nSync: " + kosherSync);
        }

        public void ForceResize()
        {
            footer.Width = this.Width;
        }

        public void FullRefresh()
        {
            if (ModManager != null)
            {
                ModManager.SyncModsFromDisk();
                ModManager.SyncConfigFromDisk();
                ModManager.UpdateReadOnlyStatus();
                ModManager.SortMods();
                autoUpdater.RunWorkerAsync();
            }
            if (TableManager != null) TableManager.Refresh();
            AMLPalette.RefreshTheme(this);
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            ForceResize();
        }

        private void refresh_Click(object sender, EventArgs e)
        {
            FullRefresh();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
        }

        private void playButton_Click(object sender, EventArgs e)
        {
            ModManager.FullUpdate();

            if ((Program.CommandLineOptions.ServerMode || ModManager.BinaryFilePath == null) && string.IsNullOrEmpty(ModManager.LaunchCommand))
            {
                TextPrompt initialPathPrompt = new TextPrompt
                {
                    StartPosition = FormStartPosition.CenterScreen,
                    DisplayText = "Select a file to run",
                    AllowBrowse = true,
                    BrowseMode = BrowseMode.File
                };

                if (initialPathPrompt.ShowDialog(this) == DialogResult.OK)
                {
                    ModManager.LaunchCommand = initialPathPrompt.OutputText;
                    ModManager.SyncConfigToDisk();
                }
            }

            if (string.IsNullOrEmpty(ModManager.LaunchCommand) && ModManager.BinaryFilePath != null)
            {
                Process.Start(ModManager.BinaryFilePath, Program.CommandLineOptions.ServerMode ? "-log" : "");
            }
            else
            {
                if (string.IsNullOrEmpty(ModManager.LaunchCommand)) return;
                try
                {
                    Process.Start(new ProcessStartInfo()
                    {
                        WorkingDirectory = Path.GetDirectoryName(ModManager.LaunchCommand),
                        FileName = ModManager.LaunchCommand
                    });
                }
                catch
                {
                    AMLUtils.ShowBasicButton(this, "Invalid path to file: \"" + ModManager.LaunchCommand + "\"", "OK", null, null);
                    ModManager.LaunchCommand = null;
                    ModManager.SyncConfigToDisk();
                }
                
            }
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

        private void loadButton_Click(object sender, EventArgs e)
        {
            if (ModManager.IsReadOnly)
            {
                AMLUtils.ShowBasicButton(this, "You cannot edit profiles while the game is open!", "OK", null, null);
                return;
            }

            ProfileSelector selectorForm = new ProfileSelector();
            selectorForm.StartPosition = FormStartPosition.Manual;
            selectorForm.Location = new Point((this.Location.X + this.Width / 2) - (selectorForm.Width / 2), (this.Location.Y + this.Height / 2) - (selectorForm.Height / 2));
            selectorForm.ShowDialog(this);
        }

        private void PeriodicCheckTimer_Tick(object sender, EventArgs e)
        {
            ModManager.UpdateReadOnlyStatus();
        }

        private void ForceRefreshTimer_Tick(object sender, EventArgs e)
        {
            TableManager.Refresh();
        }

        private void syncButton_Click(object sender, EventArgs e)
        {
            if (ModManager.IsReadOnly)
            {
                AMLUtils.ShowBasicButton(this, "You cannot sync mods while the game is open!", "OK", null, null);
                return;
            }

            TextPrompt getIPPrompt = new TextPrompt();
            getIPPrompt.DisplayText = "Enter a server address to sync with:";
            getIPPrompt.Width -= 100;
            getIPPrompt.AllowBrowse = false;
            getIPPrompt.StartPosition = FormStartPosition.Manual;
            getIPPrompt.Location = new Point((this.Location.X + this.Width / 2) - (getIPPrompt.Width / 2), (this.Location.Y + this.Height / 2) - (getIPPrompt.Height / 2));

            if (getIPPrompt.ShowDialog(this) == DialogResult.OK)
            {
                Task.Run(() =>
                {
                    AstroLauncherServerInfo serverInfo = PlayFabAPI.GetAstroLauncherData(getIPPrompt.OutputText);
                    if (serverInfo == null)
                    {
                        AMLUtils.ShowBasicButton(this, "Failed to find an online AstroLauncher server with the requested address!", "OK", null, null);
                        return;
                    }

                    if (PlayFabAPI.Dirty)
                    {
                        ModManager.SyncConfigToDisk();
                        PlayFabAPI.Dirty = false;
                    }

                    List<Mod> allMods = serverInfo.GetAllMods();
                    string kosherServerName = serverInfo.ServerName;
                    if (string.IsNullOrEmpty(kosherServerName) || kosherServerName == "Astroneer Dedicated Server") kosherServerName = getIPPrompt.OutputText;

                    ModProfile creatingProfile = new ModProfile();
                    creatingProfile.ProfileData = new Dictionary<string, Mod>();
                    int failedDownloadCount = 0;

                    // Add our current mods into the index, and specify that they should be disabled
                    ModProfile currentProf = ModManager.GenerateProfile();
                    foreach (KeyValuePair<string, Mod> entry in currentProf.ProfileData)
                    {
                        entry.Value.Enabled = false;
                        creatingProfile.ProfileData[entry.Value.CurrentModData.ModID] = entry.Value;
                    }

                    // Incorporate newly synced index files into the global index
                    List<string> DuplicateURLs = new List<string>();
                    foreach (Mod mod in allMods)
                    {
                        IndexFile thisIndexFile = mod.GetIndexFile(DuplicateURLs);
                        if (thisIndexFile != null)
                        {
                            thisIndexFile.Mods.ToList().ForEach(x => ModManager.GlobalIndexFile[x.Key] = x.Value);
                            DuplicateURLs.Add(thisIndexFile.OriginalURL);
                        }
                    }

                    // Download server mods from the newly incorporated index files
                    foreach (Mod mod in allMods)
                    {
                        if (mod.CurrentModData.Sync == SyncMode.ServerAndClient || mod.CurrentModData.Sync == SyncMode.ClientOnly)
                        {
                            bool didDownloadMod = DownloadVersionSync(mod, mod.InstalledVersion);
                            if (didDownloadMod)
                            {
                                creatingProfile.ProfileData[mod.CurrentModData.ModID] = mod;
                            }
                            else
                            {
                                failedDownloadCount++;
                            }
                            mod.Enabled = true;
                            mod.ForceLatest = false;
                        }
                    }

                    // Update available versions list to make the syncing seamless
                    ModManager.UpdateAvailableVersionsFromIndexFiles();

                    // Add the new profile to the list
                    string kosherProfileName = kosherServerName + " Synced Mods";
                    ModManager.ProfileList[kosherProfileName] = creatingProfile;
                    AMLUtils.ShowBasicButton(this, "Added a new profile named \"" + kosherProfileName + "\". " + failedDownloadCount + " mods failed to sync.", "OK", null, null);
                });
            }
        }
    }
}
