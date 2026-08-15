using System.Numerics;
using Dalamud.Game.Gui;
using Dalamud.Plugin.Services;
using DutyLootPreview.Data;
using DutyLootPreview.Extensions;
using DutyLootPreview.Resources;
using DutyLootPreview.UI;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Enums;
using KamiToolKit.Extensions;
using KamiToolKit.UiOverlay;
using static DutyLootPreview.Data.EventSignal;
using Action = System.Action;

namespace DutyLootPreview.Features.InDutyIntegration;

public unsafe class InDutyOverlayNode : OverlayNode {
    public override OverlayLayer OverlayLayer => OverlayLayer.BehindUserInterface;

    private readonly DutyLootOpenWindowButtonNode buttonNode;

    private readonly Polled<ushort> activeDutyIdPoll = Polled.Of(() => GameMain.Instance()->CurrentContentFinderConditionId);
    private readonly Watcher unlocksChangedWatch = Env.EventTrackers.UnlocksChanged.Watch();

    public Action? OnClick {
        get => buttonNode.OnClick;
        set => buttonNode.OnClick = value;
    }

    public bool CheckmarkVisible {
        get => buttonNode.CheckmarkVisible;
        set => buttonNode.CheckmarkVisible = value;
    }

    public InDutyOverlayNode() {
        buttonNode = new DutyLootOpenWindowButtonNode() {
            Size = new Vector2(32.0f, 32.0f),
            Scale = new Vector2(20.0f / 32.0f),
            TextTooltip = Strings.DutyLoot_Tooltip_InDutyButton,
            IsVisible = true,
        };
        buttonNode.AttachNode(this);
    }

    protected override void OnUpdate() {
        // === Visibility ===
        // We should be visible if:
        // 1. _ToDoList exists and we can find our "anchor" component for positioning
        // 2. We are in a Duty
        // 3. We have data for that duty.
        var dutyInfoAddon = Env.GameGui.GetAddonByName<AddonToDoList>("_ToDoList");
        if (dutyInfoAddon is null || !dutyInfoAddon->AtkUnitBase.IsActuallyVisible) {
            IsVisible = false;
            return;
        }

        var dutyNameContainer = dutyInfoAddon->AtkUnitBase.GetNodeById<AtkComponentNode>(4);
        if (dutyNameContainer is null || !dutyNameContainer->AtkResNode.IsActuallyVisible) {
            IsVisible = false;
            return;
        }

        var (activeDutyId, activeDutyIdChanged) = this.activeDutyIdPoll.Poll();
        var activeDuty = Env.DutyInfoService.GetDutyInfo(activeDutyId);
        var unlocksChanged = unlocksChangedWatch.Fired();

        IsVisible = activeDuty != null;
        if (activeDutyIdChanged || unlocksChanged) {
            CheckmarkVisible = activeDuty?.AllUnlocksUnlocked() ?? false;
        }

        // === Positioning ===
        // If we aren't visible, we want to move our component to align with `_ToDoList`
        if (!IsVisible) {
            return;
        }

        var dutyInfoPos = dutyInfoAddon->AtkUnitBase.Position;
        var dutyInfoScale = dutyInfoAddon->AtkUnitBase.Scale;

        if (dutyNameContainer is null) return;
        var dutyNameContainerPos = new Vector2(dutyNameContainer->X, dutyNameContainer->Y) * dutyInfoScale;

        var dutyLootButtonPos = new Vector2(236.0f, 29.0f) * dutyInfoScale;

        Position = dutyInfoPos + dutyNameContainerPos + dutyLootButtonPos;
        Scale = new Vector2(dutyInfoScale, dutyInfoScale);
    }
}
