using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;

namespace LupercaliaMGCore.modules.omikuji.events.bad;

public class GravityChangeEvent(IServiceProvider serviceProvider) : OmikujiEventBase(serviceProvider)
{
    public override string EventName => "Gravity Change Event";

    public override OmikujiType OmikujiType => OmikujiType.EventBad;

    public override OmikujiCanInvokeWhen OmikujiCanInvokeWhen => OmikujiCanInvokeWhen.Anytime;

    private static bool isInGravityChangeEvent = false;

    
    public readonly FakeConVar<int> GravityMax =
        new("lp_mg_omikuji_event_gravity_max", "Maximum value of sv_gravity", 800);

    public readonly FakeConVar<int> GravityMin =
        new("lp_mg_omikuji_event_gravity_min", "Minimal value of sv_gravity", 100);

    public readonly FakeConVar<float> GravityRestoreTime = new(
        "lp_mg_omikuji_event_gravity_restore_time", "How long to take gravity restored in seconds.", 10.0F);

    public readonly FakeConVar<double> EventSelectionWeight =
        new("lp_mg_omikuji_event_gravity_selection_weight", "Selection weight of this event", 30.0D);

    public override void Initialize()
    {
        TrackConVar(GravityMax);
        TrackConVar(GravityMin);
        TrackConVar(GravityRestoreTime);
        TrackConVar(EventSelectionWeight);
    }

    public override void Execute(CCSPlayerController client)
    {
        DebugLogger.LogDebug("Player drew a omikuji and invoked Gravity change event");

        int randomGravity = Random.Next(
            GravityMin.Value,
            GravityMax.Value
        );

        string msg;

        if (isInGravityChangeEvent)
        {
            msg = LocalizeOmikujiResult(client, OmikujiType,"Omikuji.BadEvent.GravityChangeEvent.Notification.AnotherEventOnGoing");
        }
        else
        {
            msg = LocalizeOmikujiResult(client, OmikujiType, "Omikuji.BadEvent.GravityChangeEvent.Notification.GravityChanged", randomGravity);
        }

        foreach (CCSPlayerController cl in Utilities.GetPlayers())
        {
            if (!cl.IsValid || cl.IsBot || cl.IsHLTV)
                continue;

            cl.PrintToChat(msg);
        }

        if (isInGravityChangeEvent)
            return;

        isInGravityChangeEvent = true;
        ConVar? sv_gravity = ConVar.Find("sv_gravity");

        float oldGravity = sv_gravity!.GetPrimitiveValue<float>();

        sv_gravity.SetValue((float)randomGravity);

        float TIMER_INTERVAL_PLACE_HOLDER = GravityRestoreTime.Value;

        Plugin.AddTimer(TIMER_INTERVAL_PLACE_HOLDER, () =>
        {
            sv_gravity.SetValue(oldGravity);
            foreach (CCSPlayerController cl in Utilities.GetPlayers())
            {
                if (!cl.IsValid || cl.IsBot || cl.IsHLTV)
                    continue;

                cl.PrintToChat(LocalizeWithPrefix(cl, "Omikuji.BadEvent.GravityChangeEvent.Notification.GravityRestored", oldGravity));
                isInGravityChangeEvent = false;
            }
        });
    }

    public override double GetOmikujiWeight()
    {
        return EventSelectionWeight.Value;
    }
}