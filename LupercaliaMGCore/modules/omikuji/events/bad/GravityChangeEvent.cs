using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using LupercaliaMGCore.model;
using LupercaliaMGCore.modules;
using Microsoft.Extensions.Logging;

namespace LupercaliaMGCore;

public class GravityChangeEvent(IServiceProvider serviceProvider) : OmikujiEventBase(serviceProvider)
{
    public override string EventName => "Gravity Change Event";

    public override OmikujiType OmikujiType => OmikujiType.EventBad;

    public override OmikujiCanInvokeWhen OmikujiCanInvokeWhen => OmikujiCanInvokeWhen.Anytime;

    private static bool isInGravityChangeEvent = false;

    public override void Execute(CCSPlayerController client)
    {
        SimpleLogging.LogDebug("Player drew a omikuji and invoked Gravity change event");

        int randomGravity = Random.Next(
            PluginSettings.m_CVOmikujiEventGravityMin.Value,
            PluginSettings.m_CVOmikujiEventGravityMax.Value
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

        float TIMER_INTERVAL_PLACE_HOLDER = PluginSettings.m_CVOmikujiEventGravityRestoreTime.Value;

        Plugin.AddTimer(TIMER_INTERVAL_PLACE_HOLDER, () =>
        {
            sv_gravity.SetValue(oldGravity);
            foreach (CCSPlayerController cl in Utilities.GetPlayers())
            {
                if (!cl.IsValid || cl.IsBot || cl.IsHLTV)
                    continue;

                cl.PrintToChat(LocalizeWithPrefix("Omikuji.BadEvent.GravityChangeEvent.Notification.GravityRestored", oldGravity));
                isInGravityChangeEvent = false;
            }
        });
    }

    public override double GetOmikujiWeight()
    {
        return PluginSettings.m_CVOmikujiEventGravitySelectionWeight.Value;
    }
}