using AstroModIntegrator;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        public bool IsReadOnly = false;

        public ModHandler(Form1 baseForm)
        {
            BaseForm = baseForm;

            string automaticSteamPath = null;
            string automaticWin10Path = null;
            try
            {
                if (Program.CommandLineOptions.ServerMode)
                {
                    if (Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Astro")))
                    {
                        automaticSteamPath = Directory.GetCurrentDirectory();
                    }
                    else
                    {
                        automaticSteamPath = CheckRegistryForSteamPath(728470); // Astroneer Dedicated Server: 728470
                    }
                }
                else
                {
                    automaticSteamPath = CheckRegistryForSteamPath(361420); // Astroneer: 361420
                    automaticWin10Path = CheckRegistryForMicrosoftStorePath();
                }
            }
            catch { }

            //automaticSteamPath = null;
            //automaticWin10Path = null;

            ValidPlatformTypesToPaths = new Dictionary<PlatformType, string>();
            if (automaticSteamPath != null) ValidPlatformTypesToPaths[PlatformType.Steam] = automaticSteamPath;
            if (automaticWin10Path != null) ValidPlatformTypesToPaths[PlatformType.Win10] = automaticWin10Path;

            SyncIndependentConfigFromDisk();

            RefreshAllPlatformsList();
            if ((Platform != PlatformType.Custom && !ValidPlatformTypesToPaths.ContainsKey(Platform)) && AllPlatforms.Count > 0) Platform = AllPlatforms[0];
            if (Program.CommandLineOptions.ServerMode) Platform = PlatformType.Server;

            DeterminePaths();
            SyncModsFromDisk();
            SyncDependentConfigFromDisk();

            if (GamePath != null && !ValidPlatformTypesToPaths.ContainsValue(GamePath))
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
                        DisplayText = "Select your game installation directory"
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

            foreach (Mod mod in Mods)
            {
                mod.Dirty = true;
            }
            FullUpdate();
            SortMods();
            RefreshAllPriorites();
            SyncConfigToDisk();
        }

        public void RefreshAllPlatformsList()
        {
            AllPlatforms = ValidPlatformTypesToPaths.Keys.OrderBy(x => (int)x).ToList();
        }

        public string MicrosoftRuntimeID = "";
        private string CheckRegistryForMicrosoftStorePath()
        {
            RegistryKey key1 = Registry.CurrentUser.OpenSubKey(@"Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppModel\Repository\Packages"); // SystemEraSoftworks.29415440E1269_1.16.70.0_x64__ftk5pbg2rayv2
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
        private string CheckRegistryForSteamPath(int appID)
        {
            string decidedSteamPath = null;
            RegistryKey key1 = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Valve\Steam");
            RegistryKey key2 = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Valve\Steam");
            if (key1 != null)
            {
                object o = key1.GetValue("InstallPath");
                if (o != null) decidedSteamPath = o as string;
            }
            else if (key2 != null)
            {
                object o = key2.GetValue("InstallPath");
                if (o != null) decidedSteamPath = o as string;
            }

            if (key1 != null) key1.Close();
            if (key2 != null) key2.Close();

            if (decidedSteamPath == null) return null;

            List<string> potentialInstallDirs = new List<string>();
            potentialInstallDirs.Add(decidedSteamPath);
            using (StreamReader f = new StreamReader(Path.Combine(decidedSteamPath, "config", "config.vdf")))
            {
                string vdfEntry = null;
                while ((vdfEntry = f.ReadLine()) != null)
                {
                    if (vdfEntry.Contains("BaseInstallFolder_"))
                    {
                        potentialInstallDirs.Add(vdfEntry.Trim().Replace("		", " ").Split(' ')[1].Replace(@"\\", @"\").Replace("\"", ""));
                    }
                }
            }

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

        private string CheckSteamPathForGame(int appID, string SteamPath)
        {
            try
            {
                using (StreamReader f = new StreamReader(Path.Combine(SteamPath, "steamapps", "appmanifest_" + appID + ".acf")))
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
                                string decidedAnswer = Path.Combine(SteamPath, "steamapps", "common", astroInstallDir);
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
            catch (IOException)
            {
                BinaryFilePath = null;
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
                using (StreamReader f = new StreamReader(Path.Combine(GamePath, "build.version")))
                {
                    string fullBuildData = f.ReadToEnd();
                    Match m = AstroBuildRegex.Match(fullBuildData);
                    if (m.Groups.Count == 2) InstalledAstroBuild = new Version(m.Groups[1].Value);
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
                XmlDocument doc = new XmlDocument();
                doc.Load(Path.Combine(GamePath, "AppxManifest.xml"));

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
            catch (IOException)
            {
                InstalledAstroBuild = null;
            }
        }

        public void DeterminePaths()
        {
            BasePath = null;
            if (!string.IsNullOrEmpty(Program.CommandLineOptions.LocalDataPath))
            {
                BasePath = Path.GetFullPath(Path.Combine(Program.CommandLineOptions.LocalDataPath, "Astro"));
            }
            else
            {
                if (Program.CommandLineOptions.ServerMode)
                {
                    BasePath = Path.Combine(GamePath != null ? GamePath : Directory.GetCurrentDirectory(), "Astro");
                }
                else if (Environment.GetEnvironmentVariable("LocalAppData") != null)
                {
                    switch(Platform)
                    {
                        case PlatformType.Steam:
                            BasePath = Path.Combine(Environment.GetEnvironmentVariable("LocalAppData"), "Astro");
                            break;
                        case PlatformType.Win10:
                            BasePath = Path.Combine(Environment.GetEnvironmentVariable("LocalAppData"), "Packages", "SystemEraSoftworks.29415440E1269_ftk5pbg2rayv2", "LocalState", "Astro");
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
                        bool isValidPath = false;
                        while (!isValidPath)
                        {
                            TextPrompt initialPathPrompt = new TextPrompt
                            {
                                StartPosition = FormStartPosition.CenterScreen,
                                DisplayText = "Select your local application data directory"
                            };

                            if (initialPathPrompt.ShowDialog(BaseForm) == DialogResult.OK)
                            {
                                CustomBasePath = initialPathPrompt.OutputText;
                                if (Path.GetFileName(initialPathPrompt.OutputText) != "Astro") CustomBasePath = Path.Combine(initialPathPrompt.OutputText, "Astro");

                                if (Directory.Exists(CustomBasePath))
                                {
                                    isValidPath = true;
                                    BasePath = CustomBasePath;
                                }
                            }
                            else
                            {
                                Environment.Exit(0);
                            }
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
            DownloadPath = Path.Combine(BasePath, "Saved", "Mods");
            Directory.CreateDirectory(DownloadPath);
            InstallPath = Path.Combine(BasePath, "Saved", "Paks");
            Directory.CreateDirectory(InstallPath);
        }

        private Metadata ExtractMetadataFromPath(string modPath)
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

        public void UpdateAvailableVersionsFromIndexFiles()
        {
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

                if (mod.ForceLatest && latestVersion != null) BaseForm.SwitchVersionSync(mod, latestVersion);
            }
            FullUpdate();
        }

        public void AggregateIndexFiles()
        {
            if (GlobalIndexFile == null) GlobalIndexFile = new Dictionary<string, IndexMod>();
            List<string> DuplicateURLs = new List<string>();
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

        public void EviscerateMod(Mod targetMod)
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
                if (newMod.CurrentModData.ModID == targetMod.CurrentModData.ModID)
                {
                    SafeDelete(modPath);
                }
            }

            for (int i = 0; i < Mods.Count; i++)
            {
                if (Mods[i].CurrentModData.ModID == targetMod.CurrentModData.ModID)
                {
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

                string[] allMods = Directory.GetFiles(DownloadPath, "*.pak", SearchOption.TopDirectoryOnly);
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

            string[] installedMods = Directory.GetFiles(InstallPath, "*.pak", SearchOption.TopDirectoryOnly);
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
                    if (Program.CommandLineOptions.ServerMode && m.CurrentModData.Sync == SyncMode.ClientOnly) continue;
                    if (m.Priority < 999)
                    {
                        File.Copy(modPath, Path.Combine(DownloadPath, modNameOnDisk));
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

        public void SyncDependentConfigFromDisk()
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
                if (!string.IsNullOrEmpty(diskConfig.GamePath)) GamePath = diskConfig.GamePath;

                KeyValuePair<PlatformType, string> prospectivePlatform = ValidPlatformTypesToPaths.FirstOrDefault(x => x.Value == GamePath);
                if (Platform == PlatformType.Unknown && !prospectivePlatform.Equals(default(KeyValuePair<PlatformType, string>))) Platform = prospectivePlatform.Key;
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
                    ModLookup[entry.Key].ForceLatest = entry.Value.ForceLatest;
                    if (entry.Value.ForceLatest || !ModLookup[entry.Key].AvailableVersions.Contains(ModLookup[entry.Key].InstalledVersion))
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

        public void SyncConfigToDisk()
        {
            var newConfig = new ModConfig();
            newConfig.GamePath = GamePath;
            newConfig.LaunchCommand = LaunchCommand;
            newConfig.Profiles = ProfileList;
            newConfig.ModsOnDisk = GenerateProfile();

            File.WriteAllBytes(Path.Combine(DownloadPath, "modconfig.json"), Encoding.UTF8.GetBytes(AMLUtils.SerializeObject(newConfig)));

            SyncIndependentConfigToDisk();
        }

        public void IntegrateMods()
        {
            if (IsReadOnly) return;
            if (GamePath == null || InstallPath == null) return;
            ModIntegrator.IntegrateMods(InstallPath, Path.Combine(GamePath, "Astro", "Content", "Paks"));
        }

        public void RefreshAllPriorites()
        {
            for (int i = 0; i < Mods.Count; i++) Mods[i].Priority = i + 1; // The mod loader should never save a mod's priority as 0 to disk, so that external applications can use 0 to force a mod to always load first
        }

        public void SwapMod(Mod previouslySelectedMod, int newModIndex)
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
            foreach (Mod mod in Mods) mod.Dirty = true; // Update all the priorities on disk to be safe
            FullUpdate();
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

        public void FullUpdate()
        {
            UpdateReadOnlyStatus();
            try
            {
                Directory.CreateDirectory(DownloadPath);
                Directory.CreateDirectory(InstallPath);

                IntegrateMods();
                SyncConfigToDisk();
                SyncModsToDisk();
            }
            catch (Exception ex)
            {
                if (ex is IOException || ex is FileNotFoundException)
                {
                    IsReadOnly = true;
                    return;
                }
                throw;
            }
        }
    }
}
