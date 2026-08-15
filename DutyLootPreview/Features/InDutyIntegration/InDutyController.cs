using System;
using System.Numerics;
using Dalamud.Game.Gui;
using Dalamud.Plugin.Services;
using DutyLootPreview.Data;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using KamiToolKit.UiOverlay;
using Lumina.Excel.Sheets;
using Action = System.Action;

namespace DutyLootPreview.Features.InDutyIntegration;

/// <summary>
/// Displays the "Open Duty Loot" button near the active duty info
/// </summary>
public class InDutyController : IDisposable {
    private OverlayController? overlayController;
    private InDutyOverlayNode? inDutyButton;

    public void Enable() {
        inDutyButton = new InDutyOverlayNode() {
            OnClick = () => Env.DutyLootPreviewAddon.Toggle(),
            Size = new Vector2(20.0f, 20.0f),
        };

        overlayController = new OverlayController();
        overlayController.AddNode(inDutyButton);
    }

    public void Dispose() {
        overlayController?.Dispose();
        overlayController = null;

        inDutyButton = null; // inDutyButton is cleaned up by overlayController.
    }

    public static unsafe uint? GetActiveDutyContentId() {
        var id = GameMain.Instance()->CurrentContentFinderConditionId;
        if (!DutyInfoService.IsSupportedContent(id)) {
            return null;
        }
        return id;
    }
}
