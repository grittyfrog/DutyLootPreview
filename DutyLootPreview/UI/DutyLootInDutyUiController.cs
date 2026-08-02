using System;
using System.Numerics;
using KamiToolKit.UiOverlay;
using Action = System.Action;

namespace DutyLootPreview.UI;

/// <summary>
/// Displays the "Open Duty Loot" button near the active duty info
/// </summary>
public class DutyLootInDutyUiController : IDisposable {
    private OverlayController? overlayController;

    public Action? OnButtonClicked { get; init; }

    public void Enable() {
        overlayController = new OverlayController();

        overlayController.AddNode(new DutyLootInDutyButtonNode() {
            OnClick = () => Env.DutyLootPreviewAddon.Toggle(),
            Size = new Vector2(20.0f, 20.0f),
        });
    }

    public void Dispose() {
        overlayController?.Dispose();
        overlayController = null;
    }
}
