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

/// <summary>
/// Global environment / service locator.
///
/// Holds:
///   - Dalamud-injected services (via [PluginService])
///   - Plugin-owned subsystems, constructed in <see cref="Initialize"/>
///
/// Every owned IDisposable is registered through <see cref="Own{T}"/>,
/// which guarantees it is disposed exactly once in reverse construction
/// order when the plugin unloads. Forgetting to call Dispose on a
/// subsystem (which leaks command handlers, UI delegates, hooks, etc. on
/// reload) is structurally impossible as long as new subsystems use Own.
/// </summary>
public class Env {
    private static readonly Stack<IDisposable> Owned = new();

    /// <summary>
    /// Register an IDisposable owned by the plugin. It will be disposed in
    /// reverse-registration order during <see cref="Dispose"/>.
    /// </summary>
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

        DutyLootPreviewAddon = Own(new DutyLootWindowAddon {
            InternalName = "DutyLootPreview",
            Title = Strings.Title_DutyLootPreview,
        });

        DutyLootPreviewAgent = Own(new DutyLootWindowAgent());
        DutyLootJournalUiController = Own(new DutyLootJournalUiController());
        DutyLootInDutyUiController = Own(new InDutyController());
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
    [PluginService] public static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] public static IClientState ClientState { get; private set; } = null!;
    [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] public static IDataManager DataManager { get; private set; } = null!;
    [PluginService] public static IFramework Framework { get; private set; } = null!;
    [PluginService] public static IGameGui GameGui { get; set; } = null!;
    [PluginService] public static IPlayerState PlayerState { get; set; } = null!;
    [PluginService] public static IPluginLog PluginLog { get; private set; } = null!;
    [PluginService] public static ISeStringEvaluator SeStringEvaluator { get; private set; } = null!;
    [PluginService] public static ITextureProvider TextureProvider { get; private set; } = null!;
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
    public static DutyLootWindowAddon DutyLootPreviewAddon { get; private set; } = null!;
    public static DutyLootWindowAgent DutyLootPreviewAgent { get; private set; } = null!;
    public static DutyLootJournalUiController DutyLootJournalUiController { get; private set; } = null!;
    public static InDutyController DutyLootInDutyUiController { get; private set; } = null!;
}
