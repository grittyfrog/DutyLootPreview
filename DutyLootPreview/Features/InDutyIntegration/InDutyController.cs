using System;
using System.Numerics;
using Dalamud.Game.Gui;
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

    /// <summary>
    /// The CFCID that player is currently in (if any).
    /// </summary>
    public uint? ActiveContentFinderConditionId {
        get;
        private set {
            field = value;
            if (field.HasValue && field.Value == 0) { field = null; }
        }
    }

    public void Enable() {
        Env.ClientState.TerritoryChanged += OnTerritoryChanged;
        Env.GameGui.AgentUpdate += OnAgentUpdate;

        inDutyButton = new InDutyOverlayNode() {
            OnClick = () => Env.DutyLootPreviewAddon.Toggle(),
            Size = new Vector2(20.0f, 20.0f),
        };

        overlayController = new OverlayController();
        overlayController.AddNode(inDutyButton);
    }

    public void Dispose() {
        Env.ClientState.TerritoryChanged -= OnTerritoryChanged;
        Env.GameGui.AgentUpdate -= OnAgentUpdate;

        overlayController?.Dispose();
        overlayController = null;

        inDutyButton = null; // inDutyButton is cleaned up by overlayController.
    }

    private unsafe void Refresh() {
        if (inDutyButton == null) { return; }

        ActiveContentFinderConditionId = GameMain.Instance()->CurrentContentFinderConditionId;
    }

    private void OnTerritoryChanged(uint u) {
        Refresh();
    }

    private void OnAgentUpdate(AgentUpdateFlag flag) {
        if (flag.HasFlag(AgentUpdateFlag.UnlocksUpdate)) {
            Refresh();
        }
    }
}
