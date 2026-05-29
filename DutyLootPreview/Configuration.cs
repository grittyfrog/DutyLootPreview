using System;
using Dalamud.Configuration;

namespace DutyLootPreview;

[Serializable]
public class Configuration : IPluginConfiguration {
    public int Version { get; set; } = 0;

    public void Save() {
        Env.PluginInterface.SavePluginConfig(this);
    }
}
