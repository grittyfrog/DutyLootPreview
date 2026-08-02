using System.Globalization;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using DutyLootPreview.Resources;
using KamiToolKit;

namespace DutyLootPreview;

public sealed class DutyLootPreview : IDalamudPlugin {
    public string Name => "Duty Loot Preview";

    public DutyLootPreview(IDalamudPluginInterface pluginInterface) {
        // Pick locale before any Strings.* lookup happens during Initialize.
        SetCultureInfo(pluginInterface.UiLanguage);
        pluginInterface.LanguageChanged += SetCultureInfo;

        Env.Initialize(pluginInterface);

        Env.Framework.Run(() => {
            Env.DutyLootPreviewAgent.Enable();
            Env.DutyLootJournalUiController.Enable();
            Env.DutyLootInDutyUiController.Enable();
        });

        Env.CommandManager.AddHandler("/dlp", new CommandInfo(OnMainCommand) {
            HelpMessage = "Toggle the Duty Loot Preview window."
        });
    }

    public void Dispose() {
        Env.PluginInterface.LanguageChanged -= SetCultureInfo;
        Env.Dispose();

        Env.CommandManager.RemoveHandler("/dlp");
    }

    private void OnMainCommand(string command, string arguments) {
        Env.DutyLootPreviewAddon.Toggle();
    }

    public static void SetCultureInfo(object? language) {
        var languageName = language?.ToString();

        Strings.CultureInfo = languageName switch {
            "ja" => CultureInfo.GetCultureInfo("ja-JP"),
            "zh" => CultureInfo.GetCultureInfo("zh-CN"),
            "de" => CultureInfo.GetCultureInfo("de-DE"),
            "fr" => CultureInfo.GetCultureInfo("fr-FR"),
            _ => CultureInfo.GetCultureInfo("en-US"),
        };

        KamiToolKitLibrary.SetCurrentCulture(Strings.CultureInfo);
    }
}
