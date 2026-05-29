using Dalamud.Plugin;

namespace DutyLootPreview;

public sealed class Plugin : IDalamudPlugin {
    public string Name => "Duty Loot Preview";

    public Plugin(IDalamudPluginInterface pluginInterface) {
        Env.Initialize(pluginInterface);
    }

    public void Dispose() {
        Env.Dispose();
    }
}
