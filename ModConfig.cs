using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.ComponentModel;

namespace AstroModLoader
{
    public class ModConfig
    {
        [JsonProperty("install_path")]
        [DefaultValue("")]
        public string GamePath;

        [JsonProperty("theme")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ModLoaderTheme Theme;

        [JsonProperty("accent")]
        public string AccentColor;

        [JsonProperty("playfab_id")]
        public string PlayFabCustomID;

        [JsonProperty("playfab_token")]
        public string PlayFabToken;

        [JsonProperty("current")]
        public ModProfile ModsOnDisk;

        [JsonProperty("profiles")]
        public Dictionary<string, ModProfile> Profiles;
    }
}
