using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using LuminaSupplemental.Excel.Model;
using LuminaSupplemental.Excel.Services;

namespace DutyLootPreview.Extensions.LuminaSupplemental;

public class DungeonChestItemSheet : LumSupSheet<DungeonChestItem> {
    private readonly Lazy<FrozenDictionary<uint, DungeonChestItem>> byRowId;

    public DungeonChestItemSheet() : base(CsvLoader.DungeonChestItemResourceName) {
        byRowId = MakeIndex(row => row.RowId);
    }

    public FrozenDictionary<uint, DungeonChestItem> ByRowId => byRowId.Value;
}

public static class DungeonChestItemExtensions {
    extension(DungeonChestItem self) {
        public DungeonChest? Chest =>
            Env.LumSup.DungeonChest.ByRowId.GetValueOrDefault(self.ChestId);
    }
}
