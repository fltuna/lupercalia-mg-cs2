using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Commands.Targeting;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using TNCSSPluginFoundation.Models.Plugin;
using TNCSSPluginFoundation.Utils.Entity;

namespace LupercaliaMGCore.modules;

public sealed class MiscCommands(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "MiscCommands";

    public override string ModuleChatPrefix => "[Misc Commands]";
    protected override bool UseTranslationKeyInModuleChatPrefix => false;

    
    public readonly FakeConVar<bool> GiveKnifeEnabled = 
        new("lp_mg_misc_cmd_give_knife", "Is give knife command enabled?", false);
    
    protected override void OnInitialize()
    {
        TrackConVar(GiveKnifeEnabled);
        
        Plugin.AddCommand("css_knife", "give knife", CommandGiveKnife);
        Plugin.AddCommand("css_spec", "Spectate", CommandSpectate);
    }

    protected override void OnUnloadModule()
    {
        Plugin.RemoveCommand("css_knife", CommandGiveKnife);
        Plugin.RemoveCommand("css_spec", CommandSpectate);
    }


    private void CommandGiveKnife(CCSPlayerController? client, CommandInfo info)
    {
        if (client == null)
            return;

        if (!PlayerUtil.IsPlayerAlive(client))
        {
            client.PrintToChat(LocalizeWithPluginPrefix(client, "General.Command.Notification.ShouldBeAlive"));
            return;
        }

        if (!GiveKnifeEnabled.Value)
        {
            client.PrintToChat(LocalizeWithPluginPrefix(client, "General.Command.Notification.FeatureEnabled"));
            return;
        }

        CCSPlayerPawn? playerPawn = client.PlayerPawn.Value;

        if (playerPawn == null)
        {
            client.PrintToChat(LocalizeWithPluginPrefix(client, "General.Command.Notification.NotUsableCurrently"));
            return;
        }

        CPlayer_WeaponServices? weaponServices = playerPawn.WeaponServices;

        if (weaponServices == null)
        {
            client.PrintToChat(LocalizeWithPluginPrefix(client, "General.Command.Notification.NotUsableCurrently"));
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
            client.PrintToChat(LocalizeWithPluginPrefix(client, "General.Command.Notification.AlreadyHave"));
        }
        else
        {
            client.PrintToChat(LocalizeWithPluginPrefix(client, "General.Command.Notification.Retrieved"));
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
            client.PrintToChat(LocalizeWithPluginPrefix(client, "Misc.Spectate.Command.Notification.MovedToSpectator"));
            return;
        }

        if (PlayerUtil.IsPlayerAlive(client))
        {
            client.PrintToChat(LocalizeWithPluginPrefix(client, "Misc.Spectate.Command.Notification.OnlyDeadOrSpectator"));
            return;
        }

        TargetResult targets = info.GetArgTargetResult(1);

        if (targets.Count() > 1)
        {
            client.PrintToChat(LocalizeWithPluginPrefix(client, "Misc.Spectate.Command.Notification.MultipleTargetsFound", targets.Count()));
            return;
        }

        client.ExecuteClientCommand($"spec_player {targets.First().PlayerName}");
        client.PrintToChat(LocalizeWithPluginPrefix(client, "Misc.Spectate.Command.Notification.NowSpectating", targets.First().PlayerName));
    }
}