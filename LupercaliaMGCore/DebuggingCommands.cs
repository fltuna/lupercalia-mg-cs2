using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Admin;
using LupercaliaMGCore.model;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace LupercaliaMGCore;

public class Debugging : IPluginModule
{
    private LupercaliaMGCore m_CSSPlugin;

    public string PluginModuleName => "DebuggingCommands";

    private readonly Dictionary<CCSPlayerController, Vector> savedPlayerPos = new();


    public Debugging(LupercaliaMGCore plugin)
    {
        m_CSSPlugin = plugin;
        m_CSSPlugin.AddCommand("css_dbg_savepos", "Save current location for teleport", CommandSavePos);
        m_CSSPlugin.AddCommand("css_dbg_restorepos", "Use saved location to teleport", CommandRestorePos);
    }

    public void AllPluginsLoaded()
    {
    }

    public void UnloadModule()
    {
        m_CSSPlugin.RemoveCommand("css_dbg_savepos", CommandSavePos);
        m_CSSPlugin.RemoveCommand("css_dbg_restorepos", CommandRestorePos);
    }

    private void CommandSavePos(CCSPlayerController? client, CommandInfo info)
    {
        if (client == null)
            return;

        if (!PluginSettings.GetInstance.m_CVDebuggingEnabled.Value)
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

        if (!PluginSettings.GetInstance.m_CVDebuggingEnabled.Value)
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

        Vector? vector = null;

        if (!savedPlayerPos.TryGetValue(client, out vector) || vector == null)
        {
            client.PrintToChat("There is no saved location! save location first!");
            return;
        }

        client.PrintToChat($"Teleported to {vector.X}, {vector.Y}, {vector.Z}");
        playerPawn.Teleport(vector, playerPawn.AbsRotation);
    }
}