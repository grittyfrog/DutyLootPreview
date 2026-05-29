using System;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace DutyLootPreview.Windows;

public class MainWindow : Window, IDisposable {
    public MainWindow() : base("Duty Loot Preview###DutyLootPreviewMain") {
        SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new(400, 300),
            MaximumSize = new(2000, 2000)
        };
    }

    public override void Draw() {
        ImGui.Text("Hello from Duty Loot Preview.");
    }

    public void Dispose() {
    }
}
