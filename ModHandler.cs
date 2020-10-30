using AstroModIntegrator;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
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
        public string BinaryFilePath;
        internal Dictionary<string, Mod> ModLookup;
        public List<Mod> Mods;
        public Dictionary<string, ModProfile> ProfileList;
        public Form1 BaseForm;
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

            DetermineBinaryFilePath();

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

        public void DetermineBinaryFilePath()
        {
            if (GamePath != null)
            {
                BinaryFilePath = null;
                string[] allExes = Directory.GetFiles(Path.Combine(GamePath, "Astro", "Binaries"), "*.exe", SearchOption.AllDirectories);
                if (allExes.Length > 0) BinaryFilePath = allExes[0];
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
                if (ModLookup.ContainsKey(newMod.ModData.ModID))
                {
                    ModLookup[newMod.ModData.ModID].AvailableVersions.Add(newMod.InstalledVersion);
                }
                else
                {
                    Mods.Add(newMod);
                    ModLookup.Add(newMod.ModData.ModID, newMod);
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

                if (ModLookup.ContainsKey(m.ModData.ModID))
                {
                    ModLookup[m.ModData.ModID].Enabled = true;
                    ModLookup[m.ModData.ModID].NameOnDisk = modNameOnDisk;
                    ModLookup[m.ModData.ModID].InstalledVersion = m.InstalledVersion;
                    ModLookup[m.ModData.ModID].Priority = m.Priority;
                }
                else
                {
                    Mod newMod = new Mod(ExtractMetadataFromPath(modPath), modNameOnDisk);
                    if (newMod.Priority < 999)
                    {
                        File.Copy(modPath, Path.Combine(DownloadPath, modNameOnDisk));
                        newMod.Enabled = true;
                        Mods.Add(newMod);
                        ModLookup.Add(newMod.ModData.ModID, newMod);
                    }
                }
            }

            SortMods();
        }

        public void SortMods()
        {
            Mods = new List<Mod>(Mods.OrderBy(o => o.NameOnDisk).ToList());
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
                            if (testMod.ModData.ModID == mod.ModData.ModID && testMod.InstalledVersion == mod.InstalledVersion)
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
                res.ProfileData.Add(mod.ModData.ModID, modClone);
            }
            return res;
        }

        public void SyncConfigToDisk()
        {
            var newConfig = new ModConfig();
            newConfig.GamePath = GamePath;
            newConfig.Theme = AMLPalette.CurrentTheme;
            newConfig.AccentColor = AMLUtils.ColorToHTML(AMLPalette.AccentColor);
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
            for (int i = 0; i < Mods.Count; i++) Mods[i].Priority = i;
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
