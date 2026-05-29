using System;

namespace DutyLootPreview.Windows;

public class PluginWindowManager : IDisposable {
    public MainWindow MainWindow { get; } = new();

    public PluginWindowManager() {
        Env.WindowSystem.AddWindow(MainWindow);

        Env.PluginInterface.UiBuilder.Draw += OnDraw;
        Env.PluginInterface.UiBuilder.OpenMainUi += OnOpenMainUi;
        Env.PluginInterface.UiBuilder.OpenConfigUi += OnOpenConfigUi;
    }

    public void Dispose() {
        Env.PluginInterface.UiBuilder.Draw -= OnDraw;
        Env.PluginInterface.UiBuilder.OpenMainUi -= OnOpenMainUi;
        Env.PluginInterface.UiBuilder.OpenConfigUi -= OnOpenConfigUi;

        Env.WindowSystem.RemoveAllWindows();

        MainWindow.Dispose();
    }

    private void OnDraw() => Env.WindowSystem.Draw();
    private void OnOpenMainUi() => MainWindow.IsOpen = true;
    private void OnOpenConfigUi() => MainWindow.IsOpen = true;
}
