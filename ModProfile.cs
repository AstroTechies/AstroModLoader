using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstroModLoader
{
    public class ModProfile
    {
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
