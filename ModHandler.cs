using AstroModIntegrator;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace AstroModLoader
{
    public class ModHandler
    {
        internal string CustomBasePath = "";
        internal string BasePath;
        internal string DownloadPath;
        internal string InstallPath;
        internal string GamePath;
        internal string LaunchCommand;
        internal Dictionary<string, Mod> ModLookup;
        internal string BinaryFilePath;
        internal Form1 BaseForm;

        public PlatformType Platform = PlatformType.Unknown;
        public Dictionary<PlatformType, string> ValidPlatformTypesToPaths;
        public List<PlatformType> AllPlatforms;
        public List<Mod> Mods;
        public Dictionary<string, ModProfile> ProfileList;
        public Dictionary<string, IndexMod> GlobalIndexFile;
        public Version InstalledAstroBuild = null;
        public volatile bool IsReadOnly = false;

        public ModHandler(Form1 baseForm)
        {
            BaseForm = baseForm;
            OurIntegrator = new ModIntegrator();
            OurIntegrator.RefuseMismatchedConnections = true;

            string automaticSteamPath = null;
            string automaticWin10Path = null;
            if (!Program.CommandLineOptions.ServerMode)
            {
                try
                {
                    automaticSteamPath = AMLUtils.FixGamePath(CheckRegistryForSteamPath(361420)); // Astroneer: 361420
                }
                catch { }

                try
                {
                    automaticWin10Path = AMLUtils.FixGamePath(CheckRegistryForMicrosoftStorePath());
                }
                catch { }
            }

            //automaticSteamPath = null;
            //automaticWin10Path = null;

            ValidPlatformTypesToPaths = new Dictionary<PlatformType, string>();
            if (automaticSteamPath != null) ValidPlatformTypesToPaths[PlatformType.Steam] = automaticSteamPath;
            if (automaticWin10Path != null) ValidPlatformTypesToPaths[PlatformType.Win10] = automaticWin10Path;

            SyncIndependentConfigFromDisk();
            if (!string.IsNullOrEmpty(CustomBasePath))
            {
                string customGamePath = GetGamePathFromBasePath(CustomBasePath);
                if (!string.IsNullOrEmpty(customGamePath)) ValidPlatformTypesToPaths[PlatformType.Custom] = customGamePath;
            }

            RefreshAllPlatformsList();
            if (!ValidPlatformTypesToPaths.ContainsKey(Platform) && AllPlatforms.Count > 0) Platform = AllPlatforms[0];
            if (Program.CommandLineOptions.ServerMode) Platform = PlatformType.Server;

            DeterminePaths();
            SyncModsFromDisk();
            SyncDependentConfigFromDisk();
            VerifyGamePath();

            if (Program.CommandLineOptions.ServerMode && Directory.Exists(Path.Combine(BasePath, "Saved")))
            {
                GamePath = Path.GetFullPath(Path.Combine(BasePath, ".."));
            }

            if (GamePath == null || !Directory.Exists(GamePath))
            {
                GamePath = null;
                if (ValidPlatformTypesToPaths.ContainsKey(Platform))
                {
                    GamePath = ValidPlatformTypesToPaths[Platform];
                }
                else
                {
                    TextPrompt initialPathPrompt = new TextPrompt
                    {
                        StartPosition = FormStartPosition.CenterScreen,
                        DisplayText = "Select your game installation directory",
                        VerifyMode = VerifyPathMode.Game
                    };

                    if (initialPathPrompt.ShowDialog(BaseForm) == DialogResult.OK)
                    {
                        GamePath = initialPathPrompt.OutputText;
                        Platform = PlatformType.Custom;
                        ValidPlatformTypesToPaths[PlatformType.Custom] = GamePath;
                        RefreshAllPlatformsList();
                    }
                    else
                    {
                        Environment.Exit(0);
                    }
                }
            }

            ApplyGamePathDerivatives();
            VerifyIntegrity();

            foreach (Mod mod in Mods)
            {
                mod.Dirty = true;
            }
            FullUpdateSynchronous();
            SortMods();
            RefreshAllPriorites();
            SyncConfigToDisk();
        }

        public void VerifyGamePath()
        {
            bool doesValidPlatformTypesToPathsContainValue = false;
            foreach (KeyValuePair<PlatformType, string> entry in ValidPlatformTypesToPaths)
            {
                if (entry.Value.PathEquals(GamePath))
                {
                    doesValidPlatformTypesToPathsContainValue = true;
                    break;
                }
            }

            if (GamePath != null && (!doesValidPlatformTypesToPathsContainValue || !Directory.Exists(GamePath)))
            {
                // Here, a game path is being provided that we can't recognize. If the set platform is Win10, that probably means there is a new update and we should just discard the one already stored; otherwise, it is custom
                if (Platform == PlatformType.Win10 && ValidPlatformTypesToPaths.ContainsKey(PlatformType.Win10))
                {
                    GamePath = ValidPlatformTypesToPaths[PlatformType.Win10];
                }
                else if (Directory.Exists(GamePath))
                {
                    Platform = PlatformType.Custom;
                    ValidPlatformTypesToPaths[PlatformType.Custom] = GamePath;
                    RefreshAllPlatformsList();
                }
            }
        }

        public void RefreshAllPlatformsList()
        {
            AllPlatforms = ValidPlatformTypesToPaths.Keys.OrderBy(x => (int)x).ToList();
        }

        public string MicrosoftRuntimeID = "";
        private string CheckRegistryForMicrosoftStorePath()
        {
            RegistryKey key1 = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64).OpenSubKey(@"Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppModel\Repository\Packages"); // SystemEraSoftworks.29415440E1269_1.16.70.0_x64__ftk5pbg2rayv2
            if (key1 == null) return null;

            RegistryKey goalKey = null;
            foreach (string k in key1.GetSubKeyNames())
            {
                if (k.StartsWith("SystemEraSoftworks"))
                {
                    goalKey = key1.OpenSubKey(k);
                    break;
                }
            }
            key1.Close();
            if (goalKey == null) return null;

            string packageID = goalKey.GetValue("PackageID") as string;
            string rootFolder = goalKey.GetValue("PackageRootFolder") as string;
            goalKey.Close();

            if (!string.IsNullOrEmpty(packageID))
            {
                string[] idBits = packageID.Split('_');
                if (idBits.Length >= 2) MicrosoftRuntimeID = idBits[0] + "_" + idBits[idBits.Length - 1];
            }
            return rootFolder;
        }

        private static Regex acfEntryReader = new Regex(@"\s+""(\w+)""\s+""([\w ]+)""", RegexOptions.Compiled);
        private static Regex isInvalidVdfEntry = new Regex(@"[^""\d]", RegexOptions.Compiled);
        private string CheckRegistryForSteamPath(int appID)
        {
            string decidedSteamPath = null;
            RegistryKey key1 = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64).OpenSubKey(@"SOFTWARE\Wow6432Node\Valve\Steam");
            RegistryKey key2 = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32).OpenSubKey(@"SOFTWARE\Valve\Steam");
            if (key1 != null)
            {
                object o = key1.GetValue("SteamPath");
                if (o != null) decidedSteamPath = o as string;
            }
            else if (key2 != null)
            {
                object o = key2.GetValue("SteamPath");
                if (o != null) decidedSteamPath = o as string;
            }

            if (key1 != null) key1.Close();
            if (key2 != null) key2.Close();

            if (decidedSteamPath == null) return null;

            List<string> potentialInstallDirs = new List<string>();
            potentialInstallDirs.Add(decidedSteamPath);

            try
            {
                using (StreamReader f = new StreamReader(Path.Combine(decidedSteamPath, "steamapps", "libraryfolders.vdf")))
                {
                    string vdfEntry = null;
                    while ((vdfEntry = f.ReadLine()) != null)
                    {
                        string[] goodEntry = vdfEntry.Trim().Replace("\t\t", " ").Split(' ');
                        if (goodEntry.Length == 2 && !isInvalidVdfEntry.IsMatch(goodEntry[0]))
                        {
                            potentialInstallDirs.Add(goodEntry[1].Replace(@"\\", @"\").Replace("\"", ""));
                        }
                    }
                }
            }
            catch (IOException) { }

            try
            {
                using (StreamReader f = new StreamReader(Path.Combine(decidedSteamPath, "config", "config.vdf")))
                {
                    string vdfEntry = null;
                    while ((vdfEntry = f.ReadLine()) != null)
                    {
                        if (vdfEntry.Contains("BaseInstallFolder_"))
                        {
                            string[] goodEntry = vdfEntry.Trim().Replace("\t\t", " ").Split(' ');
                            if (goodEntry.Length == 2 && !isInvalidVdfEntry.IsMatch(goodEntry[0]))
                            {
                                potentialInstallDirs.Add(goodEntry[1].Replace(@"\\", @"\").Replace("\"", ""));
                            }
                        }
                    }
                }
            }
            catch (IOException) { }

            List<string> astroInstallDirs = new List<string>();
            foreach (string installPath in potentialInstallDirs)
            {
                string tempAstroInstallDir = null;
                if ((tempAstroInstallDir = CheckSteamPathForGame(appID, installPath)) != null)
                {
                    astroInstallDirs.Add(tempAstroInstallDir);
                }
            }

            if (astroInstallDirs.Count > 0)
            {
                return astroInstallDirs[0];
            }

            return null;
        }

        private string CheckSteamPathForGame(int appID, string steamPath)
        {
            try
            {
                using (StreamReader f = new StreamReader(Path.Combine(steamPath, "steamapps", "appmanifest_" + appID + ".acf")))
                {
                    string acfEntry = null;
                    while ((acfEntry = f.ReadLine()) != null)
                    {
                        Match m = acfEntryReader.Match(acfEntry);
                        if (m.Groups.Count == 3 && m.Groups[1].Value == "installdir")
                        {
                            string astroInstallDir = m.Groups[2].Value;
                            if (!string.IsNullOrEmpty(astroInstallDir))
                            {
                                string decidedAnswer = Path.Combine(steamPath, "steamapps", "common", astroInstallDir);
                                if (!Directory.Exists(decidedAnswer)) return null;
                                return decidedAnswer;
                            }
                        }
                    }
                }
            }
            catch (IOException)
            {
                return null;
            }
            return null;
        }

        private static Regex AstroBuildRegex = new Regex(@"^(\d+\.\d+\.\d+\.\d) \w+"); 
        public void ApplyGamePathDerivatives()
        {
            if (GamePath == null) return;

            // BinaryFilePath
            BinaryFilePath = null;

            try
            {
                string[] allExes = Directory.GetFiles(Path.Combine(GamePath, "Astro", "Binaries"), "*.exe", SearchOption.AllDirectories);
                if (allExes.Length > 0) BinaryFilePath = allExes[0];                
            }
            catch (Exception ex)
            {
                if (ex is IOException)
                {
                    BinaryFilePath = null;
                }
                else if (ex is UnauthorizedAccessException)
                {
                    MessageBox.Show("Unable to access an important directory! Please make sure the current user has read access for the folder \"" + GamePath + "\".", "Uh oh!");
                    Environment.Exit(0);
                }
                else
                {
                    throw;
                }
            }

            // Get astroneer version
            SetInstalledAstroBuild();
        }

        private void SetInstalledAstroBuild()
        {
            InstalledAstroBuild = null;

            // Steam
            try
            {
                string buildVersionPath = Path.Combine(GamePath, "build.version");
                if (File.Exists(buildVersionPath))
                {
                    using (StreamReader f = new StreamReader(buildVersionPath))
                    {
                        string fullBuildData = f.ReadToEnd();
                        Match m = AstroBuildRegex.Match(fullBuildData);
                        if (m.Groups.Count == 2) InstalledAstroBuild = new Version(m.Groups[1].Value);
                    }
                }
            }
            catch
            {
                InstalledAstroBuild = null;
            }

            if (InstalledAstroBuild != null) return;

            // Win10
            try
            {
                string buildVersionPath = Path.Combine(GamePath, "AppxManifest.xml");
                if (File.Exists(buildVersionPath))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(buildVersionPath);

                    if (doc != null)
                    {
                        XmlNodeList identityList = doc.GetElementsByTagName("Identity");
                        if (identityList != null && identityList.Count > 0)
                        {
                            var verAttr = identityList[0].Attributes["Version"];
                            if (verAttr != null) Version.TryParse(verAttr.Value, out InstalledAstroBuild);
                        }
                    }
                }
            }
            catch
            {
                InstalledAstroBuild = null;
            }
        }

        public void DeterminePaths()
        {
            BasePath = null;
            if (!string.IsNullOrEmpty(Program.CommandLineOptions.LocalDataPath))
            {
                BasePath = AMLUtils.FixBasePath(Path.GetFullPath(Path.Combine(Program.CommandLineOptions.LocalDataPath, "Astro")));
            }
            else
            {
                if (Program.CommandLineOptions.ServerMode)
                {
                    BasePath = Path.Combine(GamePath != null ? GamePath : Directory.GetCurrentDirectory(), "Astro");
                }
                else if (Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) != null)
                {
                    switch(Platform)
                    {
                        case PlatformType.Steam:
                            BasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Astro");
                            break;
                        case PlatformType.Win10:
                            BasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages", "SystemEraSoftworks.29415440E1269_ftk5pbg2rayv2", "LocalState", "Astro");
                            break;
                    }
                }
            }

            if (BasePath == null || !Directory.Exists(BasePath))
            {
                if (Platform == PlatformType.Custom || Platform == PlatformType.Unknown)
                {
                    if (!string.IsNullOrEmpty(CustomBasePath))
                    {
                        BasePath = CustomBasePath;
                    }
                    else
                    {
                        TextPrompt initialPathPrompt = new TextPrompt
                        {
                            StartPosition = FormStartPosition.CenterScreen,
                            DisplayText = "Select your local application data directory",
                            VerifyMode = VerifyPathMode.Base
                        };

                        if (initialPathPrompt.ShowDialog(BaseForm) == DialogResult.OK)
                        {
                            CustomBasePath = initialPathPrompt.OutputText;
                            BasePath = CustomBasePath;
                        }
                        else
                        {
                            Environment.Exit(0);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Unable to find the local application data directory. Please specify one with the --data parameter.", "Uh oh!");
                    Environment.Exit(0);
                }
            }

            DetermineBasePathDerivatives();
        }

        public void DetermineBasePathDerivatives()
        {
            try
            {
                DownloadPath = Path.Combine(BasePath, "Saved", "Mods");
                Directory.CreateDirectory(DownloadPath);
                InstallPath = Path.Combine(BasePath, "Saved", "Paks");
                Directory.CreateDirectory(InstallPath);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Unable to access an important directory! Please make sure the current user has read access for the folder \"" + BasePath + "\".", "Uh oh!");
                Environment.Exit(0);
            }
        }

        internal Metadata ExtractMetadataFromPath(string modPath)
        {
            if (new Mod(null, Path.GetFileName(modPath)).Priority >= 999) return null;

            try
            {
                using (FileStream f = new FileStream(modPath, FileMode.Open, FileAccess.Read))
                {
                    return new PakExtractor(new BinaryReader(f)).ReadMetadata();
                }
            }
            catch
            {
                return null;
            }
        }

        public ManualResetEvent IsUpdatingAvailableVersionsFromIndexFilesWaitHandler = new ManualResetEvent(true);
        public void UpdateAvailableVersionsFromIndexFiles()
        {
            IsUpdatingAvailableVersionsFromIndexFilesWaitHandler.Reset();
            Dictionary<Mod, Version> switchVersionInstructions = new Dictionary<Mod, Version>();
            foreach (Mod mod in Mods)
            {
                Version latestVersion = null;
                if (mod.AvailableVersions.Count > 0) latestVersion = mod.AvailableVersions[0];
                if (GlobalIndexFile.ContainsKey(mod.CurrentModData.ModID))
                {
                    IndexMod indexMod = GlobalIndexFile[mod.CurrentModData.ModID];
                    mod.AvailableVersions.AddRange(indexMod.AllVersions.Keys.Except(mod.AvailableVersions));
                    mod.AvailableVersions.Sort();
                    mod.AvailableVersions.Reverse();
                    latestVersion = mod.AvailableVersions[0];
                    //if (indexMod.LatestVersion != null) latestVersion = indexMod.LatestVersion;
                }

                if (mod.ForceLatest && latestVersion != null) switchVersionInstructions.Add(mod, latestVersion);
            }

            AMLUtils.InvokeUI(BaseForm.TableManager.Refresh);

            foreach (KeyValuePair<Mod, Version> entry in switchVersionInstructions)
            {
                BaseForm.SwitchVersionSync(entry.Key, entry.Value);
            }
            IsUpdatingAvailableVersionsFromIndexFilesWaitHandler.Set();
        }

        private List<string> DuplicateURLs = new List<string>();

        public void ResetGlobalIndexFile()
        {
            GlobalIndexFile = new Dictionary<string, IndexMod>();
            DuplicateURLs = new List<string>();
        }

        public void AggregateIndexFiles()
        {
            if (GlobalIndexFile == null) GlobalIndexFile = new Dictionary<string, IndexMod>();
            foreach (Mod mod in Mods)
            {
                IndexFile thisIndexFile = mod.GetIndexFile(DuplicateURLs);
                if (thisIndexFile != null)
                {
                    thisIndexFile.Mods.ToList().ForEach(x => GlobalIndexFile[x.Key] = x.Value);
                    DuplicateURLs.Add(thisIndexFile.OriginalURL);
                }
            }

            UpdateAvailableVersionsFromIndexFiles();
        }

        public void SortVersions()
        {
            foreach (Mod mod in Mods)
            {
                mod.AvailableVersions.Sort();
                mod.AvailableVersions.Reverse();
            }
        }

        private void SafeDelete(string targetPath)
        {
            try
            {
                File.Delete(targetPath);
            }
            catch { }
        }

        // It's counterproductive to try and combat piracy, but it's a good idea to have some visible marker of it to serve as an explanation for the problems people with pirated copies may face
        public bool MismatchedSteamworksDLL = false;
        private static readonly byte[] steamsworksDLLHash = new byte[] { 231, 116, 51, 9, 76, 86, 67, 54, 133, 166, 138, 68, 54, 232, 27, 118, 195, 181, 225, 245 };
        public void VerifyIntegrity()
        {
            MismatchedSteamworksDLL = false;

            try
            {
                string potentialDllDir = Path.Combine(GamePath, "Engine", "Binaries", "ThirdParty", "Steamworks");
                if (Directory.Exists(potentialDllDir))
                {
                    string[] dllPaths = Directory.GetFiles(potentialDllDir, "steam_api*.dll", SearchOption.AllDirectories);
                    if (dllPaths.Length > 0 && File.Exists(dllPaths[0]))
                    {
                        var data = File.ReadAllBytes(dllPaths[0]);
                        var hash = SHA1.Create().ComputeHash(data);
                        if (!hash.SequenceEqual(steamsworksDLLHash)) MismatchedSteamworksDLL = true;
                    }
                }
            }
            catch
            {
                MismatchedSteamworksDLL = false;
            }
        }

        public void EviscerateMod(Mod targetMod, List<Version> targetVersions = null)
        {
            string[] allNormalMods = Directory.GetFiles(DownloadPath, "*.pak", SearchOption.TopDirectoryOnly);
            string[] installedMods = Directory.GetFiles(InstallPath, "*.pak", SearchOption.TopDirectoryOnly);
            string[] allMods = new string[allNormalMods.Length + installedMods.Length];
            Array.Copy(allNormalMods, allMods, allNormalMods.Length);
            Array.Copy(installedMods, 0, allMods, allNormalMods.Length, installedMods.Length);

            foreach (string modPath in allMods)
            {
                string modNameOnDisk = Path.GetFileName(modPath);
                Mod newMod = new Mod(ExtractMetadataFromPath(modPath), modNameOnDisk);
                if (newMod.CurrentModData.ModID == targetMod.CurrentModData.ModID && (targetVersions == null || targetVersions.Contains(newMod.InstalledVersion)))
                {
                    SafeDelete(modPath);
                }
            }

            for (int i = 0; i < Mods.Count; i++)
            {
                if (Mods[i].CurrentModData.ModID == targetMod.CurrentModData.ModID)
                {
                    if (targetVersions != null)
                    {
                        foreach (Version targetVersion in targetVersions)
                        {
                            Mods[i].AllModData.Remove(targetVersion);
                            Mods[i].AvailableVersions.Remove(targetVersion);
                        }

                        if (Mods[i].AvailableVersions.Count > 0)
                        {
                            Mods[i].AvailableVersions.Sort();
                            Mods[i].AvailableVersions.Reverse();
                            Mods[i].InstalledVersion = Mods[i].AvailableVersions[0];
                            continue;
                        }
                    }
                    ModLookup.Remove(Mods[i].CurrentModData.ModID);
                    Mods.RemoveAt(i);
                    i--;
                }
            }
        }

        public void SyncSingleMod(Mod newMod)
        {
            if (ModLookup.ContainsKey(newMod.CurrentModData.ModID))
            {
                if (!ModLookup[newMod.CurrentModData.ModID].AvailableVersions.Contains(newMod.InstalledVersion)) ModLookup[newMod.CurrentModData.ModID].AvailableVersions.Add(newMod.InstalledVersion);
                ModLookup[newMod.CurrentModData.ModID].AllModData[newMod.InstalledVersion] = newMod.CurrentModData;
            }
            else
            {
                Mods.Add(newMod);
                ModLookup.Add(newMod.CurrentModData.ModID, newMod);
            }
        }

        public Mod SyncSingleModFromDisk(string modPath, out bool wasClientOnly, bool updateSort = true)
        {
            Mod newMod = new Mod(ExtractMetadataFromPath(modPath), Path.GetFileName(modPath));
            if (Program.CommandLineOptions.ServerMode && newMod.CurrentModData.Sync == SyncMode.ClientOnly)
            {
                wasClientOnly = true;
                return null;
            }

            SyncSingleMod(newMod);
            if (updateSort)
            {
                SortVersions();
                SortMods();
            }

            wasClientOnly = false;
            return newMod;
        }

        public void SyncModsFromDisk(bool skipIndexing = false)
        {
            if (!skipIndexing)
            {
                Mods = new List<Mod>();

                string[] allMods = Directory.GetFiles(DownloadPath, "*_P.pak", SearchOption.TopDirectoryOnly);
                ModLookup = new Dictionary<string, Mod>();
                foreach (string modPath in allMods)
                {
                    SyncSingleModFromDisk(modPath, out _, false);
                }
            }

            SortVersions();
            foreach (Mod mod in Mods)
            {
                mod.InstalledVersion = mod.AvailableVersions[0];
            }

            string[] installedMods = Directory.GetFiles(InstallPath, "*_P.pak", SearchOption.TopDirectoryOnly);
            foreach (string modPath in installedMods)
            {
                var modNameOnDisk = Path.GetFileName(modPath);
                var m = new Mod(ExtractMetadataFromPath(modPath), modNameOnDisk);

                if (ModLookup.ContainsKey(m.CurrentModData.ModID))
                {
                    ModLookup[m.CurrentModData.ModID].Enabled = true;
                    ModLookup[m.CurrentModData.ModID].NameOnDisk = modNameOnDisk;
                    ModLookup[m.CurrentModData.ModID].InstalledVersion = m.InstalledVersion;
                    ModLookup[m.CurrentModData.ModID].Priority = m.Priority;
                    if (!ModLookup[m.CurrentModData.ModID].AvailableVersions.Contains(m.InstalledVersion))
                    {
                        File.Copy(modPath, Path.Combine(DownloadPath, modNameOnDisk));
                        ModLookup[m.CurrentModData.ModID].AvailableVersions.Add(m.InstalledVersion);
                        ModLookup[m.CurrentModData.ModID].AllModData.Add(m.InstalledVersion, m.CurrentModData);
                    }
                }
                else
                {
                    //if (Program.CommandLineOptions.ServerMode && m.CurrentModData.Sync == SyncMode.ClientOnly) continue;
                    if (m.Priority < 999)
                    {
                        File.Copy(modPath, Path.Combine(DownloadPath, modNameOnDisk), true);
                        m.Enabled = true;
                        Mods.Add(m);
                        ModLookup.Add(m.CurrentModData.ModID, m);
                    }
                }
            }

            SortMods();
        }

        public void SortMods()
        {
            Mods = new List<Mod>(Mods.OrderBy(o => o.Priority).ToList());
        }

        public long GetSizeOnDisk(Mod mod)
        {
            string[] allMods = Directory.GetFiles(DownloadPath, "*.pak", SearchOption.TopDirectoryOnly);
            string copyingPath = null;
            foreach (string modPath in allMods)
            {
                Mod testMod = new Mod(ExtractMetadataFromPath(modPath), Path.GetFileName(modPath));
                if ((testMod.CurrentModData.ModID == mod.CurrentModData.ModID || testMod.NameOnDisk == mod.NameOnDisk) && testMod.InstalledVersion == mod.InstalledVersion)
                {
                    copyingPath = modPath;
                    break;
                }
            }
            if (string.IsNullOrEmpty(copyingPath)) return -1;
            return new FileInfo(copyingPath).Length;
        }

        public void SyncModsToDisk()
        {
            if (IsReadOnly) return;
            foreach (Mod mod in Mods)
            {
                if (mod.Dirty)
                {
                    string destinedName = mod.ConstructName();
                    File.Delete(Path.Combine(InstallPath, mod.NameOnDisk));
                    if (mod.Enabled)
                    {
                        string[] allMods = Directory.GetFiles(DownloadPath, "*.pak", SearchOption.TopDirectoryOnly);
                        string copyingPath = null;
                        foreach (string modPath in allMods)
                        {
                            Mod testMod = new Mod(ExtractMetadataFromPath(modPath), Path.GetFileName(modPath));
                            if ((testMod.CurrentModData.ModID == mod.CurrentModData.ModID || testMod.NameOnDisk == mod.NameOnDisk) && testMod.InstalledVersion == mod.InstalledVersion)
                            {
                                copyingPath = modPath;
                                break;
                            }
                        }

                        if (!string.IsNullOrEmpty(copyingPath))
                        {
                            File.Copy(copyingPath, Path.Combine(InstallPath, destinedName), true);
                            mod.NameOnDisk = destinedName;
                        }
                    }
                    mod.Dirty = false;
                }
            }
        }

        private PlatformType FirstRecordedIndependentConfigPlatform = PlatformType.Unknown;
        public void SyncIndependentConfigFromDisk()
        {
            IndependentConfig independentConfig = null;
            try
            {
                independentConfig = JsonConvert.DeserializeObject<IndependentConfig>(File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AstroModLoader", "config.json")));
            }
            catch
            {
                independentConfig = null;
            }

            if (independentConfig != null)
            {
                if (!string.IsNullOrEmpty(independentConfig.AccentColor))
                {
                    try
                    {
                        AMLPalette.AccentColor = AMLUtils.ColorFromHTML(independentConfig.AccentColor);
                    }
                    catch { }
                }
                AMLPalette.CurrentTheme = independentConfig.Theme;
                AMLPalette.RefreshTheme(BaseForm);

                Platform = independentConfig.Platform;
                if (FirstRecordedIndependentConfigPlatform == PlatformType.Unknown) FirstRecordedIndependentConfigPlatform = independentConfig.Platform;
                if (!string.IsNullOrEmpty(independentConfig.CustomBasePath)) CustomBasePath = independentConfig.CustomBasePath;
                if (!string.IsNullOrEmpty(independentConfig.PlayFabCustomID)) PlayFabAPI.CustomID = independentConfig.PlayFabCustomID;
                if (!string.IsNullOrEmpty(independentConfig.PlayFabToken)) PlayFabAPI.Token = independentConfig.PlayFabToken;
            }
        }

        public string GetGamePathFromBasePath(string basePath)
        {
            ModConfig diskConfig = null;
            try
            {
                diskConfig = JsonConvert.DeserializeObject<ModConfig>(File.ReadAllText(Path.Combine(basePath, "Saved", "Mods", "modconfig.json")));
            }
            catch
            {
                diskConfig = null;
            }

            return diskConfig?.GamePath;
        }

        public void SyncDependentConfigFromDisk(bool includeGamePath = true)
        {
            ModConfig diskConfig = null;
            try
            {
                diskConfig = JsonConvert.DeserializeObject<ModConfig>(File.ReadAllText(Path.Combine(DownloadPath, "modconfig.json")));
            }
            catch
            {
                diskConfig = null;
            }
            if (diskConfig != null)
            {
                ApplyProfile(diskConfig.ModsOnDisk, false);
                ProfileList = diskConfig.Profiles;
                if (ProfileList == null) ProfileList = new Dictionary<string, ModProfile>();
                if (!string.IsNullOrEmpty(diskConfig.LaunchCommand)) LaunchCommand = diskConfig.LaunchCommand;
                OurIntegrator.RefuseMismatchedConnections = diskConfig.RefuseMismatchedConnections;
                if (includeGamePath)
                {
                    if (!string.IsNullOrEmpty(diskConfig.GamePath)) GamePath = diskConfig.GamePath;

                    KeyValuePair<PlatformType, string> prospectivePlatform = ValidPlatformTypesToPaths.FirstOrDefault(x => x.Value == GamePath);
                    if (Platform == PlatformType.Unknown && !prospectivePlatform.Equals(default(KeyValuePair<PlatformType, string>))) Platform = prospectivePlatform.Key;
                }
            }
        }

        public void SyncConfigFromDisk()
        {
            SyncIndependentConfigFromDisk();
            SyncDependentConfigFromDisk();
        }

        public void ApplyProfile(ModProfile prof, bool disableAllMods = true)
        {
            if (prof == null) return;
            if (disableAllMods)
            {
                foreach (KeyValuePair<string, Mod> rawEntry in ModLookup)
                {
                    ModLookup[rawEntry.Key].Enabled = false;
                }
            }
            
            foreach (KeyValuePair<string, Mod> entry in prof.ProfileData)
            {
                if (ModLookup.ContainsKey(entry.Key))
                {
                    ModLookup[entry.Key].Enabled = entry.Value.Enabled;
                    if (entry.Value.InstalledVersion != null) ModLookup[entry.Key].InstalledVersion = entry.Value.InstalledVersion;
                    ModLookup[entry.Key].Priority = entry.Value.Priority;
                    ModLookup[entry.Key].IsOptional = entry.Value.IsOptional;
                    ModLookup[entry.Key].ForceLatest = entry.Value.ForceLatest;
                    if (entry.Value.ForceLatest || !ModLookup[entry.Key].AllModData.ContainsKey(ModLookup[entry.Key].InstalledVersion))
                    {
                        ModLookup[entry.Key].InstalledVersion = ModLookup[entry.Key].AvailableVersions[0];
                    }
                }
            }
        }

        public ModProfile GenerateProfile()
        {
            var res = new ModProfile(new Dictionary<string, Mod>());
            foreach (Mod mod in Mods)
            {
                res.ProfileData.Add(mod.CurrentModData.ModID, (Mod)mod.Clone());
            }
            return res;
        }

        public void SyncIndependentConfigToDisk()
        {
            var newIndConfig = new IndependentConfig();
            if (Program.CommandLineOptions.ServerMode)
            {
                newIndConfig.Platform = FirstRecordedIndependentConfigPlatform;
            }
            else
            {
                newIndConfig.Platform = Platform;
            }
            newIndConfig.Theme = AMLPalette.CurrentTheme;
            newIndConfig.AccentColor = AMLUtils.ColorToHTML(AMLPalette.AccentColor);
            newIndConfig.CustomBasePath = CustomBasePath;
            newIndConfig.PlayFabCustomID = PlayFabAPI.CustomID;
            newIndConfig.PlayFabToken = PlayFabAPI.Token;

            Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AstroModLoader"));
            File.WriteAllBytes(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AstroModLoader", "config.json"), Encoding.UTF8.GetBytes(AMLUtils.SerializeObject(newIndConfig)));
        }

        public void SyncDependentConfigToDisk()
        {
            var newConfig = new ModConfig();
            newConfig.GamePath = GamePath;
            newConfig.LaunchCommand = LaunchCommand;
            newConfig.RefuseMismatchedConnections = OurIntegrator.RefuseMismatchedConnections;
            newConfig.Profiles = ProfileList;
            newConfig.ModsOnDisk = GenerateProfile();

            File.WriteAllBytes(Path.Combine(DownloadPath, "modconfig.json"), Encoding.UTF8.GetBytes(AMLUtils.SerializeObject(newConfig)));
        }

        public void SyncConfigToDisk()
        {
            AMLUtils.InvokeUI(() =>
            {
                SyncDependentConfigToDisk();
                SyncIndependentConfigToDisk();
            });
        }

        public static ModIntegrator OurIntegrator;
        public void IntegrateMods()
        {
            if (IsReadOnly || GamePath == null || InstallPath == null) return;

            List<string> optionalMods = new List<string>();
            foreach (Mod mod in Mods)
            {
                if (mod.Enabled && mod.IsOptional) optionalMods.Add(mod.CurrentModData.ModID);
            }

            if (TableHandler.ShouldContainOptionalColumn()) OurIntegrator.OptionalModIDs = optionalMods;
            OurIntegrator.IntegrateMods(InstallPath, Path.Combine(GamePath, "Astro", "Content", "Paks"));
        }

        public void RefreshAllPriorites()
        {
            for (int i = 0; i < Mods.Count; i++) Mods[i].Priority = i + 1; // The mod loader should never save a mod's priority as 0 to disk so that external applications can use 0 to force a mod to always load first
        }

        public void SwapMod(Mod previouslySelectedMod, int newModIndex, bool updateAutomatically = true)
        {
            // First we remove the old position of the mod we're changing
            for (int i = 0; i < Mods.Count; i++)
            {
                if (object.ReferenceEquals(Mods[i], previouslySelectedMod))
                {
                    Mods.RemoveAt(i);
                    break;
                }
            }

            Mods.Insert(newModIndex, previouslySelectedMod); // Insert at the new location
            RefreshAllPriorites(); // Refresh priority list
            if (updateAutomatically)
            {
                foreach (Mod mod in Mods) mod.Dirty = true; // Update all the priorities on disk to be safe
                FullUpdate();
            }
        }

        public bool GetReadOnly()
        {
            try
            {
                string targetPath = Path.Combine(InstallPath, "999-AstroModIntegrator_P.pak");
                if (!File.Exists(targetPath)) return false;
                using (FileStream f = new FileStream(targetPath, FileMode.Open, FileAccess.Write, FileShare.None)) { }
            }
            catch (IOException)
            {
                return true;
            }
            return false;
        }

        public void UpdateReadOnlyStatus()
        {
            bool nextReadOnlyState = GetReadOnly();
            if (nextReadOnlyState != IsReadOnly)
            {
                IsReadOnly = nextReadOnlyState;
                BaseForm.FullRefresh();
            }
        }

        private Semaphore fullUpdateSemaphore = new Semaphore(1, 1);
        public Task FullUpdate()
        {
            return Task.Run(() =>
            {
                if (!fullUpdateSemaphore.WaitOne(10000)) return;
                FullUpdateSynchronous(true);
            });
        }

        public void FullUpdateSynchronous(bool releaseSemaphore = false)
        {
            UpdateReadOnlyStatus();
            try
            {
                Directory.CreateDirectory(DownloadPath);
                Directory.CreateDirectory(InstallPath);

                SyncConfigToDisk();
                SyncModsToDisk();
                IntegrateMods();
            }
            catch (Exception ex)
            {
                if (releaseSemaphore) fullUpdateSemaphore.Release();
                if (ex is IOException || ex is FileNotFoundException)
                {
                    IsReadOnly = true;
                    return;
                }
                throw;
            }

            if (releaseSemaphore) fullUpdateSemaphore.Release();
        }
    }
}
