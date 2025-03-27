using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;

namespace LupercaliaMGCore;

public class PlayerRespawnAllEvent : IOmikujiEvent
{
    public string EventName => "Player Respawn All Event";

    public OmikujiType OmikujiType => OmikujiType.EVENT_LUCKY;

    public OmikujiCanInvokeWhen OmikujiCanInvokeWhen => OmikujiCanInvokeWhen.ANYTIME;

    public void execute(CCSPlayerController client)
    {
        SimpleLogging.LogDebug("Player drew a omikuji and invoked All player respawn event.");

        CCSPlayerController? alivePlayer = null;

        foreach (CCSPlayerController cl in Utilities.GetPlayers())
        {
            if (!cl.IsValid || cl.IsBot || cl.IsHLTV)
                continue;

            if (PlayerUtil.IsPlayerAlive(cl))
            {
                alivePlayer = cl;
                break;
            }
        }

        if (alivePlayer == null)
        {
            SimpleLogging.LogDebug("All player respawn event failed due to no one player is alive.");
            foreach (CCSPlayerController cl in Utilities.GetPlayers())
            {
                if (!cl.IsValid || cl.IsBot || cl.IsHLTV)
                    continue;

                cl.PrintToChat(
                    $"{Omikuji.ChatPrefix} {Omikuji.GetOmikujiLuckMessage(OmikujiType, client)} {LupercaliaMGCore.getInstance().Localizer["Omikuji.LuckyEvent.PlayerRespawnAllEvent.Notification.NoOneAlive"]}");
            }

            return;
        }

        foreach (CCSPlayerController cl in Utilities.GetPlayers())
        {
            if (!cl.IsValid || cl.IsBot || cl.IsHLTV)
                continue;

            cl.PrintToChat(
                $"{Omikuji.ChatPrefix} {Omikuji.GetOmikujiLuckMessage(OmikujiType, client)} {LupercaliaMGCore.getInstance().Localizer["Omikuji.LuckyEvent.PlayerRespawnAllEvent.Notification.Respawn"]}");

            if (PlayerUtil.IsPlayerAlive(cl))
                continue;

            cl.Respawn();
            cl.PlayerPawn.Value!.Teleport(alivePlayer.PlayerPawn.Value!.AbsOrigin, alivePlayer.PlayerPawn.Value.AbsRotation);
        }
    }

    public void initialize()
    {
    }

    public double getOmikujiWeight()
    {
        return PluginSettings.GetInstance.m_CVOmikujiEventAllPlayerRespawnSelectionWeight.Value;
    }
}