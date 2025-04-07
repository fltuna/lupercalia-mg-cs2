using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using Microsoft.Extensions.Logging;

namespace LupercaliaMGCore;

public class PlayerRespawnEvent(Omikuji omikuji, LupercaliaMGCore plugin) : OmikujiEventBase(omikuji, plugin)
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
            msg = $"{Omikuji.ChatPrefix} {Omikuji.GetOmikujiLuckMessage(OmikujiType, client)} {Plugin.Localizer["Omikuji.LuckyEvent.PlayerRespawnEvent.Notification.Respawn", client.PlayerName]}";
        }
        else
        {
            msg = $"{Omikuji.ChatPrefix} {Omikuji.GetOmikujiLuckMessage(OmikujiType, client)} {Plugin.Localizer["Omikuji.LuckyEvent.PlayerRespawnEvent.Notification.StillAlive", client.PlayerName]}";
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