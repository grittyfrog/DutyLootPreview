using System;
using System.Collections.Generic;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using DutyLootPreview.Commands;
using DutyLootPreview.Windows;

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

        WindowSystem = new WindowSystem("DutyLootPreview");

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // Order matters: anything a manager needs must be initialized before it.
        PluginWindowManager = Own(new PluginWindowManager());
        PluginCommandManager = Own(new PluginCommandManager());
    }

    public static void Dispose() {
        while (Owned.Count > 0) {
            try {
                Owned.Pop().Dispose();
            } catch (Exception ex) {
                PluginLog?.Error(ex, "Error disposing subsystem");
            }
        }
    }

    /// ===
    /// Dalamud Injections
    /// ===

    [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] public static IPluginLog PluginLog { get; private set; } = null!;
    [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] public static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] public static IClientState ClientState { get; private set; } = null!;
    [PluginService] public static IDataManager DataManager { get; private set; } = null!;
    [PluginService] public static IFramework Framework { get; private set; } = null!;
    [PluginService] public static ITextureProvider TextureProvider { get; private set; } = null!;

    /// ===
    /// Plugin globals
    /// ===

    public static WindowSystem WindowSystem { get; private set; } = null!;
    public static Configuration Configuration { get; private set; } = null!;

    /// ===
    /// Plugin subsystems
    /// ===

    public static PluginWindowManager PluginWindowManager { get; private set; } = null!;
    public static PluginCommandManager PluginCommandManager { get; private set; } = null!;
}
