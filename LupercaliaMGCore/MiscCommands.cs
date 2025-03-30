using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Commands.Targeting;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using LupercaliaMGCore.model;

namespace LupercaliaMGCore;

public class MiscCommands : IPluginModule
{
    private LupercaliaMGCore m_CSSPlugin;

    public string PluginModuleName => "MiscCommands";

    public MiscCommands(LupercaliaMGCore plugin)
    {
        m_CSSPlugin = plugin;

        m_CSSPlugin.AddCommand("css_knife", "give knife", CommandGiveKnife);
        m_CSSPlugin.AddCommand("css_spec", "Spectate", CommandSpectate);
    }

    public void AllPluginsLoaded()
    {
    }

    public void UnloadModule()
    {
        m_CSSPlugin.RemoveCommand("css_knife", CommandGiveKnife);
        m_CSSPlugin.RemoveCommand("css_spec", CommandSpectate);
    }


    private void CommandGiveKnife(CCSPlayerController? client, CommandInfo info)
    {
        if (client == null)
            return;

        if (!PlayerUtil.IsPlayerAlive(client))
        {
            client.PrintToChat(m_CSSPlugin.LocalizeStringWithPrefix("General.Command.Notification.ShouldBeAlive"));
            return;
        }

        if (!PluginSettings.GetInstance.m_CVMiscCMDGiveKnifeEnabled.Value)
        {
            client.PrintToChat(m_CSSPlugin.LocalizeStringWithPrefix("General.Command.Notification.FeatureEnabled"));
            return;
        }

        CCSPlayerPawn? playerPawn = client.PlayerPawn.Value;

        if (playerPawn == null)
        {
            client.PrintToChat(m_CSSPlugin.LocalizeStringWithPrefix("General.Command.Notification.NotUsableCurrently"));
            return;
        }

        CPlayer_WeaponServices? weaponServices = playerPawn.WeaponServices;

        if (weaponServices == null)
        {
            client.PrintToChat(m_CSSPlugin.LocalizeStringWithPrefix("General.Command.Notification.NotUsableCurrently"));
            return;
        }

        bool found = false;
        foreach (var weapon in weaponServices.MyWeapons)
        {
            if (weapon.Value?.DesignerName == "weapon_knife")
            {
                found = true;
                break;
            }
        }

        if (found)
        {
            client.PrintToChat(m_CSSPlugin.LocalizeStringWithPrefix("General.Command.Notification.AlreadyHave"));
        }
        else
        {
            client.PrintToChat(m_CSSPlugin.LocalizeStringWithPrefix("General.Command.Notification.Retrieved"));
            client.GiveNamedItem(CsItem.Knife);
        }
    }

    private void CommandSpectate(CCSPlayerController? client, CommandInfo info)
    {
        if (client == null)
            return;

        if (info.ArgCount <= 1)
        {
            if (client.Team == CsTeam.Spectator || client.Team == CsTeam.None)
                return;


            client.ChangeTeam(CsTeam.Spectator);
            client.PrintToChat(m_CSSPlugin.LocalizeStringWithPrefix("Misc.Spectate.Command.Notification.MovedToSpectator"));
            return;
        }

        if (PlayerUtil.IsPlayerAlive(client))
        {
            client.PrintToChat(m_CSSPlugin.LocalizeStringWithPrefix("Misc.Spectate.Command.Notification.OnlyDeadOrSpectator"));
            return;
        }

        TargetResult targets = info.GetArgTargetResult(1);

        if (targets.Count() > 1)
        {
            client.PrintToChat(m_CSSPlugin.LocalizeStringWithPrefix("Misc.Spectate.Command.Notification.MultipleTargetsFound", targets.Count()));
            return;
        }

        client.ExecuteClientCommand($"spec_player {targets.First().PlayerName}");
        client.PrintToChat(m_CSSPlugin.LocalizeStringWithPrefix("Misc.Spectate.Command.Notification.NowSpectating", targets.First().PlayerName));
    }
}