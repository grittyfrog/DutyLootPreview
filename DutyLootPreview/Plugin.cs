using Dalamud.Plugin;
using DutyLootPreview.Utilities;

namespace DutyLootPreview;

public sealed class Plugin : IDalamudPlugin {
    public string Name => "Duty Loot Preview";

    public Plugin(IDalamudPluginInterface pluginInterface) {
        // Pick locale before any Strings.* lookup happens during Initialize.
        Localization.SetCultureInfo(pluginInterface.UiLanguage);
        pluginInterface.LanguageChanged += Localization.SetCultureInfo;

        Env.Initialize(pluginInterface);

        Env.Framework.Run(() => {
            Env.DutyLootPreviewAgent.Enable();
            Env.DutyLootJournalUiController.Enable();
        });

    }

    public void Dispose() {
        Env.PluginInterface.LanguageChanged -= Localization.SetCultureInfo;
        Env.Dispose();
    }
}
