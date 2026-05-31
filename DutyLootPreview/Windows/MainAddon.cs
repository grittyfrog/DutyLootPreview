using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;

namespace DutyLootPreview.Windows;

/// <summary>
/// The main Duty Loot Preview window. Currently a walking-skeleton placeholder
/// containing a single text node, to be replaced with the real loot UI.
/// </summary>
public unsafe class MainAddon : NativeAddon {
    private TextNode? helloNode;

    protected override void OnSetup(AtkUnitBase* addon, System.Span<AtkValue> atkValueSpan) {
        helloNode = new TextNode {
            Position = ContentStartPosition,
            Size = ContentSize,
            FontSize = 18,
            AlignmentType = AlignmentType.Center,
            String = "Hello from Duty Loot Preview!",
        };
        helloNode.AttachNode(this);
    }
}
