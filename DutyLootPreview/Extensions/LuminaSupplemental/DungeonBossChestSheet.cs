using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using LuminaSupplemental.Excel.Model;
using LuminaSupplemental.Excel.Services;

namespace DutyLootPreview.Extensions.LuminaSupplemental;

public class DungeonBossChestSheet : LumSupSheet<DungeonBossChest> {
    private readonly Lazy<FrozenDictionary<uint, DungeonBossChest>> byRowId;

    public DungeonBossChestSheet() : base(CsvLoader.DungeonBossChestResourceName) {
        byRowId = MakeIndex(row => row.RowId);
    }

    public FrozenDictionary<uint, DungeonBossChest> ByRowId => byRowId.Value;
}

public static class DungeonBossChestExtensions {
    extension(DungeonBossChest self) {
        public ImmutableArray<DungeonBoss> Bosses =>
            Env.LumSup.DungeonBoss.ByFightKey.GetValueOrDefault((self.ContentFinderConditionId, self.FightNo), []);
    }
}
