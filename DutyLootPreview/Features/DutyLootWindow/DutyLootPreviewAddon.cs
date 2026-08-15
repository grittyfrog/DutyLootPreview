using System;
using System.Linq;
using System.Numerics;
using DutyLootPreview.Data;
using DutyLootPreview.Extensions;
using DutyLootPreview.Features.InDutyIntegration;
using DutyLootPreview.Features.JournalIntegration;
using DutyLootPreview.Resources;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.BaseTypes;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;

namespace DutyLootPreview.Features.DutyLootWindow;

public unsafe class DutyLootWindowAddon : NativeAddon {
    private const int VisibleItemCount = 12;
    internal const float ItemHeight = 32.0f;
    private const float ItemSpacing = 2.25f;
    private const float FilterBarHeight = 36.0f;
    private const float SeparatorHeight = 4.0f;
    private const float WindowOverhead = 67.75f;

    private const float WindowWidth = 350.0f;
    private const float ListAreaHeight = VisibleItemCount * ItemHeight + (VisibleItemCount - 1) * ItemSpacing;
    private const float WindowHeight = ListAreaHeight + FilterBarHeight + SeparatorHeight + ItemSpacing + WindowOverhead;

    private DutyLootFilterBarNode? filterBarNode;
    private HorizontalLineNode? separatorNode;
    private ListNode<DutyItem, DutyLootNode>? listNode;
    private TextNode? hintTextNode;

    private Polled<uint?>? activeDutyContentIdPoll;
    private Polled<uint?>? activeJournalContentIdPoll;
    private Polled<LootFilter>? lootFilterPoll;

    public DutyLootWindowAddon() {
        Size = new Vector2(WindowWidth, WindowHeight);
    }

    protected override void OnSetup(AtkUnitBase* addon, Span<AtkValue> atkValueSpan) {
        filterBarNode = new DutyLootFilterBarNode {
            Position = ContentStartPosition,
            Size = new Vector2(ContentSize.X, FilterBarHeight)
        };
        filterBarNode.AttachNode(this);

        separatorNode = new HorizontalLineNode {
            Position = ContentStartPosition + new Vector2(0, FilterBarHeight),
            Size = new Vector2(ContentSize.X, SeparatorHeight),
        };
        separatorNode.AttachNode(this);

        var listAreaPosition = ContentStartPosition + new Vector2(0, FilterBarHeight + SeparatorHeight + ItemSpacing);
        var listAreaSize = ContentSize - new Vector2(0, FilterBarHeight + SeparatorHeight + ItemSpacing);

        listNode = new ListNode<DutyItem, DutyLootNode> {
            Position = listAreaPosition,
            Size = listAreaSize,
            OptionsList = [],
            ItemSpacing = ItemSpacing,
        };
        listNode.AttachNode(this);

        hintTextNode = new TextNode {
            Position = ContentStartPosition,
            Size = ContentSize,
            TextColor = ColorHelper.GetColor(1),
            LineSpacing = 18,
            TextFlags = TextFlags.MultiLine | TextFlags.Edge | TextFlags.WordWrap,
            AlignmentType = AlignmentType.Center,
            String = Strings.DutyLoot_NoItemsMessage,
        };
        UpdateHintTextNodePosition();
        hintTextNode.AttachNode(this);

        activeDutyContentIdPoll = new(InDutyController.GetActiveDutyContentId);
        activeJournalContentIdPoll = new(JournalUiController.GetActiveJournalContentId);
        lootFilterPoll = new(() => filterBarNode.CurrentFilter);
    }

    protected override void OnUpdate(AtkUnitBase* addon) {
        Refresh();
    }

    private void Refresh() {
        if (listNode is null || hintTextNode is null || filterBarNode is null || separatorNode is null) return;
        if (lootFilterPoll is null || activeDutyContentIdPoll is null || activeJournalContentIdPoll is null) return;

        var (lootFilter, lootFilterChanged) = lootFilterPoll.Poll();
        var (activeDutyContentId, activeDutyContentIdChanged) = activeDutyContentIdPoll.Poll();
        var (activeJournalContentId, activeJournalContentIdChanged) = activeJournalContentIdPoll.Poll();

        var changed = lootFilterChanged || activeDutyContentIdChanged || activeJournalContentIdChanged;
        if (!changed) { return; }

        var activeContentId = activeDutyContentId ?? activeJournalContentId ?? null;

        var dutyInfo = activeContentId.HasValue ? Env.DutyInfoService.GetDutyInfo(activeContentId.Value) : null;
        var items = dutyInfo?.DutyItems ?? [];

        var filteredItems = lootFilter switch {
            LootFilter.Favorites => items.Where(dutyItem => Env.Config.FavoriteItems.Contains(dutyItem.Item.RowId)),
            LootFilter.Equipment => items.Where(dutyItem => dutyItem.Item.IsEquipment),
            LootFilter.Misc => items.Where(dutyItem => !dutyItem.Item.IsEquipment),
            _ => items,
        };

        var displayItems = filteredItems.Order().ToList();

        listNode.OptionsList = displayItems;
        listNode.ResetScroll();

        var hasData = items.Count != 0;
        filterBarNode.IsVisible = hasData;
        separatorNode.IsVisible = hasData;

        var hasResults = displayItems.Count > 0;
        listNode.IsVisible = hasResults;
        hintTextNode.IsVisible = !hasResults;

        if (!hasResults) {
            hintTextNode.String = true switch {
                _ when hasData => Strings.DutyLoot_NoResultsMessage,
                _ => Strings.DutyLoot_NoItemsMessage,
            };
            UpdateHintTextNodePosition();
        }
    }

    private void UpdateHintTextNodePosition() {
        if (filterBarNode is null || separatorNode is null || hintTextNode is null) return;
        var offsetTop = 0f;
        if (filterBarNode.IsVisible) offsetTop += filterBarNode.Height;
        if (separatorNode.IsVisible) offsetTop += separatorNode.Height;
        hintTextNode.Size = hintTextNode.Size with { Y = ContentSize.Y - offsetTop };
        hintTextNode.Position = hintTextNode.Position with { Y = ContentStartPosition.Y + offsetTop };
    }
}
