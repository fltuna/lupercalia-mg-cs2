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

        if (client.PlayerPawn.Value == null || client.PlayerPawn.Value.LifeState != (byte)LifeState_t.LIFE_ALIVE)
        {
            client.PrintToChat(
                LupercaliaMGCore.MessageWithPrefix(
                    m_CSSPlugin.Localizer["General.Command.Notification.ShouldBeAlive"]));
            return;
        }

        if (!PluginSettings.GetInstance.m_CVMiscCMDGiveKnifeEnabled.Value)
        {
            client.PrintToChat(
                LupercaliaMGCore.MessageWithPrefix(
                    m_CSSPlugin.Localizer["General.Command.Notification.FeatureDisabled"]));
            return;
        }

        CCSPlayerPawn? playerPawn = client.PlayerPawn.Value;

        if (playerPawn == null)
        {
            client.PrintToChat(LupercaliaMGCore.MessageWithPrefix("This command is not usable currently."));
            client.PrintToChat(
                LupercaliaMGCore.MessageWithPrefix(
                    m_CSSPlugin.Localizer["General.Command.Notification.NotUsableCurrently"]));
            return;
        }

        CPlayer_WeaponServices? weaponServices = playerPawn.WeaponServices;

        if (weaponServices == null)
        {
            client.PrintToChat(
                LupercaliaMGCore.MessageWithPrefix(
                    m_CSSPlugin.Localizer["General.Command.Notification.NotUsableCurrently"]));
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
            client.PrintToChat(
                LupercaliaMGCore.MessageWithPrefix(
                    m_CSSPlugin.Localizer["Misc.Knife.Command.Notification.AlreadyHave"]));
        }
        else
        {
            client.PrintToChat(
                LupercaliaMGCore.MessageWithPrefix(
                    m_CSSPlugin.Localizer["Misc.Knife.Command.Notification.Retrieved"]));
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
            client.PrintToChat(
                LupercaliaMGCore.MessageWithPrefix(
                    m_CSSPlugin.Localizer["Misc.Spectate.Command.Notification.MovedToSpectator"]));
            return;
        }

        if (client.PlayerPawn.Value == null || client.PlayerPawn.Value.LifeState == (byte)LifeState_t.LIFE_ALIVE)
        {
            client.PrintToChat(LupercaliaMGCore.MessageWithPrefix(
                m_CSSPlugin.Localizer["Misc.Spectate.Command.Notification.OnlyDeadOrSpectator"]));
            return;
        }

        TargetResult targets = info.GetArgTargetResult(1);

        if (targets.Count() > 1)
        {
            client.PrintToChat(LupercaliaMGCore.MessageWithPrefix(
                m_CSSPlugin.Localizer["Misc.Spectate.Command.Notification.MultipleTargetsFound", targets.Count()]));
            return;
        }

        client.PrintToChat(LupercaliaMGCore.MessageWithPrefix(
            m_CSSPlugin.Localizer["Misc.Spectate.Command.Notification.NowSpectating", targets.First().PlayerName]));
        client.ExecuteClientCommand($"spec_player {targets.First().PlayerName}");
    }
}