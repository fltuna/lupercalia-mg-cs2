using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;

namespace LupercaliaMGCore.modules.omikuji.events.misc;

public class PlayerWishingEvent(IServiceProvider serviceProvider) : OmikujiEventBase(serviceProvider)
{
    public override string EventName => "Player Wishing Event";

    public override OmikujiType OmikujiType => OmikujiType.EventMisc;

    public override OmikujiCanInvokeWhen OmikujiCanInvokeWhen => OmikujiCanInvokeWhen.Anytime;

    
    public readonly FakeConVar<double> EventSelectionWeight =
        new("lp_mg_omikuji_event_player_wishing_selection_weight", "Selection weight of this event", 30.0D);

    public override void Initialize()
    {
        TrackConVar(EventSelectionWeight);
    }

    public override void Execute(CCSPlayerController client)
    {
        DebugLogger.LogDebug("Player drew a omikuji and invoked Player wishing event");
        
        Server.PrintToChatAll(LocalizeWithPrefix(null, "Omikuji.MiscEvent.PlayerWishingEvent.Notification.Wishing", client.PlayerName));
    }

    public override double GetOmikujiWeight()
    {
        return EventSelectionWeight.Value;
    }
}