using Dalamud.Configuration;
using Dalamud.Plugin;
using Newtonsoft.Json;

namespace FPSLimiter
{
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; }
        public int CurrentFpsCap = 60;
        public int PreviousFpsCap = 15;

        [JsonIgnore] private DalamudPluginInterface pluginInterface;
        

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.pluginInterface.SavePluginConfig(this);
        }
    }
}
