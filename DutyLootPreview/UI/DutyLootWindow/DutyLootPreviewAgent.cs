using System;
using Dalamud.Game.Gui;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using KamiToolKit.Controllers;
using Lumina.Excel.Sheets;

namespace DutyLootPreview.UI.DutyLootWindow;

public class DutyLootPreviewAgent : IDisposable {
    private AddonController<AddonContentsFinder>? contentsFinder;
    private AddonController<AddonRaidFinder>? raidFinder;

    public unsafe void Enable() {
        Env.ClientState.TerritoryChanged += OnTerritoryChanged;
        Env.GameGui.AgentUpdate += OnAgentUpdate;

        contentsFinder = new AddonController<AddonContentsFinder> {
            AddonName = "ContentsFinder",
            OnSetup = _ => RefreshAddons(),
            OnRefresh = _ => RefreshAddons(),
            OnFinalize = _ => RefreshAddons(),
        };

        raidFinder = new AddonController<AddonRaidFinder> {
            AddonName = "RaidFinder",
            OnSetup = _ => RefreshAddons(),
            OnRefresh = _ => RefreshAddons(),
            OnFinalize = _ => RefreshAddons(),
        };

        contentsFinder.Enable();
        raidFinder.Enable();
    }

    public void Dispose() {
        Env.ClientState.TerritoryChanged -= OnTerritoryChanged;
        Env.GameGui.AgentUpdate -= OnAgentUpdate;
        Env.Framework.Run(() => {
            contentsFinder?.Dispose();
            raidFinder?.Dispose();
        }).GetAwaiter().GetResult();
    }

    private void RefreshAddons() {
        var activeContentId = GetActiveContentId();
        Env.DutyLootPreviewAddon.ContentFinderConditionId = activeContentId;
        Env.DutyLootJournalUiController.ActiveContentFinderConditionId = activeContentId;
    }

    private static unsafe uint? GetActiveContentId() {
        // Priority 1: Currently in a duty
        var currentDutyId = GameMain.Instance()->CurrentContentFinderConditionId;
        if (currentDutyId != 0 && IsSupportedContent(new ContentsId { ContentType = ContentsType.Regular, Id = currentDutyId })) {
            return currentDutyId;
        }

        // Priority 2: Viewing a specific duty in ContentsFinder or RaidFinder
        var agentContentsFinder = AgentContentsFinder.Instance();
        if (agentContentsFinder->IsAddonShown() && IsSupportedContent(agentContentsFinder->SelectedDuty)) {
            return agentContentsFinder->SelectedDuty.Id;
        }

        var agentRaidFinder = AgentRaidFinder.Instance();
        if (agentRaidFinder->IsAddonShown()) {
            var selectedTab = (int)agentRaidFinder->SelectedTab;
            var selectedEntry = (int)agentRaidFinder->SelectedEntry;
            var raidId = agentRaidFinder->Tabs[selectedTab].Entries[selectedEntry].ContentFinderConditionId;

            if (IsSupportedContent(new ContentsId { ContentType = ContentsType.Regular, Id = raidId })) {
                return raidId;
            }
        }

        return null;
    }

    private static bool IsSupportedContent(ContentsId content) {
        // Not for Content Roulette
        if (content.ContentType != ContentsType.Regular)
            return false;

        if (!Env.DataManager.GetExcelSheet<ContentFinderCondition>().TryGetRow(content.Id, out var cfc))
            return false;

        // Not for Guildhests (3), PvP (6), Gold Saucer (19)
        return cfc.ContentType.RowId is not (3 or 6 or 19);
    }

    private void OnTerritoryChanged(uint u) => RefreshAddons();

    private void OnAgentUpdate(AgentUpdateFlag flag) {
        if (flag.HasFlag(AgentUpdateFlag.UnlocksUpdate)) {
            RefreshAddons();
        }
    }
}
