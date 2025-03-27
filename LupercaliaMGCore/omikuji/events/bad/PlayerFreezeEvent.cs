using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;

namespace LupercaliaMGCore;

public class PlayerFreezeEvent : IOmikujiEvent
{
    public string EventName => "Player Freeze Event";

    public OmikujiType OmikujiType => OmikujiType.EVENT_BAD;

    public OmikujiCanInvokeWhen OmikujiCanInvokeWhen => OmikujiCanInvokeWhen.PLAYER_ALIVE;

    public void execute(CCSPlayerController client)
    {
        SimpleLogging.LogDebug("Player drew a omikuji and invoked Player freeze event");

        if (!PlayerUtil.IsPlayerAlive(client))
        {
            SimpleLogging.LogDebug(
                "Player freeze event failed due to player is died. But this is should not be happened.");
            return;
        }

        CCSPlayerPawn? playerPawn = client.PlayerPawn.Value;

        if (playerPawn == null)
        {
            SimpleLogging.LogDebug("Player freeze event failed due to playerPawn is null.");
            return;
        }


        playerPawn.MoveType = MoveType_t.MOVETYPE_OBSOLETE;
        playerPawn.ActualMoveType = MoveType_t.MOVETYPE_OBSOLETE;
        SimpleLogging.LogDebug("Player freeze event: Move type changed to MOVETYPE_OBSOLETE");

        Server.PrintToChatAll(
            $"{Omikuji.ChatPrefix} {Omikuji.GetOmikujiLuckMessage(OmikujiType, client)} {LupercaliaMGCore.getInstance().Localizer["Omikuji.BadEvent.PlayerFreezeEvent.Notification.Froze", client.PlayerName]}");


        LupercaliaMGCore.getInstance().AddTimer(PluginSettings.GetInstance.m_CVOmikujiEventPlayerFreeze.Value, () =>
        {
            playerPawn.MoveType = MoveType_t.MOVETYPE_WALK;
            playerPawn.ActualMoveType = MoveType_t.MOVETYPE_WALK;
            SimpleLogging.LogDebug("Player freeze event: Move type changed to MOVETYPE_WALK");
            client.PrintToChat(
                $"{Omikuji.ChatPrefix} {LupercaliaMGCore.getInstance().Localizer["Omikuji.BadEvent.PlayerFreezeEvent.Notification.UnFroze"]}");
        });
    }

    public void initialize()
    {
    }

    public double getOmikujiWeight()
    {
        return PluginSettings.GetInstance.m_CVOmikujiEventPlayerFreezeSelectionWeight.Value;
    }
}