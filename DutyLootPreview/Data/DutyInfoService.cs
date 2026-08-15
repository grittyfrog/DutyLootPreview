using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using DutyLootPreview.Extensions;
using DutyLootPreview.Extensions.LuminaSupplemental;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.Sheets;

namespace DutyLootPreview.Data;

/// <summary>
/// Retrieve duty loot info about a duty.
/// </summary>
public class DutyInfoService {
    private FrozenDictionary<uint, DutyInfo>? cache;

    public DutyInfo? GetDutyInfo(uint contentId) {
        cache ??= LoadDutyInfo();
        if (!IsSupportedContent(contentId)) {
            return null;
        }
        return cache.GetValueOrDefault(contentId);
    }

    public static bool IsSupportedContent(uint contentId) {
        return IsSupportedContent(new ContentsId { ContentType = ContentsType.Regular, Id = contentId });
    }

    public static bool IsSupportedContent(ContentsId content) {
        if (content.Id == 0)
            return false;

        // Not for Content Roulette
        if (content.ContentType != ContentsType.Regular)
            return false;

        if (!Env.DataManager.GetExcelSheet<ContentFinderCondition>().TryGetRow(content.Id, out var cfc))
            return false;

        // Not for Guildhests (3), PvP (6), Gold Saucer (19)
        return cfc.ContentType.RowId is not (3 or 6 or 19);
    }

    private FrozenDictionary<uint, DutyInfo> LoadDutyInfo() {
        var items = new List<DutyItem>();

        foreach (var fight in Env.LumSup.DungeonBossDrop.Rows) {
            if (fight.Item.ValueNullable is { } item) {
                var source = DutyItemSource.FromFight(fight);
                items.Add(new DutyItem(fight.ContentFinderConditionId, item, source is null ? [] : [source]));
            }
        }

        foreach (var fight in Env.LumSup.DungeonBossChest.Rows) {
            if (fight.Item.ValueNullable is { } item) {
                var source = DutyItemSource.FromFight(fight);
                items.Add(new DutyItem(fight.ContentFinderConditionId, item, source is null ? [] : [source]));
            }
        }

        foreach (var drop in Env.LumSup.DungeonChestItem.Rows) {
            if (drop.Item.ValueNullable is { } item && drop.Chest is { } chest && DutyItemSource.FromDungeon(drop) is { } source) {
                items.Add(new DutyItem(chest.ContentFinderConditionId, item, [source]));
            }
        }

        var merged = DutyItem.MergeDuplicates(items);
        return merged
            .GroupBy(item => item.ContentFinderConditionId)
            .ToFrozenDictionary(
                g => g.Key,
                g => new DutyInfo { ContentId = g.Key, DutyItems = g.ToList() }
            );
    }
}
