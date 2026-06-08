using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using LuminaSupplemental.Excel.Model;
using LuminaSupplemental.Excel.Services;

namespace DutyLootPreview.Extensions.LuminaSupplemental;

public class DungeonChestSheet : LumSupSheet<DungeonChest> {
    private readonly Lazy<FrozenDictionary<uint, DungeonChest>> byRowId;

    public DungeonChestSheet() : base(CsvLoader.DungeonChestResourceName) {
        byRowId = MakeIndex(row => row.RowId);
    }

    public FrozenDictionary<uint, DungeonChest> ByRowId => byRowId.Value;
}
