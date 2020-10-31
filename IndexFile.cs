using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstroModLoader
{
    public class IndexVersionData
    {
        [JsonProperty("download_url")]
        public string URL;

        [JsonProperty("filename")]
        public string Filename;
    }

    public class IndexMod
    {
        [JsonProperty("latest_version")]
        public string LatestVersion;

        [JsonProperty("versions")]
        public Dictionary<Version, IndexVersionData> AllVersions;
    }

    public class IndexFile
    {
        [JsonProperty("mods")]
        public Dictionary<string, IndexMod> Mods;
    }
}
