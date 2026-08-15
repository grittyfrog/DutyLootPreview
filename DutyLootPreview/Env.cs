using System;
using System.Collections.Generic;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using KamiToolKit;
using DutyLootPreview.Data;
using DutyLootPreview.Extensions.LuminaSupplemental;
using DutyLootPreview.Resources;
using DutyLootPreview.Features.DutyLootWindow;
using DutyLootPreview.Features.InDutyIntegration;
using DutyLootPreview.Features.JournalIntegration;

namespace DutyLootPreview;

public class Env {
    private static readonly Stack<IDisposable> Owned = new();

    private static T Own<T>(T obj) where T : IDisposable {
        Owned.Push(obj);
        return obj;
    }

    public static void Initialize(IDalamudPluginInterface pluginInterface) {
        pluginInterface.Create<Env>();

        // KTK must be initialized before any NativeAddon is constructed.
        KamiToolKitLibrary.Initialize(pluginInterface, "Duty Loot Preview");
        KamiToolKitLibrary.SetResourceManager(Strings.ResourceManager);

        Config = PluginInterface.GetPluginConfig() as DutyLootPreviewConfiguration ?? new DutyLootPreviewConfiguration();

        LumSup = new LumSupModule();
        DutyInfoService = new DutyInfoService();
        EventTrackers = Own(new EventTrackers());

        DutyLootPreviewAddon = Own(new DutyLootWindowAddon {
            InternalName = "DutyLootPreview",
            Title = Strings.Title_DutyLootPreview,
        });

        JournalUiController = Own(new JournalUiController());
        InDutyController = Own(new InDutyController());
    }

    public static void Dispose() {
        while (Owned.Count > 0) {
            try {
                Owned.Pop().Dispose();
            }
            catch (Exception ex) {
                PluginLog?.Error(ex, "Error disposing subsystem");
            }
        }

        KamiToolKitLibrary.Cleanup();
    }

    /// ===
    /// Dalamud Injections
    /// ===

    [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] public static IDataManager DataManager { get; private set; } = null!;
    [PluginService] public static IFramework Framework { get; private set; } = null!;
    [PluginService] public static IGameGui GameGui { get; set; } = null!;
    [PluginService] public static IPlayerState PlayerState { get; set; } = null!;
    [PluginService] public static IPluginLog PluginLog { get; private set; } = null!;
    [PluginService] public static ISeStringEvaluator SeStringEvaluator { get; private set; } = null!;
    [PluginService] public static IUnlockState UnlockState { get; set; } = null!;

    /// ===
    /// Plugin globals
    /// ===

    public static DutyLootPreviewConfiguration Config { get; private set; } = null!;

    /// ===
    /// Plugin subsystems
    /// ===

    public static LumSupModule LumSup { get; private set; } = null!;
    public static DutyInfoService DutyInfoService { get; private set; } = null!;
    public static EventTrackers EventTrackers { get; private set; } = null!;
    public static DutyLootWindowAddon DutyLootPreviewAddon { get; private set; } = null!;
    public static JournalUiController JournalUiController { get; private set; } = null!;
    public static InDutyController InDutyController { get; private set; } = null!;
}
