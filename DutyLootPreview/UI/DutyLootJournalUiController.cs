using System;
using System.Numerics;
using DutyLootPreview.Resources;
using FFXIVClientStructs.FFXIV.Client.UI;
using KamiToolKit.Controllers;
using KamiToolKit.Enums;

namespace DutyLootPreview.UI;

/// <summary>
/// Attaches the "Open Duty Loot" button to the Journal.
/// </summary>
public class DutyLootJournalUiController : IDisposable {
    private AddonController<AddonJournalDetail>? journalDetail;
    private DutyLootOpenWindowButtonNode? lootButtonNode;
    private ushort attachedAddonId;

    public uint? ActiveContentFinderConditionId {
        get;
        set {
            if (field != value) {
                field = value;
                refresh = true;
            }
        }
    }
    private bool refresh = true;

    public void Enable() {
        unsafe {
            journalDetail = new AddonController<AddonJournalDetail> {
                AddonName = "JournalDetail",
                OnSetup = SetupJournalDetail,
                OnFinalize = FinalizeJournalDetail,
                OnRefresh = RefreshJournalDetail,
            };
        }

        journalDetail.Enable();
    }

    public void Dispose() {
        journalDetail?.Dispose();
        journalDetail = null;
    }

    private unsafe void SetupJournalDetail(AddonJournalDetail* addon) {
        var dutyTitleNode = addon->GetNodeById(37);
        if (dutyTitleNode is null) return;

        var existing = addon->DutyNameTextNode; // ID: 38
        if (existing is null) return;

        // Only attach if parent is the Duty Finder
        if (addon->ParentId is not 0) {
            var parentAddon = RaptureAtkUnitManager.Instance()->GetAddonById(addon->ParentId);
            if (parentAddon is null || (parentAddon->NameString != "ContentsFinder" && parentAddon->NameString != "RaidFinder")) {
                return;
            }
        }

        if (journalDetail is not null) {
            CleanupAttached();
        }

        lootButtonNode = new DutyLootOpenWindowButtonNode() {
            Position = new Vector2(420.0f, 68.0f),
            Size = new Vector2(32.0f, 32.0f),
            TextTooltip = Strings.DutyLoot_Tooltip_JournalButton,
            OnClick = () => Env.DutyLootPreviewAddon.Toggle(),
            IsVisible = false,
        };
        lootButtonNode.AttachNode(dutyTitleNode, NodePosition.AfterTarget);
        attachedAddonId = addon->Id;

        Refresh();
    }

    private unsafe void RefreshJournalDetail(AddonJournalDetail* addon) {
        Refresh();
    }

    private unsafe void Refresh() {
        if (lootButtonNode == null) return;

        var addon = Env.GameGui.GetAddonByName<AddonJournalDetail>("JournalDetail");
        if (addon == null) return;

        if (!ActiveContentFinderConditionId.HasValue) {
            lootButtonNode.IsVisible = false;
            return;
        }

        var dutyInfo = Env.DutyInfoService.GetDutyInfo(ActiveContentFinderConditionId.Value);
        if (dutyInfo == null) {
            lootButtonNode.IsVisible = false;
            return;
        }

        lootButtonNode.IsVisible = true;
        lootButtonNode.CheckmarkVisible = dutyInfo.AllUnlocksUnlocked();
    }

    private unsafe void FinalizeJournalDetail(AddonJournalDetail* addon) {
        if (addon->Id != attachedAddonId) return;

        CleanupAttached();
    }

    private void CleanupAttached() {
        lootButtonNode?.Dispose();
        lootButtonNode = null;
        attachedAddonId = 0;
    }
}
