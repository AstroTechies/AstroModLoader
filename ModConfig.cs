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

        [JsonProperty("launch_command")]
        public string LaunchCommand;

        public bool ShouldSerializeLaunchCommand()
        {
            return !string.IsNullOrEmpty(LaunchCommand);
        }

        [JsonProperty("current")]
        public ModProfile ModsOnDisk;

        [JsonProperty("profiles")]
        public Dictionary<string, ModProfile> Profiles;
    }
}
