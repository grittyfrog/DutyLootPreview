using System;
using System.Collections.Generic;
using System.Collections.Frozen;
using System.Collections.Immutable;

using System.Linq;
using LuminaSupplemental.Excel.Model;
using LuminaSupplemental.Excel.Services;

namespace DutyLootPreview.Extensions.LuminaSupplemental;

public abstract class LumSupSheet<TRow> where TRow : ICsv, new() {
    private readonly Lazy<IReadOnlyList<TRow>> rows;

    protected LumSupSheet(string resourceName) {
        rows = new Lazy<IReadOnlyList<TRow>>(() => Load(resourceName));
    }

    public IReadOnlyList<TRow> Rows => rows.Value;

    protected Lazy<FrozenDictionary<TKey, TRow>> MakeIndex<TKey>(Func<TRow, TKey> keyFn) where TKey : notnull
        => new(() => Rows.ToFrozenDictionary(keyFn));

    protected Lazy<FrozenDictionary<TKey, ImmutableArray<TRow>>> MakeLookup<TKey>(Func<TRow, TKey> keyFn)
        where TKey : notnull
        => new(() => Rows
            .GroupBy(keyFn)
            .ToFrozenDictionary(g => g.Key, g => g.ToImmutableArray()));

    private static IReadOnlyList<TRow> Load(string resourceName) {
        var gameData = Env.DataManager.GameData;
        var loaded = CsvLoader.LoadResource<TRow>(
            resourceName,
            includesHeaders: true,
            out var failedLines,
            out var exceptions,
            gameData,
            gameData.Options.DefaultExcelLanguage
        );

        foreach (var line in failedLines)
            Env.PluginLog.Warning($"[LumSup:{resourceName}] failed line: {line}");
        foreach (var ex in exceptions)
            Env.PluginLog.Warning(ex, $"[LumSup:{resourceName}] parse exception");

        return loaded;
    }
}
