using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using TNCSSPluginFoundation.Utils.Entity;

namespace LupercaliaMGCore.modules.omikuji.events.lucky;

public class PlayerHealEvent(IServiceProvider serviceProvider) : OmikujiEventBase(serviceProvider)
{
    public override string EventName => "Player Heal Event";

    public override OmikujiType OmikujiType => OmikujiType.EventLucky;

    public override OmikujiCanInvokeWhen OmikujiCanInvokeWhen => OmikujiCanInvokeWhen.PlayerAlive;

    
    public readonly FakeConVar<int> HealAmount = new("lp_mg_omikuji_event_player_heal_amount",
        "How many health healed when event occur", 100);

    public readonly FakeConVar<double> EventSelectionWeight =
        new("lp_mg_omikuji_event_player_heal_selection_weight", "Selection weight of this event", 30.0D);

    public override void Initialize()
    {
        TrackConVar(HealAmount);
        TrackConVar(EventSelectionWeight);
    }

    public override void Execute(CCSPlayerController client)
    {
        DebugLogger.LogDebug("Player drew a omikuji and invoked Player heal event.");

        string msg;

        bool isPlayerAlive = PlayerUtil.IsPlayerAlive(client);

        if (isPlayerAlive)
        {
            msg = LocalizeOmikujiResult(client, OmikujiType, "Omikuji.LuckyEvent.PlayerHealEvent.Notification.Healed", client.PlayerName, HealAmount.Value);
        }
        else
        {
            msg = LocalizeOmikujiResult(client, OmikujiType, "Omikuji.LuckyEvent.PlayerHealEvent.Notification.PlayerIsDead", client.PlayerName);
        }


        foreach (CCSPlayerController cl in Utilities.GetPlayers())
        {
            if (!cl.IsValid || cl.IsBot || cl.IsHLTV)
                continue;

            cl.PrintToChat(msg);
        }

        if (!isPlayerAlive)
            return;

        CCSPlayerPawn playerPawn = client.PlayerPawn.Value!;

        if (playerPawn.MaxHealth < playerPawn.Health + HealAmount.Value)
        {
            playerPawn.Health = playerPawn.MaxHealth;
        }
        else
        {
            playerPawn.Health += HealAmount.Value;
        }

        Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_iHealth");
    }

    public override double GetOmikujiWeight()
    {
        return EventSelectionWeight.Value;
    }
}