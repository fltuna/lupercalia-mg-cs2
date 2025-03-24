using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using Microsoft.Extensions.Logging;

namespace LupercaliaMGCore;

public class PlayerRespawnEvent : IOmikujiEvent
{
    public string EventName => "Player Respawn Event";

    public OmikujiType OmikujiType => OmikujiType.EVENT_LUCKY;

    public OmikujiCanInvokeWhen OmikujiCanInvokeWhen => OmikujiCanInvokeWhen.PLAYER_DIED;

    public void execute(CCSPlayerController client)
    {
        SimpleLogging.LogDebug("Player drew a omikuji and invoked Player respawn event");

        string msg;

        bool isPlayerAlive = client.PlayerPawn.Value != null &&
                             client.PlayerPawn.Value.LifeState == (byte)LifeState_t.LIFE_ALIVE;

        if (isPlayerAlive)
        {
            msg =
                $"{Omikuji.ChatPrefix} {Omikuji.GetOmikujiLuckMessage(OmikujiType, client)} {LupercaliaMGCore.getInstance().Localizer["Omikuji.LuckyEvent.PlayerRespawnEvent.Notification.Respawn", client.PlayerName]}";
        }
        else
        {
            msg =
                $"{Omikuji.ChatPrefix} {Omikuji.GetOmikujiLuckMessage(OmikujiType, client)} {LupercaliaMGCore.getInstance().Localizer["Omikuji.LuckyEvent.PlayerRespawnEvent.Notification.StillAlive", client.PlayerName]}";
        }

        foreach (CCSPlayerController cl in Utilities.GetPlayers())
        {
            if (!cl.IsValid || cl.IsBot || cl.IsHLTV)
                continue;

            if ((client.PlayerPawn.Value == null ||
                 client.PlayerPawn.Value.LifeState != (byte)LifeState_t.LIFE_ALIVE) &&
                (cl.PlayerPawn.Value != null && cl.PlayerPawn.Value.LifeState == (byte)LifeState_t.LIFE_ALIVE))
            {
                Server.NextFrame(() =>
                {
                    client.Respawn();
                    client.Teleport(cl.PlayerPawn!.Value!.AbsOrigin);
                });
            }

            cl.PrintToChat(msg);
        }
    }

    public void initialize()
    {
    }

    public double getOmikujiWeight()
    {
        return PluginSettings.GetInstance.m_CVOmikujiEventPlayerRespawnSelectionWeight.Value;
    }
}