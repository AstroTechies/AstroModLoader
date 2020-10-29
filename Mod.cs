using AstroModIntegrator;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        [JsonIgnore]
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
            Debug.WriteLine(NameOnDisk);
            string[] nameData = NameOnDisk.Split('_')[0].Split('-');
            if (nameData.Length >= 1)
            {
                Priority = int.Parse(nameData[0]);
            }
            else
            {
                Priority = 0;
            }

            if (string.IsNullOrEmpty(ModData.ModID))
            {
                if (nameData.Length >= 2 && !string.IsNullOrEmpty(nameData[1]))
                {
                    ModData.ModID = nameData[1];
                }
                else
                {
                    ModData.ModID = "UnknownMod" + new Random().Next(10000);
                }
                ModData.Name = ModData.ModID;
            }

            if (ModData.ModVersion == null)
            {
                if (nameData.Length >= 3 && !string.IsNullOrEmpty(nameData[2]))
                {
                    ModData.ModVersion = new Version(nameData[2]);
                }
                else
                {
                    ModData.ModVersion = new Version(0, 1, 0);
                }
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
