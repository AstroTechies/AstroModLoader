using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace AstroModLoader
{
    public enum PlatformType
    {
        [EnumMember(Value = "unknown")]
        Unknown, // We have no idea about anything
        [EnumMember(Value = "custom")]
        Custom, // The game is installed somewhere, but it isn't auto detected as Steam or Win10
        [EnumMember(Value = "steam")]
        Steam,
        [EnumMember(Value = "win10")]
        Win10,
        [EnumMember(Value = "server")]
        Server
    }

    public class IndependentConfig
    {
        [JsonProperty("platform")]
        [JsonConverter(typeof(StringEnumConverter))]
        public PlatformType Platform;

        [JsonProperty("custom_base_path")]
        public string CustomBasePath;

        [JsonProperty("theme")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ModLoaderTheme Theme;

        [JsonProperty("accent")]
        public string AccentColor;

        [JsonProperty("playfab_id")]
        public string PlayFabCustomID;

        [JsonProperty("playfab_token")]
        public string PlayFabToken;
    }
}
