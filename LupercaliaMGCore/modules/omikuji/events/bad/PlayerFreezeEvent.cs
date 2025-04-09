using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using LupercaliaMGCore.model;
using LupercaliaMGCore.modules;
using Microsoft.Extensions.Logging;

namespace LupercaliaMGCore;

public class PlayerFreezeEvent(IServiceProvider serviceProvider) : OmikujiEventBase(serviceProvider)
{
    public override string EventName => "Player Freeze Event";

    public override OmikujiType OmikujiType => OmikujiType.EventBad;

    public override OmikujiCanInvokeWhen OmikujiCanInvokeWhen => OmikujiCanInvokeWhen.PlayerAlive;

    public override void Execute(CCSPlayerController client)
    {
        SimpleLogging.LogDebug("Player drew a omikuji and invoked Player freeze event");

        if (!PlayerUtil.IsPlayerAlive(client))
        {
            SimpleLogging.LogDebug("Player freeze event failed due to player is died. But this is should not be happened.");
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

        Server.PrintToChatAll(LocalizeOmikujiResult(client, OmikujiType, "Omikuji.BadEvent.PlayerFreezeEvent.Notification.Froze", client.PlayerName));


        Plugin.AddTimer(PluginSettings.m_CVOmikujiEventPlayerFreeze.Value, () =>
        {
            playerPawn.MoveType = MoveType_t.MOVETYPE_WALK;
            playerPawn.ActualMoveType = MoveType_t.MOVETYPE_WALK;
            SimpleLogging.LogDebug("Player freeze event: Move type changed to MOVETYPE_WALK");
            client.PrintToChat(LocalizeWithPrefix("Omikuji.BadEvent.PlayerFreezeEvent.Notification.UnFroze"));
        });
    }

    public override double GetOmikujiWeight()
    {
        return PluginSettings.m_CVOmikujiEventPlayerFreezeSelectionWeight.Value;
    }
}