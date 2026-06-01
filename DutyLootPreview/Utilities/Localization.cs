using System.Globalization;
using DutyLootPreview.Resources;
using KamiToolKit;

namespace DutyLootPreview.Utilities;

/// <summary>
/// Maps Dalamud's UI language string (e.g. "ja", "fr") to a .NET
/// CultureInfo and routes it to both our own Strings ResourceManager
/// and KamiToolKit's. Wired in Plugin.cs to PluginInterface.UiLanguage
/// and the LanguageChanged event.
/// </summary>
public static class Localization {
    public static void SetCultureInfo(object? language) {
        var languageName = language?.ToString();

        Strings.Culture = languageName switch {
            "ja" => CultureInfo.GetCultureInfo("ja-JP"),
            "zh" => CultureInfo.GetCultureInfo("zh-CN"),
            "de" => CultureInfo.GetCultureInfo("de-DE"),
            "fr" => CultureInfo.GetCultureInfo("fr-FR"),
            _ => CultureInfo.GetCultureInfo("en-US"),
        };

        KamiToolKitLibrary.SetCurrentCulture(Strings.Culture);
    }
}
