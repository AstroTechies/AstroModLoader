﻿using AstroModIntegrator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
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

        public CoolDataGridView dataGridView1;
        public Panel footerPanel;

        public Form1()
        {
            InitializeComponent();
            modInfo.Text = "";
            AMLUtils.InitializeInvoke(this);

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
            footerPanel.Paint += Footer_Paint;
            AMLPalette.RefreshTheme(this);

            AllowDrop = true;
            DragEnter += new DragEventHandler(Form1_DragEnter);
            DragDrop += new DragEventHandler(Form1_DragDrop);
            dataGridView1.DragEnter += new DragEventHandler(Form1_DragEnter);
            dataGridView1.DragDrop += new DragEventHandler(Form1_DragDrop);

            PeriodicCheckTimer.Enabled = true;
            CheckAllDirty.Enabled = true;

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
            if (System.Threading.SynchronizationContext.Current != null)
            {
                TableManager.Refresh();
                ModManager.FullUpdate();
            }
        }

        public bool DownloadVersionSync(Mod thisMod, Version newVersion)
        {
            try
            {
                if (!ModManager.GlobalIndexFile.ContainsKey(thisMod.CurrentModData.ModID)) throw new IndexFileException("Can't find index file entry for mod: " + thisMod.CurrentModData.ModID);
                Dictionary<Version, IndexVersionData> allVerData = ModManager.GlobalIndexFile[thisMod.CurrentModData.ModID].AllVersions;
                if (!allVerData.ContainsKey(newVersion)) throw new IndexFileException("Failed to find the requested version in the mod's index file: " + thisMod.CurrentModData.ModID + " v" + newVersion);

                using (var wb = new WebClient())
                {
                    wb.Headers[HttpRequestHeader.UserAgent] = AMLUtils.UserAgent;

                    string kosherFileName = AMLUtils.SanitizeFilename(allVerData[newVersion].Filename);

                    string tempDownloadFolder = Path.Combine(Path.GetTempPath(), "AstroModLoader", "Downloads");
                    Directory.CreateDirectory(tempDownloadFolder);
                    wb.DownloadFile(allVerData[newVersion].URL, Path.Combine(tempDownloadFolder, kosherFileName));
                    InstallModFromPath(Path.Combine(tempDownloadFolder, kosherFileName), out _);
                    ModManager.SortVersions();
                    ModManager.SortMods();
                    Directory.Delete(tempDownloadFolder, true);
                }
            }
            catch (Exception ex)
            {
                if (ex is WebException || ex is IOException || ex is IndexFileException)
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
            if (!thisMod.AllModData.ContainsKey(newVersion))
            {
                if (!thisMod.ForceLatest) return;
                thisMod.InstalledVersion = thisMod.AvailableVersions[0];
            }
            thisMod.Dirty = true;
        }

        // The rest
        private void ForceUpdateCells()
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
                        if (TableHandler.ShouldContainOptionalColumn()) taggedMod.IsOptional = (bool)row.Cells[5].Value;
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
            }).ContinueWith(res =>
            {
                TableManager.Refresh();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private volatile bool IsAllDirty = false;
        private void DataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            IsAllDirty = true;
        }

        private void CheckAllDirty_Tick(object sender, EventArgs e)
        {
            if (IsAllDirty)
            {
                ForceUpdateCells();
                IsAllDirty = false;
            }
        }

        private void PeriodicCheckTimer_Tick(object sender, EventArgs e)
        {
            ModManager.UpdateReadOnlyStatus();
        }

        public void AdjustModInfoText(string txt, string linkText = "")
        {
            string newTextFull = txt + linkText;
            var newLinkArea = new LinkArea(txt.Length, linkText.Length);
            if (this.modInfo.Text == newTextFull && this.modInfo.LinkArea.Start == newLinkArea.Start && this.modInfo.LinkArea.Length == newLinkArea.Length) return; // Partial fix for winforms rendering issue

            this.modInfo.Text = newTextFull;
            this.modInfo.LinkArea = newLinkArea;
        }

        public void SwitchPlatform(PlatformType newPlatform)
        {
            if (!ModManager.ValidPlatformTypesToPaths.ContainsKey(newPlatform)) return;
            ModManager.GamePath = null;
            ModManager.Platform = newPlatform;
            ModManager.DeterminePaths();
            ModManager.GamePath = ModManager.ValidPlatformTypesToPaths[newPlatform];
            ModManager.VerifyGamePath();
            ModManager.ApplyGamePathDerivatives();
            ModManager.VerifyIntegrity();
            ModManager.SyncIndependentConfigToDisk();
            ModManager.SyncDependentConfigFromDisk(false);
            ModManager.SyncModsFromDisk();
            ModManager.SyncDependentConfigToDisk();
            FullRefresh();
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private string[] AllowedModExtensions = new string[]
        {
            ".pak",
            ".zip"
        };

        private string AdjustNewPathToBeValid(string newPath, Metadata originalModData)
        {
            Mod testMod = new Mod(originalModData, Path.GetFileName(newPath));
            if (testMod.Priority >= 999) return null;
            return Path.Combine(Path.GetDirectoryName(newPath), testMod.ConstructName());
        }

        private string AddModFromPakPath(string newInstallingMod)
        {
            try
            {
                string newPath = AdjustNewPathToBeValid(Path.Combine(ModManager.DownloadPath, Path.GetFileName(newInstallingMod)), ModManager.ExtractMetadataFromPath(newInstallingMod));
                if (!string.IsNullOrEmpty(newPath))
                {
                    File.Copy(newInstallingMod, newPath, true);
                    return newPath;
                }
            }
            catch (IOException)
            {
                return null;
            }

            return null;
        }

        private List<Mod> InstallModFromPath(string newInstallingMod, out int numClientOnly)
        {
            numClientOnly = 0;
            string ext = Path.GetExtension(newInstallingMod);
            if (!AllowedModExtensions.Contains(ext)) return null;

            List<string> newPaths = new List<string>();

            if (ext == ".zip") // If the mod we are trying to install is a zip, we go through and copy each pak file inside that zip
            {
                string targetFolderPath = Path.Combine(Path.GetTempPath(), "AstroModLoader", Path.GetFileNameWithoutExtension(newInstallingMod)); // Extract the zip file to the temporary data folder
                ZipFile.ExtractToDirectory(newInstallingMod, targetFolderPath);

                string[] allAccessiblePaks = Directory.GetFiles(targetFolderPath, "*.pak", SearchOption.AllDirectories); // Get all pak files that exist in the zip file
                foreach (string zippedPakPath in allAccessiblePaks)
                {
                    string newPath = AddModFromPakPath(zippedPakPath);
                    if (newPath != null) newPaths.Add(newPath);
                }

                Directory.Delete(targetFolderPath, true); // Clean up the temporary data folder
            }
            else // Otherwise just copy the file itself
            {
                string newPath = AddModFromPakPath(newInstallingMod);
                if (newPath != null) newPaths.Add(newPath);
            }

            List<Mod> outputs = new List<Mod>();

            foreach (string newPath in newPaths)
            {
                try
                {
                    if (!string.IsNullOrEmpty(newPath))
                    {
                        Mod nextMod = ModManager.SyncSingleModFromDisk(newPath, out bool wasClientOnly, false);
                        if (nextMod != null) outputs.Add(nextMod);
                        if (wasClientOnly)
                        {
                            numClientOnly++;
                            File.Delete(newPath);
                        }
                    }
                }
                catch (IOException) { }
            }
            
            return outputs;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] installingModPaths = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (installingModPaths.Length > 0)
            {
                Dictionary<string, List<Version>> newMods = new Dictionary<string, List<Version>>();
                int clientOnlyCount = 0;
                int invalidExtensionCount = 0;
                int wasFolderCount = 0;
                foreach (string newInstallingMod in installingModPaths)
                {
                    if (!File.Exists(newInstallingMod))
                    {
                        wasFolderCount++;
                        continue;
                    }
                    if (!AllowedModExtensions.Contains(Path.GetExtension(newInstallingMod)))
                    {
                        invalidExtensionCount++;
                        continue;
                    }

                    List<Mod> resMods = InstallModFromPath(newInstallingMod, out int thisClientOnlyCount);
                    if (resMods == null) continue;
                    foreach (Mod resMod in resMods)
                    {
                        if (resMod == null) continue;
                        if (!newMods.ContainsKey(resMod.CurrentModData.ModID)) newMods[resMod.CurrentModData.ModID] = new List<Version>();
                        newMods[resMod.CurrentModData.ModID].AddRange(resMod.AvailableVersions);
                    }
                    clientOnlyCount += thisClientOnlyCount;
                }

                //ModManager.SyncModsFromDisk(true);
                ModManager.SortMods();
                ModManager.SortVersions();
                ModManager.RefreshAllPriorites();
                if (!autoUpdater.IsBusy) autoUpdater.RunWorkerAsync();

                foreach (Mod mod in ModManager.Mods)
                {
                    if (mod == null) continue;
                    mod.Dirty = true;

                    if (newMods.ContainsKey(mod.CurrentModData.ModID))
                    {
                        // We switch the installed version to the newest version that has just been added
                        newMods[mod.CurrentModData.ModID].Sort();
                        newMods[mod.CurrentModData.ModID].Reverse();
                        mod.InstalledVersion = newMods[mod.CurrentModData.ModID][0];

                        // If this is a new mod, we enable it or disable it automatically, but if it's not new then we respect the user's pre-existing setting 
                        if (mod.AvailableVersions.Count == 1) mod.Enabled = ModManager.InstalledAstroBuild.AcceptablySimilar(mod.CurrentModData.AstroBuild) && (!Program.CommandLineOptions.ServerMode || mod.CurrentModData.Sync != SyncMode.ClientOnly);
                    }
                }

                Task.Run(() =>
                {
                    ModManager.FullUpdate();
                });

                AMLUtils.InvokeUI(() =>
                {
                    TableManager.Refresh();
                    if (wasFolderCount > 0)
                    {
                        this.ShowBasicButton("You cannot drag in a folder!", "OK", null, null);
                    }

                    if (invalidExtensionCount > 0)
                    {
                        this.ShowBasicButton(invalidExtensionCount + " file" + (invalidExtensionCount == 1 ? " had an invalid extension" : "s had invalid extensions") + " and " + (invalidExtensionCount == 1 ? "was" : "were") + " ignored.\nAcceptable mod extensions are: " + string.Join(", ", AllowedModExtensions), "OK", null, null);
                    }

                    if (clientOnlyCount > 0)
                    {
                        this.ShowBasicButton(clientOnlyCount + " mod" + (clientOnlyCount == 1 ? " is" : "s are") + " designated as \"Client only\" and " + (clientOnlyCount == 1 ? "was" : "were") + " ignored.", "OK", null, null);
                    }
                });
            }
        }

        private void DataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return;
            if (AMLUtils.IsLinux) return;

            Type t = dataGridView1.GetType().BaseType;
            FieldInfo viewSetter = t.GetField("latestEditingControl", BindingFlags.Default | BindingFlags.NonPublic | BindingFlags.Instance);
            viewSetter.SetValue(dataGridView1, null);
        }

        private void DataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs anError)
        {
            MessageBox.Show("DataError happened! Please report this! " + anError.Context.ToString());
        }

        private void Footer_Paint(object sender, PaintEventArgs e)
        {
            using (Pen p = new Pen(AMLPalette.FooterLineColor, 1))
            {
                e.Graphics.DrawLine(p, new Point(0, 0), new Point(footerPanel.ClientSize.Width, 0));
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
            if (dataGridView1.SelectedRows.Count == 1 && !ModManager.IsReadOnly)
            {
                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];
                int newModIndex = selectedRow.Index;

                // If shift is held, that means we are changing the order
                if (canAdjustOrder && ModifierKeys == Keys.Shift && selectedMod != null && previouslySelectedMod != null && previouslySelectedMod != selectedMod)
                {
                    AMLUtils.InvokeUI(() =>
                    {
                        ModManager.SwapMod(previouslySelectedMod, newModIndex, false);
                        previouslySelectedMod = null;
                        canAdjustOrder = false;
                        TableManager.Refresh();
                        canAdjustOrder = true;

                        dataGridView1.ClearSelection();
                        dataGridView1.Rows[newModIndex].Selected = true;
                        dataGridView1.CurrentCell = dataGridView1.Rows[newModIndex].Cells[0];
                        selectedMod = ModManager.Mods[newModIndex];

                        Task.Run(() =>
                        {
                            foreach (Mod mod in ModManager.Mods) mod.Dirty = true; // Update all the priorities on disk to be safe
                            ModManager.FullUpdate();
                        });
                    });
                }
            }

            previouslySelectedMod = selectedMod;

            RefreshModInfoLabel();
        }

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            Mod selectedMod = TableManager.GetCurrentlySelectedMod();

            if (selectedMod != null && !ModManager.IsReadOnly && e.KeyCode == Keys.Delete)
            {
                if (e.Alt)
                {
                    selectedMod.AvailableVersions.Sort();
                    selectedMod.AvailableVersions.Reverse();

                    List<Version> badVersions = new List<Version>();
                    foreach (Version testingVersion in selectedMod.AvailableVersions)
                    {
                        if (testingVersion != selectedMod.AvailableVersions[0] && selectedMod.AllModData.ContainsKey(testingVersion)) badVersions.Add(testingVersion);
                    }
                    if (badVersions.Count == 0)
                    {
                        this.ShowBasicButton("You have no old versions of this mod on disk to clean!", "OK", null, null);
                    }
                    else
                    {
                        int dialogRes = this.ShowBasicButton("Are you sure you want to clean \"" + selectedMod.CurrentModData.Name + "\"?\nThese versions will be deleted from disk: " + string.Join(", ", badVersions.Select(v => v.ToString()).ToArray()), "Yes", "No", null);
                        if (dialogRes == 0)
                        {
                            ModManager.EviscerateMod(selectedMod, badVersions);
                            FullRefresh();
                            selectedMod.InstalledVersion = selectedMod.AvailableVersions[0];
                            this.ShowBasicButton("Successfully cleaned \"" + selectedMod.CurrentModData.Name + "\".", "OK", null, null);
                        }
                    }
                }
                else
                {
                    int dialogRes = this.ShowBasicButton("Are you sure you want to completely delete \"" + selectedMod.CurrentModData.Name + "\"?", "Yes", "No", null);
                    if (dialogRes == 0)
                    {
                        ModManager.EviscerateMod(selectedMod);
                        FullRefresh();
                    }
                }
                
            }
        }

        private void RefreshModInfoLabel()
        {
            AMLUtils.InvokeUI(() =>
            {
                Mod selectedMod = TableManager?.GetCurrentlySelectedMod();
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
                    case SyncMode.None:
                        kosherSync = "None";
                        break;
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

                long knownSize = -1;
                try
                {
                    knownSize = ModManager.GetSizeOnDisk(selectedMod);
                }
                catch (Exception ex)
                {
                    if (!(ex is IOException) && !(ex is FileNotFoundException)) throw;
                }

                string additionalData = "";
                if (knownSize >= 0) additionalData += "\nSize: " + AMLUtils.FormatFileSize(knownSize);

                bool hasHomepage = !string.IsNullOrEmpty(selectedMod.CurrentModData.Homepage) && AMLUtils.IsValidUri(selectedMod.CurrentModData.Homepage);
                AdjustModInfoText("Name: " + selectedMod.CurrentModData.Name + "\nDescription: " + kosherDescription + "\nSync: " + kosherSync + additionalData + (hasHomepage ? "\nWebsite: " : ""), hasHomepage ? selectedMod.CurrentModData.Homepage : "");
            });
        }

        private void modInfo_LinkClicked(object sender, EventArgs e)
        {
            Mod selectedMod = TableManager.GetCurrentlySelectedMod();
            if (selectedMod != null && !string.IsNullOrEmpty(selectedMod.CurrentModData.Homepage) && AMLUtils.IsValidUri(selectedMod.CurrentModData.Homepage)) Process.Start(selectedMod.CurrentModData.Homepage);
        }

        public void ForceResize()
        {
            footerPanel.Width = this.Width;
            modPanel.Width = this.ClientSize.Width - 15;
            dataGridView1.Width = modPanel.Width - dataGridView1.Location.X;

            int buttonHeight = footerPanel.Height / 2;
            syncButton.Height = buttonHeight;
            exitButton.Height = buttonHeight;
            syncButton.Width = (int)(buttonHeight * 4.5);
            exitButton.Width = buttonHeight * 3;

            syncButton.Location = new Point(dataGridView1.Location.X + dataGridView1.Width - syncButton.Width, dataGridView1.Location.Y + dataGridView1.Height + 10);
            exitButton.Location = new Point(this.Width - exitButton.Width - 30, (footerPanel.Height - exitButton.Height) / 2);

            dataGridView1.Invalidate();
        }

        public void FullRefresh()
        {
            if (ModManager != null)
            {
                Directory.CreateDirectory(ModManager.DownloadPath);
                Directory.CreateDirectory(ModManager.InstallPath);

                ModManager.SyncModsFromDisk();
                ModManager.SyncConfigFromDisk();
                ModManager.UpdateReadOnlyStatus();
                ModManager.SortMods();
                if (!autoUpdater.IsBusy) autoUpdater.RunWorkerAsync();
            }

            AMLUtils.InvokeUI(() =>
            {
                if (TableManager != null) TableManager.Refresh();
                AMLPalette.RefreshTheme(this);
                RefreshModInfoLabel();
            });
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            ForceResize();
        }

        private void refresh_Click(object sender, EventArgs e)
        {
            FullRefresh();
        }

        public static readonly string GitHubRepo = "AstroTechies/AstroModLoader";
        private Version latestOnlineVersion = null;
        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();

            if (!string.IsNullOrEmpty(Program.CommandLineOptions.NextLaunchPath))
            {
                ModManager.LaunchCommand = Program.CommandLineOptions.NextLaunchPath;
                Program.CommandLineOptions.NextLaunchPath = null;
                ModManager.SyncConfigToDisk();
            }

            // Fetch the latest version from github
            Task.Run(() =>
            {
                latestOnlineVersion = GitHubAPI.GetLatestVersionFromGitHub(GitHubRepo);
            }).ContinueWith(res =>
            {
                //Debug.WriteLine("Latest: " + latestOnlineVersion);
                if (latestOnlineVersion != null && latestOnlineVersion.CompareTo(Assembly.GetExecutingAssembly().GetName().Version) > 0)
                {
                    BasicButtonPopup resultButton = this.GetBasicButton("A new version of AstroModLoader (v" + latestOnlineVersion + ") is available!", "OK", "Open in browser", null);
                    resultButton.PageToVisit = GitHubAPI.GetLatestVersionURL(GitHubRepo);
                    resultButton.ShowDialog();
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());

            // Force an update for good measure
            ModManager.FullUpdate();

            // Initial resize of the menu to fit the table if necessary
            if (dataGridView1.PreferredSize.Width > dataGridView1.Size.Width) this.Size = new Size(this.Width + (dataGridView1.PreferredSize.Width - dataGridView1.Size.Width), this.Height);

            AMLUtils.InvokeUI(ForceResize);
            AMLUtils.InvokeUI(ForceResize);
        }

        private void playButton_Click(object sender, EventArgs e)
        {
            ModManager.FullUpdate();
            
            if (!Program.CommandLineOptions.ServerMode)
            {
                if (ModManager.Platform == PlatformType.Steam)
                {
                    Process.Start(@"steam://run/361420");
                    return;
                }
                else if (ModManager.Platform == PlatformType.Win10)
                {
                    if (!string.IsNullOrEmpty(ModManager.MicrosoftRuntimeID)) Process.Start(@"shell:appsFolder\" + ModManager.MicrosoftRuntimeID + "!ASTRONEER");
                    return;
                }
            }

            if ((Program.CommandLineOptions.ServerMode || AMLUtils.IsLinux || string.IsNullOrEmpty(ModManager.BinaryFilePath)) && string.IsNullOrEmpty(ModManager.LaunchCommand))
            {
                TextPrompt initialPathPrompt = new TextPrompt
                {
                    StartPosition = FormStartPosition.CenterScreen,
                    DisplayText = "Select a file to run:",
                    AllowBrowse = true,
                    BrowseMode = BrowseMode.File
                };

                if (initialPathPrompt.ShowDialog(this) == DialogResult.OK)
                {
                    ModManager.LaunchCommand = initialPathPrompt.OutputText;
                    ModManager.SyncConfigToDisk();
                }
            }

            if (string.IsNullOrEmpty(ModManager.LaunchCommand) && !string.IsNullOrEmpty(ModManager.BinaryFilePath))
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
                    this.ShowBasicButton("Invalid path to file: \"" + ModManager.LaunchCommand + "\"", "OK", null, null);
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
            settingsForm.ShowDialog(this);
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void loadButton_Click(object sender, EventArgs e)
        {
            if (ModManager.IsReadOnly)
            {
                this.ShowBasicButton("You cannot edit profiles while the game is open!", "OK", null, null);
                return;
            }

            ProfileSelector selectorForm = new ProfileSelector();
            selectorForm.StartPosition = FormStartPosition.Manual;
            selectorForm.Location = new Point((this.Location.X + this.Width / 2) - (selectorForm.Width / 2), (this.Location.Y + this.Height / 2) - (selectorForm.Height / 2));
            selectorForm.ShowDialog(this);
        }

        private bool currentlySyncing = false;
        private bool syncErrored = false;
        private string syncErrorMessage;
        private int syncFailedDownloadCount;
        private string syncKosherProfileName;
        private void syncButton_Click(object sender, EventArgs e)
        {
            if (ModManager.IsReadOnly)
            {
                this.ShowBasicButton("You cannot sync mods while the game is open!", "OK", null, null);
                return;
            }

            if (currentlySyncing)
            {
                this.ShowBasicButton("Please wait, the mod loader is currently busy syncing.", "OK", null, null);
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
                syncErrored = false;
                syncErrorMessage = "";
                syncFailedDownloadCount = 0;
                syncKosherProfileName = "";

                Task.Run(() =>
                {
                    currentlySyncing = true;
                    try
                    {
                        string ourIP = getIPPrompt.OutputText.Trim();

                        AstroLauncherServerInfo serverInfo = PlayFabAPI.GetAstroLauncherData(ourIP);
                        if (serverInfo == null)
                        {
                            syncErrored = true;
                            syncErrorMessage = "Failed to find an online AstroLauncher server with the requested address!";
                            return;
                        }

                        if (PlayFabAPI.Dirty)
                        {
                            ModManager.SyncConfigToDisk();
                            PlayFabAPI.Dirty = false;
                        }

                        List<Mod> allMods = serverInfo.GetAllMods();
                        string kosherServerName = serverInfo.ServerName;
                        if (string.IsNullOrWhiteSpace(kosherServerName) || kosherServerName == "Astroneer Dedicated Server") kosherServerName = ourIP;

                        ModProfile creatingProfile = new ModProfile();
                        creatingProfile.ProfileData = new Dictionary<string, Mod>();
                        int failedDownloadCount = 0;

                        // Add our current mods into the brand new profile, and specify that they are disabled
                        ModProfile currentProf = ModManager.GenerateProfile();
                        List<Mod> plannedOrdering = new List<Mod>();
                        foreach (KeyValuePair<string, Mod> entry in currentProf.ProfileData)
                        {
                            //entry.Value.Enabled = false;
                            entry.Value.Enabled = entry.Value.CurrentModData.Sync == SyncMode.ClientOnly;
                            creatingProfile.ProfileData[entry.Key] = entry.Value;
                            plannedOrdering.Add(entry.Value);
                        }

                        plannedOrdering = new List<Mod>(plannedOrdering.OrderBy(o => o.Priority).ToList());

                        // Incorporate newly synced index files into the global index
                        List<string> DuplicateURLs = new List<string>();
                        foreach (Mod mod in allMods)
                        {
                            if (mod.CurrentModData.Sync == SyncMode.ServerAndClient || mod.CurrentModData.Sync == SyncMode.ClientOnly)
                            {
                                IndexFile thisIndexFile = mod.GetIndexFile(DuplicateURLs);
                                if (thisIndexFile != null)
                                {
                                    thisIndexFile.Mods.ToList().ForEach(x => ModManager.GlobalIndexFile[x.Key] = x.Value);
                                    DuplicateURLs.Add(thisIndexFile.OriginalURL);
                                }
                            }
                        }

                        // Download server mods from the newly incorporated index files
                        foreach (Mod mod in allMods)
                        {
                            if (mod.CurrentModData.Sync == SyncMode.ServerAndClient || mod.CurrentModData.Sync == SyncMode.ClientOnly)
                            {
                                Mod appliedMod = null;

                                // If we already have this mod downloaded, no sense in downloading it again
                                if (ModManager.ModLookup.ContainsKey(mod.CurrentModData.ModID) && ModManager.ModLookup[mod.CurrentModData.ModID].AvailableVersions != null && ModManager.ModLookup[mod.CurrentModData.ModID].AllModData.Keys.Where(m => m.ToString() == mod.InstalledVersion.ToString()).Count() > 0)
                                {
                                    appliedMod = (Mod)ModManager.ModLookup[mod.CurrentModData.ModID].Clone();
                                    appliedMod.InstalledVersion = (Version)mod.InstalledVersion.Clone();
                                    creatingProfile.ProfileData[mod.CurrentModData.ModID] = appliedMod;
                                }
                                else
                                {
                                    // Otherwise, go ahead and download it
                                    bool didDownloadMod = DownloadVersionSync(mod, mod.InstalledVersion);
                                    if (didDownloadMod)
                                    {
                                        appliedMod = mod;
                                        creatingProfile.ProfileData[mod.CurrentModData.ModID] = appliedMod;
                                    }
                                    else
                                    {
                                        failedDownloadCount++;
                                    }
                                }

                                if (appliedMod != null)
                                {
                                    appliedMod.Enabled = true;
                                    appliedMod.ForceLatest = false;
                                    plannedOrdering.Remove(appliedMod);
                                    plannedOrdering.Insert(0, appliedMod);
                                }
                            }
                        }

                        // Update available versions list to make the syncing seamless
                        ModManager.UpdateAvailableVersionsFromIndexFiles();

                        // Enforce the planned ordering in our new profile
                        for (int i = 0; i < plannedOrdering.Count; i++)
                        {
                            string thisModID = plannedOrdering[i].CurrentModData.ModID;
                            if (creatingProfile.ProfileData.ContainsKey(thisModID)) creatingProfile.ProfileData[thisModID].Priority = i + 1;
                        }

                        // Add the new profile to the list
                        if (ModManager.ProfileList == null) ModManager.ProfileList = new Dictionary<string, ModProfile>();
                        string kosherProfileName = kosherServerName + " Synced Mods";
                        ModManager.ProfileList[kosherProfileName] = creatingProfile;
                        syncKosherProfileName = kosherProfileName;
                        syncFailedDownloadCount = failedDownloadCount;
                    }
                    catch (Exception ex)
                    {
                        if (ex is PlayFabException || ex is WebException)
                        {
                            syncErrored = true;
                            syncErrorMessage = "Failed to access PlayFab!";
                            return;
                        }
                        syncErrored = true;
                        syncErrorMessage = ex.ToString();
                        return;
                    }
                }).ContinueWith(res =>
                {
                    if (syncErrored)
                    {
                        this.ShowBasicButton(syncErrorMessage, "OK", null, null);
                    }
                    else
                    {
                        this.ShowBasicButton("Added a new profile named \"" + syncKosherProfileName + "\".\n" + (syncFailedDownloadCount == 0 ? "No" : syncFailedDownloadCount.ToString()) + " mod" + (syncFailedDownloadCount == 1 ? "" : "s") + " failed to sync.", "OK", null, null);
                    }
                    ModManager.SyncConfigToDisk();
                    TableManager.Refresh();
                    currentlySyncing = false;
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }
    }
}
