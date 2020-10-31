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

        [JsonIgnore]
        public string Name { get { return ModData.Name; } }

        [JsonConverter(typeof(VersionConverter))]
        [DisplayName("Version")]
        [JsonProperty("version")]
        public Version InstalledVersion { get { return ModData.ModVersion; } set { ModData.ModVersion = value; } }

        [JsonIgnore]
        public List<Version> AvailableVersions { get; set; }

        [JsonIgnore]
        public string Author { get { return ModData.Author; } }

        [JsonProperty("priority")]
        public int Priority = 0;

        [JsonIgnore]
        public Metadata ModData;

        [JsonIgnore]
        public string NameOnDisk;

        public Mod(Metadata modData, string nameOnDisk)
        {
            ModData = modData;
            if (ModData == null)
            {
                ModData = JsonConvert.DeserializeObject<Metadata>("{}");
                ModData.Sync = SyncMode.ClientOnly;
            }
            NameOnDisk = nameOnDisk;
            PerformNameAnalysis();
            AvailableVersions = new List<Version>();
            if (InstalledVersion != null && !AvailableVersions.Contains(InstalledVersion)) AvailableVersions.Add(InstalledVersion);
        }

        public string ConstructName()
        {
            return AMLUtils.GeneratePriorityFromPositionInList(Priority) + "-" + ModData.ModID + "-" + ModData.ModVersion + "_P.pak";
        }

        private void PerformNameAnalysis()
        {
            if (string.IsNullOrEmpty(NameOnDisk)) return;
            List<string> nameData = NameOnDisk.Split('_')[0].Split('-').ToList();
            int origCount = nameData.Count;

            if (origCount >= 1)
            {
                Priority = int.Parse(nameData[0]);
                nameData.RemoveAt(0);
            }
            else
            {
                Priority = 0;
            }

            if (string.IsNullOrEmpty(ModData.ModID))
            {
                ModData.ModID = "UnknownMod" + new Random().Next(10000);
                if (origCount >= 2)
                {
                    if (!string.IsNullOrEmpty(nameData[0])) ModData.ModID = nameData[0];
                    nameData.RemoveAt(0);
                }
                if (string.IsNullOrEmpty(ModData.Name)) ModData.Name = ModData.ModID;
            }

            if (ModData.ModVersion == null)
            {
                ModData.ModVersion = new Version(0, 1, 0);
                if (origCount >= 3)
                {
                    if (!string.IsNullOrEmpty(nameData[0])) ModData.ModVersion = new Version(nameData[0]);
                    nameData.RemoveAt(0);
                }
            }
        }

        // TODO: actually call this method somewhere
        public void ScanForAutoUpdate()
        {
            DownloadInfo di = ModData.Download;
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
