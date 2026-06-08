using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using CabinetSheet = Lumina.Excel.Sheets.Cabinet;

namespace DutyLootPreview.Extensions;

public static class ItemExtensions {
    private static readonly Lazy<FrozenDictionary<uint, uint>> CabinetLookup = new(()
        => Env.DataManager.Excel.GetSheet<CabinetSheet>()
            .Where(row => row.RowId >= 1048 && row.Item.RowId != 0)
            .ToFrozenDictionary(row => row.Item.RowId, row => row.RowId));

    extension(Item item) {
        public bool IsUnlockable => Env.UnlockState.IsItemUnlockable(item);
        public bool IsUnlocked => item.IsUnlockable && Env.UnlockState.IsItemUnlocked(item);
        public bool IsStorableInCabinet => CabinetLookup.Value.ContainsKey(item.RowId);
        public bool IsStoredInCabinet => item.IsInCabinet();

        private unsafe bool IsInCabinet() {
            if (!CabinetLookup.Value.TryGetValue(item.RowId, out var cabinetRowId))
                return false;

            // use live data if available
            if (UIState.Instance()->Cabinet.IsCabinetLoaded())
                return UIState.Instance()->Cabinet.IsItemInCabinet(cabinetRowId);

            // use cached data
            var itemFinderModule = ItemFinderModule.Instance();
            (var byteIndex, var bitOffset) = Math.DivRem(cabinetRowId - 1048, 32);
            if (itemFinderModule->CabinetItemUnlockBits.Length >= byteIndex)
                return (itemFinderModule->CabinetItemUnlockBits[(int)byteIndex] & (1 << (int)bitOffset)) != 0;

            return false;
        }

        // See: https://github.com/Haselnussbomber/HaselCommon/blob/30c023516c0f9771183bbb5c01eb8122765e8bd0/HaselCommon/Services/ItemService.cs#L298-L327
        public bool CanTryOn {
            get {
                // not equippable, Waist or SoulCrystal => false
                if (item.EquipSlotCategory.RowId is 0 or 6 or 17)
                    return false;

                // any OffHand that's not a Shield => false
                if (item.EquipSlotCategory.RowId is 2 && item.FilterGroup != 3) // 3 = Shield
                    return false;

                var race = (int)Env.PlayerState.Race.RowId;
                if (race is 0) return false;

                return true;
            }
        }

        public bool IsEquipment
            => item.FilterGroup is 1 or 2 or 3 or 4 or 45;
    }
}
