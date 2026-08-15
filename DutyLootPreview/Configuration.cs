using System;
using System.Collections.Generic;
using Dalamud.Configuration;

namespace DutyLootPreview;

[Serializable]
public class DutyLootPreviewConfiguration : IPluginConfiguration {
    public int Version { get; set; } = 0;

    public HashSet<uint> FavoriteItems = [];

    public bool ImportedFromVanillaPlus;

    public bool VanillaPlusImportPrompted;

    public void Save() {
        Env.PluginInterface.SavePluginConfig(this);
    }
}
