using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Extensions;

namespace DutyLootPreview.Extensions;

// ref: https://github.com/MidoriKami/VanillaPlus/blob/ca83f78fa9c89f5231a1053ff0ce7f74f34862ff/VanillaPlus/Extensions/AtkUnitBaseExtensions.cs#L14
public static unsafe class AtkUnitBaseExtensions {
    extension(ref AtkUnitBase addon) {
        public T* GetNodeById<T>(uint nodeId) where T : unmanaged => addon.UldManager.SearchNodeById<T>(nodeId);
        public T* GetComponentById<T>(uint nodeId) where T : unmanaged => (T*)addon.GetComponentByNodeId(nodeId);
    }
}
