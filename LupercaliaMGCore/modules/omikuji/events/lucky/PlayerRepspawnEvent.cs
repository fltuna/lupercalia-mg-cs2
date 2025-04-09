using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using LupercaliaMGCore.model;
using LupercaliaMGCore.modules;
using Microsoft.Extensions.Logging;

namespace LupercaliaMGCore;

public class PlayerRespawnEvent(IServiceProvider serviceProvider) : OmikujiEventBase(serviceProvider)
{
    public override string EventName => "Player Respawn Event";

    public override OmikujiType OmikujiType => OmikujiType.EventLucky;

    public override OmikujiCanInvokeWhen OmikujiCanInvokeWhen => OmikujiCanInvokeWhen.PlayerDied;

    public override void Execute(CCSPlayerController client)
    {
        SimpleLogging.LogDebug("Player drew a omikuji and invoked Player respawn event");

        string msg;

        bool isPlayerAlive = PlayerUtil.IsPlayerAlive(client);

        if (isPlayerAlive)
        {
            msg = LocalizeOmikujiResult(client, OmikujiType, "Omikuji.LuckyEvent.PlayerRespawnEvent.Notification.Respawn", client.PlayerName);
        }
        else
        {
            msg = LocalizeOmikujiResult(client, OmikujiType, "Omikuji.LuckyEvent.PlayerRespawnEvent.Notification.StillAlive", client.PlayerName);
        }

        foreach (CCSPlayerController cl in Utilities.GetPlayers())
        {
            if (!cl.IsValid || cl.IsBot || cl.IsHLTV)
                continue;

            if (!PlayerUtil.IsPlayerAlive(client))
            {
                Server.NextFrame(() =>
                {
                    client.Respawn();
                    client.PlayerPawn.Value!.Teleport(cl.PlayerPawn!.Value!.AbsOrigin);
                });
            }

            cl.PrintToChat(msg);
        }
    }

    public override double GetOmikujiWeight()
    {
        return PluginSettings.m_CVOmikujiEventPlayerRespawnSelectionWeight.Value;
    }
}