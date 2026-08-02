using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.ContextMenu;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using KamiToolKit.Extensions;
using DutyLootPreview.Data;
using ContextMenu = KamiToolKit.ContextMenu.ContextMenu;
using DutyLootPreview.Extensions;
using KamiToolKit.Interfaces;
using KamiToolKit.Nodes.Simplified;

namespace DutyLootPreview.UI.DutyLootWindow;

public unsafe class DutyLootNode : ListItemNode<DutyItem>, IListItemNode {
    public static float ItemHeight => DutyLootPreviewAddon.ItemHeight;
    public static float IconPadding => 2.0f;

    private readonly IconImageNode iconNode;
    private readonly TextNode itemNameTextNode;
    private readonly SimpleImageNode favoriteStarNode;
    private readonly SimpleImageNode infoIconNode;
    private readonly SimpleImageNode checkmarkIconNode;
    private readonly ContextMenu contextMenu;
    private readonly SimpleImageNode armoireIconNode;

    public DutyLootNode() {
        contextMenu = new ContextMenu();

        iconNode = new IconImageNode {
            TextureSize = new Vector2(ItemHeight),
            WrapMode = WrapMode.Stretch,
            ImageNodeFlags = ImageNodeFlags.AutoFit,
        };
        iconNode.AttachNode(this);

        favoriteStarNode = new SimpleImageNode {
            TextureCoordinates = new Vector2(96, 0),
            TextureSize = new Vector2(20, 20),
            TexturePath = "ui/uld/MinionNoteBook.tex",
            Size = new Vector2(20, 20),
            IsVisible = false,
        };
        favoriteStarNode.AttachNode(this);

        itemNameTextNode = new TextNode {
            TextFlags = TextFlags.Ellipsis,
            AlignmentType = AlignmentType.Left,
        };
        itemNameTextNode.AttachNode(this);

        infoIconNode = new SimpleImageNode {
            TextureCoordinates = new Vector2(112, 84),
            TextureSize = new Vector2(28, 28),
            TexturePath = "ui/uld/CircleButtons.tex",
            WrapMode = WrapMode.Stretch,
            ShowClickableCursor = true,
        };
        infoIconNode.AttachNode(this);

        checkmarkIconNode = new SimpleImageNode {
            TextureCoordinates = new Vector2(60, 28),
            TextureSize = new Vector2(28, 24),
            TexturePath = "ui/uld/RecipeNoteBook.tex",
            IsVisible = false,
        };
        checkmarkIconNode.AttachNode(this);

        armoireIconNode = new SimpleImageNode {
            TextureCoordinates = new Vector2(36, 18),
            TextureSize = new Vector2(18, 18),
            TexturePath = "ui/uld/ItemDetailPutIn.tex",
            IsVisible = false,
        };
        armoireIconNode.AttachNode(this);

        AddEvent(AtkEventType.MouseClick, MouseClickCallback);
    }

    private void MouseClickCallback(AtkEventListener* atkEventListener, AtkEventType atkEventType, int i, AtkEvent* atkEvent, AtkEventData* atkEventData) {
        if (ItemData is null) return;

        if (atkEventData->IsLeftClick) {
            OnLeftClick();
        }
        else if (atkEventData->IsRightClick) {
            OnRightClick();
        }
    }

    private void OnLeftClick() {
        var dutyItem = ItemData;
        if (dutyItem is null) return;

        if (dutyItem.Item.CanTryOn) {
            AgentTryon.TryOn(0, dutyItem.Item.RowId);
        }
    }

    private void OnRightClick() {
        var dutyItem = ItemData;
        if (dutyItem is null) return;
        var item = dutyItem.Item;

        contextMenu.Clear();

        if (item.CanTryOn) {
            contextMenu.AddItem(
                Env.DataManager.GetAddonText(2426), // Try On
                () => AgentTryon.TryOn(0, item.RowId));
        }

        var isFavorite = Env.Config.FavoriteItems.Contains(item.RowId);
        contextMenu.AddItem(new ContextMenuItem {
            Name = isFavorite
                       ? Env.DataManager.GetAddonText(8324)  // Remove from Favorites
                       : Env.DataManager.GetAddonText(8323), // Add to Favorites
            OnClick = () => {
                if (isFavorite) {
                    Env.Config.FavoriteItems.Remove(item.RowId);
                }
                else {
                    Env.Config.FavoriteItems.Add(item.RowId);
                }
                Env.Config.Save();
                favoriteStarNode.IsVisible = !isFavorite;
            },
        });

        contextMenu.AddItem(
            Env.DataManager.GetAddonText(4379), // Search for Item
            () => ItemFinderModule.Instance()->SearchForItem(item.RowId));

        contextMenu.AddItem(
            Env.DataManager.GetAddonText(4697), // Link
            () => AgentChatLog.Instance()->LinkItem(item.RowId));

        contextMenu.AddItem(
            Env.DataManager.GetAddonText(13439), // Search Recipes Using This Material
            () => AgentRecipeProductList.Instance()->SearchForRecipesUsingItem(item.RowId));

        contextMenu.Open();
    }

    protected override void OnSizeChanged() {
        base.OnSizeChanged();

        iconNode.Size = new Vector2(ItemHeight) - new Vector2(IconPadding * 2);
        iconNode.Position = new Vector2(IconPadding);
        var iconEndPos = iconNode.Position + iconNode.Size + new Vector2(IconPadding);

        // Scale star proportionally (original: 20x20 star on 44x44 icon)
        var starSize = iconNode.Height * (20f / 44f);
        favoriteStarNode.Size = new Vector2(starSize, starSize);

        // Position in top-right corner, slightly above icon edge
        favoriteStarNode.Position = new Vector2(iconEndPos.X - favoriteStarNode.Width, -2);

        var infoSize = Size.Y * 0.6f;
        infoIconNode.Size = new Vector2(infoSize, infoSize);
        infoIconNode.Position = new Vector2(Width - infoSize, Height / 2 - infoSize / 2);

        checkmarkIconNode.Size = new Vector2(28, 24);
        checkmarkIconNode.Position = iconEndPos - checkmarkIconNode.Size * 0.8f;

        armoireIconNode.Size = new Vector2(18, 18);
        armoireIconNode.Position = iconEndPos - armoireIconNode.Size - Vector2.One;

        itemNameTextNode.Size = new Vector2(Width - iconNode.Width - infoSize - 12.0f, Height);
        itemNameTextNode.Position = new Vector2(iconEndPos.X + 2.0f, 0.0f);
    }

    protected override void SetNodeData(DutyItem dutyItem) {
        var item = dutyItem.Item;
        iconNode.IconId = item.Icon;
        itemNameTextNode.String = item.Name;

        infoIconNode.TextTooltip = string.Join("\n", dutyItem.Sources.Select(s => s.Name).Distinct());

        if (item.IsStorableInCabinet) {
            checkmarkIconNode.IsVisible = false;
            armoireIconNode.IsVisible = true;
            armoireIconNode.TextureCoordinates = new Vector2(36, item.IsStoredInCabinet ? 18 : 0);
            // armoireIconNode.MultiplyColor = item.IsStoredInCabinet ? Vector3.One : new Vector3(0.8f, 0, 0);
        }
        else {
            checkmarkIconNode.IsVisible = item.IsUnlocked;
            armoireIconNode.IsVisible = false;
        }

        iconNode.ItemTooltip = item.RowId;
        favoriteStarNode.IsVisible = Env.Config.FavoriteItems.Contains(item.RowId);
    }
}
