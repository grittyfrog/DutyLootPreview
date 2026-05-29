using System;
using Dalamud.Game.Command;

namespace DutyLootPreview.Commands;

public class PluginCommandManager : IDisposable {
    private const string MainCommand = "/dlp";

    public PluginCommandManager() {
        Env.CommandManager.AddHandler(MainCommand, new CommandInfo(OnMainCommand) {
            HelpMessage = "Toggle the Duty Loot Preview window."
        });
    }

    private void OnMainCommand(string command, string arguments) {
        Env.PluginWindowManager.MainWindow.Toggle();
    }

    public void Dispose() {
        Env.CommandManager.RemoveHandler(MainCommand);
    }
}
