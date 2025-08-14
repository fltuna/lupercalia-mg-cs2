using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using TNCSSPluginFoundation.Utils.Entity;

namespace LupercaliaMGCore.modules.omikuji.events.lucky;

public class PlayerRespawnAllEvent(IServiceProvider serviceProvider) : OmikujiEventBase(serviceProvider)
{
    public override string EventName => "Player Respawn All Event";

    public override OmikujiType OmikujiType => OmikujiType.EventLucky;

    public override OmikujiCanInvokeWhen OmikujiCanInvokeWhen => OmikujiCanInvokeWhen.Anytime;

    public readonly FakeConVar<double> EventSelectionWeight =
        new("lp_mg_omikuji_event_all_player_respawn_selection_weight", "Selection weight of this event", 30.0D);

    public override void Initialize()
    {
        TrackConVar(EventSelectionWeight);
    }

    public override void Execute(CCSPlayerController client)
    {
        DebugLogger.LogDebug("Player drew a omikuji and invoked All player respawn event.");

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
            DebugLogger.LogDebug("All player respawn event failed due to no one player is alive.");
            foreach (CCSPlayerController cl in Utilities.GetPlayers())
            {
                if (!cl.IsValid || cl.IsBot || cl.IsHLTV)
                    continue;

                cl.PrintToChat(LocalizeOmikujiResult(client, OmikujiType, "Omikuji.LuckyEvent.PlayerRespawnAllEvent.Notification.NoOneAlive"));
            }

            return;
        }

        foreach (CCSPlayerController cl in Utilities.GetPlayers())
        {
            if (!cl.IsValid || cl.IsBot || cl.IsHLTV)
                continue;

            cl.PrintToChat(LocalizeOmikujiResult(client, OmikujiType, "Omikuji.LuckyEvent.PlayerRespawnAllEvent.Notification.Respawn"));

            if (PlayerUtil.IsPlayerAlive(cl))
                continue;

            cl.Respawn();
            cl.PlayerPawn.Value!.Teleport(alivePlayer.PlayerPawn.Value!.AbsOrigin, alivePlayer.PlayerPawn.Value.AbsRotation);
        }
    }

    public override double GetOmikujiWeight()
    {
        return EventSelectionWeight.Value;
    }
}