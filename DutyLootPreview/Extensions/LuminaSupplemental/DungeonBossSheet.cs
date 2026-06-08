using System;
using System.Collections.Frozen;
using System.Collections.Immutable;
using LuminaSupplemental.Excel.Model;
using LuminaSupplemental.Excel.Services;

namespace DutyLootPreview.Extensions.LuminaSupplemental;

public class DungeonBossSheet : LumSupSheet<DungeonBoss> {
    private readonly Lazy<FrozenDictionary<uint, DungeonBoss>> byRowId;
    private readonly Lazy<FrozenDictionary<(uint cfcId, uint fightNo), ImmutableArray<DungeonBoss>>> byFightKey;

    public DungeonBossSheet() : base(CsvLoader.DungeonBossResourceName) {
        byRowId = MakeIndex(row => row.RowId);
        byFightKey = MakeLookup(row => (row.ContentFinderConditionId, row.FightNo));
    }

    public FrozenDictionary<uint, DungeonBoss> ByRowId => byRowId.Value;
    public FrozenDictionary<(uint cfcId, uint fightNo), ImmutableArray<DungeonBoss>> ByFightKey => byFightKey.Value;
}
