using AstroModIntegrator;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstroModLoader
{
    public class Server
    {
        [JsonProperty("LobbyID")]
        public ulong LobbyID;

        [JsonProperty("BuildVersion")]
        public int BuildVersion; // unsure what this is

        [JsonProperty("RunTime")]
        public int RunTime;

        [JsonProperty("LastHeartbeat")]
        public string LastHeartbeat;

        [JsonProperty("Tags")]
        public DetailedServerInfo DetailedServerInfo { get; set; }
    }

    public class DetailedServerInfo
    {
        [JsonProperty("maxPlayers")]
        public int MaxPlayers;

        [JsonProperty("numPlayers")]
        public int NumPlayers;

        [JsonProperty("gameId")]
        public string Address;

        [JsonProperty("gameBuild")]
        [JsonConverter(typeof(VersionConverter))]
        public Version GameBuild;

        [JsonProperty("serverName")]
        public string ServerData; // AstroLauncher servers have JSON here, vanilla servers a random hex string

        [JsonProperty("category")]
        public string ServerType;

        [JsonProperty("requiresPassword")]
        public bool RequiresPassword;
    }

    public class AstroLauncherServerInfo
    {
        [JsonProperty("ServerName")]
        public string ServerName;

        [JsonProperty("ServerType")]
        public string ServerType;

        [JsonProperty("ServerPaks")]
        public List<Dictionary<string, string>> ServerPaks; // [{NameOnDisk: MetadataString}, {NameOnDisk: MetadataString}]

        public List<Mod> GetAllMods()
        {
            List<Mod> finalRes = new List<Mod>();
            if (ServerPaks == null) return finalRes;
            foreach (Dictionary<string, string> keyValuePairs in ServerPaks)
            {
                foreach (KeyValuePair<string, string> keyValuePair in keyValuePairs)
                {
                    try
                    {
                        Mod thisMod = new Mod(JsonConvert.DeserializeObject<Metadata>(keyValuePair.Value), keyValuePair.Key);
                        if (thisMod.Priority < 999) finalRes.Add(thisMod);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            return finalRes;
        }
    }

}
