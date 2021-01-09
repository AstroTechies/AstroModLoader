using AstroModIntegrator;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AstroModLoader
{
    public class Mod : ICloneable
    {
        private bool _enabled;
        [JsonIgnore]
        public bool Dirty;
        [JsonProperty("enabled")]
        [DefaultValue(true)]
        [DisplayName(" ")]
        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                _enabled = value;
                Dirty = true;
            }
        }

        [JsonProperty("force_latest")]
        [DefaultValue(false)]
        public bool ForceLatest;

        public bool ShouldSerializeForceLatest()
        {
            return ForceLatest;
        }

        [JsonConverter(typeof(VersionConverter))]
        [DisplayName("Version")]
        [JsonProperty("version")]
        public Version InstalledVersion { get; set; }

        [JsonIgnore]
        public List<Version> AvailableVersions { get; set; }

        [JsonProperty("priority")]
        public int Priority = 0;

        [JsonIgnore]
        public Dictionary<Version, Metadata> AllModData;

        [JsonIgnore]
        public Metadata CurrentModData {
            get
            {
                return AllModData[InstalledVersion];
            }
        }

        [JsonIgnore]
        public string NameOnDisk;

        [JsonIgnore]
        internal bool CannotCurrentlyUpdate = false;

        public Mod(Metadata modData, string nameOnDisk)
        {
            if (modData == null && nameOnDisk == null) return;
            if (AllModData == null) AllModData = new Dictionary<Version, Metadata>();
            NameOnDisk = nameOnDisk;

            PerformNameAnalysis();

            Priority = newPriority;
            InstalledVersion = newModVersion;

            if (modData != null)
            {
                if (modData.ModVersion != null) InstalledVersion = modData.ModVersion;
            }

            AllModData[InstalledVersion] = modData;
            if (modData == null)
            {
                AllModData[InstalledVersion] = JsonConvert.DeserializeObject<Metadata>("{}");
                AllModData[InstalledVersion].Sync = SyncMode.None;
            }

            if (!string.IsNullOrEmpty(newModID) && string.IsNullOrEmpty(CurrentModData.ModID))
            {
                CurrentModData.ModID = newModID;
            }
            if (string.IsNullOrEmpty(CurrentModData.Name)) CurrentModData.Name = CurrentModData.ModID;

            NameOnDisk = nameOnDisk;
            AvailableVersions = new List<Version>();
            if (InstalledVersion != null && !AvailableVersions.Contains(InstalledVersion)) AvailableVersions.Add(InstalledVersion);

            if (AllModData[InstalledVersion].Name.Length > 32) AllModData[InstalledVersion].Name = AllModData[InstalledVersion].Name.Substring(0, 32);
        }

        public static Regex ModIDFilterRegex = new Regex(@"[^A-Za-z0-9]", RegexOptions.Compiled);
        public string ConstructName()
        {
            return AMLUtils.GeneratePriorityFromPositionInList(Priority) + "-" + ModIDFilterRegex.Replace(CurrentModData.ModID, "") + "-" + InstalledVersion + "_P.pak";
        }

        private int newPriority;
        private string newModID;
        private Version newModVersion;
        private void PerformNameAnalysis()
        {
            if (NameOnDisk == null) NameOnDisk = "";
            List<string> nameData = NameOnDisk.Split('_')[0].Split('-').ToList();
            int origCount = nameData.Count;

            if (origCount >= 1)
            {
                try
                {
                    newPriority = int.Parse(nameData[0]);
                }
                catch (FormatException)
                {
                    newPriority = 1;
                }
                nameData.RemoveAt(0);
            }
            else
            {
                newPriority = 1;
            }

            if (origCount >= 2)
            {
                if (!string.IsNullOrEmpty(nameData[0])) newModID = nameData[0];
                nameData.RemoveAt(0);
            }
            else
            {
                newModID = NameOnDisk.Replace(".pak", "");
            }

            newModVersion = new Version(0, 1, 0);
            if (origCount >= 3)
            {
                if (!string.IsNullOrEmpty(nameData[0])) newModVersion = new Version(nameData[0]);
                nameData.RemoveAt(0);
            }
        }

        public IndexFile GetIndexFile(List<string> duplicateURLs)
        {
            DownloadInfo di = CurrentModData.Download;
            if (di == null) return null;

            try
            {
                if (di.Type == DownloadMode.IndexFile && !string.IsNullOrEmpty(di.URL))
                {
                    if (duplicateURLs != null && duplicateURLs.Contains(di.URL)) return null;
                    string rawIndexFileData = "";
                    using (var wb = new WebClient())
                    {
                        wb.Headers[HttpRequestHeader.UserAgent] = AMLUtils.UserAgent;
                        rawIndexFileData = wb.DownloadString(di.URL);
                    }
                    if (string.IsNullOrEmpty(rawIndexFileData)) return null;

                    IndexFile indexFile = JsonConvert.DeserializeObject<IndexFile>(rawIndexFileData);
                    indexFile.OriginalURL = di.URL;
                    return indexFile;
                }
            }
            catch (Exception ex)
            {
                if (ex is WebException || ex is JsonException) return null;
                throw;
            }

            return null;
        }

        public override bool Equals(object obj)
        {
            string comparer;
            if (obj is Mod mobj)
            {
                comparer = mobj.NameOnDisk;
            }
            else if (obj is string sobj)
            {
                comparer = sobj;
            }
            else
            {
                return false;
            }
            return NameOnDisk.Equals(comparer);
        }

        public override int GetHashCode()
        {
            return NameOnDisk.GetHashCode();
        }

        public override string ToString()
        {
            return Enabled.ToString();
        }

        public object Clone()
        {
            var modClone = new Mod(null, this.NameOnDisk);
            modClone.AvailableVersions = this.AvailableVersions.ToList();
            modClone.AvailableVersions.ForEach(x => x.Clone());
            modClone.AllModData = this.AllModData.ToDictionary(entry => (Version)entry.Key.Clone(), entry => (Metadata)entry.Value.Clone());
            modClone.InstalledVersion = (Version)this.InstalledVersion.Clone();
            modClone.ForceLatest = this.ForceLatest;
            modClone.Enabled = this.Enabled;
            modClone.Priority = this.Priority;
            return modClone;
        }
    }
}
