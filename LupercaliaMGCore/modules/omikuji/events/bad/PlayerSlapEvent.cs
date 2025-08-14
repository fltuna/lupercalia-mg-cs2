using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;

namespace LupercaliaMGCore.modules.omikuji.events.bad;

public class PlayerSlapEvent(IServiceProvider serviceProvider) : OmikujiEventBase(serviceProvider)
{
    public override string EventName => "Player Slap Event";

    public override OmikujiType OmikujiType => OmikujiType.EventBad;

    public override OmikujiCanInvokeWhen OmikujiCanInvokeWhen => OmikujiCanInvokeWhen.PlayerAlive;

    
    public readonly FakeConVar<int> SlapPowerMin =
        new("lp_mg_omikuji_event_player_slap_power_min", "Minimal power of slap.", 0);

    public readonly FakeConVar<int> SlapPowerMax =
        new("lp_mg_omikuji_event_player_slap_power_max", "Maximum power of slap.", 30000);

    public readonly FakeConVar<double> EventSelectionWeight =
        new("lp_mg_omikuji_event_player_slap_item_selection_weight", "Selection weight of this event", 30.0D);

    public override void Initialize()
    {
        TrackConVar(SlapPowerMin);
        TrackConVar(SlapPowerMax);
        TrackConVar(EventSelectionWeight);
    }

    public override void Execute(CCSPlayerController client)
    {
        DebugLogger.LogDebug("Player drew a omikuji and invoked Player Slap Event");

        CCSPlayerPawn? pawn = client.PlayerPawn.Value;

        if (pawn == null)
        {
            DebugLogger.LogDebug("Player Slap Event: Pawn is null! cancelling!");
            return;
        }

        Vector velo = pawn.AbsVelocity;

        int slapPowerMin = SlapPowerMin.Value;
        int slapPowerMax = SlapPowerMax.Value;

        DebugLogger.LogTrace($"Player Slap Event: Random slap power - Min: {slapPowerMin}, Max: {slapPowerMax}");

        // Taken from sourcemod
        velo.X += ((Random.NextInt64(slapPowerMin, slapPowerMax) % 180) + 50) * (((Random.NextInt64(slapPowerMin, slapPowerMax) % 2) == 1) ? -1 : 1);
        velo.Y += ((Random.NextInt64(slapPowerMin, slapPowerMax) % 180) + 50) * (((Random.NextInt64(slapPowerMin, slapPowerMax) % 2) == 1) ? -1 : 1);
        velo.Z += Random.NextInt64(slapPowerMin, slapPowerMax) % 200 + 100;
        DebugLogger.LogTrace($"Player Slap Event: Player velocity - {velo.X} {velo.Y} {velo.Z}");

        Server.PrintToChatAll(LocalizeOmikujiResult(client, OmikujiType, "Omikuji.BadEvent.PlayerSlapEvent.Notification.Slapped", client.PlayerName));
    }

    public override double GetOmikujiWeight()
    {
        return EventSelectionWeight.Value;
    }
}