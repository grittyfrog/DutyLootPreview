using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using LuminaSupplemental.Excel.Model;
using LuminaSupplemental.Excel.Services;

namespace DutyLootPreview.Extensions.LuminaSupplemental;

public class DungeonBossDropSheet : LumSupSheet<DungeonBossDrop> {
    private readonly Lazy<FrozenDictionary<uint, DungeonBossDrop>> byRowId;

    public DungeonBossDropSheet() : base(CsvLoader.DungeonBossDropResourceName) {
        byRowId = MakeIndex(row => row.RowId);
    }

    public FrozenDictionary<uint, DungeonBossDrop> ByRowId => byRowId.Value;
}

public static class DungeonBossDropExtensions {
    extension(DungeonBossDrop self) {
        public ImmutableArray<DungeonBoss> Bosses =>
            Env.LumSup.DungeonBoss.ByFightKey.GetValueOrDefault((self.ContentFinderConditionId, self.FightNo), []);
    }
}
