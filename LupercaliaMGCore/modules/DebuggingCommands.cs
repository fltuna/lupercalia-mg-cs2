using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using LupercaliaMGCore.model;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace LupercaliaMGCore.modules;

public class Debugging(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "DebuggingCommands";

    public override string ModuleChatPrefix => "[Debugging Commands]";

    private readonly Dictionary<CCSPlayerController, Vector> savedPlayerPos = new();

    protected override void OnInitialize()
    {
        Plugin.AddCommand("css_dbg_savepos", "Save current location for teleport", CommandSavePos);
        Plugin.AddCommand("css_dbg_restorepos", "Use saved location to teleport", CommandRestorePos);
    }

    protected override void OnUnloadModule()
    {
        Plugin.RemoveCommand("css_dbg_savepos", CommandSavePos);
        Plugin.RemoveCommand("css_dbg_restorepos", CommandRestorePos);
    }

    private void CommandSavePos(CCSPlayerController? client, CommandInfo info)
    {
        if (client == null)
            return;

        if (!PluginSettings.m_CVDebuggingEnabled.Value)
        {
            client.PrintToChat("Debugging feature is disabled.");
            return;
        }

        if (!PlayerUtil.IsPlayerAlive(client))
        {
            client.PrintToChat("You must be alive to use this command.");
            return;
        }

        var playerPawn = client.PlayerPawn.Value;

        if (playerPawn == null)
            return;

        var clientAbsPos = playerPawn.AbsOrigin;

        if (clientAbsPos == null)
        {
            client.PrintToChat("Failed to retrieve your position!");
            return;
        }

        var vector = new Vector(clientAbsPos.X, clientAbsPos.Y, clientAbsPos.Z);

        savedPlayerPos[client] = vector;

        client.PrintToChat($"Location saved! {vector.X}, {vector.Y}, {vector.Z}");
    }

    private void CommandRestorePos(CCSPlayerController? client, CommandInfo info)
    {
        if (client == null)
            return;

        if (!PluginSettings.m_CVDebuggingEnabled.Value)
        {
            client.PrintToChat("Debugging feature is disabled.");
            return;
        }

        if (!PlayerUtil.IsPlayerAlive(client))
        {
            client.PrintToChat("You must be alive to use this command.");
            return;
        }

        var playerPawn = client.PlayerPawn.Value;

        if (playerPawn == null)
            return;

        Vector? vector;

        if (!savedPlayerPos.TryGetValue(client, out vector))
        {
            client.PrintToChat("There is no saved location! save location first!");
            return;
        }

        client.PrintToChat($"Teleported to {vector.X}, {vector.Y}, {vector.Z}");
        playerPawn.Teleport(vector, playerPawn.AbsRotation);
    }
}