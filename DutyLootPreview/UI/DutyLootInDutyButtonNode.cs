using System.Numerics;
using Dalamud.Plugin.Services;
using DutyLootPreview.Extensions;
using DutyLootPreview.Resources;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Enums;
using KamiToolKit.Extensions;
using KamiToolKit.UiOverlay;
using Action = System.Action;

namespace DutyLootPreview.UI;

public unsafe class DutyLootInDutyButtonNode : OverlayNode {
    public override OverlayLayer OverlayLayer => OverlayLayer.BehindUserInterface;

    private readonly DutyLootOpenWindowButtonNode buttonNode;

    public Action? OnClick {
        get => buttonNode.OnClick;
        set => buttonNode.OnClick = value;
    }

    public DutyLootInDutyButtonNode() {
        buttonNode = new DutyLootOpenWindowButtonNode() {
            Size = new Vector2(32.0f, 32.0f),
            Scale = new Vector2(20.0f / 32.0f),
            TextTooltip = Strings.DutyLoot_Tooltip_InDutyButton,
            IsVisible = true,
        };
        buttonNode.AttachNode(this);
    }

    protected override void OnUpdate() {
        var dutyInfoAddon = Env.GameGui.GetAddonByName<AddonToDoList>("_ToDoList");
        var dutyInfoPos = dutyInfoAddon->AtkUnitBase.Position;
        var dutyInfoScale = dutyInfoAddon->AtkUnitBase.Scale;

        var dutyNameContainer = dutyInfoAddon->AtkUnitBase.GetNodeById<AtkComponentNode>(4);
        if (dutyNameContainer is null) return;
        var dutyNameContainerPos = new Vector2(dutyNameContainer->X, dutyNameContainer->Y) * dutyInfoScale;

        var dutyLootButtonPos = new Vector2(236.0f, 29.0f) * dutyInfoScale;

        Position = dutyInfoPos + dutyNameContainerPos + dutyLootButtonPos;
        Scale = new Vector2(dutyInfoScale, dutyInfoScale);

        UpdateVisibility();
    }

    private void UpdateVisibility() {
        var dutyInfoAddon = Env.GameGui.GetAddonByName<AddonToDoList>("_ToDoList");
        if (!dutyInfoAddon->AtkUnitBase.IsActuallyVisible) {
            IsVisible = false;
            return;
        }

        var dutyNameContainer = dutyInfoAddon->AtkUnitBase.GetNodeById<AtkComponentNode>(4);
        if (dutyNameContainer is null || !dutyNameContainer->AtkResNode.IsActuallyVisible) {
            IsVisible = false;
            return;
        }

        IsVisible = true;
    }
}
