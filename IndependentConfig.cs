using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace AstroModLoader
{
    public enum PlatformType
    {
        [EnumMember(Value = "unknown")]
        Unknown,
        [EnumMember(Value = "custom")]
        Custom,
        [EnumMember(Value = "steam")]
        Steam,
        [EnumMember(Value = "win10")]
        Win10
    }

    public class IndependentConfig
    {
        [JsonProperty("platform")]
        [JsonConverter(typeof(StringEnumConverter))]
        public PlatformType Platform;

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
