using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using DutyLootPreview.Extensions;
using DutyLootPreview.Extensions.LuminaSupplemental;

namespace DutyLootPreview.Data;

/// <summary>
/// Retrieve duty loot info about a duty.
/// </summary>
public class DutyInfoService {
    private FrozenDictionary<uint, DutyInfo>? cache;

    public DutyInfo? GetDutyInfo(uint contentId) {
        if (cache == null) {
            cache = LoadDutyInfo();
        }

        return cache.GetValueOrDefault(contentId);
    }

    private FrozenDictionary<uint, DutyInfo> LoadDutyInfo() {
        var items = new List<DutyItem>();  

        foreach (var fight in Env.LumSup.DungeonBossDrop.Rows) {
            if (fight.Item.ValueNullable is {} item) {
                var source = DutyItemSource.FromFight(fight);
                items.Add(new DutyItem(fight.ContentFinderConditionId, item, source is null ? [] : [source]));
            }
        }

        foreach (var fight in Env.LumSup.DungeonBossChest.Rows) {
            if (fight.Item.ValueNullable is {} item) {
                var source = DutyItemSource.FromFight(fight);
                items.Add(new DutyItem(fight.ContentFinderConditionId, item, source is null ? [] : [source]));
            }
        }

        foreach (var drop in Env.LumSup.DungeonChestItem.Rows) {
            if (drop.Item.ValueNullable is {} item && drop.Chest is {} chest && DutyItemSource.FromDungeon(drop) is {} source) {
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
