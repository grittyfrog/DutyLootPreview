using System;
using System.Numerics;
using DutyLootPreview.Data;
using DutyLootPreview.Resources;
using DutyLootPreview.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using KamiToolKit.Controllers;
using KamiToolKit.Enums;

namespace DutyLootPreview.Features.JournalIntegration;

/// <summary>
/// Attaches the "Open Duty Loot" button to the Journal.
/// </summary>
public class JournalUiController : IDisposable {
    private AddonController<AddonJournalDetail>? journalDetail;
    private DutyLootOpenWindowButtonNode? lootButtonNode;
    private ushort attachedAddonId;

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

        // Only attach if parent is the Duty Finder / Raid Finder
        if (addon->ParentId is not 0) {
            var parentAddon = RaptureAtkUnitManager.Instance()->GetAddonById(addon->ParentId);
            if (parentAddon is null || (parentAddon->NameString != "ContentsFinder" && parentAddon->NameString != "RaidFinder")) {
                return;
            }
        }

        CleanupAttached();

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

        var activeJournalContentFinderConditionId = GetActiveJournalContentId();

        if (!activeJournalContentFinderConditionId.HasValue) {
            lootButtonNode.IsVisible = false;
            return;
        }

        var dutyInfo = Env.DutyInfoService.GetDutyInfo(activeJournalContentFinderConditionId.Value);
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


    // JournalDetail is used by Duty Finder & Raid Finder, switching between
    // them doesn't call `Finalize`, so we need to make sure we clear any
    // previously attached button before attaching a fresh oine.
    private void CleanupAttached() {
        lootButtonNode?.Dispose();
        lootButtonNode = null;
        attachedAddonId = 0;
    }

    /// <summary>
    /// Returns the ContentId being viewed in the Journal (if any)
    /// </summary>
    public static unsafe uint? GetActiveJournalContentId() {
        var agentContentsFinder = AgentContentsFinder.Instance();
        if (agentContentsFinder->IsAddonShown() && DutyInfoService.IsSupportedContent(agentContentsFinder->SelectedDuty)) {
            return agentContentsFinder->SelectedDuty.Id;
        }

        var agentRaidFinder = AgentRaidFinder.Instance();
        if (agentRaidFinder->IsAddonShown()) {
            var selectedTab = (int)agentRaidFinder->SelectedTab;
            var selectedEntry = (int)agentRaidFinder->SelectedEntry;
            var raidId = agentRaidFinder->Tabs[selectedTab].Entries[selectedEntry].ContentFinderConditionId;

            if (DutyInfoService.IsSupportedContent(raidId)) {
                return raidId;
            }
        }

        return null;
    }

}
