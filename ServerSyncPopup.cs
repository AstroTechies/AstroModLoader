using AstroModIntegrator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AstroModLoader
{
    public partial class ServerSyncPopup : Form
    {
        private const int CP_NOCLOSE_BUTTON = 0x200;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams myCp = base.CreateParams;
                myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
                return myCp;
            }
        }

        private BackgroundWorker backgroundWorker1;
        private Form1 BaseForm;
        public ServerSyncPopup()
        {
            InitializeComponent();
            debugLabel.Text = "";
        }

        public string OurIP = "";

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            BaseForm.CurrentlySyncing = true;
            try
            {
                SetDebugText("Finding server");
                AstroLauncherServerInfo serverInfo = PlayFabAPI.GetAstroLauncherData(OurIP);
                if (serverInfo == null)
                {
                    BaseForm.syncErrored = true;
                    BaseForm.syncErrorMessage = "Failed to find an online AstroLauncher server with the requested address!";
                    return;
                }

                if (PlayFabAPI.Dirty)
                {
                    BaseForm.ModManager.SyncConfigToDisk();
                    PlayFabAPI.Dirty = false;
                }

                List<Mod> allMods = serverInfo.GetAllMods();
                string kosherServerName = serverInfo.ServerName;
                if (string.IsNullOrWhiteSpace(kosherServerName) || kosherServerName == "Astroneer Dedicated Server") kosherServerName = OurIP;

                ModProfile creatingProfile = new ModProfile();
                creatingProfile.ProfileData = new Dictionary<string, Mod>();
                int failedDownloadCount = 0;

                // Add our current mods into the brand new profile, and specify that they are disabled
                ModProfile currentProf = BaseForm.ModManager.GenerateProfile();
                List<Mod> plannedOrdering = new List<Mod>();
                foreach (KeyValuePair<string, Mod> entry in currentProf.ProfileData)
                {
                    //entry.Value.Enabled = false;
                    entry.Value.Enabled = entry.Value.CurrentModData.Sync == SyncMode.ClientOnly;
                    creatingProfile.ProfileData[entry.Key] = entry.Value;
                    plannedOrdering.Add(entry.Value);
                }

                plannedOrdering = new List<Mod>(plannedOrdering.OrderBy(o => o.Priority).ToList());

                if (worker.CancellationPending == true)
                {
                    SetDebugText("Canceled!");
                    BaseForm.syncErrored = true;
                    BaseForm.syncErrorMessage = "The syncing process was canceled.";
                    e.Cancel = true;
                    return;
                }

                // Incorporate newly synced index files into the global index
                SetDebugText("Fetching index files");
                List<string> DuplicateURLs = new List<string>();
                foreach (Mod mod in allMods)
                {
                    if (mod.CurrentModData.Sync == SyncMode.ServerAndClient || mod.CurrentModData.Sync == SyncMode.ClientOnly)
                    {
                        IndexFile thisIndexFile = mod.GetIndexFile(DuplicateURLs);
                        if (thisIndexFile != null)
                        {
                            thisIndexFile.Mods.ToList().ForEach(x => BaseForm.ModManager.GlobalIndexFile[x.Key] = x.Value);
                            DuplicateURLs.Add(thisIndexFile.OriginalURL);
                        }
                    }
                }

                // Download server mods from the newly incorporated index files
                int numMods = allMods.Count;
                for (int i = 0; i < allMods.Count; i++)
                {
                    if (worker.CancellationPending == true)
                    {
                        SetDebugText("Canceled!");
                        BaseForm.syncErrored = true;
                        BaseForm.syncErrorMessage = "The syncing process was canceled.";
                        e.Cancel = true;
                        return;
                    }

                    if (i > 0) worker.ReportProgress((int)((double)i / numMods * 100));

                    Mod mod = allMods[i];
                    if (mod.CurrentModData.Sync == SyncMode.ServerAndClient || mod.CurrentModData.Sync == SyncMode.ClientOnly)
                    {
                        Mod appliedMod = null;

                        string debugModName = mod.CurrentModData.ModID + " v" + mod.CurrentModData.ModVersion.ToString();

                        // If we already have this mod downloaded, no sense in downloading it again
                        if (BaseForm.ModManager.ModLookup.ContainsKey(mod.CurrentModData.ModID) && BaseForm.ModManager.ModLookup[mod.CurrentModData.ModID].AvailableVersions != null && BaseForm.ModManager.ModLookup[mod.CurrentModData.ModID].AllModData.Keys.Where(m => m.ToString() == mod.InstalledVersion.ToString()).Count() > 0)
                        {
                            SetDebugText("Applying " + debugModName);
                            appliedMod = (Mod)BaseForm.ModManager.ModLookup[mod.CurrentModData.ModID].Clone();
                            appliedMod.InstalledVersion = (Version)mod.InstalledVersion.Clone();
                            creatingProfile.ProfileData[mod.CurrentModData.ModID] = appliedMod;
                        }
                        else
                        {
                            // Otherwise, go ahead and download it
                            SetDebugText("Installing " + debugModName);
                            bool didDownloadMod = BaseForm.DownloadVersionSync(mod, mod.InstalledVersion);
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
                SetDebugText("Refreshing version display");
                BaseForm.ModManager.UpdateAvailableVersionsFromIndexFiles();

                // Enforce the planned ordering in our new profile
                SetDebugText("Reordering mods");
                for (int i = 0; i < plannedOrdering.Count; i++)
                {
                    string thisModID = plannedOrdering[i].CurrentModData.ModID;
                    if (creatingProfile.ProfileData.ContainsKey(thisModID)) creatingProfile.ProfileData[thisModID].Priority = i + 1;
                }
                BaseForm.ModManager.RefreshAllPriorites();

                // Add the new profile to the list
                SetDebugText("Adding profile");
                if (BaseForm.ModManager.ProfileList == null) BaseForm.ModManager.ProfileList = new Dictionary<string, ModProfile>();
                string kosherProfileName = kosherServerName + " Synced Mods";
                BaseForm.ModManager.ProfileList[kosherProfileName] = creatingProfile;
                BaseForm.syncKosherProfileName = kosherProfileName;
                BaseForm.syncFailedDownloadCount = failedDownloadCount;
                SetDebugText("Done!");

                BaseForm.syncErrored = false;
            }
            catch (Exception ex)
            {
                if (ex is PlayFabException || ex is WebException)
                {
                    BaseForm.syncErrored = true;
                    BaseForm.syncErrorMessage = "Failed to access PlayFab!";
                    return;
                }
                BaseForm.syncErrored = true;
                BaseForm.syncErrorMessage = ex.ToString();
                return;
            }
        }

        private void SetDebugText(string txt)
        {
            AMLUtils.InvokeUI(() =>
            {
                debugLabel.Text = txt;
            });
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Close();
        }

        int numDots = 0;
        private void waitingTimer_Tick(object sender, EventArgs e)
        {
            AMLUtils.InvokeUI(() =>
            {
                numDots++; if (numDots > 3) numDots = 1;
                this.label1.Text = "Working" + new string('.', numDots);
            });
        }

        private void ProgressBarPopup_Load(object sender, EventArgs e)
        {
            if (this.Owner is Form1 f1) BaseForm = (Form1)this.Owner;
            this.Text = BaseForm.Text;

            AMLPalette.RefreshTheme(this);
            this.AdjustFormPosition();

            waitingTimer.Enabled = true;
            backgroundWorker1 = new BackgroundWorker();
            backgroundWorker1.DoWork += backgroundWorker1_DoWork;
            backgroundWorker1.ProgressChanged += backgroundWorker1_ProgressChanged;
            backgroundWorker1.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
            backgroundWorker1.RunWorkerAsync();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
        }
    }
}
