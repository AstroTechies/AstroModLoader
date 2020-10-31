using AstroModIntegrator;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AstroModLoader
{
    public class Mod
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

        public Mod(Metadata modData, string nameOnDisk)
        {
            if (modData == null && nameOnDisk == null) return;
            if (AllModData == null) AllModData = new Dictionary<Version, Metadata>();
            NameOnDisk = nameOnDisk;
            PerformNameAnalysis();

            Priority = newPriority;
            InstalledVersion = newModVersion;

            AllModData[InstalledVersion] = modData;
            if (modData == null)
            {
                AllModData[InstalledVersion] = JsonConvert.DeserializeObject<Metadata>("{}");
                AllModData[InstalledVersion].Sync = SyncMode.ClientOnly;
            }

            if (!string.IsNullOrEmpty(newModID) && string.IsNullOrEmpty(CurrentModData.ModID))
            {
                CurrentModData.ModID = newModID;
            }
            if (string.IsNullOrEmpty(CurrentModData.Name)) CurrentModData.Name = CurrentModData.ModID;

            NameOnDisk = nameOnDisk;
            AvailableVersions = new List<Version>();
            if (InstalledVersion != null && !AvailableVersions.Contains(InstalledVersion)) AvailableVersions.Add(InstalledVersion);
        }

        public string ConstructName()
        {
            return AMLUtils.GeneratePriorityFromPositionInList(Priority) + "-" + CurrentModData.ModID + "-" + InstalledVersion + "_P.pak";
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
                newPriority = int.Parse(nameData[0]);
                nameData.RemoveAt(0);
            }
            else
            {
                newPriority = 1;
            }

            newModID = "UnknownMod" + new Random().Next(10000);
            if (origCount >= 2)
            {
                if (!string.IsNullOrEmpty(nameData[0])) newModID = nameData[0];
                nameData.RemoveAt(0);
            }

            newModVersion = new Version(0, 1, 0);
            if (origCount >= 3)
            {
                if (!string.IsNullOrEmpty(nameData[0])) newModVersion = new Version(nameData[0]);
                nameData.RemoveAt(0);
            }
        }

        // TODO: actually call this method somewhere
        public void ScanForAutoUpdate()
        {
            DownloadInfo di = CurrentModData.Download;
            if (di == null) return;

            switch (di.Type)
            {
                case DownloadMode.IndexFile:
                    string rawIndexFileData = "";
                    using (var wb = new WebClient())
                    {
                        wb.Headers[HttpRequestHeader.UserAgent] = "AstroModLoader " + Application.ProductVersion;
                        rawIndexFileData = wb.DownloadString(di.URL);
                    }
                    if (string.IsNullOrEmpty(rawIndexFileData)) break;

                    IndexFile indexFile = JsonConvert.DeserializeObject<IndexFile>(rawIndexFileData);
                    if (indexFile == null) break;

                    // TODO: actually parse index file data

                    break;
            }

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
    }
}
