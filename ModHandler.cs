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
using System.Windows.Forms;

namespace AstroModLoader
{
    public class ModHandler
    {
        internal string BasePath;
        internal string DownloadPath;
        internal string InstallPath;
        internal string GamePath;
        internal Dictionary<string, Mod> ModLookup;
        internal string BinaryFilePath;
        internal Form1 BaseForm;

        public List<Mod> Mods;
        public Dictionary<string, ModProfile> ProfileList;
        public Version InstalledAstroBuild = null;
        public bool IsReadOnly = false;

        public ModHandler(Form1 baseForm)
        {
            BaseForm = baseForm;
            DeterminePaths();
            SyncModsFromDisk();
            SyncConfigFromDisk();

            if (GamePath == null)
            {
                string automaticSteamPath = null;
                try
                {
                    automaticSteamPath = CheckRegistryForSteamPath();
                }
                catch (UnauthorizedAccessException)
                {
                    automaticSteamPath = null;
                }

                if (automaticSteamPath != null)
                {
                    GamePath = automaticSteamPath;
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
                    }
                    else
                    {
                        MessageBox.Show("Mod integration will be disabled until you select your game installation directory!", "Uh oh!");
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

        private static Regex acfEntryReader = new Regex(@"\s+""(\w+)""\s+""(\w+)""", RegexOptions.Compiled);
        private string CheckRegistryForSteamPath()
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

            if (decidedSteamPath == null) return null;

            string astroInstallDir = null;
            using (StreamReader f = new StreamReader(Path.Combine(decidedSteamPath, "steamapps", "appmanifest_361420.acf")))
            {
                string acfEntry = null;
                while ((acfEntry = f.ReadLine()) != null)
                {
                    Match m = acfEntryReader.Match(acfEntry);
                    if (m.Groups.Count == 3 && m.Groups[1].Value == "installdir")
                    {
                        astroInstallDir = m.Groups[2].Value;
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(astroInstallDir))
            {
                string decidedAnswer = Path.Combine(decidedSteamPath, "steamapps", "common", astroInstallDir);
                if (!Directory.Exists(decidedAnswer)) return null;
                return decidedAnswer;
            }

            return null;
        }

        private static Regex AstroBuildRegex = new Regex(@"^(\d+\.\d+\.\d+\.\d) Shipping"); 
        public void ApplyGamePathDerivatives()
        {
            // BinaryFilePath
            if (GamePath != null)
            {
                BinaryFilePath = null;
                string[] allExes = Directory.GetFiles(Path.Combine(GamePath, "Astro", "Binaries"), "*.exe", SearchOption.AllDirectories);
                if (allExes.Length > 0) BinaryFilePath = allExes[0];
            }

            // Get astroneer version
            InstalledAstroBuild = null;
            try
            {
                using (StreamReader f = new StreamReader(Path.Combine(GamePath, "build.version")))
                {
                    string fullBuildData = f.ReadToEnd();
                    Match m = AstroBuildRegex.Match(fullBuildData);
                    if (m.Groups.Count == 2) InstalledAstroBuild = new Version(m.Groups[1].Value);
                }
            }
            catch (IOException)
            {
                InstalledAstroBuild = null;
            }
        }

        private void DeterminePaths()
        {
            if (!string.IsNullOrEmpty(Program.CommandLineOptions.BasePath))
            {
                BasePath = Program.CommandLineOptions.BasePath;
            }
            else
            {
                BasePath = Path.Combine(Environment.GetEnvironmentVariable("LocalAppData"), "Astro");
            }
            DownloadPath = Path.Combine(BasePath, "Saved", "Mods");
            Directory.CreateDirectory(DownloadPath);
            InstallPath = Path.Combine(BasePath, "Saved", "Paks");
            Directory.CreateDirectory(InstallPath);
        }

        private Metadata ExtractMetadataFromPath(string modPath)
        {
            using (FileStream f = new FileStream(modPath, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    return new PakExtractor(new BinaryReader(f)).ReadMetadata();
                }
                catch
                {
                    return null;
                }
            }
        }

        public void SyncModsFromDisk()
        {
            Mods = new List<Mod>();

            string[] allMods = Directory.GetFiles(DownloadPath, "*.pak", SearchOption.TopDirectoryOnly);
            ModLookup = new Dictionary<string, Mod>();
            foreach (string modPath in allMods)
            {
                Mod newMod = new Mod(ExtractMetadataFromPath(modPath), Path.GetFileName(modPath));
                if (ModLookup.ContainsKey(newMod.CurrentModData.ModID))
                {
                    ModLookup[newMod.CurrentModData.ModID].AvailableVersions.Add(newMod.InstalledVersion);
                    ModLookup[newMod.CurrentModData.ModID].AllModData.Add(newMod.InstalledVersion, newMod.CurrentModData);
                }
                else
                {
                    Mods.Add(newMod);
                    ModLookup.Add(newMod.CurrentModData.ModID, newMod);
                }
            }

            foreach (Mod mod in Mods)
            {
                mod.AvailableVersions.Sort();
                mod.AvailableVersions.Reverse();
                mod.InstalledVersion = mod.AvailableVersions[0];
            }

            string[] installedMods = Directory.GetFiles(InstallPath, "*.pak", SearchOption.TopDirectoryOnly);
            foreach (string modPath in installedMods)
            {
                var modNameOnDisk = Path.GetFileName(modPath);
                var m = new Mod(null, modNameOnDisk);

                if (ModLookup.ContainsKey(m.CurrentModData.ModID))
                {
                    // TODO: copy if new version is in Paks folder but not Mods
                    ModLookup[m.CurrentModData.ModID].Enabled = true;
                    ModLookup[m.CurrentModData.ModID].NameOnDisk = modNameOnDisk;
                    ModLookup[m.CurrentModData.ModID].InstalledVersion = m.InstalledVersion;
                    ModLookup[m.CurrentModData.ModID].Priority = m.Priority;
                    if (!ModLookup[m.CurrentModData.ModID].AvailableVersions.Contains(m.InstalledVersion))
                    {
                        ModLookup[m.CurrentModData.ModID].AvailableVersions.Add(m.InstalledVersion);
                        ModLookup[m.CurrentModData.ModID].AllModData.Add(m.InstalledVersion, m.CurrentModData);
                    }
                }
                else
                {
                    Mod newMod = new Mod(ExtractMetadataFromPath(modPath), modNameOnDisk);
                    if (newMod.Priority < 999)
                    {
                        File.Copy(modPath, Path.Combine(DownloadPath, modNameOnDisk));
                        newMod.Enabled = true;
                        Mods.Add(newMod);
                        ModLookup.Add(newMod.CurrentModData.ModID, newMod);
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
                            Mod testMod = new Mod(null, Path.GetFileName(modPath));
                            if (testMod.CurrentModData.ModID == mod.CurrentModData.ModID && testMod.InstalledVersion == mod.InstalledVersion)
                            {
                                copyingPath = modPath;
                                break;
                            }
                        }

                        if (!string.IsNullOrEmpty(copyingPath))
                        {
                            File.Copy(copyingPath, Path.Combine(InstallPath, destinedName));
                            mod.NameOnDisk = destinedName;
                        }
                    }
                    mod.Dirty = false;
                }
            }
        }

        public void SyncConfigFromDisk()
        {
            ModConfig diskConfig;
            try
            {
                diskConfig = JsonConvert.DeserializeObject<ModConfig>(File.ReadAllText(Path.Combine(DownloadPath, "modconfig.json")));
            }
            catch
            {
                return;
            }
            if (diskConfig == null) return;
            ApplyProfile(diskConfig.ModsOnDisk);
            ProfileList = diskConfig.Profiles;
            if (ProfileList == null) ProfileList = new Dictionary<string, ModProfile>();

            if (!string.IsNullOrEmpty(diskConfig.AccentColor))
            {
                try
                {
                    AMLPalette.AccentColor = AMLUtils.ColorFromHTML(diskConfig.AccentColor);
                }
                catch { }
            }
            AMLPalette.CurrentTheme = diskConfig.Theme;
            AMLPalette.RefreshTheme(BaseForm);

            if (!string.IsNullOrEmpty(diskConfig.PlayFabCustomID)) PlayFabAPI.CustomID = diskConfig.PlayFabCustomID;
            if (!string.IsNullOrEmpty(diskConfig.PlayFabToken)) PlayFabAPI.Token = diskConfig.PlayFabToken;
            if (!string.IsNullOrEmpty(diskConfig.GamePath)) GamePath = diskConfig.GamePath;
        }

        public void ApplyProfile(ModProfile prof)
        {
            if (prof == null) return;
            foreach (KeyValuePair<string, Mod> entry in prof.ProfileData)
            {
                if (ModLookup.ContainsKey(entry.Key))
                {
                    ModLookup[entry.Key].Enabled = entry.Value.Enabled;
                    if (entry.Value.InstalledVersion != null) ModLookup[entry.Key].InstalledVersion = entry.Value.InstalledVersion;
                    ModLookup[entry.Key].Priority = entry.Value.Priority;
                }
            }
        }

        public ModProfile GenerateProfile()
        {
            var res = new ModProfile(new Dictionary<string, Mod>());
            foreach (Mod mod in Mods)
            {
                var modClone = new Mod(null, mod.NameOnDisk);
                modClone.AvailableVersions = mod.AvailableVersions;
                modClone.InstalledVersion = mod.InstalledVersion;
                modClone.Enabled = mod.Enabled;
                modClone.Priority = mod.Priority;
                res.ProfileData.Add(mod.CurrentModData.ModID, modClone);
            }
            return res;
        }

        public void SyncConfigToDisk()
        {
            var newConfig = new ModConfig();
            newConfig.GamePath = GamePath;
            newConfig.Theme = AMLPalette.CurrentTheme;
            newConfig.AccentColor = AMLUtils.ColorToHTML(AMLPalette.AccentColor);
            newConfig.PlayFabCustomID = PlayFabAPI.CustomID;
            newConfig.PlayFabToken = PlayFabAPI.Token;
            newConfig.Profiles = ProfileList;
            newConfig.ModsOnDisk = GenerateProfile();

            File.WriteAllBytes(Path.Combine(DownloadPath, "modconfig.json"), Encoding.UTF8.GetBytes(AMLUtils.SerializeObject(newConfig)));
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
                SyncConfigToDisk();
                SyncModsToDisk();
                IntegrateMods();
            }
            catch (IOException)
            {
                IsReadOnly = true;
            }
        }
    }
}
