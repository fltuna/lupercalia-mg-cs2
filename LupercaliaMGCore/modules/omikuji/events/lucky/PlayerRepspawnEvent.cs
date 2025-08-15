using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using TNCSSPluginFoundation.Utils.Entity;

namespace LupercaliaMGCore.modules.omikuji.events.lucky;

public class PlayerRespawnEvent(IServiceProvider serviceProvider) : OmikujiEventBase(serviceProvider)
{
    public override string EventName => "Player Respawn Event";

    public override OmikujiType OmikujiType => OmikujiType.EventLucky;

    public override OmikujiCanInvokeWhen OmikujiCanInvokeWhen => OmikujiCanInvokeWhen.PlayerDied;

    
    public readonly FakeConVar<double> EventSelectionWeight =
        new("lp_mg_omikuji_event_player_respawn_selection_weight", "Selection weight of this event", 30.0D);

    public override void Initialize()
    {
        TrackConVar(EventSelectionWeight);
    }

    public override void Execute(CCSPlayerController client)
    {
        DebugLogger.LogDebug("Player drew a omikuji and invoked Player respawn event");

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
        return EventSelectionWeight.Value;
    }
}