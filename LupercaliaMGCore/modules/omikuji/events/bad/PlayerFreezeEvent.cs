using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using TNCSSPluginFoundation.Utils.Entity;

namespace LupercaliaMGCore.modules.omikuji.events.bad;

public class PlayerFreezeEvent(IServiceProvider serviceProvider) : OmikujiEventBase(serviceProvider)
{
    public override string EventName => "Player Freeze Event";

    public override OmikujiType OmikujiType => OmikujiType.EventBad;

    public override OmikujiCanInvokeWhen OmikujiCanInvokeWhen => OmikujiCanInvokeWhen.PlayerAlive;

    
    public readonly FakeConVar<float> FreezeTime = new("lp_mg_omikuji_event_player_freeze_time",
        "How long to player freeze in seconds.", 3.0F);

    public readonly FakeConVar<double> EventSelectionWeight =
        new("lp_mg_omikuji_event_player_freeze_selection_weight", "Selection weight of this event", 30.0D);

    public override void Initialize()
    {
        TrackConVar(EventSelectionWeight);
        TrackConVar(FreezeTime);
    }

    public override void Execute(CCSPlayerController client)
    {
        DebugLogger.LogDebug("Player drew a omikuji and invoked Player freeze event");

        if (!PlayerUtil.IsPlayerAlive(client))
        {
            DebugLogger.LogDebug("Player freeze event failed due to player is died. But this is should not be happened.");
            return;
        }

        CCSPlayerPawn? playerPawn = client.PlayerPawn.Value;

        if (playerPawn == null)
        {
            DebugLogger.LogDebug("Player freeze event failed due to playerPawn is null.");
            return;
        }


        playerPawn.MoveType = MoveType_t.MOVETYPE_OBSOLETE;
        playerPawn.ActualMoveType = MoveType_t.MOVETYPE_OBSOLETE;
        DebugLogger.LogDebug("Player freeze event: Move type changed to MOVETYPE_OBSOLETE");

        Server.PrintToChatAll(LocalizeOmikujiResult(client, OmikujiType, "Omikuji.BadEvent.PlayerFreezeEvent.Notification.Froze", client.PlayerName));


        Plugin.AddTimer(FreezeTime.Value, () =>
        {
            playerPawn.MoveType = MoveType_t.MOVETYPE_WALK;
            playerPawn.ActualMoveType = MoveType_t.MOVETYPE_WALK;
            DebugLogger.LogDebug("Player freeze event: Move type changed to MOVETYPE_WALK");
            client.PrintToChat(LocalizeWithPrefix(client, "Omikuji.BadEvent.PlayerFreezeEvent.Notification.UnFroze"));
        });
    }

    public override double GetOmikujiWeight()
    {
        return EventSelectionWeight.Value;
    }
}