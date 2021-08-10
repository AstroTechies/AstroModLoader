using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;

namespace AstroModLoader
{
    public class ModProfile
    {
        [JsonProperty("info")]
        [DefaultValue("")]
        public string Info = null;

        public bool ShouldSerializeInfo()
        {
            return !string.IsNullOrEmpty(Info);
        }

        [JsonProperty("name")]
        [DefaultValue("")]
        public string Name = null;

        public bool ShouldSerializeName()
        {
            return !string.IsNullOrEmpty(Name);
        }

        [JsonProperty("mods")]
        public Dictionary<string, Mod> ProfileData;

        public ModProfile(Dictionary<string, Mod> profileData)
        {
            ProfileData = profileData;
        }

        public ModProfile()
        {

        }
    }
}
