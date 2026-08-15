using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace DutyLootPreview.Features.VanillaPlusImport;

public static class VanillaPlusImporter {
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private static string SourcePath => Path.Combine(
        Env.PluginInterface.ConfigFile.DirectoryName ?? string.Empty,
        "VanillaPlus", "Configs", "DutyLootPreview.config.json");

    public static void PromptIfAvailable() {
        if (Env.Config.ImportedFromVanillaPlus || Env.Config.VanillaPlusImportPrompted) return;

        var favorites = TryReadFavorites();
        if (favorites is not { Count: > 0 }) return;

        Env.ChatGui.Print($"[Duty Loot Preview] Found {favorites.Count} favorites saved in VanillaPlus. Type \"/dlp import\" to bring them over.");

        Env.Config.VanillaPlusImportPrompted = true;
        Env.Config.Save();
    }

    public static void Import() {
        var favorites = TryReadFavorites();
        if (favorites is null) {
            Env.ChatGui.Print("[Duty Loot Preview] No VanillaPlus favorites were found to import.");
            return;
        }

        var before = Env.Config.FavoriteItems.Count;
        Env.Config.FavoriteItems.UnionWith(favorites);
        var added = Env.Config.FavoriteItems.Count - before;

        Env.Config.ImportedFromVanillaPlus = true;
        Env.Config.Save();

        Env.ChatGui.Print($"[Duty Loot Preview] Imported {added} favorite{(added == 1 ? "" : "s")} from VanillaPlus.");
    }

    private static HashSet<uint>? TryReadFavorites() {
        try {
            var path = SourcePath;
            if (!File.Exists(path)) return null;

            var config = JsonSerializer.Deserialize<VanillaPlusDutyLootConfig>(File.ReadAllText(path), JsonOptions);
            return config?.FavoriteItems;
        }
        catch (Exception ex) {
            Env.PluginLog.Warning(ex, "Failed to read VanillaPlus favorites for import.");
            return null;
        }
    }

    private sealed class VanillaPlusDutyLootConfig {
        public HashSet<uint>? FavoriteItems { get; set; }
    }
}
