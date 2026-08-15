using System;
using Dalamud.Game.Gui;

namespace DutyLootPreview.Data;

public sealed class EventTrackers : IDisposable {
    public EventSignal UnlocksChanged { get; } = EventSignal.From<AgentUpdateFlag>(
        handler => Env.GameGui.AgentUpdate += handler,
        handler => Env.GameGui.AgentUpdate -= handler,
        flag => flag.HasFlag(AgentUpdateFlag.UnlocksUpdate));

    public void Dispose() {
        UnlocksChanged.Dispose();
    }
}
