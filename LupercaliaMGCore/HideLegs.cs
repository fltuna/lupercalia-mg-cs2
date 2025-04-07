using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities;
using System.Drawing;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using LupercaliaMGCore.model;

namespace LupercaliaMGCore;

public class HideLegs(LupercaliaMGCore plugin) : PluginModuleBase(plugin)
{
    public override string PluginModuleName => "HideLegs";

    private Dictionary<ulong, bool> m_steamIdToIsHideLegsActive = new();

    public override void Initialize()
    {
        Plugin.RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);

        Plugin.AddCommand("css_legs", "Toggles the visibility of the firstperson legs view model", CommandLegs);
    }

    public override void UnloadModule()
    {
        Plugin.DeregisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);

        Plugin.RemoveCommand("css_legs", CommandLegs);
    }


    private bool IsEnabled => PluginSettings.m_CVHideLegsEnabled.Value;

    private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        if (!IsEnabled)
        {
            return HookResult.Continue;
        }

        CCSPlayerController? player = @event.Userid;

        if (player == null || !player.IsValid)
        {
            return HookResult.Continue;
        }

        Plugin.AddTimer(0.5f, () => { UpdateHideLegs(player); }, TimerFlags.STOP_ON_MAPCHANGE);

        return HookResult.Continue;
    }

    private void CommandLegs(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid)
        {
            return;
        }

        if (!IsEnabled)
        {
            player.PrintToChat(LocalizeWithPrefix("HideLegs.Command.Notification.NotAvailable"));
            return;
        }

        bool isHideLegsActive = !m_steamIdToIsHideLegsActive.GetValueOrDefault(player.SteamID, false);
        m_steamIdToIsHideLegsActive[player.SteamID] = isHideLegsActive;

        var messageName = isHideLegsActive
            ? "HideLegs.Command.Notification.HideLegs"
            : "HideLegs.Command.Notification.ShowLegs";
        
        player.PrintToChat(LocalizeWithPrefix(messageName));

        UpdateHideLegs(player);
    }

    private void UpdateHideLegs(CCSPlayerController player)
    {
        bool isHideLegsActive = m_steamIdToIsHideLegsActive.GetValueOrDefault(player.SteamID, false);
        SetLegsVisibility(player, !isHideLegsActive);
    }

    // Borrowed from
    // - https://github.com/dran1x/CS2-HideLowerBody
    // - https://github.com/1Mack/CS2-HideLegs
    private void SetLegsVisibility(CCSPlayerController player, bool isVisible)
    {
        CBasePlayerPawn? playerPawn = player.PlayerPawn.Value;
        if (playerPawn == null)
        {
            return;
        }

        playerPawn.Render = Color.FromArgb(
            isVisible ? 255 : 254,
            playerPawn.Render.R,
            playerPawn.Render.G,
            playerPawn.Render.B
        );
        Utilities.SetStateChanged(playerPawn, "CBaseModelEntity", "m_clrRender");
    }
}