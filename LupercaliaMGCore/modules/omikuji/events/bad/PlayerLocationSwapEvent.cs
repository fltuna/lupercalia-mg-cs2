using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using TNCSSPluginFoundation.Utils.Entity;

namespace LupercaliaMGCore.modules.omikuji.events.bad;

public class PlayerLocationSwapEvent(IServiceProvider serviceProvider) : OmikujiEventBase(serviceProvider)
{
    public override string EventName => "Player Location Swap Event";

    public override OmikujiType OmikujiType => OmikujiType.EventBad;

    public override OmikujiCanInvokeWhen OmikujiCanInvokeWhen => OmikujiCanInvokeWhen.PlayerAlive;

    
    public readonly FakeConVar<double> EventSelectionWeight =
        new("lp_mg_omikuji_event_player_location_swap_selection_weight", "Selection weight of this event", 30.0D);

    public override void Initialize()
    {
        TrackConVar(EventSelectionWeight);
    }

    public override void Execute(CCSPlayerController client)
    {
        DebugLogger.LogDebug("Player drew a omikuji and invoked Player Location Swap Event");

        List<(Vector vector, CCSPlayerController player, bool alreadyChosen)> alive = new();

        DebugLogger.LogTrace("Player Location Swap Event: Start iterating the player list for initialize player location dictionary");
        foreach (CCSPlayerController cl in Utilities.GetPlayers())
        {
            if (!cl.IsValid || cl.IsBot || cl.IsHLTV)
                continue;

            if (!PlayerUtil.IsPlayerAlive(cl))
                continue;


            Vector absOrigin = cl.PlayerPawn.Value!.AbsOrigin!;
            Vector vec = new Vector(absOrigin.X, absOrigin.Y, absOrigin.Z);

            alive.Add((vec, cl, false));

            DebugLogger.LogTrace($"Player Location Swap Event: Vector - X: {vec.X} Y: {vec.Y} Z: {vec.Z}");
        }

        DebugLogger.LogTrace("Player Location Swap Event: Initialized player location dictionary");

        if (alive.Count <= 1)
        {
            DebugLogger.LogTrace("Player Location Swap Event: Not enough players to swap location! cancelling event!");
            Server.PrintToChatAll(LocalizeOmikujiResult(client, OmikujiType, "Omikuji.BadEvent.PlayerLocationSwapEvent.Notification.Avoided", client.PlayerName));
            return;
        }

        DebugLogger.LogTrace($"Player Location Swap Event: Found players {alive.Count}");
        DebugLogger.LogTrace("Player Location Swap Event: Shuffling the player locations");
        // Shuffle list for swapping location
        for (int i = alive.Count - 1; i >= 0; i--)
        {
            int j = Random.Next(0, alive.Count);
            (alive[i], alive[j]) = (alive[j], alive[i]);
        }

        DebugLogger.LogTrace("Player Location Swap Event: Start iterating the player list for swap player location");

        DebugLogger.LogTrace($"Player Location Swap Event: Many players alive {alive.Count}! Swapping!");
        foreach (CCSPlayerController cl in Utilities.GetPlayers())
        {
            if (!cl.IsValid || cl.IsBot || cl.IsHLTV)
                continue;

            if (!PlayerUtil.IsPlayerAlive(cl))
                continue;

            Vector? vec = null;

            for (int i = alive.Count - 1; i >= 0; i--)
            {
                if (alive[i].player != cl && !alive[i].alreadyChosen)
                {
                    var al = alive[i];
                    vec = al.vector;
                    alive[i] = (al.vector, al.player, true);
                    break;
                }
            }

            if (vec == null)
            {
                DebugLogger.LogTrace($"Player Location Swap Event: there is no teleport location available for player {cl.PlayerName} skipping");
                continue;
            }

            DebugLogger.LogTrace($"Teleport {cl.PlayerName} to {vec.X}, {vec.Y}, {vec.Z}");
            cl.PlayerPawn.Value!.Teleport(vec, cl.PlayerPawn.Value!.EyeAngles, cl.PlayerPawn.Value!.AbsVelocity);
        }


        Server.PrintToChatAll(LocalizeOmikujiResult(client, OmikujiType, "Omikuji.BadEvent.PlayerLocationSwapEvent.Notification.LocationSwapping"));
    }

    public override double GetOmikujiWeight()
    {
        return EventSelectionWeight.Value;
    }
}