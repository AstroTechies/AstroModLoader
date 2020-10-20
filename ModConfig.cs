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

        [JsonProperty("base_path")]
        [DefaultValue("")]
        public string BasePath;

        [JsonProperty("theme")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ModLoaderTheme Theme;

        [JsonProperty("accent")]
        public string AccentColor;

        [JsonProperty("current")]
        public ModProfile ModsOnDisk;

        [JsonProperty("profiles")]
        public Dictionary<string, ModProfile> Profiles;
    }
}
