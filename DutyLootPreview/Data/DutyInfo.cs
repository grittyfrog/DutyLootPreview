using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Enums;
using DutyLootPreview.Extensions;
using DutyLootPreview.Extensions.LuminaSupplemental;
using DutyLootPreview.Resources;
using Lumina.Excel.Sheets;
using LuminaSupplemental.Excel.Model;

namespace DutyLootPreview.Data;

public class DutyInfo {
    public uint? ContentId { get; init; }

    public List<DutyItem> DutyItems { get; init; } = new();
}

public record class DutyItem(
    uint ContentFinderConditionId,
    Item Item,
    List<DutyItemSource> Sources 
) : IComparable<DutyItem> {
    public int CompareTo(DutyItem? other) {
        if (other is null) return 1;

        // Misc+Unlockable (0) > Misc (1) > Equipment (2)
        var category = CategoryPriority.CompareTo(other.CategoryPriority);
        if (category != 0) return category;

        var a = Item.ItemUICategory.ValueNullable;
        var b = other.Item.ItemUICategory.ValueNullable;

        var major = -(a?.OrderMajor ?? 0).CompareTo(b?.OrderMajor ?? 0);
        if (major != 0) return major;

        var minor = -(a?.OrderMinor ?? 0).CompareTo(b?.OrderMinor ?? 0);
        if (minor != 0) return minor;

        return string.Compare(Item.Name.ToString(), other.Item.Name.ToString(), StringComparison.Ordinal);
    }

    private int CategoryPriority =>
        !Item.IsEquipment && Item.IsUnlockable ? 0
        : !Item.IsEquipment ? 1
        : 2;

    public static List<DutyItem> MergeDuplicates(IEnumerable<DutyItem> items) {
        var merged = new Dictionary<(uint, uint), DutyItem>();
        foreach (var item in items) {
            var key = (item.ContentFinderConditionId, item.Item.RowId);
            if (merged.TryGetValue(key, out var existing))
                existing.Sources.AddRange(item.Sources);
            else
                merged[key] = item;
        }
        return merged.Values.ToList();
    }
}

public abstract record DutyItemSource {
    // A specific fight: the enemies drop it directly, or spawn a coffer that does.
    public sealed record Fight(ImmutableArray<string> Enemies, bool FromChest) : DutyItemSource;

    // A coffer in the dungeon not tied to a named enemy.
    public sealed record DungeonChest : DutyItemSource;

    public string Name => this switch {
        Fight(var enemies, var fromChest) => $"{string.Join(" + ", enemies)} {(fromChest ? "(Chest)" : "(Drop)")}",
        DungeonChest => Strings.DutyLoot_DutyItemSource_DungeonChest,
        _ => string.Empty
    };

    public static DutyItemSource? FromFight(DungeonBossDrop drop) {
        if (drop.ItemId == 0) return null;
        return FromFight(drop.Bosses, fromChest: false);
    }

    public static DutyItemSource? FromFight(DungeonBossChest chest) {
        if (chest.ItemId == 0) return null;
        return FromFight(chest.Bosses, fromChest: true);
    }

    private static DutyItemSource? FromFight(ImmutableArray<DungeonBoss> bosses, bool fromChest) {
        var enemies = bosses
            .Select(boss => Env.SeStringEvaluator.EvaluateObjStr(ObjectKind.BattleNpc, boss.BNpcNameId))
            .Where(name => !string.IsNullOrEmpty(name))
            .ToImmutableArray();
        if (enemies.Length == 0) { return null; }

        return new DutyItemSource.Fight(Enemies: enemies, FromChest: fromChest);
    }

    public static DutyItemSource FromDungeon(DungeonChestItem item) {
        return new DutyItemSource.DungeonChest();  
    }
}
